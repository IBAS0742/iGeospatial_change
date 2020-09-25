using System;

using iGeospatial.Coordinates.Transforms;

namespace iGeospatial.Coordinates
{
    
    /// <summary>  
    /// Defines a rectangular region of the 2D coordinate plane.
    /// </summary>
    /// It is often used to represent the bounding box of a Geometry,
    /// e.g. the minimum and maximum x and y values of the Coordinates.
    /// 
    /// Note that Envelopes support infinite or half-infinite regions, by using the values of
    /// Double.PositiveInfinity and Double.NegativeInfinity.
    /// 
    /// When Envelope objects are created or initialized,
    /// the supplies extent values are automatically sorted into the correct order.
    [Serializable]
    public class Envelope : ICloneable
    {
        #region Internal Fields

        /// <summary>  the minimum x-coordinate</summary>
        internal double m_dMinX = 0.0;
        
        /// <summary>  the maximum x-coordinate</summary>
        internal double m_dMaxX = 0.0;
        
        /// <summary>  the minimum y-coordinate</summary>
        internal double m_dMinY = 0.0;
        
        /// <summary>  the maximum y-coordinate</summary>
        internal double m_dMaxY = 0.0;
        
        #endregion
        
        #region Constructors and Destructor
        
        /// <summary>  Creates a null Envelope.</summary>
        public Envelope()
        {
            m_dMinX = 0.0;
            m_dMaxX = 0.0;

            m_dMinY = 0.0;
            m_dMaxY = 0.0;
        }
        
        /// <summary>  
        /// Creates an Envelope for a region defined by maximum and minimum values.
        /// </summary>
        /// <param name="x1"> the first x-value
        /// </param>
        /// <param name="x2"> the second x-value
        /// </param>
        /// <param name="y1"> the first y-value
        /// </param>
        /// <param name="y2"> the second y-value
        /// </param>
        public Envelope(double x1, double x2, double y1, double y2)
        {
            if (x1 < x2)
            {
                m_dMinX = x1;
                m_dMaxX = x2;
            }
            else
            {
                m_dMinX = x2;
                m_dMaxX = x1;
            }

            if (y1 < y2)
            {
                m_dMinY = y1;
                m_dMaxY = y2;
            }
            else
            {
                m_dMinY = y2;
                m_dMaxY = y1;
            }
        }
        
        /// <summary>  
        /// Creates an Envelope for a region defined by two Coordinates.
        /// </summary>
        /// <param name="p1"> the first Coordinate
        /// </param>
        /// <param name="p2"> the second Coordinate
        /// </param>
        public Envelope(Coordinate p1, Coordinate p2) 
            : this(p1.X, p2.X, p1.Y, p2.Y)
        {
        }
        
        /// <summary>  
        /// Creates an Envelope for a region defined by a single Coordinate.
        /// </summary>
        /// <param name="p1"> the Coordinate
        /// </param>
        public Envelope(Coordinate p) : this(p.X, p.X, p.Y, p.Y)
        {
        }
        
