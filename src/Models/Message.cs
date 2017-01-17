using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KMezzenger.Models
{
    public class Message
    {
        public string message_id { get; set; }
        public string content { get; set; }
        public DateTime date_sent { get; set; }
        public string from { get; set; }
    }
}