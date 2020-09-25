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
using System.Globalization;

namespace iGeospatial.Geometries.Indexers.QuadTree
{
	
	/// <summary> 
	/// DoubleBits manipulates Double numbers by using bit manipulation and 
	/// bit-field extraction.
	/// </summary>
	/// <remarks>
	/// For some operations (such as determining the exponent)
	/// this is more accurate than using mathematical operations
	/// (which suffer from round-off error).
	/// <para>
	/// The algorithms and constants in this class apply only to IEEE-754 
	/// double-precision floating point format.
	/// </para>
	/// </remarks>
	[Serializable]
    internal sealed class DoubleBits
	{
        private double x;
        private long xBits;
		          
        public const int ExponentBias = 1023;
		
        public DoubleBits(double x)
        {
            this.x = x;

            xBits = DoubleToLongBits(x);
        }
		
		public double GetDouble()
		{
            return LongBitsToDouble(xBits);
		}

		/// <summary> Determines the exponent for the number</summary>
		public int GetExponent()
		{
            return BiasedExponent() - ExponentBias;
		}
		
		public static double PowerOf2(int exp)
		{
			if (exp > 1023 || exp < - 1022)
				throw new System.ArgumentException("Exponent out of bounds");
			long expBias = exp + ExponentBias;
			long bits = (long) expBias << 52;

            return DoubleToLongBits(bits);
		}
		
		public static int Exponent(double d)
		{
			DoubleBits db = new DoubleBits(d);
			return db.GetExponent();
		}
		
		public static double TruncateToPowerOfTwo(double d)
		{
			DoubleBits db = new DoubleBits(d);
			db.ZeroLowerBits(52);

			return db.GetDouble();
		}
		
		public static string ToBinaryString(double d)
		{
			DoubleBits db = new DoubleBits(d);

            return db.ToString();
		}
		
		public static double MaximumCommonMantissa(double d1, double d2)
		{
			if (d1 == 0.0 || d2 == 0.0)
				return 0.0;
			
			DoubleBits db1 = new DoubleBits(d1);
			DoubleBits db2 = new DoubleBits(d2);
			
			if (db1.GetExponent() != db2.GetExponent())
				return 0.0;
			
			int maxCommon = db1.NumCommonMantissaBits(db2);
			db1.ZeroLowerBits(64 - (12 + maxCommon));
			return db1.GetDouble();
		}
		
		/// <summary> Determines the exponent for the number</summary>
		public int BiasedExponent()
		{
			int signExp = (int) (xBits >> 52);
			int exp = signExp & 0x07ff;
			return exp;
		}
		
		public void  ZeroLowerBits(int nBits)
		{
			long invMask = (1L << nBits) - 1L;
			long mask = ~ invMask;
			xBits &= mask;
		}
		
		public int GetBit(int i)
		{
			long mask = (1L << i);
			return (xBits & mask) != 0?1:0;
		}
		
		/// <summary> This computes the number of common most-significant bits in the mantissa.
		/// It does not count the hidden bit, which is always 1.
		/// It does not determine whether the numbers have the same exponent - if they do
		/// not, the value computed by this function is meaningless.
		/// </summary>
		/// <param name="">db
		/// </param>
		/// <returns> the number of common most-significant mantissa bits
		/// </returns>
		public int NumCommonMantissaBits(DoubleBits db)
		{
			for (int i = 0; i < 52; i++)
			{
//				int bitIndex = i + 12;
				if (GetBit(i) != db.GetBit(i))
					return i;
			}
			return 52;
		}
		
		/// <summary> A representation of the Double bits formatted for easy readability</summary>
		public override string ToString()
		{
			string numStr = System.Convert.ToString(xBits, 2);
			// 64 zeroes!
			string zero64 = "0000000000000000000000000000000000000000000000000000000000000000";
			string padStr = zero64 + numStr;
			string bitStr = padStr.Substring(padStr.Length - 64);
			string str = bitStr.Substring(0, (1) - (0)) + "  " + 
                bitStr.Substring(1, (12) - (1)) + "(" + 
                GetExponent().ToString(CultureInfo.InvariantCulture.NumberFormat) 
                + ") " + bitStr.Substring(12) + " [ " + x + " ]";

			return str;
		}

		/// <summary>
		/// Simulation of the Java function
		/// </summary>
		/// <param name="bits"></param>
		/// <returns></returns>
		public static double LongBitsToDouble(long bits)
		{
			return BitConverter.Int64BitsToDouble(bits);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static long DoubleToLongBits(double number)
		{
			return BitConverter.DoubleToInt64Bits(number);
		}
	}
}