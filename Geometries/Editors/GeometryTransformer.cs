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
using System.Collections;

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Editors
{
	/// <summary> 
	/// A framework for processes which transform an input <see cref="Geometry"/> 
	/// into an output <see cref="Geometry"/>, possibly changing its 
	/// structure and type(s).
	/// </summary>
	/// <remarks>
	/// This class is a framework for implementing subclasses that 
	/// perform transformations on various different Geometry subclasses.
	/// It provides an easy way of applying specific transformations
	/// to given geometry types, while allowing unhandled types to be simply copied.
	/// Also, the framework handles ensuring that if subcomponents change type
	/// the parent geometries types change appropriately to maintain valid structure.
	/// Subclasses will override whichever <c>GeometryTransformer.Transform</c> methods
	/// they need to to handle particular Geometry types.
	/// <para>
	/// A typically usage would be a transformation that may transform Polygons into
	/// Polygons, LineStrings or Points.  
	/// This class would likely need to override the <see cref="GeometryTransformer.Transform"/>
	/// method to ensure that if input Polygons change type the result is a GeometryCollection,
	/// not a MultiPolygon
	/// </para>
	/// The default behaviour of this class is to simply recursively transform
	/// each Geometry component into an identical object by copying.
	/// <para>
	/// Note that all <c>GeometryTransformer.Transform</c> methods may return <see langword="null"/>,
	/// to avoid creating empty geometry objects. This will be handled correctly
	/// by the transformer.
	/// </para>
	/// The <see cref="GeometryTransformer.Transform"/> method itself will always
	/// return a geometry object.
	/// </remarks>
	/// <seealso cref="GeometryEditor"/>
	[Serializable]
    public abstract class GeometryTransformer
	{
        #region Private Fields

		/// <summary> Possible extensions:
		/// getParent() method to return immediate parent e.g. of LinearRings in Polygons
		/// </summary>
		
		private Geometry inputGeom;
		
		private GeometryFactory geomFactory;
		
		// these could eventually be exposed to clients
		/// <summary> <see langword="true"/> if empty geometries should not be included in the result</summary>
		private bool pruneEmptyGeometry = true;
		
		/// <summary> 
		/// <see langword="true"/> if a homogenous collection result
		/// from a <see cref="GeometryCollection"/> should still
		/// be a general GeometryCollection
		/// </summary>
		private bool preserveGeometryCollectionType = true;
		
		/// <summary> <see langword="true"/> if the type of the input should be preserved</summary>
		private bool preserveType;
        
        #endregion
		
        #region Constructors and Destructor
		
        protected GeometryTransformer()
		{
            Initialize();
		}
        
        #endregion

        #region Public Properties

        public GeometryFactory Factory
        {
            get
            {
                return geomFactory;
            }
        }
		
        public Geometry InputGeometry
        {
            get
            {
                return inputGeom;
            }
        }
        
        #endregion
		
        #region Public Methods

		public Geometry Transform(Geometry input)
		{
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            this.inputGeom   = input;
			this.geomFactory = input.Factory;
			
            GeometryType geomType = input.GeometryType;

            if (geomType == GeometryType.Point)
				return Transform((Point)input, null);
			if (geomType == GeometryType.MultiPoint)
				return Transform((MultiPoint)input, null);
			if (geomType == GeometryType.LinearRing)
				return Transform((LinearRing)input, null);
			if (geomType == GeometryType.LineString)
				return Transform((LineString)input, null);
			if (geomType == GeometryType.MultiLineString)
				return Transform((MultiLineString)input, null);
			if (geomType == GeometryType.Polygon)
				return Transform((Polygon)input, null);
			if (geomType == GeometryType.MultiPolygon)
				return Transform((MultiPolygon)input, null);
			if (geomType == GeometryType.GeometryCollection)
				return Transform((GeometryCollection)input, null);
			
			throw new ArgumentException("Unknown Geometry subtype: " 
                + input.Name);
		}
        
        #endregion

        #region Private Methods

        private void Initialize()
        {       
            pruneEmptyGeometry             = true;
            preserveGeometryCollectionType = true;
            preserveType                   = false;
        }

        #endregion
		
        #region Protected Methods

		/// <summary> 
		/// Convenience method which provides statndard way of
		/// creating a <see cref="ICoordinateList"/>
		/// </summary>
		/// <param name="coords">The coordinate array to copy.</param>
		/// <returns> A coordinate sequence for the array.</returns>
		protected ICoordinateList CreateCoordinatesList(Coordinate[] coordinates)
		{
			return new CoordinateCollection(coordinates);
		}
		
		/// <summary> 
		/// Convenience method which provides statndard way of 
		/// copying <see cref="ICoordinateList"/>s.
		/// </summary>
		/// <param name="seq">the sequence to copy
		/// </param>
		/// <returns> a deep copy of the sequence
		/// </returns>
		protected ICoordinateList Copy(ICoordinateList coordinates)
		{
            if (coordinates == null)
            {
                throw new ArgumentNullException("coordinates");
            }

            return (ICoordinateList)coordinates.Clone();
		}
		
		protected virtual ICoordinateList Transform(ICoordinateList coords, 
            Geometry parent)
		{
			return Copy(coords);
		}
		
		protected virtual Geometry Transform(Point geom, Geometry parent)
		{
			return geomFactory.CreatePoint(Transform(geom.Coordinates, geom));
		}
		
		protected virtual Geometry Transform(LineString geom, Geometry parent)
		{
			// should check for 1-point sequences and downgrade them to points
			return geomFactory.CreateLineString(Transform(geom.Coordinates, geom));
		}
		
		protected virtual Geometry Transform(LinearRing geom, Geometry parent)
		{
			ICoordinateList seq = Transform(geom.Coordinates, geom);
			int seqSize         = seq.Count;

			// ensure a valid LinearRing
			if (seqSize > 0 && seqSize < 4 && !preserveType)
				return geomFactory.CreateLineString(seq);

			return geomFactory.CreateLinearRing(seq);
		}
		
		protected virtual Geometry Transform(MultiPoint geom, Geometry parent)
		{
			GeometryList transGeomList = new GeometryList();
			for (int i = 0; i < geom.NumGeometries; i++)
			{
				Geometry transformGeom = Transform(geom[i], geom);
				if (transformGeom == null)
					continue;
				if (transformGeom.IsEmpty)
					continue;

				transGeomList.Add(transformGeom);
			}

			return geomFactory.BuildGeometry(transGeomList);
		}
		
		protected virtual Geometry Transform(MultiLineString geom, Geometry parent)
		{
			GeometryList transGeomList = new GeometryList();

			for (int i = 0; i < geom.NumGeometries; i++)
			{
				Geometry transformGeom = Transform(geom[i], geom);
				if (transformGeom == null)
					continue;
				if (transformGeom.IsEmpty)
					continue;

				transGeomList.Add(transformGeom);
			}

			return geomFactory.BuildGeometry(transGeomList);
		}
		
		protected virtual Geometry Transform(Polygon geom, Geometry parent)
		{
			bool isAllValidLinearRings = true;
			Geometry shell = Transform(geom.ExteriorRing, geom);
			
			if (shell == null || 
                !(shell.GeometryType == GeometryType.LinearRing) ||
                shell.IsEmpty)
            {
                isAllValidLinearRings = false;
            }
			
			GeometryList holes = new GeometryList();
			for (int i = 0; i < geom.NumInteriorRings; i++)
			{
				Geometry hole = Transform(geom.InteriorRing(i), geom);
				if (hole == null || hole.IsEmpty)
				{
					continue;
				}

				if (!(hole.GeometryType == GeometryType.LinearRing))
					isAllValidLinearRings = false;
				
				holes.Add(hole);
			}
			
			if (isAllValidLinearRings)
            {
                return geomFactory.CreatePolygon((LinearRing)shell, 
                    holes.ToLinearRingArray());
            }
			else
			{
				GeometryList components = new GeometryList();
				if (shell != null)
					components.Add(shell);
				
                components.AddRange(holes);

				return geomFactory.BuildGeometry(components);
			}
		}
		
		protected virtual Geometry Transform(MultiPolygon geom, Geometry parent)
		{
			GeometryList transGeomList = new GeometryList();
			for (int i = 0; i < geom.NumGeometries; i++)
			{
				Geometry transformGeom = Transform(geom[i], geom);
				if (transformGeom == null)
					continue;
				if (transformGeom.IsEmpty)
					continue;

				transGeomList.Add(transformGeom);
			}

			return geomFactory.BuildGeometry(transGeomList);
		}
		
		protected virtual Geometry Transform(GeometryCollection geom, Geometry parent)
		{
			GeometryList transGeomList = new GeometryList();
			for (int i = 0; i < geom.NumGeometries; i++)
			{
				Geometry transformGeom = Transform(geom[i]);
				if (transformGeom == null)
					continue;
				if (pruneEmptyGeometry && transformGeom.IsEmpty)
					continue;

				transGeomList.Add(transformGeom);
			}

			if (preserveGeometryCollectionType)
				return geomFactory.CreateGeometryCollection(transGeomList.ToArray());

			return geomFactory.BuildGeometry(transGeomList);
		}
        
        #endregion
	}
}