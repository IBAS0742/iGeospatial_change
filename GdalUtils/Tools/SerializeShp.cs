using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GdalUtils.Tools
{
        // 这个类的作用是将 shp 文件序列化为 geos 对象，便于计算
        class SerializeShp
        {
                public static void ToSerializeShp(string shpPath,string serPath,FeatureType type) {
                        Utils.SerializeObject.ToSerialize(
                                SelectFeatureHelp.OgrDSToGeometryList(
                                        SelectFeatureHelp.OpenFromShpFile(shpPath), 
                                        (int)type
                                ),
                        serPath);
                }
        }
}
