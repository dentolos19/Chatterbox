﻿using System;
using System.IO;
using System.Xml.Serialization;

namespace Chatterbox.Core
{

    public class Configuration
    {

        private static readonly string Source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "Chatterbox.cfg");
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(Configuration));

        public bool AutoCheckUpdates { get; set; } = true;

        public bool UsePortForwarding { get; set; } = false;

        public string Username { get; set; } = Environment.UserName;

        public void Save()
        {
            using var stream = new FileStream(Source, FileMode.Create);
            Serializer.Serialize(stream, this);
        }

        public static Configuration Load()
        {
            if (!File.Exists(Source))
                return new Configuration();
            using var stream = new FileStream(Source, FileMode.Open);
            return Serializer.Deserialize(stream) as Configuration;
        }

    }

}