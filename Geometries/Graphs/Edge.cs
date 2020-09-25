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
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.Graphs.Index;

namespace iGeospatial.Geometries.Graphs
{
    [Serializable]
    internal class Edge : GraphComponent
	{
        #region Private Members
		
        internal ICoordinateList pts;
		private Envelope env;
		internal EdgeIntersectionList eiList;
		private string name;
		private MonotoneChainEdge mce;
		private bool m_bIsIsolated = true;
		private Depth depth;
		private int depthDelta; // the change in area depth from the R to L side of this edge
        
        #endregion
		
        #region Constructors and Destructor

        public Edge(ICoordinateList pts, Label label)
        {
            eiList = new EdgeIntersectionList(this);
            depth  = new Depth();
            this.pts = pts;
            this.m_objLabel = label;
        }

        public Edge(ICoordinateList pts) : this(pts, null)
        {
        }
        
        #endregion
		
        #region Public Properties

		public int NumPoints
		{
			get
			{
				return pts.Count;
			}
		}

		public string Name
		{
            get
            {
                return this.name;
            }

			set
			{
				this.name = value;
			}
		}

		public ICoordinateList Coordinates
		{
			get
			{
				return pts;
			}
		}

		public Envelope Envelope
		{
			get
			{
				// compute envelope lazily
				if (env == null)
				{
					env = new Envelope();
					for (int i = 0; i < pts.Count; i++)
					{
						env.ExpandToInclude(pts[i]);
					}
				}

				return env;
			}
		}

		public Depth Depth
		{
			get
			{
				return depth;
			}
		}

		/// <summary> 
		/// The DepthDelta is the change in depth as an edge is crossed from R to L
		/// </summary>
		/// <value> the change in depth as the edge is crossed from R to L </value>
		public int DepthDelta
		{
			get
			{
				return depthDelta;
			}
			
			set
			{
				this.depthDelta = value;
			}
		}

		public int MaximumSegmentIndex
		{
			get
			{
				return (pts.Count - 1);
			}
		}

		public EdgeIntersectionList EdgeIntersectionList
		{
			get
			{
				return eiList;
			}
		}

		public MonotoneChainEdge MonotoneChainEdge
		{
			get
			{
				if (mce == null)
					mce = new MonotoneChainEdge(this);

				return mce;
			}
		}

		public bool IsClosed
		{
			get
			{
				return pts[0].Equals(pts[pts.Count - 1]);
			}
		}

		/// <summary> 
		/// An Edge is collapsed if it is an Area edge and it consists of
		/// two segments which are equal and opposite (eg a zero-width V).
		/// </summary>
		public bool IsCollapsed
		{
			get
			{
				if (!m_objLabel.IsArea())
					return false;
				
                if (pts.Count != 3)
					return false;

				if (pts[0].Equals(pts[2]))
					return true;

				return false;
			}
		}

		public Edge CollapsedEdge
		{
			get
			{
				CoordinateCollection newPts = new CoordinateCollection(2);
				newPts.Add(pts[0]);
				newPts.Add(pts[1]);

				Edge newe = new Edge(newPts, Label.ToLineLabel(m_objLabel));

				return newe;
			}
		}

        public override Coordinate Coordinate
        {
            get
            {
                if (pts.Count > 0)
                    return pts[0];

                return null;
            }
        }
		
        public override bool Isolated
        {
            get 
            {
                return m_bIsIsolated;
            }

            set 
            {
                this.m_bIsIsolated = value;
            }
        }
        
        #endregion
		
        #region Public Methods

