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

namespace iGeospatial.Geometries.Graphs
{
	/// <summary>
	/// Models a directed edge incident on a node.
	/// </summary>
	[Serializable]
    internal class DirectedEdge : EdgeEnd
	{
        #region Private Fields
        
        internal bool m_bIsForward;
        private bool  m_bIsInResult;
        private bool  m_bIsVisited;
		
        /// <summary>
        /// The symmetric edge.
        /// </summary>
        private DirectedEdge sym; 

        /// <summary>
        /// The next edge in the edge ring for the polygon containing this edge.
        /// </summary>
        private DirectedEdge next; 

        /// <summary>
        /// The next edge in the MinimalEdgeRing that Contains this edge.
        /// </summary>
        private DirectedEdge nextMin; 

        /// <summary>
        /// The EdgeRing that this edge is part of.
        /// </summary>
        private EdgeRing edgeRing;
 
        /// <summary>
        /// The MinimalEdgeRing that this edge is part of.
        /// </summary>
        private EdgeRing minEdgeRing;

        /// <summary> 
        /// The depth of each side (position) of this edge.
        /// The 0 element of the array is never used.
        /// </summary>
        private int[] depth = new int[]{0, - 999, - 999};
		
        #endregion

        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedEdge"/> class.
        /// </summary>
        /// <param name="edge">The parent edge of this edge end.</param>
        /// <param name="isForward">
        /// Indicates whether the edge end is directed forward or backward.
        /// </param>
        public DirectedEdge(Edge edge, bool isForward) : base(edge)
        {
            this.m_bIsForward = isForward;

            if (isForward)
            {
                Initialize(edge.GetCoordinate(0), edge.GetCoordinate(1));
            }
            else
            {
                int n = edge.NumPoints - 1;
                Initialize(edge.GetCoordinate(n), edge.GetCoordinate(n - 1));
            }

            ComputeDirectedLabel();
        }
        
        #endregion

        #region Public Properties

        public override Edge Edge
		{
			get
			{
				return m_objEdge;
			}
		}
		
		public virtual bool InResult
		{
			get
			{
				return m_bIsInResult;
			}
			
			set
			{
				this.m_bIsInResult = value;
			}
		}
		
		public virtual bool Visited
		{
			get
			{
				return m_bIsVisited;
			}
			
			set
			{
				this.m_bIsVisited = value;
			}
		}
		
		public virtual EdgeRing EdgeRing
		{
			get
			{
				return edgeRing;
			}
			
			set
			{
				this.edgeRing = value;
			}
		}
		
		public virtual EdgeRing MinEdgeRing
		{
			get
			{
				return minEdgeRing;
			}
			
			set
			{
				this.minEdgeRing = value;
			}
		}
		
		public virtual int DepthDelta
		{
			get
			{
				int DepthDelta = m_objEdge.DepthDelta;
				if (!m_bIsForward)
					DepthDelta = - DepthDelta;

				return DepthDelta;
			}
		}
		
		/// <summary> 
		/// This marks both DirectedEdges attached to a given Edge.
		/// This is used for edges corresponding to lines, which will only
		/// appear oriented in a single direction in the result.
		/// </summary>
		public virtual bool VisitedEdge
		{
			set
			{
				Visited     = value;
				sym.Visited = value;
			}
		}
		
		/// <summary> 
		/// Each Edge gives rise to a pair of symmetric DirectedEdges, 
		/// in opposite directions.
		/// </summary>
		/// <value>
		/// The DirectedEdge for the same Edge but in the opposite direction.
		/// </value>
		public virtual DirectedEdge Sym
		{
			get
			{
				return sym;
			}
			
			set
			{
				sym = value;
			}
		}
		
		public virtual bool Forward
		{
			get
			{
				return m_bIsForward;
			}
		}
		
		public virtual DirectedEdge Next
		{
			get
			{
				return next;
			}
			
			set
			{
				this.next = value;
			}
		}

		public virtual DirectedEdge NextMin
		{
			get
			{
				return nextMin;
			}
			
			set
			{
				this.nextMin = value;
			}
		}
		
