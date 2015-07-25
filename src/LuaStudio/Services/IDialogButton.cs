using System;

namespace LuaStudio.Services
{

    public interface IDialogButton
    {
        int ButtonId { get; set; }
        void Invoke();
        String Caption { get; set; }
        bool IsDefault { get; set; }
        bool IsCancel { get; set; }
    }

}