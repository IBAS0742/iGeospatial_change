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
using System.Diagnostics;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Indexers;

namespace iGeospatial.Geometries.Indexers.StrTree
{
    /// <summary>  
    /// A query-only R-tree created using the Sort-Tile-Recursive (STR) algorithm.
    /// For two-dimensional spatial data.
    /// </summary>
    /// <remarks>
    /// The STR packed R-tree is simple to implement and maximizes space
    /// utilization; that is, as many leaves as possible are filled to capacity.
    /// Overlap between nodes is far less than in a basic R-tree. However, once the
    /// tree has been built (explicitly or on the first call to Query), items may
    /// not be added or removed.
    /// <para>
    /// Described in: P. Rigaux, Michel Scholl and Agnes Voisard.
    /// Spatial Databases With Application To GIS.
    /// Morgan Kaufmann, San Francisco, 2002.
    /// </para>
    /// </remarks>
    internal class STRTree : AbstractSTRTree, ISpatialIndex
	{
        private IComparer xComparator;

        private IComparer yComparator;
		
        private IIntersectsOp intersectsOp;
		
		/// <summary> 
		/// Constructs an STRTree with the default node capacity.
		/// </summary>
		public STRTree() : this(10)
		{
		}
		
		/// <summary> 
		/// Constructs an STRTree with the given maximum number of child nodes that
		/// a node may have
		/// </summary>
		public STRTree(int nodeCapacity) : base(nodeCapacity)
		{
            xComparator  = new STRComparatorX(this);
            yComparator  = new STRComparatorY(this);
            intersectsOp = new STRIntersectsOp(this);
        }
		                      
		internal sealed class STRComparatorX : IComparer
		{
            private STRTree m_objSTRTree;

			public STRComparatorX(STRTree strTree)
			{
                this.m_objSTRTree = strTree;
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

                return m_objSTRTree.CompareDoubles(
                    m_objSTRTree.CentreX((Envelope) ((IBoundable) o1).Bounds), 
                    m_objSTRTree.CentreX((Envelope) ((IBoundable) o2).Bounds));
			}
		}

		internal sealed class STRComparatorY : IComparer
		{
            private STRTree m_objSTRTree;

			public STRComparatorY(STRTree strTree)
			{
                this.m_objSTRTree = strTree;
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

                return m_objSTRTree.CompareDoubles(m_objSTRTree.CentreY((Envelope) ((IBoundable) o1).Bounds), 
                    m_objSTRTree.CentreY((Envelope) ((IBoundable) o2).Bounds));
			}
		}
                                
		internal sealed class STRIntersectsOp : IIntersectsOp
		{
            private STRTree m_objSTRTree;
			
			public STRIntersectsOp(STRTree strTree)
			{
                this.m_objSTRTree = strTree;
			}

			public bool Intersects(object aBounds, object bBounds)
			{
				return ((Envelope) aBounds).Intersects((Envelope) bBounds);
			}
		}
                                    
		private sealed class STRAbstractNode : AbstractNode
		{
            private STRTree m_objSTRTree;

			internal STRAbstractNode(STRTree strTree, int Param1) 
                : base(Param1)
			{
                this.m_objSTRTree = strTree;
			}

			protected override object ComputeBounds()
			{
				Envelope bounds = null;
				for (IEnumerator i = ChildBoundables.GetEnumerator(); i.MoveNext(); )
				{
					IBoundable childBoundable = (IBoundable) i.Current;
					if (bounds == null)
					{
						bounds = new Envelope((Envelope) childBoundable.Bounds);
					}
					else
					{
						bounds.ExpandToInclude((Envelope) childBoundable.Bounds);
					}
				}
				return bounds;
			}
		}

		protected override IComparer Comparator
		{
			get
			{
				return yComparator;
			}
			
		}
		
		private double CentreX(Envelope e)
		{
			return Average(e.MinX, e.MaxX);
		}
		
		private double Average(double a, double b)
		{
			return (a + b) / 2d;
		}
		
		private double CentreY(Envelope e)
		{
			return Average(e.MinY, e.MaxY);
		}
		
		/// <summary> Creates the parent level for the given child level. First, orders the items
		/// by the x-values of the midpoints, and groups them into vertical slices.
		/// For each slice, orders the items by the y-values of the midpoints, and
		/// group them into runs of size M (the node capacity). For each run, creates
		/// a new (parent) node.
		/// </summary>
		protected override ArrayList 
            CreateParentBoundables(ArrayList childBoundables, int newLevel)
		{
			Debug.Assert(!(childBoundables.Count == 0));

            int minLeafCount = (int) Math.Ceiling((childBoundables.Count / (double) NodeCapacity));

            ArrayList sortedChildBoundables = new ArrayList(childBoundables);

            sortedChildBoundables.Sort(xComparator);

            ArrayList[] VerticalSlices = this.VerticalSlices(sortedChildBoundables, 
                (int) Math.Ceiling(Math.Sqrt(minLeafCount)));

			return CreateParentBoundablesFromVerticalSlices(VerticalSlices, newLevel);
		}
		
