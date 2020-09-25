using System;
using System.Text;

namespace iGeospatial.Coordinates
{
	/// <summary> 
	/// A position defined by a list of numbers. The ordinate
	/// values are indexed from <c>0</c> to <c>(numDim-1)</c>,
	/// where <c>numDim</c> is the dimension of the coordinate system
	/// the coordinate point belongs in.
	/// </summary>
	/// <seealso cref="ICoordinatePoint">
	/// </seealso>
	[Serializable]
	public struct CoordinatePoint : ICoordinatePoint
	{
		/// <summary> 
		/// The ordinates of the coordinate point.
		/// </summary>
		private double[] ord;
		
		/// <summary> 
		/// Construct a coordinate with the specified number of dimensions.
		/// </summary>
		public CoordinatePoint(int numDim)
		{
			ord = new double[numDim];
		}
		
		/// <summary> Construct a coordinate with the specified ordinates.
		/// The <code>ord</code> array will be copied.
		/// </summary>
		public CoordinatePoint(double[] ord)
		{
			this.ord = new double[ord.Length];

			ord.CopyTo(this.ord, 0);
		}
		
		/// <summary> 
		/// Construct a 2D coordinate from the specified ordinates.
		/// </summary>
		public CoordinatePoint(double x, double y)
		{
			ord = new double[]{x, y};
		}
		
		/// <summary> 
		/// Construct a 3D coordinate from the specified ordinates.
		/// </summary>
		public CoordinatePoint(double x, double y, double z)
		{
			ord = new double[]{x, y, z};
		}
		
		/// <summary> 
		/// Construct a coordinate from the specified {@link Point2D}.
		/// </summary>
		public CoordinatePoint(Coordinate point)
            : this(point.X, point.Y)
		{
		}
		
		/// <summary> 
		/// Construct a coordinate initialized to the same values than the 
		/// specified point.
		/// </summary>
		public CoordinatePoint(CoordinatePoint point)
		{
			ord = new double[point.ord.Length];
			point.ord.CopyTo(ord, 0);
		}
		
		/// <summary> 
		/// Set this coordinate to the specified {@link Point2D}.
		/// This coordinate must be two-dimensional.
		/// </summary>
		public void SetLocation(Coordinate point)
		{
			if (ord.Length != point.Dimension)
			{
				throw new ArgumentException();
			}

			ord[0] = point.X;
			ord[1] = point.Y;
		}
		
		/// <summary> 
		/// Set this coordinate to the specified <see cref="CoordinatePoint"/>.
		/// </summary>
		/// <param name="point">The new coordinate for this point.</param>
		public void SetLocation(ICoordinatePoint point)
		{
			EnsureDimensionMatch(point.Ordinate.Length);

			Array.Copy(point.Ordinate, 0, ord, 0, ord.Length);
		}
		
		/// <summary> 
		/// Returns the ordinate value along the specified dimension.
		/// </summary>
		public double GetOrdinate(int dimension)
		{
			return ord[dimension];
		}
		
		public void SetOrdinate(int dimension, double value)
		{
			ord[dimension] = value;
		}
		
		/// <summary> 
		/// The number of ordinates of a <see cref="CoordinatePoint"/>.
		/// </summary>
		public int Dimension
		{
            get
            {
                return ord.Length;
            }
		}
		
        public double[] Ordinate 
        {
            get
            {
                return ord;
            }
        }


		/// <summary> 
		/// Convenience method for checking the point's dimension validity.
		/// This method is usually call for argument checking.
		/// </summary>
		internal void  EnsureDimensionMatch(int expectedDimension)
		{
			int dimension = ord.Length;

			if (dimension != expectedDimension)
			{
				throw new ArgumentException();
			}
		}
		
		/// <summary> 
		/// Returns a <see cref="Coordinate"/> with the same coordinate
		/// as this <see cref="CoordinatePoint"/>.
		/// </summary>
		public Coordinate ToCoordinate()
		{
			if (ord.Length == 2)
			{
				return new Coordinate(ord[0], ord[1]);
			}
            else if (ord.Length == 3)
            {
                return new Coordinate3D(ord[0], ord[1], ord[2]);
            }

            return null;
		}
		
		/// <summary> 
		/// Returns a hash value for this coordinate.
		/// This value need not remain consistent between
		/// different implementations of the same class.
		/// </summary>
		public override int GetHashCode()
		{
			return HashCode(ord);
		}
		
		/// <summary> 
		/// Returns a hash value for the specified ordinates.
		/// </summary>
		internal static int HashCode(double[] ord)
		{
			long code = 17516481;

			if (ord != null)
			{
				for (int i = ord.Length; --i >= 0; )
				{
					code = code * 31 + BitConverter.DoubleToInt64Bits(ord[i]);
				}
			}

			return (int)(Coordinate.URShift(code, 32)) ^ (int)code;
		}
		
		/// <summary> Compares the specified object with
		/// this coordinate for equality.
		/// </summary>
		public  override bool Equals(System.Object obj)
		{
			if (obj is CoordinatePoint)
			{
				CoordinatePoint that = (CoordinatePoint)obj;

				return IsArrayEqual(this.ord, that.ord);
			}

			return false;
		}
		
		/// <summary> Returns a deep copy of this coordinate.</summary>
		public System.Object Clone()
		{
			return new CoordinatePoint(ord);
		}
		
		/// <summary> 
		/// Returns a string representation of this coordinate.
		/// The returned string is implementation dependent.
		/// It is usually provided for debugging purposes.
		/// </summary>
		public override string ToString()
		{
			return ToString(this, ord);
		}
		
		/// <summary> 
		/// Returns a string representation of an object.
		/// The returned string is implementation dependent.
		/// It is usually provided for debugging purposes.
		/// </summary>
		internal static string ToString(System.Object owner, double[] ord)
		{
			StringBuilder buffer = new StringBuilder();
			buffer.Append('[');
			for (int i = 0; i < ord.Length; i++)
			{
				if (i != 0)
					buffer.Append(", ");
				buffer.Append(ord[i]);
			}
			
            buffer.Append(']');

			return buffer.ToString();
		}

        /// <summary>
        /// Compares the entire members of one array whith the other one.
        /// </summary>
        /// <param name="array1">The array to be compared.</param>
        /// <param name="array2">The array to be compared with.</param>
        /// <returns>
        /// True if both arrays are equals otherwise it returns false.
        /// </returns>
        /// <remarks>
        /// Two arrays are equal if they contains the same elements 
        /// in the same order.
        /// </remarks>
        internal static bool IsArrayEqual(Array array1, Array array2)
        {
            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (!(array1.GetValue(i).Equals(array2.GetValue(i))))
                {
                    return false;
                }
            }

            return true;
        }

    }
}