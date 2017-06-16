using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Reflection;
using Oracle.ManagedDataAccess.Types;
using System.IO;
using System.ComponentModel;

namespace HCVN.Helpers
{
    public static class DBUtilsOra
    {
        private static string get_connection_string(string connect_name)
        {
            return ConfigurationManager.ConnectionStrings[connect_name].ConnectionString;
        }

        public static DataSet ExecuteSPDataTable(string pSPName, List<OracleParameter> pParameters, string container_name = null)
        {
            OracleConnection conn = new OracleConnection(get_connection_string(container_name));
            OracleCommand cmd = new OracleCommand(pSPName, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            if (pParameters != null)
            {
                foreach (OracleParameter param in pParameters)
                {
                    cmd.Parameters.Add(param);
                }
            }

            if (cmd.Connection.State == ConnectionState.Open)
            {
                cmd.Connection.Close();
            }
            cmd.Connection.Open();

            OracleDataAdapter da = new OracleDataAdapter(cmd);
            OracleCommandBuilder cb = new OracleCommandBuilder(da);
            DataSet ds = new DataSet();

            da.Fill(ds);

            if (cmd.Connection.State == ConnectionState.Open)
            {
                cmd.Connection.Close();
            }
            cmd.Dispose();

            return ds;
        }

        /// <summary>
        /// Execute a query command such as select statement
        /// </summary>
        /// <param name="pQueryString"></param>
        /// <param name="pParams"></param>
        /// <returns></returns>
        public static DataSet ExecuteQueryDataTable(string pQueryString, List<OracleParameter> pParameters, string container_name = null)
        {
            OracleConnection conn = new OracleConnection(get_connection_string(container_name));
            OracleCommand cmd = new OracleCommand(pQueryString, conn);
            cmd.CommandType = CommandType.Text;

            if (pParameters != null)
            {
                foreach (OracleParameter param in pParameters)
                {
                    cmd.Parameters.Add(param);
                }
            }

            if (cmd.Connection.State == ConnectionState.Open)
            {
                cmd.Connection.Close();
            }
            cmd.Connection.Open();

            OracleDataAdapter da = new OracleDataAdapter(cmd);
            OracleCommandBuilder cb = new OracleCommandBuilder(da);
            DataSet ds = new DataSet();

            da.Fill(ds);

            if (cmd.Connection.State == ConnectionState.Open)
            {
                cmd.Connection.Close();
            }
            cmd.Dispose();

            return ds;
        }

        /// <summary>
        /// Execute a stored procedure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pSPName"></param>
        /// <param name="pParams"></param>
        /// <returns></returns>
        public static List<T> ExecuteSPList<T>(string pSPName, List<OracleParameter> pParameters, string container_name = null) where T : new()
        {
            DataSet ds = ExecuteSPDataTable(pSPName, pParameters, container_name);
            DataTable dt = ds.Tables[0];

            return ConvertTo<T>(dt);
        }
        /// <summary>
        /// Execute a query command such as select statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pQueryString"></param>
        /// <param name="pParams"></param>
        /// <returns></returns>
        public static List<T> ExecuteQueryList<T>(string pQueryString, List<OracleParameter> pParameters, string container_name = null) where T : new()
        {
            DataSet ds = ExecuteQueryDataTable(pQueryString, pParameters, container_name);
            DataTable dt = ds.Tables[0];

            return ConvertTo<T>(dt);
        }
        /// <summary>
        /// Convert data table to list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datatable"></param>
        /// <returns></returns>
        public static List<T> ConvertTo<T>(DataTable datatable) where T : new()
        {
            return datatable.AsEnumerable().ToList().ConvertAll<T>(row => (T)MapDataRow(typeof(T), row));
        }

        public static object MapDataRow(Type t, DataRow row)
        {
            object obj = Activator.CreateInstance(t);
            bool hasValue = false;
            foreach (PropertyInfo prop in t.GetProperties())
            {
                if (row.Table.Columns.Contains(prop.Name))
                {
                    Type safetype = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    object safeValue = row[prop.Name] == null || Convert.IsDBNull(row[prop.Name]) ? 
                        null : 
                        safetype == typeof(Guid) ?
                            Activator.CreateInstance(prop.PropertyType, row[prop.Name]) :
                            Convert.ChangeType(row[prop.Name], safetype);
                    
                    if (safeValue != null)
                    {
                        hasValue = true;
                        prop.SetValue(obj, safeValue, null);
                    }
                }
                else if (!prop.PropertyType.FullName.StartsWith("System."))
                {
                    object child = MapDataRow(prop.PropertyType, row);
                    if (child != null)
                    {
                        hasValue = true;
                        prop.SetValue(obj, child, null);
                    }
                }
            }
            return hasValue ? obj : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="columnsName"></param>
        /// <returns></returns>
        public static T getObject<T>(DataRow row, List<string> columnsName) where T : new()
        {
            T obj = new T();
            try
            {
                string columnname = "";
                string value = "";
                PropertyInfo[] Properties;
                Properties = typeof(T).GetProperties();
                foreach (PropertyInfo objProperty in Properties)
                {
                    columnname = columnsName.Find(name => name.ToLower() == objProperty.Name.ToLower());
                    if (!string.IsNullOrEmpty(columnname))
                    {
                        if (objProperty.PropertyType.Name == "Guid")
                        {
                            value = new Guid(row[columnname].ToString()).ToString();
                        }
                        else
                        {
                            value = row[columnname].ToString();
                        }

                        if (!string.IsNullOrEmpty(value))
                        {
                            if (Nullable.GetUnderlyingType(objProperty.PropertyType) != null)
                            {
                                if (objProperty.PropertyType.Name == "Guid")
                                {
                                    value = new Guid(row[columnname].ToString()).ToString().Replace("$", "").Replace(",", "");
                                    objProperty.SetValue(obj, new Guid(row[columnname].ToString()), null);
                                }
                                else
                                {
                                    value = row[columnname].ToString().Replace("$", "").Replace(",", "");
                                    objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(Nullable.GetUnderlyingType(objProperty.PropertyType).ToString())), null);
                                }
                            }
                            else
                            {
                                if (objProperty.PropertyType.Name == "Guid")
                                {
                                    value = new Guid(row[columnname].ToString()).ToString().Replace("%", "");
                                    objProperty.SetValue(obj, new Guid(row[columnname].ToString()), null);
                                }
                                else
                                {
                                    value = row[columnname].ToString().Replace("%", "");
                                    objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(objProperty.PropertyType.ToString())), null);
                                }
                            }
                        }
                    }
                }
                return obj;
            }
            catch
            {
                return obj;
            }
        }

        public static int ExecuteNonQuerySP(string pSPName, List<OracleParameter> pParams, string container_name = null)
        {
            int result = 0;
            OracleConnection conn = new OracleConnection(get_connection_string(container_name));
            OracleCommand cmd = new OracleCommand(pSPName, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            //add parameters
            if (pParams != null)
            {
                foreach (OracleParameter param in pParams)
                {
                    cmd.Parameters.Add(param);
                }
            }

            cmd.Connection.Open();
            result = cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            cmd.Dispose();
            return result;
        }

        public static int ExecuteNonQuery(string pQueryString, string container_name = null)
        {
            int result = 0;
            OracleConnection conn = new OracleConnection(get_connection_string(container_name));
            OracleCommand cmd = new OracleCommand(pQueryString, conn);
            cmd.CommandType = CommandType.Text;

            cmd.Connection.Open();
            result = cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            cmd.Dispose();
            return result;
        }

        /// <summary>
        /// Converts a DataTable to a list with generic objects
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="table">DataTable</param>
        /// <returns>List with generic objects</returns>
        public static List<T> DataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        public static DataTable ConvertTo<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(values);
            }
            return table;
        } 

    }
}
