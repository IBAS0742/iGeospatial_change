using System;
using System.Collections;

namespace iGeospatial.Collections
{
    /// <summary>
    ///		Supports type-safe iteration over a <see cref="IntegerCollection"/>.
    /// </summary>
    public interface IIntegerEnumerator
    {
        /// <summary>
        ///		Gets the current element in the collection.
        /// </summary>
        Int32 Current {get;}

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
        bool MoveNext();

        /// <summary>
        ///		Sets the enumerator to its initial position, before the first element in the collection.
        /// </summary>
        void Reset();
    }
}
