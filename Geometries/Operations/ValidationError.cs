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
using iGeospatial.Geometries;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Operations
{
    /// <summary>
    /// 
    /// </summary>

	/// <summary> 
	/// Contains information about the nature and location of a Geometry
	/// validation error
	/// </summary>
	[Serializable]
    public sealed class ValidationError
	{                       
        #region Private Fields

		// these messages must synch up with the indexes above
		private static string[] errMsg = new string[]{
                "Topology Validation Error", 
                "Repeated Point", 
                "Hole lies outside shell", 
                "Holes are nested", 
                "Interior is disconnected", 
                "Self-intersection", 
                "Ring Self-intersection", 
                "Nested shells", 
                "Duplicate Rings", 
                "Too few points in geometry component", 
                "Invalid Coordinate",
                "Ring is not closed"
        };
		
		private ValidationErrorType errorType;
		private Coordinate pt;
        
        #endregion
		
        #region Constructors and Destructor
		
        public ValidationError(ValidationErrorType errorType, Coordinate pt)
		{
            if (pt == null)
            {
                throw new ArgumentNullException("pt");
            }

            this.errorType = errorType;
			this.pt        = pt.Clone();
		}

		public ValidationError(ValidationErrorType errorType)
            : this(errorType, null)
		{
		}
        
        #endregion
		
        #region Public Properties

		public Coordinate Coordinate
		{
			get
			{
				return pt;
			}
		}
			
		public ValidationErrorType ErrorType
		{
			get
			{
				return errorType;
			}
		}
			
		public string Message
		{
			get
			{
				return errMsg[(int)errorType];
			}
		}
        
        #endregion
		
        #region Public Methods

		public override string ToString()
		{
			return this.Message + " at or near point " + pt.ToString();
		}
        
        #endregion
	}
}