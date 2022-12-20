using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.WebHost;
using System.Web.Routing;
using System.Web.SessionState;

//https://www.strathweb.com/2012/11/adding-session-support-to-asp-net-web-api/

namespace WebNotifications.Controllers
{
    public class SessionControllerHandler : HttpControllerHandler, IRequiresSessionState
    {
        public SessionControllerHandler(RouteData routeData)
            : base(routeData)
        { }
    }
}