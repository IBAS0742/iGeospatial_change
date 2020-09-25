using System;
using System.Collections;

namespace iGeospatial.Collections
{
    /// <summary>
    ///		A strongly-typed collection of <see cref="Int32"/> objects.
    /// </summary>
    [Serializable]
    public class IntegerCollection : IList, ICloneable
    {
        #region Private Fields
                                     
        private const int DefaultCapacity = 16;

        private Int32[] m_array;
        private int m_count = 0;
        
        [NonSerialized]
        private int m_version = 0;

        #endregion
	
        #region Static Wrappers
        /// <summary>
        ///		Creates a synchronized (thread-safe) wrapper for a 
        ///     <c>IntegerCollection</c> instance.
        /// </summary>
        /// <returns>
        ///     An <c>IntegerCollection</c> wrapper that is synchronized (thread-safe).
        /// </returns>
        public static IntegerCollection Synchronized(IntegerCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new SyncIntegerCollection(list);
        }
		
        /// <summary>
        ///		Creates a read-only wrapper for a 
        ///     <c>IntegerCollection</c> instance.
        /// </summary>
        /// <returns>
        ///     An <c>IntegerCollection</c> wrapper that is read-only.
        /// </returns>
        public static IntegerCollection ReadOnly(IntegerCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");
            return new ReadOnlyIntegerCollection(list);
        }
        #endregion

        #region Constructors and Destructor

        /// <summary>
        ///		Initializes a new instance of the <c>IntegerCollection</c> class
        ///		that is empty and has the default initial capacity.
        /// </summary>
        public IntegerCollection()
        {
            m_array = new Int32[DefaultCapacity];
        }
		
        /// <summary>
        ///		Initializes a new instance of the <c>IntegerCollection</c> class
        ///		that has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///		The number of elements that the new <c>IntegerCollection</c> is initially capable of storing.
        ///	</param>
        public IntegerCollection(int capacity)
        {
            m_array = new Int32[capacity];
        }

        /// <summary>
        ///		Initializes a new instance of the <c>IntegerCollection</c> class
        ///		that contains elements copied from the specified <c>IntegerCollection</c>.
        /// </summary>
        /// <param name="c">The <c>IntegerCollection</c> whose elements are copied to the new collection.</param>
        public IntegerCollection(IntegerCollection c)
        {
            m_array = new Int32[c.Count];
            AddRange(c);
        }

        /// <summary>
        ///		Initializes a new instance of the <c>IntegerCollection</c> class
        ///		that contains elements copied from the specified <see cref="Int32"/> array.
        /// </summary>
        /// <param name="a">The <see cref="Int32"/> array whose elements are copied to the new list.</param>
        public IntegerCollection(Int32[] a)
        {
            m_array = new Int32[a.Length];
            AddRange(a);
        }
		
        internal enum Tag 
        {
            Default
        }

        internal IntegerCollection(Tag t)
        {
            m_array = null;
        }

        #endregion
		
        #region Public Properties

        /// <summary>
        ///		Gets the number of elements actually contained in the <c>IntegerCollection</c>.
        /// </summary>
        public virtual int Count
        {
            get { return m_count; }
        }

        /// <summary>
        ///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
        /// </summary>
        /// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get { return m_array.IsSynchronized; }
        }

        /// <summary>
        ///		Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        public virtual object SyncRoot
        {
            get { return m_array.SyncRoot; }
        }

        /// <summary>
        ///		Gets or sets the <see cref="Int32"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="IntegerCollection.Count"/>.</para>
        /// </exception>
        public virtual Int32 this[int index]
        {
            get
            {
                ValidateIndex(index); // throws
                return m_array[index]; 
            }
            set
            {
                ValidateIndex(index); // throws
                ++m_version; 
                m_array[index] = value; 
            }
        }

        /// <summary>
        ///		Gets a value indicating whether the collection has a fixed size.
        /// </summary>
        /// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        ///		gets a value indicating whether the <B>IList</B> is read-only.
        /// </summary>
        /// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly
        {
            get { return false; }
        }
		
