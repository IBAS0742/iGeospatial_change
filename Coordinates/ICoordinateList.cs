using System;
using System.Collections;
using iGeospatial.Coordinates;

namespace iGeospatial.Coordinates 
{
    /// <summary>
    /// Represents a strongly typed collection of <see cref="Coordinate"/>
    /// objects that can be individually accessed by index.
    /// </summary>
    /// <remarks>
    /// <c>ICoordinateList</c> provides an <see cref="IList"/>
    /// that is strongly typed for <see cref="Coordinate"/> elements.
    /// </remarks>
    public interface ICoordinateList : ICoordinateCollection, ICloneable 
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICoordinateList"/> has a fixed size.
        /// </summary>
        /// <value><see cref="true"/> if the <see cref="ICoordinateList"/> has a fixed size;
        /// otherwise, <see cref="false"/>. The default is <see cref="false"/>.</value>
        /// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>
        bool IsFixedSize 
        { 
            get; 
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICoordinateList"/> is read-only.
        /// </summary>
        /// <value><see cref="true"/> if the <see cref="ICoordinateList"/> is read-only;
        /// otherwise, <see cref="false"/>. The default is <see cref="false"/>.</value>
        /// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>
        bool IsReadOnly 
        { 
            get; 
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
        bool IsPacked
        {
            get;
        }

        /// <summary>
        /// Gets or sets the <see cref="Coordinate"/> element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the
        /// <see cref="Coordinate"/> element to get or set.</param>
        /// <value>
        /// The <see cref="Coordinate"/> element at the specified <paramref name="index"/>.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than
        /// <see cref="ICoordinateCollection.Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The property is set and the <see cref="ICoordinateList"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>
        Coordinate this[int index] { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Coordinate"/> element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the
        /// <see cref="Coordinate"/> element to get or set.</param>
        /// <value>
        /// The <see cref="Coordinate"/> element at the specified <paramref name="index"/>.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than
        /// <see cref="ICoordinateCollection.Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The property is set and the <see cref="ICoordinateList"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>
        Coordinate this[int index, Coordinate inout] { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a <see cref="Coordinate"/> to the end
        /// of the <see cref="ICoordinateList"/>.
        /// </summary>
        /// <param name="value">The <see cref="Coordinate"/> object
        /// to be added to the end of the <see cref="ICoordinateList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>The <see cref="ICoordinateList"/> index at which
        /// the <paramref name="value"/> has been added.</returns>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="ICoordinateList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <b>ICoordinateList</b> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>
        int Add(Coordinate value);

        /// <summary>
        /// Removes all elements from the <see cref="ICoordinateList"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="ICoordinateList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <b>ICoordinateList</b> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>
        void Clear();

        /// <summary>
        /// Determines whether the <see cref="ICoordinateList"/>
        /// contains the specified <see cref="Coordinate"/> element.
        /// </summary>
        /// <param name="value">The <see cref="Coordinate"/> object
        /// to locate in the <see cref="ICoordinateList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns><see cref="true"/> if <paramref name="value"/> is found in the
        /// <see cref="ICoordinateList"/>; otherwise, <see cref="false"/>.</returns>
        /// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>
        bool Contains(Coordinate value);

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the specified
        /// <see cref="Coordinate"/> in the <see cref="ICoordinateList"/>.
        /// </summary>
        /// <param name="value">The <see cref="Coordinate"/> object
        /// to locate in the <see cref="ICoordinateList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="value"/>
        /// in the <see cref="ICoordinateList"/>, if found; otherwise, -1.
        /// </returns>
        /// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>
        int IndexOf(Coordinate value);

        /// <summary>
        /// Inserts a <see cref="Coordinate"/> element into the
        /// <see cref="ICoordinateList"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which
        /// <paramref name="value"/> should be inserted.</param>
        /// <param name="value">The <see cref="Coordinate"/> object
        /// to insert into the <see cref="ICoordinateList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is greater than
        /// <see cref="ICoordinateCollection.Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="ICoordinateList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <b>ICoordinateList</b> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>
        void Insert(int index, Coordinate value);

        /// <summary>
        /// Removes the first occurrence of the specified <see cref="Coordinate"/>
        /// from the <see cref="ICoordinateList"/>.
        /// </summary>
        /// <param name="value">The <see cref="Coordinate"/> object
        /// to remove from the <see cref="ICoordinateList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="ICoordinateList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <b>ICoordinateList</b> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>
        void Remove(Coordinate value);

        /// <summary>
        /// Removes the element at the specified index of the
        /// <see cref="ICoordinateList"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than
        /// <see cref="ICoordinateCollection.Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="ICoordinateList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <b>ICoordinateList</b> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>
        void RemoveAt(int index);

        /// <summary>
        /// Copies the elements of the <see cref="ICoordinateList"/> to a new
        /// <see cref="Array"/> of <see cref="Coordinate"/> elements.
        /// </summary>
        /// <returns>A one-dimensional <see cref="Array"/> of <see cref="Coordinate"/>
        /// elements containing copies of the elements of the <see cref="CoordinateCollection"/>.</returns>
        /// <remarks>Please refer to <see cref="ArrayList.ToArray"/> for details.</remarks>
        Coordinate[] ToArray();

        /// <overloads>
        /// Reverses the order of the elements in the 
        /// <see cref="ICoordinateList"/> or a portion of it.
        /// </overloads>
        /// <summary>
        /// Reverses the order of the elements in the entire <see cref="CoordinateCollection"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ICoordinateList"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="ArrayList.Reverse"/> for details.</remarks>
        void Reverse();

        /// <summary>
        ///		Adds the elements of another <c>ICoordinateList</c> to the current <c>CoordinateCollection</c>.
        /// </summary>
        /// <param name="x">The <c>ICoordinateList</c> whose elements should be added to the end of the current <c>CoordinateCollection</c>.</param>
        /// <returns>The new <see cref="ICoordinateList.Count"/> of the <c>CoordinateCollection</c>.</returns>
        int AddRange(ICoordinateList x);

        /// <summary>
        ///		Adds the elements of a <see cref="Coordinate"/> array to the current <c>CoordinateCollection</c>.
        /// </summary>
        /// <param name="x">The <see cref="Coordinate"/> array whose elements should be added to the end of the <c>CoordinateCollection</c>.</param>
        /// <returns>The new <see cref="ICoordinateList.Count"/> of the <c>CoordinateCollection</c>.</returns>
        int AddRange(Coordinate[] x);

        /// <summary>
        ///	Copies the entire <c>ICoordinateList</c> to a one-dimensional
        ///	<see cref="Coordinate"/> array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Coordinate"/> array to copy to.</param>
        void CopyTo(Coordinate[] array);

        bool Add(ICoordinateList coord, bool allowRepeated, bool direction);

        bool Add(Coordinate[] coord, bool allowRepeated, bool direction);
		
        bool Add(ICoordinateList coord, bool allowRepeated);
		
        bool Add(Coordinate[] coord, bool allowRepeated);

        void Add(Coordinate coord, bool allowRepeated);

        void MakePrecise(PrecisionModel precision);

        new ICoordinateList Clone();

        #endregion
    }
}
