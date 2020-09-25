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

using iGeospatial.Exceptions;
using iGeospatial.Coordinates;
using iGeospatial.Geometries.Editors;
using iGeospatial.Geometries.Operations.Buffer;
using iGeospatial.Geometries.Noding;
using iGeospatial.Geometries.Noding.SnapRounds;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Computes the buffer of a geometry, for both positive and 
	/// negative buffer distances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// In GIS, the buffer of a geometry is defined as the Minkowski sum or difference 
	/// of the geometry with a circle with radius equal to the absolute value of the 
	/// buffer distance.
	/// </para>
	/// <para>
	/// In the CAD/CAM world buffers are known as offset curves.
	/// </para>
	/// Since true buffer curves may contain circular arcs,
	/// computed buffer polygons can only be approximations to the true geometry.
	/// The user can control the accuracy of the curve approximation by specifying
	/// the number of linear segments with which to approximate a curve.
	/// <para>
	/// The end cap style of a linear buffer may be specified. The
	/// following end cap styles are supported:
	/// </para>
	/// <list type="bullet">
	/// <item>
	/// <term><see cref="BufferCapType.Round"/></term>
	/// <description>
	/// The usual round end caps.
	/// </description>
	/// </item>
	/// <item>
	/// <term><see cref="BufferCapType.Flat"/></term>
	/// <description>
	/// The end caps are truncated flat at the line ends.
	/// </description>
	/// </item>
	/// <item>
	/// <term><see cref="BufferCapType.Square"/></term>
	/// <description>
	/// The end caps are squared off at the buffer 
	/// distance beyond the line ends.
	/// </description>
	/// </item>
	/// </list>
	/// The computation uses an algorithm involving iterated noding and precision reduction
	/// to provide a high degree of robustness.
	/// <seealso cref="BufferCapType"/>
	/// </remarks>
	public sealed class BufferOp
	{
        #region Private Fields

		private Geometry          m_objTargetGeometry;
		private double            m_dDistance;
		private int               m_nQuadrantSegments;
		private BufferCapType     m_enumEndCapStyle;
		
		private Geometry          m_objResultGeometry;
		private Exception         saveException; // debugging only
				                          
		private static int        MaxPrecisionDigits = 12;
        
        #endregion
		
        #region Constructors and Destructor
		
        /// <overloads>
		/// Initializes a new instance of the <see cref="BufferOp"/> class.
		/// </overloads>
		/// <summary> 
		/// Initializes a buffer computation for the given geometry.
		/// </summary>
		/// <param name="g">The geometry to buffer.</param>
		public BufferOp(Geometry g)
		{
            m_nQuadrantSegments = OffsetCurveBuilder.DefaultQuadrantSegments;
            m_enumEndCapStyle   = BufferCapType.Round;

			m_objTargetGeometry = g;
		}

        /// <summary> 
        /// Initializes a buffer computation for the given geometry.
        /// </summary>
        /// <param name="g">The geometry to buffer.</param>
        /// <param name="capStyle">
        /// The end cap style of the generated buffer.
        /// </param>
        public BufferOp(Geometry g, BufferCapType capStyle)
        {
            m_nQuadrantSegments = OffsetCurveBuilder.DefaultQuadrantSegments;
            m_enumEndCapStyle   = capStyle;

            m_objTargetGeometry = g;
        }

        /// <summary> 
        /// Initializes a buffer computation for the given geometry.
        /// </summary>
        /// <param name="g">The geometry to buffer.</param>
        /// <param name="capStyle">
        /// The end cap style of the generated buffer.
        /// </param>
        /// <param name="quadrantSegments">
        /// The number of segments used to approximate a quarter circle.
        /// </param>
        public BufferOp(Geometry g, BufferCapType capStyle, int quadrantSegments)
        {
            m_nQuadrantSegments = quadrantSegments;
            m_enumEndCapStyle   = capStyle;

            m_objTargetGeometry = g;
        }

        /// <summary> 
        /// Initializes a buffer computation for the given geometry.
        /// </summary>
        /// <param name="g">The geometry to buffer.</param>
        /// <param name="quadrantSegments">
        /// The number of segments used to approximate a quarter circle.
        /// </param>
        public BufferOp(Geometry g, int quadrantSegments)
        {
            m_nQuadrantSegments = quadrantSegments;
            m_enumEndCapStyle   = BufferCapType.Round;

            m_objTargetGeometry       = g;
        }
        
        #endregion
		
        #region Public Properties

        /// <summary> 
        /// Gets or sets the end cap style of the generated buffer.
        /// The default is <see cref="BufferCapType.Round"/>.
        /// </summary>
        /// <value>The end cap style to specify.</value>
        public BufferCapType EndCapType
		{
            get 
            {   
                return m_enumEndCapStyle;
            }

			set
			{
				m_enumEndCapStyle = value;
			}
		}
			
        /// <summary>
        /// The number of segments used to approximate a quarter circle.
        /// </summary>
        /// <value>
        /// An integer specifying the number of segments to be used.
        /// </value>
		public int QuadrantSegments
		{
            get 
            {
                return m_nQuadrantSegments;
            }

			set
			{
				m_nQuadrantSegments = value;
			}
		}
        
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// Computes the buffer of a geometry for a given buffer distance.
		/// </summary>
		/// <param name="g">The geometry to buffer.</param>
		/// <param name="distance">The buffer distance.</param>
		/// <returns>The buffer of the input geometry.</returns>
		public static Geometry Buffer(Geometry g, double distance)
		{
			BufferOp gBuf    = new BufferOp(g);
			Geometry geomBuf = gBuf.Buffer(distance);

			return geomBuf;
		}

		/// <summary> 
		/// Computes the buffer of a geometry for a given buffer distance.
		/// </summary>
		/// <param name="g">The geometry to buffer.</param>
		/// <param name="distance">The buffer distance.</param>
        /// <param name="capStyle">
        /// The end cap style of the generated buffer.
        /// </param>
        /// <returns>The buffer of the input geometry.</returns>
		public static Geometry Buffer(Geometry g, double distance, 
            BufferCapType capStyle)
		{
			BufferOp gBuf    = new BufferOp(g, capStyle);
			Geometry geomBuf = gBuf.Buffer(distance);

			return geomBuf;
		}
		
		/// <summary> 
		/// Computes the buffer for a geometry for a given buffer distance
		/// and accuracy of approximation.
		/// </summary>
		/// <param name="g">The geometry to buffer.</param>
		/// <param name="distance">The buffer distance.</param>
		/// <param name="quadrantSegments">
		/// The number of segments used to approximate a quarter circle.
		/// </param>
		/// <returns>
		/// The buffer of the input geometry. 
		/// </returns>
		public static Geometry Buffer(Geometry g, double distance, 
            int quadrantSegments)
		{
			BufferOp bufOp   = new BufferOp(g, quadrantSegments);
			Geometry geomBuf = bufOp.Buffer(distance);

			return geomBuf;
		}
		
		/// <summary> 
		/// Computes the buffer for a geometry for a given buffer distance
		/// and accuracy of approximation.
		/// </summary>
		/// <param name="g">The geometry to buffer.</param>
		/// <param name="distance">The buffer distance.</param>
		/// <param name="quadrantSegments">
		/// The number of segments used to approximate a quarter circle.
		/// </param>
        /// <param name="capStyle">
        /// The end cap style of the generated buffer.
        /// </param>
        /// <returns>
		/// The buffer of the input geometry. 
		/// </returns>
		public static Geometry Buffer(Geometry g, double distance, 
            int quadrantSegments, BufferCapType capStyle)
		{
			BufferOp bufOp   = new BufferOp(g, capStyle, quadrantSegments);
			Geometry geomBuf = bufOp.Buffer(distance);

			return geomBuf;
		}
		
		/// <summary> 
		/// Returns the buffer computed for a geometry for a given buffer distance.
		/// </summary>
		/// <param name="g">The geometry to buffer.</param>
		/// <param name="distance">The buffer distance.</param>
		/// <returns>The buffer of the input geometry.</returns>
		public Geometry Buffer(double distance)
		{
			this.m_dDistance = distance;
			ComputeGeometry();

			return m_objResultGeometry;
		}
        
        #endregion
		
        #region Private Methhods
		
        private void ComputeGeometry()
        {
            BufferOriginalPrecision();

            if (m_objResultGeometry != null)
                return ;
			
            PrecisionModel argPM = m_objTargetGeometry.Factory.PrecisionModel;
            if (argPM.ModelType == PrecisionModelType.Fixed)
                BufferFixedPrecision(argPM);
            else
                BufferReducedPrecision();
        }

		private void BufferReducedPrecision()
		{
			// try and compute with decreasing precision
			for (int precDigits = MaxPrecisionDigits; precDigits >= 0; precDigits--)
			{
				try
				{
//					BufferFixedPrecision(precDigits);
                    BufferReducedPrecision(precDigits);
				}
				catch (Exception ex)
				{
					saveException = ex;
					// don't propagate the exception - it will be detected 
                    // by fact that m_objResultGeometry is null

                    ExceptionManager.Publish(ex);
				}

				if (m_objResultGeometry != null)
					return;
			}
			
			// tried everything - have to bail
			throw saveException;
		}
		
		private void BufferOriginalPrecision()
		{
			try
			{
                // use fast noding by default
				BufferBuilder bufBuilder    = new BufferBuilder();
				bufBuilder.QuadrantSegments = m_nQuadrantSegments;
				bufBuilder.EndCapStyle      = m_enumEndCapStyle;
				m_objResultGeometry = bufBuilder.Buffer(m_objTargetGeometry, m_dDistance);
			}
			catch (Exception ex)
			{
				saveException = ex;
				// don't propagate the exception - it will be detected by 
                // fact that m_objResultGeometry is null

                ExceptionManager.Publish(ex);
            }
		}
		
        private void BufferReducedPrecision(int precisionDigits)
        {
            double sizeBasedScaleFactor = PrecisionScaleFactor(m_objTargetGeometry, 
                m_dDistance, precisionDigits);
			
            PrecisionModel fixedPM = new PrecisionModel(sizeBasedScaleFactor);
            
            BufferFixedPrecision(fixedPM);
        }
		
        private void BufferFixedPrecision(PrecisionModel fixedPM)
        {
            INoder noder = new ScaledNoder(new MCIndexSnapRounder(
                new PrecisionModel(1.0)), fixedPM.Scale);
			
            BufferBuilder bufBuilder = new BufferBuilder();
            bufBuilder.WorkingPrecisionModel = fixedPM;
            bufBuilder.Noder = noder;
            bufBuilder.QuadrantSegments = m_nQuadrantSegments;
            bufBuilder.EndCapStyle      = m_enumEndCapStyle;
            // this may throw an exception, if robustness errors are encountered
            m_objResultGeometry = bufBuilder.Buffer(m_objTargetGeometry, m_dDistance);
        }
		
		/// <summary> Compute a reasonable scale factor to limit the precision of
		/// a given combination of Geometry and Buffer Distance.
		/// The scale factor is based on a heuristic.
		/// 
		/// </summary>
		/// <param name="g">the Geometry being buffered
		/// </param>
		/// <param name="Distance">the Buffer Distance
		/// </param>
		/// <param name="maxPrecisionDigits">the mzx # of digits that should be allowed by
		/// the precision determined by the computed scale factor
		/// 
		/// </param>
		/// <returns> a scale factor that allows a reasonable amount of precision for the Buffer computation
		/// </returns>
		private static double PrecisionScaleFactor(Geometry g, double Distance, int maxPrecisionDigits)
		{
			Envelope env = g.Bounds;
			double envSize = Math.Max(env.Height, env.Width);
			double expandByDistance = Distance > 0.0?Distance:0.0;
			double bufEnvSize = envSize + 2 * expandByDistance;
			
			// the smallest power of 10 greater than the Buffer envelope
			int bufEnvLog10 = (int) (Math.Log(bufEnvSize) / Math.Log(10) + 1.0);
			int minUnitLog10 = bufEnvLog10 - maxPrecisionDigits;

			// scale factor is inverse of min Unit size, so flip sign of exponent
			double scaleFactor = Math.Pow(10.0, - minUnitLog10);
			return scaleFactor;
		}
        
        #endregion
	}
}