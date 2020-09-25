using System;

namespace iGeospatial.Coordinates.Transforms
{
	/// <summary>
	/// Summary description for TransformUtil.
	/// </summary>
	public class TransformUtil
	{
        #region Constructors and Destructor
		
        private TransformUtil()
		{
		}

        #endregion

        #region Translation
	    
        // Translate <point> by <by>
	    public static void Translate(Coordinate point, Coordinate by)
	    {
            double x = point.X;
            double y = point.Y;

		    point.X = x + by.X;
            point.Y = y + by.Y;
	    }

	    // Translate the point <point> by (<dx>, <dy>)
	    public static void Translate(Coordinate point, double dx, double dy)
	    {
            double x = point.X;
            double y = point.Y;

            point.X = x + dx;
            point.Y = y + dy;
	    }

	    // Translate the point (<x>, <y>) by (<dx>, <dy>)
	    public static void Translate(ref double x, ref double y, double dx, double dy)
	    {
		    x += dx;
		    y += dy;
	    }

	    public static void Translate(Coordinate3D point, Coordinate3D by)
	    {
            double x = point.X;
            double y = point.Y;
            double z = point.Z;

            point.X = x + by.X;
            point.Y = y + by.Y;
            point.Z = z + by.Z;
	    }

	    public static void Translate(ref double x, ref double y, ref double z,
		    double dx, double dy, double dz)
	    {
		    x += dx;
		    y += dy;
		    z += dz;
	    }
        
        #endregion

        #region Rotation
	    
        // Rotate <point> <theta> degrees about the point <about>
	    public static void Rotate(Coordinate point, double theta, Coordinate about)
	    {
		    // Translate <point> so that <about> is the origin
		    Translate(point, -about);

		    // Rotate translated <point> by <theta>
		    double sin_theta = Math.Sin(theta * Math.PI/180);
		    double cos_theta = Math.Cos(theta * Math.PI/180);

		    double tempX = point.X * cos_theta - point.Y * sin_theta;
		    double tempY = point.Y * cos_theta + point.X * sin_theta;
		    
            point.SetCoordinate(tempX, tempY);

		    // Translate the new <point> back by <about> to reset the origin position
		    Translate(point, about);
	    }

	    // Rotate <point> <theta> degrees about the point <about>
	    public static void Rotate(Coordinate point, double theta, 
            double xCenter, double yCenter)
	    {
		    // Translate <point> so that <about> is the origin
		    Translate(point, -xCenter, -yCenter);

		    // Rotate translated <point> by <theta>
		    double sin_theta = Math.Sin(theta * Math.PI/180);
		    double cos_theta = Math.Cos(theta * Math.PI/180);

		    double tempX = point.X * cos_theta - point.Y * sin_theta;
		    double tempY = point.Y * cos_theta + point.X * sin_theta;
		    
            point.SetCoordinate(tempX, tempY);

		    // Translate the new <point> back by <about> to reset the origin position
		    Translate(point, xCenter, yCenter);
	    }

	    // Rotate the point (<x>, <y>) by <theta> about the point (<ax>, <ay>)
	    public static void Rotate(ref double x, ref double y, double theta, 
            double ax, double ay)
	    {
            // Translate <point> so that <about> is the origin
            Translate(ref x, ref y, -ax, -ay);

            // Rotate translated <point> by <theta>
            double sin_theta = Math.Sin(theta * Math.PI/180);
            double cos_theta = Math.Cos(theta * Math.PI/180);

            double tempX = x * cos_theta - y * sin_theta;
            double tempY = y * cos_theta + x * sin_theta;

            x = tempX;
            y = tempY;

            // Translate the new <point> back by <about> to reset the origin position
            Translate(ref x, ref y, ax, ay);
        }

	    // Rotate <point> by <theta> about the X-axis relative to <about>
	    public static void RotateX(Coordinate3D point, double theta, 
            Coordinate3D about)
	    {
		    // Translate <point> so that <about> is origin
		    Translate(point, -about);

		    // Rotate Translate <point> by <theta> about the x-axis
		    double sin_theta = Math.Sin(theta * Math.PI/180); 
		    double cos_theta = Math.Cos(theta * Math.PI/180);

            double tempX = point.X; 
            double tempY = point.Y; 
            double tempZ = point.Z;

		    point.Y = tempY * cos_theta - tempZ * sin_theta;
		    point.Z = tempZ * cos_theta + tempY * sin_theta;

		    // Translate the new <point> back by <about> to reset the origin position
		    Translate(point, about);
	    }

