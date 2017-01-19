using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;

namespace KMezzenger.DataAccess
{
    public class MessageRepository : IMessageRepository
    {
        public bool save_message(string from, string to, string message, string message_id)
        {
            return true;
        }
    }
}