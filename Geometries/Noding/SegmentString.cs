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
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Noding
{
    /// <summary> 
    /// This represents a list of contiguous line segments, and supports 
    /// noding the segments.
    /// </summary>
    /// <remarks>
    /// The line segments are represented by an array of <see cref="Coordinate"/>s.
    /// Intended to optimize the noding of contiguous segments by reducing 
    /// the number of allocated objects.
    /// <para>
    /// SegmentStrings can carry a context object, which is useful
    /// for preserving topological or parentage information.
    /// All noded substrings are initialized with the same context object.
    /// </para>
    /// </remarks>
    [Serializable]
    internal class SegmentString
	{
        #region Private Fields

        private SegmentNodeList m_objNodeList;
        private ICoordinateList pts;
        private object          m_objData;
        
        #endregion
		
        #region Constructors and Destructor

        /// <summary> 
        /// Creates a new segment string from a list of vertices.
        /// </summary>
        /// <param name="pts">
        /// The vertices of the segment string.
        /// </param>
        /// <param name="data">
        /// The user-defined data of this segment string (may be null).
        /// </param>
        public SegmentString(ICoordinateList pts, object data)
        {
            m_objNodeList = new SegmentNodeList(this);
            this.pts  = pts;
            m_objData = data;
        }
        
        #endregion
		
        #region Public Properties

        /// <summary> 
        /// Gets or sets the user-defined data for this segment string.
        /// </summary>
        /// <value>The user-defined data.</value>
       public object Data
        {
            get
            {
                return m_objData;
            }
			
            set
            {
                m_objData = value;
            }
        }

		public SegmentNodeList NodeList
		{
			get
			{
				return m_objNodeList;
			}
		}
		
		public ICoordinateList Coordinates
		{
			get
			{
				return pts;
			}
		}
		
        public int Count
        {
            get
            {
                return pts.Count;
            }
        }
		
        public Coordinate this[int index]
        {
            get
            {
                return pts[index];
            }
        }
		
		public bool Closed
		{
			get
			{
				return pts[0].Equals(pts[pts.Count - 1]);
			}
		}
        
        #endregion

        #region Public Methods

		public Coordinate GetCoordinate(int i)
		{
			return pts[i];
		}
		
        /// <summary> 
        /// Gets the octant of the segment starting at vertex <c>index</c>.
        /// </summary>
        /// <param name="index">
        /// The index of the vertex starting the segment. Must not be the 
        /// last index in the vertex list
        /// </param>
        /// <returns>The octant of the segment at the vertex.</returns>
        public int GetSegmentOctant(int index)
        {
            if (index == pts.Count - 1)
                return -1;

            return Octant.GetOctant(GetCoordinate(index), 
                GetCoordinate(index + 1));
        }
		
		/// <summary> Adds EdgeIntersections for one or both
		/// intersections found for a segment of an edge to the edge intersection list.
		/// </summary>
		public void AddIntersections(LineIntersector li, 
            int segmentIndex, int geomIndex)
		{
			for (int i = 0; i < li.IntersectionNum; i++)
			{
				AddIntersection(li, segmentIndex, geomIndex, i);
			}
		}
		/// <summary> Add an SegmentNode for intersection intIndex.
		/// An intersection that falls exactly on a vertex
		/// of the SegmentString is normalized
		/// to use the higher of the two possible segmentIndexes
		/// </summary>
		public void AddIntersection(LineIntersector li, 
            int segmentIndex, int geomIndex, int intIndex)
		{
			Coordinate intPt = new Coordinate(li.GetIntersection(intIndex));

			AddIntersection(intPt, segmentIndex);
		}
		
        public virtual void AddIntersection(Coordinate intPt, int segmentIndex)
        {
            int normalizedSegmentIndex = segmentIndex;

            // normalize the intersection point location
            int nextSegIndex = normalizedSegmentIndex + 1;
            if (nextSegIndex < pts.Count)
            {
                Coordinate nextPt = pts[nextSegIndex];
				
                // Normalize segment index if intPt falls on vertex
                // The check for point equality is 2D only - Z values are ignored
                if (intPt.Equals(nextPt))
                {
                    normalizedSegmentIndex = nextSegIndex;
                }
            }
            
            // Add the intersection point to edge intersection list.
            m_objNodeList.Add(intPt, normalizedSegmentIndex);
        }
		
//		/// <summary> 
//		/// Add an EdgeIntersection for intersection intIndex.
//		/// An intersection that falls exactly on a vertex of the edge is normalized
//		/// to use the higher of the two possible segmentIndexes
//		/// </summary>
//		public void AddIntersection(Coordinate intPt, int segmentIndex)
//		{
//			double dist = LineIntersector.ComputeEdgeDistance(intPt, 
//                pts[segmentIndex], pts[segmentIndex + 1]);
//
//			AddIntersection(intPt, segmentIndex, dist);
//		}
//		
//		public void AddIntersection(Coordinate intPt, int segmentIndex, double dist)
//		{
//			int normalizedSegmentIndex = segmentIndex;
//			//Debug.println("edge intpt: " + intPt + " dist: " + dist);
//			// Normalize the intersection point location
//			int nextSegIndex = normalizedSegmentIndex + 1;
//			if (nextSegIndex < pts.Count)
//			{
//				Coordinate nextPt = pts[nextSegIndex];
//				
//				// Normalize segment index if intPt falls on vertex
//				// The check for point equality is 2D only - Z values are ignored
//				if (intPt.Equals(nextPt))
//				{
//					normalizedSegmentIndex = nextSegIndex;
//					dist = 0.0;
//				}
//			}
//
//			//Add the intersection point to edge intersection list.
//			m_objNodeList.Add(intPt, normalizedSegmentIndex, dist);
//		}
        
        public static IList GetNodedSubstrings(IList segStrings)
        {
            IList resultEdgelist = new ArrayList();
            GetNodedSubstrings(segStrings, resultEdgelist);
            
            return resultEdgelist;
        }
		
        public static void GetNodedSubstrings(IList segStrings, 
            IList resultEdgelist)
        {
            for (IEnumerator i = segStrings.GetEnumerator(); i.MoveNext(); )
            {
                SegmentString ss = (SegmentString) i.Current;
                ss.NodeList.AddSplitEdges(resultEdgelist);
            }
        }

        #endregion
    }
}