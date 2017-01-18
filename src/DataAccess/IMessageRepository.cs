using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KMezzenger.Models;

namespace KMezzenger.DataAccess
{
    public interface IMessageRepository
    {
        bool save_message(string from, string to, string message, string message_id);
    }
    public interface IUserRepository
    {
        string[] get_your_buddies(string username);
        bool check_user_exist(string username);
        User get_user(string username);
    }
}