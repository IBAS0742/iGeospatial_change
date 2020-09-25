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
using iGeospatial.Geometries.Indexers;
using iGeospatial.Geometries.Indexers.QuadTree;

namespace iGeospatial.Geometries.Simplifications
{
	/// <summary> An index of <see cref="LineSegment"/>s.</summary>
	[Serializable]
    public class LineSegmentIndex
	{
        #region Private Fields

		private Quadtree index = new Quadtree();
        
        #endregion
		
        #region Constructors and Destructor

		public LineSegmentIndex()
		{
		}
        
        #endregion
		
        #region Public Methods

		public void Add(TaggedLineString line)
		{
			TaggedLineSegment[] segs = line.Segments;
			
            for (int i = 0; i < segs.Length - 1; i++)
			{
				TaggedLineSegment seg = segs[i];

				Add(seg);
			}
		}
		
		public void Add(LineSegment seg)
		{
			index.Insert(new Envelope(seg.p0, seg.p1), seg);
		}
		
		public void Remove(LineSegment seg)
		{
			index.Remove(new Envelope(seg.p0, seg.p1), seg);
		}
		
		public IList Query(LineSegment querySeg)
		{
			Envelope env = new Envelope(querySeg.p0, querySeg.p1);
			
			LineSegmentVisitor visitor = new LineSegmentVisitor(querySeg);
			index.Query(env, visitor);

			IList itemsFound = visitor.Items;
			
			return itemsFound;
		}
        
        #endregion
    	
        #region LineSegmentVisitor Class

	    /// <summary> 
	    /// ItemVisitor subclass to reduce volume of query results.
	    /// </summary>
	    private sealed class LineSegmentVisitor : ISpatialIndexVisitor
	    {
            private LineSegment querySeg;
            private ArrayList items = new ArrayList();
    		
            public LineSegmentVisitor(LineSegment querySeg)
            {
                this.querySeg = querySeg;
            }
    		
		    public ArrayList Items
		    {
			    get
			    {
				    return items;
			    }
		    }
    			
		    // MD - only seems to make about a 10% difference in overall time.
		    public void VisitItem(object item)
		    {
			    LineSegment seg = (LineSegment)item;

			    if (Envelope.Intersects(seg.p0, seg.p1, 
                    querySeg.p0, querySeg.p1))
                {
                    items.Add(item);
                }
		    }
	    }
        
        #endregion
	}
}