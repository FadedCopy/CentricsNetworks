using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Data;

namespace Centrics.Models
{
    public class Database
    {
        public static Database CurrentInstance;

        public MySqlConnection conn;

        public static void Initialize(string connString)
        {
            CurrentInstance = new Database(connString);
        }

        public Database(string connString)
        {
            conn = new MySql.Data.MySqlClient.MySqlConnection(connString);
        }

        public DataTable PRQ(string query, params dynamic[] parameters)
        {
            if (OpenConnection())
            {
                MySqlCommand m = new MySqlCommand(query, conn);
                for (int i = 0; i < parameters.Length; i++)
                {
                    m.Parameters.AddWithValue("@" + (i + 1), parameters[i]);
                }
                MySqlDataReader r = m.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(r);

                CloseConnection();
                r.Close();

                return dt;
            }
            return null;
        }

        public void PNQ(string query, params dynamic[] parameters)
        {
            if (OpenConnection())
            {
                MySqlCommand m = new MySqlCommand(query, conn);
                for (int i = 0; i < parameters.Length; i++)
                {
                    m.Parameters.AddWithValue("@" + (i + 1), parameters[i]);
                }
                m.ExecuteNonQuery();
                CloseConnection();
            }
        }

        public bool Insert(string query)
        {
            if (this.OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
                return true;
            }
            return false;
        }

        public bool Delete(string query)
        {
            if (this.OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
                return true;
            }
            return false;
        }

        public bool OpenConnection()
        {
            try
            {
                conn.Open();
                return true;
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("MySql Connection Error!");
                Debug.WriteLine(e.StackTrace);
                return false;
            }
            catch (InvalidOperationException e) //connection already open
            {
                conn.Close();
                return OpenConnection();
            }
        }

        public bool CloseConnection()
        {
            try
            {
                conn.Close();
                return true;
            }
            catch (MySqlException e)
            {
                Debug.WriteLine("Could not close connection!");
                Debug.WriteLine(e.StackTrace);
                return false;
            }
        }
    }
}