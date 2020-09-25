using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OGR = OSGeo.OGR;
using GDAL = OSGeo.GDAL;

namespace GdalUtils.Tools
{
        public class ShpOp
        {
                public class ChangeFiled
                {
                        static void help() {
                                Console.WriteLine("程序名 changeField shpPath fieldName type value fieldName type value ...");
                                Console.WriteLine("type 可选有 int、int64、real、string、binary");
                                Console.WriteLine("type 写其他都默认为 string");
                    
                                Console.WriteLine("例如");
                                Console.WriteLine("程序名 changeField a.shp fid_1 int 10 fid_2 real 2.0");
                        }
                        static Utils.Field CreateField(string fname, string type, string value)
                        {
                                Utils.Field field;
                                //OFTInteger = 0,
                                //OFTReal = 2,
                                //OFTString = 4,
                                //OFTBinary = 8,
                                //OFTInteger64 = 12,

                                //OFTDate = 9,
                                //OFTTime = 10,
                                //OFTDateTime = 11,
                                switch (type)
                                {
                                        case "int":
                                                field = new Utils.Field(fname, OGR.FieldType.OFTInteger, Int32.Parse(value));
                                                break;
                                        case "int64":
                                                field = new Utils.Field(fname, OGR.FieldType.OFTInteger64, Int64.Parse(value));
                                                break;
                                        case "real":
                                                field = new Utils.Field(fname, OGR.FieldType.OFTReal, double.Parse(value));
                                                break;
                                        case "string":
                                                field = new Utils.Field(fname, OGR.FieldType.OFTString, value);
                                                break;
                                        case "binary":
                                                field = new Utils.Field(fname, OGR.FieldType.OFTBinary, int.Parse(value));
                                                break;
                                        default:
                                                field = new Utils.Field(fname, OGR.FieldType.OFTString, value);
                                                break;
                                }
                                return field;
                        }
                        /**
                         * 程序名 changeField shpPath fieldName type value ...
                         * type 可选有 int、int64、real、string、binary
                         */
                        public static void ToChangeField(string[] args)
                        {
                                if (args.Length <= 2)
                                {
                                        help();
                                } else
                                {
                                        List<Utils.Field> fields = new List<Utils.Field>();
                                        if ((args.Length - 2) % 3 == 0)
                                        {
                                                for (int i = 2; i < args.Length; i += 3)
                                                {
                                                        fields.Add(CreateField(args[i], args[i + 1], args[i + 2]));
                                                }
                                                ChangeFieldHelp(args[1], fields);
                                        } else
                                        {
                                                help();
                                        }
                                }
                        }
                        /**
                         * 如果属性存在则修改，不存在则创建
                         */
                        public static void ChangeFieldHelp(string shpPath, List<Utils.Field> fields)
                        {
                                OGR.DataSource ds = OGR.Ogr.Open(shpPath, 1);
                                int lcount = ds.GetLayerCount();
                                for (int i = 0; i < lcount; i++)
                                {
                                        fields.ForEach(field => {
                                                if (field.CheckFieldTypeRight(ds.GetLayerByIndex(i)))
                                                {
                                                        field.sfd(ds.GetLayerByIndex(i));
                                                }
                                        });
                                }
                                ds.Dispose();
                        }
                }

                public class Rasterize
                {
                        static void help()
                        {
                                Console.WriteLine("程序名 rasterize shpPath tifPath [type] [burnValue] [defaultGeoTransform] [rasterCellSize]");
                                Console.WriteLine("shpPath 矢量文件(路径)");
                                Console.WriteLine("tifPath 栅格文件(路径)");
                                Console.WriteLine("type 可选，栅格数据类型，默认为double");
                                Console.WriteLine("可选有 [byte] [uint] [int] [ulong] [long] [float] [double]");
                                Console.WriteLine("burnValue 可选，表示栅格化后的值，默认为1.0");
                                Console.WriteLine("defaultGeoTransform 可选，表示是否选用默认的 geoTransform，该值在配置中设置，默认为 true");
                                Console.WriteLine("defaultGeoTransform 可选值为 True 或 其他(其他都是false) (单词无大小写之分)");
                                Console.WriteLine("rasterCellSize 表示栅格像素大小，当 defaultGeoTransform 为 True 时，该值有 配置 设定");
                                Console.WriteLine("例子");
                                Console.WriteLine("程序名 rasterize 1.shp 2.tif double 1.0 True 0.5 // 这里的 0.5 是无效的");
                                Console.WriteLine("程序名 rasterize 1.shp 2.tif double True 0.5 // 这里的 True 和 0.5 是无效的，请将 burnValue 设置为默认值");
                                Console.WriteLine("程序名 rasterize 1.shp 2.tif double 1.0 No 0.5 // 只要不是true都会认为是false");
                                Console.WriteLine("▲注意:默认的 geotransform 只取 [1][2][4][5]，其中[0][3]是右上角坐标，由程序自动计算");
                        }
                        public static void ToRasterize(string[] args) {
                                // string shpPath,string tifPath,double burnValue = 1.0,bool defaultGeoTransform = true,double rasterCellSize = 0.00833333
                                if (args.Length < 3 || args.Length > 7)
                                {
                                        help();
                                } else
                                {
                                        double burnValue = 1.0;
                                        bool defaultGeoTransform = true;
                                        double rasterSize = 0.008333;
                                        GDAL.DataType type = GDAL.DataType.GDT_Float64;
                                        if (args.Length == 7) rasterSize = double.Parse(args[5]);
                                        if (args.Length >= 6) defaultGeoTransform = String.IsNullOrEmpty(args[4]) ?
                                                                true : String.Equals(args[4].ToLower().Trim(), "true");
                                        if (args.Length >= 5) burnValue = double.Parse(args[3]);
                                        if (args.Length >= 4)
                                        {
                                                switch (args[4].Trim().ToLower())
                                                {
                                                        case "byte":
                                                                type = GDAL.DataType.GDT_Byte;
                                                                break;
                                                        case "uint":
                                                                type = GDAL.DataType.GDT_UInt32;
                                                                break;
                                                        case "int":
                                                                type = GDAL.DataType.GDT_Int16;
                                                                break;
                                                        case "ulong":
                                                                type = GDAL.DataType.GDT_UInt32;
                                                                break;
                                                        case "long":
                                                                type = GDAL.DataType.GDT_Int32;
                                                                break;
                                                        case "float":
                                                                type = GDAL.DataType.GDT_Float32;
                                                                break;
                                                        case "double":
                                                                type = GDAL.DataType.GDT_Float64;
                                                                break;
                                                }
                                        }
                                        Utils.ShpFileOP.Rasterize(args[1], args[2],type, burnValue, defaultGeoTransform, rasterSize);
                                }
                        }
                }
        }
}
