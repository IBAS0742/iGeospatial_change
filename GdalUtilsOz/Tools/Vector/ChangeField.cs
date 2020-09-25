using System;
using System.Collections.Generic;
using OGR = OSGeo.OGR;

namespace GdalUtilsOz.Tools.Vector
{
        class ChangeField
        {
                static void help(string commandName)
                {
                        Console.WriteLine("程序名 " + commandName + " shpPath fieldName type value fieldName type value ...");
                        Console.WriteLine("type 可选有 int、int64、real、string、binary");
                        Console.WriteLine("type 写其他都默认为 string");

                        Console.WriteLine("例如");
                        Console.WriteLine("程序名 changeField a.shp fid_1 int 10 fid_2 real 2.0");
                }
                static Utils.VectorOperation.Field CreateField(string fname, string type, string value)
                {
                        Utils.VectorOperation.Field field;
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
                                        field = new Utils.VectorOperation.Field(fname, OGR.FieldType.OFTInteger, Int32.Parse(value));
                                        break;
                                case "int64":
                                        field = new Utils.VectorOperation.Field(fname, OGR.FieldType.OFTInteger64, Int64.Parse(value));
                                        break;
                                case "real":
                                        field = new Utils.VectorOperation.Field(fname, OGR.FieldType.OFTReal, double.Parse(value));
                                        break;
                                case "string":
                                        field = new Utils.VectorOperation.Field(fname, OGR.FieldType.OFTString, value);
                                        break;
                                case "binary":
                                        field = new Utils.VectorOperation.Field(fname, OGR.FieldType.OFTBinary, int.Parse(value));
                                        break;
                                default:
                                        field = new Utils.VectorOperation.Field(fname, OGR.FieldType.OFTString, value);
                                        break;
                        }
                        return field;
                }
                /**
                 * 程序名 changeField shpPath fieldName type value ...
                 * type 可选有 int、int64、real、string、binary
                 */
                public static void ToChangeField(string[] args,string commandName)
                {
                        if (args.Length <= 2)
                        {
                                help(commandName);
                        }
                        else
                        {
                                List<Utils.VectorOperation.Field> fields = new List<Utils.VectorOperation.Field>();
                                if ((args.Length - 2) % 3 == 0)
                                {
                                        for (int i = 2; i < args.Length; i += 3)
                                        {
                                                fields.Add(CreateField(args[i], args[i + 1], args[i + 2]));
                                        }
                                        ChangeFieldHelp(args[1], fields);
                                }
                                else
                                {
                                        help(commandName);
                                }
                        }
                }
                /**
                 * 如果属性存在则修改，不存在则创建
                 */
                public static void ChangeFieldHelp(string shpPath, List<Utils.VectorOperation.Field> fields)
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
}
