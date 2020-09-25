using System;
using System.Collections;

namespace iGeospatial.Geometries 
{          
    /// <summary>
    /// Implements a strongly typed collection of <see cref="Geometry"/> elements.
    /// </summary>
    /// <remarks><para>
    /// <c>GeometryList</c> provides an <see cref="ArrayList"/>
    /// that is strongly typed for <see cref="Geometry"/> elements.
    /// </para></remarks>
    [Serializable]
    public class GeometryList : IGeometryList, IList 
    {
        #region Private Fields

        private const int _defaultCapacity = 16;

        internal Geometry[] _array;
        private int _count;

        [NonSerialized]
        private int _version;

        #endregion
        
        #region Constructors and Destructor

        // helper type to identify private ctor
        private enum Tag { Default }

        private GeometryList(Tag tag) { }

        /// <overloads>
        /// Initializes a new instance of the <see cref="GeometryList"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryList"/> class
        /// that is empty and has the default initial capacity.
        /// </summary>
        /// <remarks>Please refer to <see cref="ArrayList()"/> for details.</remarks>
        public GeometryList() 
        {
            this._array = new Geometry[_defaultCapacity];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryList"/> class
        /// that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new
        /// <see cref="GeometryList"/> is initially capable of storing.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is less than zero.</exception>
        /// <remarks>Please refer to <see cref="ArrayList(Int32)"/> for details.</remarks>
        public GeometryList(int capacity) 
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity",
                    capacity, "Argument cannot be negative.");

