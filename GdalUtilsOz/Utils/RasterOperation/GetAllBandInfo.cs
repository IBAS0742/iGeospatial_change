using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDAL = OSGeo.GDAL;

namespace GdalUtilsOz.Utils.RasterOperation
{
        class CopyRasterBandInfo
        {
                // tif 文件名
                public string Name { get; set; } = "";
                // 要读取的波段
                public List<int> Bands = new List<int>();
                public CopyRasterBandInfo(string name, List<int> bands)
                {
                        Name = name;
                        Bands = bands;
                }
        }
        class NoData
        {
                public int Has;
                public double Nodata;
                public bool Ok;
                public string ToString()
                {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Has = " + Has + "\r\n");
                        sb.Append("Nodata = " + Nodata + "\r\n");
                        sb.Append("Ok = " + Ok + "\r\n");

                        return sb.ToString();
                }
        }
        class GetAllBandInfo
        {
                double[] transform = new double[6];
                public double[] Transform { get { return transform; } }
                string projection;
                public string Projection { get { return projection; } }
                delegate void CopyRaster(
                               GDAL.Dataset srcDs,
                               GDAL.Dataset distDs,
                               int srcBandInd,
                               int distBandInd);
                CopyRaster copyRaster;
                public NoData NoData { get; set; } = null;
                public int Xsize, Ysize;
                public delegate void ForEachDS(GDAL.Dataset ds);
                GDAL.DataType type;
                public GDAL.DataType Type {
                        get {
                                return type;
                        }
                }
                List<CopyRasterBandInfo> tifs = new List<CopyRasterBandInfo>();
                public bool TypeIsSame { get; set; } = true;
                Dictionary<string, GDAL.Dataset> dts = new Dictionary<string, GDAL.Dataset>();
                public GetAllBandInfo(List<CopyRasterBandInfo> _tifs = null) {
                        if (_tifs != null)
                        {
                                _tifs.ForEach(tif => {
                                        // 这里是检查文件类型（double、float、int、byte）
                                        CheckType(tif.Name, tifs.Count == 0);
                                        tifs.Add(tif);
                                });
                        }
                }
                public bool AddTif(CopyRasterBandInfo crbi) {
                        CheckType(crbi.Name, tifs.Count == 0);
                        tifs.Add(crbi);
                        return TypeIsSame;
                }
                public GDAL.Dataset GetDataSet(CopyRasterBandInfo name) {
                        if (dts.ContainsKey(name.Name))
                        {
                                return dts[name.Name];
                        } else {
                                return null;
                        }
                }
                public GDAL.Dataset GetDataSet(string name) {
                        if (dts.ContainsKey(name))
                        {
                                return dts[name];
                        } else {
                                return null;
                        }
                }
                public bool CheckType(string name,bool first) {
                        if (dts.ContainsKey(name))
                        {
                                return TypeIsSame;
                        }
                        // 每个文件将被打开
                        GDAL.Dataset ds = GDAL.Gdal.Open(name, GDAL.Access.GA_ReadOnly);
                        GDAL.Band b1 = ds.GetRasterBand(1);
                        dts.Add(name, ds);
                        if (first)
                        {
                                // 直接从第一个文件获取投影和数据类型
                                type = b1.DataType;
                                ds.GetGeoTransform(transform);
                                projection = ds.GetProjection();
                        } else if (type != b1.DataType)
                        {
                                // 往后类型不一致都返回false（警告作用，但是不报错，万一有人偏要呢？）
                                TypeIsSame = false;
                        }
                        Xsize = b1.XSize > Xsize ? b1.XSize : Xsize;
                        Ysize = b1.YSize > Ysize ? b1.YSize : Ysize;
                        return TypeIsSame;
                }
                public void ForEach(ForEachDS fn) {
                        tifs.ForEach(tif => {
                                fn(dts[tif.Name]);
                        });
                }
                public void Dispose()
                {
                        // 统一将打开的文件释放
                        foreach (KeyValuePair<string, GDAL.Dataset> kvp in dts) {
                                kvp.Value.Dispose();
                        }
                }
                private NoData CheckCopyMethod() {
                        if (NoData != null)
                        {
                                return NoData;
                        }
                        NoData = new NoData();
                        NoData.Ok = false;
                        if (tifs.Count > 0)
                        {
                                double nodata;
                                int has;
                                GDAL.Band b = dts[tifs[0].Name].GetRasterBand(1);
                                b.GetNoDataValue(out nodata, out has);
                                switch (b.DataType)
                                {
                                        case GDAL.DataType.GDT_Byte:
                                                copyRaster = CopyRasterBandToBand.CopyByte;
                                                break;
                                        case GDAL.DataType.GDT_CFloat32:
                                        case GDAL.DataType.GDT_Float32:
                                                copyRaster = CopyRasterBandToBand.CopyFloat;
                                                break;
                                        case GDAL.DataType.GDT_CFloat64:
                                        case GDAL.DataType.GDT_Float64:
                                                copyRaster = CopyRasterBandToBand.CopyDouble;
                                                break;
                                        case GDAL.DataType.GDT_CInt16:
                                        case GDAL.DataType.GDT_Int16:
                                        case GDAL.DataType.GDT_UInt16:
                                                copyRaster = CopyRasterBandToBand.CopyInt16;
                                                break;
                                        case GDAL.DataType.GDT_CInt32:
                                        case GDAL.DataType.GDT_Int32:
                                        case GDAL.DataType.GDT_UInt32:
                                                copyRaster = CopyRasterBandToBand.CopyInt32;
                                                break;
                                        default:
                                                throw new Exception("[GetAllBandInfo.CheckCopyMethod] can not get type");
                                }
                                NoData.Ok = true;
                                NoData.Has = has;
                                NoData.Nodata = nodata;
                                return NoData;
                        }
                        return NoData;
                }
                public void CopyTo(GDAL.Dataset targetDs,int totalBandCount,int from = 1) {
                        // 先获取一个nodata
                        NoData nd = CheckCopyMethod();
                        if (nd.Ok)
                        {
                                if (nd.Has == 1)
                                {
                                        for (int i = 1; i <= totalBandCount; i++)
                                        {
                                                // 统一每个图层的nodata
                                                targetDs.GetRasterBand(i).SetNoDataValue(nd.Nodata);
                                        }
                                }
                                tifs.ForEach(info => {
                                        info.Bands.ForEach(band => {
                                                // 将波段复制进来
                                                copyRaster(dts[info.Name], targetDs, band, from);
                                                from++;
                                        });
                                });
                        }
                }
                public void CopyTo(GDAL.Dataset src,GDAL.Dataset dist,int from,int to)
                {
                        NoData nd = CheckCopyMethod();
                        if (nd.Ok)
                        {
                                if (nd.Has == 1)
                                {
                                        for (int i = 1; i <= to; i++)
                                        {
                                                dist.GetRasterBand(i).SetNoDataValue(nd.Nodata);
                                        }
                                }
                                for (;from <= to;from++)
                                {
                                        copyRaster(src, dist, from, from);
                                }
                        }
                }
                public GDAL.Dataset GetOneDataset()
                {
                        if (tifs.Count > 0)
                        {
                                return dts[tifs[0].Name];
                        }
                        else
                        {
                                return null;
                        }
                }
        }

