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
using iGeospatial.Geometries.Operations;
using iGeospatial.Geometries.Visitors;

namespace iGeospatial.Geometries
{
	/// <summary>
	/// Summary description for BezierCurve.
	/// </summary>
    [Serializable]
    public class BezierCurve : Curve
    {
        /// <summary>  The points of this BezierCurve.</summary>
        private ICoordinateList points;
		
        /// <summary>  Constructs a BezierCurve with the given points.
        /// 
        /// </summary>
        /// <param name="points">         the points of the linestring, or null
        /// to create the empty geometry. This array must not contain null
        /// elements. Consecutive points may not be equal.
        /// </param>
        /// <param name="precisionModel"> the specification of the grid of allowable points
        /// for this BezierCurve
        /// </param>
        public BezierCurve(Coordinate[] coordinates, GeometryFactory factory) 
            : base(factory)
        {
            if (coordinates == null)
            {
                this.points = new CoordinateCollection();
            }
            else 
            {
                if (coordinates.Length <= 1)
                {
                    throw new ArgumentException("point array must contain 0 or >1 elements");
                }

                this.points = new CoordinateCollection(coordinates);
            }
        }
		
        /// <param name="points">         the points of the linestring, or null
        /// to create the empty geometry. Consecutive points may not be equal.
        /// </param>
        public BezierCurve(ICoordinateList coordinates, GeometryFactory factory) 
            : base(factory)
        {
            if (coordinates == null)
            {
                coordinates = new CoordinateCollection();
            }
            else 
            {
                if (coordinates.Count <= 1)
                {
                    throw new ArgumentException("point array must contain 0 or >1 elements");
                }
            }

            this.points = coordinates;
        }
		
        public override ICoordinateList Coordinates
        {
            get
            {
                return points;
            }
        }

        public override DimensionType Dimension
        {
            get
            {
                return DimensionType.Curve;
            }
        }

        public override DimensionType BoundaryDimension
        {
            get
            {
                if (IsClosed)
                {
                    return DimensionType.Empty;
                }

                return DimensionType.Point;
            }
			
        }

        public override bool IsEmpty
        {
            get
            {
                return points.Count == 0;
            }
        }

        public override int NumPoints
        {
            get
            {
                return points.Count;
            }
        }

        public virtual Point StartPoint
        {
            get
            {
                if (IsEmpty)
                {
                    return null;
                }
                return GetPoint(0);
            }
        }

        public virtual Point EndPoint
        {
            get
            {
                if (IsEmpty)
                {
                    return null;
                }
                return GetPoint(NumPoints - 1);
            }
        }

        public virtual bool IsClosed
        {
            get
            {
                if (IsEmpty)
                {
                    return false;
                }

                return GetCoordinate(0).Equals(GetCoordinate(NumPoints - 1));
            }
        }

        public virtual bool IsRing
        {
            get
            {
                return IsClosed && IsSimple;
            }
        }

        public override GeometryType GeometryType
        {
            get
            {
                return GeometryType.BezierCurve;
            }
        }

        public override string Name
        {
            get
            {
                return "BezierCurve";
            }
        }

        /// <summary>  Returns the length of this BezierCurve
        /// 
        /// </summary>
        /// <returns> the area of the polygon
        /// </returns>
        public override double Length
        {
            get
            {
                return CGAlgorithms.Length(points);
            }
        }

        public override bool IsSimple
        {
            get
            {
                return true;
//                return (new IsSimpleOp()).IsSimple((BezierCurve)this);
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
                if (IsClosed)
                {
                    return Factory.CreateMultiPoint((Coordinate[]) null);
                }
                return Factory.CreateMultiPoint(new Point[]{StartPoint, EndPoint});
            }
        }
		
        public virtual Coordinate GetCoordinate(int n)
        {
            return points[n];
        }
		
        public override Coordinate Coordinate
        {
            get
            {
                if (IsEmpty)
                    return null;

                return points[0];
            }
        }
		
