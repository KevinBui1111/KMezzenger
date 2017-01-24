﻿using KMezzenger.Helper;
using KMezzenger.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Web.Security;

namespace KMezzenger.DataAccess
{
    public class UserRepository
    {
        static IDataAccess dataAccess = new OraDataAccess();

        internal static bool ValidateUser(string username, string password)
        {
            // check user exist in database.
            User user = dataAccess.get_user(username);
            if (user == null) return false;

            if (Resources.Setting.DEBUG_ACCESS == FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5")) return true;

            // check user is in-domain.
            if (user.login_type == 1)
                return IsADValid(username, password); // Check user with AD mode
            else
            {
                //application account
                string inputHashPassword = Password.EncodePassword(password, user.hash_salt);
                return inputHashPassword.Equals(user.hash_pass);
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

        internal static void create_user(string username, string password)
        {
            string salt = Password.GenerateSalt();
            string hashpass = Password.EncodePassword(password, salt);
            dataAccess.create_user(username, hashpass, salt);
        }

        public static bool check_user_exist(string username)
        {
            return dataAccess.get_user(username) != null;
        }

        public static int save_message(string from, string to, string message, DateTime date_sent, string message_id)
        {
            return dataAccess.save_message(from, to, message, date_sent, message_id);
        }
        public static string[] get_your_buddies(string username)
        {
            return dataAccess.get_your_buddies(username);
        }

        internal static void reset_password(string userName, string password)
        {
            string salt = Password.GenerateSalt();
            string hashpass = Password.EncodePassword(password, salt);
            dataAccess.reset_password(userName, hashpass, salt);
        }
    }
}