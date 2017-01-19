using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KMezzenger.Models;

namespace KMezzenger.DataAccess
{
    public interface IRepository
    {
        bool save_message(string from, string to, string message, string message_id);
        string[] get_your_buddies(string username);
        User get_user(string username);
    }
}