        public virtual Point GetPoint(int n)
        {
            return Factory.CreatePoint(points[n]);
        }
		
		
        /// <summary>  
        /// Returns true if the given point is a vertex of this BezierCurve.
        /// </summary>
        /// <param name="pt"> the Coordinate to check
        /// </param>
        /// <returns>     true if pt is one of this BezierCurve
        /// 's vertices
        /// </returns>
        public virtual bool IsCoordinate(Coordinate pt)
        {
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].Equals(pt))
                {
                    return true;
                }
            }
            return false;
        }
		
        protected override Envelope ComputeEnvelope()
        {
            if (IsEmpty)
            {
                return new Envelope();
            }
            //Convert to array, then access array directly, to avoid the function-call overhead
            //of calling #get millions of times. #toArray may be inefficient for
            //non-BasicCoordinateSequence CoordinateSequences. [Jon Aquino]
            ICoordinateList coordinates = points;
            double minx = coordinates[0].X;
            double miny = coordinates[0].Y;
            double maxx = coordinates[0].X;
            double maxy = coordinates[0].Y;
            //OptimizeIt shows that Math#min and Math#max here are a bottleneck.
            //Replace with direct comparisons. [Jon Aquino] 
            for (int i = 1; i < coordinates.Count; i++)
            {
                minx = minx < coordinates[i].X ? minx:coordinates[i].X;
                maxx = maxx > coordinates[i].X ? maxx:coordinates[i].X;
                miny = miny < coordinates[i].Y ? miny:coordinates[i].Y;
                maxy = maxy > coordinates[i].Y ? maxy:coordinates[i].Y;
            }
            return new Envelope(minx, maxx, miny, maxy);
        }
		
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentType(other))
            {
                return false;
            }

            BezierCurve otherLineString = (BezierCurve) other;
			
            if (points.Count != otherLineString.points.Count)
            {
                return false;
            }
			
            for (int i = 0; i < points.Count; i++)
            {
                if (!points[i].Equals(otherLineString.points[i], tolerance))
                {
                    return false;
                }
            }
			
            return true;
        }
		
        public override void  Apply(ICoordinateVisitor filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            for (int i = 0; i < points.Count; i++)
            {
                filter.Visit(points[i]);
            }
        }
		
        public override void  Apply(IGeometryVisitor filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            filter.Visit(this);
        }
		
        public override void  Apply(IGeometryComponentVisitor filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            filter.Visit(this);
        }
		
        #region ICloneable Members

        public override Geometry Clone()
        {
            BezierCurve ls = (BezierCurve) base.MemberwiseClone();
            ls.points      = points.Clone();
            return ls;
        }
        
        #endregion
		
        /// <summary> Normalizes a BezierCurve.  A normalized linestring
        /// has the first point which is not equal to it's reflected point
        /// less than the reflected point.
        /// </summary>
        public override void Normalize()
        {
            for (int i = 0; i < points.Count / 2; i++)
            {
                int j = points.Count - 1 - i;
                // skip equal points on both ends
                if (!points[i].Equals(points[j]))
                {
                    if (points[i].CompareTo(points[j]) > 0)
                    {
                        Coordinates.Reverse();
                    }
                    return ;
                }
            }
        }
		
        protected override bool IsEquivalentType(Geometry other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return (other.GeometryType == GeometryType.BezierCurve);
        }
		
        protected override int CompareToGeometry(Geometry o)
        {
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }

            BezierCurve line = (BezierCurve) o;
            // MD - optimized implementation
            int i = 0;
            int j = 0;
            while (i < points.Count && j < line.points.Count)
            {
                int comparison = points[i].CompareTo(line.points[j]);
                if (comparison != 0)
                {
                    return comparison;
                }
                i++;
                j++;
            }
			
            if (i < points.Count)
            {
                return 1;
            }

            if (j < line.points.Count)
            {
                return - 1;
            }
            return 0;
        }
    }
}
