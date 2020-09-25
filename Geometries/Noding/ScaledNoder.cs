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
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Noding
{
	/// <summary> 
	/// Wraps a <see cref="INoder"/> and transforms its input into the 
	/// integer domain.
	/// </summary>
	/// <remarks>
	/// This is intended for use with Snap-Rounding noders, which typically 
	/// are only intended to work in the integer domain.
	/// <para>
	/// Offsets can be provided to increase the number of digits of 
	/// available precision.
	/// </para>
	/// </remarks>
	internal class ScaledNoder : INoder
	{
        #region Private Fields

		private INoder noder;
		private double scaleFactor;
		private double offsetX;
		private double offsetY;
		private bool   isScaled;
        
        #endregion
		
        #region Constructors and Destructor

		public ScaledNoder(INoder noder, double scaleFactor) 
            : this(noder, scaleFactor, 0, 0)
		{
		}
		
		public ScaledNoder(INoder noder, double scaleFactor, 
            double offsetX, double offsetY)
		{
			this.noder = noder;
			this.scaleFactor = scaleFactor;

            this.offsetX = offsetX;
            this.offsetY = offsetY;

			// no need to scale if input precision is already integral
			isScaled = !IntegerPrecision;
		}

        #endregion
		
        #region Public Properties

		public bool IntegerPrecision
		{
			get
			{
				return (scaleFactor == 1.0);
			}
		}
			
		public IList NodedSubstrings
		{
			get
			{
				IList splitSS = noder.NodedSubstrings;
				
                if (isScaled)
					Rescale(splitSS);

				return splitSS;
			}
		}
        
        #endregion
		
        #region Public Methods

		public void ComputeNodes(IList inputSegStrings)
		{
			IList intSegStrings = inputSegStrings;
			
            if (isScaled)
				intSegStrings = Scale(inputSegStrings);

			noder.ComputeNodes(intSegStrings);
		}
        
        #endregion
		
        #region Private Methods

		private IList Scale(IList segStrings)
		{
			return CollectionUtil.Transform(segStrings, 
                new ScaleFunction(this));
		}
		
		private ICoordinateList Scale(ICoordinateList pts)
		{
            int nCount = pts.Count;

			CoordinateCollection roundPts = new CoordinateCollection(nCount);
			for (int i = 0; i < nCount; i++)
			{
				Coordinate coord = new Coordinate((long)MathUtil.Round(
                    (pts[i].X - offsetX) * scaleFactor), 
                    (long)MathUtil.Round((pts[i].Y - offsetY) * scaleFactor));

                roundPts.Add(coord);
			}

			ICoordinateList roundPtsNoDup = 
                RemoveRepeatedPoints(roundPts);

			return roundPtsNoDup;
		}
		
		private void Rescale(IList segStrings)
		{
			CollectionUtil.Apply(segStrings, new RescaleFunction(this));
		}
		
		private void Rescale(ICoordinateList pts)
		{
            int nCount = pts.Count;

			for (int i = 0; i < nCount; i++)
			{
				pts[i].X = pts[i].X / scaleFactor + offsetX;
				pts[i].Y = pts[i].Y / scaleFactor + offsetY;
			}
		}

        /// <summary> Returns whether equals returns true for any two consecutive Coordinates 
        /// in the given array.
        /// </summary>
        private static bool HasRepeatedCoordinates(ICoordinateList coord)
        {
            for (int i = 1; i < coord.Count; i++)
            {
                if (coord[i - 1].Equals(coord[i]))
                {
                    return true;
                }
            }
            return false;
        }
				
        /// <summary> If the coordinate array argument has repeated points,
        /// constructs a new array containing no repeated points.
        /// Otherwise, returns the argument.
        /// </summary>
        /// <seealso cref="HasRepeatedCoordinates(Coordinate[])">
        /// </seealso>
        private static ICoordinateList RemoveRepeatedPoints(ICoordinateList coord)
        {
            if (!HasRepeatedCoordinates(coord))
                return coord;
            CoordinateCollection coordList = new CoordinateCollection();

            for (int i = 0; i < coord.Count; i++)
            {
                coordList.Add(coord[i], false);
            }

            return coordList;
        }
        
        #endregion

        #region ScaleFunction Class

        private class ScaleFunction : CollectionUtil.Function
        {
            private ScaledNoder m_objNoder;

            public ScaleFunction(ScaledNoder noder)
            {
                m_objNoder = noder;
            }

            public ScaledNoder Noder
            {
                get
                {
                    return m_objNoder;
                }
            }

            public object Execute(object obj)
            {
                SegmentString ss = (SegmentString) obj;

                return new SegmentString(m_objNoder.Scale(ss.Coordinates), 
                    ss.Data);
            }
        }
        
        #endregion

        #region RescaleFunction Class

        private class RescaleFunction : CollectionUtil.Function
        {
            private ScaledNoder m_objNoder;

            public RescaleFunction(ScaledNoder noder)
            {
                m_objNoder = noder;
            }

            public ScaledNoder Noder
            {
                get
                {
                    return m_objNoder;
                }
            }

            public object Execute(object obj)
            {
                SegmentString ss = (SegmentString) obj;

                m_objNoder.Rescale(ss.Coordinates);
                
                return null;
            }
        }
        
        #endregion
	}
}