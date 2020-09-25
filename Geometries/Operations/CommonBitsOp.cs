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
	/// Provides versions of <see cref="Geometry"/> spatial functions 
	/// which use common bit removal to reduce the likelihood of 
	/// robustness problems.
	/// </summary>
	/// <remarks>
	/// In the current implementation no rounding is performed on the
	/// reshifted result geometry, which means that it is possible
	/// that the returned <see cref="Geometry"/> is invalid.
	/// <para>
	/// Client classes should check the validity of the returned 
	/// result themselves.
	/// </para>
	/// </remarks>
	public sealed class CommonBitsOp
	{
        #region Private Fields

		private bool              m_bToOriginalPrecision;
		private CommonBitsRemover m_objBitsRemover;
        
        #endregion
		
        #region Constructors and Destructor
		
        /// <summary> 
		/// Creates a new instance of class, which reshifts result 
		/// <see cref="Geometry"/> instances.
		/// </summary>
		public CommonBitsOp() : this(true)
		{
		}
		
		/// <summary> 
		/// Creates a new instance of class, specifying whether
		/// the result <see cref="Geometry"/> instances should be reshifted.
		/// </summary>
		/// <param name="returnToOriginalPrecision">
		/// </param>
		public CommonBitsOp(bool returnToOriginalPrecision)
		{
			m_bToOriginalPrecision = returnToOriginalPrecision;
		}
        
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// Computes the set-theoretic intersection of two <see cref="Geometry"/> instances, using enhanced precision.</summary>
		/// <param name="geom0">the first Geometry
		/// </param>
		/// <param name="geom1">the second Geometry
		/// </param>
		/// <returns> the Geometry representing the set-theoretic intersection of the input Geometries.
		/// </returns>
		public Geometry Intersection(Geometry geom0, Geometry geom1)
		{
            if (geom0 == null)
            {
                throw new ArgumentNullException("geom0");
            }
            if (geom1 == null)
            {
                throw new ArgumentNullException("geom1");
            }

            Geometry[] geom = RemoveCommonBits(geom0, geom1);

			return ComputeResultPrecision(geom[0].Intersection(geom[1]));
		}
		
		/// <summary> Computes the set-theoretic union of two <see cref="Geometry"/> instances, using enhanced precision.</summary>
		/// <param name="geom0">the first Geometry
		/// </param>
		/// <param name="geom1">the second Geometry
		/// </param>
		/// <returns> the Geometry representing the set-theoretic union of the input Geometries.
		/// </returns>
		public Geometry Union(Geometry geom0, Geometry geom1)
		{
            if (geom0 == null)
            {
                throw new ArgumentNullException("geom0");
            }
            if (geom1 == null)
            {
                throw new ArgumentNullException("geom1");
            }

            Geometry[] geom = RemoveCommonBits(geom0, geom1);

			return ComputeResultPrecision(geom[0].Union(geom[1]));
		}
		
		/// <summary> 
		/// Computes the set-theoretic difference of two <see cref="Geometry"/> 
		/// instances, using enhanced precision.
		/// </summary>
		/// <param name="geom0">the first Geometry
		/// </param>
		/// <param name="geom1">the second Geometry, to be subtracted from the first
		/// </param>
		/// <returns>
		/// The Geometry representing the set-theoretic difference of the input geometries.
		/// </returns>
		public Geometry Difference(Geometry geom0, Geometry geom1)
		{
			Geometry[] geom = RemoveCommonBits(geom0, geom1);

			return ComputeResultPrecision(geom[0].Difference(geom[1]));
		}
		
		/// <summary> 
		/// Computes the set-theoretic symmetric difference of two geometries,
		/// using enhanced precision.
		/// </summary>
		/// <param name="geom0">the first Geometry
		/// </param>
		/// <param name="geom1">the second Geometry
		/// </param>
		/// <returns>
		/// The Geometry representing the set-theoretic symmetric difference of the 
		/// input geometries.
		/// </returns>
		public Geometry SymmetricDifference(Geometry geom0, Geometry geom1)
		{
			Geometry[] geom = RemoveCommonBits(geom0, geom1);

			return ComputeResultPrecision(geom[0].SymmetricDifference(geom[1]));
		}
		
		/// <summary> 
		/// Computes the buffer a geometry, using enhanced precision.
		/// </summary>
		/// <param name="geom0">The Geometry to buffer.</param>
		/// <param name="distance">The buffer distance.</param>
		/// <returns>
		/// The Geometry representing the buffer of the input Geometry.
		/// </returns>
		public Geometry Buffer(Geometry geom0, double distance)
		{
			Geometry geom = RemoveCommonBits(geom0);

			return ComputeResultPrecision(geom.Buffer(distance));
		}
        
        #endregion

        #region Public Static Methods

        /// <summary> 
        /// Computes the set-theoretic intersection of two <see cref="Geometry"/> instances, using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first geometry.
        /// </param>
        /// <param name="geom1">The second geometry.
        /// </param>
        /// <returns> 
        /// The <see cref="Geometry"/> representing the 
        /// set-theoretic intersection of the input geometries.
        /// </returns>
        public static Geometry GetIntersection(Geometry geom0, Geometry geom1)
        {
            if (geom0 == null)
            {
                throw new ArgumentNullException("geom0");
            }
            if (geom1 == null)
            {
                throw new ArgumentNullException("geom1");
            }

            Exception originalEx;
            try
            {
                Geometry result = geom0.Intersection(geom1);
                return result;
            }
            catch (GeometryException ex)
            {
                // If we are here, the original op encountered a precision problem
                // (or some other problem).  Retry the operation with
                // enhanced precision to see if it succeeds

                originalEx = ex;
            }
            try
            {
                CommonBitsOp cbo = new CommonBitsOp(true);
                Geometry resultEP = cbo.Intersection(geom0, geom1);
                // check that result is a valid geometry after the reshift to orginal precision
                if (!resultEP.IsValid)
                    throw originalEx;

                return resultEP;
            }
            catch (GeometryException ex2)
            {
                ExceptionManager.Publish(ex2);

                throw originalEx;
            }
        }

        /// <summary> 
        /// Computes the set-theoretic union of two <see cref="Geometry"/> instances, using enhanced precision.
        /// </summary>
        /// <param name="geom0">the first Geometry
        /// </param>
        /// <param name="geom1">the second Geometry
        /// </param>
        /// <returns> 
        /// the Geometry representing the set-theoretic union of the input Geometries.
        /// </returns>
        public static Geometry GetUnion(Geometry geom0, Geometry geom1)
        {
            if (geom0 == null)
            {
                throw new ArgumentNullException("geom0");
            }
            if (geom1 == null)
            {
                throw new ArgumentNullException("geom1");
            }

            Exception originalEx;
            try
            {
                Geometry result = geom0.Union(geom1);
                return result;
            }
            catch (GeometryException ex)
            {
                ExceptionManager.Publish(ex);

                originalEx = ex;
            }
            /*
            * If we are here, the original op encountered a precision problem
            * (or some other problem).  Retry the operation with
            * enhanced precision to see if it succeeds
            */
            try
            {
                CommonBitsOp cbo  = new CommonBitsOp(true);
                Geometry resultEP = cbo.Union(geom0, geom1);
                // check that result is a valid geometry after the reshift to orginal precision
                if (!resultEP.IsValid)
                    throw originalEx;
                return resultEP;
            }
            catch (GeometryException ex2)
            {
                ExceptionManager.Publish(ex2);

                throw originalEx;
            }
        }

        /// <summary> 
        /// Computes the set-theoretic difference of two <see cref="Geometry"/> instances, using enhanced precision.
        /// </summary>
        /// <param name="geom0">the first Geometry
        /// </param>
        /// <param name="geom1">the second Geometry
        /// </param>
        /// <returns> 
        /// the Geometry representing the set-theoretic difference of the input Geometries.
        /// </returns>
        public static Geometry GetDifference(Geometry geom0, Geometry geom1)
        {
            if (geom0 == null)
            {
                throw new ArgumentNullException("geom0");
            }
            if (geom1 == null)
            {
                throw new ArgumentNullException("geom1");
            }

            Exception originalEx;
            try
            {
                Geometry result = geom0.Difference(geom1);
                return result;
            }
            catch (GeometryException ex)
            {
                ExceptionManager.Publish(ex);

                originalEx = ex;
            }
            /*
            * If we are here, the original op encountered a precision problem
            * (or some other problem).  Retry the operation with
            * enhanced precision to see if it succeeds
            */
            try
            {
                CommonBitsOp cbo = new CommonBitsOp(true);
                Geometry resultEP = cbo.Difference(geom0, geom1);
                // check that result is a valid geometry after the reshift to orginal precision
                if (!resultEP.IsValid)
                    throw originalEx;
                return resultEP;
            }
            catch (GeometryException ex2)
            {
                ExceptionManager.Publish(ex2);

                throw originalEx;
            }
        }

        /// <summary> 
        /// Computes the set-theoretic symmetric difference of two <see cref="Geometry"/> instances, using enhanced precision.</summary>
        /// <param name="geom0">the first Geometry
        /// </param>
        /// <param name="geom1">the second Geometry
        /// </param>
        /// <returns> the Geometry representing the set-theoretic symmetric difference of the input Geometries.
        /// </returns>
        public static Geometry GetSymmetricDifference(Geometry geom0, Geometry geom1)
        {
            if (geom0 == null)
            {
                throw new ArgumentNullException("geom0");
            }
            if (geom1 == null)
            {
                throw new ArgumentNullException("geom1");
            }

            Exception originalEx;
            try
            {
                Geometry result = geom0.SymmetricDifference(geom1);
                return result;
            }
            catch (GeometryException ex)
            {
                ExceptionManager.Publish(ex);

                originalEx = ex;
            }
            /*
            * If we are here, the original op encountered a precision problem
            * (or some other problem).  Retry the operation with
            * enhanced precision to see if it succeeds
            */
            try
            {
                CommonBitsOp cbo = new CommonBitsOp(true);
                Geometry resultEP = cbo.SymmetricDifference(geom0, geom1);
                // check that result is a valid geometry after the reshift to orginal precision
                if (!resultEP.IsValid)
                    throw originalEx;
                return resultEP;
            }
            catch (GeometryException ex2)
            {
                ExceptionManager.Publish(ex2);

                throw originalEx;
            }
        }

        /// <summary> Computes the Buffer of a Geometry, using enhanced precision.
        /// This method should no longer be necessary, since the Buffer algorithm
        /// now is highly robust.
        /// 
        /// </summary>
        /// <param name="geom0">the first Geometry
        /// </param>
        /// <param name="distance">the Buffer Distance
        /// </param>
        /// <returns> the Geometry representing the Buffer of the input Geometry.
        /// </returns>
        public static Geometry GetBuffer(Geometry geometry, double distance)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            Exception originalEx;
            try
            {
                Geometry result = geometry.Buffer(distance);

                return result;
            }
            catch (GeometryException ex)
            {
                ExceptionManager.Publish(ex);

                originalEx = ex;
            }
            /*
            * If we are here, the original op encountered a precision problem
            * (or some other problem).  Retry the operation with
            * enhanced precision to see if it succeeds
            */
            try
            {
                CommonBitsOp cbo  = new CommonBitsOp(true);
                Geometry resultEP = cbo.Buffer(geometry, distance);
                // check that result is a valid geometry after the reshift to orginal precision
                if (!resultEP.IsValid)
                    throw originalEx;

                return resultEP;
            }
            catch (GeometryException ex2)
            {
                ExceptionManager.Publish(ex2);

                throw originalEx;
            }
        }
        
        #endregion
		
        #region Private Methods

		/// <summary> 
		/// If required, returning the result to the orginal precision if required.
		/// <para>
		/// In this current implementation, no rounding is performed on the
		/// reshifted result geometry, which means that it is possible
		/// that the returned Geometry is invalid.
		/// </para>
		/// </summary>
		/// <param name="result">the result Geometry to modify
		/// </param>
		/// <returns> the result Geometry with the required precision
		/// </returns>
		private Geometry ComputeResultPrecision(Geometry result)
		{
			if (m_bToOriginalPrecision)
				m_objBitsRemover.AddCommonBits(result);

			return result;
		}
		
		/// <summary> 
		/// Computes a copy of the input Geometry with the calculated common bits
		/// removed from each coordinate.
		/// </summary>
		/// <param name="geom0">the Geometry to remove common bits from
		/// </param>
		/// <returns> a copy of the input Geometry with common bits removed
		/// </returns>
		private Geometry RemoveCommonBits(Geometry geom0)
		{
			m_objBitsRemover = new CommonBitsRemover();
			m_objBitsRemover.Add(geom0);
			Geometry geom = m_objBitsRemover.RemoveCommonBits((Geometry) geom0.Clone());

			return geom;
		}
		
		/// <summary> 
		/// Computes a copy of each input <see cref="Geometry"/> instances with the 
		/// calculated common bits removed from each coordinate.
		/// </summary>
		/// <param name="geom0">a Geometry to remove common bits from
		/// </param>
		/// <param name="geom1">a Geometry to remove common bits from
		/// </param>
		/// <returns> an array containing copies
		/// of the input Geometry's with common bits removed
		/// </returns>
		private Geometry[] RemoveCommonBits(Geometry geom0, Geometry geom1)
		{
			m_objBitsRemover = new CommonBitsRemover();
			m_objBitsRemover.Add(geom0);
			m_objBitsRemover.Add(geom1);

			Geometry[] geom = new Geometry[2];
			geom[0] = m_objBitsRemover.RemoveCommonBits((Geometry)geom0.Clone());
			geom[1] = m_objBitsRemover.RemoveCommonBits((Geometry)geom1.Clone());

			return geom;
		}
        
        #endregion
	}
}