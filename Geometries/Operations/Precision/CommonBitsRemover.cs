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

namespace iGeospatial.Geometries.Operations.Precision
{
	/// <summary> 
	/// Allow computing and removing common mantissa bits from 
	/// one or more geometries.
	/// </summary>
	internal sealed class CommonBitsRemover
	{
        #region Private Fields

        private Coordinate commonCoord;
        private CommonCoordinateVisitor ccFilter;
        
        #endregion
		
        #region Constructors and Destructor
        
        public CommonBitsRemover()
        {
            ccFilter = new CommonCoordinateVisitor(this);
        }
        
        #endregion
		
        #region Public Properties

		/// <summary> 
		/// The common bits of the Coordinates in the supplied Geometries.
		/// </summary>
		public Coordinate CommonCoordinate
		{
			get
			{
				return commonCoord;
			}
		}
        
        #endregion

        #region Public Methods

		/// <summary> 
		/// Add a geometry to the set of geometries whose common bits are
		/// being computed.  After this method has executed the
		/// common coordinate reflects the common bits of all added
		/// geometries.
		/// </summary>
		/// <param name="geom">a Geometry to test for common bits.</param>
		public void Add(Geometry geom)
		{
			geom.Apply(ccFilter);
			commonCoord = ccFilter.CommonCoordinate;
		}
		
		/// <summary> Removes the common coordinate bits from a Geometry.
		/// The coordinates of the Geometry are changed.
		/// 
		/// </summary>
		/// <param name="geom">
		/// the Geometry from which to remove the common coordinate bits
		/// </param>
		/// <returns> the shifted Geometry
		/// </returns>
		public Geometry RemoveCommonBits(Geometry geom)
		{
			if (commonCoord.X == 0.0 && commonCoord.Y == 0.0)
				return geom;

			Coordinate invCoord = new Coordinate(commonCoord);
			invCoord.X = -invCoord.X;
			invCoord.Y = -invCoord.Y;
			Translater trans = new Translater(this, invCoord);
			geom.Apply(trans);
			geom.Changed();

			return geom;
		}
		
		/// <summary> Adds the common coordinate bits back into a Geometry.
		/// The coordinates of the Geometry are changed.
		/// 
		/// </summary>
		/// <param name="geom">the Geometry to which to Add the common coordinate bits
		/// </param>
		/// <returns> the shifted Geometry
		/// </returns>
		public void AddCommonBits(Geometry geom)
		{
			Translater trans = new Translater(this, commonCoord);
			geom.Apply(trans);
			geom.Changed();
		}
        
        #endregion
		
        #region CommonCoordinateVisitor Class

		internal class CommonCoordinateVisitor : ICoordinateVisitor
		{
            private CommonBitsRemover m_objBitsRemover;
            private CommonBits commonBitsX;
            private CommonBits commonBitsY;
			
			public CommonCoordinateVisitor(CommonBitsRemover bitsRemover)
			{
                this.m_objBitsRemover = bitsRemover;

                commonBitsX = new CommonBits();
                commonBitsY = new CommonBits();
			}
			
            public Coordinate CommonCoordinate
			{
				get
				{
					return new Coordinate(commonBitsX.Common, commonBitsY.Common);
				}
			}

			public CommonBitsRemover BitsRemover
			{
				get
				{
					return m_objBitsRemover;
				}
			}

			public void  Visit(Coordinate coord)
			{
                if (coord == null)
                {
                    throw new ArgumentNullException("coord");
                }

                commonBitsX.Add(coord.X);
				commonBitsY.Add(coord.Y);
			}
		}
        
        #endregion
		
        #region Translater Class

		internal class Translater : ICoordinateVisitor
		{
            private CommonBitsRemover m_objBitsRemover;

            internal Coordinate trans;
			
            public Translater(CommonBitsRemover bitsRemover, Coordinate trans)
            {
                this.m_objBitsRemover = bitsRemover;

                this.trans = trans;
            }

			public CommonBitsRemover BitsRemover
			{
				get
				{
					return m_objBitsRemover;
				}
			}

			public void Visit(Coordinate coord)
			{
                if (coord == null)
                {
                    throw new ArgumentNullException("coord");
                }

                coord.X += trans.X;
				coord.Y += trans.Y;
			}
		}
        
        #endregion
	}
}