		private ArrayList CreateParentBoundablesFromVerticalSlices(ArrayList[] VerticalSlices, int newLevel)
		{
			Debug.Assert(VerticalSlices.Length > 0);
			ArrayList parentBoundables = new ArrayList();
			for (int i = 0; i < VerticalSlices.Length; i++)
			{
				parentBoundables.AddRange(CreateParentBoundablesFromVerticalSlice(VerticalSlices[i], newLevel));
			}
			return parentBoundables;
		}
		
		protected virtual ArrayList 
            CreateParentBoundablesFromVerticalSlice(ArrayList childBoundables, int newLevel)
		{
			return base.CreateParentBoundables(childBoundables, newLevel);
		}
		
		/// <param name="childBoundables">
		/// Must be sorted by the x-value of the envelope midpoints
		/// </param>
		protected virtual ArrayList[] 
            VerticalSlices(ArrayList childBoundables, int sliceCount)
		{
			int sliceCapacity = (int) Math.Ceiling(childBoundables.Count / (double) sliceCount);
			ArrayList[] slices = new ArrayList[sliceCount];
			IEnumerator i = childBoundables.GetEnumerator();
			for (int j = 0; j < sliceCount; j++)
			{
				slices[j] = new ArrayList();
				int boundablesAddedToSlice = 0;
				while (i.MoveNext() && boundablesAddedToSlice < sliceCapacity)
				{
                    IBoundable childBoundable = (IBoundable) i.Current;
					slices[j].Add(childBoundable);
					boundablesAddedToSlice++;
				}
			}
			return slices;
		}
		
		protected override AbstractNode CreateNode(int level)
		{
			return new STRAbstractNode(this, level);
		}
		
		protected override IIntersectsOp GetIntersectsOp()
		{
			return intersectsOp;
		}
		
		/// <summary> Inserts an item having the given bounds into the tree.</summary>
		public void Insert(Envelope itemEnv, object item)
		{
            if (itemEnv == null)
            {
                throw new ArgumentNullException("itemEnv");
            }

            if (itemEnv.IsEmpty)
			{
				return ;
			}

			base.Insert(itemEnv, item);
		}
		
		/// <summary> Returns items whose bounds intersect the given envelope.</summary>
		public IList Query(Envelope searchEnv)
		{
			//Yes this method does something. It specifies that the bounds is an
			//Envelope. super.query takes an Object, not an Envelope. [Jon Aquino 10/24/2003] 
			return base.Query(searchEnv);
		}
		
        /// <summary> 
        /// Queries the index for all items whose extents intersect the 
        /// given search <see cref="Envelope"/>, and applies an 
        /// <see cref="ISpatialIndexVisitor"/> to them.
        /// Note that some kinds of indexes may also return objects which do not in fact
        /// intersect the query envelope.
        /// 
        /// </summary>
        /// <param name="searchEnv">The envelope to query for.</param>
        /// <param name="visitor">
        /// A visitor object to apply to the items found.
        /// </param>
        public void Query(Envelope searchEnv, ISpatialIndexVisitor visitor)
        {
            //Yes this method does something. It specifies that the bounds is an
            //Envelope. super.query takes an Object, not an Envelope. [Jon Aquino 10/24/2003] 
            base.Query(searchEnv, visitor);
        }
		
        /// <summary> Removes a single item from the tree.
        /// 
        /// </summary>
        /// <param name="itemEnv">the Envelope of the item to remove
        /// </param>
        /// <param name="item">the item to remove
        /// </param>
        /// <returns> <see langword="true"/> if the item was found
        /// </returns>
        public bool Remove(Envelope itemEnv, object item)
        {
            return base.Remove(itemEnv, item);
        }
		
        /// <summary> Returns the number of items in the tree.
        /// 
        /// </summary>
        /// <returns> the number of items in the tree
        /// </returns>
        public override int Size()
        {
            return base.Size();
        }
		
        /// <summary> Returns the number of items in the tree.
        /// 
        /// </summary>
        /// <returns> the number of items in the tree
        /// </returns>
        public override int Depth()
        {
            return base.Depth();
        }

	}
}