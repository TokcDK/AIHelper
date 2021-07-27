using System.Windows.Forms;

namespace AIHelper.Forms.ExtraSettings.Elements.BepinEx
{
    class BepinEx : Elements, System.IDisposable
    {
        public BepinEx()
        {
            _bepinExElement = new BepinExForm
            {
                TopLevel = false,
                //BackColor = parentForm.BackColor
            };
            ElementToPlace = _bepinExElement;
        }

        BepinExForm _bepinExElement;

        internal override string Title => "Bepinex settings";

        internal override Form ElementToShow { get => _bepinExElement; set => _bepinExElement = value as BepinExForm; }

        internal override bool Check()
        {
            return true;
        }

        internal override void Show(Form parentForm)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            _bepinExElement.Dispose();
        }
    }
}
