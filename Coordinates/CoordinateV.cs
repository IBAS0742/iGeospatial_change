using System;

namespace iGeospatial.Coordinates
{
	/// <summary>
	/// Summary description for CoordinateV.
	/// </summary>
    [Serializable]
    public class CoordinateV : Coordinate
	{
        #region Private Fields
        
        private bool m_bVisible = true;

        #endregion

        #region Constructors and Destructor
		
		/// <summary>  Constructs a CoordinateV at (0,0).</summary>
		public CoordinateV() : base(0.0, 0.0)
		{
		}
		
		/// <summary>  Constructs a CoordinateV at (x,y).
		/// 
		/// </summary>
		/// <param name="x"> the x-value
		/// </param>
		/// <param name="y"> the y-value
		/// </param>
		public CoordinateV(double x, double y) : base(x, y)
		{
		}
		
		/// <summary>  Constructs a CoordinateV at (x,y).
		/// 
		/// </summary>
		/// <param name="x"> the x-value
		/// </param>
		/// <param name="y"> the y-value
		/// </param>
		public CoordinateV(double x, double y, bool visible) : base(x, y)
		{
            m_bVisible = visible;
		}
		
		/// <summary>  
		/// Constructs a CoordinateV having the same (x,y) values as other.
		/// </summary>
		/// <param name="c"> the CoordinateV to copy.
		/// </param>
		public CoordinateV(CoordinateV c) : base(c.X, c.Y)
		{
            m_bVisible = c.m_bVisible;
		}

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets whether this coordinate is a visible point.
        /// </summary>
        /// <value>
        /// true if the point defined by the coordinate is visible, otherwise false.
        /// </value>
        public bool Visible
        {
            get
            {
                return m_bVisible;
            }

            set
            {
                m_bVisible = value;
            }
        }

        public override CoordinateType CoordinateType
        {
            get
            {
                return CoordinateType.Flagged;
            }
        }

        #endregion

        #region ICloneable Members

        public override Coordinate Clone()
        {
            return new CoordinateV(m_dX, m_dY, m_bVisible);
        }

        #endregion
    }
}
