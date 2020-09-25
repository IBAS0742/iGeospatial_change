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
using iGeospatial.Geometries.Editors;

namespace iGeospatial.Geometries.Simplifications
{
	/// <summary> 
	/// Simplifies a <see cref="Geometry"/> using the standard 
	/// Douglas-Peucker algorithm.
	/// </summary>
	/// <remarks>
	/// Ensures that any polygonal geometries returned are valid. Simple 
	/// lines are not guaranteed to remain simple after simplification.
	/// <para>
	/// Note that in general D-P does not preserve topology -
	/// e.g. polygons can be split, collapse to lines or disappear
	/// holes can be created or disappear, and lines can cross.
	/// </para>
	/// To simplify geometry while preserving topology use 
	/// <see cref="TopologyPreservingSimplifier"/>. (However, using D-P is 
	/// significantly faster).
	/// </remarks>
	[Serializable]
    public class DouglasPeuckerSimplifier
	{
        #region Private Fields

        private Geometry inputGeom;
        private double   distanceTolerance;
        
        #endregion
		
        #region Constructors and Destructor

        public DouglasPeuckerSimplifier(Geometry inputGeom)
        {
            if (inputGeom == null)
            {
                throw new ArgumentNullException("inputGeom");
            }

            this.inputGeom = inputGeom;
        }
		
        public DouglasPeuckerSimplifier(Geometry inputGeom, double tolerance)
        {
            if (inputGeom == null)
            {
                throw new ArgumentNullException("inputGeom");
            }

            if (tolerance < 0.0)
                throw new ArgumentException("Tolerance must be non-negative");
				
            this.inputGeom         = inputGeom;
            this.distanceTolerance = tolerance;
        }
        
        #endregion
		
        #region Public Properties

        /// <summary> 
		/// Gets or sets the distance tolerance for the simplification.
		/// </summary>
		/// <value>
		/// A number specifying the approximation tolerance to use.
		/// </value>
		/// <remarks>
		/// All vertices in the simplified geometry will be within this
		/// distance of the original geometry.
		/// The tolerance value must be non-negative.  A tolerance value
		/// of zero is effectively a no-op.
		/// </remarks>
		public double DistanceTolerance
		{
            get
            {
                return this.distanceTolerance;
            }

			set
			{
				if (value < 0.0)
					throw new ArgumentException("Tolerance must be non-negative");
				
                this.distanceTolerance = value;
			}
		}
        
        #endregion
		
        #region Public Methods

		public static Geometry Simplify(Geometry geom, double tolerance)
		{
			DouglasPeuckerSimplifier tss = 
                new DouglasPeuckerSimplifier(geom, tolerance);
			
            return tss.Simplify();
		}
			
        public Geometry Simplify()
        {
            return (new DPTransformer(this)).Transform(inputGeom);
        }
        
        #endregion
		
        #region DPTransformer Class

		internal class DPTransformer : GeometryTransformer
		{
            private DouglasPeuckerSimplifier m_objSimplifier;

            public DPTransformer(DouglasPeuckerSimplifier dpSimplier)
			{
                m_objSimplifier = dpSimplier;
			}

            public DouglasPeuckerSimplifier Simplifier
            {
                get
                {
                    return m_objSimplifier;
                }				
            }
			
            protected override ICoordinateList 
                Transform(ICoordinateList coords, Geometry parent)
			{
				Coordinate[] inputPts = coords.ToArray();

				Coordinate[] newPts   = 
                    DouglasPeuckerLineSimplifier.Simplify(inputPts, 
                    m_objSimplifier.DistanceTolerance);
				
                return new CoordinateCollection(newPts);
			}
			
			protected override Geometry Transform(Polygon geom, 
                Geometry parent)
			{
				Geometry roughGeom = base.Transform(geom, parent);
                // don't try and correct if the parent is going to do this
                if (parent.GeometryType == GeometryType.MultiPolygon)
                {
                    return roughGeom;
                }

                return CreateValidArea(roughGeom);
			}
			
			protected override Geometry Transform(MultiPolygon geom, 
                Geometry parent)
			{
				Geometry roughGeom = base.Transform(geom, parent);
				
                return CreateValidArea(roughGeom);
			}
			
			/// <summary> 
			/// Creates a valid area geometry from one that possibly has
			/// bad topology (i.e. self-intersections).
			/// </summary>
			/// <param name="roughAreaGeom">
			/// An area geometry possibly containing self-intersections.
			/// </param>
			/// <returns>A valid area geometry.</returns>
			/// <remarks>
			/// Since buffer can handle invalid topology, but always returns
			/// valid geometry, constructing a 0-width buffer "corrects" the
			/// topology.
			/// Note this only works for area geometries, since buffer always returns
			/// areas.  This also may return empty geometries, if the input
			/// has no actual area.
			/// </remarks>
			private Geometry CreateValidArea(Geometry roughAreaGeom)
			{
				return roughAreaGeom.Buffer(0.0);
			}
		}
       
        #endregion
	}
}