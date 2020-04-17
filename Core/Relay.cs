﻿using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Chatterbox.Core
{

    public class Relay
    {

        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(Relay));

        internal bool IsEnding { get; set; }

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

        public static Relay Parse(string data)
        {
            var buffer = Encoding.Unicode.GetBytes(data);
            using var stream = new MemoryStream(buffer);
            return Serializer.Deserialize(stream) as Relay;
        }

    }

}