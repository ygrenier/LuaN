using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LuaStudio.ViewModels
{
    public class AppViewModel : ViewModel
    {
        int _NewScriptCount;

        public AppViewModel()
        {
            Scripts = new ObservableCollection<ScriptFileViewModel>();
            NewScriptCommand = new RelayCommand(() => NewScript());
        }

        public ScriptFileViewModel NewScript()
        {
            ScriptFileViewModel result = new ScriptFileViewModel();
            Scripts.Add(result);
            result.Filename = String.Format("NoName{0}", ++_NewScriptCount);
            ActiveScript = result;
            return result;
        }

        public ObservableCollection<ScriptFileViewModel> Scripts { get; private set; }

        public ScriptFileViewModel ActiveScript
        {
            get { return _ActiveScript; }
            set { SetProperty(ref _ActiveScript, value, () => ActiveScript); }
        }
        private ScriptFileViewModel _ActiveScript;

        public RelayCommand NewScriptCommand { get; private set; }

    }
}
