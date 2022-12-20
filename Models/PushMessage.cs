using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNotifications.Models
{
    public class PushMessage
    {
        public string sessionID { get; set; }
        public string text { get; set; }
    }
}