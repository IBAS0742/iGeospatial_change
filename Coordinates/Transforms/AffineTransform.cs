using System;

namespace iGeospatial.Coordinates.Transforms
{
    /// <summary> 
    /// The AffineTransform represents 3x3 matrix used for transformations in 
    /// the two-dimensional space. As such the matrix structure has six entries
    /// instead of nine.
    /// </summary>
    /// <remarks>
    /// In general, affine transformation performs a linear mapping from 
    /// two-dimensional coordinates space to another two-dimensional coordinates space
    /// such that the "straightness" and "parallelness" of lines are preserved.  
    /// Affine transformations can be constructed using sequences of translations, 
    /// scales, rotations, and shears.
    /// <para>
    /// Such a coordinate transformation can be represented by a 3 row by 3 column 
    /// matrix with an implied last row of <c>[0 0 1]</c>.  This matrix transforms 
    /// source coordinates <c>(x, y)</c> into destination coordinates <c>(x', y')</c> 
    /// by considering them to be a column vector and multiplying the coordinate vector
    /// by the matrix according to the following equation:
    /// </para>
    /// <code>
    /// X'    = A * X
    /// 
    /// or 
    /// 
    /// [ x']   [  a11  a12  a13  ] [ x ]   [ a11x + a12y + a13 ]
    /// [ y'] = [  a21  a22  a23  ] [ y ] = [ a21x + a22y + a23 ]
    /// [ 1 ]   [   0    0    1   ] [ 1 ]   [         1         ]
    /// </code>
    /// <para>
    /// In this implementation, many constructors are provided for initialization 
    /// the matrix including initialization by specifying three points and the 
    /// three points they map to.
    /// </para>
    /// <code>
    /// [ x1'] = [  x1 y1 1  0  0  0  ] [ a11 ]
    /// [ y1'] = [  0  0  0  x1 y1 1  ] [ a12 ]
    /// [ x2'] = [  x2 y2 1  0  0  0  ] [ a13 ]
    /// [ y2'] = [  0  0  0  x2 y2 1  ] [ a21 ]
    /// [ x3'] = [  x3 y3 1  0  0  0  ] [ a22 ]
    /// [ y3'] = [  0  0  0  x3 y3 1  ] [ a23 ]
    /// 
    /// or
    /// 
    /// X' = B * X 
    /// 
    /// Solution: X = (inverse B) * X'
    /// </code>
    /// In terms of the properties, the AffineTransform class can be represented 
    /// by the following matrix:
    /// <code>
    /// [ ScaleX   ShearX  TranslateX ]      [ a11  a12  a13 ]
    /// [ ShearY   ScaleY  TranslateY ]  ==  [ a21  a22  a23 ]
    /// [   0        0        1       ]      [  0    0    1  ]
    /// </code>
    /// <para>
    /// Note that this representation of the affine transformation matrix is standard,
    /// but it is different from the .NET Framework matrix for GDI+. 
    /// </para>
    /// For a tutorial on affine transformation and matrix representation, see 
    /// <see href="http://graphics.lcs.mit.edu/classes/6.837/F01/Lecture07/">
    /// Tranformations Lecture Slides</see> and 
    /// <see href="http://www.j3d.org/matrix_faq/matrfaq_latest.html">
    /// Matrix and Quaternions FAQ.
    /// </see>
    /// </remarks>
    /// <seealso href="http://graphics.lcs.mit.edu/classes/6.837/F01/Lecture07/">
    /// Tranformations Lecture Slides
    /// </seealso>
    [Serializable]
    public class AffineTransform : ICoordinateVisitor, ICloneable
	{
        #region Private Members

        /// <summary>
        /// The matrix of this affine transformation.
        /// </summary>
        private GeneralMatrix m_objMatrix;

        /// <summary>
        /// The matrix element at row = 1, column = 1
        /// </summary>
        private double A11;

        /// <summary>
        /// The matrix element at row = 1, column = 2
        /// </summary>
        private double A12;

        /// <summary>
        /// The matrix element at row = 1, column = 3
        /// </summary>
        private double A13;

        /// <summary>
        /// The matrix element at row = 2, column = 1
        /// </summary>
        private double A21;

        /// <summary>
        /// The matrix element at row = 2, column = 2
        /// </summary>
        private double A22;

        /// <summary>
        /// The matrix element at row = 2, column = 3
        /// </summary>
        private double A23;

        #endregion

        #region Constructors and Destructor
		
        /// <overloads>
        /// Initializes a new instance of the <see cref="AffineTransform"/> class. 
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="AffineTransform"/> class 
        /// with the specified elements.
        /// </summary>
        /// <param name="a11">
        /// The value in the first row and first column of the new 
        /// <see cref="AffineTransform"/>.
        /// </param>
        /// <param name="a12">
        /// The value in the first row and second column of the new 
        /// <see cref="AffineTransform"/>.
        /// </param>
        /// <param name="a21">
        /// The value in the second row and first column of the new 
        /// <see cref="AffineTransform"/>.
        /// </param>
        /// <param name="a22">
        /// The value in the second row and second column of the new 
        /// <see cref="AffineTransform"/>.
        /// </param>
        /// <param name="a13">
        /// The value in the first row and third column of the new 
        /// <see cref="AffineTransform"/>.
        /// </param>
        /// <param name="a23">
        /// The value in the second row and third column of the new 
        /// <see cref="AffineTransform"/>.
        /// </param>
        /// <remarks>
        /// To avoid mistakes, pay careful attention to the position of the 
        /// elements. The resulting affine transformation matrix will be as follows:
        /// <code>
        /// [  a11  a12  a13 ]
        /// [  a21  a22  a23 ]
        /// [   0    0    1  ]
        /// </code>
        /// </remarks>
        public AffineTransform(double a11, double a12, double a21, double a22, 
            double a13, double a23)
        {
            m_objMatrix = CreateAffineMatrix(a11, a12, a21, a22, a13, a23);

            A11 = a11;
            A12 = a12;
            A13 = a13;
            A21 = a21;
            A22 = a22;
            A23 = a23;
        }
        
        /// <summary> 
        /// Initializes a new instance of the <see cref="AffineTransform"/> 
        /// tranforamtion class that maps a coordinate space with a known point p1 to 
        /// another coordinate space with a known point q1 via a translation 
        /// (no rotation or shear).
        /// </summary>
        /// <param name="p1">A point in the source coordinate space.</param>
        /// <param name="q1">A point in the destination coordinate space.</param>
        public AffineTransform(Coordinate p1, Coordinate q1)
        {
            Coordinate p2 = new Coordinate(p1.X + 10, p1.Y);
            Coordinate q2 = new Coordinate(q1.X + 10, q1.Y);

            Coordinate p3 = new Coordinate(p1.X, p1.Y + 10);
            Coordinate q3 = new Coordinate(q1.X, q1.Y + 10);
            
            m_objMatrix = Initialize(p1, q1, p2, q2, p3, q3);
 
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }
		
        /// <summary> 
        /// Initializes a new instance of the <see cref="AffineTransform"/> 
        /// transformation class that maps a coordinate space with known two 
        /// points, p1 and p2, to another coordinate space with known two points
        /// q1 and q2, where p1 maps to q1 and p2 maps to q2 via a translation,
        /// rotation, and scaling (no "relative" shear).
        /// </summary>
        /// <param name="p1">A point in the source coordinate space.</param>
        /// <param name="q1">
        /// A point in the destination coordinate space, the point p1 maps to.
        /// </param>
        /// <param name="p2">Another point in the source coordinate space.</param>
        /// <param name="q2">
        /// A point in the destination coordinate space, the point p2 maps to.
        /// </param>
        public AffineTransform(Coordinate p1, Coordinate q1, Coordinate p2, Coordinate q2)
        {
            Coordinate p3 = Rotate90(p1, p2);
            Coordinate q3 = Rotate90(q1, q2);
            
            m_objMatrix = Initialize(p1, q1, p2, q2, p3, q3);
 
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }
		
        /// <summary> 
        /// Initializes a new instance of the <see cref="AffineTransform"/> 
        /// transformation class that maps coordinate space of known three points 
        /// (p1, p2, p3) to another coordinate space of known three points 
        /// (q1, q2, q3), where p1 maps to q1, p2 maps to q2 and p3 maps to q3.
        /// </summary>
        /// <param name="p1">A point in the source coordinate space.</param>
        /// <param name="q1">
        /// A point in the destination coordinate space, the point p1 maps to.
        /// </param>
        /// <param name="p2">Another point in the source coordinate space.</param>
        /// <param name="q2">
        /// A point in the destination coordinate space, the point p2 maps to.
        /// </param>
        /// <param name="p3">Another point in the source coordinate space.</param>
        /// <param name="q3">
        /// A point in the destination coordinate space, the point p3 maps to.
        /// </param>
        /// <remarks>
        /// Depending on the arrangements and orientations of the points in each 
        /// coordinate space, this could results in translation, 
        /// rotation and shearing.
        /// </remarks>
        public AffineTransform(Coordinate p1, Coordinate q1, Coordinate p2, 
            Coordinate q2, Coordinate p3, Coordinate q3)
        {
            m_objMatrix = Initialize(p1, q1, p2, q2, p3, q3);
 
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }
		
