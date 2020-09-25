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
using System.Diagnostics;

using iGeospatial.Exceptions;
using iGeospatial.Coordinates;
using iGeospatial.Geometries.Editors;
using iGeospatial.Geometries.Operations.Precision;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Reduces the precision of a <see cref="Geometry"/>
	/// according to the supplied <see cref="PrecisionModel"/>, without
	/// attempting to preserve valid topology.
	/// </summary>
	/// <remarks>
	/// The topology of the resulting geometry may be invalid if
	/// topological collapse occurs due to coordinates being shifted.
	/// It is up to the client to check this and handle it if necessary.
	/// Collapses may not matter for some uses.  An example
	/// is simplifying the input to the buffer algorithm.
	/// The buffer algorithm does not depend on the validity of the input geometry.
	/// </remarks>
	public class SimplePrecisionReducer
	{
        #region Private Fields

        private PrecisionModel newPrecisionModel;
        private bool removeCollapsed;
        private bool changePrecisionModel;
        
        #endregion
		
        #region Constructors and Destructor

        public SimplePrecisionReducer(PrecisionModel pm)
        {
            newPrecisionModel    = pm;
            removeCollapsed      = true;
            changePrecisionModel = false;
        }
        
        #endregion

        #region Public Properties

        /// <summary> 
        /// Gets or sets whether the reduction will result in collapsed components
		/// being removed completely, or simply being collapsed to an (invalid)
		/// Geometry of the same type.
		/// The default is to remove collapsed components.
		/// 
		/// </summary>
		/// <param name="removeCollapsed">if <see langword="true"/> collapsed components will be removed
		/// </param>
		public virtual bool RemoveCollapsedComponents
		{
            get
            {
                return removeCollapsed;
            }

			set
			{
				removeCollapsed = value;
			}
		}
			
		/// <summary> 
		/// Gets or sets whether the <see cref="PrecisionModel"/> of the new reduced Geometry
		/// will be changed to be the <see cref="PrecisionModel"/> supplied to
		/// specify the reduction.  The default is to not change the precision model
		/// 
		/// </summary>
		/// <param name="changePrecisionModel">if <see langword="true"/> the precision model of the created Geometry will be the
		/// the precisionModel supplied in the constructor.
		/// </param>
		public virtual bool ChangePrecisionModel
		{
            get
            {
                return changePrecisionModel;
            }

			set
			{
				changePrecisionModel = value;
			}
		}
        
        #endregion
		
        #region Public Methods

		public virtual Geometry Reduce(Geometry geom)
		{
			GeometryEditor geomEdit;
			if (changePrecisionModel)
			{
                GeometryFactory factory = geom.Factory;

				GeometryFactory newFactory = new GeometryFactory(
                    newPrecisionModel, factory.CoordinateType,
                    factory.CoordinateDimension, factory.Properties);

				geomEdit = new GeometryEditor(newFactory);
			}
			else // don't change geometry factory
            {
                geomEdit = new GeometryEditor();
            }
			
			return geomEdit.Edit(geom, new PrecisionReducerCoordinateOperation(this));
		}
        
        #endregion
		
        #region PrecisionReducerCoordinateOperation Class

		private class PrecisionReducerCoordinateOperation : CoordinateOperation
		{
            private SimplePrecisionReducer enclosingInstance;

			public PrecisionReducerCoordinateOperation(SimplePrecisionReducer enclosingInstance)
			{
                this.enclosingInstance = enclosingInstance;
			}
			
			public SimplePrecisionReducer PrecisionReducer
			{
				get
				{
					return enclosingInstance;
				}
			}

			public override ICoordinateList Edit(ICoordinateList coordinates, Geometry geom)
			{
                int nCount = coordinates.Count;

				if (nCount == 0)
					return null;
				
				CoordinateCollection reducedCoords = 
                    new CoordinateCollection(nCount);

                PrecisionModel precisionModel = 
                    enclosingInstance.newPrecisionModel;

				// copy coordinates and reduce
				for (int i = 0; i < nCount; i++)
				{
					Coordinate coord = new Coordinate(coordinates[i]);
                    coord.MakePrecise(precisionModel);

                    reducedCoords.Add(coord);
				}

				// remove repeated points, to simplify returned geometry as much as possible
				CoordinateCollection noRepeatedCoordList = 
                    new CoordinateCollection(reducedCoords, false);
//				Coordinate[] noRepeatedCoords = noRepeatedCoordList.toCoordinateArray();
				
				// Check to see if the removal of repeated points
				// collapsed the coordinate List to an invalid length
				// for the type of the parent geometry.
				// It is not necessary to check for Point collapses, since the coordinate list can
				// never collapse to less than one point.
				// If the length is invalid, return the full-length coordinate array
				// first computed, or null if collapses are being removed.
				// (This may create an invalid geometry - the client must handle this.)
				int minLength = 0;

                GeometryType geomType = geom.GeometryType;

				if (geomType == GeometryType.LineString)
					minLength = 2;
				if (geomType == GeometryType.LinearRing)
					minLength = 4;
				
				ICoordinateList collapsedCoords = reducedCoords;
				if (enclosingInstance.removeCollapsed)
					collapsedCoords = null;
				
				// return null or orginal length coordinate array
				if (noRepeatedCoordList.Count < minLength)
				{
					return collapsedCoords;
				}
				
				// ok to return shorter coordinate array
				return noRepeatedCoordList;
			}
		}
        
        #endregion
    }
}