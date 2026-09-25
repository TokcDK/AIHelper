using System;
using System.Text;

namespace AIHelper.Install.Cards
{
    /// <summary>
    /// Just enough of the MessagePack format to walk the block table of a card
    /// file and to read single fields out of a block.
    /// </summary>
    /// <remarks>
    /// A card stores its metadata as MessagePack maps and arrays of scalars
    /// (see the block table and the <c>Parameter</c> block of the reference
    /// implementation in KKManager), which is what this reader supports. Every
    /// method returns false instead of throwing, so a malformed card simply fails
    /// to be detected instead of breaking the install.
    /// </remarks>
    internal sealed class MessagePackLite
    {
        private readonly byte[] _data;
        private readonly int _end;
        private int _position;

        internal MessagePackLite(byte[] data, int offset, int length)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _data = data;
            _position = offset;
            _end = Math.Min(data.Length, offset + length);
        }

        internal bool IsAtEnd => _position >= _end;

        /// <summary>
        /// Reads the entry count of a map.
        /// </summary>
        internal bool TryReadMapLength(out int length)
        {
            length = 0;

            if (!TryReadCode(out var code)) return false;

            if (code >= 0x80 && code <= 0x8F)
            {
                length = code & 0x0F;
                return true;
            }

            if (code == 0xDE) return TryReadUInt16(out length);
            if (code == 0xDF) return TryReadUInt32AsLength(out length);

            return false;
        }

        /// <summary>
        /// Reads the entry count of an array.
        /// </summary>
        internal bool TryReadArrayLength(out int length)
        {
            length = 0;

            if (!TryReadCode(out var code)) return false;

            if (code >= 0x90 && code <= 0x9F)
            {
                length = code & 0x0F;
                return true;
            }

            if (code == 0xDC) return TryReadUInt16(out length);
            if (code == 0xDD) return TryReadUInt32AsLength(out length);

            return false;
        }

        /// <summary>
        /// Reads a string value.
        /// </summary>
        internal bool TryReadString(out string value)
        {
            value = null;

            if (!TryReadCode(out var code)) return false;

            int length;

            if (code >= 0xA0 && code <= 0xBF)
            {
                length = code & 0x1F;
            }
            else if (code == 0xD9)
            {
                if (!TryReadByte(out var small)) return false;
                length = small;
            }
            else if (code == 0xDA)
            {
                if (!TryReadUInt16(out length)) return false;
            }
            else if (code == 0xDB)
            {
                if (!TryReadUInt32AsLength(out length)) return false;
            }
            else
            {
                return false;
            }

            if (length < 0 || _position + length > _end) return false;

            value = Encoding.UTF8.GetString(_data, _position, length);
            _position += length;

            return true;
        }

