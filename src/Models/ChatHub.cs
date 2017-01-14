using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections;
using System.Threading.Tasks;

namespace KMezzenger.Models
{
    [AuthorizeClaims]
    public class ChatHub : Hub
    {
        private static Dictionary<string, string> htUsers_ConIds = new Dictionary<string, string>(20);
        public void registerConId(string userID)
        {
            htUsers_ConIds[userID] = Context.ConnectionId;
        }
        public void Send(string to_connectionID, string message)
        {
            var c = Context.User.Identity;
            Clients.Client(to_connectionID).addNewMessageToPage(to_connectionID, htUsers_ConIds[to_connectionID], message);
        }

        public override Task OnConnected()
        {
            htUsers_ConIds[Context.ConnectionId] = Context.QueryString["username"];

            Clients.All.newUserConnect(htUsers_ConIds[Context.ConnectionId], Context.ConnectionId);
            Clients.Caller.onReceiveListContact(htUsers_ConIds);
            return base.OnConnected();
        }
        public override Task OnDisconnected()
        {
            htUsers_ConIds.Remove(Context.ConnectionId);
            Console.WriteLine("disconnect: ", Context.ConnectionId);

            return base.OnDisconnected();
        }
        public override Task OnReconnected()
        {
            Console.WriteLine("OnReconnected: ", Context.ConnectionId);

            return base.OnReconnected();
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AuthorizeClaimsAttribute : AuthorizeAttribute
    {
        protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return user.Identity.IsAuthenticated;
        }
    }
}