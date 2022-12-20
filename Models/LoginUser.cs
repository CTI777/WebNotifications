using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;
using System.Windows;

namespace WebNotifications.Models
{
    public class LoginUser
    {
        public string name { get; set; }

        public string sessionID { get; set; }
        
    }
}