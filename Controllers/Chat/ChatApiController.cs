using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Web;
using System.Threading.Tasks;
using WebNotifications.Models;
using System.Web.Http;
using Microsoft.Ajax.Utilities;
using System.Runtime.InteropServices.ComTypes;
using System.Web.UI;
using System.Web.Configuration;
using System.Web.Http.Results;
using System.Xml.Linq;
using WebNotifications.Tools;

namespace WebNotifications.Controllers
{
    [Route("api/chat")]
    public class ChatApiController : ApiController
    {
        private static readonly TimerProcess timer_clean = new TimerProcess(20000, CleanUp);
        private static readonly ConcurrentBag<ClientUser> clients;
        //private static readonly List<ClientUser> clients;

        //This is for handling session
        private static IDataPersistance<string> sessionStorage;

        private string getSessionId()
        {
            return sessionStorage.ObjectValue;
        }
        private string createSetSessionId(string user, string dt)
        {
            string id = Tools.Tools.GetMd5Hash(user + dt);
            sessionStorage.ObjectValue = id;
            return id;
        }

        private static void CleanUp(object state)
        {
            int j,cnt= clients.Count;

            for (int i = 1; i <= cnt;)
            {
                j = 1;
                foreach (var client in clients)
                {   
                    if (j>=i)
                    {
                        if (client.Is4Kill())
                        {
                            try {
                                ClientUser c;
                                clients.TryTake(out c);

                                c?.streamWriter?.Dispose();

                            } catch(Exception e) {}

                            break;
                        }
                    } 
                    j++;
                }

                cnt = clients.Count;
                if (cnt < 1) break;
                i = j;
                if (cnt < i) break;
            }
            
        }


        static ChatApiController()
        {

            sessionStorage = new SessionDataPersistance<string>();

            clients = new ConcurrentBag<ClientUser>();
            timer_clean.Start();
        }

        private ClientUser GetClientBySessionId(string id)
        {
            return clients.Where(c => string.Equals(c.user?.sessionID, id)).FirstOrDefault();

        }
        private User GetUserBySessionId(string id) {

            return clients.Where(c => string.Equals(c.user?.sessionID, id)).Select(c => c.user).FirstOrDefault();
        }

        [HttpPost, Route("api/chat/login")]
        public IHttpActionResult Login(LoginUser u)
        {
            var logtime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            var id = string.IsNullOrEmpty(u.sessionID) ? getSessionId() : u.sessionID;

            ClientUser client = null;

            if (string.IsNullOrEmpty(id))
            {
                id = createSetSessionId(u.name, logtime);
            } else
            {
                client = GetClientBySessionId(id);
            }

            if (client != null) {

                if ((bool)client.user?.IsValid()) { 
                    if (string.Equals(client.user?.name, u.name)) {
                        return Ok(client.user);
                    } else {
                        return Content(HttpStatusCode.BadRequest, "Wrong User");
                    }
                }

                client.Login();
                client.user.name = u.name;
                client.user.dt = logtime;
             
                return Ok(client.user);
            } 

            User user = new User(id, u.name, logtime);
            client = new ClientUser(user);
            client.Login();
            clients.Add(client);
            return Ok(user);
        }

        [HttpPost, Route("api/chat/logout")]
        public IHttpActionResult Logout(String sessionID)
        {
            ClientUser client = GetClientBySessionId(sessionID);

            if (client != null)
            {   client.Logout();
                SendMessageUserRemoved();
                return Content(HttpStatusCode.OK, "Logout Ok: User " + client.user?.name);
            }    

            return Content(HttpStatusCode.NotFound, "Wrong User");
        }


        [HttpGet, Route("api/chat/subscribe/{sessionID}")]
        // public HttpResponseMessage Subscribe(HttpRequestMessage request)
        public HttpResponseMessage Subscribe(string sessionID)
        {
            /*           
             try
             {
                 sessionID=Request.GetQueryNameValuePairs()
                 .Where(p => p.Key.Equals("sessionID"))
                 .Select(p => p.Value)
                 .FirstOrDefault()
                 .ToString();
             } catch (Exception e) {}
             HttpResponseMessage response = request.CreateResponse();
            */


            ClientUser client = sessionID!=null ? this.GetClientBySessionId(sessionID) : null;

            HttpResponseMessage response = Request.CreateResponse();

            if (client==null) { 
                response.StatusCode = HttpStatusCode.Unauthorized;
                return response;
            }

            if (client.IsValid()) {
                response.StatusCode = HttpStatusCode.NotImplemented;
                return response;
            }

            response.Content = new PushStreamContent(
                (stream,content,context) => OnStreamAvailable(stream, content, context, client), 
                "text/event-stream");
            return response;
        }

        private async void ChatCallbackMsg(ChatMessage m)
        {
            var data = string.Format("data:{0}|{1}|{2}\n\n", m.username, m.text, m.dt);
            bool notify = false;

            foreach (var client in clients)
            {
                try
                {
                    if (client.IsValid())
                    {
                        await client.streamWriter.WriteAsync(data);
                        await client.streamWriter.FlushAsync();
                        //client.streamWriter.Dispose();

                    }
                }
                catch (Exception e) {

                    client.Logout();
                    notify = true;
                }
            }

            if (notify)
            {
                SendMessageUserRemoved();
            }
        }

        private async void SendMessageUserRemoved()
        {
            ChatMessage sm = new ChatMessage()
            {
                username = "$SYSTEM_REM_USER",
                text = "Users removed",
                dt = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")

            };
            //Delay 1 second
            Task.Delay(new TimeSpan(0, 0, 1)).ContinueWith(o => { ChatCallbackMsg(sm); });
        }

        private async void SendMessageNewUser(User user)
        {
            ChatMessage m = new ChatMessage()
            {
                username = "$SYSTEM_NEW_USER",
                text = "New user: " + user.name + " (" + user.sessionID + ")",
                dt = user.dt,

            };

            //Delay 1 second
            Task.Delay(new TimeSpan(0, 0, 1)).ContinueWith(o => { ChatCallbackMsg(m); });
        }


        private void OnStreamAvailable(Stream stream, HttpContent content,TransportContext context, ClientUser client)
        {
            client.Subscribe(stream);
            
            SendMessageNewUser(client.user);
        }

        [HttpPost, Route("api/chat/push")]
        public async Task<IHttpActionResult> Push(PushMessage m)
        {
            User user = this.GetUserBySessionId(m.sessionID);

            if (user !=null)
            {
                ChatMessage message = new ChatMessage()
                {   username = user.name,
                    text = m.text,
                    dt = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
                };

                //ChatCallbackMsg(message);
                Task.Delay(new TimeSpan(0, 0, 0,0,100)).ContinueWith(o => { ChatCallbackMsg(message); });

                return Content(HttpStatusCode.OK, "Message Sent");
            }

            return Content(HttpStatusCode.NotFound, "Wrong User");
        }


        [HttpGet, Route("api/chat/users")]
        public List<User> GetUsers()
        {
            return clients.Where(c => c.IsValid()).Select(c => c.user).ToList();

        }


    }
}