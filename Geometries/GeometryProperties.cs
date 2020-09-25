#region License
// <copyright>
//         iGeospatial Geometries Package
//       
// This is part of the Open Geospatial Library for .NET.
// 
// Package Description:
// This is a collection of C# classes that implement the fundamental 
// operations required to validate a given geo-spatial data set to 
// a known topological specification.
// It aims to provide a complete implementation of the Open Geospatial
// Consortium (www.opengeospatial.org) specifications for Simple 
// Feature Geometry.
// 
// Contact Information:
//     Paul Selormey (paulselormey@gmail.com or paul@toolscenter.org)
//     
// Credits:
// This library is based on the JTS Topology Suite, a Java library by
// 
//     Vivid Solutions Inc. (www.vividsolutions.com)  
//     
// License:
// See the license.txt file in the package directory.   
// </copyright>
#endregion

using System;
using System.Collections;

using iGeospatial.Collections;

namespace iGeospatial.Geometries
{
	/// <summary>
	/// Summary description for GeometryProperties.
	/// </summary>
    [Serializable]
    public class GeometryProperties : IGeometryProperties
    {
        #region Private Fields

        private int       m_nCount;
        private int       m_nVersion;
        private Entry     m_objRootEntry;
        private IComparer m_objComparer;
        
        #endregion
		
        #region Constructors and Destructor
        
        public GeometryProperties()
        {
        }
		
        public GeometryProperties(IComparer comparer)
        {
            m_objComparer = comparer;
        }
        
        #endregion
		
        #region Public Properties

        public int Count 
        {
            get 
            {
                return m_nCount;
            }
        }
		
        public bool IsSynchronized 
        {
            get 
            {
                return false;
            }
        }
		
        public object SyncRoot 
        {
            get 
            {
                return this;
            }
        }

        public object this[string key]
        {
            get 
            {
                Entry entry = FindEntry(key);
                return entry == null ? entry : entry.value;
            }
			
            set 
            {
                Entry entry = FindEntry(key);
                if (entry != null)
                    entry.value = value;
                else
                    InternalAdd(key, value);
            }
        }
		
        public bool IsFixedSize
        {
            get 
            {
                return false;
            }
        }
		
        public bool IsReadOnly
        {
            get 
            {
                return false;
            }
        }

        public IStringCollection Keys
        {
            get 
            {
                return new KeyCollection(this);
            }
        }
		
        public ICollection Values
        {
            get 
            {
                return new ValueCollection(this);
            }
        }
        
        #endregion
		
        #region Public Methods
		
        public void Add(string key, object value)
        {
            InternalAdd(key, value);
        }
		
        public void Clear()
        {
            m_objRootEntry = null;
            m_nCount = 0;
            m_nVersion++;
        }
		
        public bool Contains(string key)
        {
            return FindEntry(key) != null ? true : false;
        }
		
        public IDictionaryEnumerator GetEnumerator()
        {
            return new EntryEnumerator(this);
        }
		
        public void Remove(object key)
        {
            if (key == null)
                throw new ArgumentNullException("key",
                    "Key cannot be null.");

			
            Entry entry = m_objRootEntry;
			
            for (Entry prev = null; entry != null; prev = entry, entry = entry.next) 
            {
                if (InternalEquals(key, entry.key)) 
                {
                    if (prev != null) 
                    {
                        prev.next = entry.next;
                    } 
                    else 
                    {
                        m_objRootEntry = entry.next;
                    }
					
                    entry.value = null;
                    m_nCount--;
                    m_nVersion++;
                }
            }
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(
                    "array",
                    "Array cannot be null.");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "index is less than 0");

            int i = index;
            foreach ( DictionaryEntry entry in this )
                array.SetValue( entry, i++ );
        }
        
        #endregion
		
        #region Private Methods

        private bool InternalEquals(object obj1, object obj2)
        {
            if (m_objComparer != null) 
            {
                if (m_objComparer.Compare(obj1, obj2) == 0) 
                {
                    return true;
                }
            } 
            else 
            {
                if (obj1.Equals(obj2)) 
                {
                    return true;
                }
            }
			
            return false;
        }
		
        private Entry FindEntry(string key)
        {
            if (key == null) 
            {
                throw new ArgumentNullException("Attempted lookup for a null key.");
            }
			
            if (m_objRootEntry == null) 
            {
                return null;
            } 
            else 
            {
                Entry entry = m_objRootEntry;
				
                while (entry != null) 
                {
                    if (InternalEquals(key, entry.key)) 
                    {
                        return entry;
                    }
					
                    entry = entry.next;
                }
            }
			
            return null;
        }

