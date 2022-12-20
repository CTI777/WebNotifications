using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;
using System.Windows;

namespace WebNotifications.Models
{
    public class ClientUser
    {
        public ClientUser(User user) {
            this.user = user;

            streamWriter = null;
            valid = -1;
        }
        public User user { get; set; }
        public int valid { get; set; }
        public StreamWriter streamWriter { get; set; }

        public void Subscribe(Stream stream) {
            if (stream != null)
            {
                streamWriter = new StreamWriter(stream);
                valid = 1;
            }
        }

        public void Login()
        {
            valid = 0;
            user?.Login();
        }

        public void Logout() {
            valid = 0;
            user?.Logout();
        }

        public bool IsValid() {
            return (valid == 1);
        }

        public bool Is4Kill()
        {
            return (valid == 0);
        }

    }
}