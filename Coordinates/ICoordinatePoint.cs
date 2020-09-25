using System;

namespace iGeospatial.Coordinates
{
	/// <summary>
	/// Summary description for ICoordinatePoint.
	/// </summary>
    public interface ICoordinatePoint : ICloneable
    {
        double[] Ordinate {get;}

        int Dimension{get;}

        double GetOrdinate(int dimension);
        void SetOrdinate(int Dimension, double value);
        
        void SetLocation(ICoordinatePoint point);
    }

}
