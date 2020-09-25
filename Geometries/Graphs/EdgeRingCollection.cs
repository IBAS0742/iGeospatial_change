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

namespace iGeospatial.Geometries.Graphs
{
    /// <summary>
    ///		A strongly-typed collection of <see cref="EdgeRing"/> objects.
    /// </summary>
    [Serializable]
    internal sealed class EdgeRingCollection : IList, ICloneable
    {
        #region Private Fields
        
        private const int DefaultCapacity = 16;

        private EdgeRing[] m_array;
        private int        m_count;

        [NonSerialized]
        private int m_version;

        #endregion
	
        #region Constructors and Destructor

        /// <summary>
        ///		Initializes a new instance of the <c>EdgeRingCollection</c> class
        ///		that is empty and has the default initial capacity.
        /// </summary>
        public EdgeRingCollection()
        {
            m_array = new EdgeRing[DefaultCapacity];
        }
		
        /// <summary>
        ///		Initializes a new instance of the <c>EdgeRingCollection</c> class
        ///		that has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///		The number of elements that the new <c>EdgeRingCollection</c> is initially capable of storing.
        ///	</param>
        public EdgeRingCollection(int capacity)
        {
            m_array = new EdgeRing[capacity];
        }

        /// <summary>
        ///		Initializes a new instance of the <c>EdgeRingCollection</c> class
        ///		that contains elements copied from the specified <c>EdgeRingCollection</c>.
        /// </summary>
        /// <param name="edgeRings">The <c>EdgeRingCollection</c> whose elements are copied to the new collection.</param>
        public EdgeRingCollection(EdgeRingCollection edgeRings)
        {
            if (edgeRings == null)
            {
                throw new ArgumentNullException("edgeRings");
            }

            m_array = new EdgeRing[edgeRings.Count];
            AddRange(edgeRings);
        }

        /// <summary>
        ///		Initializes a new instance of the <c>EdgeRingCollection</c> class
        ///		that contains elements copied from the specified <see cref="EdgeRing"/> array.
        /// </summary>
        /// <param name="edgeRings">The <see cref="EdgeRing"/> array whose elements are copied to the new list.</param>
        public EdgeRingCollection(EdgeRing[] edgeRings)
        {
            if (edgeRings == null)
            {
                throw new ArgumentNullException("edgeRings");
            }

            m_array = new EdgeRing[edgeRings.Length];
            AddRange(edgeRings);
        }
		
        #endregion
		
        #region Public Properties

        /// <summary>
        ///		Gets the number of elements actually contained in the <c>EdgeRingCollection</c>.
        /// </summary>
        public int Count
        {
            get { return m_count; }
        }

        /// <summary>
        ///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
        /// </summary>
        /// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public bool IsSynchronized
        {
            get { return m_array.IsSynchronized; }
        }

        /// <summary>
        ///		Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        public object SyncRoot
        {
            get { return m_array.SyncRoot; }
        }

        /// <summary>
        ///		Gets or sets the <see cref="EdgeRing"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="EdgeRingCollection.Count"/>.</para>
        /// </exception>
        public EdgeRing this[int index]
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
        public bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        ///		gets a value indicating whether the <B>IList</B> is read-only.
        /// </summary>
        /// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public bool IsReadOnly
        {
            get { return false; }
        }
		
