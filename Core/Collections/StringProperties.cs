using System;
using System.Collections;
namespace iGeospatial.Collections
{
    public class StringProperties : IDictionary, ICollection, IEnumerable, ICloneable
    {
        protected Hashtable innerHash;
		
        #region "Constructors"
        public  StringProperties()
        {
            innerHash = new Hashtable();
        }
		
        public StringProperties(StringProperties original)
        {
            innerHash = new Hashtable (original.innerHash);
        }
		
        public StringProperties(IDictionary dictionary)
        {
            innerHash = new Hashtable (dictionary);
        }
		
        public StringProperties(int capacity)
        {
            innerHash = new Hashtable(capacity);
        }
		
        public StringProperties(IDictionary dictionary, float loadFactor)
        {
            innerHash = new Hashtable(dictionary, loadFactor);
        }
		
        public StringProperties(IHashCodeProvider codeProvider, IComparer comparer)
        {
            innerHash = new Hashtable (codeProvider, comparer);
        }
		
        public StringProperties(int capacity, int loadFactor)
        {
            innerHash = new Hashtable(capacity, loadFactor);
        }
		
        public StringProperties(IDictionary dictionary, IHashCodeProvider codeProvider, IComparer comparer)
        {
            innerHash = new Hashtable (dictionary, codeProvider, comparer);
        }
		
        public StringProperties(int capacity, IHashCodeProvider codeProvider, IComparer comparer)
        {
            innerHash = new Hashtable (capacity, codeProvider, comparer);
        }
		
        public StringProperties(IDictionary dictionary, float loadFactor, IHashCodeProvider codeProvider, IComparer comparer)
        {
            innerHash = new Hashtable (dictionary, loadFactor, codeProvider, comparer);
        }
		
        public StringProperties(int capacity, float loadFactor, IHashCodeProvider codeProvider, IComparer comparer)
        {
            innerHash = new Hashtable (capacity, loadFactor, codeProvider, comparer);
        }
        #endregion

        #region Implementation of IDictionary
        public StringPropertiesEnumerator GetEnumerator()
        {
            return new StringPropertiesEnumerator(this);
        }
        	
        System.Collections.IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new StringPropertiesEnumerator(this);
        }
		
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
		
        public void Remove(String key)
        {
            innerHash.Remove (key);
        }
		
        void IDictionary.Remove(object key)
        {
            Remove ((String)key);
        }
		
        public bool Contains(String key)
        {
            return innerHash.Contains(key);
        }
		
        bool IDictionary.Contains(object key)
        {
            return Contains((String)key);
        }
		
        public void Clear()
        {
            innerHash.Clear();		
        }
		
        public void Add(String key, String value)
        {
            innerHash.Add (key, value);
        }
		
        void IDictionary.Add(object key, object value)
        {
            Add ((String)key, (String)value);
        }
		
        public bool IsReadOnly
        {
            get
            {
                return innerHash.IsReadOnly;
            }
        }
		
        public String this[String key]
        {
            get
            {
                return (String) innerHash[key];
            }
            set
            {
                innerHash[key] = value;
            }
        }
		
        object IDictionary.this[object key]
        {
            get
            {
                return this[(String)key];
            }
            set
            {
                this[(String)key] = (String)value;
            }
        }
        	
        public System.Collections.ICollection Values
        {
            get
            {
                return innerHash.Values;
            }
        }
		
        public System.Collections.ICollection Keys
        {
            get
            {
                return innerHash.Keys;
            }
        }
		
        public bool IsFixedSize
        {
            get
            {
                return innerHash.IsFixedSize;
            }
        }
        #endregion
		
        #region Implementation of ICollection
        public void CopyTo(System.Array array, int index)
        {
            innerHash.CopyTo (array, index);
        }
		
        public bool IsSynchronized
        {
            get
            {
                return innerHash.IsSynchronized;
            }
        }
		
        public int Count
        {
            get
            {
                return innerHash.Count;
            }
        }
		
        public object SyncRoot
        {
            get
            {
                return innerHash.SyncRoot;
            }
        }
        #endregion
		
        #region Implementation of ICloneable
        public StringProperties Clone()
        {
            StringProperties clone = new StringProperties();
            clone.innerHash = (Hashtable) innerHash.Clone();
			
            return clone;
        }
		
        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
		
        #region "HashTable Methods"
        public bool ContainsKey (String key)
        {
            return innerHash.ContainsKey(key);
        }
		
        public bool ContainsValue (String value)
        {
            return innerHash.ContainsValue(value);
        }
		
        public static StringProperties Synchronized(StringProperties nonSync)
        {
            StringProperties sync = new StringProperties();
            sync.innerHash = Hashtable.Synchronized(nonSync.innerHash);

            return sync;
        }
        #endregion

        internal Hashtable InnerHash
        {
            get
            {
                return innerHash;
            }
        }
    }
	
    public class StringPropertiesEnumerator : IDictionaryEnumerator
    {
        private IDictionaryEnumerator innerEnumerator;
		
        internal StringPropertiesEnumerator (StringProperties enumerable)
        {
            innerEnumerator = enumerable.InnerHash.GetEnumerator();
        }
		
        #region Implementation of IDictionaryEnumerator
        public String Key
        {
            get
            {
                return (String)innerEnumerator.Key;
            }
        }
		
        object IDictionaryEnumerator.Key
        {
            get
            {
                return Key;
            }
        }
		
        public String Value
        {
            get
            {
                return (String)innerEnumerator.Value;
            }
        }
		
        object IDictionaryEnumerator.Value
        {
            get
            {
                return Value;
            }
        }
		
        public System.Collections.DictionaryEntry Entry
        {
            get
            {
                return innerEnumerator.Entry;
            }
        }
        #endregion
		
        #region Implementation of IEnumerator
        public void Reset()
        {
            innerEnumerator.Reset();
        }
		
        public bool MoveNext()
        {
            return innerEnumerator.MoveNext();
        }
		
        public object Current
        {
            get
            {
                return innerEnumerator.Current;
            }
        }
        #endregion
    }
}
