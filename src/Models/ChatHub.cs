using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections;
using System.Threading.Tasks;
using KMezzenger.DataAccess;

namespace KMezzenger.Models
{
    [AuthorizeClaims]
    public class ChatHub : Hub
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();

        public void send_message(string who, string message, string message_id)
        {
            string myname = Context.User.Identity.Name;
            // check 'who' is exist ?
            bool existWho = UserRepository.check_user_exist(who);
            if (!existWho)
            {
                Clients.Caller.on_result_send_message(message_id, 0, "No one named " + who);
                return;
            }
            foreach (var connectionId in _connections.GetConnections(who))
            {
                Clients.Client(connectionId).on_receive_message(myname, message);
            }

            Clients.Caller.on_result_send_message(message_id, 1);
        }

        public override Task OnConnected()
        {
            string name = Context.User.Identity.Name;
            _connections.Add(name, Context.ConnectionId);

            Clients.All.on_buddy_connect(name);
            // Get contact of user.
            string[] contacts = null;
            Clients.Caller.on_receive_contacts(contacts);

            return base.OnConnected();
        }
        public override Task OnDisconnected()
        {
            string name = Context.User.Identity.Name;
            _connections.Remove(name, Context.ConnectionId);

            return base.OnDisconnected();
        }
        public override Task OnReconnected()
        {
            string name = Context.User.Identity.Name;
            _connections.Add(name, Context.ConnectionId);

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