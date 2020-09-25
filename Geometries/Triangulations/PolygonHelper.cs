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

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Triangulations
{
	/// <summary>
	/// Summary description for PolygonHelper.
	/// </summary>
	[Serializable]
    public class PolygonHelper
	{
		private Coordinate[] m_aVertices;
		
		public PolygonHelper()
		{
		}

		public PolygonHelper(Coordinate[] points)
		{
			int nNumOfPoitns = points.Length;
            if (nNumOfPoitns < 3)
            {     
                throw new ArgumentException("The number of points must be at least three.");
            }

            m_aVertices = new Coordinate[nNumOfPoitns];
            for (int i = 0; i < nNumOfPoitns; i++)
            {
                m_aVertices[i] = points[i];
            }
        }

        public Coordinate this[int index] 
        {
            set
            {
                m_aVertices[index] = value;
            }

            get
            {
                return m_aVertices[index];
            }
        }

		/***********************************
		 From a given point, get its vertex index.
		 If the given point is not a polygon vertex, 
		 it will return -1 
		 ***********************************/
		public int VertexIndex(Coordinate vertex)
		{
            if (vertex == null)
            {
                throw new ArgumentNullException("vertex");
            }

            int nIndex  = -1;

			int nNumPts = m_aVertices.Length;
			for (int i = 0; i < nNumPts; i++) //each vertex
			{
				if (vertex.Equals(m_aVertices[i]))
                {
                    nIndex = i;
                }
			}

			return nIndex;
		}

		/***********************************
		 From a given vertex, get its previous vertex point.
		 If the given point is the first one, 
		 it will return  the last vertex;
		 If the given point is not a polygon vertex, 
		 it will return null; 
		 ***********************************/
		public Coordinate PreviousPoint(Coordinate vertex)
		{
			int nIndex = VertexIndex(vertex);
			if (nIndex == -1)
            {
                return null;
            }
			else //a valid vertex
			{
				if (nIndex == 0) //the first vertex
				{
					int nPoints = m_aVertices.Length;

					return m_aVertices[nPoints-1];
				}
				else //not the first vertex
                {
                    return m_aVertices[nIndex-1];
                }
			}			
		}

		/***************************************
			 From a given vertex, get its next vertex point.
			 If the given point is the last one, 
			 it will return  the first vertex;
			 If the given point is not a polygon vertex, 
			 it will return null; 
		***************************************/
		public Coordinate NextPoint(Coordinate vertex)
		{
//			Coordinate nextPt = new Coordinate();

			int nIndex = VertexIndex(vertex);
			if (nIndex == -1)
            {
                return null;
            }
			else //a valid vertex
			{
				int nNumOfPt = m_aVertices.Length;
				if (nIndex == (nNumOfPt - 1)) //the last vertex
				{
					return m_aVertices[0];
				}
				else //not the last vertex
                {
                    return m_aVertices[nIndex+1];
                }
			}			
		}

		
		/******************************************
		To calculate the polygon's area

		Good for polygon with holes, but the vertices make the 
		hole  should be in different direction with bounding 
		polygon.
		
		Restriction: the polygon is not self intersecting
		ref: www.swin.edu.au/astronomy/pbourke/
			geometry/polyarea/
		*******************************************/
		public double PolygonArea()
		{
			double dblArea     = 0;
			int nNumOfVertices = m_aVertices.Length;
			
			int j;
			for (int i = 0; i < nNumOfVertices; i++)
			{
				j        = (i+1) % nNumOfVertices;
				dblArea += m_aVertices[i].X  * m_aVertices[j].Y;
				dblArea -= (m_aVertices[i].Y * m_aVertices[j].X);
			}

			dblArea = dblArea/2;

			return Math.Abs(dblArea);
		}
		
		/******************************************
		To calculate the area of polygon made by given points 

		Good for polygon with holes, but the vertices make the 
		hole  should be in different direction with bounding 
		polygon.
		
		Restriction: the polygon is not self intersecting
		ref: www.swin.edu.au/astronomy/pbourke/
			geometry/polyarea/

		As polygon in different direction, the result coulb be
		in different sign:
		If dblArea>0 : polygon in clock wise to the user 
		If dblArea<0: polygon in count clock wise to the user 		
		*******************************************/
		public static double PolygonArea(Coordinate[] points)
		{
			double dblArea = 0;
			int nNumOfPts  = points.Length;
			
			int j;
			for (int i = 0; i < nNumOfPts; i++)
			{
				j=(i+1) % nNumOfPts;
				dblArea += points[i].X*points[j].Y;
				dblArea -= (points[i].Y*points[j].X);
			}

			dblArea = dblArea/2;

			return dblArea;
		}
		
		/***********************************************
			To check a vertex concave point or a convex point
			-----------------------------------------------------------
			The out polygon is in count clock-wise direction
		************************************************/
		public PolygonVertexType VertexType(Coordinate vertex)
		{
			PolygonVertexType vertexType=PolygonVertexType.ErrorPoint;

			if (PolygonVertex(vertex))			
			{
				Coordinate pti = vertex;
				Coordinate ptj = PreviousPoint(vertex);
				Coordinate ptk = NextPoint(vertex);		

				double dArea = PolygonArea(new Coordinate[] {ptj,pti, ptk});
				
				if (dArea < 0)
					vertexType = PolygonVertexType.ConvexPoint;
				else if (dArea > 0)
					vertexType = PolygonVertexType.ConcavePoint;
			}	

			return vertexType;
		}
		
		/*********************************************
		To check the Line of vertex1, vertex2 is a Diagonal or not
  
		To be a diagonal, Line vertex1-vertex2 has no intersection 
		with polygon lines.
		
		If it is a diagonal, return true;
		If it is not a diagonal, return false;
		reference: www.swin.edu.au/astronomy/pbourke
		/geometry/lineline2d
		*********************************************/
		public bool Diagonal(Coordinate vertex1, Coordinate vertex2)
		{
            if (vertex1 == null)
            {
                throw new ArgumentNullException("vertex1");
            }
            if (vertex2 == null)
            {
                throw new ArgumentNullException("vertex2");
            }

            bool bDiagonal     = false;
			int nNumOfVertices = m_aVertices.Length;
			int j = 0;
			for (int i= 0; i < nNumOfVertices; i++) //each point
			{
				bDiagonal = true;
				j = (i+1) % nNumOfVertices;  //next point of i
        
				//Diagonal line:
				double x1 = vertex1.X;
				double y1 = vertex1.Y;
				double x2 = vertex2.X;
				double y2 = vertex2.Y;

				//PolygonHelper line:
				double x3 = m_aVertices[i].X;
				double y3 = m_aVertices[i].Y;
				double x4 = m_aVertices[j].X;
				double y4 = m_aVertices[j].Y;

				double de = (y4-y3)*(x2-x1)-(x4-x3)*(y2-y1);
				double ub = -1;
				
				if (Math.Abs(de-0) > ConstantValue.SmallValue)  //lines are not parallel
					ub = ((x2-x1)*(y1-y3)-(y2-y1)*(x1-x3))/de;

				if ((ub > 0) && (ub < 1))
				{
					bDiagonal = false;
				}
			}
            
			return bDiagonal;
		}

		
		/*************************************************
		To check FaVertices make a convex polygon or 
		concave polygon

		Restriction: the polygon is not self intersecting
		Ref: www.swin.edu.au/astronomy/pbourke
		/geometry/clockwise/index.html
		********************************************/
		public PolygonType FindPolygonType()
		{
			int nNumOfVertices = m_aVertices.Length;
			bool bSignChanged  = false;
			int nCount = 0;
			int j = 0, k = 0;

			for (int i = 0; i < nNumOfVertices; i++)
			{
				j = (i+1) % nNumOfVertices; //j:=i+1;
				k = (i+2) % nNumOfVertices; //k:=i+2;

				double crossProduct = (m_aVertices[j].X - m_aVertices[i].X)
					* (m_aVertices[k].Y - m_aVertices[j].Y);
				crossProduct = crossProduct - ((m_aVertices[j].Y - m_aVertices[i].Y)
					* (m_aVertices[k].X - m_aVertices[j].X));

				//change the value of nCount
				if ((crossProduct > 0) && (nCount == 0) )
					nCount = 1;
				else if ((crossProduct < 0) && (nCount == 0))
					nCount = -1;

				if (((nCount == 1) && (crossProduct < 0)) ||
                    ((nCount == -1) && (crossProduct > 0)))
                {
                    bSignChanged = true;
                }
			}

			if (bSignChanged)
				return PolygonType.Concave;
			else
				return PolygonType.Convex;
		}

		/***************************************************
		Check a Vertex is a principal vertex or not
		ref. www-cgrl.cs.mcgill.ca/~godfried/teaching/
		cg-projects/97/Ian/glossay.html
  
		PrincipalVertex: a vertex pi of polygon P is a principal vertex if the
		diagonal pi-1, pi+1 intersects the boundary of P only at pi-1 and pi+1.
		*********************************************************/
		public bool PrincipalVertex(Coordinate vertex)
		{
			bool bPrincipal = false;
			if (PolygonVertex(vertex)) //valid vertex
			{
				Coordinate pt1 = PreviousPoint(vertex);
				Coordinate pt2 = NextPoint(vertex);
					
				if ((pt1 != null && pt2 != null) && Diagonal(pt1, pt2))
					bPrincipal = true;
			}

			return bPrincipal;
		}

		/*********************************************
        To check whether a given point is a PolygonHelper Vertex
		**********************************************/
		public bool PolygonVertex(Coordinate point)
		{
			bool bVertex = false;
			int nIndex   = VertexIndex(point);

			if ((nIndex >= 0) && (nIndex <= m_aVertices.Length - 1))
                bVertex = true;

			return bVertex;
		}

		/*****************************************************
		To reverse polygon vertices to different direction:
		clock-wise <------->count-clock-wise
		******************************************************/
		public void ReverseVerticesDirection()
		{
			int nVertices         = m_aVertices.Length;
			Coordinate[] aTempPts = new Coordinate[nVertices];
			
			for (int i = 0; i < nVertices; i++)
            {
                aTempPts[i] = m_aVertices[i];
            }
	
			for (int i = 0; i < nVertices; i++)
            {
                m_aVertices[i] = aTempPts[nVertices-1-i];	
            }
		}

		/*****************************************
		To check vertices make a clock-wise polygon or
		count clockwise polygon

		Restriction: the polygon is not self intersecting
		Ref: www.swin.edu.au/astronomy/pbourke/
		geometry/clockwise/index.html
		*****************************************/
		public PolygonDirection VerticesDirection()
		{
			int nCount    = 0, j = 0, k = 0;
			int nVertices = m_aVertices.Length;
			
			for (int i = 0; i < nVertices; i++)
			{
				j = (i+1) % nVertices; //j:=i+1;
				k = (i+2) % nVertices; //k:=i+2;

				double crossProduct = (m_aVertices[j].X - m_aVertices[i].X)
					*(m_aVertices[k].Y- m_aVertices[j].Y);
				crossProduct = crossProduct - ((m_aVertices[j].Y- m_aVertices[i].Y)
					* (m_aVertices[k].X- m_aVertices[j].X));

				if (crossProduct > 0)
					nCount++;
				else
					nCount--;
			}
		
			if (nCount < 0) 
				return PolygonDirection.CounterClockwise;
			else if (nCount > 0)
				return PolygonDirection.Clockwise;
			else
				return PolygonDirection.Unknown;
  		}

		
		/*****************************************
		To check given points make a clock-wise polygon or
		count clockwise polygon

		Restriction: the polygon is not self intersecting
		*****************************************/
		public static PolygonDirection PointsDirection(Coordinate[] points)
		{
			int nCount  = 0, j = 0, k = 0;
			int nPoints = points.Length;
			
			if (nPoints < 3)
				return PolygonDirection.Unknown;
			
			for (int i = 0; i < nPoints; i++)
			{
				j = (i+1) % nPoints; //j:=i+1;
				k = (i+2) % nPoints; //k:=i+2;

				double crossProduct = (points[j].X - points[i].X)
					*(points[k].Y - points[j].Y);
				crossProduct = crossProduct - ((points[j].Y - points[i].Y)
					*(points[k].X - points[j].X));

				if (crossProduct>0)
					nCount++;
				else
					nCount--;
			}
		
			if (nCount < 0) 
				return PolygonDirection.CounterClockwise;
			else if (nCount > 0)
				return PolygonDirection.Clockwise;
			else
				return PolygonDirection.Unknown;
		}

		/*****************************************************
		To reverse points to different direction (order) :
		******************************************************/
		public static void ReversePointsDirection(Coordinate[] points)
		{
			int nVertices = points.Length;
			Coordinate[] aTempPts = new Coordinate[nVertices];
			
			for (int i = 0; i < nVertices; i++)
				aTempPts[i] = points[i];
	
			for (int i = 0; i < nVertices; i++)
				points[i] = aTempPts[nVertices-1-i];	
		}

        public static bool PointInsidePolygon(Coordinate[] polygonVertices, 
            Coordinate ptTest)
        {
            if (polygonVertices.Length < 3) //not a valid polygon
                return false;
			
            int  nCounter = 0;
            int nPoints   = polygonVertices.Length;
			
            Coordinate s1, p1, p2;
            s1 = ptTest;
            p1 = polygonVertices[0];
			
            for (int i = 1; i < nPoints; i++)
            {
                p2 = polygonVertices[i % nPoints];
                if (s1.Y > Math.Min(p1.Y, p2.Y))
                {
                    if (s1.Y <= Math.Max(p1.Y, p2.Y) )
                    {
                        if (s1.X <= Math.Max(p1.X, p2.X) )
                        {
                            if (p1.Y != p2.Y)
                            {
                                double xInters = (s1.Y - p1.Y) * (p2.X - p1.X) /
                                    (p2.Y - p1.Y) + p1.X;
                                if ((p1.X== p2.X) || (s1.X <= xInters) )
                                {
                                    nCounter ++;
                                }
                            }  //p1.y != p2.y
                        }
                    }
                }
                p1 = p2;
            } //for loop
  
            if ((nCounter % 2) == 0) 
                return false;
            else
                return true;
        }

        /*********** Sort points from Xmin->Xmax ******/
        public static void SortPointsByX(Coordinate[] points)
        {
            if (points.Length > 1)
            {
                Coordinate tempPt;
                for (int i = 0; i < points.Length - 2; i++)
                {
                    for (int j = i+1; j < points.Length -1; j++)
                    {
                        if (points[i].X > points[j].X)
                        {
                            tempPt    = points[j];
                            points[j] = points[i];
                            points[i] = tempPt;
                        }
                    }
                }
            }
        }

        /*********** Sort points from Ymin->Ymax ******/
        public static void SortPointsByY(Coordinate[] points)
        {
            if (points.Length > 1)
            {
                Coordinate tempPt;
                for (int i = 0; i< points.Length-2; i++)
                {
                    for (int j = i+1; j < points.Length - 1; j++)
                    {
                        if (points[i].Y > points[j].Y)
                        {
                            tempPt    = points[j];
                            points[j] = points[i];
                            points[i] = tempPt;
                        }
                    }
                }
            }
        }
    }		
}