            this._array = new Geometry[capacity];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryList"/> class
        /// that contains elements copied from the specified collection and
        /// that has the same initial capacity as the number of elements copied.
        /// </summary>
        /// <param name="collection">The <see cref="GeometryList"/>
        /// whose elements are copied to the new collection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is a null reference.</exception>
        /// <remarks>Please refer to <see cref="ArrayList(ICollection)"/> for details.</remarks>
        public GeometryList(GeometryList collection) 
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            this._array = new Geometry[collection.Count];
            Initialize(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryList"/> class
        /// that contains elements copied from the specified <see cref="Geometry"/>
        /// array and that has the same initial capacity as the number of elements copied.
        /// </summary>
        /// <param name="array">An <see cref="Array"/> of <see cref="Geometry"/>
        /// elements that are copied to the new collection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.</exception>
        /// <remarks>Please refer to <see cref="ArrayList(ICollection)"/> for details.</remarks>
        public GeometryList(Geometry[] array) 
        {
            if (array == null)
                throw new ArgumentNullException("array");

            this._array = new Geometry[array.Length];
            Initialize(array);
        }

        #endregion
        
        #region Public Properties

        /// <summary>
        /// Gets the number of elements contained in the <see cref="GeometryList"/>.
        /// </summary>
        /// <value>
        /// The number of elements contained in the <see cref="GeometryList"/>.
        /// </value>
        /// <remarks>Please refer to <see cref="ArrayList.Count"/> for details.</remarks>
        public virtual int Count 
        {
            get { return this._count; }
        }

        /// <summary>
        /// Gets or sets the capacity of the <see cref="GeometryList"/>.
        /// </summary>
        /// <value>The number of elements that the
        /// <see cref="GeometryList"/> can contain.</value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <c>Capacity</c> is set to a value that is less than <see cref="Count"/>.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Capacity"/> for details.</remarks>
        public virtual int Capacity 
        {
            get { return this._array.Length; }
            set 
            {
                if (value == this._array.Length) return;

                if (value < this._count)
                    throw new ArgumentOutOfRangeException("Capacity",
                        value, "Value cannot be less than Count.");

                if (value == 0) 
                {
                    this._array = new Geometry[_defaultCapacity];
                    return;
                }

                Geometry[] newArray = new Geometry[value];
                Array.Copy(this._array, newArray, this._count);
                this._array = newArray;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="GeometryList"/> has a fixed size.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="GeometryList"/> has a fixed size;
        /// otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        /// <remarks>Please refer to <see cref="ArrayList.IsFixedSize"/> for details.</remarks>
        public virtual bool IsFixedSize 
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="GeometryList"/> is read-only.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="GeometryList"/> is read-only;
        /// otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        /// <remarks>Please refer to <see cref="ArrayList.IsReadOnly"/> for details.</remarks>
        public virtual bool IsReadOnly 
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="GeometryList"/>
        /// is synchronized (thread-safe).
        /// </summary>
        /// <value><see langword="true"/> if access to the <see cref="GeometryList"/> is
        /// synchronized (thread-safe); otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        /// <remarks>Please refer to <see cref="ArrayList.IsSynchronized"/> for details.</remarks>
        public virtual bool IsSynchronized 
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="GeometryList"/> 
        /// ensures that all elements are unique.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="GeometryList"/> ensures that all 
        /// elements are unique; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <c>IsUnique</c> returns <see langword="true"/> exactly if the <see cref="GeometryList"/>
        /// is exposed through a <see cref="Unique"/> wrapper. 
        /// Please refer to <see cref="Unique"/> for details.
        /// </remarks>
        public virtual bool IsUnique 
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Geometry"/> element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the
        /// <see cref="Geometry"/> element to get or set.</param>
        /// <value>
        /// The <see cref="Geometry"/> element at the specified <paramref name="index"/>.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException"><para>
        /// The property is set and the <see cref="GeometryList"/> is read-only.
        /// </para><para>-or-</para><para>
        /// The property is set, the <c>GeometryList</c> already contains the
        /// specified element at a different index, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.this"/> for details.</remarks>
        public virtual Geometry this[int index] 
        {
            get 
            {
                ValidateIndex(index);
                return this._array[index];
            }
            set 
            {
                ValidateIndex(index);
                ++this._version;
                this._array[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <value>
        /// The element at the specified <paramref name="index"/>. When the property
        /// is set, this value must be compatible with <see cref="Geometry"/>.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
        /// </exception>
        /// <exception cref="InvalidCastException">The property is set to a value
        /// that is not compatible with <see cref="Geometry"/>.</exception>
        /// <exception cref="NotSupportedException"><para>
        /// The property is set and the <see cref="GeometryList"/> is read-only.
        /// </para><para>-or-</para><para>
        /// The property is set, the <c>GeometryList</c> already contains the
        /// specified element at a different index, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.this"/> for details.</remarks>
        object IList.this[int index] 
        {
            get { return this[index]; }
            set { this[index] = (Geometry) value; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize
        /// access to the <see cref="GeometryList"/>.
        /// </summary>
        /// <value>An object that can be used to synchronize
        /// access to the <see cref="GeometryList"/>.
        /// </value>
        /// <remarks>Please refer to <see cref="ArrayList.SyncRoot"/> for details.</remarks>
        public virtual object SyncRoot 
        {
            get { return this; }
        }
        #endregion
        
        #region Public Methods

        /// <summary>
        /// Adds a <see cref="Geometry"/> to the end of the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="value">The <see cref="Geometry"/> object
        /// to be added to the end of the <see cref="GeometryList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>The <see cref="GeometryList"/> index at which the
        /// <paramref name="value"/> has been added.</returns>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> already contains the specified
        /// <paramref name="value"/>, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.Add"/> for details.</remarks>
        public virtual int Add(Geometry value) 
        {
            if (this._count == this._array.Length)
                EnsureCapacity(this._count + 1);

            ++this._version;
            this._array[this._count] = value;
            return this._count++;
        }

        /// <summary>
        /// Adds an <see cref="Object"/> to the end of the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="value">
        /// The object to be added to the end of the <see cref="GeometryList"/>.
        /// This argument must be compatible with <see cref="Geometry"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>The <see cref="GeometryList"/> index at which the
        /// <paramref name="value"/> has been added.</returns>
        /// <exception cref="InvalidCastException"><paramref name="value"/>
        /// is not compatible with <see cref="Geometry"/>.</exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> already contains the specified
        /// <paramref name="value"/>, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.Add"/> for details.</remarks>
        int IList.Add(object value) 
        {
            return Add((Geometry) value);
        }

        /// <overloads>
        /// Adds a range of elements to the end of the <see cref="GeometryList"/>.
        /// </overloads>
        /// <summary>
        /// Adds the elements of another collection to the end of the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="collection">The <see cref="GeometryList"/> whose elements
        /// should be added to the end of the current collection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is a null reference.</exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> already contains one or more elements
        /// in the specified <paramref name="collection"/>, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.AddRange"/> for details.</remarks>
        public virtual void AddRange(IGeometryList collection) 
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            if (collection.Count == 0) return;
            if (this._count + collection.Count > this._array.Length)
                EnsureCapacity(this._count + collection.Count);

            ++this._version;
            Array.Copy(((GeometryList)collection).GetInnerArray(), 0,
                this._array, this._count, collection.Count);
            this._count += collection.Count;
        }

        /// <overloads>
        /// Adds a range of elements to the end of the <see cref="GeometryList"/>.
        /// </overloads>
        /// <summary>
        /// Adds the elements of another collection to the end of the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="collection">The <see cref="GeometryList"/> whose elements
        /// should be added to the end of the current collection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is a null reference.</exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> already contains one or more elements
        /// in the specified <paramref name="collection"/>, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.AddRange"/> for details.</remarks>
        public virtual void AddRange(GeometryList collection) 
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            if (collection.Count == 0) return;
            if (this._count + collection.Count > this._array.Length)
                EnsureCapacity(this._count + collection.Count);

            ++this._version;
            Array.Copy(collection.GetInnerArray(), 0,
                this._array, this._count, collection.Count);
            this._count += collection.Count;
        }

        /// <summary>
        /// Adds the elements of a <see cref="Geometry"/> array
        /// to the end of the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="array">An <see cref="Array"/> of <see cref="Geometry"/> elements
        /// that should be added to the end of the <see cref="GeometryList"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.</exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> already contains one or more elements
        /// in the specified <paramref name="array"/>, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.AddRange"/> for details.</remarks>
        public virtual void AddRange(Geometry[] array) 
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (array.Length == 0) return;
            if (this._count + array.Length > this._array.Length)
                EnsureCapacity(this._count + array.Length);

            ++this._version;
            Array.Copy(array, 0, this._array, this._count, array.Length);
            this._count += array.Length;
        }

        /// <summary>
        /// Searches the entire sorted <see cref="GeometryList"/> for an
        /// <see cref="Geometry"/> element using the default comparer
        /// and returns the zero-based index of the element.
        /// </summary>
        /// <param name="value">The <see cref="Geometry"/> object
        /// to locate in the <see cref="GeometryList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>The zero-based index of <paramref name="value"/> in the sorted
        /// <see cref="GeometryList"/>, if <paramref name="value"/> is found;
        /// otherwise, a negative number, which is the bitwise complement of the index
        /// of the next element that is larger than <paramref name="value"/> or, if there
        /// is no larger element, the bitwise complement of <see cref="Count"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Neither <paramref name="value"/> nor the elements of the <see cref="GeometryList"/>
        /// implement the <see cref="IComparable"/> interface.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.BinarySearch"/> for details.</remarks>
        public virtual int BinarySearch(Geometry value) 
        {
            return Array.BinarySearch(this._array, 0, this._count, value);
        }

        /// <summary>
        /// Removes all elements from the <see cref="GeometryList"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.Clear"/> for details.</remarks>
        public virtual void Clear() 
        {
            if (this._count == 0) return;

            ++this._version;
            Array.Clear(this._array, 0, this._count);
            this._count = 0;
        }

        /// <summary>
        /// Creates a shallow copy of the <see cref="GeometryList"/>.
        /// </summary>
        /// <returns>A shallow copy of the <see cref="GeometryList"/>.</returns>
        public virtual GeometryList Copy() 
        {
            GeometryList collection = new GeometryList(this._count);

            Array.Copy(this._array, 0, collection._array, 0, this._count);
            collection._count = this._count;
            collection._version = this._version;

            return collection;
        }

        /// <summary>
        /// Determines whether the <see cref="GeometryList"/>
        /// contains the specified <see cref="Geometry"/> element.
        /// </summary>
        /// <param name="value">The <see cref="Geometry"/> object
        /// to locate in the <see cref="GeometryList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns><see langword="true"/> if <paramref name="value"/> is found in the
        /// <see cref="GeometryList"/>; otherwise, <see langword="false"/>.</returns>
        /// <remarks>Please refer to <see cref="ArrayList.Contains"/> for details.</remarks>
        public bool Contains(Geometry value) 
        {
            return (IndexOf(value) >= 0);
        }

        /// <summary>
        /// Determines whether the <see cref="GeometryList"/> contains the specified element.
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="GeometryList"/>.
        /// This argument must be compatible with <see cref="Geometry"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns><see langword="true"/> if <paramref name="value"/> is found in the
        /// <see cref="GeometryList"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="InvalidCastException"><paramref name="value"/>
        /// is not compatible with <see cref="Geometry"/>.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Contains"/> for details.</remarks>
        bool IList.Contains(object value) 
        {
            return Contains((Geometry) value);
        }

        /// <overloads>
        /// Copies the <see cref="GeometryList"/> or a portion of it to a one-dimensional array.
        /// </overloads>
        /// <summary>
        /// Copies the entire <see cref="GeometryList"/> to a one-dimensional <see cref="Array"/>
        /// of <see cref="Geometry"/> elements, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
        /// <see cref="Geometry"/> elements copied from the <see cref="GeometryList"/>.
        /// The <c>Array</c> must have zero-based indexing.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.</exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="GeometryList"/> is greater
        /// than the available space in the destination <paramref name="array"/>.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.CopyTo"/> for details.</remarks>
        public virtual void CopyTo(Geometry[] array) 
        {
            CheckTargetArray(array, 0);
            Array.Copy(this._array, array, this._count);
        }

        /// <summary>
        /// Copies the entire <see cref="GeometryList"/> to a one-dimensional <see cref="Array"/>
        /// of <see cref="Geometry"/> elements, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
        /// <see cref="Geometry"/> elements copied from the <see cref="GeometryList"/>.
        /// The <c>Array</c> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/>
        /// at which copying begins.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than zero.</exception>
        /// <exception cref="ArgumentException"><para>
        /// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
        /// </para><para>-or-</para><para>
        /// The number of elements in the source <see cref="GeometryList"/> is greater than the
        /// available space from <paramref name="arrayIndex"/> to the end of the destination
        /// <paramref name="array"/>.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.CopyTo"/> for details.</remarks>
        public virtual void CopyTo(Geometry[] array, int arrayIndex) 
        {
            CheckTargetArray(array, arrayIndex);
            Array.Copy(this._array, 0, array, arrayIndex, this._count);
        }

        /// <summary>
        /// Copies the entire <see cref="GeometryList"/> to a one-dimensional <see cref="Array"/>,
        /// starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
        /// <see cref="Geometry"/> elements copied from the <see cref="GeometryList"/>.
        /// The <c>Array</c> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/>
        /// at which copying begins.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than zero.</exception>
        /// <exception cref="ArgumentException"><para>
        /// <paramref name="array"/> is multidimensional.
        /// </para><para>-or-</para><para>
        /// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
        /// </para><para>-or-</para><para>
        /// The number of elements in the source <see cref="GeometryList"/> is greater than the
        /// available space from <paramref name="arrayIndex"/> to the end of the destination
        /// <paramref name="array"/>.</para></exception>
        /// <exception cref="InvalidCastException">
        /// The <see cref="Geometry"/> type cannot be cast automatically
        /// to the type of the destination <paramref name="array"/>.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.CopyTo"/> for details.</remarks>
        void ICollection.CopyTo(Array array, int arrayIndex) 
        {
            CheckTargetArray(array, arrayIndex);
            CopyTo((Geometry[]) array, arrayIndex);
        }

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the specified
        /// <see cref="Geometry"/> in the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="value">The <see cref="Geometry"/> object
        /// to locate in the <see cref="GeometryList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="value"/>
        /// in the <see cref="GeometryList"/>, if found; otherwise, -1.
        /// </returns>
        /// <remarks>Please refer to <see cref="ArrayList.IndexOf"/> for details.</remarks>
        public virtual int IndexOf(Geometry value) 
        {
            return Array.IndexOf(this._array, value, 0, this._count);
        }

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the specified
        /// <see cref="Object"/> in the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="GeometryList"/>.
        /// This argument must be compatible with <see cref="Geometry"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="value"/>
        /// in the <see cref="GeometryList"/>, if found; otherwise, -1.
        /// </returns>
        /// <exception cref="InvalidCastException"><paramref name="value"/>
        /// is not compatible with <see cref="Geometry"/>.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.IndexOf"/> for details.</remarks>
        int IList.IndexOf(object value) 
        {
            return IndexOf((Geometry) value);
        }

        /// <summary>
        /// Inserts a <see cref="Geometry"/> element into the
        /// <see cref="GeometryList"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value"/>
        /// should be inserted.</param>
        /// <param name="value">The <see cref="Geometry"/> object
        /// to insert into the <see cref="GeometryList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is greater than <see cref="Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> already contains the specified
        /// <paramref name="value"/>, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.Insert"/> for details.</remarks>
        public virtual void Insert(int index, Geometry value) 
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index",
                    index, "Argument cannot be negative.");

            if (index > this._count)
                throw new ArgumentOutOfRangeException("index",
                    index, "Argument cannot exceed Count.");

            if (this._count == this._array.Length)
                EnsureCapacity(this._count + 1);

            ++this._version;
            if (index < this._count)
                Array.Copy(this._array, index,
                    this._array, index + 1, this._count - index);

            this._array[index] = value;
            ++this._count;
        }

        /// <summary>
        /// Inserts an element into the <see cref="GeometryList"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value"/>
        /// should be inserted.</param>
        /// <param name="value">The object to insert into the <see cref="GeometryList"/>.
        /// This argument must be compatible with <see cref="Geometry"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is greater than <see cref="Count"/>.</para>
        /// </exception>
        /// <exception cref="InvalidCastException"><paramref name="value"/>
        /// is not compatible with <see cref="Geometry"/>.</exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> already contains the specified
        /// <paramref name="value"/>, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.Insert"/> for details.</remarks>
        void IList.Insert(int index, object value) 
        {
            Insert(index, (Geometry) value);
        }

        /// <summary>
        /// Returns a read-only wrapper for the specified <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="collection">The <see cref="GeometryList"/> to wrap.</param>
        /// <returns>A read-only wrapper around <paramref name="collection"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is a null reference.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.ReadOnly"/> for details.</remarks>
        public static GeometryList ReadOnly(GeometryList collection) 
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            return new ReadOnlyList(collection);
        }

        /// <summary>
        /// Removes the first occurrence of the specified <see cref="Geometry"/>
        /// from the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="value">The <see cref="Geometry"/> object
        /// to remove from the <see cref="GeometryList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.Remove"/> for details.</remarks>
        public virtual void Remove(Geometry value) 
        {
            int index = IndexOf(value);
            if (index >= 0) RemoveAt(index);
        }

        /// <summary>
        /// Removes the first occurrence of the specified <see cref="Object"/>
        /// from the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="value">The object to remove from the <see cref="GeometryList"/>.
        /// This argument must be compatible with <see cref="Geometry"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <exception cref="InvalidCastException"><paramref name="value"/>
        /// is not compatible with <see cref="Geometry"/>.</exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.Remove"/> for details.</remarks>
        void IList.Remove(object value) 
        {
            Remove((Geometry) value);
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.RemoveAt"/> for details.</remarks>
        public virtual void RemoveAt(int index) 
        {
            ValidateIndex(index);

            ++this._version;

            if (index < --this._count)
            {
                int nNext = index;
                nNext     = nNext + 1;

                Array.Copy(this._array, nNext,
                    this._array, index, this._count - index);
            }

            this._array[this._count] = null;
        }

        /// <summary>
        /// Removes the specified range of elements from the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range
        /// of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote a
        /// valid range of elements in the <see cref="GeometryList"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="count"/> is less than zero.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.RemoveRange"/> for details.</remarks>
        public virtual void RemoveRange(int index, int count) 
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index",
                    index, "Argument cannot be negative.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count",
                    count, "Argument cannot be negative.");

            if (index + count > this._count)
                throw new ArgumentException(
                    "Arguments denote invalid range of elements.");

            if (count == 0) return;

            ++this._version;
            this._count -= count;

            if (index < this._count)
                Array.Copy(this._array, index + count,
                    this._array, index, this._count - index);

            Array.Clear(this._array, this._count, count);
        }

        /// <overloads>
        /// Reverses the order of the elements in the 
        /// <see cref="GeometryList"/> or a portion of it.
        /// </overloads>
        /// <summary>
        /// Reverses the order of the elements in the entire <see cref="GeometryList"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// The <see cref="GeometryList"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Reverse"/> for details.</remarks>
        public virtual void Reverse() 
        {
            if (this._count <= 1) return;
            ++this._version;
            Array.Reverse(this._array, 0, this._count);
        }

        /// <summary>
        /// Reverses the order of the elements in the specified range.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range
        /// of elements to reverse.</param>
        /// <param name="count">The number of elements to reverse.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote a
        /// valid range of elements in the <see cref="GeometryList"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="count"/> is less than zero.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="GeometryList"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Reverse"/> for details.</remarks>
        public virtual void Reverse(int index, int count) 
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index",
                    index, "Argument cannot be negative.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count",
                    count, "Argument cannot be negative.");

