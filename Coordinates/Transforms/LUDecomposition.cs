using System;

namespace iGeospatial.Coordinates.Transforms
{
	
	/// <summary>LU Decomposition.</summary>
	/// <remarks>
	/// For an m-by-n matrix A with m >= n, the LU decomposition is an m-by-n
	/// unit lower triangular matrix L, an n-by-n upper triangular matrix U,
	/// and a permutation vector piv of length m so that A(piv,:) = L*U.
	/// <code> If m &lt; n, then L is m-by-m and U is m-by-n. </code>
	/// The LU decompostion with pivoting always exists, even if the matrix is
	/// singular, so the constructor will never fail.  The primary use of the
	/// LU decomposition is in the solution of square systems of simultaneous
	/// linear equations.  This will fail if IsNonSingular() returns false.
	/// </remarks>
	[Serializable]
	internal class LUDecomposition
	{
		#region Private Fields
		
		/// <summary>Array for internal storage of decomposition.</summary>
		private double[][] LU;
		
		/// <summary>Row and column dimensions, and pivot sign.</summary>
		private int m, n, pivsign;
		
		/// <summary>Internal storage of pivot vector.</summary>
		private int[] piv;
		
		#endregion //Private Fields

        #region Constructors and Destructor
		
		/// <summary>
		/// Initializes a new LU Decomposition from the specified matrix.
		/// </summary>
		/// <param name="A">
		/// A rectangular matrix to be decomposed. It will become the target matrix for
		/// the LU operations.
		/// </param>
		public LUDecomposition(GeneralMatrix A)
		{
			// Use a "left-looking", dot-product, Crout/Doolittle algorithm.
			
			LU = A.ArrayCopy;
			m = A.RowDimension;
			n = A.ColumnDimension;
			piv = new int[m];
			for (int i = 0; i < m; i++)
			{
				piv[i] = i;
			}
			pivsign = 1;
			double[] LUrowi;
			double[] LUcolj = new double[m];
			
			// Outer loop.
			for (int j = 0; j < n; j++)
			{
				// Make a copy of the j-th column to localize references.
				for (int i = 0; i < m; i++)
				{
					LUcolj[i] = LU[i][j];
				}
				
				// Apply previous transformations.
				for (int i = 0; i < m; i++)
				{
					LUrowi = LU[i];
					
					// Most of the time is spent in the following dot product.
					int kmax = System.Math.Min(i, j);
					double s = 0.0;
					for (int k = 0; k < kmax; k++)
					{
						s += LUrowi[k] * LUcolj[k];
					}
					
					LUrowi[j] = LUcolj[i] -= s;
				}
				
				// Find pivot and exchange if necessary.
				int p = j;
				for (int i = j + 1; i < m; i++)
				{
					if (System.Math.Abs(LUcolj[i]) > System.Math.Abs(LUcolj[p]))
					{
						p = i;
					}
				}

				if (p != j)
				{
					for (int k = 0; k < n; k++)
					{
						double t = LU[p][k]; LU[p][k] = LU[j][k]; LU[j][k] = t;
					}
					int k2 = piv[p]; piv[p] = piv[j]; piv[j] = k2;
					pivsign = - pivsign;
				}
				
				// Compute multipliers.
				if (j < m & LU[j][j] != 0.0)
				{
					for (int i = j + 1; i < m; i++)
					{
						LU[i][j] /= LU[j][j];
					}
				}
			}
		}

		#endregion //  Constructor
				
		#region Public Properties

		/// <summary>Determines whether the matrix is nonsingular.</summary>
		/// <value>
		/// true if upper triangular factor, and hence the matrix, is nonsingular.
		/// </value>
		public bool IsNonSingular
		{
			get
			{
				for (int j = 0; j < n; j++)
				{
					if (LU[j][j] == 0)
						return false;
				}
				return true;
			}
		}

