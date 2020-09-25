#region License
// <copyright>
//         iGeospatial Geometries Package
//       
// This is part of the Open Geospatial Library for .NET.
// 
// Package Description:
// This is a collection of C# classes that implement the fundamental 
// operations required to validate a given geo-spatial data set to 
// a known topological specification.
// It aims to provide a complete implementation of the Open Geospatial
// Consortium (www.opengeospatial.org) specifications for Simple 
// Feature Geometry.
// 
// Contact Information:
//     Paul Selormey (paulselormey@gmail.com or paul@toolscenter.org)
//     
// Credits:
// This library is based on the JTS Topology Suite, a Java library by
// 
//     Vivid Solutions Inc. (www.vividsolutions.com)  
//     
// License:
// See the license.txt file in the package directory.   
// </copyright>
#endregion

using System;
using System.Collections;

namespace iGeospatial.Geometries.Graphs
{
    /// <summary>
    ///		Supports type-safe iteration over a <see cref="EdgeCollection"/>.
    /// </summary>
    internal interface IEdgeEnumerator
    {
        /// <summary>
        ///		Gets the current element in the collection.
        /// </summary>
        Edge Current {get;}

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
