using System;
using System.Collections;

using iGeospatial.Collections;

namespace iGeospatial.Geometries
{
	/// <summary>
	/// Summary description for IGeometryProperties.
	/// </summary>
	public interface IGeometryProperties
	{
        #region Properties

        int Count 
        { 
            get; 
        }

        object this[string key] 
        { 
            get; 
            set; 
        }

        bool IsFixedSize 
        { 
            get; 
        }

        bool IsReadOnly 
        { 
            get; 
        }

        bool IsSynchronized 
        { 
            get; 
        }

        object SyncRoot 
        { 
            get; 
        }

        IStringCollection Keys 
        { 
            get; 
        }

        ICollection Values 
        { 
            get; 
        }
        
        #endregion

        #region Methods

        void Add(string key, object value);
        void Clear();
        bool Contains(string key);
        void CopyTo(Array array, int index);
        void Remove(object key);
        IDictionaryEnumerator GetEnumerator();
        
        #endregion
    }
}