        /// <summary>
        /// Indicates whether or not this is a line edge. 
		/// </summary>
        /// <remarks>
        /// This edge is a line edge if
        /// <list type="number">
        /// <item>
        /// <description>
		/// at least one of the labels is a line label
        /// </description>
        /// </item>
        /// <item>
        /// <description>
		/// any labels which are not line labels have all Locations = EXTERIOR
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
		public virtual bool LineEdge
		{
			get
			{
				bool IsLine = m_objLabel.IsLine(0) || m_objLabel.IsLine(1);
				bool isExteriorIfArea0 = !m_objLabel.IsArea(0) || 
                    m_objLabel.IsAllPositionsEqual(0, LocationType.Exterior);
				bool isExteriorIfArea1 = !m_objLabel.IsArea(1) || 
                    m_objLabel.IsAllPositionsEqual(1, LocationType.Exterior);
				
				return IsLine && isExteriorIfArea0 && isExteriorIfArea1;
			}
		}

		/// <summary> 
		/// Determines whether or not this instance is an interior area edge.
		/// </summary>
		/// <value> true if this is an interior area edge</value>
		/// <remarks>
		/// This is an interior area edge if
		/// <list type="number">
		/// <item>
		/// <description>
		/// its label is an area label for both Geometries, and
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// for each Geometry both sides are in the interior.
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		public virtual bool InteriorAreaEdge
		{
			get
			{
				bool isInteriorAreaEdge = true;
				for (int i = 0; i < 2; i++)
				{
					if (!(m_objLabel.IsArea(i) && 
                        m_objLabel.GetLocation(i, Position.Left) == LocationType.Interior && 
                        m_objLabel.GetLocation(i, Position.Right) == LocationType.Interior))
					{
						isInteriorAreaEdge = false;
					}
				}
				return isInteriorAreaEdge;
			}
			
		}
        
        #endregion
		
        #region Public Methods

		public virtual int GetDepth(int position)
		{
			return depth[position];
		}
		
		public virtual void  SetDepth(int position, int depthVal)
		{
			if (depth[position] != -999)
			{
				if (depth[position] != depthVal)
					throw new GeometryException("assigned depths do not match", Coordinate);
			}

			depth[position] = depthVal;
		}
		
		/// <summary> 
		/// Compute the label in the appropriate orientation for this DirEdge
		/// </summary>
		private void  ComputeDirectedLabel()
		{
			m_objLabel = new Label(m_objEdge.Label);
			if (!m_bIsForward)
				m_objLabel.Flip();
		}
		
		/// <summary> 
		/// Set both edge depths.  One depth for a given side is provided.  The other is
		/// computed depending on the Location transition and the DepthDelta of the edge.
		/// </summary>
		public virtual void SetEdgeDepths(int position, int depth)
		{
			// get the depth transition delta from R to L for this directed Edge
			int DepthDelta = Edge.DepthDelta;
			if (!m_bIsForward)
				DepthDelta = - DepthDelta;
			
			// if moving from L to R instead of R to L must change sign of delta
			int directionFactor = 1;
			if (position == Position.Left)
				directionFactor = - 1;
			
			int oppositePos = Position.Opposite(position);
			int delta = DepthDelta * directionFactor;
			//TESTINGint delta = DepthDelta * DirectedEdge.DepthFactor(loc, oppositeLoc);
			int oppositeDepth = depth + delta;
			SetDepth(position, depth);
			SetDepth(oppositePos, oppositeDepth);
		}
        
        #endregion
		
        #region Public Static Methods

		/// <summary> 
		/// Computes the factor for the change in depth when moving from one 
		/// location to another.
		/// E.g. if crossing from the INTERIOR to the EXTERIOR the depth decreases, 
		/// so the factor is -1
		/// </summary>
		public static int DepthFactor(int currLocation, int nextLocation)
		{
			if (currLocation == LocationType.Exterior && 
                nextLocation == LocationType.Interior)
            {
                return 1;
            }
			else if (currLocation == LocationType.Interior && 
                nextLocation == LocationType.Exterior)
            {
                return -1;
            }

			return 0;
		}
        
        #endregion
	}
}