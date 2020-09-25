using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDotNet;
using REngineAddon.Extension;

namespace REngineAddon.SailMethod
{
        /**
         * 依赖：
         *      rgdal、ncdf4
         */
        enum DriverName {
                netcdf = 1,
                gdal = 2,
        }
        public class GeoInfo
        {
                protected bool Ok = false;
                protected string driver = "";
                protected string file, name;
                protected string crs = "";
                protected double bands = 0;
                protected REngine eng;
                protected List<string> filenameEndWith = new List<string>() { 
                        ".dat",".nc4",".tif",
                };
                public GeoInfo(REngine _eng, string _file, string _name = "r")
                {
                        filenameEndWith.ForEach(s => {
                                if (_file.EndsWith(s))
                                {
                                        Ok = true;
                                }
                        });
                        if (!Ok)
                        {
                                return;
                        }
                        eng = _eng;
                        file = _file;
                        name = _name;
                        eng.Evaluate(name + " = raster('" + file + "')");
                        Driver();
                        Crs();
                        Extent();
                        Bands();
                }
                Dictionary<string, int> extent = null;
                public Dictionary<string, int> Extent()
                {
                        if (!Ok)
                        {
                                return null;
                        }
                        if (extent == null)
                        {
                                extent = new Dictionary<string, int>();
                                string key;
                                key = "xmax"; extent[key] = eng.GetInt(name + "@extent@" + key);
                                key = "xmin"; extent[key] = eng.GetInt(name + "@extent@" + key);
                                key = "ymax"; extent[key] = eng.GetInt(name + "@extent@" + key);
                                key = "ymin"; extent[key] = eng.GetInt(name + "@extent@" + key);
                        }
                        return extent;
                }
                public string Driver()
                {
                        if (!Ok) {
                                return null;
                        }
                        if (string.IsNullOrEmpty(driver))
                        {
                                driver = eng.GetString(name + "@file@driver");
                        }
                        return driver;
                }
                public string Crs() {
                        if (!Ok)
                        {
                                return null;
                        }
                        if (string.IsNullOrEmpty(crs))
                        {
                                crs = eng.GetString(name + "@crs@projargs");
                        }
                        return crs;
                }
                public double Bands()
                {
                        if (!Ok)
                        {
                                return 0;
                        }
                        if (bands == 0)
                        {
                               bands = eng.GetReal(name + "@file@nbands");
                        }
                        return bands;
                }
                public override string ToString()
                {
                        if (!Ok)
                        {
                                return "file is not support";
                        }
                        StringBuilder sb = new StringBuilder();
                        sb.Append("driver = " + driver + "\r\n");
                        sb.Append("crs = " + crs + "\r\n");
                        sb.Append("bands = " + bands + "\r\n");
                        sb.Append("extent = \r\n");
                        extent.Keys.ToList().ForEach(key => {
                                sb.Append("\t" + key + " : " + extent[key] + "\r\n");
                        });
                        return sb.ToString();
                }
        }
}
