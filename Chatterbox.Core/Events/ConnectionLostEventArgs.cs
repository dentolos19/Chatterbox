﻿using System;

namespace Chatterbox.Core.Events
{

    public class ConnectionLostEventArgs : EventArgs
    {

        public string Reason { get; set; } = "Unknown reason.";

    }

}