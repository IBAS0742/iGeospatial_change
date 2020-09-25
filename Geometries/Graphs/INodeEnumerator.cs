using System;
using System.Collections;

namespace iGeospatial.Geometries.Graphs
{
    /// <summary>
    ///		Supports type-safe iteration over a <see cref="NodeCollection"/>.
    /// </summary>
    internal interface INodeEnumerator
    {
        /// <summary>
        ///		Gets the current element in the collection.
        /// </summary>
        Node Current {get;}

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
        bool MoveNext();

        /// <summary>
        ///		Sets the enumerator to its initial position, before the first element in the collection.
        /// </summary>
        void Reset();
    }
}
