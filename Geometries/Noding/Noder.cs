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

namespace iGeospatial.Geometries.Noding
{
    /// <summary> 
    /// Computes all intersections between segments in a set of 
    /// <see cref="SegmentString"/>s.
    /// </summary>
    /// <remarks>
    /// Intersections found are represented as <see cref="SegmentNode"/>s 
    /// and added to the <see cref="SegmentString"/>s in which they occur.
    /// As a final step in the noding a new set of segment strings split
    /// at the nodes may be returned.
    /// </remarks>
    internal interface INoder
    {
        /// <summary> 
        /// Gets a <see cref="IList"/> of fully noded 
        /// <see cref="SegmentStrings"/>.
        /// </summary>
        /// <value> 
        /// A <see cref="IList"/> of SegmentStrings
        /// </value>
        /// <remarks>
        /// The SegmentStrings have the same context as their parent.
        /// </remarks>
        IList NodedSubstrings
        {
            get;
        }
		
        /// <summary> 
        /// Computes the noding for a collection of <see cref="SegmentString"/>s.
        /// </summary>
        /// <param name="segStrings">
        /// A collection of <see cref="SegmentString"/>s to node.
        /// </param>
        /// <remarks>
        /// Some Noders may add all these nodes to the input SegmentStrings;
        /// others may only add some or none at all.
        /// </remarks>
        void ComputeNodes(IList segStrings);
    }
}