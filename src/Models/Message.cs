using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KMezzenger.Models
{
    public class Message
    {
        public long message_id { get; set; }
        public long client_message_id { get; set; }
        public string content { get; set; }
        public DateTime date_sent { get; set; }
        public DateTime date_received { get; set; }
        public string from { get; set; }
        public string to { get; set; }
    }

    public class MessageStatus
    {
        public long client_message_id { get; set; }
        // 0: sent, 1: received: 2: seen, -1: error
        public int status { get; set; }
        public string message { get; set; }
    }
}