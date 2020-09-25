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

namespace iGeospatial.Geometries.Simplifications
{
    [Serializable]
    public class TaggedLineString
	{
        #region Private Fields

        private LineString          parentLine;
        private TaggedLineSegment[] segs;
        private IList               resultSegs = new ArrayList();
        private int                 minimumSize;
        
        #endregion
		
        #region Constructors and Destructor

        public TaggedLineString(LineString parentLine) : this(parentLine, 2)
        {
        }
		
        public TaggedLineString(LineString parentLine, int minimumSize)
        {
            this.parentLine = parentLine;
            this.minimumSize = minimumSize;

            Initialize();
        }
        
        #endregion
		
        #region Public Properties

		public int MinimumSize
		{
			get
			{
				return minimumSize;
			}
		}
			
		public LineString Parent
		{
			get
			{
				return parentLine;
			}
		}
			
		public ICoordinateList ParentCoordinates
		{
			get
			{
				return parentLine.Coordinates;
			}
		}
			
		public ICoordinateList ResultCoordinates
		{
			get
			{
				return ExtractCoordinates(resultSegs);
			}
		}
			
		public int ResultSize
		{
			get
			{
				int resultSegsSize = resultSegs.Count;

				return resultSegsSize == 0 ? 0 : resultSegsSize + 1;
			}
		}
			
		public TaggedLineSegment[] Segments
		{
			get
			{
				return segs;
			}
		}
        
        #endregion
		
        #region Public Methods

		public TaggedLineSegment GetSegment(int i)
		{
			return segs[i];
		}
		
        public void AddToResult(LineSegment seg)
        {
            resultSegs.Add(seg);
        }
		
        public LineString AsLineString()
        {
            return parentLine.Factory.CreateLineString(
                ExtractCoordinates(resultSegs));
        }
		
        public virtual LinearRing AsLinearRing()
        {
            return parentLine.Factory.CreateLinearRing(
                ExtractCoordinates(resultSegs));
        }
        
        #endregion
		
        #region Private Methods

		private void Initialize()
		{
			ICoordinateList pts = parentLine.Coordinates;
            int nCount          = pts.Count;
			segs = new TaggedLineSegment[nCount - 1];
			for (int i = 0; i < nCount - 1; i++)
			{
				TaggedLineSegment seg = new TaggedLineSegment(pts[i], 
                    pts[i + 1], parentLine, i);

				segs[i] = seg;
			}
		}
		
		private static ICoordinateList ExtractCoordinates(IList segs)
		{
            int nCount       = segs.Count;

//			Coordinate[] pts = new Coordinate[nCount + 1];
            CoordinateCollection pts = new CoordinateCollection(nCount + 1);
            LineSegment seg  = null;

			for (int i = 0; i < nCount; i++)
			{
				seg    = (LineSegment)segs[i];
				pts.Add(seg.p0);
			}

			// add last point
			pts[pts.Count - 1] = seg.p1;

			return pts;
		}
        
        #endregion
	}
}