        /// <summary>
        ///		Gets or sets the number of elements the <c>EdgeRingCollection</c> can contain.
        /// </summary>
        public int Capacity
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
                        EdgeRing[] temp = new EdgeRing[value];
                        Array.Copy(m_array, temp, m_count);
                        m_array = temp;
                    }
                    else
                    {
                        m_array = new EdgeRing[DefaultCapacity];
                    }
                }
            }
        }

        #endregion
		
        #region Public Methods

        /// <summary>
        ///		Adds a <see cref="EdgeRing"/> to the end of the <c>EdgeRingCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="EdgeRing"/> to be added to the end of the <c>EdgeRingCollection</c>.</param>
        /// <returns>The index at which the value has been added.</returns>
        public int Add(EdgeRing item)
        {
            if (m_count == m_array.Length)
                EnsureCapacity(m_count + 1);

            m_array[m_count] = item;
            m_version++;

            return m_count++;
        }
		
        /// <summary>
        ///		Removes all elements from the <c>EdgeRingCollection</c>.
        /// </summary>
        public void Clear()
        {
            ++m_version;
            m_array = new EdgeRing[DefaultCapacity];
            m_count = 0;
        }

        /// <summary>
        ///		Determines whether a given <see cref="EdgeRing"/> is in the <c>EdgeRingCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="EdgeRing"/> to check for.</param>
        /// <returns><see langword="true"/> if <paramref name="item"/> is found in the <c>EdgeRingCollection</c>; otherwise, <see langword="false"/>.</returns>
        public bool Contains(EdgeRing item)
        {
            for (int i=0; i != m_count; ++i)
                if (m_array[i].Equals(item))
                    return true;
            return false;
        }

        /// <summary>
        ///		Returns the zero-based index of the first occurrence of a <see cref="EdgeRing"/>
        ///		in the <c>EdgeRingCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="EdgeRing"/> to locate in the <c>EdgeRingCollection</c>.</param>
        /// <returns>
        ///		The zero-based index of the first occurrence of <paramref name="item"/> 
        ///		in the entire <c>EdgeRingCollection</c>, if found; otherwise, -1.
        ///	</returns>
        public int IndexOf(EdgeRing item)
        {
            for (int i=0; i != m_count; ++i)
                if (m_array[i].Equals(item))
                    return i;
            return -1;
        }

        /// <summary>
        ///		Inserts an element into the <c>EdgeRingCollection</c> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="EdgeRing"/> to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="EdgeRingCollection.Count"/>.</para>
        /// </exception>
        public void Insert(int index, EdgeRing item)
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
        ///		Removes the first occurrence of a specific <see cref="EdgeRing"/> from the <c>EdgeRingCollection</c>.
        /// </summary>
        /// <param name="item">The <see cref="EdgeRing"/> to remove from the <c>EdgeRingCollection</c>.</param>
        /// <exception cref="ArgumentException">
        ///		The specified <see cref="EdgeRing"/> was not found in the <c>EdgeRingCollection</c>.
        /// </exception>
        public void Remove(EdgeRing item)
        {		   
            int i = IndexOf(item);
            if (i < 0)
                throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
            ++m_version;
            RemoveAt(i);
        }

        /// <summary>
        ///		Removes the element at the specified index of the <c>EdgeRingCollection</c>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="EdgeRingCollection.Count"/>.</para>
        /// </exception>
        public void RemoveAt(int index)
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
            EdgeRing[] temp = new EdgeRing[1];
            Array.Copy(temp, 0, m_array, m_count, 1);
            m_version++;
        }

        /// <summary>
        ///	Copies the entire <c>EdgeRingCollection</c> to a one-dimensional
        ///	<see cref="EdgeRing"/> array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="EdgeRing"/> array to copy to.</param>
        public void CopyTo(EdgeRing[] array)
        {
            this.CopyTo(array, 0);
        }

        /// <summary>
        ///	Copies the entire <c>EdgeRingCollection</c> to a one-dimensional
        ///	<see cref="EdgeRing"/> array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="EdgeRing"/> array to copy to.</param>
        /// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(EdgeRing[] array, int start)
        {
            if (m_count > array.GetUpperBound(0) + 1 - start)
                throw new System.ArgumentException("Destination array was not long enough.");
			
            Array.Copy(m_array, 0, array, start, m_count); 
        }

        /// <summary>
        ///		Adds the elements of another <c>EdgeRingCollection</c> to the current <c>EdgeRingCollection</c>.
        /// </summary>
        /// <param name="x">The <c>EdgeRingCollection</c> whose elements should be added to the end of the current <c>EdgeRingCollection</c>.</param>
        /// <returns>The new <see cref="EdgeRingCollection.Count"/> of the <c>EdgeRingCollection</c>.</returns>
        public int AddRange(EdgeRingCollection x)
        {
            if (m_count + x.Count >= m_array.Length)
                EnsureCapacity(m_count + x.Count);
			
            Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
            m_count += x.Count;
            m_version++;

            return m_count;
        }

        /// <summary>
        ///		Adds the elements of a <see cref="EdgeRing"/> array to the current <c>EdgeRingCollection</c>.
        /// </summary>
        /// <param name="x">The <see cref="EdgeRing"/> array whose elements should be added to the end of the <c>EdgeRingCollection</c>.</param>
        /// <returns>The new <see cref="EdgeRingCollection.Count"/> of the <c>EdgeRingCollection</c>.</returns>
        public int AddRange(EdgeRing[] x)
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
        public void TrimToSize()
        {
            this.Capacity = m_count;
        }

        #endregion

        #region Private Methods

        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="EdgeRingCollection.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int i)
        {
            ValidateIndex(i, false);
        }

        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="EdgeRingCollection.Count"/>.</para>
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
            this.CopyTo((EdgeRing[])array, start);
        }

        #endregion

        #region IList Members

        object IList.this[int i]
        {
            get { return (object)this[i]; }
            set { this[i] = (EdgeRing)value; }
        }

        int IList.Add(object x)
        {
            return this.Add((EdgeRing)x);
        }

        bool IList.Contains(object x)
        {
            return this.Contains((EdgeRing)x);
        }

        int IList.IndexOf(object x)
        {
            return this.IndexOf((EdgeRing)x);
        }

        void IList.Insert(int pos, object x)
        {
            this.Insert(pos, (EdgeRing)x);
        }

        void IList.Remove(object x)
        {
            this.Remove((EdgeRing)x);
        }

        void IList.RemoveAt(int pos)
        {
            this.RemoveAt(pos);
        }

        #endregion

        #region IEnumerable Members
		
        /// <summary>
        ///		Returns an enumerator that can iterate through the <c>EdgeRingCollection</c>.
        /// </summary>
        /// <returns>An <see cref="EdgeRingEnumerator"/> for the entire <c>EdgeRingCollection</c>.</returns>
        public IEdgeRingEnumerator GetEnumerator()
        {
            return new EdgeRingEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)(this.GetEnumerator());
        }

        #endregion
		
        #region ICloneable Members

        /// <summary>
        ///		Creates a shallow copy of the <see cref="EdgeRingCollection"/>.
        /// </summary>
        public object Clone()
        {
            EdgeRingCollection newColl = new EdgeRingCollection(m_count);
            Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
            newColl.m_count = m_count;
            newColl.m_version = m_version;

            return newColl;
        }
        
        #endregion

        #region Nested enumerator class

        /// <summary>
        ///		Supports simple iteration over a <see cref="EdgeRingCollection"/>.
        /// </summary>
        private sealed class EdgeRingEnumerator : IEnumerator, IEdgeRingEnumerator
        {
            #region Implementation (data)
			
            private EdgeRingCollection m_collection;
            private int m_index;
            private int m_version;
			
            #endregion
		
            #region Construction
			
            /// <summary>
            ///		Initializes a new instance of the <c>EdgeRingEnumerator</c> class.
            /// </summary>
            /// <param name="tc"></param>
            internal EdgeRingEnumerator(EdgeRingCollection tc)
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
            public EdgeRing Current
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
            ///		<see langword="true"/> if the enumerator was successfully advanced to the next element; 
            ///		<see langword="false"/> if the enumerator has passed the end of the collection.
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
    }
}
