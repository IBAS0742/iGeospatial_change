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

namespace iGeospatial.Geometries.Indexers.StrTree
{
	/// <summary> 
	/// One-dimensional version of an STR-packed R-tree. SIR stands for
	/// "Sort-Interval-Recursive". STR-packed R-trees are described in:
	/// P. Rigaux, Michel Scholl and Agnes Voisard. Spatial Databases With
	/// Application To GIS. Morgan Kaufmann, San Francisco, 2002.
	/// </summary>
	/// <seealso cref="STRTree">
	/// </seealso>
	internal class SIRTree : AbstractSTRTree
	{
        private IComparer comparator;
		
        private IIntersectsOp intersectsOp;
		                       
		internal sealed class SIRComparator : IComparer
		{
            private SIRTree m_objSIRtree;
			
			public SIRComparator(SIRTree sirTree)
			{
                this.m_objSIRtree = sirTree;
			}

            public int Compare(object o1, object o2)
			{
                if (o1 == null)
                {
                    throw new ArgumentNullException("o1");
                }
                if (o2 == null)
                {
                    throw new ArgumentNullException("o2");
                }

				return m_objSIRtree.CompareDoubles(
                    ((Interval)((IBoundable)o1).Bounds).Centre, 
                    ((Interval)((IBoundable) o2).Bounds).Centre);
			}
		}
		                         
		internal sealed class SIRIntersectsOp : IIntersectsOp
		{
            private SIRTree m_objSIRtree;
			
			public SIRIntersectsOp(SIRTree sirTree)
			{
                this.m_objSIRtree = sirTree;
			}

            public bool Intersects(object aBounds, object bBounds)
			{
                if (aBounds == null)
                {
                    throw new ArgumentNullException("aBounds");
                }
                if (bBounds == null)
                {
                    throw new ArgumentNullException("bBounds");
                }

                return ((Interval) aBounds).Intersects((Interval) bBounds);
			}
		}
                                 
		private sealed class SIRAbstractNode : AbstractNode
		{
            private SIRTree m_objSIRtree;
			
			internal SIRAbstractNode(SIRTree sirTree, int Param1)
                : base(Param1)
			{
                this.m_objSIRtree = sirTree;
			}

			protected override object ComputeBounds()
			{
				Interval bounds = null;

                for (IEnumerator i = ChildBoundables.GetEnumerator(); i.MoveNext(); )
				{
					IBoundable childBoundable = (IBoundable) i.Current;
					if (bounds == null)
					{
						bounds = new Interval((Interval) childBoundable.Bounds);
					}
					else
					{
						bounds.ExpandToInclude((Interval) childBoundable.Bounds);
					}
				}
				return bounds;
			}
		}

		protected override IComparer Comparator
		{
			get
			{
				return comparator;
			}
		}
		
		/// <summary> 
		/// Constructs an SIRTree with the default node capacity.
		/// </summary>
		public SIRTree() : this(10)
		{
		}
		
		/// <summary> 
		/// Constructs an SIRTree with the given maximum number of child nodes 
		/// that a node may have.
		/// </summary>
		public SIRTree(int nodeCapacity) : base(nodeCapacity)
		{
            comparator   = new SIRComparator(this);
            intersectsOp = new SIRIntersectsOp(this);
        }
		
		protected override AbstractNode CreateNode(int level)
		{
			return new SIRAbstractNode(this, level);
		}
		
		/// <summary> 
		/// Inserts an item having the given bounds into the tree.
		/// </summary>
		public void Insert(double x1, double x2, object item)
		{
			base.Insert(new Interval(Math.Min(x1, x2), Math.Max(x1, x2)), item);
		}
		
		/// <summary> 
		/// Returns items whose bounds intersect the given value.
		/// </summary>
		public ArrayList Query(double x)
		{
			return Query(x, x);
		}
		
		/// <summary> 
		/// Returns items whose bounds intersect the given bounds.
		/// </summary>
		/// <param name="x1">possibly equal to x2
		/// </param>
		public ArrayList Query(double x1, double x2)
		{
			return base.Query(new Interval(Math.Min(x1, x2), Math.Max(x1, x2)));
		}
		
		protected override IIntersectsOp GetIntersectsOp()
		{
			return intersectsOp;
		}
	}
}