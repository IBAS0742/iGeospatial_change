using OGR = OSGeo.OGR;
using OSGeo.GDAL;
using iGeospatial.Geometries;
using GdalUtils.Utils;

namespace GdalUtils.Tools
{
        public enum FeatureType
        {
                Point = 1,
                LineString = 2,
                Polygon = 3
        }
        class SelectFeature
        {
        }
        class SelectFeatureHelp
        {
                public static OGR.DataSource OpenFromShpFile(string path,int update = 0) {
                        GdalConfiguration.ConfigureGdal();
                        GdalConfiguration.ConfigureOgr();
                        OGR.Ogr.RegisterAll();
                        Gdal.AllRegister();
                        OGR.DataSource ds = OGR.Ogr.Open(path, update);
                        return ds;
                }
                public static GeometryList OgrDSToGeometryList(OGR.DataSource ds,int type) {
                        GeometryList list = new GeometryList();
                        int lcount = ds.GetLayerCount();
                        for (int i = 0;i < lcount;i++)
                        {
                                OGR.Layer lay = ds.GetLayerByIndex(i);
                                long fcount = lay.GetFeatureCount(0);
                                for (long j = 0;j < fcount;j++)
                                {
                                        if (type == (int)FeatureType.Point) {
                                                Transform.OgrFeatureToGeosPoint(lay.GetFeature(j)).ForEach(p => {
                                                        list.Add(p);
                                                });
                                        }else if (type == (int)FeatureType.LineString)
                                        {
                                                list.Add(Transform.OgrFeatureToGeosLineString(lay.GetFeature(j)));
                                        }else if (type == (int)FeatureType.Polygon)
                                        {
                                                list.Add(Transform.OgrFeatureToGeosPolygon(lay.GetFeature(j)));
                                        }
                                }
                        }
                        return list;
                }

                /**
                 * 从序列化文件中读出内容
                 */
                public static GeometryList OpenFromSer(string path) {
                        GeometryList list = (GeometryList)Utils.SerializeObject.FromSerialize(path);
                        return list;
                }
        }
}
