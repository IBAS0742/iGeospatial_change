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
    /// <summary> Tests whether it can be concluded
    /// that a geometry contains a corner point of a rectangle.
    /// 
    /// </summary>
    [Serializable]
    internal sealed class ContainsPointVisitor : ShortCircuitedGeometryVisitor
    {
        private ICoordinateList rectSeq;
        private Envelope rectEnv;
        private bool m_bContainsPoint;
		
        public ContainsPointVisitor(Polygon rectangle)
        {
            this.rectSeq = rectangle.ExteriorRing.Coordinates;
            rectEnv      = rectangle.Bounds;
        }
		
        public override bool IsDone
        {
            get
            {
                return m_bContainsPoint == true;
            }
        }
			
        public bool ContainsPoint()
        {
            return m_bContainsPoint;
        }
		
        public override void Visit(Geometry geom)
        {
            if (geom == null)
            {
                throw new ArgumentNullException("geom");
            }

            if (geom.GeometryType != GeometryType.Polygon)
                return;

            Polygon polygon = (Polygon)geom;

            Envelope elementEnv = geom.Bounds;
            if (!rectEnv.Intersects(elementEnv))
                return ;
            // test each corner of rectangle for inclusion
            for (int i = 0; i < 4; i++)
            {
                Coordinate rectPt = rectSeq[i];

                if (!elementEnv.Contains(rectPt))
                    continue;

                // check rect point in poly (rect is known not to touch polygon at this point)
                if (SimplePointInAreaLocator.ContainsPointInPolygon(rectPt, polygon))
                {
                    m_bContainsPoint = true;
                    return ;
                }
            }
        }
    }
}
