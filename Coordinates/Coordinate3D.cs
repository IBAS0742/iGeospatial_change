using System;

namespace iGeospatial.Coordinates
{
	/// <summary>
	/// Summary description for Coordinate3D.
	/// </summary>
    [Serializable]
    public class Coordinate3D : Coordinate
	{
        #region Private Fields

        /// <summary>  The z-coordinate.</summary>
        internal double m_dZ;		
        
        #endregion
		
        #region Constructors and Destructor

		/// <summary>  Constructs a Coordinate at (0,0,0).</summary>
		public Coordinate3D() : this(0.0, 0.0, 0.0)
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
		public Coordinate3D(double x, double y, double z)
		{
			m_dX = x;
			m_dY = y;
			m_dZ = z;
		}
		
		/// <summary>  Constructs a Coordinate at (x,y,NaN).
		/// 
		/// </summary>
		/// <param name="x"> the x-value
		/// </param>
		/// <param name="y"> the y-value
		/// </param>
		public Coordinate3D(double x, double y) : base(x, y)
		{
            m_dZ = Double.NaN;
		}
		
		/// <summary>  
		/// Constructs a Coordinate having the same (x,y,z) values as other.
		/// </summary>
		/// <param name="c"> the Coordinate to copy.
		/// </param>
		public Coordinate3D(Coordinate3D c)
		{
            if (c == null)
            {
                throw new ArgumentNullException("c");
            }

            m_dX = c.m_dX;
            m_dY = c.m_dY;
            m_dZ = c.m_dZ;
        }
        
        #endregion
		
        #region Public Properties

        public double Z
        {
            get
            {
                return m_dZ;
            }

            set 
            {
                m_dZ = value;
            }
        }

        public override int Dimension
        {
            get
            {
                return 3;
            }
        }
        
        #endregion
			
        #region Public Methods

        /// <summary>  
        /// Sets this Coordinate's (x,y,z) values to that of other.</summary>
        /// <param name="other"> the Coordinate to copy
        /// </param>
        public override void SetCoordinate(Coordinate other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            m_dX = other.m_dX;
            m_dY = other.m_dY;
            
            Coordinate3D other3D = other as Coordinate3D;

            if (other3D != null)
            {
                m_dZ = other3D.m_dZ;
            }
        }

        /// <summary>  
        /// Sets this Coordinate's (x,y,z) values to that of other.</summary>
        /// <param name="other"> the Coordinate to copy
        /// </param>
        public void SetCoordinate(Coordinate3D other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            m_dX = other.m_dX;
            m_dY = other.m_dY;
            m_dZ = other.m_dZ;
        }

        /// <summary>  
        /// Sets this Coordinates (x,y) values to that of other. 
        /// </summary>
        /// <param name="other"> the Coordinate to copy
        /// </param>
        public void SetCoordinate(float x, float y, float z)
        {
            m_dX = x;
            m_dY = y;
            m_dZ = z;
        }

        /// <summary>  
        /// Sets this Coordinates (x,y) values to that of other. 
        /// </summary>
        /// <param name="other"> the Coordinate to copy
        /// </param>
        public void SetCoordinate(double x, double y, double z)
        {
            m_dX = x;
            m_dY = y;
            m_dZ = z;
        }
		
        /// <summary>  
        /// Returns true if other has the same values for
        /// the x and y ordinates.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// 
        /// </summary>
        /// <param name="obj"> a Coordinate with which to do the comparison.
        /// </param>
        /// <returns>        true if other is a Coordinate
        /// with the same values for the x and y ordinates.
        /// </returns>
        public override bool Equals(object obj)
        {
            Coordinate3D other = obj as Coordinate3D;

            if (other == null)
            {
                return false;
            }

            return Equals3D(other);
        }
		
        public virtual bool Equals(Coordinate3D other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return (m_dX == other.m_dX) && (m_dY == other.m_dY) && 
                ((m_dZ == other.m_dZ) || 
                (Double.IsNaN(m_dZ) && Double.IsNaN(other.m_dZ)));
        }
		
        /// <summary>  Returns a String of the form <I>(x,y,z)</I> .
        /// 
        /// </summary>
        /// <returns>    a String of the form <I>(x,y,z)</I>
        /// </returns>
        public override string ToString()
        {
            return "(" + m_dX + ", " + m_dY + ", " + m_dZ + ")";
        }
		
        /// <summary> "Fixes" this Coordinate to the PrecisionModel grid.</summary>
        public override void MakePrecise(PrecisionModel precisionModel)
        {
            if (precisionModel != null)
            {
                m_dX = precisionModel.MakePrecise(m_dX);
                m_dY = precisionModel.MakePrecise(m_dY);
                m_dZ = precisionModel.MakePrecise(m_dZ);
            }
        }

