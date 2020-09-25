using System;

namespace iGeospatial.Coordinates
{
	/// <summary>
	/// Summary description for CoordinateM.
	/// </summary>
    [Serializable]
    public class CoordinateM : Coordinate
	{
        #region Private Fields
        
        /// <summary>
        /// The measure for this coordinate.
        /// </summary>
        private double m_dMeasure = Double.NaN;

        #endregion

        #region Constructors and Destructor
		
		/// <summary>  Constructs a CoordinateM at (0,0).</summary>
		public CoordinateM() : base(0.0, 0.0)
		{
		}
		
		/// <summary>  Constructs a CoordinateM at (x,y).
		/// 
		/// </summary>
		/// <param name="x"> the x-value
		/// </param>
		/// <param name="y"> the y-value
		/// </param>
		public CoordinateM(double x, double y) : base(x, y)
		{
		}
		
		/// <summary>  Constructs a CoordinateM at (x,y).
		/// 
		/// </summary>
		/// <param name="x"> the x-value
		/// </param>
		/// <param name="y"> the y-value
		/// </param>
		public CoordinateM(double x, double y, double measure) : base(x, y)
		{
            m_dMeasure = measure;
		}
		
		/// <summary>  
		/// Constructs a CoordinateM having the same (x,y) values as other.
		/// </summary>
		/// <param name="c"> the CoordinateM to copy.
		/// </param>
		public CoordinateM(CoordinateM c) : base(c.X, c.Y)
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

        public override CoordinateType CoordinateType
        {
            get
            {
                return CoordinateType.Measured;
            }
        }

        #endregion

        #region ICloneable Members

        public override Coordinate Clone()
        {
            return new CoordinateM(m_dX, m_dY, m_dMeasure);
        }

        #endregion
    }
}