	    // Rotate <point> by <theta> about the X-axis relative to <about>
	    public static void RotateX(ref double x, ref double y, ref double z, 
            double theta, double ax, double ay, double az)
	    {
		    Coordinate3D point = new Coordinate3D(x, y, z);
            Coordinate3D about = new Coordinate3D(ax, ay, az);

		    RotateX(point, theta, about);

            x = point.X; 
            y = point.Y; 
            z = point.Z;
	    }

	    // Rotate <point> by <theta> about the Y-axis relative to <about>
	    public static void RotateY(Coordinate3D point, 
            double theta, Coordinate3D about)
	    {
		    // Translate <point> so that <about> is origin
		    Translate(point, -about);

		    // Rotate Translate <point> by <theta> about the y-axis
		    double sin_theta = Math.Sin(theta * Math.PI/180); 
		    double cos_theta = Math.Cos(theta * Math.PI/180);

            double tempX = point.X; 
            double tempY = point.Y; 
            double tempZ = point.Z;
		    
            point.X = tempX * cos_theta + tempZ * sin_theta;
		    point.Z = tempZ * cos_theta - tempX * sin_theta;

		    // Translate the new <point> back by <about> to reset the origin position
		    Translate(point, about);
	    }

	    // Rotate <point> by <theta> about the Y-axis relative to <about>
	    public static void RotateY(ref double x, ref double y, ref double z, 
            double theta, double ax, double ay, double az )
	    {
            Coordinate3D point = new Coordinate3D(x, y, z);
            Coordinate3D about = new Coordinate3D(ax, ay, az);

		    RotateY(point, theta, about);

            x = point.X; 
            y = point.Y; 
            z = point.Z;
        }

	    // Rotate <point> by <theta> about the Z-axis relative to <about>
	    public static void RotateZ(Coordinate3D point, 
            double theta, Coordinate3D about)
	    {
		    // Translate <point> so that <about> is origin
		    Translate(point, -about);

		    // Rotate Translate <point> by <theta> about the z-axis
		    double sin_theta = Math.Sin(theta * Math.PI/180); 
		    double cos_theta = Math.Cos(theta * Math.PI/180);

            double tempX = point.X; 
            double tempY = point.Y; 
            double tempZ = point.Z;
		    
            point.X = tempX * cos_theta - tempY * sin_theta;
		    point.Y = tempY * cos_theta + tempX * sin_theta;

		    // Translate the new <point> back by <about> to reset the origin position
		    Translate(point, about);
	    }

	    // Rotate <point> by <theta> about the Z-axis relative to <about>
	    public static void RotateZ(ref double x, ref double y, ref double z, 
            double theta, double ax, double ay, double az)
	    {
            Coordinate3D point = new Coordinate3D(x, y, z);
            Coordinate3D about = new Coordinate3D(ax, ay, az);

		    RotateZ(point, theta, about);
		    
            x = point.X; 
            y = point.Y; 
            z = point.Z;
        }
        
        #endregion

        #region Scaling
	    
        // Scale <point> by [ <scaleX> <scaleY> ] with respect to <about>
	    public static void Scale(Coordinate point, double scaleX, double scaleY)
	    {
		    // Scale <point> by Scale factors
		    point.X = point.X * scaleX;
		    point.Y = point.Y * scaleY;
	    }

	    // Scale <point> by [ <scaleX> <scaleY> ] with respect to <about>
	    public static void Scale(Coordinate point, 
            double scaleX, double scaleY, Coordinate about)
	    {
		    // Translate <point> so that <about> is the origin
		    Translate(point, -about);

		    // Scale <point> by Scale factors
		    point.X = point.X * scaleX;
		    point.Y = point.Y * scaleY;

		    // Translate the new <point> back by <about> to reset the origin position
		    Translate(point, about);
	    }

	    // Scale the point (<x>, <y>) by [ <scaleX> <scaleY> ] with respect to the point (<ax>, <ay>)
	    public static void Scale(ref double x, ref double y, 
            double scaleX, double scaleY, double ax, double ay)
	    {
            Translate(ref x, ref y, -ax, -ay);

            // Scale <point> by Scale factors
            x = x * scaleX;
            y = y * scaleY;

            // Translate the new <point> back by <about> to reset the origin position
            Translate(ref x, ref y, ax, ay);
        }