        /// <summary>
        ///		Gets or sets the number of elements the <c>IntegerCollection</c> can contain.
        /// </summary>
        public virtual int Capacity
        {
            get { return m_array.Length; }
			
            set
            {
                if (value < m_count)
                    value = m_count;

                if (value != m_array.Length)
                {
                    if (value > 0)
                    {
                        Int32[] temp = new Int32[value];
                        Array.Copy(m_array, temp, m_count);
                        m_array = temp;
                    }
                    else
                    {
                        m_array = new Int32[DefaultCapacity];
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///		Adds a <see cref="Int32"/> to the end of the <c>IntegerCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="Int32"/> to be added to the end of the <c>IntegerCollection</c>.</param>
        /// <returns>The index at which the value has been added.</returns>
        public virtual int Add(Int32 item)
        {
            if (m_count == m_array.Length)
                EnsureCapacity(m_count + 1);

            m_array[m_count] = item;
            m_version++;

            return m_count++;
        }
		
        /// <summary>
        ///		Removes all elements from the <c>IntegerCollection</c>.
        /// </summary>
        public virtual void Clear()
        {
            ++m_version;
            m_array = new Int32[DefaultCapacity];
            m_count = 0;
        }

        /// <summary>
        ///		Determines whether a given <see cref="Int32"/> is in the <c>IntegerCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="Int32"/> to check for.</param>
        /// <returns><c>true</c> if <paramref name="item"/> is found in the <c>IntegerCollection</c>; otherwise, <c>false</c>.</returns>
        public virtual bool Contains(Int32 item)
        {
            for (int i=0; i != m_count; ++i)
            {
                if (m_array[i].Equals(item))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///		Copies the entire <c>IntegerCollection</c> to a one-dimensional
        ///		<see cref="Int32"/> array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Int32"/> array to copy to.</param>
        public virtual void CopyTo(Int32[] array)
        {
            this.CopyTo(array, 0);
        }

        /// <summary>
        ///		Copies the entire <c>IntegerCollection</c> to a one-dimensional
        ///		<see cref="Int32"/> array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Int32"/> array to copy to.</param>
        /// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public virtual void CopyTo(Int32[] array, int start)
        {
            if (m_count > array.GetUpperBound(0) + 1 - start)
                throw new System.ArgumentException("Destination array was not long enough.");
			
            Array.Copy(m_array, 0, array, start, m_count); 
        }

        /// <summary>
        ///		Returns the zero-based index of the first occurrence of a <see cref="Int32"/>
        ///		in the <c>IntegerCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="Int32"/> to locate in the <c>IntegerCollection</c>.</param>
        /// <returns>
        ///		The zero-based index of the first occurrence of <paramref name="item"/> 
        ///		in the entire <c>IntegerCollection</c>, if found; otherwise, -1.
        ///	</returns>
        public virtual int IndexOf(Int32 item)
        {
            for (int i=0; i != m_count; ++i)
            {
                if (m_array[i].Equals(item))
                    return i;
            }

            return -1;
        }

        /// <summary>
        ///		Inserts an element into the <c>IntegerCollection</c> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="Int32"/> to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="IntegerCollection.Count"/>.</para>
        /// </exception>
        public virtual void Insert(int index, Int32 item)
        {
            ValidateIndex(index, true); // throws
			
            if (m_count == m_array.Length)
                EnsureCapacity(m_count + 1);

            if (index < m_count)
            {
                Array.Copy(m_array, index, m_array, index + 1, m_count - index);
            }

            m_array[index] = item;
            m_count++;
            m_version++;
        }

        /// <summary>
        ///		Removes the first occurrence of a specific <see cref="Int32"/> from the <c>IntegerCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="Int32"/> to remove from the <c>IntegerCollection</c>.</param>
        /// <exception cref="ArgumentException">
        ///		The specified <see cref="Int32"/> was not found in the <c>IntegerCollection</c>.
        /// </exception>
        public virtual void Remove(Int32 item)
        {		   
            int i = IndexOf(item);
            if (i < 0)
                throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
            ++m_version;
            RemoveAt(i);
        }

        /// <summary>
        ///		Removes the element at the specified index of the <c>IntegerCollection</c>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="IntegerCollection.Count"/>.</para>
        /// </exception>
        public virtual void RemoveAt(int index)
        {
            ValidateIndex(index); // throws
			
            m_count--;

            if (index < m_count)
            {
                Array.Copy(m_array, index + 1, m_array, index, m_count - index);
            }
			
            // We can't set the deleted entry equal to null, because it might be a value type.
            // Instead, we'll create an empty single-element array of the right type and copy it 
            // over the entry we want to erase.
            Int32[] temp = new Int32[1];
            Array.Copy(temp, 0, m_array, m_count, 1);
            m_version++;
        }

        /// <summary>
        ///		Adds the elements of another <c>IntegerCollection</c> to the current <c>IntegerCollection</c>.
        /// </summary>
        /// <param name="x">The <c>IntegerCollection</c> whose elements should be added to the end of the current <c>IntegerCollection</c>.</param>
        /// <returns>The new <see cref="IntegerCollection.Count"/> of the <c>IntegerCollection</c>.</returns>
        public virtual int AddRange(IntegerCollection x)
        {
            if (m_count + x.Count >= m_array.Length)
                EnsureCapacity(m_count + x.Count);
			
            Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
            m_count += x.Count;
            m_version++;

            return m_count;
        }

        /// <summary>
        ///		Adds the elements of a <see cref="Int32"/> array to the current <c>IntegerCollection</c>.
        /// </summary>
        /// <param name="x">The <see cref="Int32"/> array whose elements should be added to the end of the <c>IntegerCollection</c>.</param>
        /// <returns>The new <see cref="IntegerCollection.Count"/> of the <c>IntegerCollection</c>.</returns>
        public virtual int AddRange(Int32[] x)
        {
            if (m_count + x.Length >= m_array.Length)
                EnsureCapacity(m_count + x.Length);

            Array.Copy(x, 0, m_array, m_count, x.Length);
            m_count += x.Length;
            m_version++;

            return m_count;
        }
		
        /// <summary>
        ///		Sets the capacity to the actual number of elements.
        /// </summary>
        public virtual void TrimToSize()
        {
            this.Capacity = m_count;
        }

        /// <summary>
        /// Copies the elements of the <see cref="IntegerCollection"/> to a new
        /// <see cref="Array"/> of <see cref="Int32"/> elements.
        /// </summary>
        /// <returns>
        /// A one-dimensional <see cref="Array"/> of <see cref="Int32"/>
        /// elements containing copies of the elements of the <see cref="IntegerCollection"/>.
        /// </returns>
        /// <remarks>Please refer to <see cref="ArrayList.ToArray"/> for details.</remarks>
        public virtual int[] ToArray() 
        {
            int[] array = new int[m_count];
            Array.Copy(m_array, array, m_count);

            return array;
        }

        #endregion

        #region Private Methods

        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="IntegerCollection.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int i)
        {
            ValidateIndex(i, false);
        }

        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="IntegerCollection.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int i, bool allowEqualEnd)
        {
            int max = (allowEqualEnd)?(m_count):(m_count-1);
            if (i < 0 || i > max)
                throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
        }

        private void EnsureCapacity(int min)
        {
            int newCapacity = ((m_array.Length == 0) ? DefaultCapacity : m_array.Length * 2);
            if (newCapacity < min)
                newCapacity = min;

            this.Capacity = newCapacity;
        }

        #endregion
		
        #region ICollection Members

        void ICollection.CopyTo(Array array, int start)
        {
            this.CopyTo((Int32[])array, start);
        }

        #endregion

        #region IList Members

        object IList.this[int i]
        {
            get { return (object)this[i]; }
            set { this[i] = (Int32)value; }
        }

        int IList.Add(object x)
        {
            return this.Add((Int32)x);
        }

        bool IList.Contains(object x)
        {
            return this.Contains((Int32)x);
        }

        int IList.IndexOf(object x)
        {
            return this.IndexOf((Int32)x);
        }

        void IList.Insert(int pos, object x)
        {
            this.Insert(pos, (Int32)x);
        }

        void IList.Remove(object x)
        {
            this.Remove((Int32)x);
        }

        void IList.RemoveAt(int pos)
        {
            this.RemoveAt(pos);
        }

        #endregion

        #region IEnumerable Members
		
        /// <summary>
        ///		Returns an enumerator that can iterate through the <c>IntegerCollection</c>.
        /// </summary>
        /// <returns>An <see cref="IntegerEnumerator"/> for the entire <c>IntegerCollection</c>.</returns>
        public virtual IIntegerEnumerator GetEnumerator()
        {
            return new IntegerEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)(this.GetEnumerator());
        }

        #endregion
		
        #region ICloneable Members

        /// <summary>
        ///	Creates a deep copy of the <see cref="IntegerCollection"/>.
        /// </summary>
        public virtual IntegerCollection Clone()
        {
            IntegerCollection newColl = new IntegerCollection(m_count);
            Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
            newColl.m_count = m_count;
            newColl.m_version = m_version;

            return newColl;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        
        #endregion

        #region Nested enumerator class
        /// <summary>
        ///		Supports simple iteration over a <see cref="IntegerCollection"/>.
        /// </summary>
        private class IntegerEnumerator : IEnumerator, IIntegerEnumerator
        {
            #region Implementation (data)
			
            private IntegerCollection m_collection;
            private int m_index;
            private int m_version;
			
            #endregion
		
            #region Construction
			
            /// <summary>
            ///		Initializes a new instance of the <c>IntegerEnumerator</c> class.
            /// </summary>
            /// <param name="tc"></param>
            internal IntegerEnumerator(IntegerCollection tc)
            {
                m_collection = tc;
                m_index = -1;
                m_version = tc.m_version;
            }
			
            #endregion
	
            #region Operations (type-safe IEnumerator)
			
            /// <summary>
            ///		Gets the current element in the collection.
            /// </summary>
            public Int32 Current
            {
                get { return m_collection[m_index]; }
            }

            /// <summary>
            ///		Advances the enumerator to the next element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            ///		The collection was modified after the enumerator was created.
            /// </exception>
            /// <returns>
            ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
            ///		<c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            public bool MoveNext()
            {
                if (m_version != m_collection.m_version)
                    throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                ++m_index;
                return (m_index < m_collection.Count) ? true : false;
            }

            /// <summary>
            ///		Sets the enumerator to its initial position, before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                m_index = -1;
            }
            #endregion
	
            #region Implementation (IEnumerator)
			
            object IEnumerator.Current
            {
                get { return (object)(this.Current); }
            }
			
            #endregion
        }
        #endregion
		
        #region Nested Syncronized Wrapper class
        [Serializable]
            private class SyncIntegerCollection : IntegerCollection, System.Runtime.Serialization.IDeserializationCallback
        {
            #region Implementation (data)
            private const int timeout = 0; // infinite
            private IntegerCollection collection;
            [NonSerialized]
            private System.Threading.ReaderWriterLock rwLock;
            #endregion

            #region Construction
            internal SyncIntegerCollection(IntegerCollection list) : base(Tag.Default)
            {
                rwLock = new System.Threading.ReaderWriterLock();
                collection = list;
            }
            #endregion
			
            #region IDeserializationCallback Members
            void System.Runtime.Serialization.IDeserializationCallback.OnDeserialization(object sender)
            {
                rwLock = new System.Threading.ReaderWriterLock();
            }
            #endregion
			
            #region Type-safe ICollection
            public override void CopyTo(Int32[] array)
            {
                rwLock.AcquireReaderLock(timeout);

                try
                {
                    collection.CopyTo(array);
                }
                finally
                {
                    rwLock.ReleaseReaderLock();
                }
            }

            public override void CopyTo(Int32[] array, int start)
            {
                rwLock.AcquireReaderLock(timeout);

                try
                {
                    collection.CopyTo(array, start);
                }
                finally
                {
                    rwLock.ReleaseReaderLock();
                }
            }
			
            public override int Count
            {
                get
                {
                    int count = 0;
                    rwLock.AcquireReaderLock(timeout);

                    try
                    {
                        count = collection.Count;
                    }
                    finally
                    {
                        rwLock.ReleaseReaderLock();
                    }
					
                    return count;
                }
            }

            public override bool IsSynchronized
            {
                get { return true; }
            }

            public override object SyncRoot
            {
                get { return collection.SyncRoot; }
            }
            #endregion
			
            #region Type-safe IList
            public override Int32 this[int i]
            {
                get
                {
                    Int32 thisItem;
                    rwLock.AcquireReaderLock(timeout);

                    try
                    {
                        thisItem = collection[i];
                    }
                    finally
                    {
                        rwLock.ReleaseReaderLock();
                    }
					
                    return thisItem;
                }
				
                set
                {
                    rwLock.AcquireWriterLock(timeout);

                    try
                    {
                        collection[i] = value;
                    }
                    finally
                    {
                        rwLock.ReleaseWriterLock();
                    }
                }
            }

            public override int Add(Int32 x)
            {
                int result = 0;
                rwLock.AcquireWriterLock(timeout);

                try
                {
                    result = collection.Add(x);
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }

                return result;
            }
			
            public override void Clear()
            {
                rwLock.AcquireWriterLock(timeout);

                try
                {
                    collection.Clear();
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }
            }

            public override bool Contains(Int32 x)
            {
                bool result = false;
                rwLock.AcquireReaderLock(timeout);

                try
                {
                    result = collection.Contains(x);
                }
                finally
                {
                    rwLock.ReleaseReaderLock();
                }

                return result;
            }

            public override int IndexOf(Int32 x)
            {
                int result = 0;
                rwLock.AcquireReaderLock(timeout);

                try
                {
                    result = collection.IndexOf(x);
                }
                finally
                {
                    rwLock.ReleaseReaderLock();
                }

                return result;
            }

            public override void Insert(int pos, Int32 x)
            {
                rwLock.AcquireWriterLock(timeout);

                try
                {
                    collection.Insert(pos,x);
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }
            }

            public override void Remove(Int32 x)
            {           
                rwLock.AcquireWriterLock(timeout);

                try
                {
                    collection.Remove(x);
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }
            }

            public override void RemoveAt(int pos)
            {
                rwLock.AcquireWriterLock(timeout);

                try
                {
                    collection.RemoveAt(pos);
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }
            }
			
            public override bool IsFixedSize
            {
                get { return collection.IsFixedSize; }
            }

            public override bool IsReadOnly
            {
                get { return collection.IsReadOnly; }
            }
            #endregion

            #region Type-safe IEnumerable
            public override IIntegerEnumerator GetEnumerator()
            {
                IIntegerEnumerator enumerator = null;
                rwLock.AcquireReaderLock(timeout);

                try
                {
                    enumerator = collection.GetEnumerator();
                }
                finally
                {
                    rwLock.ReleaseReaderLock();
                }

                return enumerator;
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    int result = 0;
                    rwLock.AcquireReaderLock(timeout);

                    try
                    {
                        result = collection.Capacity;
                    }
                    finally
                    {
                        rwLock.ReleaseReaderLock();
                    }

                    return result;
                }
				
                set
                {
                    rwLock.AcquireWriterLock(timeout);

                    try
                    {
                        collection.Capacity = value;
                    }
                    finally
                    {
                        rwLock.ReleaseWriterLock();
                    }
                }
            }

            public override int AddRange(IntegerCollection x)
            {
                int result = 0;
                rwLock.AcquireWriterLock(timeout);

                try
                {
                    result = collection.AddRange(x);
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }

                return result;
            }

            public override int AddRange(Int32[] x)
            {
                int result = 0;
                rwLock.AcquireWriterLock(timeout);

                try
                {
                    result = collection.AddRange(x);
                }
                finally
                {
                    rwLock.ReleaseWriterLock();
                }

                return result;
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class
        private class ReadOnlyIntegerCollection : IntegerCollection
        {
            #region Implementation (data)
            private IntegerCollection m_collection;
            #endregion

            #region Construction
            internal ReadOnlyIntegerCollection(IntegerCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }
            #endregion
			
            #region Type-safe ICollection
            public override void CopyTo(Int32[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(Int32[] array, int start)
            {
                m_collection.CopyTo(array,start);
            }
            public override int Count
            {
                get { return m_collection.Count; }
            }

            public override bool IsSynchronized
            {
                get { return m_collection.IsSynchronized; }
            }

            public override object SyncRoot
            {
                get { return this.m_collection.SyncRoot; }
            }
            #endregion
			
            #region Type-safe IList
            public override Int32 this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Int32 x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
			
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Int32 x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(Int32 x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Int32 x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Int32 x)
            {           
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
			
            public override bool IsFixedSize
            {
                get {return true;}
            }

            public override bool IsReadOnly
            {
                get {return true;}
            }
            #endregion

            #region Type-safe IEnumerable
            public override IIntegerEnumerator GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }
            #endregion

            #region Public Helpers
            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get { return m_collection.Capacity; }
				
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int AddRange(IntegerCollection x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Int32[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            #endregion
        }
        #endregion
    }
}
