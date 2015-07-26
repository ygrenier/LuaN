using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaStudio.ViewModels
{
    public abstract class DockContentViewModel : ViewModel
    {
        /// <summary>
        /// Title
        /// </summary>
        public String Title
        {
            get { return _Title; }
            set { SetProperty(ref _Title, value, () => Title); }
        }
        private String _Title;

        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive
        {
            get { return _IsActive; }
            set { SetProperty(ref _IsActive, value, () => IsActive); }
        }
        private bool _IsActive;
    }
}
