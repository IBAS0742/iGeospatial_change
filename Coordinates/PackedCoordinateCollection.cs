using System;

namespace iGeospatial.Coordinates
{
	/// <summary>
	/// Summary description for PackedCoordinateCollection.
	/// </summary>
	[Serializable]
    public class PackedCoordinateCollection : ICoordinateList
	{
        #region Constructors and Destructor

		public PackedCoordinateCollection()
		{
        }

        #endregion

        #region ICoordinateList Members

        public bool IsFixedSize
        {
            get
            {
                // TODO:  Add PackedCoordinateCollection.IsFixedSize getter implementation
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                // TODO:  Add PackedCoordinateCollection.IsReadOnly getter implementation
                return false;
            }
        }

        public bool IsPacked
        {
            get
            {
                return true;
            }
        }

        public Coordinate this[int index]
        {
            get
            {
                // TODO:  Add PackedCoordinateCollection.this getter implementation
                return null;
            }

            set
            {
                // TODO:  Add PackedCoordinateCollection.this setter implementation
            }
        }

        public Coordinate this[int index, Coordinate inout]
        {
            get
            {
                // TODO:  Add PackedCoordinateCollection.iGeospatial.Coordinates.ICoordinateList.this getter implementation
                return null;
            }

            set
            {
                // TODO:  Add PackedCoordinateCollection.iGeospatial.Coordinates.ICoordinateList.this setter implementation
            }
        }

        public int Add(Coordinate value)
        {
            // TODO:  Add PackedCoordinateCollection.Add implementation
            return 0;
        }

        public void Clear()
        {
            // TODO:  Add PackedCoordinateCollection.Clear implementation
        }

        public bool Contains(Coordinate value)
        {
            // TODO:  Add PackedCoordinateCollection.Contains implementation
            return false;
        }

        public int IndexOf(Coordinate value)
        {
            // TODO:  Add PackedCoordinateCollection.IndexOf implementation
            return 0;
        }

        public void Insert(int index, Coordinate value)
        {
            // TODO:  Add PackedCoordinateCollection.Insert implementation
        }

        public void Remove(Coordinate value)
        {
            // TODO:  Add PackedCoordinateCollection.Remove implementation
        }

        public void RemoveAt(int index)
        {
            // TODO:  Add PackedCoordinateCollection.RemoveAt implementation
        }

        public Coordinate[] ToArray()
        {
            // TODO:  Add PackedCoordinateCollection.ToArray implementation
            return null;
        }

        public void Reverse()
        {
            // TODO:  Add PackedCoordinateCollection.Reverse implementation
        }

        public int AddRange(ICoordinateList x)
        {
            // TODO:  Add PackedCoordinateCollection.AddRange implementation
            return 0;
        }

        public int AddRange(Coordinate[] x)
        {
            // TODO:  Add PackedCoordinateCollection.iGeospatial.Coordinates.ICoordinateList.AddRange implementation
            return 0;
        }

        public void CopyTo(Coordinate[] array)
        {
            // TODO:  Add PackedCoordinateCollection.CopyTo implementation
        }

        public bool Add(ICoordinateList coord, bool allowRepeated, bool direction)
        {
            // TODO:  Add PackedCoordinateCollection.iGeospatial.Coordinates.ICoordinateList.Add implementation
            return false;
        }

        public bool Add(Coordinate[] coord, bool allowRepeated, bool direction)
        {
            // TODO:  Add PackedCoordinateCollection.iGeospatial.Coordinates.ICoordinateList.Add implementation
            return false;
        }

        public bool Add(ICoordinateList coord, bool allowRepeated)
        {
            // TODO:  Add PackedCoordinateCollection.iGeospatial.Coordinates.ICoordinateList.Add implementation
            return false;
        }

        public bool Add(Coordinate[] coord, bool allowRepeated)
        {
            // TODO:  Add PackedCoordinateCollection.iGeospatial.Coordinates.ICoordinateList.Add implementation
            return false;
        }

        public void Add(Coordinate coord, bool allowRepeated)
        {
            // TODO:  Add PackedCoordinateCollection.iGeospatial.Coordinates.ICoordinateList.Add implementation
        }

        public void MakePrecise(PrecisionModel precision)
        {
            // TODO:  Add PackedCoordinateCollection.MakePrecise implementation
        }

        public ICoordinateList Clone()
        {
            // TODO:  Add PackedCoordinateCollection.Clone implementation
            return null;
        }

