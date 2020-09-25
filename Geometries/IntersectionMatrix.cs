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
using System.Text;
using System.Diagnostics;
using System.Globalization;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries
{
	/// <summary>  
	/// A Dimensionally Extended Nine-Intersection Model (DE-9IM) matrix. This class
	/// can used to represent both computed DE-9IM's (like 212FF1FF2) as well as
	/// patterns for matching them (like T*T******).
	/// </summary>
	/// <remarks>
	/// <para>
	/// Methods are provided to:
	/// <list type="number">
	/// <item>
	/// <description>
	/// set and query the elements of the matrix in a convenient fashion
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// convert to and from the standard string representation (specified in
	/// SFS Section 2.1.13.2).
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// test to see if a matrix matches a given pattern string.
	/// </description>
	/// </item>
	/// </list>
	/// </para>  
	/// For a description of the DE-9IM, see the 
	/// <see href="http://www.opengis.org/techno/specs.htm">OpenGIS Simple Features
	/// Specification for SQL</see>.
	/// </remarks>
	[Serializable]
    public sealed class IntersectionMatrix : IIntersectionMatrix
	{
        #region Private Fields

		/// <summary>  Internal representation of this IntersectionMatrix.</summary>
		private int[][] matrix;
        
        #endregion
		
        #region Constructors and Destructor
		
        /// <summary> 
		/// Creates an IntersectionMatrix with DimensionType.Empty dimension values.
		/// </summary>
		public IntersectionMatrix()
		{
			matrix = new int[3][];
			for (int i = 0; i < 3; i++)
			{
				matrix[i] = new int[3];
			}

			this.SetAll((int)DimensionType.Empty);
		}
		
		/// <summary>  Creates an IntersectionMatrix with the given dimension
		/// symbols.
		/// 
		/// </summary>
		/// <param name="elements"> a String of nine dimension symbols in row major order
		/// </param>
		public IntersectionMatrix(string elements) : this()
		{
			SetValues(elements);
		}
		
		/// <summary>  
		/// Creates an IntersectionMatrix with the same elements as
		/// other. 
		/// </summary>
		/// <param name="other">
		/// An IntersectionMatrix to copy.
		/// </param>
		public IntersectionMatrix(IntersectionMatrix other) : this()
		{
			matrix[LocationType.Interior][LocationType.Interior] = 
                other.matrix[LocationType.Interior][LocationType.Interior];
			matrix[LocationType.Interior][LocationType.Boundary] = 
                other.matrix[LocationType.Interior][LocationType.Boundary];
			matrix[LocationType.Interior][LocationType.Exterior] = 
                other.matrix[LocationType.Interior][LocationType.Exterior];
			matrix[LocationType.Boundary][LocationType.Interior] = 
                other.matrix[LocationType.Boundary][LocationType.Interior];
			matrix[LocationType.Boundary][LocationType.Boundary] = 
                other.matrix[LocationType.Boundary][LocationType.Boundary];
			matrix[LocationType.Boundary][LocationType.Exterior] = 
                other.matrix[LocationType.Boundary][LocationType.Exterior];
			matrix[LocationType.Exterior][LocationType.Interior] = 
                other.matrix[LocationType.Exterior][LocationType.Interior];
			matrix[LocationType.Exterior][LocationType.Boundary] = 
                other.matrix[LocationType.Exterior][LocationType.Boundary];
			matrix[LocationType.Exterior][LocationType.Exterior] = 
                other.matrix[LocationType.Exterior][LocationType.Exterior];
		}
        
        #endregion
		
        #region Public Properties

		/// <summary>  
		/// Returns true if this IntersectionMatrix is FF*FF****.
		/// </summary>
		/// <returns>    true if the two <see cref="Geometry"/> instances related by
		/// this IntersectionMatrix are Disjoint
		/// </returns>
		public bool Disjoint
		{
			get
			{
				return matrix[LocationType.Interior][LocationType.Interior] == (int)DimensionType.Empty && 
                    matrix[LocationType.Interior][LocationType.Boundary] == (int)DimensionType.Empty && 
                    matrix[LocationType.Boundary][LocationType.Interior] == (int)DimensionType.Empty && 
                    matrix[LocationType.Boundary][LocationType.Boundary] == (int)DimensionType.Empty;
			}
		}

		/// <summary>  
		/// Returns true if isDisjoint returns false.
		/// </summary>
		/// <returns> true if the two <see cref="Geometry"/> instances related by
		/// this IntersectionMatrix intersect
		/// </returns>
		public bool Intersects
		{
			get
			{
				return !Disjoint;
			}
		}
		
		/// <summary>  Returns true if this IntersectionMatrix is
		/// T*F**F***.
		/// 
		/// </summary>
		/// <returns>    true if the first Geometry is Within
		/// the second
		/// </returns>
		public bool Within
		{
			get
			{
				return Matches(matrix[LocationType.Interior][LocationType.Interior], 'T') && 
                    matrix[LocationType.Interior][LocationType.Exterior] == (int)DimensionType.Empty && 
                    matrix[LocationType.Boundary][LocationType.Exterior] == (int)DimensionType.Empty;
			}
		}
		
		/// <summary>  Returns true if this IntersectionMatrix is
		/// T*****FF*.
		/// 
		/// </summary>
		/// <returns>    true if the first Geometry Contains the
		/// second
		/// </returns>
		public bool Contains
		{
			get
			{
				return Matches(matrix[LocationType.Interior][LocationType.Interior], 'T') && 
                    matrix[LocationType.Exterior][LocationType.Interior] == (int)DimensionType.Empty && 
                    matrix[LocationType.Exterior][LocationType.Boundary] == (int)DimensionType.Empty;
			}
		}
        
        /// <summary>  
        /// Returns <see langword="true"/> if this <c>IntersectionMatrix</c> is
        /// <c>T*****FF*</c>
        /// or <c>*T****FF*</c>
        /// or <c>***T**FF*</c>
        /// or <c>****T*FF*</c>
        /// 
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the first <c>Geometry</c> covers the
        /// second
        /// </returns>
        public bool Covers
        {
            get
            {
                bool hasPointInCommon = Matches(
                    matrix[LocationType.Interior][LocationType.Interior], 'T') || 
                    Matches(matrix[LocationType.Interior][LocationType.Boundary], 'T') || 
                    Matches(matrix[LocationType.Boundary][LocationType.Interior], 'T') || 
                    Matches(matrix[LocationType.Boundary][LocationType.Boundary], 'T');
				
                return hasPointInCommon && 
                    matrix[LocationType.Exterior][LocationType.Interior] == 
                    (int)DimensionType.Empty && 
                    matrix[LocationType.Exterior][LocationType.Boundary] == 
                    (int)DimensionType.Empty;
            }
        }
			
        /// <summary>  
        /// Returns <see langword="true"/> if this <c>IntersectionMatrix</c> is
        /// <c>T*F**F***</c>
        /// or <c>*TF**F***</c>
        /// or <c>**FT*F***</c>
        /// or <c>**F*TF***</c>
        /// 
        /// </summary>
        /// <returns>    
        /// <see langword="true"/> if the first <c>Geometry</c>
        /// is covered by the second
        /// </returns>
        public bool IsCoveredBy
        {
            get
            {
                bool hasPointInCommon = 
                    Matches(matrix[LocationType.Interior][LocationType.Interior], 'T') || 
                    Matches(matrix[LocationType.Interior][LocationType.Boundary], 'T') || 
                    Matches(matrix[LocationType.Boundary][LocationType.Interior], 'T') || 
                    Matches(matrix[LocationType.Boundary][LocationType.Boundary], 'T');
				
                return hasPointInCommon && 
                    matrix[LocationType.Interior][LocationType.Exterior] == 
                    (int)DimensionType.Empty && 
                    matrix[LocationType.Boundary][LocationType.Exterior] == 
                    (int)DimensionType.Empty;
            }
        }
			
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// Adds one matrix to another.
		/// </summary>
		/// <param name="input">the matrix to Add
		/// </param>
		/// <remarks>
		/// Addition is defined by taking the maximum dimension value of 
		/// each position in the summand matrices.
		/// </remarks>
		public void Add(IntersectionMatrix input)
		{
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					SetAtLeast(i, j, input.GetValue(i, j));
				}
			}
		}
		
		/// <summary>  
		/// Returns the value of one of this IntersectionMatrixs
		/// elements.
		/// </summary>
		/// <param name="row">    the row of this IntersectionMatrix, indicating
		/// the interior, boundary or exterior of the first Geometry
		/// </param>
		/// <param name="column"> the column of this IntersectionMatrix,
		/// indicating the interior, boundary or exterior of the second Geometry
		/// </param>
		/// <returns>         the dimension value at the given matrix position.
		/// </returns>
		public int GetValue(int row, int column)
		{
			return matrix[row][column];
		}
		
		/// <summary>  
		/// Changes the value of one of this IntersectionMatrixs
		/// elements.
		/// </summary>
		/// <param name="row"> 
		/// The row of this <see cref="IntersectionMatrix"/>, indicating 
		/// the interior, boundary or exterior of the first 
		/// <see cref="Geometry"/>.
		/// </param>
		/// <param name="column"> 
		/// The column of this <see cref="IntersectionMatrix"/>, indicating 
		/// the interior, boundary or exterior of the second 
		/// <see cref="Geometry"/>.
		/// </param>
		/// <param name="dimensionValue"> 
		/// The new value of the element.
		/// </param>
		public void SetValue(int row, int column, int dimensionValue)
		{
			matrix[row][column] = dimensionValue;
		}
		
		/// <summary>  
		/// Changes the elements of this IntersectionMatrix to the
		/// dimension symbols in dimensionSymbols.
		/// </summary>
		/// <param name="dimensionSymbols"> 
		/// Nine dimension symbols to which to set this IntersectionMatrix
		/// s elements. Possible values are {T, F, * , 0, 1, 2}
		/// </param>
		public void SetValues(string dimensionSymbols)
		{
            if (dimensionSymbols == null)
            {
                throw new ArgumentNullException("dimensionSymbols");
            }

            int nLength = dimensionSymbols.Length;
            for (int i = 0; i < nLength; i++)
			{
				int row = i / 3;
				int col = i % 3;
				matrix[row][col] = (int)GetDimensionValue(dimensionSymbols[i]);
			}
		}
		
		/// <summary>  
		/// Changes the specified element to minimum dimension value if the
		/// element is less.
		/// </summary>
		/// <param name="row"> 
		/// The row of this <see cref="IntersectionMatrix"/>, indicating the interior, 
		/// boundary or exterior of the first <see cref="Geometry"/>.
		/// </param>
		/// <param name="column"> 
		/// The column of this <see cref="IntersectionMatrix"/>, 
		/// indicating the interior, boundary or exterior of the 
		/// second <see cref="Geometry"/>.
		/// </param>
		/// <param name="minimumDimensionValue"> 
		/// The dimension value with which to compare the element. 
		/// The order of dimension values from least to greatest is
		/// {DONTCARE, TRUE, FALSE, 0, 1, 2}.
		/// </param>
		public void SetAtLeast(int row, int column, int minimumDimensionValue)
		{
			if (matrix[row][column] < minimumDimensionValue)
			{
				matrix[row][column] = minimumDimensionValue;
			}
		}
		
		/// <summary>  
		/// If row >= 0 and column >= 0, changes the specified element to minimumDimensionValue
		/// if the element is less. Does nothing if row less than 0 or column less than 0.
		/// </summary>
		/// <param name="row">
		/// The row of this IntersectionMatrix, indicating the interior, boundary 
		/// or exterior of the first Geometry.
		/// </param>
		/// <param name="column">
		/// The column of this IntersectionMatrix, indicating the interior, 
		/// boundary or exterior of the second Geometry.
		/// </param>
		/// <param name="minimumDimensionValue">
		/// The dimension value with which to Compare the element. The order of 
		/// dimension values from least to greatest is {DONTCARE, TRUE, FALSE, 0, 1, 2}.
		/// </param>
		public void SetAtLeastIfValid(int row, int column, int minimumDimensionValue)
		{
			if (row >= 0 && column >= 0)
			{
				SetAtLeast(row, column, minimumDimensionValue);
			}
		}
		
		/// <summary> 
		///  For each element in this IntersectionMatrix, changes the
		/// element to the corresponding minimum dimension symbol if the element is less.
		/// </summary>
		/// <param name="minimumDimensionSymbols"> nine dimension symbols with which to
		/// Compare the elements of this IntersectionMatrix. The
		/// order of dimension values from least to greatest is 
		/// {DONTCARE, TRUE, FALSE, 0, 1, 2}.
		/// </param>
		public void SetAtLeast(string minimumDimensionSymbols)
		{
            if (minimumDimensionSymbols == null)
            {
                throw new ArgumentNullException("minimumDimensionSymbols");
            }

            int nCount = minimumDimensionSymbols.Length;
            for (int i = 0; i < nCount; i++)
			{
				int row = i / 3;
				int col = i % 3;
				SetAtLeast(row, col, 
                    (int)GetDimensionValue(minimumDimensionSymbols[i]));
			}
		}

		/// <summary>  
		/// Changes the elements of this IntersectionMatrix to dimensionValue.
		/// </summary>
		/// <param name="dimensionValue"> 
		/// The dimension value to which to set this IntersectionMatrix
		/// s elements. Possible values {TRUE, FALSE, DONTCARE, 0, 1, 2}.
		/// </param>
		public void SetAll(int value)
		{
            for (int ai = 0; ai < 3; ai++)
            {
                for (int bi = 0; bi < 3; bi++)
                {
                    matrix[ai][bi] = value;
                }
            }
		}
		
		/// <summary>  
		/// Returns true if this IntersectionMatrix is
		/// FT*******, F**T***** or F***T****.
		/// </summary>
		/// <param name="geometryA"> the dimension of the first Geometry
		/// </param>
		/// <param name="geometryB"> the dimension of the second Geometry
		/// </param>
		/// <returns>true if the two Geometry instances related by this 
		/// IntersectionMatrix touch; returns false if both Geometry instances are points.
		/// </returns>
		public bool IsTouches(DimensionType geometryA, 
            DimensionType geometryB)
		{
			if (geometryA > geometryB)
			{
				//no need to get transpose because pattern matrix is symmetrical
				return IsTouches(geometryB, geometryA);
			}

			if ((geometryA == DimensionType.Surface  && 
                geometryB  == DimensionType.Surface) || 
                (geometryA == DimensionType.Curve  && 
                geometryB  == DimensionType.Curve) || 
                (geometryA == DimensionType.Curve  && 
                geometryB  == DimensionType.Surface) || 
                (geometryA == DimensionType.Point  && 
                geometryB  == DimensionType.Surface) || 
                (geometryA == DimensionType.Point && 
                geometryB  == DimensionType.Curve))
			{
				return matrix[LocationType.Interior][LocationType.Interior] == (int)DimensionType.Empty && 
                    (Matches(matrix[LocationType.Interior][LocationType.Boundary], 'T')        || 
                    Matches(matrix[LocationType.Boundary][LocationType.Interior], 'T')         || 
                    Matches(matrix[LocationType.Boundary][LocationType.Boundary], 'T'));
			}

			return false;
		}
		
		/// <summary>  
		/// Returns true if this IntersectionMatrix is
		/// <list type="number">
		/// <item>
		/// <description>
		/// T*T****** (for a point and a curve, a point and an area or a line
		/// and an area)
		/// </description>
		/// </item>
		/// <item>
		/// <description>0******** (for two curves).</description>
		/// </item>
		/// </list>
		/// </summary>
		/// <param name="geometryA"> the dimension of the first Geometry
		/// </param>
		/// <param name="geometryB"> the dimension of the second Geometry
		/// </param>
		/// <returns>true if the two Geometry instances related by this 
		/// IntersectionMatrix cross. For this function to return true, 
		/// the <see cref="Geometry"/> instances must be a point and a curve; 
		/// a point and a surface; two curves; or a curve and a surface.
		/// </returns>
		public bool IsCrosses(DimensionType geometryA, 
            DimensionType geometryB)
		{
			if ((geometryA == DimensionType.Point  && 
                geometryB  == DimensionType.Curve) || 
                (geometryA == DimensionType.Point  && 
                geometryB  == DimensionType.Surface) || 
                (geometryA == DimensionType.Curve  && 
                geometryB  == DimensionType.Surface))
			{
				return Matches(matrix[LocationType.Interior][LocationType.Interior], 'T') && 
                    Matches(matrix[LocationType.Interior][LocationType.Exterior], 'T');
			}
			
            if ((geometryA == DimensionType.Curve  && 
                geometryB  == DimensionType.Point) || 
                (geometryA == DimensionType.Surface  && 
                geometryB  == DimensionType.Point) || 
                (geometryA == DimensionType.Surface  && 
                geometryB  == DimensionType.Curve))
			{
				return Matches(matrix[LocationType.Interior][LocationType.Interior], 'T') && 
                    Matches(matrix[LocationType.Exterior][LocationType.Interior], 'T');
			}

			if (geometryA == DimensionType.Curve && 
                geometryB == DimensionType.Curve)
			{
				return matrix[LocationType.Interior][LocationType.Interior] == 0;
			}

			return false;
		}
		
		/// <summary>  Returns true if this IntersectionMatrix is
		/// T*F**FFF*.
		/// 
		/// </summary>
		/// <param name="geometryA"> the dimension of the first Geometry
		/// </param>
		/// <param name="geometryB"> the dimension of the second Geometry
		/// </param>
		/// <returns>                       true if the two Geometry
		/// s related by this IntersectionMatrix are equal; the
		/// <see cref="Geometry"/> instances must have the same dimension for this function
		/// to return true
		/// </returns>
		public bool IsEquals(DimensionType geometryA, 
            DimensionType geometryB)
		{
			if (geometryA != geometryB)
			{
				return false;
			}

			return Matches(matrix[LocationType.Interior][LocationType.Interior], 'T') && 
                matrix[LocationType.Exterior][LocationType.Interior] == (int)DimensionType.Empty && 
                matrix[LocationType.Interior][LocationType.Exterior] == (int)DimensionType.Empty && 
                matrix[LocationType.Exterior][LocationType.Boundary] == (int)DimensionType.Empty && 
                matrix[LocationType.Boundary][LocationType.Exterior] == (int)DimensionType.Empty;
		}
		
		/// <summary>  
		/// Returns true if this IntersectionMatrix is
		/// <list type="number">
		/// <item>
		/// <description>T*T***T** (for two points or two surfaces)</description>
		/// </item>
		/// <item>
		/// <description>1*T***T** (for two curves).</description>
		/// </item>
		/// </list>
		/// </summary>
		/// <param name="geometryA"> the dimension of the first Geometry
		/// </param>
		/// <param name="geometryB"> the dimension of the second Geometry
		/// </param>
		/// <returns>                       true if the two Geometry
		/// s related by this IntersectionMatrix overlap. For this
		/// function to return true, the <see cref="Geometry"/> instances must
		/// be two points, two curves or two surfaces.
		/// </returns>
		public bool IsOverlaps(DimensionType geometryA, 
            DimensionType geometryB)
		{
			if ((geometryA == DimensionType.Point  && 
                geometryB  == DimensionType.Point) || 
                (geometryA == DimensionType.Surface  && 
                geometryB  == DimensionType.Surface))
			{
				return Matches(matrix[LocationType.Interior][LocationType.Interior], 'T') && 
                    Matches(matrix[LocationType.Interior][LocationType.Exterior], 'T')    && 
                    Matches(matrix[LocationType.Exterior][LocationType.Interior], 'T');
			}

			if (geometryA == DimensionType.Curve && 
                geometryB == DimensionType.Curve)
			{
				return matrix[LocationType.Interior][LocationType.Interior] == 1       && 
                    Matches(matrix[LocationType.Interior][LocationType.Exterior], 'T') && 
                    Matches(matrix[LocationType.Exterior][LocationType.Interior], 'T');
			}

			return false;
		}
		
		/// <summary>  
		/// Returns whether the elements of this IntersectionMatrix
		/// satisfies the required dimension symbols.
		/// </summary>
		/// <param name="requiredDimensionSymbols"> nine dimension symbols with which to
		/// Compare the elements of this IntersectionMatrix. Possible
		/// values are {T, F, * , 0, 1, 2}.
		/// </param>
		/// <returns> 
		/// <see langword="true"/> if this <see cref="IntersectionMatrix"/> matches the 
		/// required dimension symbols.
		/// </returns>
		public bool Matches(string requiredDimensionSymbols)
		{
            if (requiredDimensionSymbols == null)
            {
                throw new ArgumentNullException("requiredDimensionSymbols");
            }

            if (requiredDimensionSymbols.Length != 9)
            {
                throw new System.ArgumentException("Should be length 9: " + requiredDimensionSymbols);
            }

			for (int ai = 0; ai < 3; ai++)
			{
				for (int bi = 0; bi < 3; bi++)
				{
					if (!Matches(matrix[ai][bi], requiredDimensionSymbols[3 * ai + bi]))
					{
						return false;
					}
				}
			}
			
            return true;
		}
		
		/// <summary>  
		/// Transposes this IntersectionMatrix.
		/// </summary>
		/// <returns>
		/// This IntersectionMatrix as a convenience.
		/// </returns>
		public IntersectionMatrix Transpose()
		{
			int temp     = matrix[1][0];
			matrix[1][0] = matrix[0][1];
			matrix[0][1] = temp;
			
            temp         = matrix[2][0];
			matrix[2][0] = matrix[0][2];
			matrix[0][2] = temp;

			temp         = matrix[2][1];
			matrix[2][1] = matrix[1][2];
			matrix[1][2] = temp;

			return this;
		}
		
		/// <summary>  
		/// Returns a nine-character String representation of this IntersectionMatrix.
		/// </summary>
		/// <returns>
		/// The nine dimension symbols of this IntersectionMatrix in row-major order.
		/// </returns>
		public override string ToString()
		{
			StringBuilder buf = new StringBuilder("123456789");

			for (int ai = 0; ai < 3; ai++)
			{
				for (int bi = 0; bi < 3; bi++)
				{
					buf[3 * ai + bi] = GetDimensionSymbol((DimensionType)matrix[ai][bi]);
				}
			}

			return buf.ToString();
		}
        
        #endregion

        #region Public Static Methods
		
		/// <summary>  Returns true if the dimension value satisfies the dimension symbol.
		/// 
		/// </summary>
		/// <param name="actualDimensionValue">
		/// A number that can be stored in the IntersectionMatrix. Possible 
		/// values are {True, False, DontCare, 0, 1, 2}.
		/// </param>
		/// <param name="requiredDimensionSymbol"> a character used in the string
		/// representation of an IntersectionMatrix. Possible values
		/// are {T, F, * , 0, 1, 2}.
		/// </param>
		/// <returns>
		/// true if the dimension symbol encompasses the dimension value.
		/// </returns>
		public static bool Matches(int actualDimensionValue, char requiredDimensionSymbol)
		{
			if (requiredDimensionSymbol == '*')
			{
				return true;
			}

            DimensionType dim = (DimensionType)actualDimensionValue;

			if (requiredDimensionSymbol == 'T' && 
                (actualDimensionValue >= 0 || dim == DimensionType.NonEmpty))
			{
				return true;
			}
			if (requiredDimensionSymbol == 'F' && dim == DimensionType.Empty)
			{
				return true;
			}
			if (requiredDimensionSymbol == '0' && dim == DimensionType.Point)
			{
				return true;
			}
			if (requiredDimensionSymbol == '1' && dim == DimensionType.Curve)
			{
				return true;
			}
			if (requiredDimensionSymbol == '2' && dim == DimensionType.Surface)
			{
				return true;
			}

			return false;
		}
		
		/// <summary>  
		/// Returns true if each of the actual dimension symbols satisfies the
		/// corresponding required dimension symbol.
		/// </summary>
		/// <param name="actualDimensionSymbols">
		/// Nine dimension symbols to validate. Possible values 
		/// are {T, F, * , 0, 1, 2}.
		/// </param>
		/// <param name="requiredDimensionSymbols">
		/// Nine dimension symbols to validate against. Possible values 
		/// are {T, F, * , 0, 1, 2}.
		/// </param>
		/// <returns>
		/// true if each of the required dimension symbols encompass the 
		/// corresponding actual dimension symbol.
		/// </returns>
		public static bool Matches(string actualDimensionSymbols, 
            string requiredDimensionSymbols)
		{
			IntersectionMatrix m = new IntersectionMatrix(actualDimensionSymbols);
			return m.Matches(requiredDimensionSymbols);
		}

        /// <summary>  
        /// Converts the dimension value to a dimension symbol, for example, 
        /// <see cref="DimensionType.NonEmpty"> => 'T'.
        /// </summary>
        /// <param name="dimensionValue"> 
        /// a number that can be stored in the <see cref="IntersectionMatrix"/>. 
        /// Possible values are {NonEmpty, Empty, DontCare, 0, 1, 2}.
        /// </param>
        /// <returns>a character for use in the string representation of
        /// an IntersectionMatrix. Possible values are {T, F, * , 0, 1, 2}.
        /// </returns>
        /// <seealso cref="IntersectionMatrix"/>
        public static char GetDimensionSymbol(DimensionType dimensionValue)
        {
            switch (dimensionValue)
            {
                case DimensionType.Empty: 
                    return 'F';
        		
                case DimensionType.NonEmpty: 
                    return 'T';
        		                              
                case DimensionType.DontCare: 
                    return '*';
        		
                case DimensionType.Point: 
                    return '0';
        		
                case DimensionType.Curve: 
                    return '1';
        		
                case DimensionType.Surface: 
                    return '2';
            }

            throw new ArgumentException("Unknown dimension value: " 
                + dimensionValue);
        }
        
        /// <summary>  
        /// Converts the dimension symbol to a dimension value, 
        /// for example, '*' => DONTCARE.
        /// </summary>
        /// <param name="dimensionSymbol"> 
        /// A character for use in the string representation of
        /// an <see cref="IntersectionMatrix"/>. 
        /// Possible values are {T, F, * , 0, 1, 2}.
        /// </param>
        /// <returns>
        /// A number that can be stored in the <see cref="IntersectionMatrix"/>.
        /// Possible values are {TRUE, FALSE, DONTCARE, 0, 1, 2}.
        /// </returns>
        /// <seealso cref="IntersectionMatrix"/>
        public static DimensionType GetDimensionValue(char dimensionSymbol)
        {
            switch (Char.ToUpper(dimensionSymbol, CultureInfo.InvariantCulture))
            {
                case 'F': 
                    return DimensionType.Empty;
        		
                case 'T': 
                    return DimensionType.NonEmpty;
        		
                case '*': 
                    return DimensionType.DontCare;
        		
                case '0': 
                    return DimensionType.Point;
        		
                case '1': 
                    return DimensionType.Curve;
        		
                case '2': 
                    return DimensionType.Surface;
            }

            throw new ArgumentException("Unknown dimension symbol: " 
                + dimensionSymbol);
        }
        
        #endregion

        #region ICloneable Members

        /// <summary>
        /// Returns an exact copy of this object.
        /// </summary>
        /// <returns>An exact copy of this object.</returns>
        public IntersectionMatrix Clone()
        {
            return new IntersectionMatrix(this);
        }     

        /// <summary>
        /// Returns an exact copy of this object.
        /// </summary>
        /// <returns>An exact copy of this object.</returns>
        IIntersectionMatrix IIntersectionMatrix.Clone()
        {
            return new IntersectionMatrix(this);
        }

        /// <summary>
        /// Returns an exact copy of this object.
        /// </summary>
        /// <returns>An exact copy of this object.</returns>
        object ICloneable.Clone()
        {
            return new IntersectionMatrix(this);
        }     
        
        #endregion

        #region IIntersectionMatrix Members

        IIntersectionMatrix IIntersectionMatrix.Transpose()
        {
            int temp     = matrix[1][0];
            matrix[1][0] = matrix[0][1];
            matrix[0][1] = temp;
			
            temp         = matrix[2][0];
            matrix[2][0] = matrix[0][2];
            matrix[0][2] = temp;

            temp         = matrix[2][1];
            matrix[2][1] = matrix[1][2];
            matrix[1][2] = temp;

            return this;
        }

        void IIntersectionMatrix.Add(IIntersectionMatrix input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    SetAtLeast(i, j, input.GetValue(i, j));
                }
            }
        }

        #endregion
    }
}