		/// <summary>Gets the lower triangular factor.</summary>
		/// <value>A <see cref="GeneralMatrix"/> of the upper triangular factor.</value>
		public GeneralMatrix L
		{
			get
			{
				GeneralMatrix X = new GeneralMatrix(m, n);
				double[][] L = X.Array;
				for (int i = 0; i < m; i++)
				{
					for (int j = 0; j < n; j++)
					{
						if (i > j)
						{
							L[i][j] = LU[i][j];
						}
						else if (i == j)
						{
							L[i][j] = 1.0;
						}
						else
						{
							L[i][j] = 0.0;
						}
					}
				}
				return X;
			}
		}

		/// <summary>Gets the upper triangular factor.</summary>
		/// <value>A <see cref="GeneralMatrix"/> of the uppert triangular factor.</value>
		public GeneralMatrix U
		{
			get
			{
				GeneralMatrix X = new GeneralMatrix(n, n);
				double[][] U = X.Array;
				for (int i = 0; i < n; i++)
				{
					for (int j = 0; j < n; j++)
					{
						if (i <= j)
						{
							U[i][j] = LU[i][j];
						}
						else
						{
							U[i][j] = 0.0;
						}
					}
				}

				return X;
			}
		}

		/// <summary>Gets the pivot permutation vector</summary>
		/// <value>An array of the pivot permutation vector.</value>
		public int[] Pivot
		{
			get
			{
				int[] p = new int[m];
				for (int i = 0; i < m; i++)
				{
					p[i] = piv[i];
				}

				return p;
			}
		}

		/// <summary>
		/// Gets the pivot permutation vector as a one-dimensional double array.
		/// </summary>
		/// <value>An double array of pivot permutation vector.</value>
		public double[] DoublePivot
		{
			get
			{
				double[] vals = new double[m];
				for (int i = 0; i < m; i++)
				{
					vals[i] = (double) piv[i];
				}

				return vals;
			}
		}

		#endregion //  Public Properties
		
		#region Public Methods
		
		/// <summary>Calculates the determinant of the target matrix.</summary>
		/// <returns> The determinant of the target matrix.</returns>
		/// <exception cref="System.ArgumentException"> 
		/// If the matrix is not square.
		/// </exception>
		public double Determinant()
		{
			if (m != n)
			{
				throw new System.ArgumentException("Matrix must be square.");
			}
			double d = (double) pivsign;
			for (int j = 0; j < n; j++)
			{
				d *= LU[j][j];
			}

			return d;
		}
		
		/// <summary>Solve A*X = B</summary>
		/// <param name="B">  
		/// A Matrix with as many rows as A and any number of columns.
		/// </param>
		/// <returns>X so that L*U*X = B(piv,:)</returns>
		/// <exception cref="System.ArgumentException"> 
		/// If the matrix row dimensions do not agree.
		/// </exception>
		/// <exception cref="System.SystemException">
		/// If the matrix is singular.
		/// </exception>
		public GeneralMatrix Solve(GeneralMatrix B)
		{
			if (B.RowDimension != m)
			{
				throw new System.ArgumentException("Matrix row dimensions must agree.");
			}
			if (!this.IsNonSingular)
			{
				throw new System.SystemException("Matrix is singular.");
			}
			
			// Copy right hand side with pivoting
			int nx = B.ColumnDimension;
			GeneralMatrix Xmat = B.GetMatrix(piv, 0, nx - 1);
			double[][] X       = Xmat.Array;
			
			// Solve L*Y = B(piv,:)
			for (int k = 0; k < n; k++)
			{
				for (int i = k + 1; i < n; i++)
				{
					for (int j = 0; j < nx; j++)
					{
						X[i][j] -= X[k][j] * LU[i][k];
					}
				}
			}

			// Solve U*X = Y;
			for (int k = n - 1; k >= 0; k--)
			{
				for (int j = 0; j < nx; j++)
				{
					X[k][j] /= LU[k][k];
				}
				for (int i = 0; i < k; i++)
				{
					for (int j = 0; j < nx; j++)
					{
						X[i][j] -= X[k][j] * LU[i][k];
					}
				}
			}
			return Xmat;
		}

		#endregion //  Public Methods
	}
}