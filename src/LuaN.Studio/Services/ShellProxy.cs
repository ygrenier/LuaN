using LuaN.Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaN.Studio.Services
{
    class ShellProxy : IShell
    {
        IShell _Source;

        public ShellProxy(IShell source)
        {
            _Source = source;
        }

        public int DocumentCount { get { return _Source.DocumentCount; } }

        public int ToolCount { get { return _Source.ToolCount; } }

        public ITool FindTool(string name)
        {
            return _Source.FindTool(name);
        }

        public ViewModels.Documents.ILuaScriptDocumentViewModel NewLuaScript()
        {
            return _Source.NewLuaScript();
        }

        public IDocument GetDocument(int idx)
        {
            return _Source.GetDocument(idx);
        }

        public ITool GetTool(int idx)
        {
            return _Source.GetTool(idx);
        }

        public IDocument ActiveDocument { get { return _Source.ActiveDocument; } }

    }
}
