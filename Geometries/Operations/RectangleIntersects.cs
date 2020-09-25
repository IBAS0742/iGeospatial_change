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

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Visitors;
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.Operations.Predicate;
	
namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Optimized implementation of spatial predicate "intersects"
	/// for cases where the first <see cref="Geometry"/> is a rectangle.
	/// </summary>
	/// <remarks>
	/// As a further optimization, this class can be used directly 
	/// to test many geometries against a single rectangle.
	/// </remarks>
	[Serializable]
    public class RectangleIntersects
	{
        #region Private Fields

		/// <summary> 
		/// Crossover size at which brute-force intersection scanning
		/// is slower than indexed intersection detection.
		/// Must be determined empirically.  Should err on the
		/// safe side by making value smaller rather than larger.
		/// </summary>      
		public const int MaxScanSegments = 200;
		
        private Polygon rectangle;
        private Envelope rectEnv;
        
        #endregion
		
        #region Constructors and Destructor

		/// <summary> Create a new intersects computer for a rectangle.
		/// 
		/// </summary>
		/// <param name="rectangle">a rectangular geometry
		/// </param>
		public RectangleIntersects(Polygon rectangle)
		{
            if (rectangle == null)
            {
                throw new ArgumentNullException("rectangle");
            }

            this.rectangle = rectangle;
			rectEnv        = rectangle.Bounds;
		}
        
        #endregion
		
        #region Public Methods

        public static bool Intersects(Polygon rectangle, Geometry b)
        {
            RectangleIntersects rp = new RectangleIntersects(rectangle);
            return rp.Intersects(b);
        }
		
		public bool Intersects(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            if (!rectEnv.Intersects(geometry.Bounds))
				return false;
			// test envelope relationships
			EnvelopeIntersectsVisitor visitor = 
                new EnvelopeIntersectsVisitor(rectEnv);

			visitor.ApplyTo(geometry);
			
            if (visitor.Intersects())
				return true;
			
			// test if any rectangle corner is contained in the target
			ContainsPointVisitor ecpVisitor = 
                new ContainsPointVisitor(rectangle);

			ecpVisitor.ApplyTo(geometry);
			if (ecpVisitor.ContainsPoint())
				return true;
			
			// test if any lines intersect
			LineIntersectsVisitor liVisitor = 
                new LineIntersectsVisitor(rectangle);

			liVisitor.ApplyTo(geometry);
			if (liVisitor.Intersects())
				return true;
			
			return false;
		}
        
        #endregion
	}
}