        private void InternalAdd(string key, object value)
        {
            if (key == null) 
            {
                throw new ArgumentNullException("Attempted add with a null key.");
            }
			
            if (m_objRootEntry == null) 
            {
                m_objRootEntry       = new Entry();
                m_objRootEntry.key   = key;
                m_objRootEntry.value = value;
            } 
            else 
            {
                Entry entry = m_objRootEntry;
				
                while (entry != null) 
                {
                    if (InternalEquals(key, entry.key)) 
                    {
                        throw new ArgumentException("Duplicate key in add.");
                    }
					
                    if (entry.next == null) 
                    {
                        break;
                    }
					
                    entry = entry.next;
                }
				
                entry.next = new Entry();
                entry.next.key = key;
                entry.next.value = value;
            }
			
            m_nCount++;
            m_nVersion++;
        }
        
        #endregion

        #region Entry Class

        [Serializable]
        private sealed class Entry
        {
            public string key;
            public object value;
            public Entry  next;
        }
        
        #endregion

        #region EntryEnumerator Class

        private sealed class EntryEnumerator : IEnumerator, IDictionaryEnumerator
        {
            private GeometryProperties dict;
            private bool isAtStart;
            private Entry current;
            private int version;
			
            public EntryEnumerator(GeometryProperties dict)
            {
                this.dict = dict;
                version = dict.m_nVersion;

                Reset();
            }
				
            public bool MoveNext()
            {
                if (version != dict.m_nVersion) 
                {
                    throw new InvalidOperationException(
                        "The contents changed after this enumerator was instantiated.");
                }
				
                if (isAtStart) 
                {
                    current = dict.m_objRootEntry;
                    isAtStart = false;
                } 
                else 
                {
                    current = current.next;
                }
				
                return current != null ? true : false;	
            }
			
            public void Reset()
            {
                if (version != dict.m_nVersion) 
                {
                    throw new InvalidOperationException(
                        "The contents changed after this enumerator was instantiated.");
                }

                isAtStart = true;
                current = null;
            }
			
            public object Current
            {
                get 
                {
                    if (version != dict.m_nVersion) 
                    {
                        throw new InvalidOperationException(
                            "The contents changed after this enumerator was instantiated.");
                    }
					
                    if (isAtStart || current == null) 
                    {
                        throw new InvalidOperationException(
                            "Enumerator is positioned before the collection's first element or after the last element.");
                    }
					
                    return new DictionaryEntry(current.key, current.value);
                }
            }
			
            // IDictionaryEnumerator
            public DictionaryEntry Entry
            {
                get 
                {
                    if (version != dict.m_nVersion) 
                    {
                        throw new InvalidOperationException(
                            "The contents changed after this enumerator was instantiated.");
                    }

                    return (DictionaryEntry) Current;
                }
            }
			
            public object Key
            {
                get 
                {
                    if (version != dict.m_nVersion) 
                    {
                        throw new InvalidOperationException(
                            "The contents changed after this enumerator was instantiated.");
                    }
					
                    if (isAtStart || current == null) 
                    {
                        throw new InvalidOperationException(
                            "Enumerator is positioned before the collection's first element or after the last element.");
                    }
					
                    return current.key;
                }
            }
			
            public object Value
            {
                get 
                {
                    if (version != dict.m_nVersion) 
                    {
                        throw new InvalidOperationException(
                            "The contents changed after this enumerator was instantiated.");
                    }
					
                    if (isAtStart || current == null) 
                    {
                        throw new InvalidOperationException(
                            "Enumerator is positioned before the collection's first element or after the last element.");
                    }
					
                    return current.value;
                }
            }
        }
		
        
        #endregion

        #region KeyCollection Class

        private sealed class KeyCollection : IStringCollection
        {
            private GeometryProperties dict;
				
            public KeyCollection(GeometryProperties dict)
            {
                this.dict = dict;
            }
			
            // ICollection Interface
            public int Count 
            {
                get 
                {
                    return dict.Count;
                }
            }
			
            public bool IsSynchronized
            {
                get 
                {
                    return false;
                }
            }
			
            public object SyncRoot
            {
                get 
                {
                    return dict.SyncRoot;
                }
            }

