using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KMezzenger.Models;

namespace KMezzenger.DataAccess
{
    public interface IDataAccess
    {
        long save_message(string from, string to, string message, DateTime date_sent, long message_id);
        void update_message_user(long message_id, int user_id, DateTime date_update, int status);
        Message[] get_new_message(string username);

        string[] get_your_buddies(string username);
        User get_user(string username);
        void create_user(string username, string hashpass, string salt);
        void reset_password(string userName, string password, string salt);
    }
}