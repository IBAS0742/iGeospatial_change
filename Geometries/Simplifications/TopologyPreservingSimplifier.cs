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
using iGeospatial.Geometries.Editors;
using iGeospatial.Geometries.Visitors;

namespace iGeospatial.Geometries.Simplifications
{
    /// <summary> 
    /// Simplifies a geometry, ensuring that the result is a valid 
    /// geometry having the same dimension and number of components 
    /// as the input.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The simplification uses a maximum distance difference algorithm
    /// similar to the one used in the Douglas-Peucker algorithm.
    /// </para>
    /// In particular, if the input is an a real geometry
    /// (such as <see cref="Polygon"/> or <see cref="MultiPolygon"/> )
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The result has the same number of shells and holes (rings) as the input,
    /// in the same order
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The result rings touch at <c>no more</c> than the number of touching 
    /// point in the input (although they may touch at fewer points).
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [Serializable]
    public class TopologyPreservingSimplifier
	{
        #region Private Fields

		private Geometry              inputGeom;
		private TaggedLinesSimplifier lineSimplifier;
		private IDictionary           linestringMap;
        
        #endregion
		
        #region Constructors and Destructor

		public TopologyPreservingSimplifier(Geometry inputGeom)
		{
            this.lineSimplifier = new TaggedLinesSimplifier();
			this.inputGeom      = inputGeom;
		}
		
		public TopologyPreservingSimplifier(Geometry inputGeom, 
            double tolerance)
		{
            if (tolerance < 0.0)
                throw new ArgumentException("Tolerance must be non-negative");
				
            this.lineSimplifier = new TaggedLinesSimplifier(tolerance);
			this.inputGeom      = inputGeom;
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
                return lineSimplifier.DistanceTolerance;
            }

			set
			{
				if (value < 0.0)
					throw new ArgumentException("Tolerance must be non-negative");
				
                lineSimplifier.DistanceTolerance = value;
			}
		}
        
        #endregion
			
        #region Public Methods

		public Geometry Simplify()
		{
			linestringMap = new Hashtable();
			inputGeom.Apply(new LineStringMapBuilderFilter(this));
			
            lineSimplifier.Simplify(linestringMap.Values);

			Geometry result = 
                (new LineStringTransformer(this)).Transform(inputGeom);
			
            return result;
		}
			
		public static Geometry Simplify(Geometry geom, double tolerance)
		{
			TopologyPreservingSimplifier tss = 
                new TopologyPreservingSimplifier(geom, tolerance);
			
            return tss.Simplify();
		}
        
        #endregion
		
        #region LineStringTransformer Class

		private sealed class LineStringTransformer : GeometryTransformer
		{
            private TopologyPreservingSimplifier m_objSimplifier;

			public LineStringTransformer(
                TopologyPreservingSimplifier topoSimplifier)
			{
                if (topoSimplifier == null)
                {
                    throw new ArgumentNullException("topoSimplifier");
                }

                m_objSimplifier = topoSimplifier;
			}
			
            public TopologyPreservingSimplifier Simplifier
			{
				get
				{
					return m_objSimplifier;
				}
			}
				
			protected override ICoordinateList Transform(
                ICoordinateList coords, Geometry parent)
			{
                if (parent == null)
                {
                    throw new ArgumentNullException("parent");
                }

                GeometryType geomType = parent.GeometryType;

                if (geomType == GeometryType.LineString ||
                    geomType == GeometryType.LinearRing)
                {
					TaggedLineString taggedLine = 
                        (TaggedLineString)m_objSimplifier.linestringMap[parent];
					
                    return taggedLine.ResultCoordinates;
				}

				// for anything else (e.g. points) just copy the coordinates
				return base.Transform(coords, parent);
			}
		}
        
        #endregion
		
        #region LineStringMapBuilderFilter Class

		private sealed class LineStringMapBuilderFilter : IGeometryComponentVisitor
		{
            private TopologyPreservingSimplifier m_objSimplifier;
			
            public LineStringMapBuilderFilter(
                TopologyPreservingSimplifier topoSimplifier)
			{
                if (topoSimplifier == null)
                {
                    throw new ArgumentNullException("topoSimplifier");
                }

                m_objSimplifier = topoSimplifier;
            }

			public TopologyPreservingSimplifier Simplifier
			{
				get
				{
					return m_objSimplifier;
				}
			}

			public void Visit(Geometry geometry)
			{
                if (geometry == null)
                {
                    throw new ArgumentNullException("geometry");
                }

                GeometryType geomType = geometry.GeometryType;

                if (geomType == GeometryType.LinearRing)
				{
					TaggedLineString taggedLine = 
                        new TaggedLineString((LineString)geometry, 4);

					m_objSimplifier.linestringMap[geometry] = taggedLine;
				}
				else if (geomType == GeometryType.LineString)
				{
					TaggedLineString taggedLine = 
                        new TaggedLineString((LineString)geometry, 2);
					
                    m_objSimplifier.linestringMap[geometry] = taggedLine;
				}
			}
		}
        
        #endregion
	}
}