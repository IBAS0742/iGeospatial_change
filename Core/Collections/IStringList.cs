using System;
using System.Collections;

namespace iGeospatial.Collections
{
    /// <summary>
    /// Represents a strongly typed collection of <see cref="string"/>
    /// objects that can be individually accessed by index.
    /// </summary>
    /// <remarks>
    /// IStringList provides an <see cref="IList"/>
    /// that is strongly typed for <see cref="string"/> elements.
    /// </remarks>
    public interface IStringList : IStringCollection 
    {
        #region Properties
        #region IsFixedSize

        /// <summary>
        /// Gets a value indicating whether the <see cref="IStringList"/> has a fixed size.
        /// </summary>
        /// <value><c>true</c> if the <see cref="IStringList"/> has a fixed size;
        /// otherwise, <c>false</c>. The default is <c>false</c>.</value>
        /// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>

        bool IsFixedSize { get; }

        #endregion
        #region IsReadOnly

        /// <summary>
        /// Gets a value indicating whether the <see cref="IStringList"/> is read-only.
        /// </summary>
        /// <value><c>true</c> if the <see cref="IStringList"/> is read-only;
        /// otherwise, <c>false</c>. The default is <c>false</c>.</value>
        /// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>

        bool IsReadOnly { get; }

        #endregion
        #region Item

        /// <summary>
        /// Gets or sets the <see cref="string"/> element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the
        /// <see cref="string"/> element to get or set.</param>
        /// <value>
        /// The <see cref="string"/> element at the specified <paramref name="index"/>.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than
        /// <see cref="IStringCollection.Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The property is set and the <see cref="IStringList"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>

        string this[int index] { get; set; }

        #endregion
        #endregion

        #region Methods
        #region Add

        /// <summary>
        /// Adds a <see cref="string"/> to the end
        /// of the <see cref="IStringList"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> object
        /// to be added to the end of the <see cref="IStringList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>The <see cref="IStringList"/> index at which
        /// the <paramref name="value"/> has been added.</returns>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="IStringList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <b>IStringList</b> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>

        int Add(string value);

        #endregion
        #region Clear

        /// <summary>
        /// Removes all elements from the <see cref="IStringList"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="IStringList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <b>IStringList</b> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>

        void Clear();

        #endregion
        #region Contains

        /// <summary>
        /// Determines whether the <see cref="IStringList"/>
        /// contains the specified <see cref="string"/> element.
        /// </summary>
        /// <param name="value">The <see cref="string"/> object
        /// to locate in the <see cref="IStringList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns><c>true</c> if <paramref name="value"/> is found in the
        /// <see cref="IStringList"/>; otherwise, <c>false</c>.</returns>
        /// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>

        bool Contains(string value);

        #endregion
        #region IndexOf

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the specified
        /// <see cref="string"/> in the <see cref="IStringList"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> object
        /// to locate in the <see cref="IStringList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="value"/>
        /// in the <see cref="IStringList"/>, if found; otherwise, -1.
        /// </returns>
        /// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>

        int IndexOf(string value);

        #endregion
        #region Insert

        /// <summary>
        /// Inserts a <see cref="string"/> element into the
        /// <see cref="IStringList"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which
        /// <paramref name="value"/> should be inserted.</param>
        /// <param name="value">The <see cref="string"/> object
        /// to insert into the <see cref="IStringList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is greater than
        /// <see cref="IStringCollection.Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="IStringList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <b>IStringList</b> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>

        void Insert(int index, string value);

        #endregion
        #region Remove

        /// <summary>
        /// Removes the first occurrence of the specified <see cref="string"/>
        /// from the <see cref="IStringList"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> object
        /// to remove from the <see cref="IStringList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="IStringList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <b>IStringList</b> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>

        void Remove(string value);

        #endregion
        #region RemoveAt

        /// <summary>
        /// Removes the element at the specified index of the
        /// <see cref="IStringList"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than
        /// <see cref="IStringCollection.Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="IStringList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <b>IStringList</b> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>

        void RemoveAt(int index);

        #endregion
        #endregion
    }
}
