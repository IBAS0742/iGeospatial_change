using System;
using System.Text;
using System.Collections;

namespace iGeospatial.Coordinates
{
    /// <summary>
    ///	A strongly-typed collection of <see cref="Coordinate"/> objects.
    /// </summary>
    [Serializable]
    public class CoordinateCollection : ICoordinateList, IList
    {
        #region Private Fields
        
        private const int DEFAULT_CAPACITY = 16;

        private Coordinate[] m_array;

        private int m_count = 0;

        [NonSerialized]
        private int m_version = 0;
                
        #endregion
	
        #region Static Wrappers

        /// <summary>
        ///	Creates a synchronized (thread-safe) wrapper for a 
        ///	<see cref="CoordinateCollection"/> instance.
        /// </summary>
        /// <returns>
        /// An <see cref="CoordinateCollection"/> wrapper that is synchronized 
        /// (thread-safe).
        /// </returns>
        public static CoordinateCollection Synchronized(CoordinateCollection list)
        {
            if(list==null)
                throw new ArgumentNullException("list");

            return new SyncCoordinateList(list);
        }
		
        /// <summary>
        /// Creates a read-only wrapper for a <see cref="CoordinateCollection"/> 
        /// instance.
        /// </summary>
        /// <returns>
        /// An <see cref="CoordinateCollection"/> wrapper that is read-only.
        /// </returns>
        public static CoordinateCollection ReadOnly(CoordinateCollection list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            return new ReadOnlyCoordinateList(list);
        }

        #endregion

        #region Constructors and Destructor

        /// <summary>
        ///	Initializes a new instance of the <see cref="CoordinateCollection"/> 
        ///	class that is empty and has the default initial capacity.
        /// </summary>
        public CoordinateCollection()
        {
            m_array = new Coordinate[DEFAULT_CAPACITY];
        }
		
        /// <summary>
        ///	Initializes a new instance of the <see cref="CoordinateCollection"/> 
        ///	class	that has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///	The number of elements that the new <see cref="CoordinateCollection"/> is initially capable of storing.
        ///	</param>
        public CoordinateCollection(int capacity)
        {
            m_array = new Coordinate[capacity];
        }

        /// <summary>
        ///	Initializes a new instance of the <see cref="CoordinateCollection"/> class
        ///	that contains elements copied from the specified <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <param name="c">
        /// The <see cref="CoordinateCollection"/> whose elements are copied 
        /// to the new collection.
        /// </param>
        public CoordinateCollection(CoordinateCollection c)
        {
            m_array = new Coordinate[c.Count];
            AddRange(c);
        }

        /// <summary>
        ///	Initializes a new instance of the <see cref="CoordinateCollection"/> 
        ///	class that contains elements copied from the specified 
        ///	<see cref="Coordinate"/> array.
        /// </summary>
        /// <param name="a">
        /// The <see cref="Coordinate"/> array whose elements are copied to the 
        /// new list.
        /// </param>
        public CoordinateCollection(Coordinate[] coords)
        {
            if (HasNullElements(coords))
            {
                throw new System.ArgumentException("Null coordinate");
            }

            m_array = new Coordinate[coords.Length];
            AddRange(coords);
        }
		
		/// <summary> 
		/// Constructs a new list from an array of Coordinates, allowing caller 
		/// to specify if repeated points are to be removed.
		/// </summary>
		/// <param name="coords">
		/// The array of coordinates to load into the list.
		/// </param>
		/// <param name="allowRepeated">
		/// If false, repeated points are removed.
		/// </param>
		public CoordinateCollection(Coordinate[] coords, bool allowRepeated)
		{
            if (HasNullElements(coords))
            {
                throw new ArgumentException("Null coordinate");
            }

            m_array = new Coordinate[coords.Length];

            Add(coords, allowRepeated);
		}
		
		/// <summary> 
		/// Constructs a new list from an array of Coordinates, allowing caller 
		/// to specify if repeated points are to be removed.
		/// </summary>
		/// <param name="coords">
		/// The array of coordinates to load into the list.
		/// </param>
		/// <param name="allowRepeated">
		/// If false, repeated points are removed.
		/// </param>
		public CoordinateCollection(ICoordinateList coords, bool allowRepeated)
		{
            if (HasNullElements(coords))
            {
                throw new System.ArgumentException("Null coordinate");
            }

            m_array = new Coordinate[coords.Count];

            Add(coords, allowRepeated);
		}
		
