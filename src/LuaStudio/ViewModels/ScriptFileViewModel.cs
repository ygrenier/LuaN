using ICSharpCode.AvalonEdit.Document;
using System;

namespace LuaStudio.ViewModels
{

    public class ScriptFileViewModel : ViewModel
    {

        public ScriptFileViewModel()
        {
            ScriptContent = new TextDocument();
        }

        private void ScriptContent_Changed(object sender, DocumentChangeEventArgs e)
        {
            Modified = true;
        }

        public TextDocument ScriptContent
        {
            get { return _ScriptContent; }
            set {
                if (_ScriptContent != null)
                    _ScriptContent.Changed -= ScriptContent_Changed;
                SetProperty(ref _ScriptContent, value, () => ScriptContent);
                if (_ScriptContent != null)
                    _ScriptContent.Changed += ScriptContent_Changed;
            }
        }
        private TextDocument _ScriptContent;

        public String Filename
        {
            get { return _Filename; }
            set { SetProperty(ref _Filename, value, () => Filename); }
        }
        private String _Filename;

        public bool Modified
        {
            get { return _Modified; }
            private set { SetProperty(ref _Modified, value, () => Modified); }
        }
        private bool _Modified = false;

    }

}