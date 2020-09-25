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

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries
{
	/// <summary> 
	/// Computes various kinds of common geometric shapes.
	/// </summary>
	/// <remarks>
	/// This allows various ways of specifying the location and extent 
	/// of the shapes, as well as number of line segments used to 
	/// form them.
	/// </remarks>
	[Serializable]
    public class GeometryFactoryEx : GeometryFactory
	{
        #region Private Members

        private GeometryFactory geomFact;
        private Dimensions      dim;
        private int             nPts     = 100;
		
        #endregion

        #region Constructors and Destructor

        public GeometryFactoryEx() : base()
        {
            dim = new Dimensions(this);
        }

        public GeometryFactoryEx(PrecisionModel precisionModel)
            : base(precisionModel)
        {
            dim = new Dimensions(this);
        }
		
        /// <summary> 
		/// Create a shape factory which will create shapes using the given
		/// <see cref="GeometryFactory"/>.
		/// </summary>
		/// <param name="geomFact">the factory to use
		/// </param>
		public GeometryFactoryEx(GeometryFactory factory)
		{
            dim           = new Dimensions(this);
            this.geomFact = factory;
		}
		
        #endregion

        #region Public Properties

		/// <summary> 
		/// Sets the location of the shape by specifying the base coordinate
		/// (which in most cases is the
		/// lower left point of the envelope containing the shape).
		/// 
		/// </summary>
		/// <param name="base">the base coordinate of the shape
		/// </param>
		public virtual Coordinate Base
		{
            get
            {
                return dim.Base;
            }

			set
			{
				dim.Base = value;
			}
		}

		/// <summary> 
		/// Sets the location of the shape by specifying the centre of
		/// the shape's bounding box.
		/// </summary>
		/// <param name="centre">
		/// The centre coordinate of the shape.
		/// </param>
		public virtual Coordinate Centre
		{
            get
            {
                return dim.Centre;
            }

			set
			{
				dim.Centre = value;
			}
		}

		/// <summary> 
		/// Sets the total number of points in the created <see cref="Geometry"/>.
		/// </summary>
		public virtual int NumPoints
		{
            get
            {
                return this.nPts;
            }

			set
			{
				this.nPts = value;
			}
		}

		/// <summary> 
		/// Gets or sets the size of the extent of the shape in both x and y directions.
		/// 
		/// </summary>
		/// <param name="size">the size of the shape's extent
		/// </param>
		public virtual void SetSize(int size)
		{
            dim.Size = size;
		}

		/// <summary> 
		/// Gets or sets the width of the shape. 
		/// </summary>
		/// <param name="width">the width of the shape
		/// </param>
		public virtual double Width
		{
            get
            {
                return dim.Width;
            }

			set
			{
				dim.Width = value;
			}
		}

		/// <summary> 
		/// Gets or sets the height of the shape. 
		/// </summary>
		/// <value>The height of the shape</value>
		public virtual double Height
		{
            get
            {
                return dim.Height;
            }

			set
			{
				dim.Height = value;
			}
		}

        #endregion

        #region Public Methods

		/// <summary> 
		/// Creates a rectangular Polygon.
		/// </summary>
		/// <returns> A rectangular Polygon </returns>
		public virtual Polygon CreateRectangle()
		{
			int i;
			int ipt = 0;
			int nSide = nPts / 4;
			if (nSide < 1)
				nSide = 1;

			double XsegLen = dim.Envelope.Width / nSide;
			double YsegLen = dim.Envelope.Height / nSide;
			
			Coordinate[] pts = new Coordinate[4 * nSide + 1];
			Envelope env     = dim.Envelope;
			
//			double maxx = env.MinX + nSide * XsegLen;
//			double maxy = env.MinY + nSide * XsegLen;
			
			for (i = 0; i < nSide; i++)
			{
				double x   = env.MinX + i * XsegLen;
				double y   = env.MinY;
				pts[ipt++] = new Coordinate(x, y);
			}

			for (i = 0; i < nSide; i++)
			{
				double x   = env.MaxX;
				double y   = env.MinY + i * YsegLen;
				pts[ipt++] = new Coordinate(x, y);
			}

			for (i = 0; i < nSide; i++)
			{
				double x   = env.MaxX - i * XsegLen;
				double y   = env.MaxY;
				pts[ipt++] = new Coordinate(x, y);
			}

			for (i = 0; i < nSide; i++)
			{
				double x   = env.MinX;
				double y   = env.MaxY - i * YsegLen;
				pts[ipt++] = new Coordinate(x, y);
			}

			pts[ipt++] = new Coordinate(pts[0]);
			
            if (geomFact != null)
            {
                LinearRing ring = geomFact.CreateLinearRing(pts);
                Polygon poly    = geomFact.CreatePolygon(ring, null);

                return poly;
            }
            else
            {
                LinearRing ring = this.CreateLinearRing(pts); 
                Polygon poly    = this.CreatePolygon(ring, null);

                return poly;
            }
		}
		
		/// <summary> 
		/// Creates a circular Polygon.
		/// </summary>
		/// <returns> A circle. </returns>
		public virtual Polygon CreateCircle()
		{
			Envelope env   = dim.Envelope;
			double xRadius = env.Width / 2.0;
			double yRadius = env.Height / 2.0;
			
			double centreX = env.MinX + xRadius;
			double centreY = env.MinY + yRadius;
			
			Coordinate[] pts = new Coordinate[nPts + 1];
			int iPt = 0;
			for (int i = 0; i < nPts; i++)
			{
				double ang = i * (2 * Math.PI / nPts);
				double x   = xRadius * Math.Cos(ang) + centreX;
				double y   = yRadius * Math.Sin(ang) + centreY;
				Coordinate pt = new Coordinate(x, y);
				pts[iPt++] = pt;
			}
			pts[iPt] = pts[0];
			
            if (geomFact != null)
            {
                LinearRing ring = geomFact.CreateLinearRing(pts);
                Polygon poly    = geomFact.CreatePolygon(ring, null);

                return poly;
            }
            else
            {
                LinearRing ring = this.CreateLinearRing(pts);
                Polygon poly    = this.CreatePolygon(ring, null);

                return poly;
            }
		}
		
		/// <summary> 
		/// Creates a elliptical arc, as a LineString.
		/// </summary>
		/// <returns> An elliptical arc </returns>
		public virtual LineString CreateArc(double startAng, double endAng)
		{
			Envelope env   = dim.Envelope;
			double xRadius = env.Width / 2.0;
			double yRadius = env.Height / 2.0;
			
			double centreX = env.MinX + xRadius;
			double centreY = env.MinY + yRadius;
			
			double angSize = (endAng - startAng);
			if (angSize <= 0.0 || angSize > 2 * Math.PI)
				angSize = 2 * Math.PI;

			double angInc = angSize / nPts;
			
			Coordinate[] pts = new Coordinate[nPts];
			int iPt = 0;
			for (int i = 0; i < nPts; i++)
			{
				double ang = startAng + i * angInc;
				double x   = xRadius * Math.Cos(ang) + centreX;
				double y   = yRadius * Math.Sin(ang) + centreY;

				Coordinate pt = new Coordinate(x, y);
                if (geomFact != null)
                {
                    pt.MakePrecise(geomFact.PrecisionModel);
                }
                else
                {
                    pt.MakePrecise(this.PrecisionModel);
                }

				pts[iPt++] = pt;
			}

            if (geomFact != null)
            {
                LineString line = geomFact.CreateLineString(pts);

                return line;
            }
            else
            {
                LineString line = this.CreateLineString(pts);

                return line;
            }
		}
		
        #endregion

        #region ICloneable Members

        public override GeometryFactory Clone()
        {
            GeometryFactoryEx objFactory = new GeometryFactoryEx(this);

            return objFactory;
        }

        #endregion

        #region Inner Private Dimensions Class

        /// <summary>
        /// Holds various information on the shape to be create, and is solely used by the
        /// <see cref="GeometryFactoryEx"/> class to maintain this information.
        /// </summary>
		[Serializable]
        private sealed class Dimensions
		{
            #region Private Members
            
            public Coordinate m_objBase;
            public Coordinate m_objCentre;
            public double     width;
            public double     height;
			
            private GeometryFactoryEx m_objGeometricShapeFactory;
			
            #endregion

            #region Constructors and Destructor
            
            public Dimensions(GeometryFactoryEx geometricShapeFactory)
			{
                this.m_objGeometricShapeFactory = geometricShapeFactory;
			}
			
            #endregion

            #region Public Properties

			public Coordinate Base
			{
				get
                {
                    return this.m_objBase;
                }

                set
				{
					this.m_objBase = value;
				}
			}
			
			public Coordinate Centre
			{
                get
                {
                    return this.m_objCentre;
                }

				set
				{
					this.m_objCentre = value;
				}
			}
			
			public double Size
			{
				set
				{
					height = value;
					width  = value;
				}
			}
			
			public double Width
			{
                get
                {
                    return this.width;
                }

				set
				{
					this.width = value;
				}
			}
			
			public double Height
			{
                get
                {
                    return this.height;
                }

				set
				{
					this.height = value;
				}
			}
			
			public Envelope Envelope
			{
				get
				{
					if (m_objBase != null)
					{
						return new Envelope(m_objBase.X, m_objBase.X + width, 
                            m_objBase.Y, m_objBase.Y + height);
					}

					if (m_objCentre != null)
					{
						return new Envelope(m_objCentre.X - width / 2, 
                            m_objCentre.X + width / 2, m_objCentre.Y - height / 2, 
                            m_objCentre.Y + height / 2);
					}
					
                    return new Envelope(0, width, 0, height);
				}
			}

			public GeometryFactoryEx ShapeFactory
			{
				get
				{
					return m_objGeometricShapeFactory;
				}
			}

            #endregion
		}

        #endregion
    }
}