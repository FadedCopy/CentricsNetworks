using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;
using FluentEmail.Core;
using FluentEmail.Razor;

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
                //change the attendedondate to maybe signed date? for clarity cuz now 2 dates?
                // or remove and manually add lol
                string AddQuery = "insert into centrics.servicereport(clientcompanyname,clientaddress,clienttel,clientcontactperson,purposeofvisit,description,remarks,date,timestart,timeend,mshused,attendedbystaffname,attendedondate,jobstatus,daterecorded,reportstatus) values (@clientcompanyname,@clientaddress,@clienttel,@clientcontactperson,@purposeofvisit,@description,@remarks,@date,@timestart,@timeend,@mshused,@attendedbystaffname,@attendondate,@jobstatus,@daterecorded,@reportstatus)";
                MySqlCommand c = new MySqlCommand(AddQuery, conn);

                //combining values purposeofvisit array
                string[] listpurpose = model.PurposeOfVisit;
                string combinedpurpose = "";
                if (model.PurposeOfVisit.Length > 1) {
                    for (int i = 0; i < listpurpose.Length; i++)
                    {
                        if (listpurpose[i] != listpurpose.Last())
                        {
                            combinedpurpose += listpurpose[i] + ",";
                        }
                        else
                        {
                            combinedpurpose += listpurpose[i];
                        }
                    }
                }else if(model.PurposeOfVisit.Length == 1)
                {
                    combinedpurpose = model.PurposeOfVisit[0];
                }
     
                String jobcombined = "";
                if(model.JobStatus.Length > 1)
                {                   
                    for(int i = 0; i< model.JobStatus.Length; i++)
                    {
                        if (model.JobStatus[i] != model.JobStatus.Last()) {
                            jobcombined += model.JobStatus[i] + ",";
                        }
                        else {
                            jobcombined += model.JobStatus[i];
                        }

                    }
                    
                }else if(model.JobStatus.Length == 1)
                {
                    jobcombined = model.JobStatus[0];
                }


                c.Parameters.AddWithValue("@clientcompanyname", model.ClientCompanyName);
                c.Parameters.AddWithValue("@clientaddress", model.ClientAddress);
                c.Parameters.AddWithValue("@clienttel", model.ClientTel);
                c.Parameters.AddWithValue("@clientcontactperson", model.ClientContactPerson);
                c.Parameters.AddWithValue("@purposeofvisit", combinedpurpose);
                c.Parameters.AddWithValue("@description", model.Description);
                c.Parameters.AddWithValue("@remarks", model.Remarks);
                c.Parameters.AddWithValue("@date", model.Date);
                c.Parameters.AddWithValue("@timestart", model.TimeStart);
                c.Parameters.AddWithValue("@timeend", model.TimeEnd);
                c.Parameters.AddWithValue("@mshused", model.MSHUsed);
                c.Parameters.AddWithValue("@attendedbystaffname", model.AttendedByStaffName);
                c.Parameters.AddWithValue("@attendondate", model.AttendedOnDate);
                c.Parameters.AddWithValue("@jobstatus",jobcombined);
                c.Parameters.AddWithValue("@daterecorded",DateTime.Now);
                //wait for integration remember
                //c.Parameters.AddWithValue("@reportfrom", model.ReportFrom);
                c.Parameters.AddWithValue("@reportstatus", "Pending");

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

        public double GetRemainingMSHByCompany(ServiceReport model)
        {
            double totalmsh = 0.0;
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string Query = "select * from centrics.contract where endvalid > @today and startvalid < @tod and companyname = @companyname ";
                MySqlCommand c = new MySqlCommand(Query, conn);
                string todau = DateTime.Today.ToString("yyyy/MM/dd");
                c.Parameters.AddWithValue("@companyname", model.ClientCompanyName);
                c.Parameters.AddWithValue("@today", todau);
                c.Parameters.AddWithValue("@tod", todau);
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        totalmsh += int.Parse(r["msh"].ToString());
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
            return totalmsh;
        }

        public List<SelectListItem> GetClientByContract()
        {
            List<SelectListItem> listOfClient = new List<SelectListItem>();
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string Query = "select distinct companyname from centrics.contract where endvalid > @today and startvalid < @tod ";
                MySqlCommand c = new MySqlCommand(Query, conn);
                string todau = DateTime.Today.ToString("yyyy/MM/dd");
                Debug.WriteLine(todau + " que?");
                Debug.WriteLine("'" + todau + "'");
                c.Parameters.AddWithValue("@today",todau);
                c.Parameters.AddWithValue("@tod",todau);
                using (MySqlDataReader r = c.ExecuteReader() )
                {
                    while (r.Read())
                    {
                        string text = r["companyname"].ToString();
                        listOfClient.Add(new SelectListItem() { Value = text, Text = text});
                        
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
                            idcontract = int.Parse(r["idcontract"].ToString()),
                            ClientCompany = r["companyname"].ToString(),
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

        internal object GeneratePdf()
        {
            throw new NotImplementedException();
        }

        public void SubtractRemains(double remains, ServiceReport model)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                Contract cont = GetLatestContractBySRModel(model);
                conn.Open();
                string Query = "update contract set msh = @msh where idcontract = @idcontract";
                MySqlCommand c = new MySqlCommand(Query, conn);

                c.Parameters.AddWithValue("@msh", cont.MSH - remains);
                c.Parameters.AddWithValue("@idcontract", cont.idcontract);

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

        public double SubtractMSHUsingSR(ServiceReport model)
        {
            double remains = 0.0;
            MySqlConnection conn = GetConnection();
            try
            {
                Contract cont = GetLatestContractBySRModel(model);
                conn.Open();
                string Query = "update contract set msh = @msh where idcontract = @idcontract";
                MySqlCommand c = new MySqlCommand(Query, conn);
                
                if (cont.MSH - model.MSHUsed < 0)
                {
                    c.Parameters.AddWithValue("@msh",cont.MSH - cont.MSH);
                     remains = model.MSHUsed - cont.MSH;
                }
                else
                {
                    c.Parameters.AddWithValue("@msh",cont.MSH - model.MSHUsed);
                    remains = 0;
                }

                
                c.Parameters.AddWithValue("@idcontract", cont.idcontract);

                c.ExecuteNonQuery();
                return remains;
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
            return remains;
        }

        public void AddBilling(ServiceReport model) {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string AddQuery = "update centrics.servicereport set labour = @labour,transport =@transport,parts = @parts, others= @others, invoiceno = @invoiceno, invoiceDate = @invoicedate where id = @id";
                
                MySqlCommand c = new MySqlCommand(AddQuery, conn);

                c.Parameters.AddWithValue("@labour", model.Labour);
                c.Parameters.AddWithValue("@transport", model.Transport);
                c.Parameters.AddWithValue("@parts", model.Parts);
                c.Parameters.AddWithValue("@others", model.Others);
                c.Parameters.AddWithValue("@invoiceno", model.InvoiceNo);
                c.Parameters.AddWithValue("@invoicedate", model.InvoiceDate);
                c.Parameters.AddWithValue("@id", model.SerialNumber);

                Debug.WriteLine("before me is king");
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

        public Contract GetLatestContractBySRModel(ServiceReport model)
        {
            MySqlConnection conn = GetConnection();
            List<Contract> listcont = new List<Contract>();
            try
            {
                conn.Open();
                string Query = "select * from contract where companyname = @companyname";

                MySqlCommand c = new MySqlCommand(Query, conn);
                c.Parameters.AddWithValue("@companyname", model.ClientCompanyName);

                
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        listcont.Add(new Contract
                        {
                            idcontract = int.Parse(r["idcontract"].ToString()),
                            ClientCompany = r["companyname"].ToString(),
                            MSH = double.Parse(r["msh"].ToString()),
                            StartValid = DateTime.Parse(r["startvalid"].ToString()),
                            EndValid = DateTime.Parse(r["endvalid"].ToString())

                        });
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
            Contract returner = new Contract();
            List<Contract> contracts = new List<Contract>();
            if (listcont.Count() > 1)
            {
                for(int i = 0; i < listcont.Count(); i++)
                {
                    if (listcont[i].StartValid.CompareTo(DateTime.Today) < 0 && listcont[i].EndValid.CompareTo(DateTime.Today)> 0)
                    {
                        if (listcont[i].MSH > 0)
                        {
                            contracts.Add(listcont[i]);
                        }
                        
                    }
                }
                DateTime lowest = contracts[0].EndValid;
                DateTime thisone = new DateTime();
                for (int i = 0; i < contracts.Count(); i++)
                {
                    thisone = contracts[i].EndValid;
                    if (thisone.CompareTo(lowest) < 0)
                    {
                        lowest = thisone;
                    }
                }
                for (int i = 0; i < contracts.Count(); i++)
                {
                    if (contracts[i].EndValid == lowest)
                    {
                        returner = contracts[i];
                    }
                }
            }else if(listcont.Count() == 1)
            {
                if (listcont[0].StartValid.CompareTo(DateTime.Today) < 0 && listcont[0].EndValid.CompareTo(DateTime.Today) > 0)
                {
                    returner = listcont[0];
                }
                //if empty do the check at service report pulling data for the companies
            }
            Debug.WriteLine(returner.EndValid);
            return returner;
        }

        public Contract getContract(int idcontract)
        {
            MySqlConnection conn = GetConnection();
            Contract dummy = new Contract();
            try
            {
                conn.Open();
                string Query = "select * from contract where idcontract= @idcontract";

                MySqlCommand c = new MySqlCommand(Query, conn);
                c.Parameters.AddWithValue("@idcontract", idcontract);
                
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        dummy = new Contract
                        {
                            idcontract = int.Parse(r["idcontract"].ToString()),
                            ClientCompany = r["companyname"].ToString(),
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
        
        public void ModifyContract(Contract model) {

            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "update centrics.contract set endvalid=@endvalid,msh=@msh where idcontract =@idcontract";
                MySqlCommand c = new MySqlCommand(query, conn);
                Debug.WriteLine("this is msh: " + model.MSH + ";" + model.EndValid + ";" + model.idcontract );
                c.Parameters.AddWithValue("@endvalid", model.EndValid);
                c.Parameters.AddWithValue("@msh", model.MSH);
                c.Parameters.AddWithValue("@idcontract",model.idcontract);

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

        public List<ServiceReport> getPendingReports()
        {
            List <ServiceReport> getReports = new List<ServiceReport>();
            MySqlConnection conn = GetConnection();
            try {
                conn.Open();
                string query = "select * from centrics.servicereport where reportstatus ='Pending' order by daterecorded desc";

                MySqlCommand c = new MySqlCommand(query, conn);

                using (MySqlDataReader r = c.ExecuteReader()){
                    while (r.Read())
                    {
                        getReports.Add(new ServiceReport
                        {
                            SerialNumber = int.Parse(r["id"].ToString()),
                            ClientCompanyName = r["clientcompanyname"].ToString(),
                            ClientAddress = r["clientaddress"].ToString(),
                            Description = r["description"].ToString(),
                            JobStat = r["jobstatus"].ToString()                            
                        });
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
            return getReports;
        }

        public List<ServiceReport> getConfirmedReports()
        {
            List<ServiceReport> getReports = new List<ServiceReport>();
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "select * from centrics.servicereport where reportstatus ='Confirmed' order by daterecorded desc";

                MySqlCommand c = new MySqlCommand(query, conn);

                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        getReports.Add(new ServiceReport
                        {
                            SerialNumber = int.Parse(r["id"].ToString()),
                            ClientCompanyName = r["clientcompanyname"].ToString(),
                            ClientAddress = r["clientaddress"].ToString(),
                            Description = r["description"].ToString(),
                            JobStat = r["jobstatus"].ToString()
                        });
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
            return getReports;
        }

        public int getReportCounts()
        {
            MySqlConnection conn = GetConnection();
            int count = 0;
            try
            {
                conn.Open();
                string query = "select count(*) as count from centrics.servicereport";

                MySqlCommand c = new MySqlCommand(query, conn);

                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        count = int.Parse(r["count"].ToString());
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
            return count;
        }
        
        public void SendEmailRegardingExpiry()
        {
            Email.DefaultRenderer = new RazorRenderer();

            var template = "Dear @Model.Company"+"<br/>"+"Your Company, @Model.Company, Service Contract with Centrics Network has is expiring soon." + "<br/>"
                + "The Contract Details are as follows: <br/> Company Name: @Model.Company <br/> Start of Validity: @Model.StartValid <br/> End of Validity: @Model.EndValid <br/> Remaning Service Hours: @Model.Remains <br/>" + 
                "Please contact us at 999 if you want to continue using our service"+"<br/>This is an auto-generated. Do not reply this email." + "<br/><img src='http://centricsnetworks.com.sg/wp-content/uploads/2015/12/logo1.png'/>" ;

            var email = Email
                .From("ai.permacostt@gmail.com")
                .To("johnfoohw@gmail.com")
                .Subject("You can be king again")
                .UsingTemplate(template, new { Company = "KFC", StartValid = "1/4/2017", EndValid = "1/4/2018", Remains = "15", });

            Debug.WriteLine(email);
            email.Send();
        }

        //maybe status? Comfirmed or Pending for the print?
        public ServiceReport getServiceReport(int serial)
        {
            ServiceReport SR = new ServiceReport();
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                String query = "Select * from servicereport where id = @id";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@id",serial);
                
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        SR = new ServiceReport
                        {
                            SerialNumber = int.Parse(r["id"].ToString()),
                            ClientCompanyName = r["clientcompanyname"].ToString(),
                            ClientAddress = r["clientaddress"].ToString(),
                            ClientTel = int.Parse(r["clienttel"].ToString()),
                            ClientContactPerson = r["clientcontactperson"].ToString(),
                            PurposeOfVisit = r["purposeofvisit"].ToString().Split(','),
                            Description = r["description"].ToString(),
                            Remarks = r["remarks"].ToString(),
                            Date = DateTime.Parse(r["date"].ToString()),
                            TimeStart = DateTime.Parse(r["timestart"].ToString()),
                            TimeEnd = DateTime.Parse(r["timeend"].ToString()),
                            MSHUsed = double.Parse(r["mshused"].ToString()),
                            AttendedByStaffName = r["mshused"].ToString(),
                            AttendedOnDate = DateTime.Parse(r["attendedondate"].ToString()),
                            JobStatus = r["jobstatus"].ToString().Split(','),
                            ReportStatus = r["reportstatus"].ToString()
                            
                        };
                        if (!DBNull.Value.Equals(r["labour"]))
                        {
                            SR.Labour = Double.Parse(r["labour"].ToString());
                        }
                        if (!DBNull.Value.Equals(r["transport"]))
                        {
                            SR.Transport = Double.Parse(r["transport"].ToString());
                        }
                        if (!DBNull.Value.Equals(r["parts"]))
                        {
                            SR.Parts = Double.Parse(r["parts"].ToString());
                        }
                        if (!DBNull.Value.Equals(r["others"]))
                        {
                            SR.Others = Double.Parse(r["others"].ToString());
                        }
                        if (!DBNull.Value.Equals(r["invoiceno"]))
                        {
                            SR.InvoiceNo = int.Parse(r["invoiceno"].ToString());
                        }
                        if (!DBNull.Value.Equals(r["invoicedate"]))
                        {
                            SR.InvoiceDate = DateTime.Parse(r["invoicedate"].ToString());
                        }

                    }
                };
                String jobcombined = "";
                if (SR.JobStatus.Length > 1)
                {
                    for (int i = 0; i < SR.JobStatus.Length; i++)
                    {
                        if (SR.JobStatus[i] != SR.JobStatus.Last())
                        {
                            jobcombined += SR.JobStatus[i] + ",";
                        }
                        else
                        {
                            jobcombined += SR.JobStatus[i];
                        }

                    }

                }
                else if (SR.JobStatus.Length == 1)
                {
                    jobcombined = SR.JobStatus[0];
                }
                SR.JobStat = jobcombined;

                string[] listpurpose = SR.PurposeOfVisit;
                string combinedpurpose = "";
                if (SR.PurposeOfVisit.Length > 1)
                {
                    for (int i = 0; i < listpurpose.Length; i++)
                    {
                        if (listpurpose[i] != listpurpose.Last())
                        {
                            combinedpurpose += listpurpose[i] + ",";
                        }
                        else
                        {
                            combinedpurpose += listpurpose[i];
                        }
                    }
                }
                else if (SR.PurposeOfVisit.Length == 1)
                {
                    combinedpurpose = SR.PurposeOfVisit[0];
                }
                SR.Purpose = combinedpurpose;
                

            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }

            return SR;
        }
        
        public void ReportEdit(ServiceReport model)
        {
            MySqlConnection conn = GetConnection();

            try
            {
                conn.Open();
                //missing query
                string query = "Update centrics.servicereport set clienttel = @clienttel, clientcontactperson = @clientcontactperson, purposeofvisit = @purposeofvisit, description = @description, remarks = @remarks, date= @date, timestart=@timestart, timeend = @timeend,mshused = @mshused, attendedbystaffname = @attendedbystaffname, attendedondate = @attendedondate, jobstatus = @jobstatus where id = @id";

                MySqlCommand c = new MySqlCommand(query, conn);

                Debug.WriteLine("this is a item: " + model.AttendedOnDate);

                string[] listpurpose = model.PurposeOfVisit;
                string combinedpurpose = "";
                if (model.PurposeOfVisit.Length > 1)
                {
                    for (int i = 0; i < listpurpose.Length; i++)
                    {
                        if (listpurpose[i] != listpurpose.Last())
                        {
                            combinedpurpose += listpurpose[i] + ",";
                        }
                        else
                        {
                            combinedpurpose += listpurpose[i];
                        }
                    }
                }
                else if (model.PurposeOfVisit.Length == 1)
                {
                    combinedpurpose = model.PurposeOfVisit[0];
                }

                String jobcombined = "";
                if (model.JobStatus.Length > 1)
                {
                    for (int i = 0; i < model.JobStatus.Length; i++)
                    {
                        if (model.JobStatus[i] != model.JobStatus.Last())
                        {
                            jobcombined += model.JobStatus[i] + ",";
                        }
                        else
                        {
                            jobcombined += model.JobStatus[i];
                        }

                    }

                }
                else if (model.JobStatus.Length == 1)
                {
                    jobcombined = model.JobStatus[0];
                }

                c.Parameters.AddWithValue("@id",model.SerialNumber);
                c.Parameters.AddWithValue("@clienttel",model.ClientTel);
                c.Parameters.AddWithValue("@clientcontactperson",model.ClientContactPerson);
                c.Parameters.AddWithValue("@purposeofvisit", combinedpurpose);
                c.Parameters.AddWithValue("@description", model.Description);
                c.Parameters.AddWithValue("@remarks", model.Remarks);
                c.Parameters.AddWithValue("@date", model.Date);
                c.Parameters.AddWithValue("@timestart", model.TimeStart);
                c.Parameters.AddWithValue("@timeend", model.TimeEnd);
                c.Parameters.AddWithValue("@mshused", model.MSHUsed);
                c.Parameters.AddWithValue("@attendedbystaffname", model.AttendedByStaffName);
                c.Parameters.AddWithValue("@attendedondate", model.AttendedOnDate);
                c.Parameters.AddWithValue("@jobstatus", jobcombined);

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

        public void ReportConfirm(int id)
        {
            MySqlConnection conn = GetConnection();
            
            try
            {
                conn.Open();
                string query = "Update centrics.servicereport set reportstatus = @Confirmed where id = @id";

                MySqlCommand c = new MySqlCommand(query, conn);
                string confirmed = "Confirmed";
                c.Parameters.AddWithValue("@id",id);
                c.Parameters.AddWithValue("@Confirmed", confirmed);

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

        public Boolean CheckEditExistingEmail(EditUserViewModel user)
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
            }
            catch (MySqlException e)
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
        //Get a single user by UserID
        public User getUser(int UserID)
        {
            //Initialize User to place returned User object
            User userRetrieved = new User();

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
                        userRetrieved = new User()
                        {
                            UserID = Convert.ToInt32(r["userID"]),
                            FirstName = r["firstName"].ToString(),
                            LastName = r["lastName"].ToString(),
                            UserEmail = r["email"].ToString(),
                            UserRole = r["userRole"].ToString()
                        };
                    }
                    Debug.WriteLine(userRetrieved.UserID);
                    Debug.WriteLine(userRetrieved.FirstName);
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
            return userRetrieved;
        }

        //Get ALL users
        public List<User> getUsers()
        {
            //Initialize List of Users to return
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

        public void DeleteUser(int UserID)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "delete from users where userID = @UserID";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@UserID", UserID);
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

        public void EditUser(EditUserViewModel user, int UserID)
        {
            MySqlConnection conn = GetConnection();
            Debug.WriteLine("Edit" + UserID);
            Debug.WriteLine("edit" + user.FirstName);
            try
            {
                conn.Open();
                string query = "update users set firstName=@firstName, lastName=@lastName, email=@email, userRole=@role where userID=@userID";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@firstName", user.FirstName);
                c.Parameters.AddWithValue("@lastName", user.LastName);
                c.Parameters.AddWithValue("@email", user.UserEmail);
                c.Parameters.AddWithValue("@role", user.UserRole);
                c.Parameters.AddWithValue("@userID", UserID);

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

        public void ChangePassword(string NewPassword, User user)
        {

        }
    }
}
