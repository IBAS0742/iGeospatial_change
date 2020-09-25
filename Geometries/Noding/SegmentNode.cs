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

namespace iGeospatial.Geometries.Noding
{
	/// <summary> 
	/// Represents an intersection point between two <see cref="SegmentString"/>s.
	/// </summary>
    [Serializable]
    internal class SegmentNode : IComparable
	{
        #region Private Fields

        private SegmentString segString;
        private Coordinate coord; // the point of intersection
        private int segmentIndex; // the index of the containing line segment in the parent edge
        private int segmentOctant;
        private bool isInterior;
				
        #endregion

        #region Constructors and Destructor

        public SegmentNode(SegmentString segString, Coordinate coord, 
            int segmentIndex, int segmentOctant)
        {
            this.segString     = segString;
            this.coord         = new Coordinate(coord);
            this.segmentIndex  = segmentIndex;
            this.segmentOctant = segmentOctant;
            isInterior         = !coord.Equals(
                segString.GetCoordinate(segmentIndex));
        }
        
        #endregion

        #region Public Properties

        public Coordinate Coordinate
        {
            get
            {
                return this.coord;
            }
        }

        public int SegmentIndex
        {
            get
            {
                return this.segmentIndex;
            }
        }
		
        public bool Interior
        {
            get
            {
                return this.isInterior;
            }
        }
        
        #endregion

        #region Public Methods

        public bool IsEndPoint(int maxSegmentIndex)
        {
            if (segmentIndex == 0 && !isInterior)
                return true;

            if (segmentIndex == maxSegmentIndex)
                return true;
            
            return false;
        }
        
        #endregion
		
        #region IComparable Members

		public int CompareTo(SegmentNode other)
		{
            if (segmentIndex < other.segmentIndex)
                return -1;

            if (segmentIndex > other.segmentIndex)
                return 1;
			
            if (coord.Equals(other.coord))
                return 0;
			
            return SegmentPointComparator.Compare(segmentOctant, 
                coord, other.coord);
        }
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj">
        /// </param>
        /// <returns> 
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// -1 this SegmentNode is located before the argument location
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// 0 this SegmentNode is at the argument location
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// 1 this SegmentNode is located after the argument location
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(object obj)
        {
            SegmentNode other = (SegmentNode) obj;
			
            if (segmentIndex < other.segmentIndex)
                return -1;

            if (segmentIndex > other.segmentIndex)
                return 1;
			
            if (coord.Equals(other.coord))
                return 0;
			
            return SegmentPointComparator.Compare(segmentOctant, 
                coord, other.coord);
        }
        
        #endregion
	}
}