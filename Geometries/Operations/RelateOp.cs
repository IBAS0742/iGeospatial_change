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

using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries;
using iGeospatial.Geometries.Operations;
using iGeospatial.Coordinates;

using iGeospatial.Geometries.Operations.Relate;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Implements the <c>Relate()</c> operation on <see cref="Geometry"/> instances.
	/// </summary>
	public sealed class RelateOp : GraphGeometryOp
	{
        #region Private Fields

        private RelateComputer     m_objRelater;

        private IntersectionMatrix m_objMatrix;
        
        #endregion
		
        #region Constructors and Destructor
        
        public RelateOp(Geometry g0, Geometry g1) : base(g0, g1)
        {
            m_objRelater = new RelateComputer(arg);
        }
        
        #endregion
		
        #region Public Properties

        public IntersectionMatrix Matrix 
        {
            get 
            {
                if (m_objMatrix == null)
                {
                    m_objMatrix = this.Relate();
                }

                return m_objMatrix;
            }
        }
        
        #endregion

        #region Public Methods

		public IntersectionMatrix Relate()
		{
            m_objMatrix = m_objRelater.ComputeIM();

            return m_objMatrix;
		}

		public static IntersectionMatrix Relate(Geometry a, Geometry b)
		{
			RelateOp relOp        = new RelateOp(a, b);
			IntersectionMatrix im = relOp.Relate();

			return im;
		}
        
        #endregion
	}
}