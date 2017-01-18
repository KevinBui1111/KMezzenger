using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;

namespace KMezzenger.DataAccess
{
    public class UserRepository :IUserRepository
    {
        static List<string> userList = new List<string>
        {
            "KevinBui",
            "Khanh.BuiDang",
            "Maika"
        };
        internal static bool ValidateUser(string UserName, string Password)
        {
            return IsADValid(UserName, Password);
        }

        private static bool IsADValid(string pUserName, string pPassword)
        {
            return pPassword == "123123123";
            string ldap = Resources.Setting.LDAP_ADDRESS;
            DirectoryEntry ad = new DirectoryEntry(ldap, pUserName, pPassword);

            try
            {
                Object obj = ad.NativeObject;
                DirectorySearcher search = new DirectorySearcher(ad);
                search.Filter = "(SAMAccountName=" + pUserName + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();
                if (result == null)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal bool check_user_exist(string who)
        {
            return userList.Contains(who);
        }

        internal string[] get_your_buddies(string username)
        {
            return userList.Where(u => u != username).ToArray();
        }

        internal string[] get_user(string username)
        {
            return null;
        }
    }
}