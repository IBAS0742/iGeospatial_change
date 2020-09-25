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

namespace iGeospatial.Geometries.Operations.Precision
{
	/// <summary> 
	/// Determines the maximum number of common most-significant
	/// bits in the mantissa of one or numbers.
	/// </summary>
	/// <remarks>
	/// This can be used to compute the double-precision number which
	/// is represented by the common bits.
	/// If there are no common bits, the number computed is 0.0.
	/// </remarks>
	internal sealed class CommonBits
	{
        #region Private Fields

        private bool isFirst = true;
        private int commonMantissaBitsCount = 53;
        private long commonBits;
        private long commonSignExp;
        
        #endregion
		
        #region Constructors and Destructor
        
        public CommonBits()
        {
        }
        
        #endregion
		
        #region Public Properties

		public double Common
		{
			get
			{
				return BitConverter.Int64BitsToDouble(commonBits);
			}
		}
        
        #endregion
		
        #region Public Methods
		
		public void  Add(double num)
		{
			long numBits = BitConverter.DoubleToInt64Bits(num);
			if (isFirst)
			{
				commonBits = numBits;
				commonSignExp = SignExpBits(commonBits);
				isFirst = false;
				return ;
			}
			
			long numSignExp = SignExpBits(numBits);
			if (numSignExp != commonSignExp)
			{
				commonBits = 0;
				return ;
			}
			
			//    System.out.println(toString(commonBits));
			//    System.out.println(toString(numBits));
			commonMantissaBitsCount = NumCommonMostSigMantissaBits(commonBits, numBits);
			commonBits = ZeroLowerBits(commonBits, 64 - (12 + commonMantissaBitsCount));
			//    System.out.println(toString(commonBits));
		}

		/// <summary> 
		/// A representation of the Double bits formatted for easy readability
		/// </summary>
		public string ToString(long bits)
		{
			double x = BitConverter.Int64BitsToDouble(bits);
			string numStr = System.Convert.ToString(bits, 2);
			string padStr = "0000000000000000000000000000000000000000000000000000000000000000" + numStr;
			string bitStr = padStr.Substring(padStr.Length - 64);
			string str = bitStr.Substring(0, (1) - (0)) + "  " + bitStr.Substring(1, (12) - (1)) + "(exp) " + bitStr.Substring(12) + " [ " + x + " ]";
			return str;
		}
        
        #endregion

        #region Public Static Methods

		/// <summary> Computes the bit pattern for the sign and exponent of a
		/// double-precision number.
		/// </summary>
		/// <param name="">num
		/// </param>
		/// <returns> the bit pattern for the sign and exponent
		/// </returns>
		public static long SignExpBits(long num)
		{
			return num >> 52;
		}
		
		/// <summary> This computes the number of common most-significant bits in the mantissas
		/// of two double-precision numbers.
		/// It does not count the hidden bit, which is always 1.
		/// It does not determine whether the numbers have the same exponent - if they do
		/// not, the value computed by this function is meaningless.
		/// </summary>
		/// <param name="">db
		/// </param>
		/// <returns> the number of common most-significant mantissa bits
		/// </returns>
		public static int NumCommonMostSigMantissaBits(long num1, long num2)
		{
			int count = 0;
			for (int i = 52; i >= 0; i--)
			{
				if (GetBit(num1, i) != GetBit(num2, i))
					return count;

				count++;
			}
			return 52;
		}
		
		/// <summary> Zeroes the lower n bits of a bitstring.</summary>
		/// <param name="bits">the bitstring to alter
		/// </param>
		/// <param name="i">the number of bits to zero
		/// </param>
		/// <returns> the zeroed bitstring
		/// </returns>
		public static long ZeroLowerBits(long bits, int nBits)
		{
			long invMask = (1L << nBits) - 1L;
			long mask = ~invMask;
			long zeroed = bits & mask;
			return zeroed;
		}
		
		/// <summary> Extracts the i'th bit of a bitstring.</summary>
		/// <param name="bits">the bitstring to extract from
		/// </param>
		/// <param name="i">the bit to extract
		/// </param>
		/// <returns> the value of the extracted bit
		/// </returns>
		public static int GetBit(long bits, int i)
		{
			long mask = (1L << i);
			return (bits & mask) != 0?1:0;
		}
        
        #endregion
	}
}