using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OGR = OSGeo.OGR;

namespace GdalUtilsOz.Utils.VectorOperation
{
        class Field
        {
                string _fileName = "";
                OSGeo.OGR.FieldDefn pFieldDefn;
                int intValue;
                string strValue;
                double doubleValue;
                public delegate void SetFieldDelegate(OGR.Layer lay);  //返回值为空
                public SetFieldDelegate sfd;
                public Field(string fieldName, OGR.FieldType type, int value)
                {
                        intValue = value;
                        pFieldDefn = new OSGeo.OGR.FieldDefn(fieldName, type);
                        _fileName = fieldName;
                        sfd = SetIntField;
                }
                public Field(string fieldName, OGR.FieldType type, double value)
                {
                        doubleValue = value;
                        pFieldDefn = new OSGeo.OGR.FieldDefn(fieldName, type);
                        _fileName = fieldName;
                        sfd = SetDoubleField;
                }
                public Field(string fieldName, OGR.FieldType type, string value)
                {
                        strValue = value;
                        pFieldDefn = new OSGeo.OGR.FieldDefn(fieldName, type);
                        _fileName = fieldName;
                        sfd = SetStringField;
                }
                public Field(string fieldName, OGR.FieldType type, SetFieldDelegate _sfd = null)
                {
                        pFieldDefn = new OSGeo.OGR.FieldDefn(fieldName, type);
                        _fileName = fieldName;
                        if (_sfd == null)
                        {
                                sfd = SetFiledAsIndex;
                        }
                        else
                        {
                                sfd = _sfd;
                        }
                }

                void SetFiledAsIndex(OGR.Layer lay)
                {
                        //第二个参数如果为TRUE，则根据格式驱动程序的限制，可能以略有不同的形式创建该字段。
                        lay.CreateField(pFieldDefn, 1);
                        long fcount = lay.GetFeatureCount(1);
                        for (long i = 0; i < fcount; i++)
                        {
                                OGR.Feature f = lay.GetFeature(i);
                                f.SetField(_fileName, (int)i);
                                lay.SetFeature(f);
                        }
                }
                void SetDoubleField(OGR.Layer lay)
                {
                        //第二个参数如果为TRUE，则根据格式驱动程序的限制，可能以略有不同的形式创建该字段。
                        lay.CreateField(pFieldDefn, 1);
                        long fcount = lay.GetFeatureCount(1);
                        for (long i = 0; i < fcount; i++)
                        {
                                OGR.Feature f = lay.GetFeature(i);
                                f.SetField(_fileName, doubleValue);
                                lay.SetFeature(f);
                        }
                }
                void SetIntField(OGR.Layer lay)
                {
                        //第二个参数如果为TRUE，则根据格式驱动程序的限制，可能以略有不同的形式创建该字段。
                        lay.CreateField(pFieldDefn, 1);
                        long fcount = lay.GetFeatureCount(1);
                        for (long i = 0; i < fcount; i++)
                        {
                                OGR.Feature f = lay.GetFeature(i);
                                f.SetField(_fileName, intValue);
                                lay.SetFeature(f);
                        }
                }
                void SetStringField(OGR.Layer lay)
                {
                        if (!CheckFieldExist(lay))
                        {
                                //第二个参数如果为TRUE，则根据格式驱动程序的限制，可能以略有不同的形式创建该字段。
                                lay.CreateField(pFieldDefn, 1);
                        }
                        else
                        {
                                Console.WriteLine(_fileName + " is exist");
                        }
                        long fcount = lay.GetFeatureCount(1);
                        for (long i = 0; i < fcount; i++)
                        {
                                OGR.Feature f = lay.GetFeature(i);
                                f.SetField(_fileName, strValue);
                                lay.SetFeature(f);
                        }
                }

                public bool CheckFieldExist(OGR.Layer lay)
                {
                        // 第二个参数为false好像有点像不存在即创建的感觉
                        return lay.FindFieldIndex(_fileName, 1) != -1;
                }
                public bool CheckFieldTypeRight(OGR.Layer lay)
                {
                        bool ret = CheckFieldExist(lay);
                        if (ret)
                        {
                                return lay.GetFeature(0).GetFieldType(_fileName) == pFieldDefn.GetFieldType();
                        }
                        else
                        {
                                return true;
                        }
                }
        }
}
