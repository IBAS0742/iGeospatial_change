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

using iGeospatial.Exceptions;
using iGeospatial.Coordinates;
using iGeospatial.Geometries.Operations.Precision;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Provides versions of <see cref="Geometry"/> spatial functions which use enhanced 
	/// precision techniques to reduce the likelihood of robustness problems.
	/// </summary>
	public sealed class EnhancedPrecisionOp
	{
        private EnhancedPrecisionOp()
        {
        }

		/// <summary> 
		/// Computes the set-theoretic intersection of two 
		/// <see cref="Geometry"/>s, using enhanced precision.
		/// </summary>
		/// <param name="geom0">The first <see cref="Geometry"/>.</param>
		/// <param name="geom1">The second <see cref="Geometry"/>.</param>
		/// <returns> 
		/// The Geometry representing the set-theoretic intersection of 
		/// the input Geometries.
		/// </returns>
		public static Geometry Intersection(Geometry geom0, Geometry geom1)
		{
			Exception originalEx;
			try
			{
				Geometry result = geom0.Intersection(geom1);
				return result;
			}
			catch (Exception ex)
			{
				originalEx = ex;
			}

			// If we are here, the original op encountered a precision 
			// problem (or some other problem).  Retry the operation with
			// enhanced precision to see if it succeeds
			try
			{
				CommonBitsOp cbo = new CommonBitsOp(true);
				Geometry resultEP = cbo.Intersection(geom0, geom1);
				// check that result is a valid geometry after the reshift 
                // to orginal precision
				if (!resultEP.IsValid)
					throw originalEx;

				return resultEP;
			}
			catch
			{
				throw originalEx;
			}
		}

		/// <summary> 
		/// Computes the set-theoretic union of two <see cref="Geometry"/>s, 
		/// using enhanced precision.
		/// </summary>
		/// <param name="geom0">The first <see cref="Geometry"/>.</param>
		/// <param name="geom1">The second <see cref="Geometry"/>.</param>
		/// <returns> 
		/// The <see cref="Geometry"/> representing the set-theoretic union 
		/// of the input Geometries.
		/// </returns>
		public static Geometry Union(Geometry geom0, Geometry geom1)
		{
			Exception originalEx;
			try
			{
				Geometry result = geom0.Union(geom1);
				return result;
			}
			catch (Exception ex)
			{
				originalEx = ex;
			}

            // If we are here, the original op encountered a precision problem
			// (or some other problem).  Retry the operation with
			// enhanced precision to see if it succeeds
			try
			{
				CommonBitsOp cbo = new CommonBitsOp(true);
				Geometry resultEP = cbo.Union(geom0, geom1);
				// check that result is a valid geometry after the reshift 
                // to orginal precision
				if (!resultEP.IsValid)
					throw originalEx;

				return resultEP;
			}
			catch
			{
				throw originalEx;
			}
		}

		/// <summary> 
		/// Computes the set-theoretic difference of two 
		/// <see cref="Geometry"/>s, using enhanced precision.
		/// </summary>
		/// <param name="geom0">The first <see cref="Geometry"/>.</param>
		/// <param name="geom1">The second <see cref="Geometry"/>.</param>
		/// <returns> 
		/// The <see cref="Geometry"/> representing the set-theoretic 
		/// difference of the input Geometries.
		/// </returns>
		public static Geometry Difference(Geometry geom0, Geometry geom1)
		{
			Exception originalEx;
			try
			{
				Geometry result = geom0.Difference(geom1);
				return result;
			}
			catch (Exception ex)
			{
				originalEx = ex;
			}

            // If we are here, the original op encountered a precision problem
			// (or some other problem).  Retry the operation with
			// enhanced precision to see if it succeeds
			try
			{
				CommonBitsOp cbo = new CommonBitsOp(true);
				Geometry resultEP = cbo.Difference(geom0, geom1);
				// check that result is a valid geometry after the reshift 
                // to orginal precision
				if (!resultEP.IsValid)
					throw originalEx;

				return resultEP;
			}
			catch
			{
				throw originalEx;
			}
		}

		/// <summary> 
		/// Computes the set-theoretic symmetric difference of two 
		/// <see cref="Geometry"/>s, using enhanced precision.
		/// </summary>
		/// <param name="geom0">The first <see cref="Geometry"/>.</param>
		/// <param name="geom1">The second <see cref="Geometry"/>.</param>
		/// <returns> 
		/// The <see cref="Geometry"/> representing the set-theoretic 
		/// symmetric difference of the input Geometries.
		/// </returns>
		public static Geometry SymmetricDifference(Geometry geom0, 
            Geometry geom1)
		{
			Exception originalEx;
			try
			{
				Geometry result = geom0.SymmetricDifference(geom1);
				return result;
			}
			catch (Exception ex)
			{
				originalEx = ex;
			}

            // If we are here, the original op encountered a precision problem
			// (or some other problem).  Retry the operation with
			// enhanced precision to see if it succeeds
			try
			{
				CommonBitsOp cbo = new CommonBitsOp(true);
				Geometry resultEP = cbo.SymmetricDifference(geom0, geom1);
				// check that result is a valid geometry after the reshift 
                // to orginal precision
				if (!resultEP.IsValid)
					throw originalEx;

				return resultEP;
			}
			catch
			{
				throw originalEx;
			}
		}

		/// <summary> 
		/// Computes the buffer of a <see cref="Geometry"/>, using enhanced precision.
		/// This method should no longer be necessary, since the buffer algorithm
		/// now is highly robust.
		/// </summary>
		/// <param name="geom0">The first <see cref="Geometry"/>.</param>
		/// <param name="distance">The buffer distance.</param>
		/// <returns> 
		/// The <see cref="Geometry"/> representing the buffer of the 
		/// input <see cref="Geometry"/>.
		/// </returns>
		public static Geometry Buffer(Geometry geom, double distance)
		{
			Exception originalEx;
			try
			{
				Geometry result = geom.Buffer(distance);
				return result;
			}
			catch (Exception ex)
			{
				originalEx = ex;
			}

            // If we are here, the original op encountered a precision problem
			// (or some other problem).  Retry the operation with
			// enhanced precision to see if it succeeds
			try
			{
				CommonBitsOp cbo = new CommonBitsOp(true);
				Geometry resultEP = cbo.Buffer(geom, distance);
				// check that result is a valid geometry after the reshift 
                // to orginal precision
				if (!resultEP.IsValid)
					throw originalEx;

				return resultEP;
			}
			catch
			{
				throw originalEx;
			}
		}
	}
}