        static class GetAllBandInfoalc
        {
                static public void Calc(this GDAL.Dataset ds, List<string> exp,double nodata) {
                        // 可以开始计算了
                        GDAL.Band b = ds.GetRasterBand(1);
                        switch (b.DataType)
                        {
                                case GDAL.DataType.GDT_Byte:
                                        Utils.RasterOperation.RasterBandOperation.SingleDoubleFile.OpByte opByte =
                                                new Utils.RasterOperation.RasterBandOperation.SingleDoubleFile.OpByte(ds,nodata);
                                        for (int i = 0; i < exp.Count; i++)
                                        {
                                                opByte.ToCalc(exp[i]);
                                        }
                                        break;
                                case GDAL.DataType.GDT_CFloat32:
                                case GDAL.DataType.GDT_Float32:
                                        Utils.RasterOperation.RasterBandOperation.SingleDoubleFile.OpFloat opFloat =
                                                new Utils.RasterOperation.RasterBandOperation.SingleDoubleFile.OpFloat(ds, nodata);
                                        for (int i = 0; i < exp.Count; i++)
                                        {
                                                opFloat.ToCalc(exp[i]);
                                        }
                                        break;
                                case GDAL.DataType.GDT_CFloat64:
                                case GDAL.DataType.GDT_Float64:
                                        Utils.RasterOperation.RasterBandOperation.SingleDoubleFile.OpDouble opDouble=
                                                new Utils.RasterOperation.RasterBandOperation.SingleDoubleFile.OpDouble(ds, nodata);
                                        for (int i = 0; i < exp.Count; i++)
                                        {
                                                opDouble.ToCalc(exp[i]);
                                        }
                                        break;
                                case GDAL.DataType.GDT_CInt16:
                                case GDAL.DataType.GDT_Int16:
                                case GDAL.DataType.GDT_UInt16:
                                        Utils.RasterOperation.RasterBandOperation.SingleDoubleFile.OpInt16 opInt16 =
                                                new Utils.RasterOperation.RasterBandOperation.SingleDoubleFile.OpInt16(ds, nodata);
                                        for (int i = 0; i < exp.Count; i++)
                                        {
                                                opInt16.ToCalc(exp[i]);
                                        }
                                        break;
                                case GDAL.DataType.GDT_CInt32:
                                case GDAL.DataType.GDT_Int32:
                                case GDAL.DataType.GDT_UInt32:
                                        Utils.RasterOperation.RasterBandOperation.SingleDoubleFile.OpInt32 opInt32 =
                                                new Utils.RasterOperation.RasterBandOperation.SingleDoubleFile.OpInt32(ds, nodata);
                                        for (int i = 0; i < exp.Count; i++)
                                        {
                                                opInt32.ToCalc(exp[i]);
                                        }
                                        break;
                                default:
                                        throw new Exception("[GetAllBandInfo.CheckCopyMethod] can not get type");
                        }
                }
        }
}
