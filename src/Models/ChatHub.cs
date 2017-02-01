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

        public MessageStatus send_message(string who, string message, DateTime date_sent, long client_message_id)
        {
            string myname = Context.User.Identity.Name;
            // check 'who' is exist ?
            User user_to = UserRepository.get_user(who);
            if (user_to == null)
                return new MessageStatus { message = "No one named " + who, client_message_id = client_message_id, status = -1 };

            //save message to database
            long message_id = UserRepository.save_message(myname, who, message, date_sent, client_message_id);
            var message_object = new Message
            {
                message_id = message_id,
                client_message_id = client_message_id,
                from = myname,
                content = message,
                date_sent = date_sent
            };
            bool is_sent = false;
            foreach (var connectionId in _connections.GetConnections(who))
            {
                Clients.Client(connectionId).on_receive_message(message_object);
                is_sent = true;
            }

            if (is_sent)
                UserRepository.update_message_user(message_id, user_to.user_id, date_sent, 1);

            return new MessageStatus { client_message_id = client_message_id, status = 0 };
        }
        public void received_message(Message message)
        {
            User user_to = UserRepository.get_user(Context.User.Identity.Name);

            UserRepository.update_message_user(message.message_id, user_to.user_id, message.date_received, 2);

            foreach (var connectionId in _connections.GetConnections(message.from))
            {
                Clients.Client(connectionId).on_deliveried_message(new MessageStatus { client_message_id = message.client_message_id, status = 1 });
            }
        }

        public override Task OnConnected()
        {
            string name = Context.User.Identity.Name;
            _connections.Add(name, Context.ConnectionId);

            // Get contact of user.
            string[] contacts = UserRepository.get_your_buddies(name);

            foreach (var connectionId in _connections.GetConnections(contacts))
            {
                Clients.Client(connectionId).on_buddy_status_changed(new UserStatus { username = name, status = 1 });
            }

            // GET Buddies status
            IEnumerable<UserStatus> userStatus = get_user_status(contacts);
            Clients.Caller.on_receive_contacts(userStatus);

            // get new message
            Message[] messages = UserRepository.get_new_message(name);
            foreach (var message_object in messages)
            {
                Clients.Caller.on_receive_message(message_object);
            }

            return base.OnConnected();
        }
        public override Task OnDisconnected()
        {
            string name = Context.User.Identity.Name;
            _connections.Remove(name, Context.ConnectionId);

            string[] contacts = UserRepository.get_your_buddies(name);

            foreach (var connectionId in _connections.GetConnections(contacts))
            {
                Clients.Client(connectionId).on_buddy_status_changed(new UserStatus { username = name, status = 0 });
            }

            return base.OnDisconnected();
        }
        public override Task OnReconnected()
        {
            string name = Context.User.Identity.Name;
            _connections.Add(name, Context.ConnectionId);

            return base.OnReconnected();
        }

        private IEnumerable<UserStatus> get_user_status(string[] user)
        {
            return user.Select(u => new UserStatus { username = u, status = _connections.HasConnection(u) ? 1 : 0});
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