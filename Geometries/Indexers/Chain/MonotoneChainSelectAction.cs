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
using iGeospatial.Geometries;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Indexers.Chain
{
	/// <summary> 
	/// The action for the internal iterator for performing
	/// envelope select queries on a MonotoneChain
	/// </summary>
    [Serializable]
    internal class MonotoneChainSelectAction
	{
		// these envelopes are used during the MonotoneChain search process
		internal Envelope tempEnv1;
		
		internal LineSegment selectedSegment;
		
		public MonotoneChainSelectAction()
		{
            tempEnv1        = new Envelope();
            selectedSegment = new LineSegment((GeometryFactory)null);
		}

		/// <summary> This function can be overridden if the original chain is needed</summary>
		public virtual void Select(MonotoneChain mc, int start)
		{
			mc.GetLineSegment(start, selectedSegment);
			Select(selectedSegment);
		}
		
		/// <summary> This is a convenience function which can be overridden to obtain the actual
		/// line segment which is selected
		/// </summary>
		/// <param name="">seg
		/// </param>
		public virtual void Select(LineSegment seg)
		{
		}
	}
}