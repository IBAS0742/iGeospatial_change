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

using iGeospatial.Geometries.Indexers.StrTree;
using iGeospatial.Geometries.Indexers.QuadTree;

namespace iGeospatial.Geometries.Indexers
{
	/// <summary>
	/// Summary description for SpatialIndexerFactory.
	/// </summary>
	public sealed class SpatialIndexerFactory
	{
        #region Constructors and Destructor

		private SpatialIndexerFactory()
		{
		}
        
        #endregion

        #region Public Methods

        public static ISpatialIndex Create(SpatialIndexType type)
        {
            if (type == SpatialIndexType.QuadTree)
            {
                return new Quadtree();
            }

            if (type == SpatialIndexType.RTree)
            {
                return new STRTree();
            }

            return null;
        }

        public static ISpatialIndex Create(SpatialIndexType type, 
            int nodeCapacity)
        {
            if (type == SpatialIndexType.QuadTree)
            {
                return new Quadtree();
            }

            if (type == SpatialIndexType.RTree)
            {
                return new STRTree(nodeCapacity);
            }

            return null;
        }

        #endregion
	}
}
