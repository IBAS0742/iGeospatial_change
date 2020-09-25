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

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Graphs;

namespace iGeospatial.Geometries.Operations.Relate
{
	/// <summary> 
	/// Represents a node in the topological graph used to compute 
	/// spatial relationships.
	/// </summary>
	internal class RelateNode : Node
	{
        #region Constructors and Destructor

		public RelateNode(Coordinate coord, EdgeEndStar edges) 
            : base(coord, edges)
		{
		}

        #endregion
		
        #region Public Methods

		/// <summary> Update the IM with the contribution for this component.
		/// A component only contributes if it has a labelling for both parent geometries
		/// </summary>
		public override void ComputeIM(IntersectionMatrix im)
		{
			im.SetAtLeastIfValid(m_objLabel.GetLocation(0), m_objLabel.GetLocation(1), 0);
		}

		/// <summary> 
		/// Update the IM with the contribution for the EdgeEnds incident on this node.
		/// </summary>
		public void updateIMFromEdges(IntersectionMatrix im)
		{
			((EdgeEndBundleStar)this.Edges).UpdateIM(im);
		}
        
        #endregion
	}
}