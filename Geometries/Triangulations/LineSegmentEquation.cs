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
    //line: ax+by+c=0, with start point and end point
    //direction from start point ->end point
    /// </summary>
    [Serializable]
    public class LineSegmentEquation : LineEquation
    {
        private Coordinate m_startPoint;
        private Coordinate m_endPoint;

        public LineSegmentEquation(Coordinate startPoint, Coordinate endPoint)
            : base(startPoint,endPoint)
        {
            this.m_startPoint=startPoint;
            this.m_endPoint= endPoint;
        }

        public Coordinate StartPoint
        {
            get
            {
                return m_startPoint;
            }
        }

        public Coordinate EndPoint
        {
            get
            {
                return m_endPoint;
            }
        }

        /*** chagne the line's direction ***/
        public void ChangeLineDirection()
        {
            Coordinate tempPt;
            tempPt=this.m_startPoint;
            this.m_startPoint=this.m_endPoint;
            this.m_endPoint=tempPt;
        }

        /*** To calculate the line segment length:   ***/
        public double FindLineSegmentLength()
        {
            double d=(m_endPoint.X-m_startPoint.X)	*(m_endPoint.X-m_startPoint.X);
            d += (m_endPoint.Y-m_startPoint.Y)	*(m_endPoint.Y-m_startPoint.Y);
            d=Math.Sqrt(d);

            return d;
        }

        /********************************************************** 
            Get point location, using windows coordinate system: 
            y-axes points down.
            Return Value:
            -1:point at the left of the line (or above the line if the line is horizontal)
             0: point in the line segment or in the line segment 's extension
             1: point at right of the line (or below the line if the line is horizontal)    
         ***********************************************************/
        public int GetPointLocation(Coordinate point)
        {
            if (point == null)
            {
                throw new ArgumentNullException("point");
            }

            double Bx = m_endPoint.X;
            double By = m_endPoint.Y;
			  
            double Ax = m_startPoint.X;
            double Ay = m_startPoint.Y;
			  
            double Cx = point.X;
            double Cy = point.Y;
			
            if (this.IsHorizontalLine)
            {
                if (Math.Abs(Ay-Cy)<ConstantValue.SmallValue) //equal
                    return 0;
                else if (Ay > Cy)
                    return -1;   //Y Axis points down, point is above the line
                else //Ay<Cy
                    return 1;    //Y Axis points down, point is below the line
            }
            else //Not a horizontal line
            {
                //make the line direction bottom->up
                if (m_endPoint.Y>m_startPoint.Y)
                    this.ChangeLineDirection();

                double L=this.FindLineSegmentLength();
                double s=((Ay-Cy)*(Bx-Ax)-(Ax-Cx)*(By-Ay))/(L*L);
				 
                //Note: the Y axis is pointing down:
                if (Math.Abs(s-0)<ConstantValue.SmallValue) //s=0
                    return 0; //point is in the line or line extension
                else if (s>0) 
                    return -1; //point is left of line or above the horizontal line
                else //s<0
                    return 1;
            }
        }

        /***Get the minimum x value of the points in the line***/
        public double Xmin
        {
            get
            {
                return Math.Min(m_startPoint.X, m_endPoint.X);
            }
        }

        /***Get the maximum  x value of the points in the line***/
        public double Xmax
        {
            get
            {
                return Math.Max(m_startPoint.X, m_endPoint.X);
            }
        }

        /***Get the minimum y value of the points in the line***/
        public double Ymin
        {
            get
            {
                return Math.Min(m_startPoint.Y, m_endPoint.Y);
            }
        }

        /***Get the maximum y value of the points in the line***/
        public double Ymax
        {
            get
            {
                return Math.Max(m_startPoint.Y, m_endPoint.Y);
            }
        }

        /***Check whether this line is in a longer line***/
        public bool InLine(LineSegmentEquation longerLineSegment)
        {
            if (longerLineSegment == null)
            {
                throw new ArgumentNullException("longerLineSegment");
            }

            bool bInLine = false;  
            if ((longerLineSegment.OnLine(m_startPoint)) && 
                (longerLineSegment.OnLine(m_endPoint)))
                bInLine = true;

            return bInLine;
        }

        /***To check whether the point is in a line segment***/
        public bool OnLine(Coordinate point)
        {
            if (point == null)
            {
                throw new ArgumentNullException("point");
            }

            bool bInline = false;

            double Bx = this.EndPoint.X;
            double By = this.EndPoint.Y;
            double Ax = this.StartPoint.X;
            double Ay = this.StartPoint.Y;
            double Cx = point.X;
            double Cy = point.Y;
  
            double L = this.FindLineSegmentLength();
            double s = Math.Abs(((Ay-Cy)*(Bx-Ax)-(Ax-Cx)*(By-Ay))/(L*L));
  
            if (Math.Abs(s-0)<ConstantValue.SmallValue)
            {
                if ((point.Equals(this.StartPoint)) || 
                    (point.Equals(this.EndPoint)))
                {
                    bInline = true;
                }
                else if ((Cx<this.Xmax) && (Cx>this.Xmin)
                    &&(Cy< this.Ymax) && (Cy>this.Ymin))
                {
                    bInline = true;
                }
            }

            return bInline;
        }

        /************************************************
         * Offset the line segment to generate a new line segment
         * If the offset direction is along the x-axis or y-axis, 
         * Parameter is true, other wise it is false
         * ***********************************************/
        public LineSegmentEquation OffsetLine(double distance, bool rightOrDown)
        {
            //offset a line with a given distance, generate a new line
            //rightOrDown=true means offset to x incress direction,
            // if the line is horizontal, offset to y incress direction
  
            LineSegmentEquation line;
            Coordinate newStartPoint=new Coordinate();
            Coordinate newEndPoint=new Coordinate();
			
            double alphaInRad= this.LineAngle; // 0-PI
            if (rightOrDown)
            {
                if (this.IsHorizontalLine) //offset to y+ direction
                {
                    newStartPoint.X =this.m_startPoint.X;
                    newStartPoint.Y=this.m_startPoint.Y + distance;

                    newEndPoint.X =this.m_endPoint.X;
                    newEndPoint.Y=this.m_endPoint.Y + distance;
                    line=new LineSegmentEquation(newStartPoint,newEndPoint);
                }
                else //offset to x+ direction
                {
                    if (Math.Sin(alphaInRad)>0)  
                    {
                        newStartPoint.X=m_startPoint.X + Math.Abs(distance*Math.Sin(alphaInRad));
                        newStartPoint.Y=m_startPoint.Y - Math.Abs(distance* Math.Cos(alphaInRad)) ;
						
                        newEndPoint.X=m_endPoint.X + Math.Abs(distance*Math.Sin(alphaInRad));
                        newEndPoint.Y=m_endPoint.Y - Math.Abs(distance* Math.Cos(alphaInRad)) ;
					
                        line= new LineSegmentEquation(
                            newStartPoint, newEndPoint);
                    }
                    else //sin(FalphaInRad)<0
                    {
                        newStartPoint.X=m_startPoint.X + Math.Abs(distance*Math.Sin(alphaInRad));
                        newStartPoint.Y=m_startPoint.Y + Math.Abs(distance* Math.Cos(alphaInRad)) ;
                        newEndPoint.X=m_endPoint.X + Math.Abs(distance*Math.Sin(alphaInRad));
                        newEndPoint.Y=m_endPoint.Y + Math.Abs(distance* Math.Cos(alphaInRad)) ;

                        line=new LineSegmentEquation(
                            newStartPoint, newEndPoint);
                    }
                } 
            }//{rightOrDown}
            else //leftOrUp
            {
                if (this.IsHorizontalLine) //offset to y directin
                {
                    newStartPoint.X=m_startPoint.X;
                    newStartPoint.Y=m_startPoint.Y - distance;

                    newEndPoint.X=m_endPoint.X;
                    newEndPoint.Y=m_endPoint.Y - distance;
                    line=new LineSegmentEquation(
                        newStartPoint, newEndPoint);
                }
                else //offset to x directin
                {
                    if (Math.Sin(alphaInRad)>=0)
                    {
                        newStartPoint.X=m_startPoint.X - Math.Abs(distance*Math.Sin(alphaInRad));
                        newStartPoint.Y=m_startPoint.Y + Math.Abs(distance* Math.Cos(alphaInRad)) ;
                        newEndPoint.X=m_endPoint.X - Math.Abs(distance*Math.Sin(alphaInRad));
                        newEndPoint.Y=m_endPoint.Y + Math.Abs(distance* Math.Cos(alphaInRad)) ;
                        
                        line=new LineSegmentEquation(
                            newStartPoint, newEndPoint);
                    }
                    else //sin(FalphaInRad)<0
                    {
                        newStartPoint.X=m_startPoint.X - Math.Abs(distance*Math.Sin(alphaInRad));
                        newStartPoint.Y=m_startPoint.Y - Math.Abs(distance* Math.Cos(alphaInRad)) ;
                        newEndPoint.X=m_endPoint.X - Math.Abs(distance*Math.Sin(alphaInRad));
                        newEndPoint.Y=m_endPoint.Y - Math.Abs(distance* Math.Cos(alphaInRad)) ;
                            
                        line=new LineSegmentEquation(
                            newStartPoint, newEndPoint);
                    }
                }				
            }
            return line;	
        }

        /********************************************************
        To check whether 2 lines segments have an intersection
        *********************************************************/
        public bool IntersectedWith(LineSegmentEquation line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            double x1 = this.m_startPoint.X;
            double y1 = this.m_startPoint.Y;
            double x2 = this.m_endPoint.X;
            double y2 = this.m_endPoint.Y;
            double x3 = line.m_startPoint.X;
            double y3 = line.m_startPoint.Y;
            double x4 = line.m_endPoint.X;
            double y4 = line.m_endPoint.Y;

            double de = (y4-y3)*(x2-x1)-(x4-x3)*(y2-y1);
            //if de<>0 then //lines are not parallel
            if (Math.Abs(de-0)<ConstantValue.SmallValue) //not parallel
            {
                double ua = ((x4-x3)*(y1-y3)-(y4-y3)*(x1-x3))/de;
                double ub = ((x2-x1)*(y1-y3)-(y2-y1)*(x1-x3))/de;

                if ((ua > 0) && (ub < 1))
                    return true;
                else
                    return false;
            }
            else	//lines are parallel
            {
                return false;
            }
        }
		
    }
}
