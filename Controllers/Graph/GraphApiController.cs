using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;
using WebNotifications.Models;
using WebNotifications.Tools;


namespace WebNotifications.Controllers
{
    [Route("api/graph")]
    public class GraphApiController : ApiController
    {
        private static readonly Lazy<Timer> _timer = new Lazy<Timer>(() => new Timer(TimerCallback, null, 0, 5000));
        private static readonly TimerProcess timer_clean = new TimerProcess(20000, CleanUp);

        private static readonly ConcurrentBag<GraphClient> clients;
        static GraphApiController()
        {
            clients = new ConcurrentBag<GraphClient>();
            timer_clean.Start();

        }

        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            Timer t = _timer.Value;
            

            HttpResponseMessage response = request.CreateResponse();
            response.Content = new PushStreamContent(OnStreamAvailable, "text/event-stream");
            return response;
        }


        private static void TimerCallback(object state)
        {
            Random randNum = new Random();

            var msg = "data:" + randNum.Next(30, 100) + "\n";
            foreach (var client in clients)
            {
                try
                {
                    if (client.valid) {
                        client.streamWriter.WriteLine(msg);
                        client.streamWriter.Flush();
                        client.streamWriter.Dispose();
                    }
                    
                }

                catch (Exception e)
                {
                    client.valid = false;
                }

            }
            //To set timer with random interval
            //  _timer.Value.Change(TimeSpan.FromMilliseconds(randNum.Next(1,3)*500), TimeSpan.FromMilliseconds(-1));

        }

        private void OnStreamAvailable(Stream stream, HttpContent content,
           TransportContext context)
        {
            var client = new GraphClient(stream);
            clients.Add(client);
        }


        private static void CleanUp(object state)
        {
            int j, cnt = clients.Count;
            if (cnt < 1) return;
         
            for (int i = 1; i <= cnt;)
            {
                j = 1;
                foreach (var client in clients)
                {
                    if (j >= i)
                    {
                        if (!client.valid)
                        {
                            try
                            {
                                GraphClient c;
                                clients.TryTake(out c);

                               
                                c?.streamWriter?.Dispose();
                            }
                            catch (Exception e) { }
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
    }
}