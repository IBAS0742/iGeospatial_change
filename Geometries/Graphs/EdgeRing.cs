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
using System.Diagnostics;
using System.Collections;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Graphs
{
	[Serializable]
    internal abstract class EdgeRing
	{
		internal DirectedEdge startDe; // the directed edge which starts the list of edges for this EdgeRing
		private int maxNodeDegree = - 1;

        private ArrayList edges; // the DirectedEdges making up this EdgeRing

        private CoordinateCollection pts;

        private Label label; // label stores the locations of each geometry on the face surrounded by this ring
		private LinearRing m_objRing; // the ring created for this EdgeRing
		private bool m_bIsHole;
		private EdgeRing m_objShell; // if non-null, the ring is a hole and this EdgeRing is its containing shell

        private ArrayList holes; // a list of EdgeRings which are holes in this EdgeRing
		
		internal GeometryFactory geometryFactory;
		
        protected EdgeRing(DirectedEdge start, GeometryFactory geometryFactory)
        {
            edges = new ArrayList();
            pts   = new CoordinateCollection();
            label = new Label(LocationType.None);
            holes = new ArrayList();

            this.geometryFactory = geometryFactory;

            ComputePoints(start);
            ComputeRing();
        }
		
		public bool IsIsolated
		{
			get
			{
				return (label.GeometryCount == 1);
			}
		}

		public bool IsHole
		{
			get
			{
				//ComputePoints();
				return m_bIsHole;
			}
		}

		public LinearRing Ring
		{
			get
			{
				return m_objRing;
			}
		}

		public Label Label
		{
			get
			{
				return label;
			}
		}

		/// <summary> Returns the list of DirectedEdges that make up this EdgeRing</summary>
        public ArrayList Edges
		{
			get
			{
				return edges;
			}
		}

		public int MaxNodeDegree
		{
			get
			{
				if (maxNodeDegree < 0)
					ComputeMaxNodeDegree();
				return maxNodeDegree;
			}
		}

        public EdgeRing Shell
        {
            get 
            {
                return m_objShell;
            }

            set
            {
                m_objShell = value;
                if (m_objShell != null)
                    m_objShell.AddHole(this);
            }
        }
		
		public Coordinate GetCoordinate(int i)
		{
			return pts[i];
		}

		public bool IsShell
		{
            get 
            {
                return (m_objShell == null);
            }
		}

		public void  AddHole(EdgeRing ring)
		{
			holes.Add(ring);
		}
		
		public Polygon ToPolygon(GeometryFactory geometryFactory)
		{
			LinearRing[] holeLR = new LinearRing[holes.Count];

            for (int i = 0; i < holes.Count; i++)
			{
				holeLR[i] = ((EdgeRing) holes[i]).Ring;
			}

			Polygon poly = geometryFactory.CreatePolygon(m_objRing, holeLR);

			return poly;
		}

		/// <summary> 
		/// Compute a LinearRing from the point list previously collected.
		/// Test if the ring is a hole (i.e. if it is CCW) and set the hole flag
		/// accordingly.
		/// </summary>
		public void ComputeRing()
		{
			if (m_objRing != null)
				return; // don't compute more than once

			m_objRing = geometryFactory.CreateLinearRing(pts);
			m_bIsHole = CGAlgorithms.IsCCW(m_objRing.Coordinates);
		}

		public abstract DirectedEdge GetNext(DirectedEdge de);
		public abstract void  SetEdgeRing(DirectedEdge de, EdgeRing er);
		
		/// <summary> 
		/// Collect all the points from the DirectedEdges of this ring into a 
		/// contiguous list.
		/// </summary>
		protected void ComputePoints(DirectedEdge start)
		{
			startDe = start;
			DirectedEdge de = start;
			bool isFirstEdge = true;
			do 
			{
				Debug.Assert(de != null, "found null Directed Edge");
				if (de.EdgeRing == this)
				{
					throw new GeometryException("Directed Edge visited twice during ring-building at " + de.Coordinate);
				}
				
				edges.Add(de);

                Label label = de.Label;
				Debug.Assert(label.IsArea());
				MergeLabel(label);
				AddPoints(de.Edge, de.Forward, isFirstEdge);
				isFirstEdge = false;
				SetEdgeRing(de, this);
				de = GetNext(de);
			}
			while (de != startDe);
		}
		
		private void ComputeMaxNodeDegree()
		{
			maxNodeDegree = 0;
			DirectedEdge de = startDe;
			do 
			{
				Node node = de.Node;
				int degree = ((DirectedEdgeStar) node.Edges).GetOutgoingDegree(this);
				if (degree > maxNodeDegree)
					maxNodeDegree = degree;
				de = GetNext(de);
			}
			while (de != startDe);

			maxNodeDegree *= 2;
		}
		
		public void SetInResult()
		{
			DirectedEdge de = startDe;
			do 
			{
				de.Edge.InResult = true;
				de = de.Next;
			}
			while (de != startDe);
		}
		
		protected void MergeLabel(Label deLabel)
		{
			MergeLabel(deLabel, 0);
			MergeLabel(deLabel, 1);
		}

		/// <summary> Merge the RHS label from a DirectedEdge into the label for this EdgeRing.
		/// The DirectedEdge label may be null.  This is acceptable - it results
		/// from a node which is NOT an intersection node between the Geometries
		/// (e.g. the end node of a LinearRing).  In this case the DirectedEdge label
		/// does not contribute any information to the overall labelling, and is simply skipped.
		/// </summary>
		protected void MergeLabel(Label deLabel, int geomIndex)
		{
			int loc = deLabel.GetLocation(geomIndex, Position.Right);
			// no information to be had from this label
			if (loc == LocationType.None)
				return;

			// if there is no current RHS value, set it
			if (label.GetLocation(geomIndex) == LocationType.None)
			{
				label.SetLocation(geomIndex, loc);

				return;
			}
		}

		protected void AddPoints(Edge edge, bool isForward, bool isFirstEdge)
		{
			ICoordinateList edgePts = edge.Coordinates;
			if (isForward)
			{
				int startIndex = 1;
				if (isFirstEdge)
					startIndex = 0;

				for (int i = startIndex; i < edgePts.Count; i++)
				{
					pts.Add(edgePts[i]);
				}
			}
			else
			{
				// is backward
				int startIndex = edgePts.Count - 2;
				if (isFirstEdge)
					startIndex = edgePts.Count - 1;

				for (int i = startIndex; i >= 0; i--)
				{
					pts.Add(edgePts[i]);
				}
			}
		}
		
		/// <summary> 
		/// This method will cause the ring to be computed.
		/// It will also check any holes, if they have been assigned.
		/// </summary>
		public bool ContainsPoint(Coordinate p)
		{
			LinearRing shell = m_objRing;
			Envelope env     = shell.Bounds;

			if (!env.Contains(p))
				return false;

			if (!CGAlgorithms.IsPointInRing(p, shell.Coordinates))
				return false;
			
			for (IEnumerator i = holes.GetEnumerator(); i.MoveNext(); )
			{
				EdgeRing hole = (EdgeRing) i.Current;
				if (hole.ContainsPoint(p))
					return false;
			}

			return true;
		}
	}
}