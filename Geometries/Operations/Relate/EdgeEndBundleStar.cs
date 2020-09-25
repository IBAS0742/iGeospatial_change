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
using iGeospatial.Geometries.Graphs;

namespace iGeospatial.Geometries.Operations.Relate
{
	/// <summary> 
	/// An ordered list of <see cref="EdgeEndBundle"/>s around a <see cref="RelateNode"/>.
	/// They are maintained in CCW order (starting with the positive x-axis) around the node
	/// for efficient lookup and topology building.
	/// </summary>
	internal class EdgeEndBundleStar : EdgeEndStar
	{
        #region Constructors and Destructor

		/// <summary>
		/// Initializes a new instance of the <see cref="EdgeEndBundleStar"/> class.
		/// </summary>
		public EdgeEndBundleStar()
		{
		}

        #endregion
		
        #region Public Methods

		/// <summary> 
		/// Insert a EdgeEnd in order in the list. If there is an existing 
		/// EdgeEndBundle which is parallel, the EdgeEnd is added to the bundle.
		/// Otherwise, a new EdgeEndBundle is created to contain the EdgeEnd.
		/// </summary>
		public override void Insert(EdgeEnd e)
		{
			EdgeEndBundle eb = (EdgeEndBundle) edgeMap[e];
			if (eb == null)
			{
				eb = new EdgeEndBundle(e);
				InsertEdgeEnd(e, eb);
			}
			else
			{
				eb.Insert(e);
			}
		}
		
		/// <summary> 
		/// Update the IM with the contribution for the EdgeEndBundle around the node.
		/// </summary>
		public void UpdateIM(IntersectionMatrix im)
		{
			for (IEnumerator it = Iterator(); it.MoveNext(); )
			{
				EdgeEndBundle esb = (EdgeEndBundle) it.Current;
				esb.UpdateIM(im);
			}
		}
        
        #endregion
	}
}