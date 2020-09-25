using iGeospatial.Geometries;
using OSGeo.OGR;
using System.Collections.Generic;
using OGR = OSGeo.OGR;

namespace GdalUtilsOz.Utils.ShiftGeosOgr
{
        class FromGeosToOgr
        {
                /**
                 * 将 geos 对象转储到 ogr 的 datasource 中
                 * 
                 * geometryList 是 geos 内的一种类型，需要转换为 ogr 类型才能存储到 ds 中
                 * ds 是 ogr 的一种类型
                 */
                public static void SaveGeoGeometryListToOgrDS(GeometryList geometryList, OGR.DataSource ds, int layerIndex = 0)
                {
                        OGR.Layer layer = ds.GetLayerByIndex(layerIndex);
                        for (int i = 0; i < geometryList.Count; i++)
                        {
                                OGR.Feature feature = new OGR.Feature(layer.GetLayerDefn());
                                OSGeo.OGR.Geometry geometry = OSGeo.OGR.Geometry.CreateFromWkt(geometryList[i].ToString());
                                feature.SetGeometry(geometry);
                                layer.CreateFeature(feature);
                        }
                }
        }
}
