﻿using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Chatterbox.Core.Communication
{

    public class ComMessage
    {

        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(ComMessage));

        public string Name { get; set; }

        public string Message { get; set; }

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

        public static ComMessage Parse(string data)
        {
            var buffer = Encoding.Unicode.GetBytes(data);
            using var stream = new MemoryStream(buffer);
            return Serializer.Deserialize(stream) as ComMessage;
        }
    }

}