using System;

namespace iGeospatial.Coordinates.Transforms
{
	/// <summary>
	/// Summary description for CoordinateMapping.
	/// </summary>
	[Serializable]
    public class CoordinateMapping : ICoordinateVisitor, ICloneable
	{
        #region Private Fields

        private Envelope m_objViewportEnvelope;
        private Envelope m_objWorldEnvelope;

        private double m_dFactorA; 
        private double m_dFactorB; 
        private double m_dFactorC; 
        private double m_dFactorD;
        
        #endregion

        #region Constructors and Destructor

        // Constructor
        public CoordinateMapping()
        {
            m_objWorldEnvelope   = new Envelope(0.0, 0.0, 0.0, 0.0);
            m_objViewportEnvelope = new Envelope(0.0, 0.0, 0.0, 0.0);

            m_dFactorA = 1.0;
            m_dFactorB = 1.0; 
            m_dFactorC = 0.0; 
            m_dFactorD = 0.0;
        }
        
        #endregion

        #region Public Properties
        
        public Envelope WorldExtents 
        {
            get 
            {
                return m_objWorldEnvelope;
            }

            set
            {
                m_objWorldEnvelope = value;
//TODO--PAUL                m_objWorldEnvelope.Normalize();

                Recalculate();
            }
        }

        public Envelope ViewportExtents
        {
            get 
            {
                return m_objViewportEnvelope;
            }

            set
            {
                m_objViewportEnvelope = value;

                Recalculate();
            }
        }
        
        #endregion

        #region Public Methods

        public void SetWorldSpaceExtents(double x1, double y1, double x2, double y2)
        {
            this.WorldExtents = new Envelope(x1, y1, x2, y2);
        }

        public void GetWorldSpaceExtents(ref double x1, ref double y1, 
            ref double x2, ref double y2) 
        {
            x1 = m_objWorldEnvelope.MinX;
            y1 = m_objWorldEnvelope.MinY;
            x2 = m_objWorldEnvelope.MaxX;
            y2 = m_objWorldEnvelope.MaxY;
        }

        public void SetViewportSpaceExtents(double x1, double y1, double x2, double y2)
        {
            this.ViewportExtents = new Envelope(x1, y1, x2, y2);
        }

        public void GetViewportSpaceExtents(ref double x1, ref double y1, 
            ref double x2, ref double y2) 
        {
            x1 = m_objViewportEnvelope.MinX;
            y1 = m_objViewportEnvelope.MinY;
            x2 = m_objViewportEnvelope.MaxX;
            y2 = m_objViewportEnvelope.MaxY;
        }

        // Map <point> from world-space tp the viewport
        public void ToViewport(Coordinate point) 
        {
            if (m_objWorldEnvelope.Width != 0)
            {
                point.X = (m_dFactorA * point.X + m_dFactorC);
            }

            if (m_objWorldEnvelope.Height != 0)
            {
                point.Y = (m_dFactorB * point.Y + m_dFactorD);
            }
        }

        public void ToViewport(ref double x, ref double y) 
        {
            Coordinate point = new Coordinate(x, y);

            ToViewport(point);
            
            x = point.X;
            y = point.Y;
        }

        // The aspect ratio should be maintained
        public void ToViewport(ref double scalar) 
        {
            // Compute if the window isn't bad
            if (m_objWorldEnvelope.Width != 0)
            {
                scalar *= Math.Abs(m_objViewportEnvelope.Width / m_objWorldEnvelope.Width);
            }
        }

        // Map <point> from viewport to world-space
        public void ToWorld(Coordinate point) 
        {
            double tempX = 0, tempY = 0;
            double viewWidth  = m_objViewportEnvelope.Width;
            double viewHeight = m_objViewportEnvelope.Height;
            double viewLeft   = m_objViewportEnvelope.MinX;
            double viewBottom = m_objViewportEnvelope.MinY;
            double winLeft    = m_objWorldEnvelope.MinX;
            double winBottom  = m_objWorldEnvelope.MinY;

            if (viewWidth != 0)
            {
                tempX = (point.X - viewLeft) * (m_objWorldEnvelope.Width / viewWidth) + winLeft;
            }

            if ( viewHeight != 0 )
            {
                tempY = (point.Y - viewBottom) * (m_objWorldEnvelope.Height / viewHeight) + winBottom;
            }

            point.SetCoordinate(tempX, tempY);
        }

        public void ToWorld(ref double x, ref double y) 
        {
            Coordinate point = new Coordinate(x, y);

            ToWorld(point);
            
            x = point.X;
            y = point.Y;
        } 

