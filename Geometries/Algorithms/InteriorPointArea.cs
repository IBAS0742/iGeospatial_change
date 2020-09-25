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
using iGeospatial.Geometries;

namespace iGeospatial.Geometries.Algorithms
{
	
	/// <summary> 
	/// Computes a point in the interior of an area or surface geometry, 
	/// such as a <see cref="Polygon"/>.
	/// </summary>
	/// <remarks>
	/// Algorithm
	/// <list type="number">
	/// <item>
	/// <description>
	/// Find the intersections between the geometry and the horizontal bisector 
	/// of the area's envelope.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Pick the midpoint of the largest intersection (the intersections will be 
	/// lines and points).
	/// </description>
	/// </item>
	/// </list>
	/// 
	/// Note: If a fixed precision model is used, in some cases this method may 
	/// return a point which does not lie in the interior.
	/// </remarks>
	public sealed class InteriorPointArea
	{
        #region Private Members
        
        private GeometryFactory factory;
        private Coordinate      interiorPoint;
        private double          maxWidth;

        #endregion
		
        #region Constructors and Destructor
        
        public InteriorPointArea(Geometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            factory = geometry.Factory;
            Add(geometry);
        }

        #endregion
		
        #region Public Properties
		
        /// <summary>
        /// Gets a coordinate or point in the interior of a closed geometry.
        /// </summary>
        /// <value>
        /// The <see cref="iGeospatial.Coordinates.Coordinate"/> of an interior point
        /// of the geometry.
        /// </value>
        public Coordinate InteriorPoint
		{
			get
			{
				return interiorPoint;
			}
		}

        #endregion
		
		/// <summary> 
		/// Finds a reasonable point at which to label a Geometry.
		/// </summary>
		/// <param name="geometry">the geometry to analyze
		/// </param>
		/// <returns> the midpoint of the largest intersection between the geometry and
		/// a line halfway down its envelope
		/// </returns>
		public void AddPolygon(Geometry geometry)
		{
			LineString bisector = HorizontalBisector(geometry);
			
			Geometry intersections = bisector.Intersection(geometry);
			Geometry widestIntersection = WidestGeometry(intersections);
			
			double width = widestIntersection.Bounds.Width;
			if (interiorPoint == null || width > maxWidth)
			{
				interiorPoint = Centre(widestIntersection.Bounds);
				maxWidth = width;
			}
		}
		
		/// <summary> Returns the centre point of the envelope. </summary>
		/// <param name="envelope">The envelope to analyze.</param>
		/// <returns> The centre of the envelope.</returns>
		public Coordinate Centre(Envelope envelope)
		{
            if (envelope == null)
            {
                throw new ArgumentNullException("envelope");
            }

            return new Coordinate(Average(envelope.MinX, envelope.MaxX), 
                Average(envelope.MinY, envelope.MaxY));
		}
		
        /// <summary>
        /// Finds the widest <see cref="Geometry"/> in a geometry collection or list.
        /// </summary>
        /// <param name="geometry">
        /// The geometry to perform the operation on.
        /// </param>
        /// <returns>
        /// If geometry to perform the operation on is a collection, the widest 
        /// sub-geometry; otherwise, the geometry itself.
        /// </returns>
		private Geometry WidestGeometry(Geometry geometry)
		{
            if (!(geometry.IsCollection))
			{
				return geometry;
			}

			return WidestGeometry((GeometryCollection) geometry);
		}
		
        /// <summary>
        /// Compute a horizontal bisector of a <see cref="Geometry"/>.
        /// </summary>
        /// <param name="geometry">
        /// The geometry for which a bisector is required.
        /// </param>
        /// <returns>A <see cref="LineString"/> line segment of the bisector.</returns>
		private LineString HorizontalBisector(Geometry geometry)
		{
			Envelope envelope = geometry.Bounds;
			
			// Assert: for areas, minx <> maxx
			double avgY = Average(envelope.MinY, envelope.MaxY);

			return factory.CreateLineString(
                new Coordinate[]{new Coordinate(envelope.MinX, avgY), 
                                    new Coordinate(envelope.MaxX, avgY)});
		}
		
        /// <summary>
        /// Finds the widest geometry in a geometry collection or list. 
        /// The calculation is based on the envelope of the geometry.
        /// </summary>
        /// <param name="gc">
        /// The geometry collection from which to find the widest geometry.
        /// </param>
        /// <returns>The widest <see cref="Geometry"/>in the collection.</returns>
		private Geometry WidestGeometry(GeometryCollection gc)
		{
			if (gc.IsEmpty)
			{
				return gc;
			}
			
			Geometry widestGeometry = gc.GetGeometry(0);
            int nCount = gc.NumGeometries;
			for (int i = 1; i < nCount; i++)
			{
				//Start at 1
				if (gc.GetGeometry(i).Bounds.Width > 
                    widestGeometry.Bounds.Width)
				{
					widestGeometry = gc.GetGeometry(i);
				}
			}

			return widestGeometry;
		}
		
        /// <summary>
        /// Computes the average of two double values.
        /// </summary>
        /// <param name="a">The first double value.</param>
        /// <param name="b">The second double value.</param>
        /// <returns>
        /// A double value, which is an average of the two passed parameters.
        /// </returns>
		private static double Average(double a, double b)
		{
			return (a + b) / 2.0;
		}
		
		/// <summary> 
		/// Tests the interior vertices (if any) defined by a linear Geometry 
		/// for the best inside point. If a Geometry is not of dimension 1 
		/// it is not tested.
		/// </summary>
		/// <param name="geom">The geometry to add.</param>
		private void Add(Geometry geom)
		{
            GeometryType geomType = geom.GeometryType;

            if (geomType == GeometryType.Polygon)
            {
                AddPolygon(geom);
            }
            else if (geomType == GeometryType.GeometryCollection ||
                geomType == GeometryType.MultiPolygon ||
                geomType == GeometryType.MultiCircle ||
                geomType == GeometryType.MultiEllipse ||
                geomType == GeometryType.MultiRectangle)
			{
				GeometryCollection gc = (GeometryCollection) geom;
                int nCount = gc.NumGeometries;

				for (int i = 0; i < nCount; i++)
				{
					Add(gc.GetGeometry(i));
				}
			}
		}
	}
}