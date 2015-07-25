using LuaStudio.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio
{
    public static class DialogExtensions
    {
        public static IDialogButton[] IsDefault(this IDialogButton[] buttons, int id)
        {
            if(buttons!= null)
            {
                foreach (var btn in buttons)
                    btn.IsDefault = btn.ButtonId == id;
            }
            return buttons;                
        }
        public static IDialogButton[] IsCancel(this IDialogButton[] buttons, int id)
        {
            if (buttons != null)
            {
                foreach (var btn in buttons)
                    btn.IsCancel = btn.ButtonId == id;
            }
            return buttons;
        }
    }
}
