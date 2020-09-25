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
using iGeospatial.Collections;
using iGeospatial.Collections.Sets;

using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.Graphs.Index;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Tests whether a <see cref="Geometry"/> is simple.
	/// </summary>
	/// <remarks>
	/// Only <see cref="Geometry"/> instances whose definition allows them
	/// to be simple or non-simple are tested. For instance, 
	/// <see cref="Polygon"/>s must be simple by definition, so no test 
	/// is provided. 
	/// <para>
	/// To test whether a given Polygon is valid, use the 
	/// <see cref="Geometry.IsValid"/>.
	/// </para>
	/// </remarks>
	public sealed class IsSimpleOp
	{
        #region Constructors and Destructor
		
        /// <summary>
        /// Initializes a new instance of the <see cref="IsSimpleOp"/> class.
        /// </summary>
        public IsSimpleOp()
		{
		}
        
        #endregion
		
        #region Public Methods

        /// <overloads>
        /// Determines whether the arguement geometry is simple or not.
        /// </overloads>
        /// <summary>
        /// Determines whether a <see cref="LineString"/> geometry is simple.
        /// </summary>
        /// <param name="geometry">
        /// A <see cref="LineString"/> geometry instance to be tested.
        /// </param>
        /// <returns>
        /// Returns true if the <see cref="LineString"/> geometry is simple,
        /// otherwise returns false.
        /// </returns>
		public bool IsSimple(LineString geometry)
		{
			return IsSimpleLinearGeometry(geometry);
		}
 
        public static bool Simple(LineString geometry)
        {
            IsSimpleOp isSimpleTester = new IsSimpleOp();

            return isSimpleTester.IsSimple(geometry);
        }
		
        /// <summary>
        /// Determines whether a <see cref="MultiLineString"/> geometry is simple.
        /// </summary>
        /// <param name="geometry">
        /// A <see cref="MultiLineString"/> geometry instance to be tested.
        /// </param>
        /// <returns>
        /// Returns true if the <see cref="MultiLineString"/> geometry is simple,
        /// otherwise returns false.
        /// </returns>
		public bool IsSimple(MultiLineString geometry)
		{
			return IsSimpleLinearGeometry(geometry);
		}
 
        public static bool Simple(MultiLineString geometry)
        {
            IsSimpleOp isSimpleTester = new IsSimpleOp();

            return isSimpleTester.IsSimple(geometry);
        }
		
        /// <summary>
        /// Determines whether a <see cref="MultiPoint"/> geometry is simple.
        /// </summary>
        /// <param name="multiPoints">
        /// A <see cref="MultiPoint"/> geometry instance to be tested.
        /// </param>
        /// <returns>
        /// Returns true if the <see cref="MultiPoint"/> geometry is simple,
        /// otherwise returns false.
        /// </returns>
        /// <remarks>
        /// A <see cref="MultiPoint"/> is simple if and only if it has 
        /// no repeated points.
        /// </remarks>
        public bool IsSimple(MultiPoint multiPoints)
		{
            if (multiPoints == null)
            {
                throw new ArgumentNullException("multiPoints");
            }

            if (multiPoints.IsEmpty)
				return true;

			ISet points = new HashedSet();
			
            int nCount = multiPoints.NumGeometries;
            for (int i = 0; i < nCount; i++)
			{
				Point pt = (Point) multiPoints.GetGeometry(i);
				Coordinate p = pt.Coordinate;
				if (points.Contains(p))
					return false;
				
                points.Add(p);
			}

			return true;
		}
 
        public static bool Simple(MultiPoint multiPoints)
        {
            IsSimpleOp isSimpleTester = new IsSimpleOp();

            return isSimpleTester.IsSimple(multiPoints);
        }

        #endregion
		
        #region Private Methods

		private bool IsSimpleLinearGeometry(LineString geom)
		{
			if (geom.IsEmpty)
				return true;

			GeometryGraph graph = new GeometryGraph(0, geom);
			LineIntersector li  = new RobustLineIntersector();
			SegmentIntersector si = graph.ComputeSelfNodes(li, true);
			// if no self-intersection, must be simple
			if (!si.HasIntersection)
				return true;
			if (si.HasProperIntersection())
				return false;
			if (HasNonEndpointIntersection(graph))
				return false;
			if (HasClosedEndpointIntersection(graph))
				return false;

			return true;
		}

		private bool IsSimpleLinearGeometry(MultiLineString geom)
		{
			if (geom.IsEmpty)
				return true;

			GeometryGraph graph = new GeometryGraph(0, geom);
			LineIntersector li  = new RobustLineIntersector();
			SegmentIntersector si = graph.ComputeSelfNodes(li, true);
			// if no self-intersection, must be simple
			if (!si.HasIntersection)
				return true;
			if (si.HasProperIntersection())
				return false;
			if (HasNonEndpointIntersection(graph))
				return false;
			if (HasClosedEndpointIntersection(graph))
				return false;

			return true;
		}
		
		/// <summary> 
		/// For all edges, check if there are any intersections which are NOT at an endpoint.
		/// The Geometry is not simple if there are intersections not at endpoints.
		/// </summary>
		private bool HasNonEndpointIntersection(GeometryGraph graph)
		{
			for (IEdgeEnumerator i = graph.EdgeIterator; i.MoveNext(); )
			{
				Edge e = i.Current;
				int maxSegmentIndex = e.MaximumSegmentIndex;

                for (IEnumerator eiIt = e.EdgeIntersectionList.Iterator(); eiIt.MoveNext(); )
				{
					EdgeIntersection ei = (EdgeIntersection) eiIt.Current;
					if (!ei.IsEndPoint(maxSegmentIndex))
						return true;
				}
			}
			return false;
		}
		
		/// <summary> 
		/// Test that no edge intersection is the endpoint of a closed line.  
		/// To check this we compute the degree of each endpoint. 
		/// The degree of endpoints of closed lines must be exactly 2.
		/// </summary>
		private bool HasClosedEndpointIntersection(GeometryGraph graph)
		{
			IDictionary endPoints = new SortedList();
			for (IEdgeEnumerator i = graph.EdgeIterator; i.MoveNext(); )
			{
				Edge e = i.Current;
//				int maxSegmentIndex = e.MaximumSegmentIndex;
				bool isClosed = e.IsClosed;
				Coordinate p0 = e.GetCoordinate(0);
				AddEndpoint(endPoints, p0, isClosed);
				Coordinate p1 = e.GetCoordinate(e.NumPoints - 1);
				AddEndpoint(endPoints, p1, isClosed);
			}
			
			for (IEnumerator i = endPoints.Values.GetEnumerator(); i.MoveNext(); )
			{
				EndpointInfo eiInfo = (EndpointInfo) i.Current;
				if (eiInfo.isClosed && eiInfo.degree != 2)
					return true;
			}
			return false;
		}
		
		/// <summary> 
		/// Add an endpoint to the map, creating an entry for it if none exists.
		/// </summary>
		private void  AddEndpoint(IDictionary endPoints, Coordinate p, bool isClosed)
		{
			EndpointInfo eiInfo = (EndpointInfo) endPoints[p];
			if (eiInfo == null)
			{
				eiInfo = new EndpointInfo(this, p);
//				object tempObject;
//				tempObject   = eiInfo;
//				endPoints[p] = tempObject;
//				object generatedAux = tempObject;
				endPoints[p] = eiInfo;
			}

			eiInfo.AddEndpoint(isClosed);
		}
        
        #endregion
		
        #region EndpointInfo Class

        internal sealed class EndpointInfo
        {
            private IsSimpleOp m_objIsSimpleOp;

            internal Coordinate pt;
            internal bool isClosed;
            internal int degree;
			
            public IsSimpleOp SimpleOp
            {
                get
                {
                    return m_objIsSimpleOp;
                }
            }
			
            public EndpointInfo(IsSimpleOp isSimpleOp, Coordinate pt)
            {
                this.m_objIsSimpleOp = isSimpleOp;

                this.pt  = pt;
            }
			
            public void  AddEndpoint(bool isClosed)
            {
                degree++;

                this.isClosed |= isClosed;
            }
        }
        
        #endregion
	}
}