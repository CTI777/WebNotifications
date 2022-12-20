using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;
using System.Windows;

namespace WebNotifications.Models
{
    public class GraphClient
    {
        public GraphClient(Stream stream)
        {         
            streamWriter = new StreamWriter(stream);
            valid = true;
        }

        public bool valid { get; set; }
        public StreamWriter streamWriter { get; set; }


    }
}