        /// <summary>
        /// Reads an integer value of any of the integer MessagePack types.
        /// </summary>
        internal bool TryReadInt64(out long value)
        {
            value = 0;

            if (!TryReadCode(out var code)) return false;

            if (code <= 0x7F)
            {
                value = code;
                return true;
            }

            if (code >= 0xE0)
            {
                value = code - 0x100;
                return true;
            }

            switch (code)
            {
                case 0xCC:
                    if (!TryReadByte(out var u8)) return false;
                    value = u8;
                    return true;
                case 0xCD:
                    if (!TryReadUInt16(out var u16)) return false;
                    value = u16;
                    return true;
                case 0xCE:
                    if (!TryReadUInt32(out var u32)) return false;
                    value = u32;
                    return true;
                case 0xCF:
                    if (!TryReadUInt64(out var u64)) return false;
                    value = unchecked((long)u64);
                    return true;
                case 0xD0:
                    if (!TryReadByte(out var i8)) return false;
                    value = unchecked((sbyte)i8);
                    return true;
                case 0xD1:
                    if (!TryReadUInt16(out var i16)) return false;
                    value = unchecked((short)i16);
                    return true;
                case 0xD2:
                    if (!TryReadUInt32(out var i32)) return false;
                    value = unchecked((int)i32);
                    return true;
                case 0xD3:
                    if (!TryReadUInt64(out var i64)) return false;
                    value = unchecked((long)i64);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Skips one complete value, including nested maps and arrays.
        /// </summary>
        internal bool TrySkip()
        {
            if (!TryReadCode(out var code)) return false;

            // positive fixint, negative fixint, nil, false, true
            if (code <= 0x7F || code >= 0xE0) return true;
            if (code == 0xC0 || code == 0xC2 || code == 0xC3) return true;

            // fixstr
            if (code >= 0xA0 && code <= 0xBF) return TrySkipBytes(code & 0x1F);

            // fixmap
            if (code >= 0x80 && code <= 0x8F) return TrySkipItems(2 * (code & 0x0F));

            // fixarray
            if (code >= 0x90 && code <= 0x9F) return TrySkipItems(code & 0x0F);

            switch (code)
            {
                case 0xC4:
                    if (!TryReadByte(out var bin8)) return false;
                    return TrySkipBytes(bin8);
                case 0xC5:
                    if (!TryReadUInt16(out var bin16)) return false;
                    return TrySkipBytes(bin16);
                case 0xC6:
                    if (!TryReadUInt32AsLength(out var bin32)) return false;
                    return TrySkipBytes(bin32);
                case 0xC7:
                    if (!TryReadByte(out var ext8)) return false;
                    return TrySkipBytes(ext8 + 1);
                case 0xC8:
                    if (!TryReadUInt16(out var ext16)) return false;
                    return TrySkipBytes(ext16 + 1);
                case 0xC9:
                    if (!TryReadUInt32AsLength(out var ext32)) return false;
                    return TrySkipBytes(ext32 + 1);
                case 0xCA:
                    return TrySkipBytes(4);
                case 0xCB:
                    return TrySkipBytes(8);
                case 0xCC:
                case 0xD0:
                    return TrySkipBytes(1);
                case 0xCD:
                case 0xD1:
                    return TrySkipBytes(2);
                case 0xCE:
                case 0xD2:
                    return TrySkipBytes(4);
                case 0xCF:
                case 0xD3:
                    return TrySkipBytes(8);
                case 0xD4:
                    return TrySkipBytes(2);
                case 0xD5:
                    return TrySkipBytes(3);
                case 0xD6:
                    return TrySkipBytes(5);
                case 0xD7:
                    return TrySkipBytes(9);
                case 0xD8:
                    return TrySkipBytes(17);
                case 0xD9:
                    if (!TryReadByte(out var str8)) return false;
                    return TrySkipBytes(str8);
                case 0xDA:
                    if (!TryReadUInt16(out var str16)) return false;
                    return TrySkipBytes(str16);
                case 0xDB:
                    if (!TryReadUInt32AsLength(out var str32)) return false;
                    return TrySkipBytes(str32);
                case 0xDC:
                    if (!TryReadUInt16(out var array16)) return false;
                    return TrySkipItems(array16);
                case 0xDD:
                    if (!TryReadUInt32AsLength(out var array32)) return false;
                    return TrySkipItems(array32);
                case 0xDE:
                    if (!TryReadUInt16(out var map16)) return false;
                    return TrySkipItems(2 * map16);
                case 0xDF:
                    if (!TryReadUInt32AsLength(out var map32)) return false;
                    return TrySkipItems(2 * map32);
                default:
                    return false;
            }
        }

        private bool TrySkipItems(int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (!TrySkip()) return false;
            }

            return true;
        }

        private bool TrySkipBytes(int count)
        {
            if (count < 0 || _position + count > _end) return false;

            _position += count;
            return true;
        }

        private bool TryReadCode(out byte code)
        {
            code = 0;

            if (!TryReadByte(out var value)) return false;

            code = (byte)value;
            return true;
        }

        private bool TryReadByte(out int value)
        {
            value = 0;

            if (_position >= _end) return false;

            value = _data[_position];
            _position++;

            return true;
        }

        private bool TryReadUInt16(out int value)
        {
            value = 0;

            if (_position + 2 > _end) return false;

            // MessagePack stores every multi byte number most significant byte
            // first, unlike the little endian block table length of the card header.
            value = (_data[_position] << 8) | _data[_position + 1];
            _position += 2;

            return true;
        }

        private bool TryReadUInt32(out uint value)
        {
            value = 0;

            if (_position + 4 > _end) return false;

            value = ((uint)_data[_position] << 24)
                | ((uint)_data[_position + 1] << 16)
                | ((uint)_data[_position + 2] << 8)
                | _data[_position + 3];
            _position += 4;

            return true;
        }

        private bool TryReadUInt64(out ulong value)
        {
            value = 0;

            if (_position + 8 > _end) return false;

            for (var i = 0; i < 8; i++)
            {
                value = (value << 8) | _data[_position + i];
            }

            _position += 8;

            return true;
        }

        private bool TryReadUInt32AsLength(out int value)
        {
            value = 0;

            if (!TryReadUInt32(out var raw)) return false;

            // lengths above int.MaxValue can not be used as a length anyway
            if (raw > int.MaxValue) return false;

            value = (int)raw;
            return true;
        }
    }
}
