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
	/// <summary> Base class for <see cref="INoder"/>s which make a single
	/// pass to find intersections.
	/// This allows using a custom <see cref="ISegmentIntersector"/>
	/// (which for instance may simply identify intersections, rather than
	/// insert them).
	/// 
	/// </summary>
	internal abstract class SinglePassNoder : INoder
	{
		internal ISegmentIntersector segInt;
		
		protected SinglePassNoder()
		{
		}
		
		/// <summary> 
		/// Gets or sets the SegmentIntersector to use with this noder.
		/// </summary>
		/// <remarks>
		/// A SegmentIntersector will normally add intersection nodes
		/// to the input segment strings, but it may not - it may simply 
		/// record the presence of intersections.
		/// However, some Noders may require that intersections be added.
		/// </remarks>
		/// <value></value>
		public ISegmentIntersector SegmentIntersector
		{
            get
            {
                return this.segInt;
            }

			set
			{
				this.segInt = value;
			}
		}
			
		/// <summary> 
		/// Returns a collection of fully noded <see cref="SegmentString"/>s.
		/// The SegmentStrings have the same context as their parent.
		/// </summary>
		/// <value> 
		/// A Collection of SegmentStrings.
		/// </value>
		public abstract IList NodedSubstrings
        {
            get;
        }
		
		/// <summary> 
		/// Computes the noding for a collection of <see cref="SegmentString"/>s.
		/// Some Noders may add all these nodes to the input SegmentStrings;
		/// others may only add some or none at all.
		/// </summary>
		/// <param name="segStrings">a collection of <see cref="SegmentString"/>s to node
		/// </param>
		public abstract void ComputeNodes(IList segStrings);
	}
}