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

using iGeospatial.Exceptions;
using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.PlanarGraphs;

namespace iGeospatial.Geometries.Operations.Polygonize
{
	/// <summary> 
	/// Represents a ring of <see cref="PolygonizeDirectedEdge"/>s which form
	/// a ring of a polygon.  The ring may be either an outer shell or a hole.
	/// </summary>
	internal class EdgeRing
	{
        #region Private Members

		private GeometryFactory factory;
		
		private ArrayList deList;
		
		// cache the following data for efficiency
		private LinearRing ring;
		
		private ICoordinateList ringPts;
		private GeometryList holes;
		
        #endregion

        #region Constructors and Destructor

		public EdgeRing(GeometryFactory factory)
		{
            deList = new ArrayList();

			this.factory = factory;
		}

        #endregion
		
        #region Public Properties

		/// <summary> Tests whether this ring is a hole.
		/// Due to the way the edges in the polyongization graph are linked,
		/// a ring is a hole if it is oriented counter-clockwise.
		/// </summary>
		/// <returns> true if this ring is a hole
		/// </returns>
		public bool Hole
		{
			get
			{
				LinearRing ring = Ring;
				return CGAlgorithms.IsCCW(ring.Coordinates);
			}
		}

		/// <summary> 
		/// Computes the {@link Polygon formed by this ring and any contained holes.
		/// 
		/// </summary>
		/// <returns> the Polygon formed by this ring and its holes.
		/// </returns>
		public Polygon Polygon
		{
			get
			{
				LinearRing[] holeLR = null;
				if (holes != null)
				{
                    holeLR = holes.ToLinearRingArray();
				}

				Polygon poly = factory.CreatePolygon(ring, holeLR);
				return poly;
			}
		}

		/// <summary> Tests if the {@link LinearRing} ring formed by this edge ring is topologically valid.</summary>
		/// <returns>
		/// </returns>
		public bool IsValid
		{
			get
			{
				ringPts = this.Coordinates;
				if (ringPts.Count <= 3)
					return false;
				
//                LinearRing generatedAux = Ring;
				if (ring == null)
				{
					ring = Ring;
				}
				return ring.IsValid;
			}
		}

		/// <summary> 
		/// Gets the coordinates for this ring as a <see cref="LineString"/>.
		/// Used to return the coordinates in this ring
		/// as a valid geometry, when it has been detected that the ring is topologically
		/// invalid.
		/// </summary>
		/// <value> 
		/// A <see cref="LineString"/> containing the coordinates 
		/// in this ring.
		/// </value>
		public LineString LineString
		{
			get
			{
				ringPts = this.Coordinates;
				return factory.CreateLineString(ringPts);
			}
		}

		/// <summary> 
		/// Returns this ring as a <see cref="LinearRing"/>, or null if an Exception occurs while
		/// creating it (such as a topology problem). Details of problems are written to
		/// standard output.
		/// </summary>
		public LinearRing Ring
		{
			get
			{
				if (ring != null)
					return ring;

				ringPts = this.Coordinates;
				if (ringPts.Count < 3)
				{
                    return null;
				}

				try
				{
					ring = factory.CreateLinearRing(ringPts);

                    return ring;
				}
				catch (System.Exception ex)
				{
                    ExceptionManager.Publish(ex);

                    throw;
                }
			}
		}
        
        #endregion
		
        #region Private Properties

        /// <summary> 
        /// Computes the list of coordinates which are contained in this ring.
        /// The coordinatea are computed once only and cached.
        /// 
        /// </summary>
        /// <returns> an array of the Coordinates in this ring
        /// </returns>
        private ICoordinateList Coordinates
        {
            get
            {
                if (ringPts == null)
                {
                    ringPts = new CoordinateCollection();

                    for (IEnumerator i = deList.GetEnumerator(); i.MoveNext(); )
                    {
                        DirectedEdge de = (DirectedEdge) i.Current;
                        PolygonizeEdge edge = (PolygonizeEdge) de.Edge;
                        AddEdge(edge.Line.Coordinates, de.EdgeDirection, ringPts);
                    }
                }

                return ringPts;
            }
        }
        
        #endregion

        #region Public Methods

