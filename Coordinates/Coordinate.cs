using System;
using System.Diagnostics;

namespace iGeospatial.Coordinates
{
	/// <summary> 
	/// A lightweight class used to store coordinates on the 2-dimensional Cartesian 
	/// plane. 
	/// </summary>
	/// <remarks>
	/// It is distinct from Point, which is a subclass of Geometry. 
	/// Unlike objects of type Point (which contain additional information such 
	/// as an envelope, a precision model, and spatial reference system information), 
	/// a Coordinate only Contains ordinate values and accessor methods.
	/// <para>
	/// Coordinates are two-dimensional points, with an additional
	/// z-ordinate. OTS does not support any operations on the z-ordinate except
	/// the basic accessor functions. Constructed coordinates will have a
	/// z-ordinate of NaN.  The standard comparison functions will ignore
	/// the z-ordinate.
	/// </para>
	/// </remarks>  
	[Serializable]
	public class Coordinate : ICoordinatePoint, ICloneable, IComparable
	{
        #region Internal Fields
		
        /// <summary>  The x-coordinate.</summary>
		internal double m_dX;

		/// <summary>  The y-coordinate.</summary>
		internal double m_dY;

        #endregion
		
        #region Constructors and Destructor
		
        /// <summary>  
		/// Constructs a Coordinate at (x,y).
		/// </summary>
		/// <param name="x"> the x-value
		/// </param>
		/// <param name="y"> the y-value
		/// </param>
		public Coordinate(double x, double y)
		{
			m_dX = x;
			m_dY = y;
		}
		
		/// <summary>  Constructs a Coordinate at (0,0).</summary>
		public Coordinate() : this(0.0, 0.0)
		{
		}
		
		/// <summary>  
		/// Constructs a Coordinate having the same (x,y) values as other.
		/// </summary>
		/// <param name="c"> the Coordinate to copy.
		/// </param>
		public Coordinate(Coordinate c)
		{
            if (c == null)
            {
                throw new ArgumentNullException("c");
            }

            m_dX = c.m_dX;
            m_dY = c.m_dY;
        }

        #endregion
		
        #region Public Properties

        public double X
        {
            get
            {
                return m_dX;
            }

            set 
            {
                m_dX = value;
            }
        }
		
        public double Y
        {
            get
            {
                return m_dY;
            }

            set 
            {
                m_dY = value;
            }
        }

        public virtual CoordinateType CoordinateType
        {
            get
            {
                return CoordinateType.Default;
            }
        }

        #endregion
		
        #region Public Methods

		/// <summary>  
		/// Sets this Coordinates (x,y) values to that of other. 
		/// </summary>
		/// <param name="other"> the Coordinate to copy
		/// </param>
		public virtual void SetCoordinate(Coordinate other)
		{
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

			m_dX = other.m_dX;
			m_dY = other.m_dY;
		}

		/// <summary>  
		/// Sets this Coordinates (x,y) values to that of other. 
		/// </summary>
		/// <param name="other"> the Coordinate to copy
		/// </param>
		public void SetCoordinate(float x, float y)
		{
			m_dX = x;
			m_dY = y;
		}

		/// <summary>  
		/// Sets this Coordinates (x,y) values to that of other. 
		/// </summary>
		/// <param name="other"> the Coordinate to copy
		/// </param>
		public void SetCoordinate(double x, double y)
		{
			m_dX = x;
			m_dY = y;
		}
		
		/// <summary>  
		/// Returns true if other has the same values for the x and y ordinates.
		/// Since Coordinates are 2.5D, this routine ignores the z value 
		/// when making the comparison.
		/// </summary>
		/// <param name="obj"> a Coordinate with which to do the comparison.
		/// </param>
		/// <returns>        true if other is a Coordinate
		/// with the same values for the x and y ordinates.
		/// </returns>
		public override bool Equals(object obj)
		{
            Coordinate other = obj as Coordinate;

			if (other == null)
			{
				return false;
			}

			return Equals2D(other);
		}
		
		public virtual bool Equals(Coordinate other)
		{
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (m_dX != other.m_dX)
            {
                return false;
            }
			
            if (m_dY != other.m_dY)
            {
                return false;
            }
			
            return true;
        }
		
        public virtual bool Equals(Coordinate other, double tolerance)
        {
            if (tolerance == 0)
            {
                return this.Equals(other);
            }

            return (this.Distance(other) <= tolerance);
        }
		
		/// <summary>  
		/// Returns a String of the form (x,y,z).
		/// </summary>
		/// <returns>A string of the form (x,y,z).</returns>
		public override string ToString()
		{
			return "(" + m_dX + ", " + m_dY + ")";
		}
		
		/// <summary> 
		/// "Fixes" this Coordinate to the PrecisionModel grid.
		/// </summary>
		public virtual void MakePrecise(PrecisionModel precisionModel)
		{
            if (precisionModel != null)
            {
                m_dX = precisionModel.MakePrecise(m_dX);
                m_dY = precisionModel.MakePrecise(m_dY);
            }
		}

		public virtual double Distance(Coordinate p)
		{
			double dx = m_dX - p.X;
			double dy = m_dY - p.Y;
			
			return Math.Sqrt(dx * dx + dy * dy);
		}
		
		public override int GetHashCode()
		{
			int result = 17;
			result = 37 * result + HashCode(m_dX);
			result = 37 * result + HashCode(m_dY);

			return result;
		}
        
