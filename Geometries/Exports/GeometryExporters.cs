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
using System.Reflection;

using iGeospatial.Geometries.IO;

namespace iGeospatial.Geometries.Exports
{
	/// <summary>
	/// This provides management services to all the geometry exporters.
	/// It provides static methods for registering and unregistering the
	/// exporters, as well as retrieving the registered exporters.
	/// </summary>
	public sealed class GeometryExporters
	{
        #region Private Static Members

        private static ArrayList m_listExporters;

        #endregion
		
        #region Constructors and Destructor
        
        private GeometryExporters()
		{
		}

//        static GeometryExporters()
//        {
//            GeometryWkExporters.RegisterAll();
//        }

        #endregion

        #region Public Static Properties

        public static int Exporters
        {
            get
            {
                if (m_listExporters != null)
                {
                    return m_listExporters.Count;
                }

                return 0;
            }
        }
        
        #endregion

        #region Public Static Methods
        
        public static IGeometryExporter GetExporter(GeometryExportType exportType)
        {
            ExporterInfo info = GetExporterInfo(exportType);

            if (info != null)
            {
                return CreateExporter(info.AssemblyName, info.TypeName);
            }

            return null;
        }
        
        public static void Register(GeometryExportType type, 
            string exporterAssembly, string exporterName)
        {
            if (m_listExporters == null)
            {
                m_listExporters = new ArrayList();
            }

            bool found = false;

            if (m_listExporters.Count > 0)
            {
                for (int i = 0; i < m_listExporters.Count; i++)
                {
                    ExporterInfo info = (ExporterInfo)m_listExporters[i];

                    if (info != null && info.ExportType == type)
                    {
                        found = true;
                        m_listExporters[i] =                 
                            new ExporterInfo(type, exporterAssembly, exporterName);

                        break;
                    }
                }
            }

            if (!found)                                              
            {
                m_listExporters.Add(new ExporterInfo(type, exporterAssembly, exporterName));
            }
        }

        public static void Unregister(GeometryExportType type)
        {
            if (m_listExporters != null && m_listExporters.Count > 0)
            {
                for (int i = 0; i < m_listExporters.Count; i++)
                {
                    ExporterInfo info = (ExporterInfo)m_listExporters[i];

                    if (info != null && info.ExportType == type)
                    {
                        m_listExporters.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Private Static Methods
        
        /// <summary>
        /// Returns an object instantiated by the Activator, using fully-qualified 
        /// assembly/type supplied.
        /// <para>
        /// Assembly parameter example: 
        /// "iGeospatial.Geometries.IO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
        /// </para>
        /// Type parameter example: iGeospatial.Geometries.IO.GeometryWktExporter"
        /// </summary>
        /// <param name="assemblyName">The fully-qualified assembly name.</param>
        /// <param name="typeName">The type name.</param>
        /// <returns>
        /// Instance of requested assembly/type typed as IGeometryExporter
        /// </returns>
        private static IGeometryExporter CreateExporter(string assemblyName, string typeName)
        {
            Assembly assemblyInstance = null;
            Type typeInstance		  = null;

            try 
            {
                //  use full asm name to get assembly instance
                assemblyInstance = Assembly.Load(assemblyName.Trim());
            }
            catch (Exception ex)
            {		
                throw new GeometryExportException("Error occurred when loading the assembly", ex); 
            }
			
            try
            {
                //  use type name to get type from asm; note we WANT case specificity 
                typeInstance = assemblyInstance.GetType( typeName.Trim(), true, false );

                return (IGeometryExporter)Activator.CreateInstance(typeInstance);
            }
            catch( Exception ex)
            {	
                throw new GeometryExportException("Error occurred when trying to create the exporter", 
                    ex);
            }
        }
                                  
        private static ExporterInfo GetExporterInfo(GeometryExportType type)
        {
            if (m_listExporters != null && m_listExporters.Count > 0)
            {
                for (int i = 0; i < m_listExporters.Count; i++)
                {
                    ExporterInfo info = (ExporterInfo)m_listExporters[i];

                    if (info != null && info.ExportType == type)
                    {
                        return info;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Inner Private ExporterInfo class
        
        private sealed class ExporterInfo
        {
            #region Private Members
            
            private GeometryExportType m_enumType;

            private string m_strExporterAssembly;

            private string m_strExporterName;

            #endregion

            #region Constructors and Destructor
            
            public ExporterInfo(GeometryExportType type,  
                string exporterAssembly, string exporterName)
            {
                m_enumType            = type;
                m_strExporterAssembly = exporterAssembly;
                m_strExporterName     = exporterName;
            }

            #endregion

            #region Public Properties
            
            public GeometryExportType ExportType 
            {
                get
                {
                    return m_enumType;
                }
            }

            public string AssemblyName
            {
                get
                {
                    return m_strExporterAssembly;
                }
            }

            public string TypeName
            {
                get
                {
                    return m_strExporterName;
                }
            }

            #endregion
        }

        #endregion
	}
}
