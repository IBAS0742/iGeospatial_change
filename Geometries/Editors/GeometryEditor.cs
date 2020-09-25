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
	/// Supports creating a new <see cref="Geometry"/> which is a modification 
	/// of an existing one.
	/// </summary>
	/// <remarks>
	/// <see cref="Geometry"/> objects are intended to be treated as immutable.
	/// This class allows you to "modify" a Geometry by traversing it and creating 
	/// a new Geometry with the same overall structure but possibly modified components.
	/// The following kinds of modifications can be made:
	/// <list type="number">
	/// <item>
	/// <description>
	/// The values of the coordinates may be changed. Changing coordinate values may 
	/// make the result Geometry invalid; this is not checked by the GeometryEditor.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// The coordinate lists may be changed (e.g. by adding or deleting coordinates).
	/// The modifed coordinate lists must be consistent with their original parent component
	/// (e.g. a <see cref="LinearRing"/> must always have at least 4 coordinates, 
	/// and the first and last coordinate must be equal).
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Components of the original geometry may be deleted (e.g. holes may be 
	/// removed from a <see cref="Polygon"/>, or LineStrings removed from a 
	/// <see cref="MultiLineString"/>).
	/// Deletions will be propagated up the component tree appropriately.
	/// </description>
	/// </item>
	/// </list>
	/// Note that all changes must be consistent with the original Geometry's structure
	/// (e.g. a Polygon cannot be collapsed into a LineString).
	/// <para>
	/// The resulting Geometry is not checked for validity.
	/// If validity needs to be enforced, the <see cref="Geometry.IsValid"/> 
	/// should be checked.
	/// </para>
	/// <seealso cref="Geometry.IsValid">Geometry.IsValid</seealso>
	/// </remarks>
	public class GeometryEditor
	{
		/// <summary> The factory used to create the modified Geometry</summary>
		private GeometryFactory m_objFactory;
		
		/// <summary> 
		/// Creates a new <see cref="GeometryEditor"/> object which will create an edited 
		/// <see cref="Geometry"/> with the same <see cref="GeometryFactory"/> as the 
		/// input Geometry.
		/// </summary>
		public GeometryEditor()
		{
		}
		
		/// <summary> 
		/// Creates a new GeometryEditor object which will create the edited 
		/// <see cref="Geometry"/> with the given <see cref="GeometryFactory"/>. 
		/// </summary>
		/// <param name="factory">
		/// The <see cref="GeometryFactory"/> to create the edited Geometry with.
		/// </param>
		public GeometryEditor(GeometryFactory factory)
		{
			m_objFactory = factory;
		}
		
		/// <summary> 
		/// Edit the input <see cref="Geometry"/> with the given edit operation.
		/// Clients will create subclasses of <see cref="IGeometryEdit"/> or
		/// <see cref="CoordinateGeometryEdit"/> to perform required modifications.
		/// </summary>
		/// <param name="geometry">The Geometry to edit</param>
		/// <param name="operation">The edit operation to carry out.</param>
		/// <returns>A new Geometry which is the result of the editing.</returns>
		public virtual Geometry Edit(Geometry geometry, IGeometryEdit operation)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            // if client did not supply a GeometryFactory, use the one from the input Geometry
			if (m_objFactory == null)
				m_objFactory = geometry.Factory;
			
            GeometryType geomType = geometry.GeometryType;

            if (geomType == GeometryType.GeometryCollection)
			{
				return EditGeometryCollection((GeometryCollection)geometry, operation);
			}
			
			if (geomType == GeometryType.Polygon)
			{
				return EditPolygon((Polygon) geometry, operation);
			}
			
			if (geomType == GeometryType.Point)
			{
				return operation.Edit(geometry, m_objFactory);
			}
			
			if (geomType == GeometryType.LineString ||
                geomType == GeometryType.LinearRing)
			{
				return operation.Edit(geometry, m_objFactory);
			}
			
			Debug.Assert(false, "Should never reach here: Unsupported Geometry classes should be caught in the IGeometryEdit.");
			
			return null;
		}
		
		private Polygon EditPolygon(Polygon polygon, IGeometryEdit operation)
		{
			Polygon newPolygon = (Polygon)operation.Edit(polygon, m_objFactory);
			
			if (newPolygon.IsEmpty)
			{
				//RemoveSelectedPlugIn relies on this behaviour. [Jon Aquino]
				return newPolygon;
			}
			
			LinearRing shell = (LinearRing)Edit(newPolygon.ExteriorRing, operation);
			
			if (shell.IsEmpty)
			{
				//RemoveSelectedPlugIn relies on this behaviour. [Jon Aquino]
				return m_objFactory.CreatePolygon(null, null);
			}
			
			GeometryList holes = new GeometryList();
			
			for (int i = 0; i < newPolygon.NumInteriorRings; i++)
			{
				LinearRing hole = (LinearRing) Edit(newPolygon.InteriorRing(i), operation);
				
				if (hole.IsEmpty)
				{
					continue;
				}
				
				holes.Add(hole);
			}
			
			return m_objFactory.CreatePolygon(shell, holes.ToLinearRingArray());
		}
		
		private GeometryCollection EditGeometryCollection(GeometryCollection collection, 
            IGeometryEdit operation)
		{
			GeometryCollection newCollection = 
                (GeometryCollection) operation.Edit(collection, m_objFactory);

            GeometryList geometries = new GeometryList();
			
			for (int i = 0; i < newCollection.NumGeometries; i++)
			{
				Geometry geometry = Edit(newCollection.GetGeometry(i), operation);
				
				if (geometry.IsEmpty)
				{
					continue;
				}
				
				geometries.Add(geometry);
			}
			
            GeometryType geomType = newCollection.GeometryType;

            if (geomType == GeometryType.MultiPoint)
			{
				return m_objFactory.CreateMultiPoint(geometries.ToPointArray());
			}
			
			if (geomType == GeometryType.MultiLineString)
			{
				return m_objFactory.CreateMultiLineString(
                    geometries.ToLineStringArray());
			}
			
			if (geomType == GeometryType.MultiPolygon)
			{
				return m_objFactory.CreateMultiPolygon(geometries.ToPolygonArray());
			}
			
			return m_objFactory.CreateGeometryCollection(geometries.ToArray());
		}
	}		
}