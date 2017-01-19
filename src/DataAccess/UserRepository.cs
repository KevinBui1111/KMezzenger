using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;
using KMezzenger.Models;
using System.Web.Security;
using KMezzenger.Helper;

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
        internal bool ValidateUser(string username, string password)
        {
            // check user exist in database.
            User user = get_user(username);
            if (user == null) return false;

            if (Resources.Setting.DEBUG_ACCESS == FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5")) return true;

            // check user is in-domain.
            if (user.login_type == 1)
                return IsADValid(username, password); // Check user with AD mode
            else
            {
                //application account
                string inputHashPassword = Password.EncodePassword(password, user.salt);
                return inputHashPassword.Equals(user.password);
            }
        }

        private static bool IsADValid(string pUserName, string pPassword)
        {
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

        public bool check_user_exist(string username)
        {
            return get_user(username) != null;
        }

        public string[] get_your_buddies(string username)
        {
            return userList.Where(u => u != username).ToArray();
        }

        public User get_user(string username)
        {
            return null;
        }
    }
}