		public Coordinate GetCoordinate(int i)
		{
			return pts[i];
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

		/// <summary> Add an EdgeIntersection for intersection intIndex.
		/// An intersection that falls exactly on a vertex of the edge is normalized
		/// to use the higher of the two possible segmentIndexes
		/// </summary>
		public void AddIntersection(LineIntersector li, int segmentIndex, int geomIndex, int intIndex)
		{
			Coordinate intPt = new Coordinate(li.GetIntersection(intIndex));
			int normalizedSegmentIndex = segmentIndex;
			double dist = li.GetEdgeDistance(geomIndex, intIndex);
			// Normalize the intersection point location
			int nextSegIndex = normalizedSegmentIndex + 1;
			if (nextSegIndex < pts.Count)
			{
				Coordinate nextPt = pts[nextSegIndex];
				
				// Normalize segment index if intPt falls on vertex
				// The check for point equality is 2D only - Z values are ignored
				if (intPt.Equals(nextPt))
				{
					normalizedSegmentIndex = nextSegIndex;
					dist = 0.0;
				}
			}

			// Add the intersection point to edge intersection list.
//			EdgeIntersection ei = eiList.Add(intPt, normalizedSegmentIndex, dist);
			eiList.Add(intPt, normalizedSegmentIndex, dist);
		}
		
		/// <summary> Update the IM with the contribution for this component.
		/// A component only contributes if it has a labelling for both parent geometries
		/// </summary>
		public override void ComputeIM(IntersectionMatrix im)
		{
			UpdateIM(m_objLabel, im);
		}
		
		/// <summary> 
		/// Determines whether the specified <see cref="object"/> is equal to 
		/// the current <see cref="Edge"/>.
		/// </summary>
		/// <remarks>
		/// The two objects are equal if the coordinates of specified object are the 
		/// same or the reverse of the coordinates in the current object.
		/// </remarks>
		public override bool Equals(object o)
		{
            Edge e = o as Edge;
			if (e == null)
				return false;
			
			if (pts.Count != e.pts.Count)
				return false;
			
			bool isEqualForward = true;
			bool isEqualReverse = true;
			int iRev   = pts.Count;
            int nCount = iRev;
			
            for (int i = 0; i < nCount; i++)
			{
				if (!pts[i].Equals(e.pts[i]))
				{
					isEqualForward = false;
				}
				
                if (!pts[i].Equals(e.pts[--iRev]))
				{
					isEqualReverse = false;
				}

				if (!isEqualForward && !isEqualReverse)
					return false;
			}

			return true;
		}
		
		/// <summary> 
		/// Determines whether the specified <see cref="object"/> is equal to 
		/// the current <see cref="Edge"/>.
		/// </summary>
		/// <remarks>
		/// The two objects are equal if the coordinates of specified object are the 
		/// same or the reverse of the coordinates in the current object.
		/// </remarks>
		public bool Equals(Edge e)
		{
			if (e == null)
				return false;
			
			if (pts.Count != e.pts.Count)
				return false;
			
			bool isEqualForward = true;
			bool isEqualReverse = true;
			int iRev   = pts.Count;
			int nCount = iRev;

            for (int i = 0; i < nCount; i++)
			{
				if (!pts[i].Equals(e.pts[i]))
				{
					isEqualForward = false;
				}

				if (!pts[i].Equals(e.pts[--iRev]))
				{
					isEqualReverse = false;
				}

				if (!isEqualForward && !isEqualReverse)
					return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
        /// <summary>
        /// Determines whether the coordinate sequences of the specified Edge and this
        /// instance are identical.
        /// </summary>
        /// <param name="e">
        /// The <see cref="Edge"/> to compare with the current. <see cref="Edge"/>.
        /// </param>
        /// <returns>
        /// true if the coordinate sequences of the Edges are identical.
        /// </returns>
        public bool IsPointwiseEqual(Edge e)
		{
			if (pts.Count != e.pts.Count)
				return false;
			
            int nCount = pts.Count;
			for (int i = 0; i < nCount; i++)
			{
				if (!pts[i].Equals(e.pts[i]))
				{
					return false;
				}
			}

			return true;
		}
        
        #endregion

        #region Public Static Methods

		/// <summary> Updates an IM from the label for an edge.
		/// Handles edges from both L and A geometries.
		/// </summary>
		public static void UpdateIM(Label label, IntersectionMatrix im)
		{
			im.SetAtLeastIfValid(label.GetLocation(0, Position.On), 
                label.GetLocation(1, Position.On), 1);

			if (label.IsArea())
			{
				im.SetAtLeastIfValid(label.GetLocation(0, Position.Left), 
                    label.GetLocation(1, Position.Left), 2);

				im.SetAtLeastIfValid(label.GetLocation(0, Position.Right), 
                    label.GetLocation(1, Position.Right), 2);
			}
		}
        
        #endregion
	}
}