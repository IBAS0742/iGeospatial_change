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
using iGeospatial.Geometries.Operations;

namespace iGeospatial.Geometries.Editors
{
	/// <summary> 
	/// Reduces the precision of a <see cref="Geometry"/> according to 
	/// the supplied <see cref="PrecisionModel"/>, without attempting to 
	/// preserve valid topology.
	/// </summary>
	/// <remarks>
	/// The topology of the resulting geometry may be invalid if
	/// topological collapse occurs due to coordinates being shifted.
	/// It is up to the client to check this and handle it if necessary.
	/// <para>
	/// Collapses may not matter for some uses.  An example
	/// is simplifying the input to the buffer algorithm.
	/// The buffer algorithm does not depend on the validity of the 
	/// input geometry.
	/// </para>
	/// </remarks>
	public class GeometryPrecisionReducer
	{
        private PrecisionModel newPrecisionModel;
        private bool removeCollapsed;
        private bool changePrecisionModel;
		
        public GeometryPrecisionReducer(PrecisionModel pm)
        {
            removeCollapsed = true;
            newPrecisionModel = pm;
        }
		
		/// <summary> 
		/// Gets or sets whether the reduction will result in collapsed components
		/// being removed completely, or simply being collapsed to an (invalid)
		/// Geometry of the same type.
		/// </summary>
		/// <param name="removeCollapsed">
		/// if true collapsed components will be removed.
		/// </param>
		public virtual bool RemoveCollapsedComponents
		{
            get
            {
                return this.removeCollapsed;
            }

			set
			{
				this.removeCollapsed = value;
			}
		}

		/// <summary> 
		/// Gets or sets whether the PrecisionModel of the new reduced Geometry
		/// will be changed to be the PrecisionModel supplied to
		/// specify the reduction.  The default is to not change the precision model
		/// 
		/// </summary>
		/// <param name="changePrecisionModel">if true the precision model of the created Geometry will be the
		/// the precisionModel supplied in the constructor.
		/// </param>
		public virtual bool ChangePrecisionModel
		{
            get
            {
                return this.changePrecisionModel;
            }

			set
			{
				this.changePrecisionModel = value;
			}
		}

		public virtual Geometry Reduce(Geometry geom)
		{
			GeometryEditor geomEdit;
			if (changePrecisionModel)
			{
				GeometryFactory newFactory = new GeometryFactory(newPrecisionModel);
				geomEdit = new GeometryEditor(newFactory);
			}
			else  // don't change geometry factory 
            {
                geomEdit = new GeometryEditor();
            }
			
			return geomEdit.Edit(geom, new PrecisionReducerGeometryEdit(this));
		}
		
		private class PrecisionReducerGeometryEdit : CoordinateGeometryEdit
		{
            private GeometryPrecisionReducer m_objPrecisionReducer;
			
			public PrecisionReducerGeometryEdit(GeometryPrecisionReducer precisionReducer)
			{
                this.m_objPrecisionReducer = precisionReducer;
            }

			public GeometryPrecisionReducer PrecisionReducer
			{
				get
				{
					return m_objPrecisionReducer;
				}
			}

			public override ICoordinateList Edit(ICoordinateList coordinates, 
                Geometry geometry)
			{
                if (coordinates == null)
                {
                    throw new ArgumentNullException("coordinates");
                }
                if (geometry == null)
                {
                    throw new ArgumentNullException("geometry");
                }

                int nCount = coordinates.Count;
                if (nCount == 0)
					return null;
				
				Coordinate[] reducedCoords = new Coordinate[nCount];
				
                // copy coordinates and reduce
				for (int i = 0; i < nCount; i++)
				{
					Coordinate coord = new Coordinate(coordinates[i]);  
					coord.MakePrecise(m_objPrecisionReducer.newPrecisionModel);
					reducedCoords[i] = coord;
				}

				// remove repeated points, to simplify returned geometry as much as possible
				ICoordinateList noRepeatedCoordList = 
                    new CoordinateCollection(reducedCoords, false);
				Coordinate[] noRepeatedCoords       = 
                    new Coordinate[noRepeatedCoordList.Count];
                noRepeatedCoordList.CopyTo(noRepeatedCoords);
				
				// Check to see if the removal of repeated points
				// collapsed the coordinate List to an invalid length
				// for the type of the parent geometry.
				// It is not necessary to check for Point collapses, since the coordinate list can
				// never collapse to less than one point.
				// If the length is invalid, return the full-length coordinate array
				// first computed, or null if collapses are being removed.
				// (This may create an invalid geometry - the client must handle this.)
				int minLength = 0;
				if (geometry.GeometryType == GeometryType.LineString)
					minLength = 2;
				else if (geometry.GeometryType == GeometryType.LinearRing)
					minLength = 4;
				
				Coordinate[] collapsedCoords = reducedCoords;
				if (m_objPrecisionReducer.removeCollapsed)
					collapsedCoords = null;
				
				// return null or orginal length coordinate array
				if (noRepeatedCoords.Length < minLength)
				{
					return new CoordinateCollection(collapsedCoords);
				}
				
				// ok to return shorter coordinate array
				return new CoordinateCollection(noRepeatedCoords);
			}
		}
	}
}