        private enum Tag 
        {
            Default
        }

        private CoordinateCollection(Tag t)
        {
            m_array = null;
        }

        #endregion
		
        #region Public Properties

        /// <summary>
        ///	Gets the number of elements actually contained in the <see cref="CoordinateCollection"/>.
        /// </summary>
        public virtual int Count
        {
            get 
            { 
                return m_count; 
            }
        }

        /// <summary>
        ///	Gets or sets the <see cref="Coordinate"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///	<para><paramref name="index"/> is less than zero</para>
        ///	<para>-or-</para>
        ///	<para><paramref name="index"/> is equal to or greater than <see cref="CoordinateCollection.Count"/>.</para>
        /// </exception>
        public virtual Coordinate this[int index]
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
        ///	Gets or sets the <see cref="Coordinate"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///	<para><paramref name="index"/> is less than zero</para>
        ///	<para>-or-</para>
        ///	<para><paramref name="index"/> is equal to or greater than <see cref="CoordinateCollection.Count"/>.</para>
        /// </exception>
        public virtual Coordinate this[int index, Coordinate inout]
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
        ///	Gets a value indicating whether access to the collection is synchronized (thread-safe).
        /// </summary>
        /// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized
        {
            get 
            { 
                return false; 
            }
        }

        /// <summary>
        ///	Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        public virtual object SyncRoot
        {
            get 
            { 
                return this; 
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
        /// Gets a value indicating whether this <see cref="ICoordinateList"/> is
        /// packed.
        /// </summary>
        /// <value>
        /// This property returns <see cref="true"/> if the ordinates in the list
        /// are packed, otherwise, returns <see cref="false"/>.
        /// </value>
        /// <remarks>
        /// A packed coordinate list is optimized for memory and does not contain
        /// directly the instances of the <see cref="Coordinate"/> as a list, but
        /// a single array of numbers representing all the coordinates.
        /// </remarks>
        public virtual bool IsPacked
        {
            get
            {
                return false;
            }
        }
		
        /// <summary>
        ///		Gets or sets the number of elements the <see cref="CoordinateCollection"/> can contain.
        /// </summary>
        public virtual int Capacity
        {
            get 
            { 
                return m_array.Length; 
            }
			
            set
            {
                if (value < m_count)
                    value = m_count;

                if (value != m_array.Length)
                {
                    if (value > 0)
                    {
                        Coordinate[] temp = new Coordinate[value];
                        Array.Copy(m_array, temp, m_count);
                        m_array = temp;
                    }
                    else
                    {
                        m_array = new Coordinate[DEFAULT_CAPACITY];
                    }
                }
            }
        }

        #endregion
		
        #region Public Methods

        /// <summary>
        ///		Adds a <see cref="Coordinate"/> to the end of the <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="Coordinate"/> to be added to the end of the <see cref="CoordinateCollection"/>.</param>
        /// <returns>The index at which the value has been added.</returns>
        public virtual int Add(Coordinate item)
        {
            if (m_count == m_array.Length)
                EnsureCapacity(m_count + 1);

            m_array[m_count] = item;

            m_version++;

            return m_count++;
        }
		
        /// <summary>
        ///		Removes all elements from the <see cref="CoordinateCollection"/>.
        /// </summary>
        public virtual void Clear()
        {
            ++m_version;
            m_array = new Coordinate[DEFAULT_CAPACITY];
            m_count = 0;
        }

        public virtual CoordinateCollection Copy()
        {
            CoordinateCollection newColl = new CoordinateCollection(m_count);
            Array.Copy(m_array, 0, newColl.m_array, 0, m_count);
            newColl.m_count   = m_count;
            newColl.m_version = m_version;

            return newColl;
        }

        /// <summary>
        ///		Determines whether a given <see cref="Coordinate"/> is in the <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="Coordinate"/> to check for.</param>
        /// <returns><see cref="true"/> if <paramref name="item"/> is found in the <see cref="CoordinateCollection"/>; otherwise, <see cref="false"/>.</returns>
        public virtual bool Contains(Coordinate item)
        {
            for (int i=0; i != m_count; ++i)
                if (m_array[i].Equals(item))
                    return true;
            return false;
        }

        /// <summary>
        ///		Returns the zero-based index of the first occurrence of a <see cref="Coordinate"/>
        ///		in the <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="Coordinate"/> to locate in the <see cref="CoordinateCollection"/>.</param>
        /// <returns>
        ///		The zero-based index of the first occurrence of <paramref name="item"/> 
        ///		in the entire <see cref="CoordinateCollection"/>, if found; otherwise, -1.
        ///	</returns>
        public virtual int IndexOf(Coordinate item)
        {
            for (int i=0; i != m_count; ++i)
                if (m_array[i].Equals(item))
                    return i;
            return -1;
        }

        /// <summary>
        ///		Inserts an element into the <see cref="CoordinateCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="Coordinate"/> to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="CoordinateCollection.Count"/>.</para>
        /// </exception>
        public virtual void Insert(int index, Coordinate item)
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
        ///		Removes the first occurrence of a specific <see cref="Coordinate"/> from the <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="Coordinate"/> to remove from the <see cref="CoordinateCollection"/>.</param>
        /// <exception cref="ArgumentException">
        ///		The specified <see cref="Coordinate"/> was not found in the <see cref="CoordinateCollection"/>.
        /// </exception>
        public virtual void Remove(Coordinate item)
        {		   
            int i = IndexOf(item);
            if (i < 0)
                throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
            ++m_version;
            RemoveAt(i);
        }

        /// <summary>
        ///		Removes the element at the specified index of the <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="CoordinateCollection.Count"/>.</para>
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
            Coordinate[] temp = new Coordinate[1];
            Array.Copy(temp, 0, m_array, m_count, 1);
            m_version++;
        }

        /// <summary>
        ///	Copies the entire <see cref="CoordinateCollection"/> to a one-dimensional
        ///	<see cref="Coordinate"/> array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Coordinate"/> array to copy to.</param>
        public virtual void CopyTo(Coordinate[] array)
        {
            this.CopyTo(array, 0);
        }

        /// <summary>
        ///	Copies the entire <see cref="CoordinateCollection"/> to a one-dimensional
        ///	<see cref="Coordinate"/> array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Coordinate"/> array to copy to.</param>
        /// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public virtual void CopyTo(Coordinate[] array, int start)
        {
            if (m_count > array.GetUpperBound(0) + 1 - start)
                throw new System.ArgumentException("Destination array was not long enough.");
			
            Array.Copy(m_array, 0, array, start, m_count); 
        }

		public virtual bool Add(Coordinate[] coord, bool allowRepeated, bool direction)
		{
			if (direction)
			{
                int nCount = coord.Length;
				for (int i = 0; i < nCount; i++)
				{
					Add(coord[i], allowRepeated);
				}
			}
			else
			{
                int nCount = coord.Length;
				for (int i = nCount - 1; i >= 0; i--)
				{
					Add(coord[i], allowRepeated);
				}
			}
			return true;
		}
		
		public virtual bool Add(Coordinate[] coord, bool allowRepeated)
		{
			Add(coord, allowRepeated, true);
			return true;
		}
		
		public virtual bool Add(object obj, bool allowRepeated)
		{
			Add((Coordinate) obj, allowRepeated);
			return true;
		}

		public virtual void  Add(Coordinate coord, bool allowRepeated)
		{
			// don't Add duplicate coordinates
			if (!allowRepeated)
			{
				if (Count >= 1)
				{
					Coordinate last = this[Count - 1];
					if (last.Equals(coord))
						return ;
				}
			}

			this.Add(coord);
		}
		
		public virtual bool AddAll(ArrayList coll, bool allowRepeated)
		{
			bool isChanged = false;

            for (IEnumerator i = coll.GetEnumerator(); i.MoveNext(); )
			{
				Add((Coordinate) i.Current, allowRepeated);
				isChanged = true;
			}
			return isChanged;
		}
		
		/// <summary> 
		/// Ensure this coordList is a ring, by adding the start point if necessary
		/// </summary>
		public virtual void CloseRing()
		{
			if (Count > 0)
				Add(this[0], false);
		}
		
		public virtual Coordinate[] ToArray()
		{
            Coordinate[] array = new Coordinate[this.m_count];
            Array.Copy(this.m_array, array, this.m_count);

            return array;
        }

        /// <summary>  
        /// Returns true if the array Contains any null elements.
        /// </summary>
        /// <param name="array">An array to validate.</param>
        /// <returns>true if any of arrays elements are null.</returns>
        protected internal static bool HasNullElements(Coordinate[] array)
        {
            int nCount = array.Length;

            for (int i = 0; i < nCount; i++)
            {
                if (array[i] == null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>  
        /// Returns true if the array Contains any null elements.
        /// </summary>
        /// <param name="array">An array to validate.</param>
        /// <returns>true if any of arrays elements are null.</returns>
        protected internal static bool HasNullElements(ICoordinateList array)
        {
            int nCount = array.Count;

            for (int i = 0; i < nCount; i++)
            {
                if (array[i] == null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Shifts the positions until firstCoordinate is first.
        /// </summary>
        /// <param name="firstCoordinate">The Coordinate to make first</param>
        public void Scroll(Coordinate firstCoordinate) 
        {
            if (firstCoordinate == null)
            {
                throw new ArgumentNullException( "firstCoordinate" );
            }

            int index = this.IndexOf( firstCoordinate );
            if (index > -1)
            {
                Coordinate[] newCoordinates = new Coordinate[this.Count ];
                this.CopyTo( index, newCoordinates, 0, this.Count - index );	// copies from index to end
                this.CopyTo( 0, newCoordinates, this.Count - index, index );		// copies from 0 to index
                this.Clear();  // now clear array to refill with scrolled array.
                this.AddRange( newCoordinates ); // add newCoordinates to coordinates array
            }
            else
            {
                throw new ArgumentException("firstCoordinate not found in ArrayList", "firstCoordinate");
            }

        }

        public bool Add(ICoordinateList coord, bool allowRepeated, bool direction)
        {
            if (direction) 
            {
                int nCount = coord.Count;
                for (int i = 0; i < nCount; i++) 
                {
                    Add(coord[i], allowRepeated);
                }
            }
            else 
            {
                int nCount = coord.Count;
                for (int i = nCount - 1; i >= 0; i--) 
                {
                    Add(coord[i], allowRepeated);
                }
            }

            return true;
        }

        public bool Add(ICoordinateList coord, bool allowRepeated)
        {
            Add(coord, allowRepeated, true);
            
            return true;
        }

        public void MakePrecise(PrecisionModel precision)
        {
            int nCount = this.Count;
            for (int i = 0; i < nCount; i++)
            {
                this[i].MakePrecise(precision);
            }
        }

        /// <summary>
        ///		Adds the elements of another <see cref="CoordinateCollection"/> to the current <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <param name="x">The <see cref="CoordinateCollection"/> whose elements should be added to the end of the current <see cref="CoordinateCollection"/>.</param>
        /// <returns>The new <see cref="CoordinateCollection.Count"/> of the <see cref="CoordinateCollection"/>.</returns>
        public virtual int AddRange(ICoordinateList x)
        {
//            if (m_count + x.Count >= m_array.Length)
//                EnsureCapacity(m_count + x.Count);
//			
//            Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
//            m_count += x.Count;
//            m_version++;
//
//            return m_count;
            return AddRange(x.ToArray());
        }

        /// <summary>
        ///		Adds the elements of a <see cref="Coordinate"/> array to the current <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <param name="x">The <see cref="Coordinate"/> array whose elements should be added to the end of the <see cref="CoordinateCollection"/>.</param>
        /// <returns>The new <see cref="CoordinateCollection.Count"/> of the <see cref="CoordinateCollection"/>.</returns>
        public virtual int AddRange(Coordinate[] x)
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
        /// Searches the entire sorted <see cref="CoordinateCollection"/> for an
        /// <see cref="Coordinate"/> element using the default comparer
        /// and returns the zero-based index of the element.
        /// </summary>
        /// <param name="value">The <see cref="Coordinate"/> object
        /// to locate in the <see cref="CoordinateCollection"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>The zero-based index of <paramref name="value"/> in the sorted
        /// <see cref="CoordinateCollection"/>, if <paramref name="value"/> is found;
        /// otherwise, a negative number, which is the bitwise complement of the index
        /// of the next element that is larger than <paramref name="value"/> or, if there
        /// is no larger element, the bitwise complement of <see cref="Count"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Neither <paramref name="value"/> nor the elements of the <see cref="CoordinateCollection"/>
        /// implement the <see cref="IComparable"/> interface.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.BinarySearch"/> for details.</remarks>
        public virtual int BinarySearch(Coordinate value) 
        {
            if (this.m_count == 0) 
                return ~0;

            int index, left = 0, right = this.m_count - 1;

            if ((object) value == null) 
            {
                do 
                {
                    index = (left + right) / 2;
                    if ((object) this.m_array[index] == null)
                        return index;
                    right = index - 1;
                } while (left <= right);

                return ~left;
            }

            do 
            {
                index = (left + right) / 2;
                int result = value.CompareTo(this.m_array[index]);

                if (result == 0)
                    return index;
                else if (result < 0)
                    right = index - 1;
                else
                    left = index + 1;
            } while (left <= right);

            return ~left;
        }

        /// <summary>
        /// Removes the specified range of elements from the <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range
        /// of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote a
        /// valid range of elements in the <see cref="CoordinateCollection"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="count"/> is less than zero.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="CoordinateCollection"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <b>CoordinateCollection</b> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.RemoveRange"/> for details.</remarks>
        public virtual void RemoveRange(int index, int count) 
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index",
                    index, "Argument cannot be negative.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count",
                    count, "Argument cannot be negative.");

            if (index + count > this.m_count)
                throw new ArgumentException(
                    "Arguments denote invalid range of elements.");

            if (count == 0) return;

            ++this.m_version;
            this.m_count -= count;

            if (index < this.m_count)
                Array.Copy(this.m_array, index + count,
                    this.m_array, index, this.m_count - index);

            Array.Clear(this.m_array, this.m_count, count);
        }

        /// <overloads>
        /// Reverses the order of the elements in the 
        /// <see cref="CoordinateCollection"/> or a portion of it.
        /// </overloads>
        /// <summary>
        /// Reverses the order of the elements in the entire <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// The <see cref="CoordinateCollection"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Reverse"/> for details.</remarks>
        public virtual void Reverse() 
        {
            if (this.m_count <= 1) return;
            ++this.m_version;
            Array.Reverse(this.m_array, 0, this.m_count);
        }

        /// <summary>
        /// Reverses the order of the elements in the specified range.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range
        /// of elements to reverse.</param>
        /// <param name="count">The number of elements to reverse.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote a
        /// valid range of elements in the <see cref="CoordinateCollection"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="count"/> is less than zero.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="CoordinateCollection"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Reverse"/> for details.</remarks>
        public virtual void Reverse(int index, int count) 
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index",
                    index, "Argument cannot be negative.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count",
                    count, "Argument cannot be negative.");

            if (index + count > this.m_count)
                throw new ArgumentException(
                    "Arguments denote invalid range of elements.");

            if (count <= 1 || this.m_count <= 1) return;
            ++this.m_version;
            Array.Reverse(this.m_array, index, count);
        }

        /// <overloads>
        /// Sorts the elements in the <see cref="CoordinateCollection"/> or a portion of it.
        /// </overloads>
        /// <summary>
        /// Sorts the elements in the entire <see cref="CoordinateCollection"/>
        /// using the <see cref="IComparable"/> implementation of each element.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// The <see cref="CoordinateCollection"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Sort"/> for details.</remarks>
        public virtual void Sort() 
        {
            if (this.m_count <= 1) return;
            ++this.m_version;
            Array.Sort(this.m_array, 0, this.m_count);
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="CoordinateCollection"/>
        /// using the specified <see cref="IComparer"/> interface.
        /// </summary>
        /// <param name="comparer">
        /// <para>The <see cref="IComparer"/> implementation to use when comparing elements.</para>
        /// <para>-or-</para>
        /// <para>A null reference to use the <see cref="IComparable"/> implementation 
        /// of each element.</para></param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="CoordinateCollection"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Sort"/> for details.</remarks>
        public virtual void Sort(IComparer comparer) 
        {
            if (this.m_count <= 1) return;
            ++this.m_version;
            Array.Sort(this.m_array, 0, this.m_count, comparer);
        }

        /// <summary>
        /// Sorts the elements in the specified range 
        /// using the specified <see cref="IComparer"/> interface.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range
        /// of elements to sort.</param>
        /// <param name="count">The number of elements to sort.</param>
        /// <param name="comparer">
        /// <para>The <see cref="IComparer"/> implementation to use when comparing elements.</para>
        /// <para>-or-</para>
        /// <para>A null reference to use the <see cref="IComparable"/> implementation 
        /// of each element.</para></param>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote a
        /// valid range of elements in the <see cref="CoordinateCollection"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="count"/> is less than zero.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="CoordinateCollection"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Sort"/> for details.</remarks>
        public virtual void Sort(int index, int count, IComparer comparer) 
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index",
                    index, "Argument cannot be negative.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count",
                    count, "Argument cannot be negative.");

            if (index + count > this.m_count)
                throw new ArgumentException(
                    "Arguments denote invalid range of elements.");

            if (count <= 1 || this.m_count <= 1) return;
            ++this.m_version;
            Array.Sort(this.m_array, index, count, comparer);
        }
		
        public override string ToString()
        {
            StringBuilder strBuf = new StringBuilder("CoordinateCollection ");
            strBuf.Append("(");
            for (int i = 0; i < m_array.Length; i++)
            {
                if (i > 0)
                    strBuf.Append(", ");
                strBuf.Append(m_array[i].ToString());
            }
            strBuf.Append(")");

            return strBuf.ToString();
        }

        #endregion

        #region Public Static Methods

        /// <summary> Returns whether equals returns true for any two consecutive Coordinates 
        /// in the given array.
        /// </summary>
        public static bool HasRepeatedCoordinates(ICoordinateList coord)
        {
            for (int i = 1; i < coord.Count; i++)
            {
                if (coord[i - 1].Equals(coord[i]))
                {
                    return true;
                }
            }
            return false;
        }
		
        /// <summary> 
        /// Returns either the given coordinate array if its length is greater than the
        /// given amount, or an empty coordinate array.
        /// </summary>
        public static Coordinate[] AtLeastNCoordinatesOrNothing(int n, Coordinate[] c)
        {
            return c.Length >= n ? c : new Coordinate[]{};
        }
		
        /// <summary> If the coordinate array argument has repeated points,
        /// constructs a new array containing no repeated points.
        /// Otherwise, returns the argument.
        /// </summary>
        /// <seealso cref="HasRepeatedCoordinates(Coordinate[])">
        /// </seealso>
        public static ICoordinateList RemoveRepeatedCoordinates(ICoordinateList coord)
        {
            if (!HasRepeatedCoordinates(coord))
                return coord;
            CoordinateCollection coordList = new CoordinateCollection();

            for (int i = 0; i < coord.Count; i++)
            {
                coordList.Add(coord[i], false);
            }

            return coordList;
        }
		
        /// <summary> 
        /// Returns true if the two arrays are identical, both null, or pointwise
        /// equal (as compared using <see cref="Coordinate.Equals"/>).
        /// </summary>
        /// <seealso cref="Coordinate.Equals">Coordinate.Equals</seealso>
        public static bool Equals(ICoordinateList coord1, ICoordinateList coord2)
        {
            if (coord1 == coord2)
                return true;
			
            if (coord1 == null || coord2 == null)
                return false;
			
            if (coord1.Count != coord2.Count)
                return false;

            for (int i = 0; i < coord1.Count; i++)
            {
                if (!coord1[i].Equals(coord2[i]))
                    return false;
            }

            return true;
        }
		
        /// <summary>  
        /// Returns the minimum coordinate, using the usual lexicographic comparison.
        /// </summary>
        /// <param name="coordinates">The array to search.</param>
        /// <returns>
        /// The minimum coordinate in the array, found using <c>CompareTo</c>
        /// </returns>
        /// <seealso cref="Coordinate.CompareTo">
        /// </seealso>
        public static Coordinate MinimumCoordinate(ICoordinateList coordinates)
        {
            Coordinate minCoord = null;

            for (int i = 0; i < coordinates.Count; i++)
            {
                if (minCoord == null || minCoord.CompareTo(coordinates[i]) > 0)
                {
                    minCoord = coordinates[i];
                }
            }

            return minCoord;
        }
        
        #endregion

        #region Private Methods

        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="CoordinateCollection.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int i)
        {
            ValidateIndex(i, false);
        }

        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="CoordinateCollection.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int i, bool allowEqualEnd)
        {
            int max = (allowEqualEnd)?(m_count):(m_count-1);
            if (i < 0 || i > max)
                throw new System.ArgumentOutOfRangeException("Index was out of range.  Must be non-negative and less than the size of the collection.", (object)i, "Specified argument was out of the range of valid values.");
        }

        private void EnsureCapacity(int min)
        {
            int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
            if (newCapacity < min)
                newCapacity = min;

            this.Capacity = newCapacity;
        }

        #endregion
		
        #region ICollection Members

        void ICollection.CopyTo(Array array, int start)
        {
            if (m_count > array.GetUpperBound(0) + 1 - start)
                throw new System.ArgumentException("Destination array was not long enough.");
			
            Array.Copy(m_array, 0, array, start, m_count); 
            //            this.CopyTo(array, start);
        }
    
        // Copies a section of this list to the given array at the given index.
        // 
        // The method uses the Array.Copy method to copy the elements.
        // 
        public virtual void CopyTo(int index, Array array, int arrayIndex, int count) 
        {
            if (m_count - index < count)
                throw new ArgumentException("Invalid Length");
            if ((array != null) && (array.Rank != 1))
                throw new ArgumentException("Rank of multiple dimension not supported");
            // Delegate rest of error checking to Array.Copy.
            Array.Copy(m_array, index, array, arrayIndex, count);
        }


        #endregion

        #region IList Members

        object IList.this[int i]
        {
            get { return (object)this[i]; }
            set { this[i] = (Coordinate)value; }
        }

        int IList.Add(object x)
        {
            return this.Add((Coordinate)x);
        }

        bool IList.Contains(object x)
        {
            return this.Contains((Coordinate)x);
        }

        int IList.IndexOf(object x)
        {
            return this.IndexOf((Coordinate)x);
        }

        void IList.Insert(int pos, object x)
        {
            this.Insert(pos, (Coordinate)x);
        }

        void IList.Remove(object x)
        {
            this.Remove((Coordinate)x);
        }

        void IList.RemoveAt(int pos)
        {
            this.RemoveAt(pos);
        }

        #endregion

        #region IEnumerable Members
		
        /// <summary>
        ///		Returns an enumerator that can iterate through the <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <returns>An <see cref="Enumerator"/> for the entire <see cref="CoordinateCollection"/>.</returns>
        public virtual ICoordinateEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)(this.GetEnumerator());
        }

