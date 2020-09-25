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

using iGeospatial.Geometries;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Indexers.Chain
{
	
	/// <summary> 
	/// The action for the internal iterator for performing
	/// overlap queries on a MonotoneChain
	/// </summary>
    [Serializable]
    internal class MonotoneChainOverlapAction
	{
		// these envelopes are used during the MonotoneChain search process
		internal Envelope tempEnv1;

		internal Envelope tempEnv2;
		
		internal LineSegment overlapSeg1;

        internal LineSegment overlapSeg2;
		
		public MonotoneChainOverlapAction()
		{
            tempEnv1    = new Envelope();
            tempEnv2    = new Envelope();
            overlapSeg1 = new LineSegment((GeometryFactory)null);
            overlapSeg2 = new LineSegment((GeometryFactory)null);
		}

		/// <summary> This function can be overridden if the original chains are needed
		/// 
		/// </summary>
		/// <param name="start1">
		/// the index of the start of the overlapping segment from mc1
		/// </param>
		/// <param name="start2">
		/// the index of the start of the overlapping segment from mc2
		/// </param>
		public virtual void Overlap(MonotoneChain mc1, int start1, 
            MonotoneChain mc2, int start2)
		{
			mc1.GetLineSegment(start1, overlapSeg1);
			mc2.GetLineSegment(start2, overlapSeg2);

			Overlap(overlapSeg1, overlapSeg2);
		}
		
		/// <summary> 
		/// This is a convenience function which can be overridden to obtain the actual
		/// line segments which overlap
		/// </summary>
		/// <param name="seg1"> </param>
		/// <param name="seg2"> </param>
		public virtual void Overlap(LineSegment seg1, LineSegment seg2)
		{
		}
	}
}