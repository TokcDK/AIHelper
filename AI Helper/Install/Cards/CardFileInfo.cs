namespace AIHelper.Install.Cards
{
    /// <summary>
    /// Result of reading the header of a card file.
    /// </summary>
    internal sealed class CardFileInfo
    {
        internal CardFileInfo(string marker, CardContentKind kind, string formatVersion)
        {
            Marker = marker;
            Kind = kind;
            FormatVersion = formatVersion;
        }

        /// <summary>
        /// The <c>【...】</c> tag a card carries behind its PNG image, or null for
        /// the formats which have no such tag.
        /// </summary>
        internal string Marker { get; }

        /// <summary>
        /// Detected content kind.
        /// </summary>
        internal CardContentKind Kind { get; }

        /// <summary>
        /// Format version of the card body, or null when the format has none.
        /// </summary>
        internal string FormatVersion { get; }

        /// <summary>
        /// True when the kind is known and the card can be moved to a target folder.
        /// </summary>
        internal bool IsDetected => Kind != CardContentKind.Unknown;

        public override string ToString()
        {
            return "CardFileInfo(" + Kind + ", marker=" + (Marker ?? "<none>") + ", version=" + (FormatVersion ?? "<none>") + ")";
        }
    }
}
