using RDotNet;
using REngineAddon.Extension;
using System;

namespace REngineAddon.SailMethod
{
        public class GetInfoNc4 : GeoInfo
        {
                bool read = false;
                string nc4Name = "_nc4";
                public GetInfoNc4(REngine _eng, string _file, string _name = "r") : 
                        base(_eng, _file, _name)
                {
                        if (driver == "netcdf")
                        {
                                NcBands();
                        }
                }
                public void ReadNc4()
                {
                        if (read)
                        {
                                return;
                        }
                        read = true;
                        eng.Evaluate(name + nc4Name + " = ncdf4::nc_open('" + file + "');");
                }
                public double NcBands()
                {
                        if (!Ok)
                        {
                                return 0;
                        }
                        ReadNc4();
                        bands = eng.GetReal(name + nc4Name + "$nvars");
                        return bands;
                }
        }
}