	    // Scale <point> by [ <scaleX> <scaleY> <scaleZ> ] wrt <about>
	    public static void Scale(Coordinate3D point, 
            double scaleX, double scaleY, double scaleZ, Coordinate3D about )
	    {
		    // Translate <point> so that <about> is the origin
		    Translate(point, -about);

		    // Scale <point> by Scale factors
		    point.X = point.X * scaleX;
		    point.Y = point.Y * scaleY;
		    point.Z = point.Z * scaleZ;

		    // Translate the new <point> back by <about> to reset the origin position
		    Translate(point, about);
	    }

	    // Scale the point (<x>, <y>, <z> ) by [ <sx> <sy> <sz> ] 
	    // with respect to the point (<ax>, <ay>, <az> )
	    public static void Scale(ref double x, ref double y, ref double z, 
		    double sx, double sy, double sz,
		    double ax, double ay, double az)
	    {
            Coordinate3D point = new Coordinate3D(x, y, z);
            Coordinate3D about = new Coordinate3D(ax, ay, az);

            Scale(point, sx, sy, sz, about);
		    
            x = point.X; 
            y = point.Y; 
            z = point.Z;
        }
        
        #endregion

        #region Reflection
	    
        // Reflect <point> about the x-axis
	    public static void ReflectXAxis(Coordinate point)
	    {
		    // Negate the y-component of <point>
		    point.Y = -point.Y;
	    }

	    // Reflect the point (<x>, <y>) about the x-axis
	    public static void ReflectXAxis( ref double x, ref double y )
	    {
		    y = -y;
	    }

	    // Reflect <point> about the y-axis
	    static void ReflectYAxis(Coordinate point)
	    {
		    // Negate the x-component of <point>
		    point.X = -point.X;
	    }

	    // Reflect the point (<x>, <y>) about the y-axis
	    public static void ReflectYAxis(ref double x, ref double y)
	    {
		    x = -x;
	    }

	    // Reflect <point> about the origin
	    static void ReflectOrigin(Coordinate point)
	    {
		    // Negate both of the points coordinates
		    point = -point;
	    }

	    // Reflect the point (<x>, <y>) about the origin
	    public static void ReflectOrigin(ref double x, ref double y)
	    {
		    x = -x;
		    y = -y;
	    }

	    // Reflect <point> about <about>
	    public static void ReflectPoint(Coordinate point, Coordinate about)
	    {
		    // Translate <point> so that <about> is the origin
		    Translate(point, -about);

		    // Reflect <point> as though <about> was the origin
		    ReflectOrigin(point);

		    // Translate the new <point> back by <about> to reset the origin position
		    Translate(point, about);
	    }

	    // Reflect the point (<x>, <y>) about the point (<ax>, <ay>)
	    public static void ReflectPoint(ref double x, ref double y, 
            double ax, double ay)
	    {
            // Translate <point> so that <about> is the origin
            Translate(ref x, ref y, -ax, -ay);

            // Reflect <point> as though <about> was the origin
            ReflectOrigin(ref x, ref y);

            // Translate the new <point> back by <about> to reset the origin position
            Translate(ref x, ref y, ax, ay);
        }

	    // Reflect <point> in the XY plane
	    public static void ReflectXY(Coordinate3D point)
	    {
		    point.Z = -point.Z;
	    }

	    // Reflect the point ( <x>, <y>, <z> ) in the XY plane
	    public static void ReflectXY(ref double x, ref double y, ref double z)
	    {
		    z = -z;
	    }

	    // Reflect <point> in the XZ plane
	    public static void ReflectXZ(Coordinate3D point)
	    {
		    point.Y = -point.Y;
	    }

	    // Reflect the point ( <x>, <y>, <z> ) in the XZ plane
	    public static void ReflectXZ(ref double x, ref double y, ref double z)
	    {
		    y = -y;
	    }

	    // Reflect <point> in the YZ plane
	    public static void ReflectYZ(Coordinate3D point)
	    {
		    point.X = -point.X;
	    }

	    // Reflect the point ( <x>, <y>, <z> ) in the YZ plane
	    public static void ReflectYZ(ref double x, ref double y, ref double z)
	    {
		    x = -x;
	    }
        
        #endregion

	}
}
