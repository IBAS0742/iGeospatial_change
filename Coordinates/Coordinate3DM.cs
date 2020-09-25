using System;

namespace iGeospatial.Coordinates
{
	/// <summary>
	/// Summary description for Coordinate3DM.
	/// </summary>
    [Serializable]
    public class Coordinate3DM : Coordinate3D
	{
        #region Private Fields
        
        /// <summary>
        /// The measure for this coordinate.
        /// </summary>
        private double m_dMeasure;

        #endregion

        #region Constructors and Destructor

		/// <summary>  Constructs a Coordinate at (0,0,0).</summary>
		public Coordinate3DM() : base()
		{
            m_dMeasure = Double.NaN;
		}
		
		/// <summary>  Constructs a Coordinate at (x,y,z).
		/// 
		/// </summary>
		/// <param name="x"> the x-value
		/// </param>
		/// <param name="y"> the y-value
		/// </param>
		/// <param name="z"> the z-value
		/// </param>
		public Coordinate3DM(double x, double y, double z) 
            : base(x, y, z)
		{
            m_dMeasure = Double.NaN;
		}
		
		/// <summary>  Constructs a Coordinate at (x,y,z).
		/// 
		/// </summary>
		/// <param name="x"> the x-value
		/// </param>
		/// <param name="y"> the y-value
		/// </param>
		/// <param name="z"> the z-value
		/// </param>
		public Coordinate3DM(double x, double y, double z, double measure) 
            : base(x, y, z)
		{
            m_dMeasure = measure;
		}
		
		/// <summary>  
		/// Constructs a Coordinate having the same (x,y,z) values as other.
		/// </summary>
		/// <param name="c"> the Coordinate to copy.
		/// </param>
		public Coordinate3DM(Coordinate3D c) : base(c)
		{
            m_dMeasure = Double.NaN;
		}
		
		/// <summary>  
		/// Constructs a Coordinate having the same (x,y,z) values as other.
		/// </summary>
		/// <param name="c"> the Coordinate to copy.
		/// </param>
		public Coordinate3DM(Coordinate3DM c) : base(c)
		{
            m_dMeasure = c.m_dMeasure;
		}
		
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets measure value for the point represented by this coordinate.
        /// </summary>
        /// <value>
        /// A double precision value specifying the measure.
        /// </value>
        public double Measure
        {
            get
            {
                return m_dMeasure;
            }

            set
            {
                m_dMeasure = value;
            }
        }

        #endregion

        #region ICloneable Members

        public override Coordinate Clone()
        {
            return new Coordinate3DM(m_dX, m_dY, m_dZ, m_dMeasure);
        }

        #endregion
    }
}