            if (index + count > this._count)
                throw new ArgumentException(
                    "Arguments denote invalid range of elements.");

            if (count <= 1 || this._count <= 1) return;
            ++this._version;
            Array.Reverse(this._array, index, count);
        }

        /// <overloads>
        /// Sorts the elements in the <see cref="GeometryList"/> or a portion of it.
        /// </overloads>
        /// <summary>
        /// Sorts the elements in the entire <see cref="GeometryList"/>
        /// using the <see cref="IComparable"/> implementation of each element.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// The <see cref="GeometryList"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Sort"/> for details.</remarks>
        public virtual void Sort() 
        {
            if (this._count <= 1) return;
            ++this._version;
            Array.Sort(this._array, 0, this._count);
        }

        /// <summary>
        /// Sorts the elements in the entire <see cref="GeometryList"/>
        /// using the specified <see cref="IComparer"/> interface.
        /// </summary>
        /// <param name="comparer">
        /// <para>The <see cref="IComparer"/> implementation to use when comparing elements.</para>
        /// <para>-or-</para>
        /// <para>A null reference to use the <see cref="IComparable"/> implementation 
        /// of each element.</para></param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="GeometryList"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Sort"/> for details.</remarks>
        public virtual void Sort(IComparer comparer) 
        {
            if (this._count <= 1) return;
            ++this._version;
            Array.Sort(this._array, 0, this._count, comparer);
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
        /// valid range of elements in the <see cref="GeometryList"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="count"/> is less than zero.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="GeometryList"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Sort"/> for details.</remarks>
        public virtual void Sort(int index, int count, IComparer comparer) 
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index",
                    index, "Argument cannot be negative.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count",
                    count, "Argument cannot be negative.");

