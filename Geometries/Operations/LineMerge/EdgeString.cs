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

namespace iGeospatial.Geometries.Operations.LineMerge
{
	/// <summary> 
	/// A sequence of <see cref="LineMergeDirectedEdge"/>s forming one of 
	/// the lines that will be output by the line-merging process.
	/// </summary>
	internal sealed class EdgeString
	{
        #region Private Fields

        private GeometryFactory factory;
        private ArrayList directedEdges;
        private ICoordinateList coordinates;
        
        #endregion

        #region Constructors and Destructor

		/// <summary> 
		/// Constructs an <see cref="EdgeString"/> with the given factory used to convert 
		/// this <see cref="EdgeString"/> to a <see cref="LineString"/>.
		/// </summary>
		public EdgeString(GeometryFactory factory)
		{
            directedEdges = new ArrayList();

			this.factory = factory;
		}

        #endregion

        #region Private Properties

		private ICoordinateList Coordinates
		{
			get
			{
				if (coordinates == null)
				{
					int forwardDirectedEdges = 0;
					int reverseDirectedEdges = 0;
					coordinates = new CoordinateCollection();

                    for (IEnumerator i = directedEdges.GetEnumerator(); i.MoveNext(); )
					{
						LineMergeDirectedEdge directedEdge = (LineMergeDirectedEdge) i.Current;
						if (directedEdge.EdgeDirection)
						{
							forwardDirectedEdges++;
						}
						else
						{
							reverseDirectedEdges++;
						}

						coordinates.Add(((LineMergeEdge) directedEdge.Edge).Line.Coordinates, 
                            false, directedEdge.EdgeDirection);
					}

					if (reverseDirectedEdges > forwardDirectedEdges)
					{
                        coordinates.Reverse();
					}
				}
				
				return coordinates;
			}
		}
        
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// Adds a directed edge which is known to form part of this line.
		/// </summary>
		public void Add(LineMergeDirectedEdge directedEdge)
		{
			directedEdges.Add(directedEdge);
		}
		
		/// <summary> 
		/// Converts this EdgeString into a LineString.
		/// </summary>
		public LineString ToLineString()
		{
			return factory.CreateLineString(Coordinates);
		}
        
        #endregion
	}
}