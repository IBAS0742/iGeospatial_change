using System;

namespace iGeospatial.Coordinates
{
	/// <summary>
	/// Summary description for Coordinate3DV.
	/// </summary>
    [Serializable]
    public class Coordinate3DV : Coordinate3D
	{
        #region Private Fields
        
        private bool m_bVisible = true;

        #endregion

        #region Constructors and Destructor

		/// <summary>  Constructs a Coordinate at (0,0,0).</summary>
		public Coordinate3DV() : base()
		{
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
		public Coordinate3DV(double x, double y, double z) 
            : base(x, y, z)
		{
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
		public Coordinate3DV(double x, double y, double z, bool visible) 
            : base(x, y, z)
		{
            m_bVisible = visible;
		}
		
		/// <summary>  
		/// Constructs a Coordinate having the same (x,y,z) values as other.
		/// </summary>
		/// <param name="c"> the Coordinate to copy.
		/// </param>
		public Coordinate3DV(Coordinate3D c) : base(c)
		{
		}
		
		/// <summary>  
		/// Constructs a Coordinate having the same (x,y,z) values as other.
		/// </summary>
		/// <param name="c"> the Coordinate to copy.
		/// </param>
		public Coordinate3DV(Coordinate3DV c) : base(c)
		{
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

        #endregion

        #region ICloneable Members

        public override Coordinate Clone()
        {
            return new Coordinate3DV(m_dX, m_dY, m_dZ, m_bVisible);
        }

        #endregion
    }
}