            public void CopyTo(string[] array, int index)
            {
                int i = index;
                foreach (string obj in this)
                {
                    array[i++] = obj;
                }
            }

            public void CopyTo(Array array, int index)
            {
                int i = index;
                foreach (string obj in this)
                {
                    array.SetValue(obj, i++);
                }
            }
			
            // IEnumerable Interface
            public IStringEnumerator GetEnumerator()
            {
                return new EntryCollectionEnumerator(dict);
            }
			
            private sealed class EntryCollectionEnumerator : IStringEnumerator
            {
                private GeometryProperties dict;
                private bool isAtStart;
                private int version;
                private Entry current;
					
                public EntryCollectionEnumerator(GeometryProperties dict)
                {
                    this.dict = dict;
                    isAtStart = true;
                    version = dict.m_nVersion;
                }
				
                public string Current
                {
                    get 
                    {
                        if (version != dict.m_nVersion) 
                        {
                            throw new InvalidOperationException(
                                "The Collection's contents changed after this " +
                                "enumerator was instantiated.");
                        }
						
                        if (isAtStart || current == null) 
                        {
                            throw new InvalidOperationException(
                                "Enumerator is positioned before the collection's " +
                                "first element or after the last element.");
                        }
						
                        return current.key;
                    }
                }
				
                public bool MoveNext()
                {
                    if (version != dict.m_nVersion) 
                    {
                        throw new InvalidOperationException(
                            "The Collection's contents changed after this " +
                            "enumerator was instantiated.");
                    }
					
                    if (isAtStart) 
                    {
                        current = dict.m_objRootEntry;
                        isAtStart = false;
                    } 
                    else 
                    {
                        current = current.next;
                    }
					
                    return current != null ? true : false;
                }
				
                public void Reset()
                {
                    if (version != dict.m_nVersion) 
                    {
                        throw new InvalidOperationException(
                            "The Collection's contents changed after this " +
                            "enumerator was instantiated.");
                    }

                    isAtStart = true;
                    current = null;
                }
            }
        }
        
        #endregion

        #region ValueCollection Class

        private sealed class ValueCollection : ICollection
        {
            private GeometryProperties dict;
				
            public ValueCollection(GeometryProperties dict)
            {
                this.dict = dict;
            }
			
            #region ICollection Members

            public int Count 
            {
                get 
                {
                    return dict.Count;
                }
            }
			
            public bool IsSynchronized
            {
                get 
                {
                    return false;
                }
            }
			
            public object SyncRoot
            {
                get 
                {
                    return dict.SyncRoot;
                }
            }

            public void CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }

                int i = index;
                foreach (object obj in this )
                    array.SetValue(obj, i++);
            }
            
            #endregion
			
            // IEnumerable Interface
            public IEnumerator GetEnumerator()
            {
                return new EntryCollectionEnumerator(dict);
            }
			
            private sealed class EntryCollectionEnumerator : IEnumerator
            {
                private GeometryProperties dict;
                private bool isAtStart;
                private int version;
                private Entry current;
					
                public EntryCollectionEnumerator(GeometryProperties dict)
                {
                    this.dict = dict;
                    isAtStart = true;
                    version = dict.m_nVersion;
                }
				
                public object Current
                {
                    get 
                    {
                        if (version != dict.m_nVersion) 
                        {
                            throw new InvalidOperationException(
                                "The Collection's contents changed after this " +
                                "enumerator was instantiated.");
                        }
						
                        if (isAtStart || current == null) 
                        {
                            throw new InvalidOperationException(
                                "Enumerator is positioned before the collection's " +
                                "first element or after the last element.");
                        }
						
                        return current.value;
                    }
                }
				
                public bool MoveNext()
                {
                    if (version != dict.m_nVersion) 
                    {
                        throw new InvalidOperationException(
                            "The Collection's contents changed after this " +
                            "enumerator was instantiated.");
                    }
					
                    if (isAtStart) 
                    {
                        current = dict.m_objRootEntry;
                        isAtStart = false;
                    } 
                    else 
                    {
                        current = current.next;
                    }
					
                    return current != null ? true : false;
                }
				
                public void Reset()
                {
                    if (version != dict.m_nVersion) 
                    {
                        throw new InvalidOperationException(
                            "The Collection's contents changed after this " +
                            "enumerator was instantiated.");
                    }

                    isAtStart = true;
                    current = null;
                }
            }
        }
        
        #endregion
    }
}
