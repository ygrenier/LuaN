using LuaStudio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaStudio.Services;
using LuaStudio.Resources;

namespace LuaStudio.Dialogs
{
    /// <summary>
    /// ViewModel for confirm dialog box
    /// </summary>
    public class ConfirmDialogViewModel : ViewModel
    {
        public ConfirmDialogViewModel()
        {
            Title = "Title";
            Message = "Message.";
            Buttons = null;
            ButtonClickCommand = new RelayCommand<IDialogButton>(
                btn =>
                {
                    LastClickedButton = btn;
                    if (btn != null)
                        btn.Invoke();
                }
                );
        }

        private IDialogButton[] CleanButtons(IDialogButton[] buttons)
        {
            buttons = (buttons ?? new IDialogButton[0]).Where(b => b != null).ToArray();
            if (buttons.Length == 0)
            {
                buttons = new IDialogButton[]
                {
                    new DialogButton(Locales.DialogButton_Yes_Caption, true),
                    new DialogButton(Locales.DialogButton_No_Caption)
                };
            }
            var btn = buttons.FirstOrDefault(b => b.IsDefault) ?? buttons.FirstOrDefault();
            foreach (var b in buttons) b.IsDefault = b == btn;
            btn = buttons.FirstOrDefault(b => b.IsCancel) ?? buttons.LastOrDefault();
            foreach (var b in buttons) b.IsCancel = b == btn;
            return buttons;
        }

        public String Title
        {
            get { return _Title; }
            set { SetProperty(ref _Title, value, () => Title); }
        }
        private String _Title;

        public String Message
        {
            get { return _Message; }
            set { SetProperty(ref _Message, value, () => Message); }
        }
        private String _Message;

        public IDialogButton LastClickedButton
        {
            get { return _LastClickedButton; }
            set { SetProperty(ref _LastClickedButton, value, () => LastClickedButton); }
        }
        private IDialogButton _LastClickedButton;

        public Services.IDialogButton[] Buttons
        {
            get { return _Buttons; }
            set {
                _Buttons = CleanButtons(value);
                RaisePropertyChanged(() => Buttons);
            }
        }
        private Services.IDialogButton[] _Buttons;

        public RelayCommand<IDialogButton> ButtonClickCommand { get; private set; }

    }
}
