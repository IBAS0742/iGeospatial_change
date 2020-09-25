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

namespace iGeospatial.Geometries
{
//    Point              
//    MultiPoint         
//    LineSegment        
//    LineString         
//    LinearRing         
//    MultiLineString    
//    Triangle           
//    MultiTriangle      
//    Polygon            
//    MultiPolygon       
//    Rectangle          
//    RoundedRectangle   
//    MultiRectangle     
//    CircularArc        
//    EllipticArc        
//    MultiArc           
//    BezierCurve        
//    SplineCurve 
//    MultiCurve
//    Circle             
//    Ellipse            
//    MultiCircle        
//    MultiEllipse       
//    GeometryCollection 
//    Text               
//    MultiText          

    /// <summary>
	/// Summary description for GeometryType.
	/// </summary>
	[Serializable]
    public enum GeometryType
	{
        /// <summary>
        /// Unknown or unspecified geometry. May be used to represent empty
        /// geometry as in Shapefile format.
        /// </summary>
        None               = 0,

        /// <summary>
        /// Point geometry.
        /// </summary>
        Point              = 1,

        /// <summary>
        /// A line segment geometry.
        /// </summary>
        LineSegment        = 2,

        /// <summary>
        /// LineString or polyline geometry.
        /// </summary>
        LineString         = 3,

        /// <summary>
        /// LinearRing geometry.
        /// </summary>
        LinearRing         = 4,

        /// <summary>
        /// A triangular geometry.
        /// </summary>
        Triangle           = 5,

        /// <summary>
        /// Polygon geometry.
        /// </summary>
        Polygon            = 6,

        /// <summary>
        /// Multi-parts polygon geometry.
        /// </summary>
        MultiPolygon       = 7,

        /// <summary>
        /// Multi-parts point geometry.
        /// </summary>
        MultiPoint         = 8,

        /// <summary>
        /// Multi-parts LineString or polyline geometry.
        /// </summary>
        MultiLineString    = 9,

        /// <summary>
        /// GeometryCollection geometry or a collection of geometries.
        /// </summary>
        GeometryCollection = 10,

        /// <summary>
        /// Arc geometry.
        /// </summary>
        CircularArc        = 11,

        /// <summary>
        /// Arc geometry.
        /// </summary>
        EllipticArc        = 23,

        /// <summary>
        /// Text geometry.
        /// </summary>
        Text               = 12,

        /// <summary>
        /// Circle geometry.
        /// </summary>
        Circle             = 13,

        /// <summary>
        /// Ellipse geometry.
        /// </summary>
        Ellipse            = 14,
     
        /// <summary>
        /// Rectangle geometry.
        /// </summary>
        Rectangle          = 15,
     
        /// <summary>
        /// Rectangle geometry.
        /// </summary>
        RoundedRectangle   = 16,

        /// <summary>
        /// Multi-parts arc geometry.
        /// </summary>
        MultiArc           = 17,

        /// <summary>
        /// Multi-parts or (multi-lines) text geometry.
        /// </summary>
        MultiText          = 18,

        /// <summary>
        /// Multi-parts circle geometry.
        /// </summary>
        MultiCircle        = 19,

        /// <summary>
        /// Multi-parts ellipse geometry.
        /// </summary>
        MultiEllipse       = 20,
     
        /// <summary>
        /// Multi-parts rectangle geometry.
        /// </summary>
        MultiRectangle     = 21,
     
        /// <summary>
        /// Multi-parts rectangle geometry.
        /// </summary>
        MultiTriangle      = 22,

        /// <summary>
        /// Arc geometry.
        /// </summary>
        SplineCurve        = 24,

        /// <summary>
        /// Arc geometry.
        /// </summary>
        BezierCurve        = 25,

        /// <summary>
        /// Arc geometry.
        /// </summary>
        MultiCurve         = 26,

        /// <summary>
        /// This marks the beginning of user-defined geometries.
        /// </summary>
        Custom             = 50
    }
}
