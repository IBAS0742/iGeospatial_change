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
using iGeospatial.Geometries.Visitors;

namespace iGeospatial.Geometries
{
	/// <summary> 
	/// Represents a linear polygon, which may include holes.
	/// </summary>
	/// <remarks>
	/// The shell and holes of the polygon are represented by <see cref="LinearRing"/>s.
	/// In a valid polygon, holes may touch the shell or other holes at a single point.
	/// However, no sequence of touching holes may split the polygon into two pieces.
	/// The orientation of the rings in the polygon does not matter.
	/// <para>
	/// The shell and holes must conform to the assertions specified in the
	/// <see href="http://www.opengis.org/specs/">OpenGIS Simple Features 
	/// Specification for SQL</see> 
	/// </para>
	/// </remarks>
	[Serializable]
    public class Polygon : Surface
	{
		/// <summary>  
		/// The exterior boundary, or null if this Polygon is the empty geometry.
		/// </summary>
		internal LinearRing m_objShell;
		
		/// <summary>  The interior boundaries, if any.</summary>
		internal LinearRing[] m_arrHoles;
		
		/// <summary>  
		/// Constructs a Polygon with the given exterior boundary and interior boundaries.
		/// </summary>
		/// <param name="shell">
		/// The outer boundary of the new Polygon, or <see langword="null"/> or an 
		/// empty LinearRing if the empty geometry is to be created.
		/// </param>
		/// <param name="holes">
		/// The inner boundaries of the new Polygon, or null or empty LinearRings 
		/// if the empty geometry is to be created.
		/// </param>
		public Polygon(LinearRing shell, LinearRing[] holes, GeometryFactory factory) 
            : base(factory)
		{
            if (shell == null)
			{
                GeometryFactory objFactory = factory;
                if (objFactory == null)
                {
                    objFactory = GeometryFactory.GetInstance();
                }

				shell = objFactory.CreateLinearRing((ICoordinateList) null);
			}
			
            if (holes != null && HasNullElements(holes))
			{
				throw new System.ArgumentException("holes must not contain null elements");
			}
			
            if (shell.IsEmpty && HasNonEmptyElements(holes))
			{
				throw new System.ArgumentException("shell is empty but holes are not");
			}

			m_objShell = shell;
			m_arrHoles = holes;
		}

        public override GeometryType GeometryType
        {
            get
            {
                return GeometryType.Polygon;
            }
        }

        public override string Name
        {
            get
            {
                return "Polygon";
            }
        }
		
		public override ICoordinateList Coordinates
		{
			get
			{
				if (IsEmpty)
				{
					return new CoordinateCollection();
				}
				
                CoordinateCollection coordinates = new CoordinateCollection(NumPoints);
//				int k = - 1;
				ICoordinateList shellCoordinates = m_objShell.Coordinates;
				
                for (int x = 0; x < shellCoordinates.Count; x++)
				{
//					k++;
					coordinates.Add(shellCoordinates[x]);
				}

                if (m_arrHoles != null)
                {
                    for (int i = 0; i < m_arrHoles.Length; i++)
                    {
                        ICoordinateList childCoordinates = m_arrHoles[i].Coordinates;
                        for (int j = 0; j < childCoordinates.Count; j++)
                        {
                            coordinates.Add(childCoordinates[j]);
                        }
                    }
                }
				
                return coordinates;
			}
		}

		public override int NumPoints
		{
			get
			{
				int numPoints = m_objShell.NumPoints;

                if (m_arrHoles != null)
                {
                    for (int i = 0; i < m_arrHoles.Length; i++)
                    {
                        numPoints += m_arrHoles[i].NumPoints;
                    }
                }

				return numPoints;
			}
		}

		public override DimensionType Dimension
		{
			get
			{
				return DimensionType.Surface;
			}
		}

		public override DimensionType BoundaryDimension
		{
			get
			{
				return DimensionType.Curve;
			}
		}