        /// <summary>
        /// Initializes a new instance of the <see cref="AffineTransform"/> class
        /// with elements from another <see cref="AffineTransform"/> object, a copy
        /// constructor. 
        /// </summary>
        /// <param name="affine">
        /// The <see cref="AffineTransform"/> object from which the affine 
        /// transformation elements of the newly created object are copied.
        /// </param>
        public AffineTransform(AffineTransform affine)
		{
            m_objMatrix = affine.m_objMatrix.Copy();
 
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }
		
        /// <summary>
        /// Initializes a new instance of the <see cref="AffineTransform"/>
        /// class with elements from a <see cref="GeneralMatrix"/> object.
        /// </summary>
        /// <param name="matrix">
        /// The <see cref="GeneralMatrix"/> object from which the affine 
        /// transformation elements of the newly created object are copied.
        /// </param>
        internal AffineTransform(GeneralMatrix matrix)
        {
            m_objMatrix = matrix.Copy();
 
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether this <see cref="AffineTransform"/> 
        /// object is the identity matrix.
        /// </summary>
        /// <value>
        /// This property is true if this <see cref="AffineTransform"/> is 
        /// identity; otherwise, false.
        /// </value>
        /// <remarks>
        /// An identity <see cref="AffineTransform"/> object is represented as:
        /// <code>
        /// [ 1    0    0 ]
        /// [ 0    1    0 ]
        /// [ 0    0    1 ]
        /// </code>
        /// An identity matrix transforms a point to the same point, that is 
        /// it has not affect the coordinates of the point.
        /// </remarks>
        public bool IsIdentity
        {
            get
            {
                if (A11 != 1)
                    return false;
                if (A12 != 0)
                    return false;
                if (A13 != 0)
                    return false;

                if (A21 != 0)
                    return false;
                if (A22 != 1)
                    return false;
                if (A23 != 0)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="AffineTransform"/> 
        /// object has inverse or is invertible.
        /// </summary>
        /// <value>
        /// true if the <see cref="AffineTransform"/> is invertible, false otherwise.
        /// </value>
        /// <remarks>
        /// A matrix is invertible if the determinant is non-zero.
        /// </remarks>
        public bool HasInverse
        {
            get
            {
                return ((A11 * A22 - A21 * A12) != 0);
            }
        }

        /// <summary>
        /// Gets the determinant of the affine matrix.
        /// </summary>
        /// <value>
        /// The determinant of the matrix.
        /// </value>
        /// <remarks>
        /// The determinant of a matrix is a value that is used to indicate whether
        /// the matrix has an inverse. If zero, then no inverse exists. If non-zero,
        /// then the matrix has an inverse.
        /// <para>
        /// Mathematically, the determinant of a matrix is calculated using the 
        /// Kramer's rule and for the two-dimensional affine matrix this reduces to:
        /// </para>
        /// <code>
        ///	    |  a11  a12  a13  |
        ///	    |  a21  a22  a23  |  =  a11 * a22 - a21 * a12
        ///     |   0    0    1   |
        /// </code>
        /// </remarks>
        public double Determinant
        {
            get
            {
                return (A11 * A22 - A21 * A12);
            }
        }

        /// <summary>
        /// Gets an array of floating-point values that represents the elements 
        /// of this <see cref="AffineTransform"/> object.
        /// </summary>
        /// <value>
        /// An array of floating-point values that represents the elements of 
        /// this <see cref="AffineTransform"/> object.
        /// </value>
        public double[] Elements
        {
            get
            {
                double[] elements = new double[6];
                elements[0] = A11;
                elements[1] = A12;
                elements[2] = A13;
                elements[3] = A21;
                elements[4] = A22;
                elements[5] = A23;

                return elements;
            }

            set
            {
                if (value.Length == 6)
                {
                    m_objMatrix = CreateAffineMatrix(value);

                    A11 = value[0];
                    A12 = value[1];
                    A13 = value[2];
                    A21 = value[3];
                    A22 = value[4];
                    A23 = value[5];
                }
            }
        }

        /// <summary>
        /// Gets or sets the X coordinate scaling element (a11) of the 3x3 affine 
        /// transformation matrix.
        /// </summary>
        /// <value>
        /// The X coordinate scalling element.
        /// </value>
        public double ScaleX
        {
            get
            {
                return A11;
            }

            set
            {
                A11 = value;
                m_objMatrix.SetElement(0, 0, A11);
            }
        }

        /// <summary>
        /// Gets or sets the Y coordinate scaling element (a22) of the 3x3 affine 
        /// transformation matrix.
        /// </summary>
        /// <value>
        /// The Y coordinate scalling element.
        /// </value>
        public double ScaleY
        {
            get
            {
                return A22;
            }

            set
            {
                A22 = value;
                m_objMatrix.SetElement(1, 1, A22);
            }
        }

        /// <summary>
        /// Gets or sets the X coordinate shearing element (a12) of the 3x3 affine 
        /// transformation matrix.
        /// </summary>
        /// <value>
        /// The X coordinate shearing element.
        /// </value>
        public double ShearX
        {
            get
            {
                return A12;
            }

            set
            {
                A12 = value;
                m_objMatrix.SetElement(0, 1, A12);
            }
        }

        /// <summary>
        /// Gets or sets the Y coordinate shearing element (a21) of the 3x3 affine 
        /// transformation matrix.
        /// </summary>
        /// <value>
        /// The Y coordinate shearing element.
        /// </value>
        public double ShearY
        {
            get
            {
                return A21;
            }

            set
            {
                A21 = value;
                m_objMatrix.SetElement(1, 0, A21);
            }
        }

        /// <summary>
        /// Gets or sets the X coordinate translation element (a13) of the 3x3 affine
        /// transformation matrix.
        /// </summary>
        /// <value>
        /// The X coordinate translation element.
        /// </value>
        public double TranslateX
        {
            get
            {
                return A13;
            }

            set
            {
                A13 = value;
                m_objMatrix.SetElement(0, 2, A13);
            }
        }

        /// <summary>
        /// Gets or sets the Y coordinate translation element (a23) of the 3x3 affine
        /// transformation matrix.
        /// </summary>
        /// <value>
        /// The Y coordinate translation element.
        /// </value>
        public double TranslateY
        {
            get
            {
                return A23;
            }

            set
            {
                A23 = value;
                m_objMatrix.SetElement(1, 2, A23);
            }
        }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets an identity <see cref="AffineTransform"/> matrix object.
        /// </summary>
        /// <value>
        /// A <see cref="AffineTransform"/> object, which has identity matrix.
        /// </value>
        /// <remarks>
        /// An identity <see cref="AffineTransform"/> object is represented as:
        /// <code>
        /// [ 1    0    0 ]
        /// [ 0    1    0 ]
        /// [ 0    0    1 ]
        /// </code>
        /// An identity matrix transforms a point to the same point, that is 
        /// it has not affect the coordinates of the point.
        /// </remarks>
        public static AffineTransform Identity 
        { 
            get
            {
                return new AffineTransform(GeneralMatrix.Identity(3, 3));
            }
        }

        #endregion

        #region Public Methods
		
        /// <summary> 
        /// Applies the affine transform to a point.
        /// </summary>
        /// <param name="point">the input to the affine transform
        /// </param>
        /// <remarks>
        /// This matrix operation is defined as follows:
        /// <code>
        /// [ x' ] = [  a11  a12  a13 ] [ x ]
        /// [ y' ] = [  a21  a22  a23 ] [ y ]
        /// [ 1  ] = [   0    0    1  ] [ 1 ]
        /// </code>
        /// </remarks>
        public void Transform(ref float x, ref float y)
        {
            float X = x;
            float Y = y;
			
            x = (float)((A11 * X) + (A12 * Y) + A13);
            y = (float)((A21 * X) + (A22 * Y) + A23);
        }
		
        /// <summary> 
        /// Applies the affine transform to a point.
        /// </summary>
        /// <param name="point">the input to the affine transform
        /// </param>
        /// <remarks>
        /// This matrix operation is defined as follows:
        /// <code>
        /// [ x' ] = [  a11  a12  a13 ] [ x ]
        /// [ y' ] = [  a21  a22  a23 ] [ y ]
        /// [ 1  ] = [   0    0    1  ] [ 1 ]
        /// </code>
        /// </remarks>
        public void Transform(ref double x, ref double y)
        {
            double X = x;
            double Y = y;
			
            x = (A11 * X) + (A12 * Y) + A13;
            y = (A21 * X) + (A22 * Y) + A23;
        }
		
        /// <summary> 
        /// Applies the affine transform to a point.
        /// </summary>
        /// <param name="point">the input to the affine transform
        /// </param>
        /// <remarks>
        /// This matrix operation is defined as follows:
        /// <code>
        /// [ x' ] = [  a11  a12  a13 ] [ x ]
        /// [ y' ] = [  a21  a22  a23 ] [ y ]
        /// [ 1  ] = [   0    0    1  ] [ 1 ]
        /// </code>
        /// </remarks>
        public void Transform(Coordinate point)
        {
            if (point == null)
            {
                throw new ArgumentNullException("point");
            }

            double x = point.X;
            double y = point.Y;
			
            point.X = (A11 * x) + (A12 * y) + A13;
            point.Y = (A21 * x) + (A22 * y) + A23;
        }

        // Transform a list of points.
        public void Transform(Coordinate[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            double x, y;
            for (int i = points.Length - 1; i >= 0; --i)
            {
                x = points[i].X;
                y = points[i].Y;

                points[i].X = x * A11 + y * A12 + A13;
                points[i].Y = x * A21 + y * A22 + A23;
            }
        }

        // Transform a vector.
        public void TransformVector(Coordinate vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException("vector");
            }

            double x = vector.X;
            double y = vector.Y;

            vector.X = x * A11 + y * A12;
            vector.Y = x * A21 + y * A22;
       }

        // Transform a list of vectors.
        public void TransformVectors(Coordinate[] vectors)
        {
            if (vectors == null)
            {
                throw new ArgumentNullException("vectors");
            }

            double x, y;
            for (int i = vectors.Length - 1; i >= 0; --i)
            {
                x = vectors[i].X;
                y = vectors[i].Y;

                vectors[i].X = x * A11 + y * A12;
                vectors[i].Y = x * A21 + y * A22;
            }
        }

        /// <summary>
        /// Rotates this matrix about the origin.
        /// </summary>
        /// <param name="angle">The angle to rotate specifed in degrees</param>
        /// <remarks> 
        /// This multiplies this transform with a rotation transformation.
        /// It is equivalent to calling <c>AffineTransform.Multiply(R)</c>, 
        /// where R is an <see cref="AffineTransform"/> represented by the 
        /// following matrix:
        /// <code>
        /// [  cos(angle)    -sin(angle)    0  ]
        /// [  sin(angle)     cos(angle)    0  ]
        /// [      0              0         1  ]
        /// </code>
        /// Rotating with a positive angle rotates points on the positive
        /// x axis toward the positive y axis, counterclockwise.
        /// </remarks>
        public void Rotate(double angle)
        {
            Rotate(angle, AffineOrder.Append);
        }

        /// <summary>
        /// Applies a rotation of an amount specified in the angle parameter, 
        /// around the origin (zero x and y coordinates) for this 
        /// <see cref="AffineTransform"/> object.
        /// </summary>
        /// <param name="angle">The angle (extent) of the rotation in degrees.</param>
        /// <param name="order">
        /// A <see cref="AffineOrder"/> enumeration that specifies the order 
        /// (append or prepend) in which the rotation is applied to this 
        /// <see cref="AffineTransform"/> object.
        /// </param>
        /// <remarks> 
        /// This multiplies this transform with a rotation transformation.
        /// It is equivalent to calling <c>AffineTransform.Multiply(R, order)</c>, 
        /// where R is an <see cref="AffineTransform"/> represented by the 
        /// following matrix:
        /// <code>
        /// [  cos(angle)    -sin(angle)    0  ]
        /// [  sin(angle)     cos(angle)    0  ]
        /// [      0              0         1  ]
        /// </code>
        /// Rotating with a positive angle rotates points on the positive
        /// x axis toward the positive y axis, counterclockwise.
        /// </remarks>
        public void Rotate(double angle, AffineOrder order)
        {
            double a11 = 0.0;
            double a12 = 0.0;
            double a13 = 0.0;
            double a21 = 0.0;
            double a22 = 0.0;
            double a23 = 0.0;

            double dsin = Math.Sin(angle * (Math.PI/180));
            double dcos = Math.Cos(angle * (Math.PI/180));

            if (Math.Abs(dsin) < 1e-15)
            {
                a12 = a21 = 0.0;
                if (dcos < 0)
                {
                    a11 = a22 = -1.0;
                }
                else
                {
                    a11 = a22 = 1.0;
                }
            }
            else if (Math.Abs(dcos) < 1e-15)
            {
                a11 = a22 = 0.0;
                if (dsin < 0.0)
                {
                    a12 = 1.0;
                    a21 = -1.0;
                }
                else
                {
                    a12 = -1.0;
                    a21 = 1.0;
                }
            }
            else
            {
                a11 = dcos;
                a12 = -dsin;
                a21 = dsin;
                a22 = dcos;
            }

            // 1. Prepare the rotation transformation.
            GeneralMatrix matrix = CreateAffineMatrix(a11, a12, a21, a22, a13, a23);

            // 2. Multiply the matrices, either by appending or prepending...
            if (order == AffineOrder.Append)
            {
                m_objMatrix = m_objMatrix.Multiply(matrix);
            }
            else
            {
                m_objMatrix = matrix.Multiply(m_objMatrix); 
            }
 
            // 3. Re-initialize the elements
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }

        /// <summary>
        /// Rotates this matrix about the specified point.
        /// </summary>
        /// <param name="angle">The angle to rotate specifed in degrees.</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <remarks> 
        /// This operation multiplies this transform with a transform that rotates
        /// coordinates around an anchor point.
        /// <para>
        /// This operation is equivalent to translating the coordinates so
        /// that the anchor point is at the origin (TX1), then rotating them
        /// about the new origin (TX2), and finally translating so that the
        /// intermediate origin is restored to the coordinates of the original
        /// anchor point (TX3).
        /// </para>
        /// This operation is equivalent to the following sequence of calls:
        /// <code>
        /// Translate(x, y);	// TX3: final translation
        /// Rotate(angle);		// TX2: rotate around anchor
        /// Translate(-x, -y);	// TX1: translate anchor to origin
        /// </code>
        /// Rotating with a positive angle theta rotates points on the positive
        /// x axis toward the positive y axis, counterclockwise.
        /// </remarks>
        public void RotateAt(double angle, double x, double y)
        {
            // Prepare the elements...
            double a11 = 0.0;
            double a12 = 0.0;
            double a13 = 0.0;
            double a21 = 0.0;
            double a22 = 0.0;
            double a23 = 0.0;

            double dsin = Math.Sin(angle * (Math.PI/180));
            double dcos = Math.Cos(angle * (Math.PI/180));

            if (Math.Abs(dsin) < 1e-15)
            {
                a12 = a21 = 0.0;
                if (dcos < 0)
                {
                    a11 = a22 = -1.0;
                }
                else
                {
                    a11 = a22 = 1.0;
                }
            }
            else if (Math.Abs(dcos) < 1e-15)
            {
                a11 = a22 = 0.0;
                if (dsin < 0.0)
                {
                    a12 = 1.0;
                    a21 = -1.0;
                }
                else
                {
                    a12 = -1.0;
                    a21 = 1.0;
                }
            }
            else
            {
                a11 = dcos;
                a12 = -dsin;
                a21 = dsin;
                a22 = dcos;
            }

            // handle the offset point...
            double dSin  = a21;
            double dTemp = 1.0 - a11;
            a13 = x * dTemp + y * dSin;
            a23 = y * dTemp - x * dSin;

            // 1. Prepare the rotation transformation.
            GeneralMatrix matrix = CreateAffineMatrix(a11, a12, a21, a22, a13, a23);

            // 2. Multiply the matrices, either by appending...
            m_objMatrix = m_objMatrix.Multiply(matrix);
 
            // 3. Re-initialize the elements
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }

        public void RotateAt(double angle, double x, double y, AffineOrder order)
        {
            // Prepare the elements...
            double a11 = 0.0;
            double a12 = 0.0;
            double a13 = 0.0;
            double a21 = 0.0;
            double a22 = 0.0;
            double a23 = 0.0;

            double dsin = Math.Sin(angle * (Math.PI/180));
            double dcos = Math.Cos(angle * (Math.PI/180));

            if (Math.Abs(dsin) < 1e-15)
            {
                a12 = a21 = 0.0;
                if (dcos < 0)
                {
                    a11 = a22 = -1.0;
                }
                else
                {
                    a11 = a22 = 1.0;
                }
            }
            else if (Math.Abs(dcos) < 1e-15)
            {
                a11 = a22 = 0.0;
                if (dsin < 0.0)
                {
                    a12 = 1.0;
                    a21 = -1.0;
                }
                else
                {
                    a12 = -1.0;
                    a21 = 1.0;
                }
            }
            else
            {
                a11 = dcos;
                a12 = -dsin;
                a21 = dsin;
                a22 = dcos;
            }

            // handle the offset point...
            double dSin  = a21;
            double dTemp = 1.0 - a11;
            a13 = x * dTemp + y * dSin;
            a23 = y * dTemp - x * dSin;

            // 1. Prepare the rotation transformation.
            GeneralMatrix matrix = CreateAffineMatrix(a11, a12, a21, a22, a13, a23);

            // 2. Multiply the matrices, either by prepending or appending...
            if (order == AffineOrder.Append)
            {
                m_objMatrix = m_objMatrix.Multiply(matrix);
            }
            else
            {
                m_objMatrix = matrix.Multiply(m_objMatrix); 
            }
 
            // 3. Re-initialize the elements
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }

        /// <summary>
        /// Rotates this matrix about the specified point.
        /// </summary>
        /// <param name="angle">The angle to rotate specifed in degrees.</param>
        /// <param name="center">The center of rotation.</param>
        public void RotateAt(double angle, Coordinate center)
        {
            RotateAt(angle, center.X, center.Y);
        }

        public void RotateAt(double angle, Coordinate center, AffineOrder order)
        {
            RotateAt(angle, center.X, center.Y, order);
        }

        /// <summary>
        /// Applies the specified scale vector to this 
        /// <see cref="AffineTransform"/> object around the origin 
        /// by appending the scale vector.
        /// </summary>
        /// <param name="scaleX">
        /// The value by which to scale this <see cref="AffineTransform"/> 
        /// in the x-axis direction.
        /// </param>
        /// <param name="scaleY">
        /// The value by which to scale this <see cref="AffineTransform"/> 
        /// in the y-axis direction.
        /// </param>
        public void Scale(double scaleX, double scaleY)
        {
            Scale(scaleX, scaleY, AffineOrder.Append);
        }

        /// <summary>
        /// Applies the specified scale vector (scaleX and scaleY) to this 
        /// <see cref="AffineTransform"/> object using the specified order.
        /// </summary>
        /// <param name="scaleX">
        /// The value by which to scale this <see cref="AffineTransform"/> 
        /// in the x-axis direction.
        /// </param>
        /// <param name="scaleY">
        /// The value by which to scale this <see cref="AffineTransform"/> 
        /// in the y-axis direction.
        /// </param>
        /// <param name="order">
        /// A <see cref="AffineOrder"/> enumeration that specifies the order 
        /// (append or prepend) in which the scale vector is applied to this 
        /// <see cref="AffineTransform"/>.
        /// </param>
        public void Scale(double scaleX, double scaleY, AffineOrder order)
        {
            // 1. Prepare the translation transformation.
            GeneralMatrix matrix = GeneralMatrix.Identity(3, 3);
            matrix.SetElement(0, 0, scaleX);
            matrix.SetElement(1, 1, scaleY);

            // 2. Multiply the matrices, either by appending or prepending...
            if (order == AffineOrder.Append)
            {
                m_objMatrix = m_objMatrix.Multiply(matrix);
            }
            else
            {
                m_objMatrix = matrix.Multiply(m_objMatrix); 
            }
 
            // 3. Re-initialize the elements
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }

        /// <summary>
        /// Scales this matrix around the point provided.
        /// </summary>
        /// <param name="scaleX">
        /// The value by which to scale this <see cref="AffineTransform"/> 
        /// in the x-axis direction.
        /// </param>
        /// <param name="scaleY">
        /// The value by which to scale this <see cref="AffineTransform"/> 
        /// in the y-axis direction.
        /// </param>
        /// <param name="point">The point about which to scale.</param>
        public void ScaleAt(double scaleX, double scaleY, Coordinate point)
        {
        }

        /// <summary>
        /// Applies the specified shear vector to this <see cref="AffineTransform"/> 
        /// by appending the shear vector.
        /// </summary>
        /// <param name="shearX">The horizontal shear factor.</param>
        /// <param name="shearY">The vertical shear factor. </param>
        /// <remarks>
        /// The transformation applied in this method is a pure shear only if 
        /// one of the parameters is 0. Applied to a rectangle at the origin, 
        /// when the shearY factor is 0, the transformation moves the bottom 
        /// edge horizontally by shearX times the height of the rectangle. 
        /// When the shearX factor is 0, it moves the right edge vertically by 
        /// shearY times the width of the rectangle. Caution is in order when 
        /// both parameters are nonzero, because the results are hard to predict. 
        /// For example, if both factors are 1, the transformation is singular 
        /// (hence noninvertible), squeezing the entire plane to a single line.
        /// </remarks>
        public void Shear(double shearX, double shearY)
        {
            Shear(shearX, shearY, AffineOrder.Append);
        }

        /// <summary>
        /// Applies the specified shear vector to this <see cref="AffineTransform"/> 
        /// object in the specified order.
        /// </summary>
        /// <param name="shearX">The horizontal shear factor.</param>
        /// <param name="shearY">The vertical shear factor. </param>
        /// <param name="order">
        /// A <see cref="AffineOrder"/> enumeration that specifies the order 
        /// (append or prepend) in which the shear is applied.
        /// </param>
        /// <remarks>
        /// The transformation applied in this method is a pure shear only if 
        /// one of the parameters is 0. Applied to a rectangle at the origin, 
        /// when the shearY factor is 0, the transformation moves the bottom 
        /// edge horizontally by shearX times the height of the rectangle. 
        /// When the shearX factor is 0, it moves the right edge vertically by 
        /// shearY times the width of the rectangle. Caution is in order when 
        /// both parameters are nonzero, because the results are hard to predict. 
        /// For example, if both factors are 1, the transformation is singular 
        /// (hence noninvertible), squeezing the entire plane to a single line.
        /// </remarks>
        public void Shear(double shearX, double shearY, AffineOrder order)
        {
            // 1. Prepare the translation transformation.
            GeneralMatrix matrix = GeneralMatrix.Identity(3, 3);
            matrix.SetElement(0, 1, shearX);
            matrix.SetElement(1, 0, shearY);

            // 2. Multiply the matrices, either by appending or prepending...
            if (order == AffineOrder.Append)
            {
                m_objMatrix = m_objMatrix.Multiply(matrix);
            }
            else
            {
                m_objMatrix = matrix.Multiply(m_objMatrix); 
            }
 
            // 3. Re-initialize the elements
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }

        /// <summary> 
        /// Applies the specified translation vector to this 
        /// <see cref="AffineTransform"/> object by appending the translation vector.
        /// </summary>
        /// <param name="translateX">
        /// The distance by which coordinates are translated in the X axis direction.
        /// </param>
        /// <param name="translateY">
        /// The distance by which coordinates are translated in the Y axis direction.
        /// </param>
        /// <remarks>
        /// This operation concatenates this transform with a translation transformation.
        /// It is equivalent to calling <code>AffineTransform.Multiply(T)</code>, 
        /// where T is an <see cref="AffineTransform"/> represented by the following matrix:
        /// <code>
        /// [   1    0    translateX  ]
        /// [   0    1    translateY  ]
        /// [   0    0         1      ]
        /// </code>
        /// </remarks>
        public void Translate(double translateX, double translateY)
        {
            Translate(translateX, translateY, AffineOrder.Append);
        }

        /// <summary> 
        /// Applies the specified translation vector to this 
        /// <see cref="AffineTransform"/> object by prepending the translation vector.
        /// </summary>
        /// <param name="translateX">
        /// The distance by which coordinates are translated in the X axis direction.
        /// </param>
        /// <param name="translateY">
        /// The distance by which coordinates are translated in the Y axis direction.
        /// </param>
        /// <param name="order">
        /// The <see cref="AffineOrder"/> enumeration that represents the 
        /// order of the multiplication. 
        /// </param>
        /// <remarks>
        /// This operation concatenates this transform with a translation transformation.
        /// It is equivalent to calling <code>AffineTransform.Multiply(T, order)</code>, 
        /// where T is an <see cref="AffineTransform"/> represented by the following matrix:
        /// <code>
        /// [   1    0    translateX  ]
        /// [   0    1    translateY  ]
        /// [   0    0         1      ]
        /// </code>
        /// </remarks>
        public void Translate(double translateX, double translateY, AffineOrder order)
        {
            // 1. Prepare the translation transformation.
            GeneralMatrix matrix = GeneralMatrix.Identity(3, 3);
            matrix.SetElement(0, 2, translateX);
            matrix.SetElement(1, 2, translateY);

            // 2. Multiply the matrices, either by appending or prepending...
            if (order == AffineOrder.Append)
            {
                m_objMatrix = m_objMatrix.Multiply(matrix);
            }
            else
            {
                m_objMatrix = matrix.Multiply(m_objMatrix); 
            }
 
            // 3. Re-initialize the elements
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skewX"></param>
        public void SkewX(double skewX)
        {
            // 1. Prepare the skew transformation.
            GeneralMatrix matrix = GeneralMatrix.Identity(3, 3);
            matrix.SetElement(1, 0, Math.Tan(skewX) * (Math.PI/180));

            // 2. Multiply the matrices, by appending...
            m_objMatrix = m_objMatrix.Multiply(matrix);
 
            // 3. Re-initialize the elements
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skewY"></param>
        public void SkewY(double skewY)
        {
            // 1. Prepare the skew transformation.
            GeneralMatrix matrix = GeneralMatrix.Identity(3, 3);
            matrix.SetElement(0, 1, Math.Tan(skewY) * (Math.PI/180));

            // 2. Multiply the matrices, either appending...
            m_objMatrix = m_objMatrix.Multiply(matrix);
 
            // 3. Re-initialize the elements
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skewX"></param>
        /// <param name="skewY"></param>
        /// <remarks>
        /// The skewing matrix represented as:
        /// <code>
        /// [      1        tan(skewY)    0  ]
        /// [  tan(skewX)        1        0  ]
        /// [      0             0        1  ]
        /// </code>
        /// Thus the skew transformation is a special type of shearing
        /// </remarks>
        public void Skew(double skewX, double skewY)
        {
            Skew(skewX, skewY, AffineOrder.Append);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skewX"></param>
        /// <param name="skewY"></param>
        /// <param name="order"></param>
        public void Skew(double skewX, double skewY, AffineOrder order)
        {
            // 1. Prepare the skew transformation.
            GeneralMatrix matrix = GeneralMatrix.Identity(3, 3);
            matrix.SetElement(1, 0, Math.Tan(skewX) * (Math.PI/180));
            matrix.SetElement(0, 1, Math.Tan(skewY) * (Math.PI/180));

            // 2. Multiply the matrices, either by appending or prepending...
            if (order == AffineOrder.Append)
            {
                m_objMatrix = m_objMatrix.Multiply(matrix);
            }
            else
            {
                m_objMatrix = matrix.Multiply(m_objMatrix); 
            }
 
            // 3. Re-initialize the elements
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }

        /// <summary>
        /// Creates and returns the inverse of this transformation, if it is invertible.
        /// </summary>
        /// <returns>
        /// A <see cref="AffineTransform"/> instance, which is the inverse of the
        /// current transform.
        /// </returns>
        /// <remarks>
        /// If you wish to rather inverse the current instance, use the 
        /// <see cref="AffineTransform.Invert"/> method.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// If the affine transform is non-invertable.
        /// </exception>
        public AffineTransform Inverse()
        {
            if ((A11 * A22 - A21 * A12) != 0)
            {
                GeneralMatrix inverse = m_objMatrix.Inverse();

                return new AffineTransform(inverse);
            }

            throw new InvalidOperationException();
        }
        
        /// <summary>
        /// Converts this transform to its inverse, if it is invertible.
        /// </summary>
        /// <remarks>
        /// This operation will change this affine transformation to its inverse.
        /// If you wish to create a separate inverse transform from this, use the
        /// <see cref="AffineTransform.Inverse"/> method.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// If the affine transform is non-invertable.
        /// </exception>
        public void Invert()
        {
            if ((A11 * A22 - A21 * A12) == 0)
            {
                m_objMatrix = m_objMatrix.Inverse();
 
                // Re-initialize the elements
                A11 = m_objMatrix.GetElement(0, 0);
                A12 = m_objMatrix.GetElement(0, 1);
                A13 = m_objMatrix.GetElement(0, 2);

                A21 = m_objMatrix.GetElement(1, 0);
                A22 = m_objMatrix.GetElement(1, 1);
                A23 = m_objMatrix.GetElement(1, 2);
            }

            throw new InvalidOperationException();
        }
		
        /// <overloads>Multiplies this transform by the specified transform.</overloads>
        /// <summary> 
        /// Multiplies this <see cref="AffineTransform"/> matrix object by the 
        /// specified matrix object by appending the specified matrix.
        /// </summary>
        /// <param name="affine">the <code>AffineTransform</code> object to be
        /// concatenated with this <code>AffineTransform</code> object.
        /// </param>
        /// <remarks>
        /// Concatenates an <code>AffineTransform</code> <code>Tx</code> to
        /// this <code>AffineTransform</code> Cx in the most commonly useful
        /// way to provide a new user space
        /// that is mapped to the former user space by <code>Tx</code>.
        /// Cx is updated to perform the combined transformation.
        /// Transforming a point p by the updated transform Cx' is
        /// equivalent to first transforming p by <code>Tx</code> and then
        /// transforming the result by the original transform Cx like this:
        /// Cx'(p) = Cx(Tx(p))  
        /// In matrix notation, if this transform Cx is
        /// represented by the matrix [this] and <code>Tx</code> is represented
        /// by the matrix [Tx] then this method does the following:
        /// <pre>
        /// [this] = [this] x [Tx]
        /// </pre>
        /// </remarks>
        public void Multiply(AffineTransform affine)
        {
            Multiply(affine, AffineOrder.Append);
        }
		
        /// <summary> 
        /// Multiplies this <see cref="AffineTransform"/> matrix object by the matrix 
        /// specified in the matrix parameter, and in the order specified in 
        /// the order parameter.
        /// </summary>
        /// <param name="affine">the <code>AffineTransform</code> object to be
        /// concatenated with this <code>AffineTransform</code> object.
        /// </param>
        /// <remarks>
        /// Concatenates an <code>AffineTransform</code> <code>Tx</code> to
        /// this <code>AffineTransform</code> Cx
        /// in a less commonly used way such that <code>Tx</code> modifies the
        /// coordinate transformation relative to the absolute pixel
        /// space rather than relative to the existing user space.
        /// Cx is updated to perform the combined transformation.
        /// Transforming a point p by the updated transform Cx' is
        /// equivalent to first transforming p by the original transform
        /// Cx and then transforming the result by 
        /// <code>Tx</code> like this: 
        /// Cx'(p) = Tx(Cx(p))  
        /// In matrix notation, if this transform Cx
        /// is represented by the matrix [this] and <code>Tx</code> is
        /// represented by the matrix [Tx] then this method does the
        /// following:
        /// <pre>
        /// [this] = [Tx] x [this]
        /// </pre>
        /// </remarks>
        public void  Multiply(AffineTransform affine, AffineOrder order)
        {
            // 1. Multiply the matrices, either by appending or prepending...
            if (order == AffineOrder.Append)
            {
                m_objMatrix = m_objMatrix.Multiply(affine.m_objMatrix);
            }
            else
            {
                m_objMatrix = affine.m_objMatrix.Multiply(m_objMatrix); 
            }
 
            // 2. Re-initialize the elements
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }
		
        /// <summary> Reset this transform to the Identity transform.</summary>
        public void Reset()
        {
            A11 = A22 = 1.0;
            A12 = A13 = A21 = A23 = 0.0;

            m_objMatrix = GeneralMatrix.Identity(3, 3);
        }
		
        /// <summary> 
        /// Reset this <see cref="AffineTransform"/> to the specified affine transform.
        /// </summary>
        /// <param name="affine">The <see cref="AffineTransform"/> object from which to
        /// copy the transform matrix parameters.
        /// </param>
        public void Reset(AffineTransform affine)
        {
            m_objMatrix = affine.m_objMatrix.Copy();
 
            A11 = m_objMatrix.GetElement(0, 0);
            A12 = m_objMatrix.GetElement(0, 1);
            A13 = m_objMatrix.GetElement(0, 2);

            A21 = m_objMatrix.GetElement(1, 0);
            A22 = m_objMatrix.GetElement(1, 1);
            A23 = m_objMatrix.GetElement(1, 2);
        }
		
        /// <summary>
        /// Reset this <see cref="AffineTransform"/> to the matrix specified by 
        /// the six values.
        /// </summary>
        /// <param name="a11">X scaling element.</param>
        /// <param name="a12">X shearing element.</param>
        /// <param name="a13">X translation element.</param>
        /// <param name="a21">Y shearing element.</param>
        /// <param name="a22">Y scaling element.</param>
        /// <param name="a23">Y translation element.</param>
        public void Reset(double a11, double a12, double a13, 
            double a21, double a22, double a23)
        {
            A11 = a11;
            A12 = a12;
            A13 = a13;

            A21 = a21;
            A22 = a22;
            A23 = a23;

            // Initialize the first row
            m_objMatrix.SetElement(0, 0, a11);
            m_objMatrix.SetElement(0, 1, a12);
            m_objMatrix.SetElement(0, 2, a13);

            // Initialize the second row
            m_objMatrix.SetElement(1, 0, a21);
            m_objMatrix.SetElement(1, 1, a22);
            m_objMatrix.SetElement(1, 2, a23);
            
            // Initialize the third row, just in case!
            m_objMatrix.SetElement(2, 0, 0);
            m_objMatrix.SetElement(2, 1, 0);
            m_objMatrix.SetElement(2, 2, 1);
        }
		
        /// <summary> 
        /// Reset this <see cref="AffineTransform"/> to a translation transformation.
        /// </summary>
        /// <param name="translateX">
        /// The distance by which coordinates are translated in the X direction.
        /// </param>
        /// <param name="translateY">
        /// The distance by which coordinates are translated in the Y direction.
        /// </param>
        /// <remarks>
        /// The matrix representing this transform becomes:
        /// <code>
        /// [ 1    0    translateX ]
        /// [ 0    1    translateY ]
        /// [ 0    0         1     ]
        /// </code>
         /// </remarks>
       public void ResetToTranslation(double translateX, double translateY)
        {
            A11 = 1.0;
            A12 = 0.0;
            A13 = translateX;

            A21 = 0.0;
            A22 = 1.0;
            A23 = translateY;

            // Initialize the first row
            m_objMatrix.SetElement(0, 0, A11);
            m_objMatrix.SetElement(0, 1, A12);
            m_objMatrix.SetElement(0, 2, A13);

            // Initialize the second row
            m_objMatrix.SetElement(1, 0, A21);
            m_objMatrix.SetElement(1, 1, A22);
            m_objMatrix.SetElement(1, 2, A23);
            
            // Initialize the third row, just in case!
            m_objMatrix.SetElement(2, 0, 0);
            m_objMatrix.SetElement(2, 1, 0);
            m_objMatrix.SetElement(2, 2, 1);
        }
		
        /// <summary> 
        /// Reset this <see cref="AffineTransform"/> to a rotation transformation.
        /// </summary>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <remarks>
        /// The matrix representing this transform becomes:
        /// <pre>
        /// [ cos(angle)  -sin(angle)   0 ]
        /// [ sin(angle)   cos(angle)   0 ]
        /// [     0           0         1 ]
        /// </pre>
        /// Rotating with a positive angle rotates points on the positive
        /// X axis toward the positive Y axis in a Cartesian Coordinate System.
        /// </remarks>
        public void ResetToRotation(double angle)
        {
            A13 = 0.0;
            A23 = 0.0;
            double dSin = System.Math.Sin(angle * (Math.PI/180));
            double dCos = System.Math.Cos(angle * (Math.PI/180));

            if (System.Math.Abs(dSin) < 1e-15)
            {
                A12 = A21 = 0.0;
                if (dCos < 0)
                {
                    A11 = A22 = -1.0;
                }
                else
                {
                    A11 = A22 = 1.0;
                }
            }
            else if (System.Math.Abs(dCos) < 1e-15)
            {
                A11 = A22 = 0.0;
                if (dSin < 0.0)
                {
                    A12 = 1.0;
                    A21 = -1.0;
                }
                else
                {
                    A12 = -1.0;
                    A21 = 1.0;
                }
            }
            else
            {
                A11 = dCos;
                A12 = -dSin;
                A21 = dSin;
                A22 = dCos;
            }

            // Initialize the first row
            m_objMatrix.SetElement(0, 0, A11);
            m_objMatrix.SetElement(0, 1, A12);
            m_objMatrix.SetElement(0, 2, A13);

            // Initialize the second row
            m_objMatrix.SetElement(1, 0, A21);
            m_objMatrix.SetElement(1, 1, A22);
            m_objMatrix.SetElement(1, 2, A23);
            
            // Initialize the third row, just in case!
            m_objMatrix.SetElement(2, 0, 0);
            m_objMatrix.SetElement(2, 1, 0);
            m_objMatrix.SetElement(2, 2, 1);
        }
		
        /// <summary> 
        /// Reset this <see cref="AffineTransform"/> to a translated 
        /// rotation transformation.
        /// </summary>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <param name="x">
        /// The X coordinate of the anchor point of the rotation.
        /// </param>
        /// <param name="y">
        /// The Y coordinate of the anchor point of the rotation.
        /// </param>
        /// <remarks>
        /// This operation is equivalent to translating the coordinates so
        /// that the anchor point is at the origin (P1), then rotating them
        /// about the new origin (P2), and finally translating so that the
        /// intermediate origin is restored to the coordinates of the original
        /// anchor point (P3).
        /// <para>
        /// This operation is equivalent to the following sequence of calls:
        /// </para>
        /// <code>
        /// AffineTransform affine = AffineTransform.Identity;
        /// 
        /// affine.ResetTranslation(x, y);  // P3: final translation
        /// affine.Rotate(angle);           // P2: rotate around anchor
        /// affine.Translate(-x, -y);	    // P1: translate anchor to origin
        /// </code>
        /// The matrix representing this transform becomes:
        /// <code>
        /// [ cos(angle)  -sin(angle)  x - x * cos + y * sin ]
        /// [ sin(angle)   cos(angle)  y - x * sin - y * cos ]
        /// [     0            0                   1         ]
        /// </code>
        /// Rotating with a positive angle theta rotates points on the positive
        /// x axis toward the positive y axis.
        /// </remarks>
        public void  ResetToRotation(double angle, double x, double y)
        {
            ResetToRotation(angle);

            double dSin  = A21;
            double dTemp = 1.0 - A11;
            A13 = x * dTemp + y * dSin;
            A23 = y * dTemp - x * dSin;

            // Initialize the first row
            m_objMatrix.SetElement(0, 2, A13);

            // Initialize the second row
            m_objMatrix.SetElement(1, 2, A23);
        }
		
        /// <summary> 
        /// Reset this <see cref="AffineTransform"/> to a scaling transformation.
        /// </summary>
        /// <param name="scaleX">
        /// The factor by which coordinates are scaled along the X direction.
        /// </param>
        /// <param name="scaleY">
        /// The factor by which coordinates are scaled along the Y direction.
        /// </param>
        /// <remarks>
        /// The matrix representing this transform becomes:
        /// <code>
        /// [ scaleX    0      0 ]
        /// [ 0       scaleY   0 ]
        /// [ 0         0      1 ]
        /// </code>
        /// </remarks>
        public void  ResetToScale(double scaleX, double scaleY)
        {
            A11 = scaleX;
            A12 = 0.0;
            A13 = 0.0;

            A21 = 0.0;
            A22 = scaleY;
            A23 = 0.0;

            // Initialize the first row
            m_objMatrix.SetElement(0, 0, A11);
            m_objMatrix.SetElement(0, 1, A12);
            m_objMatrix.SetElement(0, 2, A13);

            // Initialize the second row
            m_objMatrix.SetElement(1, 0, A21);
            m_objMatrix.SetElement(1, 1, A22);
            m_objMatrix.SetElement(1, 2, A23);
            
            // Initialize the third row, just in case!
            m_objMatrix.SetElement(2, 0, 0);
            m_objMatrix.SetElement(2, 1, 0);
            m_objMatrix.SetElement(2, 2, 1);
        }
		
        /// <summary> 
        /// Reset this <see cref="AffineTransform"/> to a shearing transformation.
        /// </summary>
        /// <param name="shearX">the multiplier by which coordinates are shifted in the
        /// direction of the positive X axis as a factor of their Y coordinate
        /// </param>
        /// <param name="shearY">the multiplier by which coordinates are shifted in the
        /// direction of the positive Y axis as a factor of their X coordinate
        /// </param>
        /// <remarks>
        /// The matrix representing this transform becomes:
        /// <code>
        /// [  1      shearX   0 ]
        /// [ shearY    1      0 ]
        /// [  0        0      1 ]
        /// </code>
        /// </remarks>
        public void  ResetToShear(double shearX, double shearY)
        {
            A11 = 1.0;
            A12 = ShearX;
            A13 = 0.0;

            A21 = ShearY;
            A22 = 1.0;
            A23 = 0.0;

            // Initialize the first row
            m_objMatrix.SetElement(0, 0, A11);
            m_objMatrix.SetElement(0, 1, A12);
            m_objMatrix.SetElement(0, 2, A13);

            // Initialize the second row
            m_objMatrix.SetElement(1, 0, A21);
            m_objMatrix.SetElement(1, 1, A22);
            m_objMatrix.SetElement(1, 2, A23);
            
            // Initialize the third row, just in case!
            m_objMatrix.SetElement(2, 0, 0);
            m_objMatrix.SetElement(2, 1, 0);
            m_objMatrix.SetElement(2, 2, 1);
        }

        /// <summary>
        /// Flips this transform horizontally.
        /// </summary>
        /// <remarks>
        /// The matrix representing this transform becomes:
        /// <code>
        /// [ a11   a12  a13 ]        [ -a11  a12  a13 ]
        /// [ a21   a22  a23 ]  ==>>  [ -a21  a22  a23 ]
        /// [  0     0    1  ]        [  0     0    1  ]
        /// </code>
        /// </remarks>
        public void FlipX()
        {
            A11 = -A11;
            A21 = -A21;

            m_objMatrix.SetElement(0, 0, A11);
            m_objMatrix.SetElement(1, 0, A21);
        }

        /// <summary>
        /// Flips this transform vertically.
        /// </summary>
        /// <remarks>
        /// The matrix representing this transform becomes:
        /// <code>
        /// [ a11   a12  a13 ]        [ a11  -a12  a13 ]
        /// [ a21   a22  a23 ]  ==>>  [ a21  -a22  a23 ]
        /// [  0     0    1  ]        [  0     0    1  ]
        /// </code>
        /// </remarks>
        public void FlipY()
        {
            A12 = -A12;
            A22 = -A22;

            m_objMatrix.SetElement(0, 1, A12);
            m_objMatrix.SetElement(1, 1, A22);
        }
		
        #endregion

        #region Public Static Methods
		
        /// <summary> 
        /// Creates and returns a new transform representing a translation transformation.
        /// </summary>
        /// <param name="translateX">The distance by which coordinates are translated in the
        /// X axis direction
        /// </param>
        /// <param name="translateY">The distance by which coordinates are translated in the
        /// Y axis direction
        /// </param>
        /// <returns> An <see cref="AffineTransform"/> object that represents a
        /// translation transformation, created with the specified vector.
        /// </returns>
        /// <remarks>
        /// The matrix representing the returned transform is:
        /// <code>
        /// [  1    0    translateX  ]
        /// [  0    1    translateY  ]
        /// [  0    0         1      ]
        /// </code>
        /// </remarks>
        public static AffineTransform CreateTranslation(double translateX, double translateY)
        {
            AffineTransform affine = AffineTransform.Identity;
            affine.ResetToTranslation(translateX, translateY);

            return affine;
        }
		
        /// <summary> 
        /// Creates and returns a new transform representing a rotation transformation.
        /// </summary>
        /// Rotating with a positive angle theta rotates points on the positive
        /// x axis toward the positive y axis.
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <returns> 
        /// An <see cref="AffineTransform"/> object that is a rotation
        /// transformation, created with the specified angle of rotation.
        /// </returns>
        /// <remarks>
        /// The matrix representing the returned transform is:
        /// <code>
        /// [ cos(angle)    -sin(angle)    0 ]
        /// [ sin(angle)     cos(angle)    0 ]
        /// [     0              0         1 ]
        /// </code>
        /// </remarks>
        public static AffineTransform CreateRotation(double angle)
        {
            AffineTransform affine = AffineTransform.Identity;

            affine.ResetToRotation(angle);
            
            return affine;
        }
		
        /// <summary> 
        /// Creates and returns a new transform that rotates coordinates 
        /// around an anchor point.
        /// </summary>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <param name="x">
        /// The x-coordinate of the anchor point of the rotation.
        /// </param>
        /// <param name="y">
        /// The y-coordinate of the anchor point of the rotation.
        /// </param>
        /// <returns> 
        /// An <see cref="AffineTransform"/> object that rotates coordinates 
        /// around the specified point by the specified angle of rotation.
        /// </returns>
        /// <remarks>
        /// This operation is equivalent to translating the coordinates so
        /// that the anchor point is at the origin (TX1), then rotating them
        /// about the new origin (TX2), and finally translating so that the
        /// intermediate origin is restored to the coordinates of the original
        /// anchor point (TX3).
        /// <para>
        /// This operation is equivalent to the following sequence of calls:
        /// </para>
        /// <code>
        /// AffineTransform affine = AffineTransform.Identity;
        /// affine.ResetToTranslation(x, y);	// TX3: final translation
        /// affine.Rotate(theta);		        // TX2: rotate around anchor
        /// affine.Translate(-x, -y);	        // TX1: translate anchor to origin
        /// </code>
        /// The matrix representing the returned transform is:
        /// <code>
        /// [  cos(angle)    -sin(angle)    x - x * cos + y * sin  ]
        /// [  sin(angle)     cos(angle)    y - x * sin - y * cos  ]
        /// [      0              0                     1          ]
        /// </code>
        /// Rotating with a positive angle theta rotates points on the positive
        /// x axis toward the positive y axis.
        /// </remarks>
        public static AffineTransform CreateRotationAt(double angle, double x, double y)
        {
            AffineTransform affine = AffineTransform.Identity;
            affine.ResetToRotation(angle, x, y);

            return affine;
        }
		
        /// <summary> 
        /// Creates and returns a new transform representing a scaling transformation.
        /// </summary>
        /// <param name="scaleX">
        /// The factor by which coordinates are scaled along the X axis direction.
        /// </param>
        /// <param name="scaleY">
        /// The factor by which coordinates are scaled along the Y axis direction.
        /// </param>
        /// <returns> 
        /// An <see cref="AffineTransform"/> object that scales 
        /// coordinates by the specified factors.
        /// </returns>
        /// <remarks>
        /// The matrix representing the returned transform is:
        /// <code>
        /// [  scaleX      0     0  ]
        /// [  0        scaleY   0  ]
        /// [  0           0     1  ]
        /// </code>
        /// </remarks>
        public static AffineTransform CreateScaling(double scaleX, double scaleY)
        {
            AffineTransform affine = AffineTransform.Identity;
            affine.ResetToScale(scaleX, scaleY);

            return affine;
        }
		
        /// <summary> 
        /// Creates and returns a new transform representing a shearing transformation.
        /// </summary>
        /// <param name="shearX">
        /// The multiplier by which coordinates are shifted in the direction of 
        /// the positive X axis as a factor of their Y coordinate
        /// </param>
        /// <param name="shearY">
        /// The multiplier by which coordinates are shifted in the direction of 
        /// the positive Y axis as a factor of their X coordinate
        /// </param>
        /// <returns> 
        /// An <see cref="AffineTransform"/> object that shears coordinates by 
        /// the specified multipliers.
        /// </returns>
        /// <remarks>
        /// The matrix representing the returned transform is:
        /// <code>
        /// [   1      shearX   0   ]
        /// [  shearY    1      0   ]
        /// [   0        0      1   ]
        /// </code>
        /// </remarks>
        public static AffineTransform CreateShearing(double shearX, double shearY)
        {
            AffineTransform affine = AffineTransform.Identity;
            affine.ResetToShear(shearX, shearY);

            return affine;
        }

        /// <summary>
        /// Creates and returns a new transform representing a skew transformation.
        /// </summary>
        /// <param name="skewX">The skew angle in the x dimension in degrees.</param>
        /// <remarks>
        /// The matrix representing the returned transform is:
        /// <code>
        /// [  1   tan(skewX)   0  ]
        /// [  0        1       0  ]
        /// [  0        0       1  ]
        /// </code>
        /// Thus the skew transformation is a special type of shearing.
        /// </remarks>
        public static AffineTransform CreateSkewX(double skewX)
        {
            AffineTransform affine = AffineTransform.Identity;

            affine.ShearX = Math.Tan(skewX * (Math.PI/180));

            return affine;
        }

        /// <summary>
        /// Creates and returns a new transform representing a skew transformation.
        /// </summary>
        /// <param name="skewY">The skew angle in the y dimension in degrees.</param>
        /// <remarks>
        /// The matrix representing the returned transform is:
        /// <code>
        /// [      1         0    0  ]
        /// [  tan(skewY)    1    0  ]
        /// [      0         0    1  ]
        /// </code>
        /// Thus the skew transformation is a special type of shearing
        /// </remarks>
        public static AffineTransform CreateSkewY(double skewY)
        {
            AffineTransform affine = AffineTransform.Identity;

            affine.ShearY = Math.Tan(skewY * (Math.PI/180));

            return affine;
        }

        /// <summary>
        /// Creates and returns a new transform representing a skew transformation.
        /// </summary>
        /// <param name="skewX">The skew angle in the x dimension in degrees.</param>
        /// <param name="skewY">The skew angle in the y dimension in degrees.</param>
        /// <remarks>
        /// The matrix representing the returned transform is:
        /// <code>
        /// [      1         tan(skewX)   0  ]
        /// [  tan(skewY)        1        0  ]
        /// [      0             0        1  ]
        /// </code>
        /// Thus the skew transformation is a special type of shearing
        /// </remarks>
        public static AffineTransform CreateSkew(double skewX, double skewY)
        {
            AffineTransform affine = AffineTransform.Identity;

            affine.ShearX = Math.Tan(skewX * (Math.PI/180));
            affine.ShearY = Math.Tan(skewY * (Math.PI/180));

            return affine;
        }

		
        #endregion
		
        #region Private Static Methods

        private static GeneralMatrix  Initialize(Coordinate p1, Coordinate q1, Coordinate p2, 
            Coordinate q2, Coordinate p3, Coordinate q3)
        {
            double[][] AArray = {new double[]{p1.X, p1.Y, 1,    0,    0, 0}, 
                                 new double[]{   0,    0, 0, p1.X, p1.Y, 1}, 
                                 new double[]{p2.X, p2.Y, 1,    0,    0, 0}, 
                                 new double[]{   0,   0,  0, p2.X, p2.Y, 1}, 
                                 new double[]{p3.X, p3.Y, 1,    0,    0, 0}, 
                                 new double[]{   0,   0,  0, p3.X, p3.Y, 1}
                                };

            GeneralMatrix A = new GeneralMatrix(AArray);
            
            double[][] BArray = {new double[]{q1.X}, 
                                  new double[]{q1.Y}, 
                                  new double[]{q2.X}, 
                                  new double[]{q2.Y}, 
                                  new double[]{q3.X}, 
                                  new double[]{q3.Y}
                                 };
            
            GeneralMatrix B = new GeneralMatrix(BArray);
            
            GeneralMatrix C = A.Solve(B);

            return CreateAffineMatrix(C.RowPackedCopy);
        }
 		
        /// <summary> 
        /// Determines where a point would end up if it were rotated 90 degrees about
        /// another point.
        /// </summary>
        /// <param name="a">The fixed point.</param>
        /// <param name="b">The point to rotate (b itself will not be changed).</param>
        /// <returns><c>b</c> rotated 90 degrees clockwise about <c>a</c></returns>
        private static Coordinate Rotate90(Coordinate a, Coordinate b)
        {
            return new Coordinate(b.Y - a.Y + a.X, a.X - b.X + a.Y);
        }

        private static GeneralMatrix CreateAffineMatrix(double a11, double a12, double a21, double a22, 
            double a13, double a23)
        {
            GeneralMatrix matrix = new GeneralMatrix(3, 3);

            // Initialize the first row
            matrix.SetElement(0, 0, a11);
            matrix.SetElement(0, 1, a12);
            matrix.SetElement(0, 2, a13);

            // Initialize the second row
            matrix.SetElement(1, 0, a21);
            matrix.SetElement(1, 1, a22);
            matrix.SetElement(1, 2, a23);
            
            // Initialize the third row
            matrix.SetElement(2, 0, 0);
            matrix.SetElement(2, 1, 0);
            matrix.SetElement(2, 2, 1);

            return matrix;
        }

        private static GeneralMatrix CreateAffineMatrix(double[] elements)
        {
            GeneralMatrix matrix = new GeneralMatrix(3, 3);

            // Initialize the first row
            matrix.SetElement(0, 0, elements[0]);
            matrix.SetElement(0, 1, elements[1]);
            matrix.SetElement(0, 2, elements[2]);

            // Initialize the second row
            matrix.SetElement(1, 0, elements[3]);
            matrix.SetElement(1, 1, elements[4]);
            matrix.SetElement(1, 2, elements[5]);
            
            // Initialize the third row
            matrix.SetElement(2, 0, 0);
            matrix.SetElement(2, 1, 0);
            matrix.SetElement(2, 2, 1);

            return matrix;
        }

        #endregion
		
        #region Public Overridable Base Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the value of this 
        ///  <see cref="System.Object"/>
        /// </summary>
        /// <returns> 
        /// A <see cref="System.String"/> representing the value of this
        /// <see cref="System.Object"/>.
        /// </returns>
        /// <remarks>
        /// The string is formatted in the form:
        /// <code>
        /// AFFINETRANSFORM[[a11, a12, a13], [a21, a22, a23]]
        /// </code>
        /// </remarks>
        public override System.String ToString()
        {
            return ("AFFINETRANSFORM[[" + A11 + ", " + A12 + ", " + A13 + "], [" + A21 + ", " + A22 + ", " + A23 + "]]");
        }
		
        /// <summary> Returns the hashcode for this transform.</summary>
        /// <returns> A hash code for this transform. </returns>
        public override int GetHashCode()
        {
            long lBits = BitConverter.DoubleToInt64Bits(A11);

            lBits = lBits * 31 + BitConverter.DoubleToInt64Bits(A12);
            lBits = lBits * 31 + BitConverter.DoubleToInt64Bits(A13);
            lBits = lBits * 31 + BitConverter.DoubleToInt64Bits(A21);
            lBits = lBits * 31 + BitConverter.DoubleToInt64Bits(A22);
            lBits = lBits * 31 + BitConverter.DoubleToInt64Bits(A23);

            return (((int) lBits) ^ ((int) (lBits >> 32)));
        }
		
        /// <summary> 
        /// Determines whether this <see cref="AffineTransform"/> and the specified
        /// object represent the same affine coordinate transform.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="System.Object"/> to test for equality with this
        /// <see cref="AffineTransform"/> instance.
        /// </param>
        /// <returns> 
        /// true if the specified argument equals this <see cref="AffineTransform"/> object, 
        /// false otherwise.
        /// </returns>
        public override bool Equals(System.Object obj)
        {
            if (!(obj is AffineTransform))
            {
                return false;
            }
			
            AffineTransform affine = (AffineTransform) obj;
			
            return ((A11 == affine.A11) && (A12 == affine.A12) && (A13 == affine.A13) && 
                (A21 == affine.A21) && (A22 == affine.A22) && (A23 == affine.A23));
        }
		
        /// <summary> 
        /// Determines whether this <see cref="AffineTransform"/> and the specified
        /// object represent the same affine coordinate transform.
        /// </summary>
        /// <param name="obj">
        /// The transform to test for equality with this 
        /// <see cref="AffineTransform"/> instance.
        /// </param>
        /// <returns> 
        /// true if the specified argument equals this <see cref="AffineTransform"/> object, 
        /// false otherwise.
        /// </returns>
        public bool Equals(AffineTransform obj)
        {
            return ((A11 == obj.A11) && (A12 == obj.A12) && (A13 == obj.A13) && 
                (A21 == obj.A21) && (A22 == obj.A22) && (A23 == obj.A23));
        }

        #endregion

        #region Public Operator Overloading
        
        /// <summary>
        /// Compares two transforms for exact equality.
        /// </summary>
        /// <param name="transform1">The first transform to compare.</param>
        /// <param name="transform2">The second transform to compare.</param>
        /// <returns>
        /// true if both transforms are equal, false otherwise.
        /// </returns>
        public static bool operator ==(AffineTransform transform1, AffineTransform transform2)
        {
            return transform1.Equals(transform2);
        }
        
        /// <summary>
        /// Compares two transforms for exact inequality.
        /// </summary>
        /// <param name="transform1">The first transform to compare.</param>
        /// <param name="transform2">The second transform to compare.</param>
        /// <returns>
        /// true if both transforms are not equal, false otherwise.
        /// </returns>
        public static bool operator !=(AffineTransform transform1, AffineTransform transform2)
        {
            return !transform1.Equals(transform2);
        }
        
        /// <summary>
        /// Multiplies two transformations.
        /// </summary>
        /// <param name="transform1">The first transform to compare.</param>
        /// <param name="transform2">The second transform to compare.</param>
        /// <returns>
        /// A new <see cref="AffineTransform"/> transform, which is multiplication 
        /// of the specified transform parameters.
        /// </returns>
        public static AffineTransform operator *(AffineTransform transform1, AffineTransform transform2)
        {
            GeneralMatrix matrix = transform1.m_objMatrix.Multiply(transform2.m_objMatrix);

            return new AffineTransform(matrix);
        }

        #endregion

        #region ICoordinateVisitor Members

        /// <summary>
        /// Applies this affine transform filter to the specified coordinate.
        /// </summary>
        /// <param name="coord">The ccordinate to filter.</param>
        /// <remarks>
        /// This affine transformation is a 2-dimensional transform, so Z-values
        /// of 3-dimensional coordinates will not be affected.
        /// </remarks>
        public void Visit(Coordinate coord)
        {
            double x = (A11 * coord.X) + (A12 * coord.Y) + A13;
            double y = (A21 * coord.X) + (A22 * coord.Y) + A23;
			
            coord.X = x;
            coord.Y = y;
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Creates an exact copy of this <see cref="AffineTransform"/> object.
        /// </summary>
        /// <returns>
        /// The <see cref="AffineTransform"/> this method creates, cast as an object.
        /// </returns>
        public object Clone()
        {
            return new AffineTransform(this);
        }

        #endregion
    }
}
