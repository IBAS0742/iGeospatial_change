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
using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Operations.Relate
{
	/// <summary> 
	/// Contains all <see cref="EdgeEnd"/>s which start at the same point and are parallel.
	/// </summary>
	internal class EdgeEndBundle : EdgeEnd
	{
        #region Private Fields

		private ArrayList edgeEnds;
        
        #endregion
		
        #region Constructors and Destructor

        public EdgeEndBundle(EdgeEnd e) 
            : base(e.Edge, e.Coordinate, e.DirectedCoordinate, new Label(e.Label))
        {
            edgeEnds = new ArrayList();

            Insert(e);
        }

        #endregion

        #region Public Properties

        public override Label Label
		{
			get
			{
				return m_objLabel;
			}
		}

		public ArrayList EdgeEnds
		{
			get
			{
				return edgeEnds;
			}
		}
        
        #endregion
		
        #region Public Methods

		public IEnumerator Iterator()
		{
			return edgeEnds.GetEnumerator();
		}
		
		public void Insert(EdgeEnd e)
		{
			edgeEnds.Add(e);
		}
                                
		/// <summary> This computes the overall edge label for the set of
		/// edges in this EdgeEndBundle.  It essentially merges
		/// the ON and side labels for each edge.  These labels must be compatible
		/// </summary>
		public override void ComputeLabel()
		{
			// create the label.  If any of the edges belong to areas,
			// the label must be an area label
			bool IsArea = false;

			for (IEnumerator it = Iterator(); it.MoveNext(); )
			{
				EdgeEnd e = (EdgeEnd) it.Current;
				if (e.Label.IsArea())
					IsArea = true;
			}
			if (IsArea)
				m_objLabel = new Label(LocationType.None, LocationType.None, LocationType.None);
			else
				m_objLabel = new Label(LocationType.None);
			
			// compute the On label, and the side labels if present
			for (int i = 0; i < 2; i++)
			{
				ComputeLabelOn(i);
				if (IsArea)
					ComputeLabelSides(i);
			}
		}
		
		/// <summary> 
		/// Update the IM with the contribution for the computed label 
		/// for the EdgeStubs.
		/// </summary>
		public void UpdateIM(IntersectionMatrix im)
		{
			Edge.UpdateIM(m_objLabel, im);
		}
        
        #endregion
        
        #region Private Methods

		/// <summary> 
		/// Compute the overall ON location for the list of EdgeEndBundle.
		/// </summary>
		/// <remarks>
		/// This is essentially equivalent to computing the self-overlay of 
		/// a single Geometry.
		/// <para>
		/// EdgeEndBundle can be either on the boundary (eg Polygon edge)
		/// OR in the interior (e.g. segment of a LineString)
		/// of their parent Geometry.
		/// In addition, GeometryCollections use the mod-2 rule to determine
		/// whether a segment is on the boundary or not.
		/// Finally, in GeometryCollections it can still occur that an edge is both
		/// on the boundary and in the interior (e.g. a LineString segment lying on
		/// top of a Polygon edge.) In this case as usual the Boundary is given precendence.
		/// </para>
		/// These observations result in the following rules for computing the ON location:
		/// <list type="bullet">
		/// <item>
		/// <description>
		/// if there are an odd number of boundary edges, the attribute is boundary.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// if there are an even number >= 2 of boundary edges, the attribute is interior.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// if there are any interior edges, the attribute is interior.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// otherwise, the attribute is NULL.
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		private void ComputeLabelOn(int geomIndex)
		{
			// compute the ON location value
			int boundaryCount = 0;
			bool foundInterior = false;
			
			for (IEnumerator it = Iterator(); it.MoveNext(); )
			{
				EdgeEnd e = (EdgeEnd) it.Current;
				int loc = e.Label.GetLocation(geomIndex);
				if (loc == LocationType.Boundary)
					boundaryCount++;
				if (loc == LocationType.Interior)
					foundInterior = true;
			}
			int loc2 = LocationType.None;
			if (foundInterior)
				loc2 = LocationType.Interior;
			if (boundaryCount > 0)
			{
				loc2 = GeometryGraph.DetermineBoundary(boundaryCount);
			}
			m_objLabel.SetLocation(geomIndex, loc2);
		}

		/// <summary> Compute the labelling for each side</summary>
		private void ComputeLabelSides(int geomIndex)
		{
			ComputeLabelSide(geomIndex, Position.Left);
			ComputeLabelSide(geomIndex, Position.Right);
		}
		
		/// <summary> 
		/// Computes the label for a side.
		/// </summary>
		/// <remarks>
		/// To compute the summary label for a side, the algorithm is:
		/// <code>
		/// FOR all edges
		/// IF any edge's location is INTERIOR for the side, side location = INTERIOR
		/// ELSE IF there is at least one EXTERIOR attribute, side location = EXTERIOR
		/// ELSE  side location = NULL
		/// </code>
		/// <para>
		/// Note that it is possible for two sides to have apparently contradictory information
		/// i.e. one edge side may indicate that it is in the interior of a geometry, while
		/// another edge side may indicate the exterior of the same geometry.  This is
		/// not an incompatibility - GeometryCollections may contain two Polygons that touch
		/// along an edge.  This is the reason for Interior-primacy rule above - it
		/// results in the summary label having the Geometry interior on both sides.
		/// </para>
		/// </remarks>
		private void ComputeLabelSide(int geomIndex, int side)
		{
			for (IEnumerator it = Iterator(); it.MoveNext(); )
			{
				EdgeEnd e = (EdgeEnd) it.Current;
				if (e.Label.IsArea())
				{
					int loc = e.Label.GetLocation(geomIndex, side);
					if (loc == LocationType.Interior)
					{
						m_objLabel.SetLocation(geomIndex, side, LocationType.Interior);
						return ;
					}
					else if (loc == LocationType.Exterior)
						m_objLabel.SetLocation(geomIndex, side, LocationType.Exterior);
				}
			}
		}
        
        #endregion
    }
}