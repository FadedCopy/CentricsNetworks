using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;

namespace Centrics.Models
{
    public class CentricsContext
    {
        public string ConnectionString { get; set; }

        public CentricsContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public void AddServiceReport(ServiceReport model)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                //clientemail?
                string AddQuery = "insert into centrics.servicereport(clientcompanyname,clientaddress,clienttel,clientcontactperson,purposeofvisit,description,remarks,date,timestart,timeend,mshused,attendedbystaffname,attendedondate,jobstatus) values (@clientcompanyname,@clientaddress,@clienttel,@clientcontactperson,@purposeofvisit,@description,@remarks,@date,@timestart,@timeend,@mshused,@attendedbystaffname,@attendondate,@jobstatus)";
                MySqlCommand c = new MySqlCommand(AddQuery, conn);

                c.Parameters.AddWithValue("@clientcompanyname", model.ClientCompanyName);
                c.Parameters.AddWithValue("@clientaddress", model.ClientAddress);
                c.Parameters.AddWithValue("@clienttel", model.ClientTel);
                c.Parameters.AddWithValue("@clientcontactperson", model.ClientContactPerson);
                c.Parameters.AddWithValue("@purposeofvisit", model.PurposeOfVisit);
                c.Parameters.AddWithValue("@description", model.Description);
                c.Parameters.AddWithValue("@remarks", model.Remarks);
                c.Parameters.AddWithValue("@date", model.Date);
                c.Parameters.AddWithValue("@timestart", model.TimeStart);
                c.Parameters.AddWithValue("@timeend", model.TimeEnd);
                c.Parameters.AddWithValue("@mshused", model.MSHUsed);
                c.Parameters.AddWithValue("@attendedbystaffname", model.AttendedByStaffName);
                c.Parameters.AddWithValue("@attendedondate", model.AttendedOnDate);

                c.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
        }
        //boolean this maybe? question this
        public void AddClient(Client model)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string AddQuery = "insert into client(clientcompanyname,clientaddress,clienttel,clientcontactperson,clientemailaddress) values (@clientcompanyname,@clientaddress,@clienttel,@clientcontactperson,@clientemailaddress)";
                MySqlCommand c = new MySqlCommand(AddQuery, conn);

                c.Parameters.AddWithValue("@clientcompanyname", model.ClientCompanyName);
                c.Parameters.AddWithValue("@clientaddress", model.ClientAddress);
                c.Parameters.AddWithValue("@clienttel", model.ClientTel);
                c.Parameters.AddWithValue("@clientcontactperson", model.ClientContactPerson);
                c.Parameters.AddWithValue("@clientemailaddress", model.ClientEmailAddress);