        // The aspect ratio should be maintained
        public void ToWorld(ref double scalar) 
        {
            // Compute if the viewport isn't bad
            if (m_objViewportEnvelope.Width != 0)
            {
                scalar *= Math.Abs(m_objWorldEnvelope.Width / m_objViewportEnvelope.Width);
            }
        }

        public bool IsPointInWorldSpace(Coordinate pt) 
        {
            if (pt.X >= m_objWorldEnvelope.MinX  &&
                pt.X <= m_objWorldEnvelope.MaxX &&
                pt.Y >= m_objWorldEnvelope.MinY  &&
                pt.Y <= m_objWorldEnvelope.MaxY )
            {
                return true;
            }

            return false;
        }

        public bool IsPointInWorldSpace(double x, double y) 
        {
            return IsPointInWorldSpace(new Coordinate(x, y));
        }

        public void Pan(Coordinate newCenter)
        {
            // Translate window to new center
            Coordinate center = m_objWorldEnvelope.Center;
            
            m_objWorldEnvelope.Translate(newCenter - center);
            
            Recalculate();
        }

        public void Pan(double newCenterX, double newCenterY)
        {
            Pan(new Coordinate(newCenterX, newCenterY));
        }

        public void ZoomBy(double factor)
        {
            // Abort if attempt to zoom by zero
            if (factor == 0.0)
            {
                return;
            }

            // Zoom the window
            if (factor < 0.0)
            {
                m_objWorldEnvelope.Scale(-factor, -factor);
            }
            else
            {
                m_objWorldEnvelope.Scale(1.0 / factor, 1.0 / factor);
            }

            Recalculate();
        }

        public void ZoomTo(double factor, Coordinate newCenter)
        {
            ZoomBy(factor);
            Pan(newCenter);
        }

        public void ZoomTo(double factor, double newCenterX, double newCenterY)
        {
            ZoomTo(factor, new Coordinate(newCenterX, newCenterY));
        }          
        
        #endregion

        #region Private Methods

        private void Recalculate()
        {
            // Abort if either rectangle has a zero height or width
            if (m_objViewportEnvelope.Height == 0 || m_objWorldEnvelope.Height == 0 || 
                m_objViewportEnvelope.Width  == 0 || m_objWorldEnvelope.Width  == 0)
            {
                return;
            }

            double viewportWidth  = m_objViewportEnvelope.Width;
            double viewportHeight = m_objViewportEnvelope.Height;
            double windowWidth    = m_objWorldEnvelope.Width;
            double windowHeight   = m_objWorldEnvelope.Height;

            // calculate the aspect ratios
            double aspectViewport, aspectWindow;
            aspectViewport = Math.Abs(viewportWidth / viewportHeight);
            aspectWindow   = Math.Abs(windowWidth   / windowHeight  );

            if (aspectViewport > aspectWindow)	// Window needs widened
            {
                double newWindowWidth = windowHeight * aspectViewport;
                m_objWorldEnvelope.Inflate((newWindowWidth - windowWidth) / 2.0, 0);
            }
            else
            {
                double newWindowHeight = windowWidth / aspectViewport;
                m_objWorldEnvelope.Inflate(0, (newWindowHeight - windowHeight) / 2.0);
            }

            viewportWidth  = m_objViewportEnvelope.Width;
            viewportHeight = m_objViewportEnvelope.Height;
            windowWidth    = m_objWorldEnvelope.Width;
            windowHeight   = m_objWorldEnvelope.Height;

            // Calculate helper values
            m_dFactorA = viewportWidth / windowWidth;
            
            m_dFactorB = viewportHeight / windowHeight;
            
            m_dFactorC = m_objViewportEnvelope.MinX + 
                (-m_objWorldEnvelope.MinX * viewportWidth) / windowWidth;

            m_dFactorD = m_objViewportEnvelope.MinY + 
                (-m_objWorldEnvelope.MinY * viewportHeight) / windowHeight;
            
//            m_dFactorC = (-m_objWorldEnvelope.MinX * viewportWidth) / windowWidth;
//
//            m_dFactorD = (-m_objWorldEnvelope.MinY * viewportHeight) / windowHeight;
        }
        
        #endregion

        #region ICoordinateVisitor Members

        public void Visit(Coordinate coord)
        {
            // TODO:  Add CoordinateMapping.Filter implementation
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            // TODO:  Add CoordinateMapping.Clone implementation
            return null;
        }

        #endregion
    }
}