            if (index + count > this._count)
                throw new ArgumentException(
                    "Arguments denote invalid range of elements.");

            if (count <= 1 || this._count <= 1) return;
            ++this._version;
            Array.Sort(this._array, index, count, comparer);
        }

        /// <summary>
        /// Returns a synchronized (thread-safe) wrapper
        /// for the specified <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="collection">The <see cref="GeometryList"/> to synchronize.</param>
        /// <returns>
        /// A synchronized (thread-safe) wrapper around <paramref name="collection"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is a null reference.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Synchronized"/> for details.</remarks>
        public static GeometryList Synchronized(GeometryList collection) 
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            return new SyncList(collection);
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="GeometryList"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.TrimToSize"/> for details.</remarks>
        public virtual void TrimToSize() 
        {
            Capacity = this._count;
        }

        /// <summary>
        /// Returns a wrapper for the specified <see cref="GeometryList"/>
        /// ensuring that all elements are unique.
        /// </summary>
        /// <param name="collection">The <see cref="GeometryList"/> to wrap.</param>    
        /// <returns>
        /// A wrapper around <paramref name="collection"/> ensuring that all elements are unique.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="collection"/> contains duplicate elements.</exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is a null reference.</exception>
        /// <remarks><para>
        /// The <c>Unique</c> wrapper provides a set-like collection by ensuring
        /// that all elements in the <see cref="GeometryList"/> are unique.
        /// </para><para>
        /// <c>Unique</c> raises an <see cref="ArgumentException"/> if the specified 
        /// <paramref name="collection"/> contains any duplicate elements. The returned
        /// wrapper raises a <see cref="NotSupportedException"/> whenever the user attempts 
        /// to add an element that is already contained in the <c>GeometryList</c>.
        /// </para><para>
        /// <strong>Note:</strong> The <c>Unique</c> wrapper reflects any changes made
        /// to the underlying <paramref name="collection"/>, including the possible
        /// creation of duplicate elements. The uniqueness of all elements is therefore
        /// no longer assured if the underlying collection is manipulated directly.
        /// </para></remarks>
        public static GeometryList Unique(GeometryList collection) 
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
                           
            for (int i = collection.Count - 1; i > 0; i--)
            {
                if (collection.IndexOf(collection[i]) < i)
                {
                    throw new ArgumentException(
                        "Argument cannot contain duplicate elements.",
                        "collection");
                }
            }

            return new UniqueList(collection);
        }

        /// <summary>
        /// Copies the elements of the <see cref="GeometryList"/> to a new
        /// <see cref="Array"/> of <see cref="Geometry"/> elements.
        /// </summary>
        /// <returns>A one-dimensional <see cref="Array"/> of <see cref="Geometry"/>
        /// elements containing copies of the elements of the <see cref="GeometryList"/>.</returns>
        /// <remarks>Please refer to <see cref="ArrayList.ToArray"/> for details.</remarks>
        public virtual Geometry[] ToArray() 
        {
            Geometry[] array = new Geometry[this._count];
            Array.Copy(this._array, 0, array, 0, this._count);
//            Array.Copy(this._array, array, this._count);
            return array;
        }
		
        /// <summary>  
        /// Converts the List to an array.
        /// </summary>
        /// <param name="points">The List of Points to convert.</param>
        /// <returns>The List in array format.</returns>
        public Point[] ToPointArray()
        {
            if (this._count > 0 && this[0].GeometryType == GeometryType.Point)
            {
                Point[] pointArray = new Point[this._count];
                Array.Copy(this._array, 0, pointArray, 0, this._count);
//                this.CopyTo(pointArray, 0);

                return pointArray;
            }

            return null;
        }
		
        /// <summary>  Converts the List to an array.
        /// 
        /// </summary>
        /// <param name="lineStrings"> the List of LineStrings to convert
        /// </param>
        /// <returns>              the List in array format
        /// </returns>
        public LineString[] ToLineStringArray()
        {
            if (this._count > 0 && 
                this[0].GeometryType == GeometryType.LineString)
            {
                LineString[] lineStringArray = new LineString[this._count];
                Array.Copy(this._array, 0, lineStringArray, 0, this._count);
//                this.CopyTo(lineStringArray, 0);

                return lineStringArray;
            }

            return null;
        }
		
        /// <summary>  Converts the List to an array.
        /// 
        /// </summary>
        /// <param name="linearRings"> the List of LinearRings to convert
        /// </param>
        /// <returns>              the List in array format
        /// </returns>
        public LinearRing[] ToLinearRingArray()
        {
            if (this._count > 0 && 
                this[0].GeometryType == GeometryType.LinearRing)
            {
                LinearRing[] linearRingArray = new LinearRing[this._count];
                Array.Copy(this._array, 0, linearRingArray, 0, this._count);
//                this.CopyTo(linearRingArray, 0);

                return linearRingArray;
            }

            return null;
        }
		
        /// <summary>  Converts the List to an array.
        /// 
        /// </summary>
        /// <param name="polygons"> the List of Polygons to convert
        /// </param>
        /// <returns>           the List in array format
        /// </returns>
        public Polygon[] ToPolygonArray()
        {
            if (this._count > 0 && 
                this[0].GeometryType == GeometryType.Polygon)
            {
                Polygon[] polygonArray = new Polygon[this._count];
                Array.Copy(this._array, 0, polygonArray, 0, this._count);
//                this.CopyTo(polygonArray, 0);

                return polygonArray;
            }

            return null;
        }
		
        /// <summary>  Converts the List to an array.
        /// 
        /// </summary>
        /// <param name="multiPolygons"> the List of MultiPolygons to convert
        /// </param>
        /// <returns>                the List in array format
        /// </returns>
        public MultiPolygon[] ToMultiPolygonArray()
        {
            if (this._count > 0 && 
                this[0].GeometryType == GeometryType.MultiPolygon)
            {
                MultiPolygon[] multiPolygonArray = 
                    new MultiPolygon[this._count];
                Array.Copy(this._array, 0, multiPolygonArray, 0, this._count);
//                this.CopyTo(multiPolygonArray, 0);

                return multiPolygonArray;
            }

            return null;
        }
		
        /// <summary>  Converts the List to an array.
        /// 
        /// </summary>
        /// <param name="multiLineStrings"> the List of MultiLineStrings to convert
        /// </param>
        /// <returns>                   the List in array format
        /// </returns>
        public MultiLineString[] ToMultiLineStringArray()
        {
            if (this._count > 0 && 
                this[0].GeometryType == GeometryType.MultiLineString)
            {
                MultiLineString[] multiLineStringArray = 
                    new MultiLineString[this._count];
                Array.Copy(this._array, 0, multiLineStringArray, 0, this._count);
//                this.CopyTo(multiLineStringArray, 0);

                return multiLineStringArray;
            }

            return null;
        }
		
        /// <summary>  Converts the List to an array.
        /// 
        /// </summary>
        /// <param name="multiPoints"> the List of MultiPoints to convert
        /// </param>
        /// <returns>              the List in array format
        /// </returns>
        public MultiPoint[] ToMultiPointArray()
        {
            if (this._count > 0 && 
                this[0].GeometryType == GeometryType.MultiPoint)
            {
                MultiPoint[] multiPointArray = new MultiPoint[this._count];
                Array.Copy(this._array, 0, multiPointArray, 0, this._count);
//                this.CopyTo(multiPointArray, 0);

                return multiPointArray;
            }

            return null;
        }

        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Gets the list of elements contained in the <see cref="GeometryList"/> instance.
        /// </summary>
        /// <returns>
        /// A one-dimensional <see cref="Array"/> with zero-based indexing that contains all 
        /// <see cref="Geometry"/> elements in the <see cref="GeometryList"/>.
        /// </returns>
        /// <remarks>
        /// Use <c>InnerArray</c> to access the element array of a <see cref="GeometryList"/>
        /// instance that might be a read-only or synchronized wrapper. This is necessary because
        /// the element array field of wrapper classes is always a null reference.
        /// </remarks>
        protected virtual Geometry[] GetInnerArray() 
        {
            return this._array; 
        }

        #endregion
        
        #region Private Methods

        private void CheckEnumIndex(int index) 
        {
            if (index < 0 || index >= this._count)
            {
                throw new InvalidOperationException(
                    "Enumerator is not on a collection element.");
            }
        }

        private void CheckEnumVersion(int version) 
        {
            if (version != this._version)
            {
                throw new InvalidOperationException(
                    "Enumerator invalidated by modification to collection.");
            }
        }

        private void CheckTargetArray(Array array, int arrayIndex) 
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (array.Rank > 1)
            {
                throw new ArgumentException(
                    "Argument cannot be multidimensional.", "array");
            }

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex",
                    arrayIndex, "Argument cannot be negative.");
            if (arrayIndex >= array.Length)
            {
                throw new ArgumentException(
                    "Argument must be less than array length.", "arrayIndex");
            }

            if (this._count > array.Length - arrayIndex)
            {
                throw new ArgumentException(
                    "Argument section must be large enough for collection.", "array");
            }
        }

        private void UpdateCapacity(int newCapacity)
        {
            if (newCapacity == this._array.Length) 
                return;

            if (newCapacity < this._count)
            {
                throw new ArgumentOutOfRangeException("newCapacity",
                    newCapacity, "Value cannot be less than Count.");
            }

            if (newCapacity == 0) 
            {
                this._array = new Geometry[_defaultCapacity];
                return;
            }

            Geometry[] newArray = new Geometry[newCapacity];
            Array.Copy(this._array, newArray, this._count);

            this._array = newArray;
        }

        private void EnsureCapacity(int minimum) 
        {
            int newCapacity = (this._array.Length == 0 ?
            _defaultCapacity : this._array.Length * 2);

            if (newCapacity < minimum) 
                newCapacity = minimum;

            UpdateCapacity(newCapacity);
        }

        private void ValidateIndex(int index) 
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index",
                    index, "Argument cannot be negative.");

            if (index >= this._count)
                throw new ArgumentOutOfRangeException("index",
                    index, "Argument must be less than Count.");
        }

        /// <overloads>
        /// Adds a range of elements to the end of the <see cref="GeometryList"/>.
        /// </overloads>
        /// <summary>
        /// Adds the elements of another collection to the end of the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="collection">The <see cref="GeometryList"/> whose elements
        /// should be added to the end of the current collection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is a null reference.</exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> already contains one or more elements
        /// in the specified <paramref name="collection"/>, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.AddRange"/> for details.</remarks>
        private void Initialize(IGeometryList collection) 
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            if (collection.Count == 0) return;
            if (this._count + collection.Count > this._array.Length)
                EnsureCapacity(this._count + collection.Count);

            ++this._version;
            Array.Copy(((GeometryList)collection).GetInnerArray(), 0,
                this._array, this._count, collection.Count);
            this._count += collection.Count;
        }

        /// <overloads>
        /// Adds a range of elements to the end of the <see cref="GeometryList"/>.
        /// </overloads>
        /// <summary>
        /// Adds the elements of another collection to the end of the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="collection">The <see cref="GeometryList"/> whose elements
        /// should be added to the end of the current collection.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collection"/> is a null reference.</exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> already contains one or more elements
        /// in the specified <paramref name="collection"/>, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.AddRange"/> for details.</remarks>
        private void Initialize(GeometryList collection) 
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            if (collection.Count == 0) return;
            if (this._count + collection.Count > this._array.Length)
                EnsureCapacity(this._count + collection.Count);

            ++this._version;
            Array.Copy(collection.GetInnerArray(), 0,
                this._array, this._count, collection.Count);
            this._count += collection.Count;
        }

        /// <summary>
        /// Adds the elements of a <see cref="Geometry"/> array
        /// to the end of the <see cref="GeometryList"/>.
        /// </summary>
        /// <param name="array">An <see cref="Array"/> of <see cref="Geometry"/> elements
        /// that should be added to the end of the <see cref="GeometryList"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.</exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="GeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> has a fixed size.</para>
        /// <para>-or-</para>
        /// <para>The <c>GeometryList</c> already contains one or more elements
        /// in the specified <paramref name="array"/>, and the <c>GeometryList</c>
        /// ensures that all elements are unique.</para></exception>
        /// <remarks>Please refer to <see cref="ArrayList.AddRange"/> for details.</remarks>
        private void Initialize(Geometry[] array) 
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (array.Length == 0) return;
            if (this._count + array.Length > this._array.Length)
                EnsureCapacity(this._count + array.Length);

            ++this._version;
            Array.Copy(array, 0, this._array, this._count, array.Length);
            this._count += array.Length;
        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        /// Returns an <see cref="IGeometryEnumerator"/> that can
        /// iterate through the <see cref="GeometryList"/>.
        /// </summary>
        /// <returns>An <see cref="IGeometryEnumerator"/>
        /// for the entire <see cref="GeometryList"/>.</returns>
        /// <remarks>Please refer to <see cref="ArrayList.GetEnumerator"/> for details.</remarks>
        public virtual IGeometryEnumerator GetEnumerator() 
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> that can
        /// iterate through the <see cref="GeometryList"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/>
        /// for the entire <see cref="GeometryList"/>.</returns>
        /// <remarks>Please refer to <see cref="ArrayList.GetEnumerator"/> for details.</remarks>
        IEnumerator IEnumerable.GetEnumerator() 
        {
            return (IEnumerator) GetEnumerator();
        }
        
        #endregion

        #region ICloneable Members
        
        /// <summary>
        /// Creates a deep copy of the <see cref="GeometryList"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="GeometryList"/>.</returns>
        /// <remarks>
        /// <c>Clone</c> is similar to <see cref="Clone"/> but creates a deep copy of
        /// every <see cref="Geometry"/> element in the <see cref="GeometryList"/>
        /// by invoking its <see cref="Geometry.Clone"/> method.
        /// <para>
        /// <c>Clone</c> never returns a <c>GeometryList</c> with a read-only,
        /// synchronized, or unique wrapper, whereas <c>Copy</c> preserves any
        /// wrappers around this <c>GeometryList</c>.
        /// </para>
        /// </remarks>
        public virtual GeometryList Clone() 
        {
            GeometryList collection = new GeometryList(this._count);

            for (int i = 0; i < this._count; i++)
            {
                if  (this._array[i] != null)
                {
                    collection._array[i] = (Geometry) this._array[i].Clone();
                }
            }

            collection._count   = this._count;
            collection._version = this._version;

            return collection;
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="GeometryList"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="GeometryList"/>.</returns>
        object ICloneable.Clone()
        {
            return this.Clone();
        }
        
        #endregion

        #region Class Enumerator

        [Serializable]
        private sealed class Enumerator : IGeometryEnumerator, IEnumerator 
        {
            #region Private Fields

            private readonly GeometryList _collection;
            private readonly int _version;
            private int _index;

            #endregion

            #region Internal Constructors

            internal Enumerator(GeometryList collection) 
            {
                this._collection = collection;
                this._version = collection._version;
                this._index = -1;
            }

            #endregion

            #region Public Properties

            public Geometry Current 
            {
                get 
                {
                    this._collection.CheckEnumIndex(this._index);
                    this._collection.CheckEnumVersion(this._version);
                    return this._collection[this._index];
                }
            }

            object IEnumerator.Current 
            {
                get { return Current; }
            }

            #endregion

            #region Public Methods

            public bool MoveNext() 
            {
                this._collection.CheckEnumVersion(this._version);
                return (++this._index < this._collection.Count);
            }

            public void Reset() 
            {
                this._collection.CheckEnumVersion(this._version);
                this._index = -1;
            }

            #endregion
        }

        #endregion

        #region Class ReadOnlyList

        [Serializable]
        private sealed class ReadOnlyList : GeometryList 
        {
            #region Private Fields

            private GeometryList _collection;

            #endregion
            
            #region Internal Constructors

            internal ReadOnlyList(GeometryList collection):
                base(Tag.Default) 
            {
                this._collection = collection;
            }

            #endregion

            #region Public Properties

            public override int Capacity 
            {
                get { return this._collection.Capacity; }
                set 
                {
                    throw new NotSupportedException(
                        "Read-only collections cannot be modified."); }
            }

            public override int Count 
            {
                get { return this._collection.Count; }
            }

            public override bool IsFixedSize 
            {
                get { return true; }
            }

            public override bool IsReadOnly 
            {
                get { return true; }
            }

            public override bool IsSynchronized 
            {
                get { return this._collection.IsSynchronized; }
            }

            public override bool IsUnique 
            {
                get { return this._collection.IsUnique; }
            }

            public override Geometry this[int index] 
            {
                get { return this._collection[index]; }
                set 
                {
                    throw new NotSupportedException(
                        "Read-only collections cannot be modified."); }
            }

            public override object SyncRoot 
            {
                get { return this._collection.SyncRoot; }
            }

            #endregion
            
            #region Public Methods

            public override int Add(Geometry value) 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override void AddRange(IGeometryList collection) 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override void AddRange(GeometryList collection) 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override void AddRange(Geometry[] array) 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override int BinarySearch(Geometry value) 
            {
                return this._collection.BinarySearch(value);
            }

            public override void Clear() 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override GeometryList Clone() 
            {
                return this._collection.Clone();
            }

            public override GeometryList Copy() 
            {
                return new ReadOnlyList((GeometryList) this._collection.Copy());
            }

            public override void CopyTo(Geometry[] array) 
            {
                this._collection.CopyTo(array);
            }

            public override void CopyTo(Geometry[] array, int arrayIndex) 
            {
                this._collection.CopyTo(array, arrayIndex);
            }

            public override IGeometryEnumerator GetEnumerator() 
            {
                return this._collection.GetEnumerator();
            }

            public override int IndexOf(Geometry value) 
            {
                return this._collection.IndexOf(value);
            }

            public override void Insert(int index, Geometry value) 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override void Remove(Geometry value) 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override void RemoveAt(int index) 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override void RemoveRange(int index, int count) 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override void Reverse() 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override void Reverse(int index, int count) 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override void Sort() 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override void Sort(IComparer comparer) 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override void Sort(int index, int count, IComparer comparer) 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            public override Geometry[] ToArray() 
            {
                return this._collection.ToArray();
            }

            public override void TrimToSize() 
            {
                throw new NotSupportedException(
                    "Read-only collections cannot be modified.");
            }

            #endregion

            #region Protected Methods

            protected override Geometry[] GetInnerArray()
            {
                return this._collection.GetInnerArray(); 
            }

            #endregion
        }

        #endregion
        
        #region Class SyncList

        [Serializable]
        private sealed class SyncList : GeometryList 
        {
            #region Private Fields

            private GeometryList _collection;
            private object _root;

            #endregion

            #region Internal Constructors

            internal SyncList(GeometryList collection):
                base(Tag.Default) 
            {

                this._root = collection.SyncRoot;
                this._collection = collection;
            }

            #endregion

            #region Public Properties

            public override int Capacity 
            {
                get { lock (this._root) return this._collection.Capacity; }
                set { lock (this._root) this._collection.Capacity = value; }
            }

            public override int Count 
            {
                get { lock (this._root) return this._collection.Count; }
            }

            public override bool IsFixedSize 
            {
                get { return this._collection.IsFixedSize; }
            }

            public override bool IsReadOnly 
            {
                get { return this._collection.IsReadOnly; }
            }

            public override bool IsSynchronized 
            {
                get { return true; }
            }

            public override bool IsUnique 
            {
                get { return this._collection.IsUnique; }
            }

            public override Geometry this[int index] 
            {
                get { lock (this._root) return this._collection[index]; }
                set { lock (this._root) this._collection[index] = value;  }
            }

            public override object SyncRoot 
            {
                get { return this._root; }
            }

            #endregion

            #region Public Methods

            public override int Add(Geometry value) 
            {
                lock (this._root) return this._collection.Add(value);
            }

            public override void AddRange(IGeometryList collection) 
            {
                lock (this._root) this._collection.AddRange(collection);
            }

            public override void AddRange(GeometryList collection) 
            {
                lock (this._root) this._collection.AddRange(collection);
            }

            public override void AddRange(Geometry[] array) 
            {
                lock (this._root) this._collection.AddRange(array);
            }

            public override int BinarySearch(Geometry value) 
            {
                lock (this._root) return this._collection.BinarySearch(value);
            }

            public override void Clear() 
            {
                lock (this._root) this._collection.Clear();
            }

            public override GeometryList Clone() 
            {
                lock (this._root) return this._collection.Clone();
            }

            public override GeometryList Copy() 
            {
                lock (this._root)
                    return new SyncList((GeometryList) this._collection.Copy());
            }

            public override void CopyTo(Geometry[] array) 
            {
                lock (this._root) this._collection.CopyTo(array);
            }

            public override void CopyTo(Geometry[] array, int arrayIndex) 
            {
                lock (this._root) this._collection.CopyTo(array, arrayIndex);
            }

            public override IGeometryEnumerator GetEnumerator() 
            {
                lock (this._root) return this._collection.GetEnumerator();
            }

            public override int IndexOf(Geometry value) 
            {
                lock (this._root) return this._collection.IndexOf(value);
            }

            public override void Insert(int index, Geometry value) 
            {
                lock (this._root) this._collection.Insert(index, value);
            }

            public override void Remove(Geometry value) 
            {
                lock (this._root) this._collection.Remove(value);
            }

            public override void RemoveAt(int index) 
            {
                lock (this._root) this._collection.RemoveAt(index);
            }

            public override void RemoveRange(int index, int count) 
            {
                lock (this._root) this._collection.RemoveRange(index, count);
            }

            public override void Reverse() 
            {
                lock (this._root) this._collection.Reverse();
            }

            public override void Reverse(int index, int count) 
            {
                lock (this._root) this._collection.Reverse(index, count);
            }

            public override void Sort() 
            {
                lock (this._root) this._collection.Sort();
            }

            public override void Sort(IComparer comparer) 
            {
                lock (this._root) this._collection.Sort(comparer);
            }

            public override void Sort(int index, int count, IComparer comparer) 
            {
                lock (this._root) this._collection.Sort(index, count, comparer);
            }

            public override Geometry[] ToArray() 
            {
                lock (this._root) return this._collection.ToArray();
            }

            public override void TrimToSize() 
            {
                lock (this._root) this._collection.TrimToSize();
            }

            #endregion

            #region Protected Methods

            protected override Geometry[] GetInnerArray()
            {
                lock (this._root) 
                    return this._collection.GetInnerArray(); 
            }

            #endregion
        }

        #endregion
        
        #region Class UniqueList

        [Serializable]
        private sealed class UniqueList : GeometryList 
        {
            #region Private Fields

            private GeometryList _collection;

            #endregion

            #region Internal Constructors

            internal UniqueList(GeometryList collection) 
                : base(Tag.Default) 
            {
                this._collection = collection;
            }

            #endregion

            #region Public Properties

            public override int Capacity 
            {
                get { return this._collection.Capacity; }
                set { this._collection.Capacity = value; }
            }

            public override int Count 
            {
                get { return this._collection.Count; }
            }

            public override bool IsFixedSize 
            {
                get { return this._collection.IsFixedSize; }
            }

            public override bool IsReadOnly 
            {
                get { return this._collection.IsReadOnly; }
            }

            public override bool IsSynchronized 
            {
                get { return this._collection.IsSynchronized; }
            }

            public override bool IsUnique 
            {
                get { return true; }
            }

            public override Geometry this[int index] 
            {
                get { return this._collection[index]; }
                set 
                {
                    CheckUnique(index, value);
                    this._collection[index] = value;
                }
            }

            public override object SyncRoot 
            {
                get { return this._collection.SyncRoot; }
            }

            #endregion

            #region Public Methods

            public override int Add(Geometry value) 
            {
                CheckUnique(value);
                return this._collection.Add(value);
            }

            public override void AddRange(IGeometryList collection) 
            {
                if (collection == null)
                {
                    throw new ArgumentNullException("collection");
                }

                foreach (Geometry value in collection)
                    CheckUnique(value);
            
                this._collection.AddRange(collection);
            }

            public override void AddRange(GeometryList collection) 
            {
                if (collection == null)
                {
                    throw new ArgumentNullException("collection");
                }

                foreach (Geometry value in collection)
                    CheckUnique(value);
            
                this._collection.AddRange(collection);
            }

            public override void AddRange(Geometry[] array) 
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }

                foreach (Geometry value in array)
                    CheckUnique(value);
            
                this._collection.AddRange(array);
            }

            public override int BinarySearch(Geometry value) 
            {
                return this._collection.BinarySearch(value);
            }

            public override void Clear() 
            {
                this._collection.Clear();
            }

            public override GeometryList Clone() 
            {
                return this._collection.Clone();
            }

            public override GeometryList Copy() 
            {
                return new UniqueList((GeometryList) this._collection.Copy());
            }

            public override void CopyTo(Geometry[] array) 
            {
                this._collection.CopyTo(array);
            }

            public override void CopyTo(Geometry[] array, int arrayIndex) 
            {
                this._collection.CopyTo(array, arrayIndex);
            }

            public override IGeometryEnumerator GetEnumerator() 
            {
                return this._collection.GetEnumerator();
            }

            public override int IndexOf(Geometry value) 
            {
                return this._collection.IndexOf(value);
            }

            public override void Insert(int index, Geometry value) 
            {
                CheckUnique(value);
                this._collection.Insert(index, value);
            }

            public override void Remove(Geometry value) 
            {
                this._collection.Remove(value);
            }

            public override void RemoveAt(int index) 
            {
                this._collection.RemoveAt(index);
            }

            public override void RemoveRange(int index, int count) 
            {
                this._collection.RemoveRange(index, count);
            }

            public override void Reverse() 
            {
                this._collection.Reverse();
            }

            public override void Reverse(int index, int count) 
            {
                this._collection.Reverse(index, count);
            }

            public override void Sort() 
            {
                this._collection.Sort();
            }

            public override void Sort(IComparer comparer) 
            {
                this._collection.Sort(comparer);
            }

            public override void Sort(int index, int count, IComparer comparer) 
            {
                this._collection.Sort(index, count, comparer);
            }

            public override Geometry[] ToArray() 
            {
                return this._collection.ToArray();
            }

            public override void TrimToSize() 
            {
                this._collection.TrimToSize();
            }

            #endregion

            #region Protected Methods

            protected override Geometry[] GetInnerArray()
            {
                return this._collection.GetInnerArray(); 
            }

            #endregion

            #region Private Methods

            private void CheckUnique(Geometry value) 
            {
                if (IndexOf(value) >= 0)
                    throw new NotSupportedException(
                        "Unique collections cannot contain duplicate elements.");
            }

            private void CheckUnique(int index, Geometry value) 
            {
                int existing = IndexOf(value);
                if (existing >= 0 && existing != index)
                    throw new NotSupportedException(
                        "Unique collections cannot contain duplicate elements.");
            }

            #endregion
        }

        #endregion
    }
}
