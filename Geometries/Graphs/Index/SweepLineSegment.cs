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
using iGeospatial.Geometries.Graphs;

namespace iGeospatial.Geometries.Graphs.Index
{
	[Serializable]
    internal class SweepLineSegment
	{
        #region Private Fields

        private Edge            edge;
        private ICoordinateList pts;
        private int             ptIndex;
        
        #endregion
		
        #region Constructors and Destructor

        public SweepLineSegment(Edge edge, int ptIndex)
        {
            this.edge    = edge;
            this.ptIndex = ptIndex;
            this.pts     = edge.Coordinates;
        }

        #endregion

        #region Public Properties

		public double MinX
		{
			get
			{
				double x1 = pts[ptIndex].X;
				double x2 = pts[ptIndex + 1].X;

				return x1 < x2 ? x1 : x2;
			}  			
		}

		public double MaxX
		{
			get
			{
				double x1 = pts[ptIndex].X;
				double x2 = pts[ptIndex + 1].X;

				return x1 > x2 ? x1 : x2;
			}  			
		}
        
        #endregion
		
        #region Public Methods

		public void ComputeIntersections(SweepLineSegment ss, 
            SegmentIntersector si)
		{
			si.AddIntersections(edge, ptIndex, ss.edge, ss.ptIndex);
		}
        
        #endregion
	}
}