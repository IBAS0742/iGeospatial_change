using System;

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Editors
{
	/// <summary>
	/// Summary description for CoordinateOperation.
	/// </summary>
    public class CoordinateOperation : CoordinateGeometryEdit
    {
        public CoordinateOperation()
        {
        }

        public override ICoordinateList Edit(
            ICoordinateList coordinates, Geometry geometry)
        {
            return coordinates;
        }
    }
}
