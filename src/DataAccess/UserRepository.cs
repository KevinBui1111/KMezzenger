using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;

namespace KMezzenger.DataAccess
{
    public class UserRepository
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
            return true;
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

        internal static bool check_user_exist(string who)
        {
            return userList.Contains(who);
        }
    }
}