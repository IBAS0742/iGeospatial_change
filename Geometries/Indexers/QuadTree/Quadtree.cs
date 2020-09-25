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

namespace iGeospatial.Geometries.Indexers.QuadTree
{
	/// <summary> 
	/// A Quadtree is a spatial index structure for efficient querying of 2D rectangles.  
	/// If other kinds of spatial objects need to be indexed they can be represented 
	/// by their envelopes.
	/// </summary>
	/// <remarks>
	/// The quadtree structure is used to provide a primary filter
	/// for range rectangle queries.  The Query() method returns a list of
	/// all objects which <c>may</c> intersect the query rectangle.  Note that
	/// it may return objects which do not in fact intersect.
	/// A secondary filter is required to test for exact intersection.
	/// Of course, this secondary filter may consist of other tests besides
	/// intersection, such as testing other kinds of spatial relationships.
	/// <para>
	/// This implementation does not require specifying the extent of the inserted
	/// items beforehand.  It will automatically expand to accomodate any extent
	/// of dataset.
	/// </para>
	/// This data structure is also known as an <c>MX-CIF quadtree</c>
	/// following the usage of Samet and others.
	/// </remarks>
	[Serializable]
    internal class Quadtree : ISpatialIndex
	{
		private Root root;

		/// <summary> minExtent is the minimum envelope extent of all items
		/// inserted into the tree so far. It is used as a heuristic value
		/// to construct non-zero envelopes for features with zero X and/or Y extent.
		/// Start with a non-zero extent, in case the first feature inserted has
		/// a zero extent in both directions.  This value may be non-optimal, but
		/// only one feature will be inserted with this value.
		/// </summary>
		private double minExtent = 1.0;
		
		/// <summary> Constructs a Quadtree with zero items.</summary>
		public Quadtree()
		{
			root = new Root();
		}
		
		/// <summary> 
		/// Ensure that the envelope for the inserted item has non-zero extents.
		/// Use the current minExtent to pad the envelope, if necessary
		/// </summary>
		public static Envelope EnsureExtent(Envelope itemEnv, double minExtent)
		{
			//The names "ensureExtent" and "minExtent" are misleading -- sounds like
			//this method ensures that the extents are greater than minExtent.
			//Perhaps we should rename them to "ensurePositiveExtent" and "defaultExtent".
			//[Jon Aquino]
			double minx = itemEnv.MinX;
			double maxx = itemEnv.MaxX;
			double miny = itemEnv.MinY;
			double maxy = itemEnv.MaxY;
			// has a non-zero extent
			if (minx != maxx && miny != maxy)
				return itemEnv;
			
			// pad one or both extents
			if (minx == maxx)
			{
				minx = minx - minExtent / 2.0;
				maxx = minx + minExtent / 2.0;
			}
			if (miny == maxy)
			{
				miny = miny - minExtent / 2.0;
				maxy = miny + minExtent / 2.0;
			}
			return new Envelope(minx, maxx, miny, maxy);
		}
		
		/// <summary> Returns the number of levels in the tree.</summary>
		public int Depth()
		{
			//I don't think it's possible for root to be null. Perhaps we should
			//remove the check. [Jon Aquino]
			//Or make an assertion [Jon Aquino 10/29/2003] 
			if (root != null)
				return root.Depth();
			return 0;
		}

		/// <summary> Returns the number of items in the tree.</summary>
		public int Size()
		{
			if (root != null)
				return root.Size();
			return 0;
		}
		
		public void Insert(Envelope itemEnv, object item)
		{
			CollectStats(itemEnv);
			Envelope insertEnv = EnsureExtent(itemEnv, minExtent);
			root.Insert(insertEnv, item);
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
            Envelope posEnv = EnsureExtent(itemEnv, minExtent);
            return root.Remove(posEnv, item);
        }
		
		public IList Query(Envelope searchEnv)
		{
			/// <summary> the items that are matched are the items in quads which
			/// overlap the search envelope
			/// </summary>
			ArrayList foundItems = new ArrayList();
			root.AddAllItemsFromOverlapping(searchEnv, foundItems);

			return foundItems;
		}
		
        public void Query(Envelope searchEnv, ISpatialIndexVisitor visitor)
        {
            // the items that are matched are the items in quads which
            // overlap the search envelope
                         
            root.Visit(searchEnv, visitor);
        }
		
		/// <summary> Return a list of all items in the Quadtree</summary>
		public IList QueryAll()
		{
			ArrayList foundItems = new ArrayList();
			root.AddAllItems(foundItems);

			return foundItems;
		}
		
		private void CollectStats(Envelope itemEnv)
		{
			double delX = itemEnv.Width;
			if (delX < minExtent && delX > 0.0)
				minExtent = delX;
			
			double delY = itemEnv.Width;
			if (delY < minExtent && delY > 0.0)
				minExtent = delY;
		}
	}
}