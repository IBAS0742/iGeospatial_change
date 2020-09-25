using System;
using System.Collections.Generic;
using OGR = OSGeo.OGR;
using OSGeo.GDAL;
using iGeospatial.Geometries;
using GdalUtils.Utils;

namespace GdalUtils.Tools {
        class SelectSomeFromAnotherHelp {
                static public void SelectSomeFromAnother(string[] args)
                {
                        if (args.Length == 3)
                        {
                                ToSelectSomeFromAnother(args[1],args[2]);
                        }
                        else
                        {
                                Console.WriteLine("SelectSomeFromAnother inPath outPath");
                        }
                }

                static public void ToSelectSomeFromAnother(string shpPath, string oShpPath)
                {
                        OGR.DataSource ds = SelectFeatureHelp.OpenFromShpFile(shpPath,1);
                        OGR.Layer layer = ds.GetLayerByIndex(0);
                        layer.DeleteFeature(0);
                        // var map = new Dictionary<string, OGR.wkbGeometryType>();
                        // map.Add("lay",OGR.wkbGeometryType.wkbLineString);
                        // CreateShpFile.ToCreateShpFile(oShpPath, Prj.Prjection.EPSG4326,map);
                        layer.Dispose();
                        ds.Dispose();
                }
        }
}
