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

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> 
	/// Implements an algorithm to compute the sign of a 2x2 determinant for double 
	/// precision values robustly.
	/// </summary>
	/// <remarks>
	/// It is a direct translation of code developed by Olivier Devillers.
	/// <para>
	/// The original code carries the following copyright notice:
	/// </para>  
	/// <para>
	/// Author : Olivier Devillers (Olivier.Devillers@sophia.inria.fr)
	/// </para>
	/// <para>
	/// Copyright (c) 1995  by  INRIA Prisme Project
	/// BP 93 06902 Sophia Antipolis Cedex, France.
	/// All rights reserved
	/// </para>
	/// </remarks>
	public sealed class RobustDeterminant
	{
        private RobustDeterminant()
        {
        }

        /// <summary>
        /// Computes the sign of a 2x2 determinant for double precision values robustly.
        /// </summary>
        /// <param name="x1">Horizontal (x) coordinate of first point.</param>
        /// <param name="y1">Vertical (y) coordinate of first point.</param>
        /// <param name="x2">Horizontal (x) coordinate of second point.</param>
        /// <param name="y2">Vertical (y) coordinate of second point.</param>
        /// <returns></returns>
		public static int SignOfDeterminant(double x1, double y1, double x2, double y2)
		{
			// returns -1 if the determinant is negative,
			// returns  1 if the determinant is positive,
			// retunrs  0 if the determinant is null.
			int sign;
			double swap;
			double k;
			long count = 0;
			
			sign = 1;
			
			/*
			*  testing null entries
			*/
			if ((x1 == 0.0) || (y2 == 0.0))
			{
				if ((y1 == 0.0) || (x2 == 0.0))
				{
					return 0;
				}
				else if (y1 > 0)
				{
					if (x2 > 0)
					{
						return - sign;
					}
					else
					{
						return sign;
					}
				}
				else
				{
					if (x2 > 0)
					{
						return sign;
					}
					else
					{
						return - sign;
					}
				}
			}
			if ((y1 == 0.0) || (x2 == 0.0))
			{
				if (y2 > 0)
				{
					if (x1 > 0)
					{
						return sign;
					}
					else
					{
						return - sign;
					}
				}
				else
				{
					if (x1 > 0)
					{
						return - sign;
					}
					else
					{
						return sign;
					}
				}
			}
			
			/*
			*  making y coordinates positive and permuting the entries
			*/
			/*
			*  so that y2 is the biggest one
			*/
			if (0.0 < y1)
			{
				if (0.0 < y2)
				{
					if (y1 <= y2)
					{
						;
					}
					else
					{
						sign = - sign;
						swap = x1;
						x1 = x2;
						x2 = swap;
						swap = y1;
						y1 = y2;
						y2 = swap;
					}
				}
				else
				{
					if (y1 <= - y2)
					{
						sign = - sign;
						x2 = - x2;
						y2 = - y2;
					}
					else
					{
						swap = x1;
						x1 = - x2;
						x2 = swap;
						swap = y1;
						y1 = - y2;
						y2 = swap;
					}
				}
			}
			else
			{
				if (0.0 < y2)
				{
					if (- y1 <= y2)
					{
						sign = - sign;
						x1 = - x1;
						y1 = - y1;
					}
					else
					{
						swap = - x1;
						x1 = x2;
						x2 = swap;
						swap = - y1;
						y1 = y2;
						y2 = swap;
					}
				}
				else
				{
					if (y1 >= y2)
					{
						x1 = - x1;
						y1 = - y1;
						x2 = - x2;
						y2 = - y2;
						;
					}
					else
					{
						sign = - sign;
						swap = - x1;
						x1 = - x2;
						x2 = swap;
						swap = - y1;
						y1 = - y2;
						y2 = swap;
					}
				}
			}
			
			/*
			*  making x coordinates positive
			*/
			/*
			*  if |x2| < |x1| one can conclude
			*/
			if (0.0 < x1)
			{
				if (0.0 < x2)
				{
					if (x1 <= x2)
					{
						;
					}
					else
					{
						return sign;
					}
				}
				else
				{
					return sign;
				}
			}
			else
			{
				if (0.0 < x2)
				{
					return - sign;
				}
				else
				{
					if (x1 >= x2)
					{
						sign = - sign;
						x1 = - x1;
						x2 = - x2;
						;
					}
					else
					{
						return - sign;
					}
				}
			}
			
			/*
			*  all entries strictly positive   x1 <= x2 and y1 <= y2
			*/
			while (true)
			{
				count = count + 1;
				k = Math.Floor(x2 / x1);
				x2 = x2 - k * x1;
				y2 = y2 - k * y1;
				
				/*
				*  testing if R (new U2) is in U1 rectangle
				*/
				if (y2 < 0.0)
				{
					return - sign;
				}
				if (y2 > y1)
				{
					return sign;
				}
				
				/*
				*  finding R'
				*/
				if (x1 > x2 + x2)
				{
					if (y1 < y2 + y2)
					{
						return sign;
					}
				}
				else
				{
					if (y1 > y2 + y2)
					{
						return - sign;
					}
					else
					{
						x2 = x1 - x2;
						y2 = y1 - y2;
						sign = - sign;
					}
				}
				if (y2 == 0.0)
				{
					if (x2 == 0.0)
					{
						return 0;
					}
					else
					{
						return - sign;
					}
				}
				if (x2 == 0.0)
				{
					return sign;
				}
				
				/*
				*  exchange 1 and 2 role.
				*/
				k = Math.Floor(x1 / x2);
				x1 = x1 - k * x2;
				y1 = y1 - k * y2;
				
				/*
				*  testing if R (new U1) is in U2 rectangle
				*/
				if (y1 < 0.0)
				{
					return sign;
				}
				if (y1 > y2)
				{
					return - sign;
				}
				
				/*
				*  finding R'
				*/
				if (x2 > x1 + x1)
				{
					if (y2 < y1 + y1)
					{
						return - sign;
					}
				}
				else
				{
					if (y2 > y1 + y1)
					{
						return sign;
					}
					else
					{
						x1 = x2 - x1;
						y1 = y2 - y1;
						sign = - sign;
					}
				}
				if (y1 == 0.0)
				{
					if (x1 == 0.0)
					{
						return 0;
					}
					else
					{
						return sign;
					}
				}
				if (x1 == 0.0)
				{
					return - sign;
				}
			}
		}
	}
}