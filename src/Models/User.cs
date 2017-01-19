using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KMezzenger.Models
{
    public class User
    {
        public int user_id { get; set; }
        public string username { get; set; }
        public string hash_pass { get; set; }
        public string hash_salt { get; set; }

        public int login_type { get; set; }
        public int status { get; set; }
    }
}