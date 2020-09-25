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

namespace iGeospatial.Geometries.Indexers.BinTree
{
	/// <summary> 
	/// An BinTree (or "Binary Interval Tree") is a 1-dimensional version of a quadtree.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The BinTree indexes 1-dimensional intervals (which may be the projection 
	/// of 2-D objects on an axis).
	/// It supports range searching (where the range may be a single point).
	/// </para>
	/// This implementation does not require specifying the extent of the inserted
	/// items beforehand.  It will automatically expand to accomodate any extent
	/// of dataset.
	/// <para>
    /// The bintree structure is used to provide a primary filter
    /// for interval queries.  The query() method returns a list of
    /// all objects which <c>may</c> intersect the query interval.
    /// Note that it may return objects which do not in fact intersect.
    /// A secondary filter is required to test for exact intersection.
    /// Of course, this secondary filter may consist of other tests besides
    /// intersection, such as testing other kinds of spatial relationships.
    /// </para>
	/// This index is different to the Interval Tree of Edelsbrunner
	/// or the Segment Tree of Bentley.
	/// </remarks>
    [Serializable]
    internal class Bintree
	{
		private Root root;

		/// <summary>  Statistics
		/// 
		/// minExtent is the minimum extent of all items
		/// inserted into the tree so far. It is used as a heuristic value
		/// to construct non-zero extents for features with zero extent.
		/// Start with a non-zero extent, in case the first feature inserted has
		/// a zero extent in both directions.  This value may be non-optimal, but
		/// only one feature will be inserted with this value.
		/// 
		/// </summary>
		private double minExtent = 1.0;
		
        /// <summary>
        /// Initializes a new instance of the <see cref="Bintree"/> class.
        /// </summary>
        public Bintree()
        {
            root = new Root();
        }
		
		/// <summary> 
		/// Ensure that the Interval for the inserted item has non-zero extents.
		/// Use the current minExtent to pad it, if necessary
		/// </summary>
		public static Interval EnsureExtent(Interval itemInterval, double minExtent)
		{
			double min = itemInterval.Min;
			double max = itemInterval.Max;
			// has a non-zero extent
			if (min != max)
				return itemInterval;
			
			// pad extent
			if (min == max)
			{
				min = min - minExtent / 2.0;
				max = min + minExtent / 2.0;
			}
			return new Interval(min, max);
		}
		
		public int Depth()
		{
			if (root != null)
				return root.Depth();

			return 0;
		}

		public int Size()
		{
			if (root != null)
				return root.Size();
			return 0;
		}

		/// <summary> 
		/// Compute the total number of nodes in the tree
		/// </summary>
		/// <returns> The number of nodes in the tree.</returns>
		public int NodeSize()
		{
			if (root != null)
				return root.NodeSize();
			return 0;
		}
		
		public void Insert(Interval itemInterval, object item)
		{
			CollectStats(itemInterval);
			Interval insertInterval = EnsureExtent(itemInterval, minExtent);

            root.Insert(insertInterval, item);
        }
		
		public IEnumerator Iterator()
		{
			ArrayList foundItems = new ArrayList();

			root.AddAllItems(foundItems);

			return foundItems.GetEnumerator();
		}
		
		public ArrayList Query(double x)
		{
			return Query(new Interval(x, x));
		}
		
		/// <summary> min and max may be the same value</summary>
		public ArrayList Query(Interval interval)
		{
			// The items that are matched are all items in intervals
			// which overlap the query interval
			ArrayList foundItems = new ArrayList();
			Query(interval, foundItems);

			return foundItems;
		}
		
		public void Query(Interval interval, ArrayList foundItems)
		{
			root.AddAllItemsFromOverlapping(interval, foundItems);
		}
		
		private void CollectStats(Interval interval)
		{
			double del = interval.Width;
			if (del < minExtent && del > 0.0)
				minExtent = del;
		}
	}
}