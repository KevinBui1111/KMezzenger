using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KMezzenger.Models;
using Oracle.DataAccess.Client;
using System.Data;
using HCVN.Helpers;
using Oracle.DataAccess.Types;

namespace KMezzenger.DataAccess
{
    public class OraDataAccess : IDataAccess
    {
        public long save_message(string from, string to, string message, DateTime date_sent, long client_message_id)
        {
            OracleParameter pResult = new OracleParameter("RESULT", OracleDbType.Int64, ParameterDirection.Output);

            List<OracleParameter> listParameter = new List<OracleParameter>() {
                new OracleParameter("PARAM", message),
                new OracleParameter("PARAM", from),
                new OracleParameter("PARAM", to),
                new OracleParameter("PARAM", date_sent),
                new OracleParameter("PARAM", client_message_id),
                pResult,
            };

            DBUtilsOra.ExecuteNonQuerySP("SAVE_MESSAGE", listParameter, "CONN_KMESS");
            OracleDecimal res = (OracleDecimal)pResult.Value;

            return res.ToInt64();
        }

        public string[] get_your_buddies(string username)
        {
            OracleParameter pResult = new OracleParameter("RESULT", OracleDbType.RefCursor, ParameterDirection.ReturnValue);
            var listParameter = new List<OracleParameter>() {
                pResult,
                new OracleParameter("PARAM", username)
            };

            return DBUtilsOra.ExecuteSPList<User>("GET_BUDDIES", listParameter, "CONN_KMESS")
                .Select(u => u.username)
                .ToArray();
        }

        public User get_user(string username)
        {
            OracleParameter pResult = new OracleParameter("RESULT", OracleDbType.RefCursor, ParameterDirection.ReturnValue);
            var listParameter = new List<OracleParameter>() {
                pResult,
                new OracleParameter("PARAM", username)
            };

            return DBUtilsOra.ExecuteSPList<User>("GET_USER", listParameter, "CONN_KMESS").SingleOrDefault();
        }

        public void create_user(string username, string hashpass, string salt)
        {
            List<OracleParameter> listParameter = new List<OracleParameter>() {
                new OracleParameter("PARAM", username),
                new OracleParameter("PARAM", hashpass),
                new OracleParameter("PARAM", salt),
            };

            DBUtilsOra.ExecuteNonQuerySP("CREATE_USER", listParameter, "CONN_KMESS");
        }

        public void reset_password(string username, string hashpass, string salt)
        {
            List<OracleParameter> listParameter = new List<OracleParameter>() {
                new OracleParameter("PARAM", username),
                new OracleParameter("PARAM", hashpass),
                new OracleParameter("PARAM", salt),
            };

            DBUtilsOra.ExecuteNonQuerySP("SET_PASSWORD", listParameter, "CONN_KMESS");
        }

        public void update_message_user(long message_id, int user_id, DateTime date_update, int status)
        {
            List<OracleParameter> listParameter = new List<OracleParameter>() {
                new OracleParameter("PARAM", message_id),
                new OracleParameter("PARAM", user_id),
                new OracleParameter("PARAM", date_update),
                new OracleParameter("PARAM", status),
            };

            DBUtilsOra.ExecuteNonQuerySP("UPDATE_MESSAGE_USER", listParameter, "CONN_KMESS");
        }

        public Message[] get_new_message(string username)
        {
            OracleParameter pResult = new OracleParameter("RESULT", OracleDbType.RefCursor, ParameterDirection.ReturnValue);
            var listParameter = new List<OracleParameter>() {
                pResult,
                new OracleParameter("PARAM", username)
            };

            return DBUtilsOra.ExecuteSPList<Message>("GET_NEW_MESSAGE", listParameter, "CONN_KMESS").ToArray();
        }
    }
}