        public virtual double Distance(Coordinate3D p)
        {
            double dx = m_dX - p.m_dX;
            double dy = m_dY - p.m_dY;
            double dz = m_dZ - p.m_dZ;
			
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
		
        public override int GetHashCode()
        {
            //Algorithm from Effective Java by Joshua Bloch [Jon Aquino]
            int result = 17;
            result = 37 * result + HashCode(m_dX);
            result = 37 * result + HashCode(m_dY);
            result = 37 * result + HashCode(m_dZ);

            return result;
        }

        public override double GetOrdinate(int dimension)
        {
            if (dimension == 0)
                return m_dX;

            if (dimension == 1)
                return m_dY;

            if (dimension == 2)
                return m_dZ;

            throw new CoordinateException("The dimension is out of range.");
        }

        public override void SetOrdinate(int dimension, double value)
        {
            if (dimension == 0)
            {
                m_dX = value;
                return;
            }

            if (dimension == 1)
            {
                m_dY = value;
                return;
            }

            if (dimension == 2)
            {
                m_dZ = value;
                return;
            }

            throw new CoordinateException("The dimension is out of range.");
        }
        
        #endregion
                        
        #region Internal Methods
		
        /// <summary>  Returns true if other has the same values for x,
        /// y and z.
        /// 
        /// </summary>
        /// <param name="other"> a Coordinate with which to do the 3D comparison.
        /// </param>
        /// <returns>        true if other is a Coordinate
        /// with the same values for x, y and z.
        /// </returns>
        internal bool Equals3D(Coordinate3D other)
        {
            return (m_dX == other.m_dX) && (m_dY == other.m_dY) && 
                ((m_dZ == other.m_dZ) || 
                (Double.IsNaN(m_dZ) && Double.IsNaN(other.m_dZ)));
        }
        
        #endregion

        #region Public Operator Overloading

        // Equals
        public static bool operator ==(Coordinate3D left, Coordinate3D right)
        {
            if ((object)left == null) 
            {
                if ((object)right == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if ((object)right == null)
                return false;

            return left.Equals(right);
        }

        // Not equals
        public static bool operator !=(Coordinate3D left, Coordinate3D right)
        {
            return !(left == right);
        }

        public static bool operator <(Coordinate3D left, Coordinate3D right) 
        {
            if ((object)left == null)
                throw new ArgumentNullException("left");

            return (left.CompareTo(right) < 0);
        }
        
        public static bool operator <=(Coordinate3D left, Coordinate3D right) 
        {
            if ((object)left == null)
                throw new ArgumentNullException("left");

            return (left.CompareTo(right) <= 0);
        }
        
        public static bool operator >(Coordinate3D left, Coordinate3D right) 
        {
            return (right < left);
        }
        
        public static bool operator >=(Coordinate3D left, Coordinate3D right) 
        {
            return (right <= left);
        }

        // Addition
        public static Coordinate operator+(Coordinate3D left, Coordinate3D right)
        {
            double tempX = left.m_dX + right.m_dX;
            double tempY = left.m_dY + right.m_dY;
            double tempZ = left.m_dZ + right.m_dZ;

            return new Coordinate3D(tempX, tempY, tempZ);
        }

        // Subtraction
        public static Coordinate operator-(Coordinate3D left, Coordinate3D right)
        {
            double tempX = left.m_dX - right.m_dX;
            double tempY = left.m_dY - right.m_dY;
            double tempZ = left.m_dZ - right.m_dZ;

            return new Coordinate3D(tempX, tempY, tempZ);
        }

        // Unary negation
        public static Coordinate3D operator-(Coordinate3D point)
        {
            return new Coordinate3D(-point.m_dX, -point.m_dY, -point.m_dZ);
        }

        // Scalar multiplication
        public static Coordinate3D operator*(Coordinate3D point, double scalar)
        {
            double tempX = point.m_dX * scalar;
            double tempY = point.m_dY * scalar;
            double tempZ = point.m_dZ * scalar;

            return new Coordinate3D(tempX, tempY, tempZ);
        }

        #endregion
		
        #region IComparable Members

        /// <summary>  
        /// Compares this object with the specified object for order.
        /// </summary>
        /// <param name="o"> the Coordinate with which this Coordinate
        /// is being compared
        /// </param>
        /// <returns>A negative integer, zero, or a positive integer as this Coordinate
        /// is less than, equal to, or greater than the specified Coordinate
        /// </returns>
        /// <remarks>
        /// Returns
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// -1, if <c> this.x &lt; other.x || ((this.x == other.x) && (this.y &lt; other.y)) </c>
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// 0, if <c>this.x == other.x && this.y = other.y</c>
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// 1, if <c>this.x &gt; other.x || ((this.x == other.x) && (this.y &gt; other.y))</c>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        public override int CompareTo(object o)
        {
            Coordinate3D other = o as Coordinate3D;
            if (other == null)
            {
                return -1;
            }
			
            if (m_dX < other.m_dX)
            {
                return -1;
            }
			
            if (m_dX > other.m_dX)
            {
                return 1;
            }
			
            if (m_dY < other.m_dY)
            {
                return -1;
            }
			
            if (m_dY > other.m_dY)
            {
                return 1;
            }
			
            if (m_dZ < other.m_dZ)
            {
                return -1;
            }
			
            if (m_dZ > other.m_dZ)
            {
                return 1;
            }
			
            return 0;
        }
        
        #endregion
		
        #region ICloneable Members

        public override Coordinate Clone()
        {
            return new Coordinate3D(m_dX, m_dY, m_dZ);
        }
        
        #endregion
    }
}