		public override bool IsEmpty
		{
			get
			{
				return m_objShell.IsEmpty;
			}
		}

		public override bool IsSimple
		{
			get
			{
				return true;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        public override bool IsRectangle
        {
            get
            {
                if (this.NumInteriorRings != 0) 
                    return false;
                if (m_objShell == null) 
                    return false;

                if (m_objShell.NumPoints != 5) 
                    return false;

                ICoordinateList seq = m_objShell.Coordinates;

                // check vertices have correct values
                Envelope env    = this.Bounds;
                Coordinate coord = null;

                for (int i = 0; i < 5; i++) 
                {
                    coord = seq[i];
                    double x = coord.X;
                    if (!(x == env.MinX || x == env.MaxX)) 
                        return false;

                    double y = coord.Y;
                    if (!(y == env.MinY || y == env.MaxY)) 
                        return false;
                }

                // check vertices are in right order
                coord = seq[0];
                double prevX = coord.X;
                double prevY = coord.Y;
                for (int i = 1; i <= 4; i++) 
                {
                    coord    = seq[i];
                    double x = coord.X;
                    double y = coord.Y;
                    
                    bool xChanged = x != prevX;
                    bool yChanged = y != prevY;

                    if (xChanged == yChanged)
                        return false;
                    
                    prevX = x;
                    prevY = y;
                }

                return true;
            }
        }

		public LinearRing ExteriorRing
		{
			get
			{
				return m_objShell;
			}
		}

        /// <summary>
        /// Gets the interior holes of this polygon.
        /// </summary>
        public LinearRing[] InteriorRings
        {
            get
            {
                return m_arrHoles;
            }
        }

		public int NumInteriorRings
		{
			get
			{
                if (m_arrHoles != null)
                {
                    return m_arrHoles.Length;
                }

				return 0;
			}
		}
			
		/// <summary>Gets the area of this Polygon.</summary>
		/// <value>The area of the polygon.</value>
		public override double Area
		{
			get
			{
				double area = 0.0;
				area += Math.Abs(CGAlgorithms.SignedArea(m_objShell.Coordinates));

                if (m_arrHoles != null)
                {
                    for (int i = 0; i < m_arrHoles.Length; i++)
                    {
                        area -= Math.Abs(CGAlgorithms.SignedArea(m_arrHoles[i].Coordinates));
                    }
                }

				return area;
			}
		}

		/// <summary>Gets the perimeter of this <see cref="Polygon"/>.</summary>
		/// <value>The perimeter of the polygon</value>
		public override double Length
		{
			get
			{
				double len = 0.0;
				len += m_objShell.Length;

                if (m_arrHoles != null)
                {
                    for (int i = 0; i < m_arrHoles.Length; i++)
                    {
                        len += m_arrHoles[i].Length;
                    }
                }

				return len;
			}
		}

		public override Geometry Boundary
		{
			get
			{
				if (IsEmpty)
				{
					return Factory.CreateGeometryCollection(null);
				}

                int nHoles = 0;
                if (m_arrHoles != null)
                {
                    nHoles = m_arrHoles.Length;
                }
				LinearRing[] rings = new LinearRing[nHoles + 1];
				rings[0]           = m_objShell;

                if (m_arrHoles != null)
                {
                    for (int i = 0; i < m_arrHoles.Length; i++)
                    {
                        rings[i + 1] = m_arrHoles[i];
                    }
                }

                if (rings.Length == 1)
                    return Factory.CreateLinearRing(rings[0].Coordinates);

				return Factory.CreateMultiLineString(rings);
			}
		}
		
		public override Coordinate Coordinate
		{
            get
            {
                return m_objShell.Coordinate;
            }
		}
		
		public LinearRing InteriorRing(int n)
		{
            if (m_arrHoles != null && n < m_arrHoles.Length)
            {
                return m_arrHoles[n];
            }

            return null;
		}
		
		protected override Envelope ComputeEnvelope()
		{
			return m_objShell.Bounds;
		}
		
		public override bool EqualsExact(Geometry other, double tolerance)
		{
			if (!IsEquivalentType(other))
			{
				return false;
			}

			Polygon otherPolygon       = (Polygon)other;
			Geometry thisShell         = m_objShell;
			Geometry otherPolygonShell = otherPolygon.m_objShell;
			
            if (!thisShell.EqualsExact(otherPolygonShell, tolerance))
			{
				return false;
			}
			
            if (this.NumInteriorRings != otherPolygon.NumInteriorRings)
			{
				return false;
			}
			
            if (m_arrHoles != null)
            {
                for (int i = 0; i < m_arrHoles.Length; i++)
                {
                    if (!((Geometry) m_arrHoles[i]).EqualsExact(otherPolygon.m_arrHoles[i], tolerance))
                    {
                        return false;
                    }
                }
            }
			
            return true;
		}
		
		public override void  Apply(ICoordinateVisitor filter)
		{
			m_objShell.Apply(filter);
            
            if (m_arrHoles != null)
            {
                for (int i = 0; i < m_arrHoles.Length; i++)
                {
                    m_arrHoles[i].Apply(filter);
                }
            }
		}
		
		public override void Apply(IGeometryVisitor filter)
		{
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            filter.Visit(this);
		}
		
		public override void Apply(IGeometryComponentVisitor filter)
		{
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            filter.Visit(this);
			m_objShell.Apply(filter);

            if (m_arrHoles != null)
            {
                for (int i = 0; i < m_arrHoles.Length; i++)
                {
                    m_arrHoles[i].Apply(filter);
                }
            }
		}
		
		public override Geometry Clone()
		{
			Polygon poly = (Polygon) base.MemberwiseClone();
			poly.m_objShell = (LinearRing) m_objShell.Clone();

            if (m_arrHoles != null)
            {
                poly.m_arrHoles = new LinearRing[m_arrHoles.Length];
                for (int i = 0; i < m_arrHoles.Length; i++)
                {
                    poly.m_arrHoles[i] = (LinearRing)m_arrHoles[i].Clone();
                }
            }

			return poly; // return the clone
		}
		
		public override Geometry ConvexHull()
		{
			return ExteriorRing.ConvexHull();
		}
		
		public override void Normalize()
		{
			Normalize(m_objShell, true);

            if (m_arrHoles != null)
            {
                for (int i = 0; i < m_arrHoles.Length; i++)
                {
                    Normalize(m_arrHoles[i], false);
                }

                Array.Sort(m_arrHoles);
            }
		}
		
		protected override int CompareToGeometry(Geometry o)
		{
			LinearRing thisShell  = m_objShell;
			LinearRing otherShell = ((Polygon)o).m_objShell;

            return thisShell.CompareTo(otherShell);
		}
		
        private void Normalize(LinearRing ring, bool clockwise) 
        {
            if (ring.IsEmpty) 
            {
                return;
            }

            CoordinateCollection uniqueCoordinates = new CoordinateCollection();
            for (int i = 0; i < ring.NumPoints - 1; i++)
            {
                uniqueCoordinates.Add(ring.GetCoordinate(i));		// copy all but last one into uniquecoordinates
            }

            Coordinate minCoordinate = CoordinateCollection.MinimumCoordinate(ring.Coordinates);
            uniqueCoordinates.Scroll(minCoordinate);

            ICoordinateList ringCoordinates = ring.Coordinates;
            ringCoordinates.Clear();
            ringCoordinates.AddRange(uniqueCoordinates);
            ringCoordinates.Add(uniqueCoordinates[0].Clone());		// add back in the closing point.
			
            if (CGAlgorithms.IsCCW(ringCoordinates) == clockwise)
            {
                ringCoordinates.Reverse();
            }
        }
	}
}