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
using iGeospatial.Geometries.Indexers.StrTree;
using iGeospatial.Geometries.Indexers.Chain;

namespace iGeospatial.Geometries.Noding
{
	/// <summary> 
	/// Nodes a set of {@link SegmentString}s using a index based
	/// on {@link MonotoneChain}s and a {@link SpatialIndex}.
	/// The {@link SpatialIndex} used should be something that supports
	/// envelope (range) queries efficiently (such as a {@link Quadtree}
	/// or {@link STRTree}.
	/// 
	/// </summary>
	internal class MCIndexNoder : SinglePassNoder
	{
		private IList monoChains    = new ArrayList();
		private ISpatialIndex index = new STRTree();
		private int idCounter = 0;
		private IList nodedSegStrings;
		// statistics
		private int nOverlaps = 0;
		
		public MCIndexNoder()
		{
		}
		
		public virtual IList MonotoneChains
		{
			get
			{
				return monoChains;
			}
		}
			
		public virtual ISpatialIndex Index
		{
			get
			{
				return index;
			}
		}
			
		public override IList NodedSubstrings
		{
			get
			{
				return SegmentString.GetNodedSubstrings(nodedSegStrings);
			}
		}
			
		public override void ComputeNodes(IList inputSegStrings)
		{
			this.nodedSegStrings = inputSegStrings;
			for (IEnumerator i = inputSegStrings.GetEnumerator(); i.MoveNext(); )
			{
				Add((SegmentString) i.Current);
			}

			IntersectChains();
		}
		
		private void IntersectChains()
		{
			MonotoneChainOverlapAction overlapAction = 
                new SegmentOverlapAction(this, segInt);
			
			for (IEnumerator i = monoChains.GetEnumerator(); i.MoveNext(); )
			{
				MonotoneChain queryChain = (MonotoneChain) i.Current;
				IList overlapChains = index.Query(queryChain.Envelope);

                for (IEnumerator j = overlapChains.GetEnumerator(); j.MoveNext(); )
				{
					MonotoneChain testChain = (MonotoneChain) j.Current;
					// following test makes sure we only compare each pair of chains once
					// and that we don't compare a chain to itself
					if (testChain.Id > queryChain.Id)
					{
						queryChain.ComputeOverlaps(testChain, overlapAction);
						nOverlaps++;
					}
				}
			}
		}
		
		private void Add(SegmentString segStr)
		{
			IList segChains = MonotoneChainBuilder.GetChains(
                segStr.Coordinates, segStr);

            for (IEnumerator i = segChains.GetEnumerator(); i.MoveNext(); )
			{
				MonotoneChain mc = (MonotoneChain) i.Current;
				mc.Id = idCounter++;
				index.Insert(mc.Envelope, mc);
				monoChains.Add(mc);
			}
		}
		
		public class SegmentOverlapAction : MonotoneChainOverlapAction
		{
			private MCIndexNoder m_objIndexNoder;
            private ISegmentIntersector si = null;

			public MCIndexNoder IndexNoder
			{
				get
				{
					return m_objIndexNoder;
				}
			}
			
			public SegmentOverlapAction(MCIndexNoder indexNoder, 
                ISegmentIntersector si)
			{
                m_objIndexNoder = indexNoder;
				this.si = si;
			}
			
			public override void Overlap(MonotoneChain mc1, int start1, 
                MonotoneChain mc2, int start2)
			{
				SegmentString ss1 = (SegmentString) mc1.Context;
				SegmentString ss2 = (SegmentString) mc2.Context;
				si.ProcessIntersections(ss1, start1, ss2, start2);
			}
		}
	}
}