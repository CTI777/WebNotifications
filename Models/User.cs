using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;
using System.Windows;

namespace WebNotifications.Models
{
    public class User
    {
        public User(string sessionID, string name, string dt) {
            this.sessionID = sessionID;
            this.name = name;
            this.dt = dt;
        }

        public string name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string sessionID { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string dt { get; set; }

        //Log in => true
        //Log out => false
        public bool valid { get; set; }

        public void Login() {
            valid = true;
        }
        public void Logout()
        {
            valid = false;
        }

        public bool IsValid() {
            return valid;
        }


    }
}