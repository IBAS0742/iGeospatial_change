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

namespace iGeospatial.Geometries.Indexers.StrTree
{
	/// <summary> 
	/// IBoundable wrapper for a non-IBoundable spatial object. 
	/// Used internally by AbstractSTRTree.
	/// </summary>
	internal class ItemBoundable : IBoundable
	{
        private object bounds;
        private object item;
		
        public ItemBoundable(object bounds, object item)
        {
            this.bounds = bounds;
            this.item = item;
        }
			
		public object Bounds
		{
			get
			{
				return bounds;
			}
		}
			
		public object Item
		{
			get
			{
				return item;
			}
		}
	}
}