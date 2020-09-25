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
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Operations.Predicate
{
    /// <summary> Tests whether any line segment of a geometry intersects a given rectangle.
    /// Optimizes the algorithm used based on the number of line segments in the
    /// test geometry.
    /// 
    /// </summary>
    [Serializable]
    internal sealed class LineIntersectsVisitor : ShortCircuitedGeometryVisitor
    {
        private Polygon rectangle;
        private ICoordinateList rectSeq;
        private Envelope rectEnv;
        private bool m_bIntersects;
		
        public LineIntersectsVisitor(Polygon rectangle)
        {
            this.rectangle = rectangle;
            this.rectSeq   = rectangle.ExteriorRing.Coordinates;
            rectEnv        = rectangle.Bounds;
        }
		
        public override bool IsDone
        {
            get
            {
                return m_bIntersects == true;
            }
        }
			
        public bool Intersects()
        {
            return m_bIntersects;
        }
		
        public override void Visit(Geometry geom)
        {
            if (geom == null)
            {
                throw new ArgumentNullException("geom");
            }

            Envelope elementEnv = geom.Bounds;
            if (!rectEnv.Intersects(elementEnv))
                return;

            // check if general relate algorithm should be used, since it's faster for large inputs
            if (geom.NumPoints > 
                RectangleIntersects.MaxScanSegments)
            {
                m_bIntersects = rectangle.Relate(geom).Intersects;
                return ;
            }

            ComputeSegmentIntersection(geom);
        }
		
        private void ComputeSegmentIntersection(Geometry geom)
        {
            // check segment intersection
            // get all lines from geom (e.g. if it's a multi-ring polygon)
            IGeometryList lines = LineStringExtracter.GetLines(geom);

            SegmentIntersectionTester si = new SegmentIntersectionTester();
            bool hasIntersection         = si.HasIntersectionWithLineStrings(
                rectSeq, lines);

            if (hasIntersection)
            {
                m_bIntersects = true;
                return;
            }
        }
    }
}
