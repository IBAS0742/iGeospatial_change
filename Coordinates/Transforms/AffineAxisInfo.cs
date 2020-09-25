using System;
using System.Text;

namespace iGeospatial.Coordinates.Transforms
{
	/// <summary>
	/// This defines the orientation of the coordinate system being used
	/// in coordinate transformation or mapping.
	/// </summary>
    [Serializable]
    public struct AffineAxisInfo
	{
        #region Private Fields

        /// <summary>
        /// The horizontal or x-axis orientation.
        /// </summary>
        private AffineAxisOrientation m_enumHorizontal;

        /// <summary>
        /// The vertical or y-axis orientation.
        /// </summary>
        private AffineAxisOrientation m_enumVertical;

        #endregion   
     
        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AffineAxisInfo"/> structure
        /// with the specified axes orientations.
        /// </summary>
        /// <param name="horizontal">
        /// A <see cref="AffineAxisOrientation"/> enumeration specifying the 
        /// horizontal axis.
        /// </param>
        /// <param name="vertical">
        /// A <see cref="AffineAxisOrientation"/> enumeration specifying the
        /// vertical axis.
        /// </param>
        public AffineAxisInfo(AffineAxisOrientation horizontal, 
            AffineAxisOrientation vertical)
        {
            m_enumHorizontal = horizontal;
            m_enumVertical   = vertical;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the horizontal or x-axis orientation.
        /// </summary>
        /// <value>
        /// An <see cref="AffineAxisOrientation"/> enumeration type specifying
        /// the direction of the horizontal axis. This is either,
        /// <see cref="AffineAxisOrientation.Left"/> or 
        /// <see cref="AffineAxisOrientation.Right"/>
        /// </value>
        public AffineAxisOrientation Horizontal
        {
            get
            {
                return m_enumHorizontal;
            }

            set
            {
                m_enumHorizontal = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertical or y-axis orientation.
        /// </summary>
        /// <value>
        /// An <see cref="AffineAxisOrientation"/> enumeration type specifying
        /// the direction of the vertical axis. This is either,
        /// <see cref="AffineAxisOrientation.Up"/> or 
        /// <see cref="AffineAxisOrientation.Down"/>
        /// </value>
        public AffineAxisOrientation Vertical
        {
            get
            {
                return m_enumVertical;
            }

            set
            {
                m_enumVertical = value;
            }
        }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Creates and returns a new <see cref="AffineAxisInfo"/> defining the
        /// default world coordinates axes.
        /// </summary>
        /// <value>
        /// A new <see cref="AffineAxisInfo"/> instance, defining the default 
        /// world coordinate system orientations.
        /// </value>
        /// <remarks>
        /// For world coordinates, the horizontal axis points to the right and
        /// the vertical axis points upwards.
        /// </remarks>
        public static AffineAxisInfo World
        {
            get
            {
                return new AffineAxisInfo(AffineAxisOrientation.Right, 
                    AffineAxisOrientation.Up);
            }
        }

        /// <summary>
        /// Creates and returns a new <see cref="AffineAxisInfo"/> defining the
        /// default screen coordinates axes, mainly the computer screen.
        /// </summary>
        /// <value>
        /// A new <see cref="AffineAxisInfo"/> instance, defining the default 
        /// screen coordinate system orientations.
        /// </value>
        /// <remarks>
        /// For screen coordinates, the horizontal axis points to the right and
        /// the vertical axis points downwards.
        /// </remarks>
        public static AffineAxisInfo Screen
        {
            get
            {
                return new AffineAxisInfo(AffineAxisOrientation.Right, 
                    AffineAxisOrientation.Down);
            }
        }

        #endregion

        #region Public Methods

        /// <overloads>
        /// Specifies whether this <see cref="AffineAxisInfo"/> and the specified
        /// argument contains the same orientations.
        /// </overloads>
        /// <summary>
        /// Specifies whether this <see cref="AffineAxisInfo"/> contains the 
        /// same orientations as the specified <see cref="AffineAxisInfo"/>.
        /// </summary>
        /// <param name="obj">The <see cref="AffineAxisInfo"/> to test.</param>
        /// <returns>
        /// This method returns true if obj has the same orientations as this instance.
        /// </returns>
        public bool Equals(AffineAxisInfo obj)
        {
            if ((obj.m_enumHorizontal == m_enumHorizontal) &&
                (obj.m_enumVertical == m_enumVertical))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Specifies whether this <see cref="AffineAxisInfo"/> contains the 
        /// same orientations as the specified <see cref="System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to test.</param>
        /// <returns>
        /// This method returns true if obj is a <see cref="AffineAxisInfo"/> 
        /// and has the same orientations as this instance.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(AffineAxisInfo))
            {
                return false;
            }

            AffineAxisInfo axisInfo = (AffineAxisInfo)obj;

            return Equals(axisInfo);
        }

        /// <summary>
        /// Returns a hash code for this <see cref="AffineAxisInfo"/> object.
        /// </summary>
        /// <returns>
        /// An integer value that specifies a hash value for this 
        /// <see cref="AffineAxisInfo"/> object.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a text representation of this object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> containing the axes information
        /// in the format "AXISINFO[Horizontal, Vertical]"
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("AXISINFO[");
            builder.Append(m_enumHorizontal.ToString());
            builder.Append(", ");
            builder.Append(m_enumVertical.ToString());
            builder.Append("]");

            return builder.ToString();
        }     

        #endregion

        #region Public Operator Overloading
        
        /// <summary>
        /// Compares two axes information for exact equality.
        /// </summary>
        /// <param name="axisInfo1">The first axis information to compare.</param>
        /// <param name="axisInfo2">The second axis information to compare.</param>
        /// <returns>
        /// true if both axes information are equal, false otherwise.
        /// </returns>
        public static bool operator ==(AffineAxisInfo axisInfo1, AffineAxisInfo axisInfo2)
        {
            return axisInfo1.Equals(axisInfo2);
        }
        
        /// <summary>
        /// Compares two axes information for exact inequality.
        /// </summary>
        /// <param name="axisInfo1">The first axis information to compare.</param>
        /// <param name="axisInfo2">The second axis information to compare.</param>
        /// <returns>
        /// true if both axes information are not equal, false otherwise.
        /// </returns>
        public static bool operator !=(AffineAxisInfo axisInfo1, AffineAxisInfo axisInfo2)
        {
            return !axisInfo1.Equals(axisInfo2);
        }
        
        #endregion
	}
}
