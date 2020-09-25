using System;
using System.Collections;

namespace iGeospatial.Coordinates
{
    /// <summary>
    /// Defines size, enumerators, and synchronization methods for strongly
    /// typed collections of <see cref="Coordinate"/> elements.
    /// </summary>
    /// <remarks>
    /// <b>ICoordinateCollection</b> provides an <see cref="ICollection"/>
    /// that is strongly typed for <see cref="Coordinate"/> elements.
    /// </remarks>
    public interface ICoordinateCollection 
    {
        #region Properties

        /// <summary>
        /// Gets the number of elements contained in the
        /// <see cref="ICoordinateCollection"/>.
        /// </summary>
        /// <value>The number of elements contained in the
        /// <see cref="ICoordinateCollection"/>.</value>
        /// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>
        int Count { get; }

        /// <summary>
        /// Gets a value indicating whether access to the
        /// <see cref="ICoordinateCollection"/> is synchronized (thread-safe).
        /// </summary>
        /// <value><see cref="true"/> if access to the <see cref="ICoordinateCollection"/> is
        /// synchronized (thread-safe); otherwise, <see cref="false"/>. The default is <see cref="false"/>.</value>
        /// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>
        bool IsSynchronized { get; }

        /// <summary>
        /// Gets an object that can be used to synchronize access
        /// to the <see cref="ICoordinateCollection"/>.
        /// </summary>
        /// <value>An object that can be used to synchronize access
        /// to the <see cref="ICoordinateCollection"/>.</value>
        /// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>
        object SyncRoot { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Copies the entire <see cref="ICoordinateCollection"/> to a one-dimensional <see cref="Array"/>
        /// of <see cref="Coordinate"/> elements, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
        /// <see cref="Coordinate"/> elements copied from the <see cref="ICoordinateCollection"/>.
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
        /// The number of elements in the source <see cref="ICoordinateCollection"/> is greater
        /// than the available space from <paramref name="arrayIndex"/> to the end of the destination
        /// <paramref name="array"/>.</para></exception>
        /// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>
        void CopyTo(Coordinate[] array, int arrayIndex);

        /// <summary>
        /// Returns an <see cref="ICoordinateEnumerator"/> that can
        /// iterate through the <see cref="ICoordinateCollection"/>.
        /// </summary>
        /// <returns>An <see cref="ICoordinateEnumerator"/>
        /// for the entire <see cref="ICoordinateCollection"/>.</returns>
        /// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>
        ICoordinateEnumerator GetEnumerator();

        #endregion
    }
}
