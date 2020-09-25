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
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.Graphs;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// This is <see langword="abstract"/> base class for operations 
	/// that require <see cref="GeometryGraph"/>s. 
	/// </summary>
	public abstract class GraphGeometryOp
	{
        #region Internal Fields
		
		internal LineIntersector li;
		internal PrecisionModel resultPrecisionModel;

		/// <summary> 
		/// The operation args into an array so they can be accessed 
		/// by index.
		/// </summary>
		internal GeometryGraph[] arg; // the arg(s) of the operation
        
        #endregion

        #region Constructors and Destructor
		
        protected GraphGeometryOp(Geometry g0, Geometry g1)
		{
            if (g0 == null)
            {
                throw new ArgumentNullException("g0");
            }
            if (g1 == null)
            {
                throw new ArgumentNullException("g1");
            }

            li  = new RobustLineIntersector();

			// use the most precise model for the result
			if (g0.PrecisionModel.CompareTo(g1.PrecisionModel) >= 0)
				ComputationPrecision = g0.PrecisionModel;
			else
				ComputationPrecision = g1.PrecisionModel;
			
			arg    = new GeometryGraph[2];
			arg[0] = new GeometryGraph(0, g0);
			arg[1] = new GeometryGraph(1, g1);
		}

		protected GraphGeometryOp(Geometry g0)
		{
            if (g0 == null)
            {
                throw new ArgumentNullException("g0");
            }

            li  = new RobustLineIntersector();

			ComputationPrecision = g0.PrecisionModel;
			
			arg    = new GeometryGraph[1];
			arg[0] = new GeometryGraph(0, g0); ;
		}
        
        #endregion
		
        #region Public Properties

        public PrecisionModel ComputationPrecision
        {
            get 
            {
                return resultPrecisionModel;
            }

            set
            {
                resultPrecisionModel = value;
                li.PrecisionModel    = resultPrecisionModel;
            }
        }
		
		public Geometry GetArgGeometry(int i)
		{
			return arg[i].Geometry;
		}
        
        #endregion
	}
}