using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Chatterbox.Core.Comms
{

    public class CommMessage
    {

        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(CommMessage));

        public string Username { get; set; }
        public string Message { get; set; }
        public string Command { get; set; } = "message";
        public DateTime Time { get; } = DateTime.Now;

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

        public static CommMessage Parse(string data)
        {
            var buffer = Encoding.Unicode.GetBytes(data);
            using var stream = new MemoryStream(buffer);
            return (CommMessage)Serializer.Deserialize(stream);
        }

    }

}