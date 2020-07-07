using System.Windows.Forms;

namespace AIHelper.Forms.ExtraSettings.Elements.BepinEx
{
    class BepinEx:Elements, System.IDisposable
    {
        public BepinEx()
        {
            BepinExElement = new ExtraSettings.Elements.BepinEx.BepinExForm
            {
                TopLevel = false,
                //BackColor = parentForm.BackColor
            };
            ElementToPlace = BepinExElement;
        }

        BepinExForm BepinExElement;

        internal override string Title => throw new System.NotImplementedException();

        internal override Form ElementToShow { get => BepinExElement; set => BepinExElement=value as BepinExForm; }

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
            BepinExElement.Dispose();
        }
    }
}
