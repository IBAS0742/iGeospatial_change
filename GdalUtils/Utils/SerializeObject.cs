using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GdalUtils.Utils
{
        public class SerializeObject
        {
                private static IFormatter formatter = new BinaryFormatter();
                public static void ToSerialize(Object obj,string path) {
                        Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                        formatter.Serialize(stream, obj);
                        stream.Close();
                }

                public static object FromSerialize(string path)
                {
                        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                        return formatter.Deserialize(stream);
                }

                public static void ToXMLSerialize(Object obj,string path,Type type)
                {
                        FileStream stream = new FileStream(path,FileMode.Create);
                        XmlSerializer serizer = new XmlSerializer(type);
                        serizer.Serialize(stream, obj);
                        stream.Close();
                }
                public static object FromXMLSerialize(string path,Type type)
                {
                        FileStream stream = new FileStream(path, FileMode.Open);
                        XmlSerializer serizer = new XmlSerializer(type);
                        object obj = serizer.Deserialize(stream);
                        stream.Close();
                        return obj;
                }
        }
}
