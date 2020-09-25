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
    /// that a rectangle intersects a geometry,
    /// based on the locations of the envelope(s) of the geometry.
    /// 
    /// </summary>
    [Serializable]
    internal sealed class EnvelopeIntersectsVisitor : ShortCircuitedGeometryVisitor
    {
        private Envelope rectEnv;
        private bool m_bIntersects;
		
        public EnvelopeIntersectsVisitor(Envelope rectEnv)
        {
            this.rectEnv = rectEnv;
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
		
        public override void Visit(Geometry element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Envelope elementEnv = element.Bounds;
            // disjoint
            if (!rectEnv.Intersects(elementEnv))
            {
                return ;
            }
            // fully contained - must intersect
            if (rectEnv.Contains(elementEnv))
            {
                m_bIntersects = true;
                return ;
            }
            /// <summary> Since the envelopes intersect and the test element is connected,
            /// if its envelope is completely bisected by an edge of the rectangle
            /// the element and the rectangle must touch.
            /// (Note it is NOT possible to make this conclusion
            /// if the test envelope is "on a corner" of the rectangle
            /// envelope)
            /// </summary>
            if (elementEnv.MinX >= rectEnv.MinX && 
                elementEnv.MaxX <= rectEnv.MaxX)
            {
                m_bIntersects = true;
                return ;
            }
            if (elementEnv.MinY >= rectEnv.MinY && 
                elementEnv.MaxY <= rectEnv.MaxY)
            {
                m_bIntersects = true;

                return;
            }
        }
    }
}
