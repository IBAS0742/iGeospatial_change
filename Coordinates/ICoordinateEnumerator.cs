using System;

namespace iGeospatial.Coordinates
{
    /// <summary>
    /// Supports type-safe iteration over a collection that
    /// contains <see cref="Coordinate"/> elements.
    /// </summary>
    /// <remarks>
    /// <b>ICoordinateEnumerator</b> provides an <see cref="IEnumerator"/>
    /// that is strongly typed for <see cref="Coordinate"/> elements.
    /// </remarks>
    public interface ICoordinateEnumerator 
    {
        #region Properties

        /// <summary>
        /// Gets the current <see cref="Coordinate"/> element in the collection.
        /// </summary>
        /// <value>The current <see cref="Coordinate"/> element in the collection.</value>
        /// <exception cref="InvalidOperationException"><para>The enumerator is positioned
        /// before the first element of the collection or after the last element.</para>
        /// <para>-or-</para>
        /// <para>The collection was modified after the enumerator was created.</para></exception>
        /// <remarks>Please refer to <see cref="IEnumerator.Current"/> for details, but note
        /// that <b>Current</b> fails if the collection was modified since the last successful
        /// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>
        Coordinate Current { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns><see cref="true"/> if the enumerator was successfully advanced to the next element;
        /// <see cref="false"/> if the enumerator has passed the end of the collection.</returns>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified after the enumerator was created.</exception>
        /// <remarks>Please refer to <see cref="IEnumerator.MoveNext"/> for details.</remarks>
        bool MoveNext();

        /// <summary>
        /// Sets the enumerator to its initial position,
        /// which is before the first element in the collection.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The collection was modified after the enumerator was created.</exception>
        /// <remarks>Please refer to <see cref="IEnumerator.Reset"/> for details.</remarks>
        void Reset();

        #endregion
    }
}
