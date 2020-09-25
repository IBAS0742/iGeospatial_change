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
using iGeospatial.Geometries.Algorithms;

using iGeospatial.Geometries.Indexers;
using iGeospatial.Geometries.Indexers.Chain;
using iGeospatial.Geometries.Indexers.StrTree;
using iGeospatial.Geometries.Indexers.QuadTree;

using LineIntersector = iGeospatial.Geometries.Algorithms.LineIntersector;

namespace iGeospatial.Geometries.Noding.SnapRounds
{
	/// <summary> 
	/// "Snaps" all <see cref="SegmentString"/>s in a 
	/// <see cref="ISpatialIndex"/> containing  
	/// <see cref="MonotoneChain"/>s to a given <see cref="HotPixel"/>.
	/// </summary>
	internal class MCIndexPointSnapper
	{
        #region Private Fields

        public static int   nSnaps = 0;
		
        private IList       monoChains;
        private STRTree     index;
		
        public MCIndexPointSnapper(IList monoChains, 
            ISpatialIndex index)
        {
            this.monoChains = monoChains;
            this.index      = (STRTree)index;
        }
        
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// Snaps (nodes) all interacting segments to this hot pixel.
		/// The hot pixel may represent a vertex of an edge,
		/// in which case this routine uses the optimization
		/// of not noding the vertex itself
		/// 
		/// </summary>
		/// <param name="hotPixel">the hot pixel to snap to
		/// </param>
		/// <param name="parentEdge">the edge containing the vertex, if applicable, or <code>null</code>
		/// </param>
		/// <param name="vertexIndex">the index of the vertex, if applicable, or -1
		/// </param>
		/// <returns> <see langword="true"/> if a node was added for this pixel
		/// </returns>
		public bool Snap(HotPixel hotPixel, SegmentString parentEdge, int vertexIndex)
		{
			Envelope pixelEnv = hotPixel.SafeEnvelope;
			
            HotPixelSnapAction hotPixelSnapAction = new HotPixelSnapAction(
                this, hotPixel, parentEdge, vertexIndex);
			
			index.Query(pixelEnv, new PointSnapperItemVisitor(pixelEnv, 
                hotPixelSnapAction, this));

			return hotPixelSnapAction.NodeAdded;
		}
		
		public bool Snap(HotPixel hotPixel)
		{
			return Snap(hotPixel, null, - 1);
		}
        
        #endregion
		
        #region PointSnapperItemVisitor Class

        private class PointSnapperItemVisitor : ISpatialIndexVisitor
        {
            private Envelope            pixelEnv;
            private HotPixelSnapAction  hotPixelSnapAction;
            private MCIndexPointSnapper enclosingInstance;

            public PointSnapperItemVisitor(Envelope pixelEnv, 
                HotPixelSnapAction hotPixelSnapAction, 
                MCIndexPointSnapper pointSnapper)
            {
                this.pixelEnv           = pixelEnv;
                this.hotPixelSnapAction = hotPixelSnapAction;
                this.enclosingInstance  = pointSnapper;
            }

            public MCIndexPointSnapper PointSnapper
            {
                get
                {
                    return enclosingInstance;
                }
            }
				
            public void VisitItem(object item)
            {
                MonotoneChain testChain = (MonotoneChain)item;
				
                testChain.Select(pixelEnv, hotPixelSnapAction);
            }
        }
        
        #endregion

        #region HotPixelSnapAction Class

		public class HotPixelSnapAction : MonotoneChainSelectAction
		{
            private MCIndexPointSnapper enclosingInstance;
			
            private HotPixel      hotPixel;
            private SegmentString parentEdge;
            private int           vertexIndex;
            private bool          m_bIsNodeAdded;
			
            public HotPixelSnapAction(MCIndexPointSnapper pointSnapper, 
                HotPixel hotPixel, SegmentString parentEdge, int vertexIndex)
            {
                this.enclosingInstance = pointSnapper;

                this.hotPixel          = hotPixel;
                this.parentEdge        = parentEdge;
                this.vertexIndex       = vertexIndex;
            }
			
            public bool NodeAdded
			{
				get
				{
					return m_bIsNodeAdded;
				}
			}
				
			public MCIndexPointSnapper PointSnapper
			{
				get
				{
					return enclosingInstance;
				}
			}
				
			public override void Select(MonotoneChain mc, int startIndex)
			{
				SegmentString ss = (SegmentString) mc.Context;
				// don't snap a vertex to itself
				if (parentEdge != null)
				{
					if (ss == parentEdge && startIndex == vertexIndex)
						return;
				}

				m_bIsNodeAdded = SimpleSnapRounder.AddSnappedNode(hotPixel, 
                    ss, startIndex);
			}
		}
        
        #endregion
    }
}