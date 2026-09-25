using NLog;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace AIHelper.Games.Settings
{
    /// <summary>
    /// The XML file a game keeps its own settings in.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Keeps the encoding the file was read with. The games disagree about it:
    /// the Illusion setup files are UTF-16 while the CM3D2 config is UTF-8, so a
    /// hardcoded encoding silently rewrites a file the game has to read back.
    /// </para>
    /// <para>
    /// Values are written lazily: <see cref="SetValue(string, object)"/> marks the
    /// document dirty and <see cref="Save"/> writes only when something really
    /// changed, so filling the controls does not rewrite the file every time.
    /// </para>
    /// </remarks>
    internal sealed class SettingsXmlFile
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly string _path;
        private readonly Encoding _newFileEncoding;
        private readonly Func<XDocument> _defaultDocumentFactory;

        private XDocument _document;
        private Encoding _encoding;
        private bool _isDirty;

        /// <summary>
        /// A file that could not be parsed is left untouched on disk: writing
        /// defaults over it would destroy the settings of the user.
        /// </summary>
        private bool _isReadOnly;

        internal SettingsXmlFile(string path, Encoding newFileEncoding, Func<XDocument> defaultDocumentFactory)
        {
            _path = path;
            _newFileEncoding = newFileEncoding;
            _defaultDocumentFactory = defaultDocumentFactory;
        }

        /// <summary>
        /// Path of the file this instance works on.
        /// </summary>
        internal string FilePath => _path;

        /// <summary>
        /// The document that is currently loaded. Never null after <see cref="Load"/>.
        /// </summary>
        internal XDocument Document => _document;

        /// <summary>
        /// Reads the file, or creates it with default values when it does not
        /// exist or is empty.
        /// </summary>
        internal void Load()
        {
            _isDirty = false;
            _isReadOnly = false;
            _encoding = _newFileEncoding;

            if (TryLoadExistingFile())
            {
                return;
            }

            _document = _defaultDocumentFactory();
            _isDirty = true;
            Save();
        }

        /// <summary>
        /// Writes the document back when it was changed since the last save.
        /// </summary>
        /// <returns>true when the file was written.</returns>
        internal bool Save()
        {
            if (_isReadOnly || !_isDirty || _document == null)
            {
                return false;
            }

            try
            {
                var directory = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // StreamWriter writes the byte order mark of the encoding, and
                // XDocument.Save takes the encoding of the writer it is given
                using (var writer = new StreamWriter(_path, false, _encoding))
                {
                    _document.Save(writer);
                }

                _isDirty = false;
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Can not write the game settings file '" + _path + "'.\r\n" + ex);
                return false;
            }
        }

        /// <summary>
        /// Reads a value that sits directly below the document root.
        /// </summary>
        /// <returns>The stored text, or an empty string when the element is missing.</returns>
        internal string GetValue(string key) => GetValue(null, key);

        /// <summary>
        /// Reads a value below a category element.
        /// </summary>
        /// <param name="category">Name of the category element, or null for the document root.</param>
        /// <param name="key">Name of the value element.</param>
        /// <returns>The stored text, or an empty string when the element is missing.</returns>
        internal string GetValue(string category, string key)
        {
            var element = FindValueElement(category, key);
            return element != null ? element.Value : string.Empty;
        }

        /// <summary>
        /// Writes a value directly below the document root.
        /// </summary>
        /// <returns>true when the stored text changed.</returns>
        internal bool SetValue(string key, object value) => SetValue(null, key, value);

        /// <summary>
        /// Writes a value below a category element, creating the category when it
        /// is missing. A null <paramref name="value"/> removes the element.
        /// </summary>
        /// <param name="category">Name of the category element, or null for the document root.</param>
        /// <param name="key">Name of the value element.</param>
        /// <param name="value">New value, or null to remove the element.</param>
        /// <returns>true when the stored text changed.</returns>
        internal bool SetValue(string category, string key, object value)
        {
            var parent = GetCategoryElement(category, true);
            if (parent == null)
            {
                return false;
            }

            var element = parent.Element(key);

            if (value == null)
            {
                if (element == null)
                {
                    return false;
                }

                element.Remove();
                _isDirty = true;
                return true;
            }

            // the same conversion XElement would apply, so the comparison below
            // sees exactly what would end up in the file
            var newValue = Convert.ToString(value, CultureInfo.InvariantCulture);

            if (element != null)
            {
                if (element.Value == newValue)
                {
                    return false;
                }

                element.Value = newValue;
                _isDirty = true;
                return true;
            }

            parent.Add(new XElement(key, newValue));
            _isDirty = true;
            return true;
        }

        /// <summary>
        /// Reads the file when it exists and has content.
        /// </summary>
        /// <returns>true when a document is available afterwards, including the fallback after a parse error.</returns>
        private bool TryLoadExistingFile()
        {
            if (!File.Exists(_path) || new FileInfo(_path).Length == 0)
            {
                return false;
            }

            try
            {
                _document = XDocument.Load(_path);
                _encoding = DetectEncoding(_path);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Can not read the game settings file '" + _path + "'. Default values are shown and the file is left as it is.\r\n" + ex);

                _document = _defaultDocumentFactory();
                _isReadOnly = true;
                return true;
            }
        }

        /// <summary>
        /// Determines the encoding a file has to be written back with.
        /// </summary>
        /// <remarks>
        /// A file without a byte order mark is treated as UTF-8, which is what the
        /// games read such a file as.
        /// </remarks>
        private static Encoding DetectEncoding(string path)
        {
            try
            {
                using (var stream = File.OpenRead(path))
                {
                    var bom = new byte[4];
                    var read = stream.Read(bom, 0, bom.Length);

                    if (read >= 2 && bom[0] == 0xFF && bom[1] == 0xFE) return Encoding.Unicode;
                    if (read >= 2 && bom[0] == 0xFE && bom[1] == 0xFF) return Encoding.BigEndianUnicode;
                    if (read >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) return new UTF8Encoding(true);
                }
            }
            catch (Exception ex)
            {
                _log.Warn("Can not detect the encoding of '" + path + "', it is written back as UTF-8.\r\n" + ex);
            }

            return new UTF8Encoding(false);
        }

        private XElement GetCategoryElement(string category, bool create)
        {
            if (_document == null || _document.Root == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(category))
            {
                return _document.Root;
            }

            var element = _document.Root.Element(category);

            if (element == null && create)
            {
                element = new XElement(category);
                _document.Root.Add(element);
            }

            return element;
        }

        private XElement FindValueElement(string category, string key)
        {
            var parent = GetCategoryElement(category, false);
            return parent?.Element(key);
        }
    }
}
