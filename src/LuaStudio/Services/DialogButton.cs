
using LuaStudio.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.Services
{

    public class DialogButton : IDialogButton
    {
        public const int YesButtonId = 1;
        public const int NoButtonId = 2;
        public const int CancelButtonId = 3;
        public const int OkButtonId = 4;

        public DialogButton(String caption, bool isDefault = false, bool isCancel = false, Action<DialogButton> onInvoke = null)
        {
            this.Caption = caption;
            this.IsDefault = isDefault;
            this.IsCancel = isCancel;
            this.OnInvoke = onInvoke;
        }

        public static IDialogButton YesButton() { return new DialogButton(Locales.DialogButton_Yes_Caption) { ButtonId = YesButtonId }; }
        public static IDialogButton NoButton() { return new DialogButton(Locales.DialogButton_No_Caption) { ButtonId = NoButtonId }; }
        public static IDialogButton CancelButton() { return new DialogButton(Locales.DialogButton_Cancel_Caption) { ButtonId = CancelButtonId }; }
        public static IDialogButton OkButton() { return new DialogButton(Locales.DialogButton_Ok_Caption) { ButtonId = OkButtonId }; }
        public static IDialogButton[] YesNoButtons() { return new IDialogButton[] { YesButton(), NoButton() }; }
        public static IDialogButton[] YesNoCancelButtons() { return new IDialogButton[] { YesButton(), NoButton(), CancelButton() }; }

        public void Invoke()
        {
            if (OnInvoke != null)
                OnInvoke(this);
        }
        public int ButtonId { get; set; }
        public String Caption { get; set; }
        public bool IsDefault { get; set; }
        public bool IsCancel { get; set; }
        public Action<DialogButton> OnInvoke { get; set; }
    }

}