        #endregion
		
        #region ICloneable Members

        /// <summary>
        ///	Creates a shallow copy of the <see cref="CoordinateCollection"/>.
        /// </summary>
        public virtual CoordinateCollection Clone()
        {
            int nCount = this.Count;
            Coordinate[] cloneCoordinates = new Coordinate[nCount];
            for (int i = 0; i < nCount; i++)
            {
                cloneCoordinates[i] = m_array[i].Clone();
            }
			
            return new CoordinateCollection(cloneCoordinates);
        }

        ICoordinateList ICoordinateList.Clone()
        {
            return this.Clone();
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        
        #endregion

        #region Nested enumerator class

        /// <summary>
        ///		Supports simple iteration over a <see cref="CoordinateCollection"/>.
        /// </summary>
        private class Enumerator : IEnumerator, ICoordinateEnumerator
        {
            #region Implementation (data)
			
            private CoordinateCollection m_collection;
            private int m_index;
            private int m_version;
			
            #endregion
		
            #region Construction
			
            /// <summary>
            ///		Initializes a new instance of the <c>Enumerator</c> class.
            /// </summary>
            /// <param name="tc"></param>
            internal Enumerator(CoordinateCollection tc)
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
            public Coordinate Current
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
            ///		<see cref="true"/> if the enumerator was successfully advanced to the next element; 
            ///		<see cref="false"/> if the enumerator has passed the end of the collection.
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
        private class SyncCoordinateList : CoordinateCollection
        {
            #region Private Fields
            
            private CoordinateCollection collection;

            private object               m_objRoot;
            
            #endregion

            #region Construction

            internal SyncCoordinateList(CoordinateCollection list) : base(Tag.Default)
            {
                collection = list;
                m_objRoot  = list.SyncRoot;
            }

            #endregion
			
            #region Type-safe ICollection

            public override void CopyTo(Coordinate[] array)
            {
                lock(m_objRoot)
                {
                    collection.CopyTo(array);
                }
            }

            public override void CopyTo(Coordinate[] array, int start)
            {
                lock(m_objRoot)
                {
                    collection.CopyTo(array, start);
                }
            }
			
            public override int Count
            {
                get
                {
                    lock(m_objRoot)
                    {
                        return collection.Count;
                    }
                }
            }

            public override bool IsSynchronized
            {
                get { return true; }
            }

            public override object SyncRoot
            {
                get { return m_objRoot; }
            }
            #endregion
			
            #region Type-safe IList

            public override Coordinate this[int i]
            {
                get
                {
                    lock(m_objRoot)
                    {
                        return collection[i];
                    }
                }
				
                set
                {
                    lock(m_objRoot)
                    {
                        collection[i] = value;
                    }
                }
            }

            public override int Add(Coordinate x)
            {
                lock(m_objRoot)
                {
                   return collection.Add(x);
                }
            }
			
            public override void Clear()
            {
                lock(m_objRoot)
                {
                    collection.Clear();
                }
            }

            public override bool Contains(Coordinate x)
            {
                lock(m_objRoot)
                {
                    return collection.Contains(x);
                }
            }

            public override int IndexOf(Coordinate x)
            {
                lock(m_objRoot)
                {
                    return collection.IndexOf(x);
                }
            }

            public override void Insert(int pos, Coordinate x)
            {
                lock(m_objRoot)
                {
                    collection.Insert(pos,x);
                }
            }

            public override void Remove(Coordinate x)
            {         
                lock(m_objRoot)
                {
                    collection.Remove(x);
                }
            }

            public override void RemoveAt(int pos)
            {
                lock(m_objRoot)
                {
                    collection.RemoveAt(pos);
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

            public override ICoordinateEnumerator GetEnumerator()
            {
                lock(m_objRoot)
                {
                    return collection.GetEnumerator();
                }
            }

            #endregion

            #region Public Helpers

            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    lock(m_objRoot)
                    {
                        return collection.Capacity;
                    }
                }
				
                set
                {
                    lock(m_objRoot)
                    {
                        collection.Capacity = value;
                    }
                }
            }

            public override int AddRange(ICoordinateList x)
            {
                lock(m_objRoot)
                {
                    return collection.AddRange(x);
                }
            }

            public override int AddRange(Coordinate[] x)
            {
                lock(m_objRoot)
                {
                    return collection.AddRange(x);
                }
            }

            #endregion
        }

        #endregion

        #region Nested Read Only Wrapper class

        private class ReadOnlyCoordinateList : CoordinateCollection
        {
            #region Implementation (data)
            private CoordinateCollection m_collection;
            #endregion

            #region Construction

            internal ReadOnlyCoordinateList(CoordinateCollection list) : base(Tag.Default)
            {
                m_collection = list;
            }
            #endregion
			
            #region Type-safe ICollection
            public override void CopyTo(Coordinate[] array)
            {
                m_collection.CopyTo(array);
            }

            public override void CopyTo(Coordinate[] array, int start)
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

            public override Coordinate this[int i]
            {
                get { return m_collection[i]; }
                set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
            }

            public override int Add(Coordinate x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
			
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(Coordinate x)
            {
                return m_collection.Contains(x);
            }

            public override int IndexOf(Coordinate x)
            {
                return m_collection.IndexOf(x);
            }

            public override void Insert(int pos, Coordinate x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(Coordinate x)
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
            
            public override ICoordinateEnumerator GetEnumerator()
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

            public override int AddRange(ICoordinateList x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(Coordinate[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            #endregion
        }

        #endregion
    }
}
