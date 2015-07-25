using LuaStudio.Services;
using LuaStudio.TextEditors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LuaStudio.ViewModels.Tools
{

    public class SnippetsToolViewModel : ToolViewModel
    {
        IMessageSubscription<DocumentNotifyMessage> _Subscription;

        public SnippetsToolViewModel()
        {
            this.Snippets = new ObservableCollection<SnippetDefinition>();
            Title = "Snippets";
            _Subscription = AppContext.Current.Messenger.Subscribe<DocumentNotifyMessage>(OnDocumentNotify);
        }

        private void OnDocumentNotify(DocumentNotifyMessage message)
        {
            if(message!=null && message.Notification == DocumentNotification.IsActive)
            {
                Snippets.Clear();
                Documents.TextEditorViewModel tEditor = message.Document as Documents.TextEditorViewModel;
                if(tEditor!=null && tEditor.TextDefinition!= null)
                {
                    foreach (var snippet in tEditor.TextDefinition.GetSnippets().OrderBy(s=>s.Word))
                    {
                        Snippets.Add(snippet);
                    }
                }
            }
        }

        public ObservableCollection<SnippetDefinition> Snippets { get; private set; }
    }

}