        #endregion

        #region ICoordinateCollection Members

        public int Count
        {
            get
            {
                // TODO:  Add PackedCoordinateCollection.Count getter implementation
                return 0;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                // TODO:  Add PackedCoordinateCollection.IsSynchronized getter implementation
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                // TODO:  Add PackedCoordinateCollection.SyncRoot getter implementation
                return null;
            }
        }

        public void CopyTo(Coordinate[] array, int arrayIndex)
        {
            // TODO:  Add PackedCoordinateCollection.iGeospatial.Coordinates.ICoordinateCollection.CopyTo implementation
        }

        public ICoordinateEnumerator GetEnumerator()
        {
            // TODO:  Add PackedCoordinateCollection.GetEnumerator implementation
            return null;
        }

        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return null;
        }

        #endregion
    }

    [Serializable]
    internal sealed class DoubleList : ICloneable
    {
        #region Private Fields

        private const int DefaultCapacity = 16;

        private double[] m_array;
        private int m_count = 0;

        [NonSerialized]
        private int m_version = 0;
        
        #endregion

        #region Constructors and Destructor

        /// <summary>
        ///		Initializes a new instance of the <c>DoubleList</c> class
        ///		that is empty and has the default initial capacity.
        /// </summary>
        public DoubleList()
        {
            m_array = new double[DefaultCapacity];
        }
		
        /// <summary>
        ///		Initializes a new instance of the <c>DoubleList</c> class
        ///		that has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///		The number of elements that the new <c>DoubleList</c> is initially capable of storing.
        ///	</param>
        public DoubleList(int capacity)
        {
            m_array = new double[capacity];
        }

        /// <summary>
        ///		Initializes a new instance of the <c>DoubleList</c> class
        ///		that contains elements copied from the specified <c>DoubleList</c>.
        /// </summary>
        /// <param name="c">The <c>DoubleList</c> whose elements are copied to the new collection.</param>
        public DoubleList(DoubleList c)
        {
            m_array = new double[c.Count];
            AddRange(c);
        }

        /// <summary>
        ///		Initializes a new instance of the <c>DoubleList</c> class
        ///		that contains elements copied from the specified <see cref="double"/> array.
        /// </summary>
        /// <param name="a">The <see cref="double"/> array whose elements are copied to the new list.</param>
        public DoubleList(double[] a)
        {
            if (a != null && a.Length > 0)
            {
                m_array = a;
                m_count = a.Length;
            }
        }
		
        private enum Tag 
        {
            Default
        }

        private DoubleList(Tag t)
        {
            m_array = null;
        }

        #endregion
		
        #region Public Properties

        /// <summary>
        ///		Gets the number of elements actually contained in the <c>DoubleList</c>.
        /// </summary>
        public int Count
        {
            get { return m_count; }
        }

        /// <summary>
        ///		Gets or sets the <see cref="double"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="DoubleList.Count"/>.</para>
        /// </exception>
        public double this[int index]
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
        ///		Gets or sets the number of elements the <c>DoubleList</c> can contain.
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
                        double[] temp = new double[value];
                        Array.Copy(m_array, temp, m_count);
                        m_array = temp;
                    }
                    else
                    {
                        m_array = new double[DefaultCapacity];
                    }
                }
            }
        }

        #endregion
		
        #region Public Methods

        /// <summary>
        ///	Adds a <see cref="double"/> to the end of the <c>DoubleList</c>.
        /// </summary>
        /// <param name="item">The <see cref="double"/> to be added to the end of the <c>DoubleList</c>.</param>
        /// <returns>The index at which the value has been added.</returns>
        public int Add(double item)
        {
            if (m_count == m_array.Length)
                EnsureCapacity(m_count + 1);

            m_array[m_count] = item;
            m_version++;

            return m_count++;
        }
		
        /// <summary>
        /// Removes all elements from the <c>DoubleList</c>.
        /// </summary>
        public void Clear()
        {
            ++m_version;
            m_array = new double[DefaultCapacity];
            m_count = 0;
        }

        /// <summary>
        ///	Copies the entire <c>DoubleList</c> to a one-dimensional
        ///	<see cref="double"/> array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="double"/> array to copy to.</param>
        public void CopyTo(double[] array)
        {
            this.CopyTo(array, 0);
        }

        /// <summary>
        ///	Copies the entire <c>DoubleList</c> to a one-dimensional
        ///	<see cref="double"/> array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="double"/> array to copy to.</param>
        /// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo(double[] array, int start)
        {
            if (m_count > array.GetUpperBound(0) + 1 - start)
                throw new System.ArgumentException("Destination array was not long enough.");
			
            Array.Copy(m_array, 0, array, start, m_count); 
        }

        /// <summary>
        ///		Determines whether a given <see cref="double"/> is in the <c>DoubleList</c>.
        /// </summary>
        /// <param name="item">The <see cref="double"/> to check for.</param>
        /// <returns><c>true</c> if <paramref name="item"/> is found in the <c>DoubleList</c>; otherwise, <c>false</c>.</returns>
        public bool Contains(double item)
        {
            for (int i=0; i != m_count; ++i)
            {
                if (m_array[i].Equals(item))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///	Returns the zero-based index of the first occurrence of a <see cref="double"/>
        ///	in the <c>DoubleList</c>.
        /// </summary>
        /// <param name="item">The <see cref="double"/> to locate in the <c>DoubleList</c>.</param>
        /// <returns>
        ///		The zero-based index of the first occurrence of <paramref name="item"/> 
        ///		in the entire <c>DoubleList</c>, if found; otherwise, -1.
        ///	</returns>
        public int IndexOf(double item)
        {
            for (int i=0; i != m_count; ++i)
            {
                if (m_array[i].Equals(item))
                    return i;
            }

            return -1;
        }

        /// <summary>
        ///		Inserts an element into the <c>DoubleList</c> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="double"/> to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="DoubleList.Count"/>.</para>
        /// </exception>
        public void Insert(int index, double item)
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
        ///		Removes the first occurrence of a specific <see cref="double"/> from the <c>DoubleList</c>.
        /// </summary>
        /// <param name="item">The <see cref="double"/> to remove from the <c>DoubleList</c>.</param>
        /// <exception cref="ArgumentException">
        ///		The specified <see cref="double"/> was not found in the <c>DoubleList</c>.
        /// </exception>
        public void Remove(double item)
        {		   
            int i = IndexOf(item);
            if (i < 0)
                throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
            ++m_version;
            RemoveAt(i);
        }

        /// <summary>
        ///		Removes the element at the specified index of the <c>DoubleList</c>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="DoubleList.Count"/>.</para>
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
            double[] temp = new double[1];
            Array.Copy(temp, 0, m_array, m_count, 1);
            m_version++;
        }

        /// <summary>
        ///		Adds the elements of another <c>DoubleList</c> to the current <c>DoubleList</c>.
        /// </summary>
        /// <param name="x">The <c>DoubleList</c> whose elements should be added to the end of the current <c>DoubleList</c>.</param>
        /// <returns>The new <see cref="DoubleList.Count"/> of the <c>DoubleList</c>.</returns>
        public int AddRange(DoubleList x)
        {
            if (m_count + x.Count >= m_array.Length)
                EnsureCapacity(m_count + x.Count);
			
            Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
            m_count += x.Count;
            m_version++;

            return m_count;
        }

        /// <summary>
        ///	Adds the elements of a <see cref="double"/> array to the current <c>DoubleList</c>.
        /// </summary>
        /// <param name="x">The <see cref="double"/> array whose elements should be added to the end of the <c>DoubleList</c>.</param>
        /// <returns>The new <see cref="DoubleList.Count"/> of the <c>DoubleList</c>.</returns>
        public int AddRange(double[] x)
        {
            if (m_count + x.Length >= m_array.Length)
                EnsureCapacity(m_count + x.Length);

            Array.Copy(x, 0, m_array, m_count, x.Length);
            m_count += x.Length;
            m_version++;

            return m_count;
        }
		
        /// <summary>
        ///	Sets the capacity to the actual number of elements.
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
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="DoubleList.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int i)
        {
            ValidateIndex(i, false);
        }

        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="DoubleList.Count"/>.</para>
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

        #region ICloneable Members
		
        /// <summary>
        ///	Creates a deep copy of the <see cref="DoubleList"/>.
        /// </summary>
        public DoubleList Clone()
        {
            DoubleList newColl = new DoubleList(m_count);

            Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
            newColl.m_count = m_count;
            newColl.m_version = m_version;

            return newColl;
        }

        /// <summary>
        ///	Creates a deep copy of the <see cref="DoubleList"/>.
        /// </summary>
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }
}
