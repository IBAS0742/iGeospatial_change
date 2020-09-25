using System;
using OSGeo.GDAL;
using OSGeo.OGR;

namespace TestGdalCoreRuntime {
        class Program {
                static void Main(string[] args) {
                        Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "Yes");
                        GdalConfiguration.ConfigureGdal();
                        GdalConfiguration.ConfigureOgr();
                        OSGeo.OGR.Ogr.RegisterAll();
                        OSGeo.GDAL.Gdal.AllRegister();
                        // SUBDATASET_1_NAME=HDF4_SDS:UNKNOWN:"f:\TERRA_2010_03_25_03_09_GZ.MOD021KM.hdf":0
                        string path = @"HDF4_SDS:sur_refl_b01:""S:\2020_download_data\fire\MOD09\MOD09A1.A2019193.h27v08.006.2019202033135.hdf"":0";
                        Dataset ds = Gdal.Open(path,Access.GA_ReadOnly);
                        //Gdal.Open
                }
        }
}