        #endregion

        #region Internal Methods

		/// <summary>  
		/// Returns whether the planar projections of the two Coordinates are equal.
		/// </summary>
		/// <param name="other"> a Coordinate with which to do the 2D comparison.
		/// </param>
		/// <returns>        true if the x- and y-coordinates are equal; the
		/// z-coordinates do not have to be equal.
		/// </returns>
		internal bool Equals2D(Coordinate other)
		{
			if (m_dX != other.m_dX)
			{
				return false;
			}
			
			if (m_dY != other.m_dY)
			{
				return false;
			}
			
			return true;
		}
        
        #endregion

        #region Public Operator Overloading

        // Equals
        public static bool operator ==(Coordinate left, Coordinate right)
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
        public static bool operator !=(Coordinate left, Coordinate right)
        {
            return !(left == right);
        }

        public static bool operator <(Coordinate left, Coordinate right) 
        {
            if ((object)left == null)
                throw new ArgumentNullException("left");

            return (left.CompareTo(right) < 0);
        }
        
        public static bool operator <=(Coordinate left, Coordinate right) 
        {
            if ((object)left == null)
                throw new ArgumentNullException("left");

            return (left.CompareTo(right) <= 0);
        }
        
        public static bool operator >(Coordinate left, Coordinate right) 
        {
            return (right < left);
        }
        
        public static bool operator >=(Coordinate left, Coordinate right) 
        {
            return (right <= left);
        }

        // Addition
        public static Coordinate operator+(Coordinate left, Coordinate right)
        {
            if ((object)left == null)
            {
                if ((object)right == null)
                {
                    return null;
                }

                return new Coordinate(right);
            }

            double tempX = left.m_dX + right.m_dX;
            double tempY = left.m_dY + right.m_dY;

            return new Coordinate(tempX, tempY);
        }

        // Subtraction
        public static Coordinate operator-(Coordinate left, Coordinate right)
        {
            if ((object)left == null)
            {
                if ((object)right == null)
                {
                    return null;
                }

                return new Coordinate(right);
            }

            double tempX = left.m_dX - right.m_dX;
            double tempY = left.m_dY - right.m_dY;

            return new Coordinate(tempX, tempY);
        }

        // Unary negation
        public static Coordinate operator-(Coordinate point)
        {
            if ((object)point == null)
            {
                return null;
            }

            return new Coordinate(-point.m_dX, -point.m_dY);
        }

        // Scalar multiplication
        public static Coordinate operator*(Coordinate point, double scalar)
        {
            if ((object)point == null)
            {
                return null;
            }

            double tempX = point.m_dX * scalar;
            double tempY = point.m_dY * scalar;

            return new Coordinate(tempX, tempY);
        }

        #endregion
		
        #region Public Static Methods

		/// <summary> 
		/// Returns a hash code for a double value, using the algorithm from
		/// Joshua Bloch's book Effective Java"
		/// </summary>
		public static int HashCode(double x)
		{
            long f = BitConverter.DoubleToInt64Bits(x);
			return (int) (f ^ (URShift(f, 32)));
		}

        public static int URShift(int number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            else
                return (number >> bits) + (2 << ~bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static int URShift(int number, long bits)
        {
            return URShift(number, (int)bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static long URShift(long number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            else
                return (number >> bits) + (2L << ~bits);
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static long URShift(long number, long bits)
        {
            return URShift(number, (int)bits);
        }
		
        public static bool Equals(Coordinate a, Coordinate b, double tolerance)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }

            if ((object)a == null)
            {
                throw new ArgumentNullException("a");
            }

            if ((object)b == null)
            {
                throw new ArgumentNullException("b");
            }

            if (tolerance == 0)
            {
                return a.Equals(b);
            }

            return a.Distance(b) <= tolerance;
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
		/// -1, if <c> this.x &lt; other.x || ((this.x == other.x) &amp;&amp; (this.y &lt; other.y)) </c>
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// 0, if <c>this.x == other.x &amp;&amp; this.y = other.y</c>
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// 1, if <c>this.x &gt; other.x || ((this.x == other.x) &amp;&amp; (this.y &gt; other.y))</c>
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		public virtual int CompareTo(Coordinate other)
		{
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
			
			return 0;
		}   

        public virtual int CompareTo(object o)
        {
            return CompareTo((Coordinate)o);
        }
        
        #endregion

        #region ICloneable Members

        public virtual Coordinate Clone()
        {
            return new Coordinate(m_dX, m_dY);
        }
        
        Object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

        #region ICoordinatePoint Members

        public virtual int Dimension
        {
            get
            {
                return 2;
            }
        }

        public virtual double GetOrdinate(int dimension)
        {
            if (dimension == 0)
                return m_dX;

            if (dimension == 1)
                return m_dY;

            throw new CoordinateException("The dimension is out of range.");
        }

        public virtual void SetOrdinate(int dimension, double value)
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

            throw new CoordinateException("The dimension is out of range.");
        }

        public virtual double[] Ordinate
        {
            get
            {
                return new double[]{m_dX, m_dY};
            }
        }

        public virtual void SetLocation(ICoordinatePoint point)
        {
            if (point.Dimension >= 2)
            {
                m_dX = point.GetOrdinate(0);
                m_dY = point.GetOrdinate(1);
            }
            else
            {
                throw new ArgumentException("The dimension of the point is not valid");
            }
        }

        #endregion
    }
}