        /// <summary>  
        /// Create an Envelope from an existing Envelope.
        /// </summary>
        /// <param name="env"> the Envelope to initialize from
        /// </param>
        public Envelope(Envelope env)
        {
            if (env == null)
            {
                throw new ArgumentNullException("env");
            }

            m_dMinX = env.m_dMinX;
            m_dMaxX = env.m_dMaxX;
            m_dMinY = env.m_dMinY;
            m_dMaxY = env.m_dMaxY;
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>  Initialize to a null Envelope.</summary>
        public virtual void Initialize()
        {
            SetEmpty();
        }
        
        /// <summary>  
        /// Initialize an Envelope for a region defined by maximum and minimum values.
        /// </summary>
        /// <param name="x1"> the first x-value
        /// </param>
        /// <param name="x2"> the second x-value
        /// </param>
        /// <param name="y1"> the first y-value
        /// </param>
        /// <param name="y2"> the second y-value
        /// </param>
        public virtual void Initialize(double x1, double x2, double y1, double y2)
        {
            if (x1 < x2)
            {
                m_dMinX = x1;
                m_dMaxX = x2;
            }
            else
            {
                m_dMinX = x2;
                m_dMaxX = x1;
            }

            if (y1 < y2)
            {
                m_dMinY = y1;
                m_dMaxY = y2;
            }
            else
            {
                m_dMinY = y2;
                m_dMaxY = y1;
            }
        }
        
        /// <summary>  
        /// Initialize an Envelope to a region defined by two Coordinates.
        /// </summary>
        /// <param name="p1"> the first Coordinate
        /// </param>
        /// <param name="p2"> the second Coordinate
        /// </param>
        public virtual void Initialize(Coordinate p1, Coordinate p2)
        {
            Initialize(p1.X, p2.X, p1.Y, p2.Y);
        }
        
        /// <summary>  
        /// Initialize an Envelope to a region defined by a single Coordinate.
        /// </summary>
        /// <param name="p1"> the first Coordinate
        /// </param>
        /// <param name="p2"> the second Coordinate
        /// </param>
        public virtual void Initialize(Coordinate p)
        {
            Initialize(p.X, p.X, p.Y, p.Y);
        }
        
        /// <summary>  
        /// Initialize an Envelope from an existing Envelope.
        /// </summary>
        /// <param name="env"> the Envelope to initialize from
        /// </param>
        public virtual void Initialize(Envelope env)
        {
            m_dMinX = env.m_dMinX;
            m_dMaxX = env.m_dMaxX;
            m_dMinY = env.m_dMinY;
            m_dMaxY = env.m_dMaxY;
        }
        
        #endregion
        
        #region Public Properties
        
        /// <summary>  
        /// Returns true if this Envelope is a "null" envelope.
        /// </summary>
        /// <returns>    true if this Envelope is uninitialized
        /// or is the envelope of the empty geometry.
        /// </returns>
        public virtual bool IsEmpty
        {
            get
            {
                return ((m_dMinX == 0.0) && (m_dMaxX == 0.0) && 
                    (m_dMinY == 0.0) && (m_dMaxY == 0.0));
            }
        }

        public virtual bool IsNormalized
        {
            get
            {
                return (m_dMinX <= m_dMaxX && m_dMinY <= m_dMaxY);
            }
        }

        /// <summary>  
        /// Returns the difference between the maximum and minimum x values.
        /// </summary>
        /// <returns>    max x - min x, or 0 if this is a null Envelope
        /// </returns>
        public virtual double Width
        {
            get
            {
                if (IsEmpty)
                {
                    return 0;
                }

                return (m_dMaxX - m_dMinX);
            }
        }
            
        /// <summary>  
        /// Returns the difference between the maximum and minimum y values.
        /// </summary>
        /// <returns>    max y - min y, or 0 if this is a null Envelope
        /// </returns>
        public virtual double Height
        {
            get
            {
                if (IsEmpty)
                {
                    return 0;
                }

                return (m_dMaxY - m_dMinY);
            }
        }
            
        /// <summary>  
        /// Returns the Envelopes minimum x-value. min x > max x indicates 
        /// that this is a null Envelope.
        /// </summary>
        /// <returns>    the minimum x-coordinate
        /// </returns>
        public virtual double MinX
        {
            get
            {
                return m_dMinX;
            }

            set
            {
                m_dMinX = value;
            }
        }

        /// <summary>  Returns the Envelopes maximum x-value. min x > max x
        /// indicates that this is a null Envelope.
        /// 
        /// </summary>
        /// <returns>    the maximum x-coordinate
        /// </returns>
        public virtual double MaxX
        {
            get
            {
                return m_dMaxX;
            }

            set
            {
                m_dMaxX = value;
            }
        }

        /// <summary>  Returns the Envelopes minimum y-value. min y > max y
        /// indicates that this is a null Envelope.
        /// 
        /// </summary>
        /// <returns>    the minimum y-coordinate
        /// </returns>
        public virtual double MinY
        {
            get
            {
                return m_dMinY;
            }

            set
            {
                m_dMinY = value;
            }
        }

        /// <summary>  Returns the Envelopes maximum y-value. min y > max y
        /// indicates that this is a null Envelope.
        /// 
        /// </summary>
        /// <returns>    the maximum y-coordinate
        /// </returns>
        public virtual double MaxY
        {
            get
            {
                return m_dMaxY;
            }

            set
            {
                m_dMaxY = value;
            }
        }

        public virtual Coordinate Min
        {
            get
            {
                return new Coordinate(m_dMinX, m_dMinY);
            }
        }

        public virtual Coordinate Max
        {
            get
            {
                return new Coordinate(m_dMaxX, m_dMaxY);
            }
        }

        public Coordinate Center
        {
            get
            {
                return new Coordinate((m_dMinX + m_dMaxX) / 2.0, 
                    (m_dMinY + m_dMaxY) / 2.0);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        public override bool Equals(System.Object other)
        {
            if (!(other is Envelope))
            {
                return false;
            }

            Envelope otherEnvelope = (Envelope) other;
            if (IsEmpty)
            {
                return otherEnvelope.IsEmpty;
            }

            return (m_dMaxX == otherEnvelope.m_dMaxX) && 
                (m_dMaxY == otherEnvelope.m_dMaxY)    && 
                (m_dMinX == otherEnvelope.m_dMinX)    && 
                (m_dMinY == otherEnvelope.m_dMinY);
        }
        
        public override System.String ToString()
        {
            return "Envelope[" + m_dMinX + " : " + m_dMaxX + ", " + m_dMinY + " : " + m_dMaxY + "]";
        }
        
        public override int GetHashCode()
        {
            //Algorithm from Effective Java by Joshua Bloch [Jon Aquino]
            int result = 17;
            result = 37 * result + Coordinate.HashCode(m_dMinX);
            result = 37 * result + Coordinate.HashCode(m_dMaxX);
            result = 37 * result + Coordinate.HashCode(m_dMinY);
            result = 37 * result + Coordinate.HashCode(m_dMaxY);

            return result;
        }
        
        /// <summary> 
        /// Test the point q to see whether it Intersects 
        /// the Envelope defined by p1-p2.
        /// </summary>
        /// <param name="p1">one extremal point of the envelope
        /// </param>
        /// <param name="p2">another extremal point of the envelope
        /// </param>
        /// <param name="q">the point to test for intersection
        /// </param>
        /// <returns> true if q Intersects the envelope p1-p2
        /// </returns>
        public static bool Intersects(Coordinate p1, Coordinate p2, Coordinate q)
        {
            if (((q.X >= (p1.X < p2.X ? p1.X : p2.X)) && 
                (q.X <= (p1.X > p2.X ? p1.X : p2.X))) && 
                ((q.Y >= (p1.Y < p2.Y ? p1.Y : p2.Y)) && 
                (q.Y <= (p1.Y > p2.Y ? p1.Y : p2.Y))))
            {
                return true;
            }

            return false;
        }
        
        /// <summary> Test the envelope defined by p1-p2 for intersection
        /// with the envelope defined by q1-q2
        /// </summary>
        /// <param name="p1">one extremal point of the envelope P
        /// </param>
        /// <param name="p2">another extremal point of the envelope P
        /// </param>
        /// <param name="q1">one extremal point of the envelope Q
        /// </param>
        /// <param name="q2">another extremal point of the envelope Q
        /// </param>
        /// <returns> true if Q Intersects P
        /// </returns>
        public static bool Intersects(Coordinate p1, Coordinate p2, 
            Coordinate q1, Coordinate q2)
        {
            double minq = Math.Min(q1.X, q2.X);
            double maxq = Math.Max(q1.X, q2.X);
            double minp = Math.Min(p1.X, p2.X);
            double maxp = Math.Max(p1.X, p2.X);
            
            if (minp > maxq)
                return false;
            
            if (maxp < minq)
                return false;
            
            minq = Math.Min(q1.Y, q2.Y);
            maxq = Math.Max(q1.Y, q2.Y);
            minp = Math.Min(p1.Y, p2.Y);
            maxp = Math.Max(p1.Y, p2.Y);
            
            if (minp > maxq)
                return false;
            
            if (maxp < minq)
                return false;
            
            return true;
        }
                
        /// <summary>  
        /// Makes this Envelope a "null" envelope, that is, the envelope
        /// of the empty geometry.
        /// </summary>
        public virtual void SetEmpty()
        {
            m_dMinX = 0.0;
            m_dMaxX = 0.0;

            m_dMinY = 0.0;
            m_dMaxY = 0.0;
        }
        
        /// <summary>  Enlarges the boundary of the Envelope so that it Contains
        /// (x,y). Does nothing if (x,y) is already on or Within the boundaries.
        /// 
        /// </summary>
        /// <param name="x"> the value to lower the minimum x to or to raise the maximum x to
        /// </param>
        /// <param name="y"> the value to lower the minimum y to or to raise the maximum y to
        /// </param>
        public virtual void ExpandToInclude(Coordinate p)
        {
            ExpandToInclude(p.X, p.Y);
        }
        
        /// <summary>  Enlarges the boundary of the Envelope so that it Contains
        /// (x,y). Does nothing if (x,y) is already on or Within the boundaries.
        /// 
        /// </summary>
        /// <param name="x"> the value to lower the minimum x to or to raise the maximum x to
        /// </param>
        /// <param name="y"> the value to lower the minimum y to or to raise the maximum y to
        /// </param>
        public virtual void ExpandToInclude(double x, double y)
        {
            if (IsEmpty)
            {
                m_dMinX = x;
                m_dMaxX = x;
                m_dMinY = y;
                m_dMaxY = y;
            }
            else
            {
                if (x < m_dMinX)
                {
                    m_dMinX = x;
                }
                
                if (x > m_dMaxX)
                {
                    m_dMaxX = x;
                }
                
                if (y < m_dMinY)
                {
                    m_dMinY = y;
                }

                if (y > m_dMaxY)
                {
                    m_dMaxY = y;
                }
            }
        }
        
        /// <summary>  
        /// Enlarges the boundary of the Envelope so that it Contains
        /// other. Does nothing if other is wholly on or
        /// Within the boundaries.
        /// </summary>
        /// <param name="other"> the Envelope to merge with
        /// </param>
        public virtual void ExpandToInclude(Envelope other)
        {
            if (other.IsEmpty)
            {
                return;
            }

            if (IsEmpty)
            {
                m_dMinX = other.MinX;
                m_dMaxX = other.MaxX;
                m_dMinY = other.MinY;
                m_dMaxY = other.MaxY;
            }
            else
            {
                if (other.m_dMinX < m_dMinX)
                {
                    m_dMinX = other.m_dMinX;
                }
                if (other.m_dMaxX > m_dMaxX)
                {
                    m_dMaxX = other.m_dMaxX;
                }
                if (other.m_dMinY < m_dMinY)
                {
                    m_dMinY = other.m_dMinY;
                }
                if (other.m_dMaxY > m_dMaxY)
                {
                    m_dMaxY = other.m_dMaxY;
                }
            }
        }
        
        /// <summary>  
        /// Returns true if the given point lies in or on the envelope.
        /// </summary>
        /// <param name="p"> the point which this Envelope is
        /// being checked for containing
        /// </param>
        /// <returns>    true if the point lies in the interior or
        /// on the boundary of this Envelope.
        /// </returns>
        public virtual bool Contains(Coordinate p)
        {
            return Contains(p.X, p.Y);
        }
        
        /// <summary>  
        /// Returns true if the given point lies in or on the envelope.
        /// </summary>
        /// <param name="x"> the x-coordinate of the point which this Envelope is
        /// being checked for containing
        /// </param>
        /// <param name="y"> the y-coordinate of the point which this Envelope is
        /// being checked for containing
        /// </param>
        /// <returns> true if (x, y) lies in the interior or
        /// on the boundary of this Envelope.
        /// </returns>
        public virtual bool Contains(double x, double y)
        {
            return x >= m_dMinX && x <= m_dMaxX && y >= m_dMinY && y <= m_dMaxY;
        }
        
        /// <summary>  
        /// Check if the region defined by other overlaps (intersects) 
        /// the region of this Envelope.
        /// </summary>
        /// <param name="other"> the Envelope which this Envelope is
        /// being checked for overlapping
        /// </param>
        /// <returns> true if the Envelopes overlap
        /// </returns>
        public virtual bool Intersects(Envelope other)
        {
            if (IsEmpty || other.IsEmpty)
            {
                return false;
            }

            return !(other.MinX > m_dMaxX || other.MaxX < m_dMinX || 
                other.MinY > m_dMaxY || other.MaxY < m_dMinY);
        }
        
        /// <summary>  Check if the point p
        /// Overlaps (lies inside) the region of this Envelope.
        /// 
        /// </summary>
        /// <param name="other"> the Coordinate to be tested
        /// </param>
        /// <returns>        true if the point Overlaps this Envelope
        /// </returns>
        public virtual bool Intersects(Coordinate p)
        {
            return Intersects(p.X, p.Y);
        }

        /// <summary>  Check if the point (x, y)
        /// Overlaps (lies inside) the region of this Envelope.
        /// 
        /// </summary>
        /// <param name="x"> the x-ordinate of the point
        /// </param>
        /// <param name="y"> the y-ordinate of the point
        /// </param>
        /// <returns>        true if the point Overlaps this Envelope
        /// </returns>
        public virtual bool Intersects(double x, double y)
        {
            return !(x > m_dMaxX || x < m_dMinX || y > m_dMaxY || y < m_dMinY);
        }
        
        /// <summary>  Returns true if the Envelope other
        /// lies wholely inside this Envelope (inclusive of the boundary).
        /// 
        /// </summary>
        /// <param name="other"> the Envelope which this Envelope is
        /// being checked for containing
        /// </param>
        /// <returns>        true if other
        /// is contained in this Envelope
        /// </returns>
        public virtual bool Contains(Envelope other)
        {
            if (IsEmpty || other.IsEmpty)
            {
                return false;
            }
            return other.MinX >= m_dMinX && other.MaxX <= m_dMaxX && 
                other.MinY >= m_dMinY && other.MaxY <= m_dMaxY;
        }
        
        /// <summary> Computes the distance between this and another
        /// Envelope.
        /// The distance between overlapping Envelopes is 0.  Otherwise, the
        /// Distance is the Euclidean Distance between the closest points.
        /// </summary>
        public virtual double Distance(Envelope env)
        {
            if (Intersects(env))
                return 0;
            
            double dx = 0.0;
            
            if (m_dMaxX < env.m_dMinX)
                dx = env.m_dMinX - m_dMaxX;
            
            if (m_dMinX > env.m_dMaxX)
                dx = m_dMinX - env.m_dMaxX;
            
            double dy = 0.0;

            if (m_dMaxY < env.m_dMinY)
                dy = env.m_dMinY - m_dMaxY;
            
            if (m_dMinY > env.m_dMaxY)
                dy = m_dMinY - env.m_dMaxY;
            
            // if either is zero, the envelopes overlap either 
            // vertically or horizontally
            if (dx == 0.0)
                return dy;
            
            if (dy == 0.0)
                return dx;
            
            return Math.Sqrt(dx * dx + dy * dy);
        }
        
        public void Inflate(double cxy)
        {
            Inflate(cxy, cxy);
        }

        public void Inflate(double cx, double cy)
        {
            m_dMinX -= cx;
            m_dMaxX += cx;
            m_dMinY -= cy;
            m_dMaxY += cy;
        }

        // Translate the rectangle by <by>
        public void Translate(Coordinate by)
        {
            TransformUtil.Translate(ref m_dMinX, ref m_dMinY, by.X, by.Y);
            TransformUtil.Translate(ref m_dMaxX, ref m_dMaxY, by.X, by.Y);
        }

        // Translate the rectangle by [ <dx>, <dy> ]
        public void Translate(double dx, double dy)
        {
            TransformUtil.Translate(ref m_dMinX, ref m_dMinY, dx, dy);
            TransformUtil.Translate(ref m_dMaxX, ref m_dMaxY, dx, dy);
        }

        // Reflect the rectangle in the XAxis
        public void ReflectXAxis()
        {
            TransformUtil.ReflectXAxis(ref m_dMinX, ref m_dMinY);
            TransformUtil.ReflectXAxis(ref m_dMaxX, ref m_dMaxY);

            Normalize();
        }

        // Reflect the rectangle in the YAxis
        public void ReflectYAxis()
        {
            TransformUtil.ReflectYAxis(ref m_dMinX, ref m_dMinY);
            TransformUtil.ReflectYAxis(ref m_dMaxX, ref m_dMaxY);
        }

        // Reflect the rectangle in the origin
        public void ReflectOrigin()
        {
            TransformUtil.ReflectOrigin(ref m_dMinX, ref m_dMinY);
            TransformUtil.ReflectOrigin(ref m_dMaxX, ref m_dMaxY);
        }

        public void Scale(double sx, double sy)
        {
            Coordinate center = this.Center;
            double cenX = center.X; 
            double cenY = center.Y;

            TransformUtil.Scale(ref m_dMinX, ref m_dMinY, sx, sy, cenX, cenY);
            TransformUtil.Scale(ref m_dMaxX, ref m_dMaxY, sx, sy, cenX, cenY);
        }

        // Ensure the rectangle is of the forms ( lowerLeft, upperRight )
        public void Normalize()
        {
            if (m_dMaxX < m_dMinX)
            {
                double tmp = m_dMinX;
                m_dMinX = m_dMaxX;
                m_dMaxX = tmp;
            }

            if (m_dMinY > m_dMaxY)
            {
                double tmp = m_dMinY;
                m_dMinY = m_dMaxY;
                m_dMaxY = tmp;
            }
        }
        
        /// <summary>
        /// Computes the intersection of two <see cref="Envelope"/>s.
        /// </summary>
        /// <param name="env">The envelope to intersect with.</param>
        /// <returns>
        /// The intersection of the envelopes (this will be the null envelope 
        /// if either argument is empty, or they do not intersect.
        /// </returns>
        public virtual Envelope Intersection(Envelope env)
        {
            if (this.IsEmpty || env.IsEmpty || !Intersects(env)) 
                return null;

            double intMinX = m_dMinX > env.m_dMinX ? m_dMinX : env.m_dMinX;
            double intMinY = m_dMinY > env.m_dMinY ? m_dMinY : env.m_dMinY;
            double intMaxX = m_dMaxX < env.m_dMaxX ? m_dMaxX : env.m_dMaxX;
            double intMaxY = m_dMaxY < env.m_dMaxY ? m_dMaxY : env.m_dMaxY;

            return new Envelope(intMinX, intMaxX, intMinY, intMaxY);
        }

        #endregion

        #region ICloneable Members

        public virtual Envelope Clone()
        {
            return new Envelope(m_dMinX, m_dMinY, m_dMaxX, m_dMaxY);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }
}