using System;
using System.Collections.Generic;
using System.Linq;

namespace GdalUtils.Utils
{
        class Prj
        {
                public enum Prjection
                {
                        EPSG4326
                };
                static Dictionary<string, string> prjDict = new Dictionary<string, string>() {
                        { "EPSG4326", "GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]" }
                };
                public static string getPrjString(Prjection prj) {
                        return getPrjString(prj.ToString());
                }
                public static string getPrjString(string _prj)
                {
                        if (prjDict.Keys.Contains(_prj))
                        {
                                return prjDict[_prj];
                        }
                        else
                        {
                                throw new Exception("找不到投影格式[" + _prj + "]");
                        }
                }
                public static Prjection getPrj(string str)
                {
                        return (Prjection)Enum.Parse(typeof(Prjection), str);
                }
        }
}
