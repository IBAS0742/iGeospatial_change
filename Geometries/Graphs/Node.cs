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

namespace iGeospatial.Geometries.Graphs
{
    [Serializable]
    internal class Node : GraphComponent
	{
        #region Private Fields

		private Coordinate coord; // only non-null if this node is precise
		
        private EdgeEndStar m_objEdges;

        #endregion

        #region Constructors and Destructor

        public Node(Coordinate coord, EdgeEndStar edges)
        {
            this.coord = coord;
            this.m_objEdges = edges;
            m_objLabel = new Label(0, LocationType.None);
        }

        #endregion
		
        #region Public Properties

		public EdgeEndStar Edges
		{
			get
			{
				return m_objEdges;
			}
		}
		
		public override Coordinate Coordinate
		{
            get
            {
                return coord;
            }
		}
		
        public override bool Isolated
        {
            get 
            {
                return (m_objLabel.GeometryCount == 1);
            }

            set 
            {
                // does nothing...
            }
        }

        /// <summary> Tests whether any incident edge is flagged as
        /// being in the result.
        /// This test can be used to determine if the node is in the result,
        /// since if any incident edge is in the result, the node must be in the result as well.
        /// 
        /// </summary>
        /// <returns> <see langword="true"/> if any indicident edge in the in the result
        /// </returns>
        public bool IsIncidentEdgeInResult
        {
            get
            {
                for (IEnumerator it = this.Edges.Edges.GetEnumerator(); 
                    it.MoveNext(); )
                {
                    DirectedEdge de = (DirectedEdge) it.Current;
                    if (de.Edge.InResult)
                        return true;
                }
                return false;
            }
        }
        
        #endregion

        #region Public Methods

		/// <summary> Updates the label of a node to BOUNDARY,
		/// obeying the mod-2 boundaryDetermination rule.
		/// </summary>
		public void SetLabelBoundary(int argIndex)
		{
			// determine the current location for the point (if any)
			int loc = LocationType.None;
			if (m_objLabel != null)
				loc = m_objLabel.GetLocation(argIndex);

			// flip the loc
			int newLoc;
			switch (loc)
			{
				case LocationType.Boundary:  
                    newLoc = LocationType.Interior; 
                    break;
				
				case LocationType.Interior:  
                    newLoc = LocationType.Boundary; 
                    break;
				
				default:  
                    newLoc = LocationType.Boundary; 
                    break;
				
			}

			m_objLabel.SetLocation(argIndex, newLoc);
		}

		/// <summary> Basic nodes do not compute IMs</summary>
		public override void ComputeIM(IntersectionMatrix im)
		{
		}

		/// <summary> Add the edge to the list of edges at this node</summary>
		public void Add(EdgeEnd e)
		{
			// Assert: start pt of e is equal to node point
			m_objEdges.Insert(e);
			e.Node = this;
		}
		
		public void MergeLabel(Node n)
		{
			MergeLabel(n.m_objLabel);
		}
		
		/// <summary> To merge labels for two nodes,
		/// the merged location for each LabelElement is computed.
		/// The location for the corresponding node LabelElement is set to the result,
		/// as long as the location is non-null.
		/// </summary>
		
		public void MergeLabel(Label label2)
		{
			for (int i = 0; i < 2; i++)
			{
				int loc = ComputeMergedLocation(label2, i);
				int thisLoc = m_objLabel.GetLocation(i);
				if (thisLoc == LocationType.None)
					m_objLabel.SetLocation(i, loc);
			}
		}
		
		public void SetLabel(int argIndex, int onLocation)
		{
			if (m_objLabel == null)
			{
				m_objLabel = new Label(argIndex, onLocation);
			}
			else
            {
                m_objLabel.SetLocation(argIndex, onLocation);
            }
		}
        
        #endregion
		
        #region Private Methods

		/// <summary> The location for a given eltIndex for a node will be one
		/// of { null, INTERIOR, BOUNDARY }.
		/// A node may be on both the boundary and the interior of a geometry;
		/// in this case, the rule is that the node is considered to be in the boundary.
		/// The merged location is the maximum of the two input values.
		/// </summary>
		private int ComputeMergedLocation(Label label2, int eltIndex)
		{
			int loc = LocationType.None;
			loc     = m_objLabel.GetLocation(eltIndex);
			if (!label2.IsNull(eltIndex))
			{
				int nLoc = label2.GetLocation(eltIndex);
				if (loc != LocationType.Boundary)
					loc = nLoc;
			}
			return loc;
		}
        
        #endregion
	}
}