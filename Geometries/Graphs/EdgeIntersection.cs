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

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Graphs
{
	/// <summary> 
	/// Represents a point on an edge which intersects with another edge.
	/// </summary>
	/// <remarks>
	/// The intersection may either be a single point, or a line segment
	/// (in which case this point is the start of the line segment)
	/// The intersection point must be precise.
	/// </remarks>
	internal class EdgeIntersection : IComparable
	{
        #region Public Members

        /// <summary>
        /// the point of intersection
        /// </summary>
		public Coordinate coord; 

        /// <summary>
        /// the index of the containing line segment in the parent edge
        /// </summary>
		public int segmentIndex; 

        /// <summary>
        /// the edge distance of this point along the containing line segment
        /// </summary>
		public double dist;
		
        #endregion

		public EdgeIntersection(Coordinate coord, int segmentIndex, double dist)
		{
			this.coord        = new Coordinate(coord);
			this.segmentIndex = segmentIndex;
			this.dist         = dist;
		}
		
		public int CompareTo(object obj)
		{
			EdgeIntersection other = (EdgeIntersection)obj;

			return Compare(other.segmentIndex, other.dist);
		}

        /// <summary>
        /// Compares the current <see cref="EdgeIntersection"/> fields with 
        /// the specified segment index and distance values.
        /// </summary>
        /// <param name="segmentIndex">Index of the line segment</param>
        /// <param name="dist">
        /// The edge distance of this point along the line segment.
        /// </param>
        /// <returns> 
        /// Returns
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// -1, if this EdgeIntersection is located before the argument location
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// 0, if this EdgeIntersection is at the argument location
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// 1, if this EdgeIntersection is located after the argument location
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public int Compare(int segmentIndex, double dist)
		{
			if (this.segmentIndex < segmentIndex)
				return -1;
			if (this.segmentIndex > segmentIndex)
				return 1;
			if (this.dist < dist)
				return -1;
			if (this.dist > dist)
				return 1;
			return 0;
		}
		
		public bool IsEndPoint(int maxSegmentIndex)
		{
			if (segmentIndex == 0 && dist == 0.0)
				return true;
			if (segmentIndex == maxSegmentIndex)
				return true;

			return false;
		}
	}
}