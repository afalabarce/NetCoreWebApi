using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;

namespace NetCoreWebApi.Extensions
{
    public static class ExtensionsDAL
    {
        
        public static DataTable ExecuteSql<T>(this T connection, string sqlQuery) where T: DbConnection
        {
            try
            {                
                DbCommand cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sqlQuery;
                DataTable returnValue = new DataTable();

                if (connection is SqlConnection)
                    new SqlDataAdapter(cmd as SqlCommand).Fill(returnValue);
                else if (connection is NpgsqlConnection)
                    new NpgsqlDataAdapter(cmd as NpgsqlCommand).Fill(returnValue);
                else
                    new MySqlDataAdapter(cmd as MySqlCommand).Fill(returnValue);
                return returnValue;
            }
            catch
            {

            }

            return null;
        }

        // Kotlin: fun <T, R> T.let(block: (T) -> R): R
        public static R let<T, R>(this T self, Func<T, R> block)
        {
            return block(self);
        }

        // Kotlin: fun <T> T.also(block: (T) -> Unit): T
        public static T also<T>(this T self, Action<T> block)
        {
            block(self);
            return self;
        }
    }
}

