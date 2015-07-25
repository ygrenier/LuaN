using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;

namespace LuaStudio
{
    /// <summary>
    /// Context application
    /// </summary>
    public class AppContext
    {
        static AppContext _Current;
        CompositionContainer _Container;

        [ImportMany]
        IEnumerable<Lazy<TextEditors.ITextDefinition>> _TextDefinitions = null;

        /// <summary>
        /// Hide the constructor
        /// </summary>
        AppContext() {
            // Create catalogs
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(AppContext).Assembly));
            // Build MEF container
            _Container = new CompositionContainer(catalog);
            // Compose the context
            _Container.ComposeParts(this);
        }

        /// <summary>
        /// Build a new context
        /// </summary>
        static AppContext BuildContext()
        {
            return new AppContext();
        }

        /// <summary>
        /// List the text definitions availables
        /// </summary>
        public IEnumerable<TextEditors.ITextDefinition> GetTextDefinitions()
        {
            return _TextDefinitions.Select(l => l.Value);
        }

        /// <summary>
        /// Get a service
        /// </summary>
        public T GetService<T>()
        {
            return _Container.GetExportedValue<T>();
        }

        /// <summary>
        /// Current singleton instance
        /// </summary>
        public static AppContext Current
        {
            get {
                if (_Current == null)
                    _Current = BuildContext();
                return _Current;
            }
        }
    }
}
