using System;
using System.IO;
using System.Globalization;
using System.Resources;
using System.Reflection; 

namespace iGeospatial.Resources
{
	/// <summary>
	/// Summary description for Resources.
	/// </summary>
	public sealed class Resources
	{
        #region Private Members

        // This is the resource manager, for which this class is 
        // just a convenience wrapper
        private ResourceManager _resourceManager = null;

        private static readonly Resources _resource;

        #endregion

        #region Constructors and Destructor

        //  make constructor private so no one can directly create an 
        // instance of Resources, only use the Static Property ResourceManager
        private Resources()
        {
            _resourceManager = new ResourceManager(this.GetType().Namespace + ".Resources", 
                Assembly.GetExecutingAssembly());
        }

        //  static constructor private by nature.  
        // Initialize our read-only member _resourceManager here, 
        // there will only ever be one copy.
        static Resources()
        {
            _resource = new Resources();
        }

        #endregion
		
        #region Properties
        
        //  return the singleton instance of Resource
        public static Resources ResourceManager
        {
            get
            {
                return _resource;
            }
        }
		
        //  a convenience Indexer that access the internal resource manager
        public string this [string key]
        {
            get
            {
                return _resourceManager.GetString(key, CultureInfo.CurrentCulture);
            }
        }

        public string this [string key, params object[] par]
        {
            get
            {
                return string.Format(CultureInfo.CurrentUICulture, 
                    _resourceManager.GetString(key, CultureInfo.CurrentCulture), par);
            }
        }


        #endregion

        #region Methods
        
        public Stream GetStream(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        }

        public static ResourceManager CreateResourceManager(string name)
        {
            ResourceManager resourceManager = new ResourceManager(name, 
                Assembly.GetExecutingAssembly());

            return resourceManager;
        }

        #endregion
    }
}
