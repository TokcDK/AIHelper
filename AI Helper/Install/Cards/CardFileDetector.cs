using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AIHelper.Install.Cards
{
    /// <summary>
    /// Detects what a card file contains by reading the metadata the game itself
    /// wrote behind the preview image, so the file name does not matter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A card is a PNG image followed by a data block. Right behind the PNG
    /// <c>IEND</c> chunk the file carries a tag which names the game that made it
    /// (for example <c>【KoiKatuChara】</c>). Cards of the old HoneySelect format
    /// start with the tag directly, every other game writes an <see cref="int"/>
    /// product number in front of it. Studio scenes carry a plain version string
    /// instead of a tag, and are recognized by its shape.
    /// </para>
    /// <para>
    /// Several games share one tag (AI Girl and HoneySelect 2 both write
    /// <c>【AIS_Chara】</c>), so the tag alone can not tell female cards from male
    /// ones. That bit lives in the <c>Parameter</c> block of the card, which is
    /// read through the block table the file stores as MessagePack.
    /// </para>
    /// <para>
    /// The block table is located by searching for its MessagePack key instead of
    /// replaying the header of each game, because the games disagree on the fields
    /// they write before the table. This keeps one code path for all of them.
    /// </para>
    /// </remarks>
    internal static class CardFileDetector
    {
        /// <summary>
        /// Last chunk of every PNG file.
        /// </summary>
        private static readonly byte[] PngEndChunk = { 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };

        /// <summary>
        /// The MessagePack key <c>"lstInfo"</c> of the block table.
        /// </summary>
        /// <remarks>
        /// Searched as raw bytes on purpose: the four bytes in front of the key are
        /// the entry count of the table, so finding the key also finds the start of
        /// the table.
        /// </remarks>
        private static readonly byte[] BlockTableKey = { 0xA7, 0x6C, 0x73, 0x74, 0x49, 0x6E, 0x66, 0x6F };

        private const string BlockTablePropertyName = "lstInfo";
        private const string ParameterBlockName = "Parameter";
        private const string SexFieldName = "sex";

        private const int MaleSexValue = 0;
        private const int FemaleSexValue = 1;

        /// <summary>
        /// Tags written behind the PNG of a card, mapped to what they mean.
        /// </summary>
        private static readonly Dictionary<string, MarkerInfo> Markers = new Dictionary<string, MarkerInfo>(StringComparer.Ordinal)
        {
            // Character cards whose tag does not tell the sex of the character.
            { "【KoiKatuChara】", new MarkerInfo(CardContentKind.Unknown, true) },
            { "【KoiKatuCharaS】", new MarkerInfo(CardContentKind.Unknown, true) },
            { "【KoiKatuCharaSP】", new MarkerInfo(CardContentKind.Unknown, true) },
            { "【KoiKatuCharaSun】", new MarkerInfo(CardContentKind.Unknown, true) },
            { "【EroMakeChara】", new MarkerInfo(CardContentKind.Unknown, true) },
            { "【AIS_Chara】", new MarkerInfo(CardContentKind.Unknown, true) },
            { "【RG_Chara】", new MarkerInfo(CardContentKind.Unknown, true) },
            { "【HCChara】", new MarkerInfo(CardContentKind.Unknown, true) },
            { "【HCPChara】", new MarkerInfo(CardContentKind.Unknown, true) },
            { "【SVChara】", new MarkerInfo(CardContentKind.Unknown, true) },
            { "【ACChara】", new MarkerInfo(CardContentKind.Unknown, true) },

            // Clothes and coordinate cards.
            { "【KoiKatuClothes】", new MarkerInfo(CardContentKind.Coordinate, false) },
            { "【AIS_Clothes】", new MarkerInfo(CardContentKind.Coordinate, false) },
            { "【SVClothes】", new MarkerInfo(CardContentKind.Coordinate, false) },
            { "【ACClothes】", new MarkerInfo(CardContentKind.Coordinate, false) },
            { "【HoneySelectClothesFemale】", new MarkerInfo(CardContentKind.Coordinate, false) },
            { "【HoneySelectClothesMale】", new MarkerInfo(CardContentKind.Coordinate, false) },

            // The old HoneySelect tags name the sex already.
            { "【HoneySelectCharaFemale】", new MarkerInfo(CardContentKind.FemaleCharacter, false) },
            { "【HoneySelectCharaMale】", new MarkerInfo(CardContentKind.MaleCharacter, false) },
        };

        /// <summary>
        /// Reads <paramref name="filePath"/> and detects its content kind.
        /// </summary>
        /// <returns>
        /// A <see cref="CardFileInfo"/> when the file is a card of a known format,
        /// otherwise null. Read errors are not swallowed and reach the caller.
        /// </returns>
        internal static CardFileInfo Detect(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            if (!File.Exists(filePath)) return null;

            return Detect(File.ReadAllBytes(filePath));
        }

        /// <summary>
        /// Detects the content kind of an already read card file.
        /// </summary>
        /// <returns>
        /// A <see cref="CardFileInfo"/> when <paramref name="data"/> is a card of a
        /// known format, otherwise null. Malformed data never throws.
        /// </returns>
        internal static CardFileInfo Detect(byte[] data)
        {
            if (data == null) return null;

            var pngEndIndex = IndexOf(data, PngEndChunk, 0);
            if (pngEndIndex < 0) return null;

            var trailerOffset = pngEndIndex + PngEndChunk.Length;

            // Old HoneySelect format: the tag sits right behind the PNG.
            string marker;
            int afterMarker;
            if (TryReadDotNetString(data, trailerOffset, out marker, out afterMarker) && Markers.ContainsKey(marker))
            {
                return CreateInfo(marker, data, afterMarker);
            }

            // Every other game writes an int product number in front of the tag.
            if (TryReadDotNetString(data, trailerOffset + sizeof(int), out marker, out afterMarker) && Markers.ContainsKey(marker))
            {
                return CreateInfo(marker, data, afterMarker);
            }

            // Studio scenes have no tag, only a version string, and the scene data
            // follows it. Requiring data behind the version keeps a plain image
            // which happens to end with a version shaped text from being taken for
            // a scene, because an ordinary PNG file ends at its IEND chunk.
            if (TryReadDotNetString(data, trailerOffset, out var sceneVersion, out var afterSceneVersion)
                && afterSceneVersion < data.Length
                && IsVersionLike(sceneVersion))
            {
                return new CardFileInfo(null, CardContentKind.StudioScene, sceneVersion);
            }

            return null;
        }

        /// <summary>
        /// Turns a found tag into a result, reading the sex of the character when
        /// the tag alone does not tell it.
        /// </summary>
        private static CardFileInfo CreateInfo(string marker, byte[] data, int afterMarker)
        {
            var markerInfo = Markers[marker];

            if (!markerInfo.NeedsSex)
            {
                return new CardFileInfo(marker, markerInfo.Kind, null);
            }

            int sex;
            string version;
            if (!TryReadParameterSex(data, afterMarker, out sex, out version))
            {
                return new CardFileInfo(marker, CardContentKind.Unknown, null);
            }

            if (sex == FemaleSexValue) return new CardFileInfo(marker, CardContentKind.FemaleCharacter, version);
            if (sex == MaleSexValue) return new CardFileInfo(marker, CardContentKind.MaleCharacter, version);

            return new CardFileInfo(marker, CardContentKind.Unknown, version);
        }

        /// <summary>
        /// Reads the sex field of the <c>Parameter</c> block.
        /// </summary>
        /// <param name="data">Content of the card file</param>
        /// <param name="searchFrom">Offset behind the tag of the card</param>
        /// <param name="sex">Sex value stored in the block, or -1 when it could not be read</param>
        /// <param name="version">Version of the <c>Parameter</c> block, when it could be read</param>
        /// <returns>True when the sex field was read</returns>
        private static bool TryReadParameterSex(byte[] data, int searchFrom, out int sex, out string version)
        {
            sex = -1;
            version = null;

            var keyIndex = IndexOf(data, BlockTableKey, searchFrom);
            if (keyIndex < 5) return false;

            // The four bytes in front of the key are the little endian entry count
            // of the block table, so the table itself starts right after them.
            var tableOffset = keyIndex - 1;
            if (!TryReadInt32(data, tableOffset - sizeof(int), out var tableLength)) return false;
            if (tableLength <= 0 || tableOffset + tableLength > data.Length) return false;

            long blockPosition;
            long blockSize;
            if (!TryFindBlock(data, tableOffset, tableLength, ParameterBlockName, out blockPosition, out blockSize, out version))
            {
                return false;
            }

            // An Int64 block table size follows the table, and block positions are
            // relative to the end of it.
            var blockStart = tableOffset + tableLength + sizeof(long) + blockPosition;
            if (blockStart < 0 || blockSize <= 0 || blockStart + blockSize > data.Length) return false;

            return TryReadSexField(data, (int)blockStart, (int)blockSize, out sex);
        }

        /// <summary>
        /// Walks the block table and returns the position and the size of the block
        /// called <paramref name="blockName"/>.
        /// </summary>
        private static bool TryFindBlock(byte[] data, int tableOffset, int tableLength, string blockName, out long blockPosition, out long blockSize, out string blockVersion)
        {
            blockPosition = 0;
            blockSize = 0;
            blockVersion = null;

            var reader = new MessagePackLite(data, tableOffset, tableLength);
            if (!reader.TryReadMapLength(out var propertyCount)) return false;

            for (var i = 0; i < propertyCount; i++)
            {
                if (!reader.TryReadString(out var propertyName)) return false;

                if (propertyName != BlockTablePropertyName)
                {
                    if (!reader.TrySkip()) return false;
                    continue;
                }

                if (!reader.TryReadArrayLength(out var blockCount)) return false;

                for (var j = 0; j < blockCount; j++)
                {
                    bool isMatch;
                    if (!TryReadBlockEntry(reader, blockName, out blockPosition, out blockSize, out blockVersion, out isMatch))
                    {
                        return false;
                    }

                    if (isMatch) return true;
                }

                // the table has a single property, there is nothing left to scan
                return false;
            }

            return false;
        }

        /// <summary>
        /// Reads one entry of the block table and reports whether it describes the
        /// wanted block.
        /// </summary>
        private static bool TryReadBlockEntry(MessagePackLite reader, string blockName, out long blockPosition, out long blockSize, out string blockVersion, out bool isMatch)
        {
            blockPosition = 0;
            blockSize = 0;
            blockVersion = null;
            isMatch = false;

            if (!reader.TryReadMapLength(out var fieldCount)) return false;

            string name = null;

            for (var i = 0; i < fieldCount; i++)
            {
                if (!reader.TryReadString(out var fieldName)) return false;

                switch (fieldName)
                {
                    case "name":
                        if (!reader.TryReadString(out name)) return false;
                        break;
                    case "version":
                        if (!reader.TryReadString(out blockVersion)) return false;
                        break;
                    case "pos":
                        if (!reader.TryReadInt64(out blockPosition)) return false;
                        break;
                    case "size":
                        if (!reader.TryReadInt64(out blockSize)) return false;
                        break;
                    default:
                        if (!reader.TrySkip()) return false;
                        break;
                }
            }

            isMatch = name == blockName;
            return true;
        }

        /// <summary>
        /// Reads the sex field out of an already located block.
        /// </summary>
        private static bool TryReadSexField(byte[] data, int blockOffset, int blockLength, out int sex)
        {
            sex = -1;

            var reader = new MessagePackLite(data, blockOffset, blockLength);
            if (!reader.TryReadMapLength(out var fieldCount)) return false;

            for (var i = 0; i < fieldCount; i++)
            {
                if (!reader.TryReadString(out var fieldName)) return false;

                if (fieldName == SexFieldName)
                {
                    if (!reader.TryReadInt64(out var value)) return false;

                    sex = (int)value;
                    return true;
                }

                if (!reader.TrySkip()) return false;
            }

            return false;
        }

        /// <summary>
        /// Reads a string written by <see cref="BinaryReader.ReadString"/>, that is
        /// a 7 bit encoded length followed by UTF-8 bytes.
        /// </summary>
        private static bool TryReadDotNetString(byte[] data, int offset, out string value, out int nextOffset)
        {
            value = null;
            nextOffset = offset;

            if (data == null || offset < 0 || offset > data.Length) return false;

            var length = 0L;
            var shift = 0;

            while (true)
            {
                if (offset >= data.Length) return false;
                if (shift > 35) return false;

                var current = data[offset];
                offset++;

                length |= (long)(current & 0x7F) << shift;
                shift += 7;

                if ((current & 0x80) == 0) break;
            }

            if (length < 0 || length > int.MaxValue) return false;
            if (offset + length > data.Length) return false;

            value = Encoding.UTF8.GetString(data, offset, (int)length);
            nextOffset = offset + (int)length;

            return true;
        }

        /// <summary>
        /// Reads a little endian 32 bit integer.
        /// </summary>
        private static bool TryReadInt32(byte[] data, int offset, out int value)
        {
            value = 0;

            if (data == null || offset < 0 || offset + sizeof(int) > data.Length) return false;

            value = data[offset]
                | (data[offset + 1] << 8)
                | (data[offset + 2] << 16)
                | (data[offset + 3] << 24);

            return true;
        }

        /// <summary>
        /// Reports whether the text looks like the version string of a studio scene,
        /// that is whether it consists of digits and dots only.
        /// </summary>
        private static bool IsVersionLike(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            var hasDigit = false;

            for (var i = 0; i < value.Length; i++)
            {
                var symbol = value[i];

                if (symbol >= '0' && symbol <= '9')
                {
                    hasDigit = true;
                    continue;
                }

                if (symbol != '.') return false;
            }

            return hasDigit;
        }

        /// <summary>
        /// Returns the index of the first occurrence of <paramref name="needle"/> in
        /// <paramref name="haystack"/> at or behind <paramref name="start"/>, or -1.
        /// </summary>
        private static int IndexOf(byte[] haystack, byte[] needle, int start)
        {
            if (haystack == null || needle == null || needle.Length == 0) return -1;

            var lastPossibleIndex = haystack.Length - needle.Length;

            for (var i = Math.Max(0, start); i <= lastPossibleIndex; i++)
            {
                if (haystack[i] != needle[0]) continue;

                var isMatch = true;

                for (var j = 1; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch) return i;
            }

            return -1;
        }

        /// <summary>
        /// Meaning of a single card tag.
        /// </summary>
        private sealed class MarkerInfo
        {
            internal MarkerInfo(CardContentKind kind, bool needsSex)
            {
                Kind = kind;
                NeedsSex = needsSex;
            }

            /// <summary>
            /// Content kind of the tag, or <see cref="CardContentKind.Unknown"/> when
            /// the tag does not tell the sex of the character.
            /// </summary>
            internal CardContentKind Kind { get; }

            /// <summary>
            /// True when the sex field of the card must be read to know the kind.
            /// </summary>
            internal bool NeedsSex { get; }
        }
    }
}
