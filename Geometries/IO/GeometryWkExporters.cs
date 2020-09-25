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
using System.Reflection; 

using iGeospatial.Geometries;
using iGeospatial.Geometries.Exports;

namespace iGeospatial.Geometries.IO
{
	/// <summary>
	/// This exposes static methods for registering OpenGIS well-known format 
	/// exporters, that is well-known text (WKT) and well-known binary (WKB).
	/// </summary>
	public sealed class GeometryWkExporters
	{
		private GeometryWkExporters()
		{
		}

        /// <summary>
        /// Registers a well-known text format exporter.
        /// </summary>
        public static void RegisterWkt()
        {
            GeometryExporters.Register(GeometryExportType.Wkt,
                Assembly.GetExecutingAssembly().FullName, 
                "iGeospatial.Geometries.IO.GeometryWktExporter");
        }

        /// <summary>
        /// Registers a well-known binary format exporter.
        /// </summary>
        public static void RegisterWkb()
        {
            GeometryExporters.Register(GeometryExportType.Wkb,
                Assembly.GetExecutingAssembly().FullName, 
                "iGeospatial.Geometries.IO.GeometryWkbExporter");
        }

        /// <summary>
        /// Registers a specified export.
        /// </summary>
        /// <param name="type">
        /// The type of exporter to register. In this case, it is either 
        /// <see cref="iGeospatial.Geometries.Exports.GeometryExportType.Wkt"/> or
        /// <see cref="iGeospatial.Geometries.Exports.GeometryExportType.Wkb"/>
        /// </param>
        public static void Register(GeometryExportType type)
        {
            if (type == GeometryExportType.Wkt)
            {
                RegisterWkt();
            }

            if (type == GeometryExportType.Wkb)
            {
                RegisterWkb();
            }
        }

        /// <summary>
        /// Registers both well-known format (text and binary) exporters.
        /// </summary>
        public static void RegisterAll()
        {
            // Register the WKT
            RegisterWkt();

            //...and register the WKB
            RegisterWkb();
        }
	}
}
