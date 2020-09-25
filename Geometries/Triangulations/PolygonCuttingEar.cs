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
// The Polygon Triangulation is based on codes developed by
//          Frank Shen                                    
//     
// License:
// See the license.txt file in the package directory.   
// </copyright>
#endregion

using System;
using System.Collections;
using System.Diagnostics;

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Triangulations
{
	/// <summary>
	/// Summary description for PolygonCuttingEar. 
	/// </summary>
	[Serializable]
    public sealed class PolygonCuttingEar
	{
        #region Private Fields

		private Coordinate[] m_aInputVertices;
		private Coordinate[] m_aUpdatedPolygonVertices;
				
		private ArrayList m_alEars;

		private Coordinate[][] m_aPolygons;
        
        #endregion

        #region Constructors and Destructor

		public PolygonCuttingEar(Coordinate[] vertices)
		{
			int nVertices = vertices.Length;
			if (nVertices < 3)
			{
				throw new ArgumentException("To make a polygon, "
					+ " at least 3 points are required!");
			}

            m_alEars = new ArrayList();

			//initalize the 2D points
			m_aInputVertices = new Coordinate[nVertices];

            for (int i = 0; i < nVertices; i++)
                m_aInputVertices[i] = vertices[i];
          
			//make a working copy,  m_aUpdatedPolygonVertices are
			//in count clock direction from user view
			SetUpdatedPolygonVertices();
		}
        
        #endregion

        #region Public Properties

		public int NumberOfPolygons
		{
			get
			{
                if (m_aPolygons != null)
                {
                    return m_aPolygons.Length;
                }

                return 0;
			}
		}
        
        #endregion

        #region Public Methods

		public Coordinate[] Polygon(int index)
		{
            if (m_aPolygons != null)
            {
                if (index < m_aPolygons.Length)
                {
                    return m_aPolygons[index];
                }
            }

			return null;
		}

		/// <summary>
		/// To cut an ear from polygon to make ears and an updated polygon.
		/// </summary>
        public void CutEar()
		{
//			PolygonHelper polygon = new PolygonHelper(m_aUpdatedPolygonVertices);
			bool bFinish          = false;

			if (m_aUpdatedPolygonVertices.Length == 3) //triangle, don't have to cut ear
				bFinish = true;
			
			Coordinate pt = new Coordinate();
			while (bFinish == false) //UpdatedPolygon
			{
				int i          = 0;
				bool bNotFound = true;
                //loop till find an ear
				while (bNotFound && (i < m_aUpdatedPolygonVertices.Length)) 
				{
					pt = m_aUpdatedPolygonVertices[i];
					if (IsEarOfUpdatedPolygon(pt))
						bNotFound=false; //got one, pt is an ear
					else
						i++;
				} //bNotFount

				//An ear found:}
				if (pt != null)
					UpdatePolygonVertices(pt);
       
//				polygon = new PolygonHelper(m_aUpdatedPolygonVertices);

				if (m_aUpdatedPolygonVertices.Length == 3)
					bFinish = true;
			} //bFinish=false

			SetPolygons();
		}	
        
        #endregion

        #region Private Methods

		/****************************************************
		To fill m_aUpdatedPolygonVertices array with input array.
		
		m_aUpdatedPolygonVertices is a working array that will 
		be updated when an ear is cut till m_aUpdatedPolygonVertices
		makes triangle (a convex polygon).
	   ******************************************************/
		private void SetUpdatedPolygonVertices()
		{
			int nVertices             = m_aInputVertices.Length;
			m_aUpdatedPolygonVertices = new Coordinate[nVertices];

			for (int i = 0; i< nVertices; i++)
				m_aUpdatedPolygonVertices[i] = m_aInputVertices[i];
			
			//m_aUpdatedPolygonVertices should be in count clock wise
			if (PolygonHelper.PointsDirection(m_aUpdatedPolygonVertices)
				== PolygonDirection.Clockwise)
				PolygonHelper.ReversePointsDirection(m_aUpdatedPolygonVertices);
		}

		/**********************************************************
		To check the Pt is in the Triangle or not.
		If the Pt is in the line or is a vertex, then return true.
		If the Pt is out of the Triangle, then return false.

		This method is used for triangle only.
		***********************************************************/
		private bool TriangleContainsPoint(Coordinate[] trianglePts, Coordinate pt)
		{
			if (trianglePts.Length != 3)
				return false;
 
			for (int i = trianglePts.GetLowerBound(0); 
				i < trianglePts.GetUpperBound(0); i++)
			{
				if (pt.Equals(trianglePts[i]))
					return true;
			}
			
			bool bIn=false;

			LineSegmentEquation line0 = new LineSegmentEquation(
                trianglePts[0],trianglePts[1]);
			LineSegmentEquation line1 = new LineSegmentEquation(
                trianglePts[1],trianglePts[2]);
			LineSegmentEquation line2 = new LineSegmentEquation(
                trianglePts[2],trianglePts[0]);
                                       
			if (line0.OnLine(pt) || line1.OnLine(pt) || line2.OnLine(pt))
            {
                bIn = true;
            }
			else //point is not in the lines
			{
				double dblArea0 = PolygonHelper.PolygonArea(
                    new Coordinate[]{trianglePts[0], trianglePts[1], pt});
				double dblArea1 = PolygonHelper.PolygonArea(
                    new Coordinate[]{trianglePts[1], trianglePts[2], pt});
				double dblArea2 = PolygonHelper.PolygonArea(
                    new Coordinate[]{trianglePts[2], trianglePts[0], pt});

				if (dblArea0 > 0)
				{
					if ((dblArea1 > 0) &&(dblArea2 > 0))
						bIn = true;
				}
				else if (dblArea0 < 0)
				{
					if ((dblArea1 < 0) && (dblArea2 < 0))
						bIn = true;
				}
			}
				
			return bIn;			
		}
		
		/****************************************************************
		To check whether the Vertex is an ear or not based updated Polygon vertices

		ref. www-cgrl.cs.mcgill.ca/~godfried/teaching/cg-projects/97/Ian
		/algorithm1.html

		If it is an ear, return true,
		If it is not an ear, return false;
		*****************************************************************/
		private bool IsEarOfUpdatedPolygon(Coordinate vertex)		
		{
			PolygonHelper polygon = new PolygonHelper(m_aUpdatedPolygonVertices);

			if (polygon.PolygonVertex(vertex))
			{
				bool bEar = true;
				if (polygon.VertexType(vertex) == PolygonVertexType.ConvexPoint)
				{
					Coordinate pi = vertex;
					Coordinate pj = polygon.PreviousPoint(vertex); //previous vertex
					Coordinate pk = polygon.NextPoint(vertex);//next vertex

					for (int i = m_aUpdatedPolygonVertices.GetLowerBound(0);
						i < m_aUpdatedPolygonVertices.GetUpperBound(0); i++)
					{
						Coordinate pt = m_aUpdatedPolygonVertices[i];
						if ( !(pt.Equals(pi) || pt.Equals(pj) || pt.Equals(pk)))
						{
							if (TriangleContainsPoint(new Coordinate[] {pj, pi, pk}, pt))
								bEar=false;
						}
					}
				} //ThePolygon.getVertexType(Vertex)=ConvexPt
				else  //concave point
                {
					bEar = false; //not an ear/
                }

				return bEar;
			}
			else //not a polygon vertex;
			{
				Debug.WriteLine("IsEarOfUpdatedPolygon: " +
					"Not a polygon vertex");

				return false;
			}
		}

		/****************************************************
		Set up m_aPolygons:
		add ears and been cut Polygon togather
		****************************************************/
		private void SetPolygons()
		{
			int nPolygon = m_alEars.Count + 1; //ears plus updated polygon
			m_aPolygons  = new Coordinate[nPolygon][];

			for (int i = 0; i < (nPolygon - 1); i++) //add ears
			{
				Coordinate[] points = (Coordinate[])m_alEars[i];

				m_aPolygons[i]    = new Coordinate[3]; //3 vertices each ear
				m_aPolygons[i][0] = points[0];
				m_aPolygons[i][1] = points[1];
				m_aPolygons[i][2] = points[2];
			}
				
			//add UpdatedPolygon:
			m_aPolygons[nPolygon - 1] = new 
				Coordinate[m_aUpdatedPolygonVertices.Length];

			for (int i = 0; i < m_aUpdatedPolygonVertices.Length; i++)
			{
				m_aPolygons[nPolygon - 1][i] = m_aUpdatedPolygonVertices[i];
			}
		}

		/********************************************************
		To update m_aUpdatedPolygonVertices:
		Take out Vertex from m_aUpdatedPolygonVertices array, add 3 points
		to the m_aEars
		**********************************************************/
		private void UpdatePolygonVertices(Coordinate vertex)
		{
			ArrayList alTempPts = new ArrayList(); 

			for (int i = 0; i< m_aUpdatedPolygonVertices.Length; i++)
			{				
                //add 3 pts to FEars
				if (vertex.Equals(m_aUpdatedPolygonVertices[i])) 
				{ 
					PolygonHelper polygon=new PolygonHelper(m_aUpdatedPolygonVertices);
					Coordinate pti = vertex;
					Coordinate ptj = polygon.PreviousPoint(vertex); //previous point
					Coordinate ptk = polygon.NextPoint(vertex); //next point
					
                    //3 vertices of each ear
					Coordinate[] aEar = new Coordinate[3]; 
					aEar[0] = ptj;
					aEar[1] = pti;
					aEar[2] = ptk;

					m_alEars.Add(aEar);
				}
				else	
				{
					alTempPts.Add(m_aUpdatedPolygonVertices[i]);
				} //not equal points
			}
			
			if  ((m_aUpdatedPolygonVertices.Length - alTempPts.Count) == 1)
			{
				int nLength = m_aUpdatedPolygonVertices.Length;
				m_aUpdatedPolygonVertices = new Coordinate[nLength-1];
        
				for (int  i = 0; i < alTempPts.Count; i++)
                {
                    m_aUpdatedPolygonVertices[i] = (Coordinate)alTempPts[i];
                }
			}
		}
        
        #endregion
	}
}
