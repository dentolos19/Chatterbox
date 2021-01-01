using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Chatterbox.Core
{

    public class CbMessage
    {

        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(CbMessage));
        
        public string Username { get; set; }
        public string Message { get; set; }
        public CbMessageCreator Creator { get; set; } = CbMessageCreator.Unknown;
        public DateTime Time { get; set; } = DateTime.Now;

        public static CbMessage Parse(string data)
        {
            var buffer = Encoding.Unicode.GetBytes(data);
            using var stream = new MemoryStream(buffer);
            return (CbMessage)Serializer.Deserialize(stream);
        }

        public override string ToString()
        {
            var settings = new XmlWriterSettings
            {
                NewLineHandling = NewLineHandling.None,
                Indent = false
            };
            using var strWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(strWriter, settings);
            Serializer.Serialize(xmlWriter, this);
            return strWriter.ToString();
        }

    }

}