		/// <summary> 
		/// Find the innermost enclosing shell EdgeRing containing the argument 
		/// EdgeRing, if any.
		/// The innermost enclosing ring is the smallest enclosing ring.
		/// The algorithm used depends on the fact that:
		/// <para>
		/// ring A Contains ring B iff envelope(ring A) Contains envelope(ring B)
		/// </para>
		/// This routine is only safe to use if the chosen point of the hole
		/// is known to be properly contained in a shell
		/// (which is guaranteed to be the case if the hole does not touch its shell)
		/// </summary>
		/// <returns> Returns a containing EdgeRing, if there is one or 
		/// <see langword="null"/> if no containing EdgeRing is found.
		/// </returns>
		public static EdgeRing FindEdgeRingContaining(EdgeRing testEr, ArrayList shellList)
		{
			LinearRing testRing = testEr.Ring;
			Envelope testEnv    = testRing.Bounds;
			Coordinate testPt   = testRing.GetCoordinate(0);
			
			EdgeRing minShell = null;
			Envelope minEnv   = null;

			for (IEnumerator it = shellList.GetEnumerator(); it.MoveNext(); )
			{
				EdgeRing tryShell = (EdgeRing) it.Current;
				LinearRing tryRing = tryShell.Ring;
				Envelope tryEnv = tryRing.Bounds;
				if (minShell != null)
					minEnv = minShell.Ring.Bounds;
				bool isContained = false;
				// the hole envelope cannot equal the shell envelope
				if (tryEnv.Equals(testEnv))
					continue;
				
				testPt = PointNotInList(testRing.Coordinates, tryRing.Coordinates);

				if (tryEnv.Contains(testEnv) && CGAlgorithms.IsPointInRing(testPt, tryRing.Coordinates))
					isContained = true;
				// check if this new containing ring is smaller than the current minimum ring
				if (isContained)
				{
					if (minShell == null || minEnv.Contains(tryEnv))
					{
						minShell = tryShell;
					}
				}
			}

			return minShell;
		}

		/// <summary> 
		/// Adds a {@link DirectedEdge} which is known to form part of this ring.
		/// </summary>
		/// <param name="de">the {@link DirectedEdge} to Add.
		/// </param>
		public void Add(DirectedEdge de)
		{
			deList.Add(de);
		}
		
		/// <summary> 
		/// Adds a hole to the polygon formed by this ring.
		/// </summary>
		/// <param name="hole">the {@link LinearRing} forming the hole.
		/// </param>
		public void AddHole(LinearRing hole)
		{
			if (holes == null)
			{
				holes = new GeometryList();
			}

			holes.Add(hole);
		}
        
        #endregion

        #region Private Methods

		/// <summary> 
		/// Finds a point in a list of points which is not contained in another 
		/// list of points.
		/// </summary>
		/// <param name="testPts">The Coordinates to test</param>
		/// <param name="pts">
		/// An array of Coordinates to test the input points against.
		/// </param>
		/// <returns> 
		/// A Coordinate from testPts which is not in pts, or <see langword="null"/>.
		/// </returns>
		private static Coordinate PointNotInList(ICoordinateList testPts, ICoordinateList pts)
		{
            int nCount = testPts.Count;
            for (int i = 0; i < nCount; i++)
            {
                Coordinate testPt = testPts[i];
                if (IndexOf(testPt, pts) < 0)
                    return testPt;
            }

            return null;
		}
		
        /// <summary>  
        /// Returns the index of <code>coordinate</code> in <code>coordinates</code>.
        /// The first position is 0; the second, 1; etc.
        /// 
        /// </summary>
        /// <param name="coordinate">  the <code>Coordinate</code> to search for
        /// </param>
        /// <param name="coordinates"> the array to search
        /// </param>
        /// <returns>              the position of <code>coordinate</code>, or -1 if it is
        /// not found
        /// </returns>
        private static int IndexOf(Coordinate coordinate, 
            ICoordinateList coordinates)
        {
            int nCount = coordinates.Count;

            for (int i = 0; i < nCount; i++)
            {
                if (coordinate.Equals(coordinates[i]))
                {
                    return i;
                }
            }

            return -1;
        }
		
        private static void AddEdge(ICoordinateList coords, bool isForward, 
            ICoordinateList coordList)
        {
            if (isForward)
            {
                for (int i = 0; i < coords.Count; i++)
                {
                    coordList.Add(coords[i], false);
                }
            }
            else
            {
                for (int i = coords.Count - 1; i >= 0; i--)
                {
                    coordList.Add(coords[i], false);
                }
            }
        }
        
        #endregion
	}
}