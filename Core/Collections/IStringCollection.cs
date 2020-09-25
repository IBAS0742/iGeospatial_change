using System;
using System.Collections;

namespace iGeospatial.Collections
{
    /// <summary>
    /// Defines size, enumerators, and synchronization methods for strongly
    /// typed collections of <see cref="string"/> elements.
    /// </summary>
    /// <remarks>
    /// IStringCollection provides an <see cref="ICollection"/>
    /// that is strongly typed for <see cref="string"/> elements.
    /// </remarks>
    public interface IStringCollection 
    {
        #region Properties

        #region Count

        /// <summary>
        /// Gets the number of elements contained in the
        /// <see cref="IStringCollection"/>.
        /// </summary>
        /// <value>The number of elements contained in the
        /// <see cref="IStringCollection"/>.</value>
        /// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>
        int Count { get; }

        #endregion
        
        #region IsSynchronized

        /// <summary>
        /// Gets a value indicating whether access to the
        /// <see cref="IStringCollection"/> is synchronized (thread-safe).
        /// </summary>
        /// <value><c>true</c> if access to the <see cref="IStringCollection"/> is
        /// synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
        /// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>
        bool IsSynchronized { get; }

        #endregion
        
        #region SyncRoot

        /// <summary>
        /// Gets an object that can be used to synchronize access
        /// to the <see cref="IStringCollection"/>.
        /// </summary>
        /// <value>An object that can be used to synchronize access
        /// to the <see cref="IStringCollection"/>.</value>
        /// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>

        object SyncRoot { get; }

        #endregion
        
        #endregion

        #region Methods
        
        #region CopyTo

        /// <summary>
        /// Copies the entire <see cref="IStringCollection"/> to a one-dimensional <see cref="Array"/>
        /// of <see cref="string"/> elements, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
        /// <see cref="string"/> elements copied from the <see cref="IStringCollection"/>.
        /// The <b>Array</b> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/>
        /// at which copying begins.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than zero.</exception>
        /// <exception cref="ArgumentException"><para>
        /// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
        /// </para><para>-or-</para><para>
        /// The number of elements in the source <see cref="IStringCollection"/> is greater
        /// than the available space from <paramref name="arrayIndex"/> to the end of the destination
        /// <paramref name="array"/>.</para></exception>
        /// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>
        void CopyTo(string[] array, int arrayIndex);

        #endregion
        
        #region GetEnumerator

        /// <summary>
        /// Returns an <see cref="IStringEnumerator"/> that can
        /// iterate through the <see cref="IStringCollection"/>.
        /// </summary>
        /// <returns>An <see cref="IStringEnumerator"/>
        /// for the entire <see cref="IStringCollection"/>.</returns>
        /// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>

        IStringEnumerator GetEnumerator();

        #endregion

        #endregion
    }
}
