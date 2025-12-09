using System.IO;
using System.Xml.Serialization;

namespace Desktop.Helpers
{
    public class XmlHelper
    {
        public static T LoadFromXml<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath)) return new T();

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stream)!;
            }
        }

        public static void SaveToXml<T>(string filePath, T data)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stream, data);
            }
        }
    }
}
