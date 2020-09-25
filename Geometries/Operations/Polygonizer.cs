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
using iGeospatial.Geometries.Visitors;
using iGeospatial.Geometries.Operations.Polygonize;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Polygonizes a set of <see cref="Geometry"/> instances which contain linework that
	/// represents the edges of a planar graph.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Any dimension of Geometry is handled - the constituent linework is extracted
	/// to form the edges.
	/// The edges must be correctly noded; that is, they must only meet
	/// at their endpoints.  The Polygonizer will still run on incorrectly noded input
	/// but will not form polygons from incorrected noded edges.
	/// </para>
	/// The Polygonizer reports the follow kinds of errors:
	/// <list type="number">
	/// <item>
	/// <description>
	/// Dangles - edges which have one or both ends which are not incident on 
	/// another edge endpoint.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Cut Edges - edges which are connected at both ends but which do not form 
	/// part of polygon
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Invalid Ring Lines - edges which form rings which are invalid
	/// (e.g. the component lines contain a self-intersection)
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	public sealed class Polygonizer
	{
        #region Private Members
		
        // default factory
		private LineStringAdder lineStringAdder;

        #endregion
		
        #region Internal Members

		internal PolygonizeGraph graph;

        // initialize with empty collections, in case nothing is computed
		internal ICollection  m_arrDangles;
		internal ArrayList    m_arrCutEdges;
		internal GeometryList m_arrInvalidRingLines;
		
		internal ArrayList holeList;
		internal ArrayList shellList;
		internal GeometryList polyList;
		
        #endregion

        #region Constructors and Destructor
		
        /// <summary> 
		/// Create a polygonizer with the same <see cref="GeometryFactory"/> as 
		/// the input geometries.
		/// </summary>
		public Polygonizer()
		{
            lineStringAdder       = new LineStringAdder(this);
            m_arrDangles          = new ArrayList();
            m_arrCutEdges         = new ArrayList();
            m_arrInvalidRingLines = new GeometryList();
        }

        #endregion

        #region Public Properties

		/// <summary> 
		/// Gets the list of polygons formed by the polygonization.
		/// </summary>
		/// <value> A collection of <see cref="Polygons"/>. </value>
		public IGeometryList Polygons
		{
			get
			{
				Polygonize();

				return polyList;
			}
		}
		
        /// <summary> 
        /// Get the list of dangling lines found during polygonization.
        /// </summary>
		/// <value> 
		/// A collection of the input <see cref="LineString"/>s which are dangles.
		/// </value>
		public ICollection Dangles
		{
			get
			{
				Polygonize();

				return m_arrDangles;
			}
		}

		/// <summary> 
		/// Get the list of cut edges found during polygonization.
		/// </summary>
		/// <value> 
		/// A collection of the input <see cref="LineString"/>s which are cut edges.
		/// </value>
		public ICollection CutEdges
		{
			get
			{
				Polygonize();

				return m_arrCutEdges;
			}
		}

		/// <summary> 
		/// Get the list of lines forming invalid rings found during polygonization.
		/// </summary>
		/// <value> 
		/// A collection of the input <see cref="LineString"/>s which form invalid rings.
		/// </value>
		public IGeometryList InvalidRingLines
		{
			get
			{
				Polygonize();

				return m_arrInvalidRingLines;
			}
		}
		
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// Add a collection of geometries to be polygonized. May be called multiple times.
		/// Any dimension of Geometry may be added; the constituent linework 
		/// will be extracted and used
		/// </summary>
		/// <param name="geometryList">
		/// A list of Geometry instances with linework to be polygonized.
		/// </param>
		public void Add(IGeometryList geometryList)
		{
            if (geometryList == null)
            {
                throw new ArgumentNullException("geometryList");
            }

            int nCount = geometryList.Count;
            for (int i = 0; i < nCount; i++)
            {
                Add(geometryList[i]);
            }

//			for (IEnumerator i = geomList.GetEnumerator(); i.MoveNext(); )
//			{
//				Geometry geometry = (Geometry) i.Current;
//				Add(geometry);
//			}
		}
		
		/// <summary> 
		/// Add a geometry to the linework to be polygonized. May be called multiple times.
		/// Any dimension of Geometry may be added; the constituent linework 
		/// will be extracted and used
		/// </summary>
		/// <param name="g">A Geometry with linework to be polygonized. </param>
		public void Add(Geometry g)
		{
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }

            g.Apply(lineStringAdder);
		}

		/// <summary> 
		/// Add a linestring to the graph of polygon edges.
		/// </summary>
		/// <param name="line">
		/// The <see cref="LineString"/> to add to the list.
		/// </param>
		public void Add(LineString line)
		{
			// create a new graph using the factory from the input Geometry
			if (graph == null)
				graph = new PolygonizeGraph(line.Factory);

			graph.AddEdge(line);
		}
		
		/// <summary> 
		/// Perform the polygonization, if it has not already been carried out.
		/// </summary>
		public void Polygonize()
		{
			// check if already computed
			if (polyList != null)
				return;
			
//?            polyList = new ArrayList();
			
            m_arrDangles  = graph.DeleteDangles();
			m_arrCutEdges = graph.DeleteCutEdges();
			ArrayList edgeRingList = graph.EdgeRings;
			
			ArrayList validEdgeRingList = new ArrayList();
			m_arrInvalidRingLines = new GeometryList();
			FindValidRings(edgeRingList, validEdgeRingList, m_arrInvalidRingLines);
			
			FindShellsAndHoles(validEdgeRingList);
			AssignHolesToShells(holeList, shellList);
			
			polyList = new GeometryList();

			for (IEnumerator i = shellList.GetEnumerator(); i.MoveNext(); )
			{
				EdgeRing er = (EdgeRing) i.Current;

                polyList.Add(er.Polygon);
			}
		}
        
        #endregion
		
        #region Private Methods
		
		private void  FindValidRings(ArrayList edgeRingList, 
            ArrayList validEdgeRingList, GeometryList invalidRingList)
		{
			for (IEnumerator i = edgeRingList.GetEnumerator(); i.MoveNext(); )
			{
				EdgeRing er = (EdgeRing) i.Current;
				if (er.IsValid)
				{
					validEdgeRingList.Add(er);
				}
				else
				{
					invalidRingList.Add(er.LineString);
				}
			}
		}
		
		private void FindShellsAndHoles(ArrayList edgeRingList)
		{
			holeList = new ArrayList();
			shellList = new ArrayList();
			for (IEnumerator i = edgeRingList.GetEnumerator(); i.MoveNext(); )
			{
				EdgeRing er = (EdgeRing) i.Current;
				if (er.Hole)
				{
					holeList.Add(er);
				}
				else
				{
					shellList.Add(er);
				}
			}
		}
		
		private static void AssignHolesToShells(ArrayList holeList, ArrayList shellList)
		{
			for (IEnumerator i = holeList.GetEnumerator(); i.MoveNext(); )
			{
				EdgeRing holeER = (EdgeRing) i.Current;
				AssignHoleToShell(holeER, shellList);
			}
		}
		
		private static void AssignHoleToShell(EdgeRing holeER, ArrayList shellList)
		{
			EdgeRing shell = EdgeRing.FindEdgeRingContaining(holeER, shellList);
			if (shell != null)
				shell.AddHole(holeER.Ring);
		}
        
        #endregion

        #region LineStringAdder Class

		/// <summary> 
		/// Add every linear element in a geometry into the polygonizer graph.
		/// </summary>
		private sealed class LineStringAdder : IGeometryComponentVisitor
		{
            private Polygonizer m_objPolygonizer;
			
			public LineStringAdder(Polygonizer polygonizer)
			{
                this.m_objPolygonizer = polygonizer;
			}

            public Polygonizer Polygonizer
			{
				get
				{
					return m_objPolygonizer;
				}
			}

			public void Visit(Geometry g)
			{
                if (g == null)
                {
                    throw new ArgumentNullException("g");
                }

                GeometryType geomType = g.GeometryType;

                if (geomType == GeometryType.LineString ||
                    geomType == GeometryType.LinearRing)
                {
                    m_objPolygonizer.Add((LineString) g);
                }
			}
		}
        
        #endregion
	}
}