                c.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
        }

        public List<SelectListItem> GetClientByContract()
        {
            List<SelectListItem> listOfClient = new List<SelectListItem>();
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string Query = "select distinct companyname from contract where endvalid > @today";
                MySqlCommand c = new MySqlCommand(Query, conn);
                c.Parameters.AddWithValue("@today", " ' " + DateTime.Today + " ' " );

                using (MySqlDataReader r = c.ExecuteReader() )
                {
                    while (r.Read())
                    {
                        string text = r["companyname"].ToString();
                        listOfClient.Add(new SelectListItem{ Text = text, Value = text});
                    }
                }
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
            return listOfClient;
        }

        public void AddContract(Contract model)
        {
            
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string AddQuery = "insert into centrics.contract (companyname, msh, startvalid, endvalid) values (@companyname,@msh,@startvalid,@endvalid)";
                MySqlCommand c = new MySqlCommand(AddQuery, conn);

                c.Parameters.AddWithValue("@companyname", model.ClientCompany);
                c.Parameters.AddWithValue("@msh", model.MSH);
                c.Parameters.AddWithValue("@startvalid", model.StartValid);
                c.Parameters.AddWithValue("@endvalid", model.EndValid);

                c.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
        }

        public List<Contract> getContracts()
        {
            DateTime today = DateTime.Now;
            List<Contract> ExpiringContracts = new List<Contract>();
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string Query = "select * from contract";
                MySqlCommand c = new MySqlCommand(Query, conn);
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Contract dummy = new Contract
                        {
                            ClientCompany = r["clientcompany"].ToString(),
                            MSH = double.Parse(r["msh"].ToString()),
                            StartValid = DateTime.Parse(r["startvalid"].ToString()),
                            EndValid = DateTime.Parse(r["endvalid"].ToString())
                        };
                        ExpiringContracts.Add(dummy);
                    }
                }
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
            return ExpiringContracts;
        }

        public Contract getContract(Contract model)
        {
            MySqlConnection conn = GetConnection();
            Contract dummy = new Contract();
            try
            {
                conn.Open();
                string Query = "select * from contract where companyname =@companyname, msh = @msh, startvalid = @startvalid, endvalid = @endvalid";

                MySqlCommand c = new MySqlCommand(Query, conn);
                c.Parameters.AddWithValue("@companyname", model.ClientCompany);
                c.Parameters.AddWithValue("@msh", model.MSH);
                c.Parameters.AddWithValue("@startvalid", model.StartValid);
                c.Parameters.AddWithValue("@endvalid", model.EndValid);

                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        dummy = new Contract
                        {
                            ClientCompany = r["clientcompany"].ToString(),
                            MSH = double.Parse(r["msh"].ToString()),
                            StartValid = DateTime.Parse(r["startvalid"].ToString()),
                            EndValid = DateTime.Parse(r["endvalid"].ToString())
                        };
                    }
                }
            }

            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
            return dummy;
        }

        public string HashPassword(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            string hashedPassword = Convert.ToBase64String(hashBytes);

            return hashedPassword;
        }

        public Boolean CompareHashedPasswords(string loginPassword, string hashedPassword)
        {
            Boolean passwordMatch = false;

            // Extract the bytes 
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            // Get the salt 
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            // Compute the hash on the password the user entered 
            var pbkdf2 = new Rfc2898DeriveBytes(loginPassword, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            // Compare the results
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    passwordMatch = false;
                else if (hashBytes[i + 16] == hash[i])
                    passwordMatch = true;
             
            return passwordMatch;
        }

        public Boolean CheckExistingEmail(User user)
        {
            Boolean validEmail = false;
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string checkQuery = "select * from users where email = @registrationEmail";
                MySqlCommand c = new MySqlCommand(checkQuery, conn);
                c.Parameters.AddWithValue("@registrationEmail", user.UserEmail);

                using (MySqlDataReader r = c.ExecuteReader())
                {
                    if (r.Read())
                    {
                        validEmail = false;
                    }
                    else
                    {
                        validEmail = true;
                    }
                }
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            conn.Close();

            return validEmail;
        }

        public Boolean LoginUser(LoginViewModel user)
        {
            MySqlConnection conn = GetConnection();
            Boolean matchPassword = false;
            try
            {
                conn.Open();
                string query = "select * from users where email = @email";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@email", user.UserEmail);

                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        matchPassword = CompareHashedPasswords(user.UserPassword, r["password"].ToString());
                    }
                }
            } catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }

            return matchPassword;                
        }

        public void RegisterUser(User model)
        {
            Boolean validEmail = CheckExistingEmail(model);

            if (validEmail)
            {
                MySqlConnection conn = GetConnection();
                try
                {
                    conn.Open();
                    string AddQuery = "insert into users(firstName, lastName,  email, password, userRole) values (@firstName, @lastName, @email, @password, @role)";
                    MySqlCommand c = new MySqlCommand(AddQuery, conn);
                    string hashedPassword = HashPassword(model.UserPassword);
                    c.Parameters.AddWithValue("@firstName", model.FirstName);
                    c.Parameters.AddWithValue("@lastName", model.LastName);
                    c.Parameters.AddWithValue("@email", model.UserEmail);
                    c.Parameters.AddWithValue("@password", hashedPassword);
                    c.Parameters.AddWithValue("@role", model.UserRole);

                    c.ExecuteNonQuery();
                }
                catch (MySqlException e)
                {
                    Debug.WriteLine(e);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public List<User> getUsers()
        {
            //Initiaize List of Users to return
            List<User> ListofUsers = new List<User>();

            //Establish connection to MySQL Database
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "select * from users";
                MySqlCommand c = new MySqlCommand(query, conn);
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    //Loops for every row of the users table
                    while (r.Read())
                    {
                        User user = new User
                        {
                            UserID = Convert.ToInt32(r["userID"]),
                            FirstName = r["firstName"].ToString(),
                            LastName = r["lastName"].ToString(),
                            UserEmail = r["email"].ToString(),
                            UserRole = r["userRole"].ToString()
                        };
                        ListofUsers.Add(user);
                    }
                }
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
            return ListofUsers;
        }
    }
}
