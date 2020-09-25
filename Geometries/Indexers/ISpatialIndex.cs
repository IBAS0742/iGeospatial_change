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

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Indexers
{
	/// <summary> 
	/// The basic operations supported by classes implementing spatial 
	/// index algorithms.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A spatial index typically provides a primary filter for range rectangle 
	/// queries. A secondary filter is required to test for exact intersection. 
	/// Of course, this secondary filter may consist of other tests besides 
	/// intersection, such as testing other kinds of spatial relationships.
	/// </para>
	/// </remarks>
	public interface ISpatialIndex
	{
		/// <summary> 
		/// Adds a spatial item with an extent specified by the given 
		/// <see cref="iGeospatial.Coordinates.Envelope"/> to the index
		/// </summary>
		void Insert(Envelope itemEnv, object item);
		
		/// <summary> 
		/// Queries the index for all items whose extents intersect the given 
		/// search <see cref="iGeospatial.Coordinates.Envelope"/> or rectangular
		/// area.
		/// </summary>
		/// <param name="searchEnv"> 
		/// The envelope defining the search or query area. 
		/// </param>
		/// <returns> A list of the items found by the query. </returns>
		/// <remarks>
		/// Note that some kinds of indexes may also return objects which do not in fact
		/// intersect the query envelope.
		/// </remarks>
		IList Query(Envelope searchEnv);
		
        /// <summary> 
        /// Queries the index for all items whose extents intersect the 
        /// given search <see cref="Envelope"/>, and applies an 
        /// <see cref="ISpatialIndexVisitor"/> to them.
        /// Note that some kinds of indexes may also return objects which do not in fact
        /// intersect the query envelope.
        /// 
        /// </summary>
        /// <param name="searchEnv">The envelope to query for.</param>
        /// <param name="visitor">
        /// A visitor object to apply to the items found.
        /// </param>
        void Query(Envelope searchEnv, ISpatialIndexVisitor visitor);
		
        /// <summary> Removes a single item from the tree.
        /// 
        /// </summary>
        /// <param name="itemEnv">the Envelope of the item to remove
        /// </param>
        /// <param name="item">the item to remove
        /// </param>
        /// <returns> <see langword="true"/> if the item was found
        /// </returns>
        bool Remove(Envelope itemEnv, object item);
    }
}