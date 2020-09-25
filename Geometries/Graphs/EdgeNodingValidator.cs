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
using iGeospatial.Geometries.Noding;

namespace iGeospatial.Geometries.Graphs
{
	/// <summary> Validates that a collection of SegmentStrings is correctly noded.
	/// Throws an appropriate exception if an noding error is found.
	/// </summary>
	internal class EdgeNodingValidator
	{
        private NodingValidator nv;
		
        public EdgeNodingValidator(ArrayList edges)
        {
            nv = new NodingValidator(ToSegmentStrings(edges));
        }
		
		public void CheckValid()
		{
			nv.CheckValid();
		}
		
		private static ArrayList ToSegmentStrings(ArrayList edges)
		{
			// convert Edges to SegmentStrings
			ArrayList segStrings = new ArrayList();

            for (IEnumerator i = edges.GetEnumerator(); i.MoveNext(); )
			{
				Edge e = (Edge) i.Current;
				segStrings.Add(new SegmentString(e.Coordinates, e));
			}
			return segStrings;
		}
	}
}