using System;
using System.Drawing;
using System.Windows.Forms;

namespace AIHelper.Manage.Functions.InfoEditor
{
    /// <summary>
    /// The control which shows and edits the data one update source holds for one mod.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The fields are built from the data's own public writable properties rather than
    /// listed here, so a source which grows a coordinate gets its field without this class
    /// being touched. A property the user cannot write is not a field, and a property the
    /// data has no caption for is not one the editor knows how to show; both are skipped
    /// rather than guessed at.
    /// </para>
    /// <para>
    /// The control edits the model and raises <see cref="DataChanged"/>; it stores nothing
    /// itself. When the value is written down, and what it means, is the mod's business —
    /// which belongs to the block and to the editor, not to a source.
    /// </para>
    /// </remarks>
    internal sealed class UpdateSourceInfoControl
    {
        private readonly UpdateSourceData _data;
        private readonly ToolTip _toolTip;

        /// <param name="data">Data the control edits. It is the model the fields bind to.</param>
        /// <param name="toolTip">Tooltip the fields explain themselves with.</param>
        internal UpdateSourceInfoControl(UpdateSourceData data, ToolTip toolTip)
        {
            _data = data;
            _toolTip = toolTip;

            Control = Build();
        }

        /// <summary>
        /// Raised when the user edits one of the fields, with the model already holding the
        /// value which was typed.
        /// </summary>
        internal event EventHandler DataChanged;

        /// <summary>The control holding every field of the source's data.</summary>
        internal Control Control { get; }

        /// <summary>
        /// Data the control edits. The editor needs it to know which mod a change belongs to.
        /// </summary>
        internal UpdateSourceData Data
        {
            get { return _data; }
        }

        private Control Build()
        {
            var dataPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                Size = new Size(EditorLayout.PanelSeedSize, EditorLayout.PanelSeedSize),
                Margin = new Padding(0),
                BorderStyle = BorderStyle.Fixed3D,
            };

            foreach (var property in _data.GetType().GetProperties())
            {
                if (!property.CanWrite) continue;

                string caption;
                string description;
                if (!_data.TryGetFieldText(property.Name, out caption, out description)) continue;

                Control field;
                if (property.PropertyType == typeof(string))
                {
                    field = CreateTextField(property.Name, caption, description);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    field = CreateCheckBoxField(property.Name, caption, description);
                }
                else continue;

                dataPanel.Controls.Add(field);

                EditorLayout.GrowToCover(dataPanel, EditorLayout.OccupiedSize(field));
            }

            return dataPanel;
        }

        /// <summary>
        /// One text field: a caption and the text box which edits the value.
        /// </summary>
        private Control CreateTextField(string propertyName, string caption, string description)
        {
            TextBox textBox;

            var row = EditorLayout.CreateFieldRow(caption, description, _toolTip, out textBox);

            var binding = BindField(textBox, nameof(textBox.Text), propertyName);
            textBox.TextChanged += (o, e) => OnFieldChanged(binding);

            return row;
        }

        /// <summary>
        /// One boolean field: a check box which edits the value.
        /// </summary>
        private Control CreateCheckBoxField(string propertyName, string caption, string description)
        {
            var checkBox = EditorLayout.CreateCheckBoxField(caption, description, _toolTip);

            var binding = BindField(checkBox, nameof(checkBox.Checked), propertyName);
            checkBox.CheckedChanged += (o, e) => OnFieldChanged(binding);

            return checkBox;
        }

        /// <summary>
        /// Connects a control to one property of the data.
        /// </summary>
        /// <returns>
        /// The binding, so that the field can push the edited value into the data without
        /// looking the binding up again by index.
        /// </returns>
        private Binding BindField(Control control, string controlProperty, string propertyName)
        {
            var binding = new Binding(
                controlProperty,
                _data,
                propertyName,
                true,
                DataSourceUpdateMode.OnPropertyChanged);

            control.DataBindings.Add(binding);

            return binding;
        }

        /// <summary>
        /// Pushes the value the user just typed into the data and tells the editor about it.
        /// </summary>
        /// <remarks>
        /// The push happens here rather than at the moment the editor stores the value, so
        /// that a burst of keystrokes leaves the data holding the last one: the editor waits
        /// before it writes, and by then the event is long over.
        /// </remarks>
        private void OnFieldChanged(Binding binding)
        {
            binding.WriteValue(); // force the edited value into the model before announcing it

            var handler = DataChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
