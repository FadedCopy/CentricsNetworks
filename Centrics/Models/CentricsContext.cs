﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using Hangfire;
using Hangfire.Storage;
using System.Data.SqlClient;

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
                
                string AddQuery = "insert into centrics.servicereport(clientcompanyname,clientaddress,clienttel,clientcontactperson,purposeofvisit,description,remarks,timestart,timeend,mshused,attendedbystaffname,attendedondate,jobstatus,daterecorded,reportstatus,reportfrom) values (@clientcompanyname,@clientaddress,@clienttel,@clientcontactperson,@purposeofvisit,@description,@remarks,@timestart,@timeend,@mshused,@attendedbystaffname,@attendondate,@jobstatus,@daterecorded,@reportstatus,@reportfrom)";
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
                ClientAddress cA = getOneClient(model.ClientCompanyName);

                c.Parameters.AddWithValue("@clientcompanyname", model.ClientCompanyName);
                c.Parameters.AddWithValue("@clientaddress", model.ClientAddress);
                if (cA.ContactNoList != null)
                {
                    int number;
                    
                    if (cA.ContactNoList[0] != null)
                    {
                        bool result = (Int32.TryParse(cA.ContactNoList[0], out number));
                        if (result)
                        {
                            c.Parameters.AddWithValue("@clienttel", number);

                        }
                        else
                        {
                            c.Parameters.AddWithValue("@clienttel", DBNull.Value);
                        }
                    }
                    
                    
                }
                else
                {
                    c.Parameters.AddWithValue("@clienttel", DBNull.Value);
                }
                if (cA.ContactList != null)
                {
                    c.Parameters.AddWithValue("@clientcontactperson", cA.ContactList[0]);
                }
                c.Parameters.AddWithValue("@purposeofvisit", combinedpurpose);
                c.Parameters.AddWithValue("@description", model.Description);
                c.Parameters.AddWithValue("@remarks", model.Remarks);
                c.Parameters.AddWithValue("@timestart", model.TimeStart);
                c.Parameters.AddWithValue("@timeend", model.TimeEnd);
                c.Parameters.AddWithValue("@mshused", CalculateMSH(model.TimeStart,model.TimeEnd));
                c.Parameters.AddWithValue("@attendedbystaffname", model.AttendedByStaffName);
                c.Parameters.AddWithValue("@attendondate", DateTime.Parse(model.TimeStart.ToShortDateString()));
                c.Parameters.AddWithValue("@jobstatus",jobcombined);
                c.Parameters.AddWithValue("@daterecorded",DateTime.Now);
                
                c.Parameters.AddWithValue("@reportfrom", model.ReportFrom);
                c.Parameters.AddWithValue("@reportstatus", "Pending");

                c.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
        }
        
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

        public SRCalculation GetMSHMultiplier()
        {
            SRCalculation rCalculation = new SRCalculation();
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string Query = "select * from centrics.mshmultipliers where row = 1";
                MySqlCommand c = new MySqlCommand(Query, conn);
                
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        rCalculation = new SRCalculation
                        {
                            After6pmto12am = Convert.ToDouble(r["after6"].ToString()),
                            From12amto9am = Convert.ToDouble(r["before9"].ToString()),
                            SaturdayMultiplier = Convert.ToDouble(r["sat"].ToString()),
                            SundayMultiplier = Convert.ToDouble(r["sun"].ToString())
                        };

                    }
                }
                return rCalculation;
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
            return null;
        }

        //msh multiplier settings
        public void UpdateMSHMultiplier(SRCalculation values)
        {

            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string UpdateQuery = "update centrics.mshmultipliers set before9 = @before9, after6 = @after6,sat = @sat,sun = @sun where row = 1";
                MySqlCommand c = new MySqlCommand(UpdateQuery, conn);

                c.Parameters.AddWithValue("@before9", values.From12amto9am);
                c.Parameters.AddWithValue("@after6", values.After6pmto12am);
                c.Parameters.AddWithValue("@sat", values.SaturdayMultiplier);
                c.Parameters.AddWithValue("@sun", values.SundayMultiplier);
                
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


        //get totalmsh of company
        public double GetRemainingMSHByCompany(ServiceReport model)
        {
            double totalmsh = 0.0;
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string Query = "select * from centrics.contract where endvalid >= @today and startvalid <= @tod and companyname = @companyname ";
                MySqlCommand c = new MySqlCommand(Query, conn);
                string todau = DateTime.Today.ToString("yyyy/MM/dd");
                c.Parameters.AddWithValue("@companyname", model.ClientCompanyName);
                c.Parameters.AddWithValue("@today", todau);
                c.Parameters.AddWithValue("@tod", todau);
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        totalmsh += Convert.ToDouble(r["msh"].ToString());
                        
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
                string Query = "select distinct companyname from centrics.contract where endvalid >= @today and startvalid <= @tod ";
                MySqlCommand c = new MySqlCommand(Query, conn);
                string todau = DateTime.Today.ToString("yyyy/MM/dd");

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
                string AddQuery = "insert into centrics.contract (companyname, msh, startvalid, endvalid, centricid,contracttype,email) values (@companyname,@msh,@startvalid,@endvalid,@centricid,@contracttype,@email)";
                MySqlCommand c = new MySqlCommand(AddQuery, conn);

                ClientAddress cA = getOneClient(model.ClientCompany);

                c.Parameters.AddWithValue("@companyname", model.ClientCompany);
                c.Parameters.AddWithValue("@msh", model.MSH);
                c.Parameters.AddWithValue("@startvalid", model.StartValid);
                c.Parameters.AddWithValue("@endvalid", model.EndValid);
                c.Parameters.AddWithValue("@centricid", cA.CustomerID);
                c.Parameters.AddWithValue("@contracttype", model.ContractType);
                string hi = getOneClient(model.ClientCompany).EmailList[0];
                if (!hi.Equals("")) {
                    c.Parameters.AddWithValue("@email",hi);
                }
                else
                {
                    c.Parameters.AddWithValue("@email", DBNull.Value);
                }
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
                            EndValid = DateTime.Parse(r["endvalid"].ToString()),
                            CentricID = int.Parse(r["centricid"].ToString()),
                            ContractType = r["contracttype"].ToString(),
                            Email = r["email"].ToString()
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

        //check for expiring contracts
        public void Emailcaller()
        {
            List<Contract> contracts = getContracts();
            int counter = contracts.Count;
            int i = 0;
            while (counter != 0)
            {
                if (contracts[i].EndValid >= DateTime.Now.Date)
                {
                    if ((contracts[i].EndValid.Date - DateTime.Now.Date).TotalDays == 1)
                    {
                        Emailsender(1, contracts[i]);
                    }else if((contracts[i].EndValid.Date - DateTime.Now.Date).TotalDays == 7)
                    {
                        Emailsender(7, contracts[i]);
                    }else if((contracts[i].EndValid.Date - DateTime.Now.Date).TotalDays == 30)
                    {
                        Emailsender(30, contracts[i]);
                    }
                }
                i++;
                counter--;
            }
        }
        
        public void Selfcaller()
        {
            // RecurringJob.AddOrUpdate(() => Emailcaller(), Cron.Daily());
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringjob in connection.GetRecurringJobs())
                {
                    Debug.WriteLine(recurringjob.Id);
                }
            }
            
            //RecurringJob.AddOrUpdate(() => Emailcaller(), Cron.MinuteInterval(2));
        }
        // both of this are to test scheduler
        public void callmemaybe()
        {
            //Emailsender(7, getContract(1));
            Debug.WriteLine(500 + 500 - 122 + "smh");
        }

        public void Emailsender(int days, Contract contract)
        {
            Debug.WriteLine("Go SPAM?");
            string lol = "soon";
            if(days == 1)
            {
                lol = "today";
            }else if (days == 7)
            {
                lol = "next week";
            }else if(days == 30)
            {
                lol = "next month";
            }

            
            SmtpClient client = new SmtpClient("outlook.centricsnetworks.com.sg");
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("crm@centricsnetworks.com.sg", "Password123");


            

            MailMessage mailMessage = new MailMessage();
            mailMessage = getMailWithImg(mailMessage,contract,lol);
            mailMessage.IsBodyHtml = true;
            
            mailMessage.From = new MailAddress("crm@centricsnetworks.com.sg");
            //mailMessage.To.Add("James_Ng@centricsnetworks.com.sg");
            //mailMessage.To.Add(contract.Email); //this should not work, the one below might?

            //mailMessage.To.Add("wenjie_lee@centricsnetworks.com.sg");
            mailMessage.To.Add("crm@centricsnetworks.com.sg");
            
            
            ClientAddress cA = GetClientAddressList(contract.ClientCompany);
            
            //if (cA.EmailList != null)
            //{
            //    mailMessage.To.Add(cA.EmailList[0]);
            //}

            
            //mailMessage.Body = "nobody has no body there nobody is nobody cuz nobody can see him <br /> thanks, from centrics";
            mailMessage.Subject = "Contract Expiry from Centrics Networks";
            client.Send(mailMessage);
            //Mailbox unavailable. The server response was: 5.7.54 SMTP; Unable to relay recipient in non-accepted domain. Need to fix
        }

        private MailMessage getMailWithImg(MailMessage mail, Contract contract, string lol)
        {
            mail.AlternateViews.Add(getEmbeddedImage(contract,lol,@"Images\logo1.png"));   
            return mail;
        }
        private AlternateView getEmbeddedImage(Contract contract,string lol,String filePath )
        {
            
            LinkedResource res = new LinkedResource(filePath, MediaTypeNames.Image.Jpeg);
            res.ContentId = Guid.NewGuid().ToString();
            
            string html = "Dear " + contract.ClientCompany + ", <br /> <br />"
                + "The purpose of this e-mail is to inform you about the contract that you have with Centrics Networks. <br />"
                + "<br/>" + "Contract Info: <br />"
                + "Your company: " + contract.ClientCompany + "<br/>"
                + "Start of Contract :" + contract.StartValid.ToShortDateString() + "<br/>"
                + "End of Contract: " + contract.EndValid.ToShortDateString() + "<br/>"
                + "Remaining Hours: " + contract.MSH
                + "<br/> <br/> " + "The contract stated above is set to expire in " + lol + "."
                + "<br/> <br/> " + "Please contact Centrics at 6833-7898."
                + "<br/> <br/> " + "This email is auto-generated. Please do not reply to this email."
                + "<br> <br/>" + " Warm Regards, <br/> Centrics <br /> <b>Centrics Networks Pte. Ltd.</b>"
                + "<br/>"+@"<img src='cid:" + res.ContentId + @"'/>"
                + "<br/>" + "26 Sin Ming Lane, #06-120 Midview City, Singapore (573971)"
                + "<br/>" + "Main: 6833 7898 | Fax: 6833 7897 " +
                "<br/>" + "Web: www.centricsnetworks.com.sg"
                ;
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(res);
            return alternateView;
        }

        //Haven't bind to Service report
        public void SREmail(ServiceReport SR)
        {
            SmtpClient client = new SmtpClient("outlook.centricsnetworks.com.sg")
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("crm@centricnetworks.com.sg", "Password123")
            };


            MailMessage mailMessage = new MailMessage();
            mailMessage = SRMailImage(mailMessage,SR);
            mailMessage.IsBodyHtml = true;

            mailMessage.From = new MailAddress("crm@centricsnetworks.com.sg");
            //mailMessage.To.Add("James_ng@centricsnetworks.com.sg");
            
            ClientAddress cA = GetClientAddressList(SR.ClientCompanyName);
            //if (cA.EmailList != null)
            //{
            //    mailMessage.To.Add(cA.EmailList[0]);
            //}

            //mailMessage.To.Add(contract.Email);
            //mailMessage.Body = "nobody has no body there nobody is nobody cuz nobody can see him <br /> thanks, from centrics";
            mailMessage.Subject = "Centrics Networks Service Report";
            client.Send(mailMessage);

        }

        private MailMessage SRMailImage(MailMessage mail, ServiceReport SR)
        {
            mail.AlternateViews.Add(SREmbedImage(SR, @"~\Images\logo1.png"));
            return mail;
        }
        private AlternateView SREmbedImage(ServiceReport SR,String filePath)
        {

            LinkedResource res = new LinkedResource(filePath, MediaTypeNames.Image.Jpeg)
            {
                ContentId = Guid.NewGuid().ToString()
            };

            string html = "Dear " + SR.ClientCompanyName + ", <br /> <br />"
                + "The purpose of this e-mail is to inform you about a recent usage of centrics networks services <br />"
                + "<br/>" + "Contract Info: <br />"
                + "Your company: " + SR.ClientCompanyName + ", has recently been serviced by centrics networks. <br/>" 
                + "The Service stated above happended on: " + SR.TimeStart + " to "+ SR.TimeEnd + ". <br/>"
                + "The Purpose of the visit was: " + SR.PurposeOfVisit + "<br/>"
                + "Service Hours: " + SR.MSHUsed
                + "<br/> <br/> " + "Please contact Centrics Networks at 6833-7898 if there is any wrong information or any enquires."
                + "<br/> <br/> " + "This email is auto-generated. Please do not reply to this email."
                + "<br> <br/>" + " Warm Regards, <br/> Centrics <br /> <b>Centrics Networks Pte. Ltd.</b>"
                + "<br/>" + @"<img src='cid:" + res.ContentId + @"'/>"
                + "<br/>" + "26 Sin Ming Lane, #06-120 Midview City, Singapore (573971)"
                + "<br/>" + "Main: 6833 7898 | Fax: 6833 7897 " +
                "<br/>" + "Web: www.centricsnetworks.com.sg"
                ;
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(res);
            return alternateView;
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

        public ServiceReport SubtractMSHUsingSR(ServiceReport model)
        {
            double remains = 0.0;
            MySqlConnection conn = GetConnection();
            try
            {
                Contract cont = GetLatestContractBySRModel(model);
                conn.Open();
                string Query = "update contract set msh = @msh where idcontract = @idcontract";
                MySqlCommand c = new MySqlCommand(Query, conn);
                
                //model.msh > cont.msh
                if (model.MSHUsed > cont.MSH)
                {
                    c.Parameters.AddWithValue("@msh",cont.MSH - cont.MSH);
                    remains = model.MSHUsed - cont.MSH;
                    model.MSHUsed = remains;
                }
                else
                {
                    c.Parameters.AddWithValue("@msh",cont.MSH - model.MSHUsed);
                    remains = 0;
                    model.MSHUsed = 0;
                }

                
                c.Parameters.AddWithValue("@idcontract", cont.idcontract);

                c.ExecuteNonQuery();
                return model;
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
            return model;

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

        //trying to get the earliest expiring contract
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
                    if (listcont[i].StartValid.CompareTo(DateTime.Today) <= 0 && listcont[i].EndValid.CompareTo(DateTime.Today) >= 0)
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
                if (listcont[0].StartValid.CompareTo(DateTime.Today) <= 0 && listcont[0].EndValid.CompareTo(DateTime.Today) >= 0)
                {
                    returner = listcont[0];
                }
            }
            return returner;
        }
        
        public Contract getContract(int idcontract)
        {
            MySqlConnection conn = GetConnection();
            Contract dummy = new Contract();
            try
            {
                conn.Open();
                string Query = "select * from contract where idcontract= @idcontract order by endvalid asc";

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
                            EndValid = DateTime.Parse(r["endvalid"].ToString()),
                            CentricID = int.Parse(r["centricid"].ToString()),
                            Email = r["email"].ToString(),
                            ContractType = r["contracttype"].ToString()
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

        //probably working time counter
        public double CalculateMSH(DateTime jobstart, DateTime jobend)
        {
            SRCalculation multipliers = GetMSHMultiplier();
            Debug.WriteLine("jobstart" + jobstart);
            Debug.WriteLine("jobend" + jobend);
            if(jobstart > jobend)
            {
                return 0;
            }
            DateTime startdate9am = jobstart.Date.AddHours(9);
            DateTime startdate6pm = jobstart.Date.AddHours(18);
            #region old code(commented out)
            //if (jobstart.DayOfWeek == DayOfWeek.Saturday && jobend.DayOfWeek == DayOfWeek.Saturday)
            //{
            //    //start and end on sat
            //    return (jobend - jobstart).TotalHours * 1.5;
            //}
            //if (jobstart.DayOfWeek == DayOfWeek.Saturday && jobend.DayOfWeek == DayOfWeek.Sunday)
            //{
            //    //start on sat, end on sunday
            //    DateTime sunday12am = jobstart.Date.AddDays(1).AddHours(0);
            //    return (((sunday12am - jobstart).TotalHours) * 1.5) + ((jobend - sunday12am).TotalHours * 2);
            //}
            //if (jobstart.DayOfWeek == DayOfWeek.Sunday && jobend.DayOfWeek == DayOfWeek.Sunday)
            //{
            //    //start and end on sun
            //    return (jobend - jobstart).TotalHours * 2;
            //}
            //if (jobstart.DayOfWeek == DayOfWeek.Sunday && jobend.DayOfWeek != DayOfWeek.Monday)
            //{
            //    //start on sun, end not on sunday
            //    DateTime monday12am = jobstart.Date.AddDays(1).AddHours(0);
            //    if (jobend < jobend.Date.AddHours(9)) {
            //        return ((monday12am - jobstart).TotalHours * 2) + ((jobend - monday12am).TotalHours * 1.5);
            //    } else if (jobend > jobend.Date.AddHours(9))
            //    {
            //        return (((monday12am - jobstart).TotalHours * 2) + (((jobend.Date.AddHours(9) - monday12am).TotalHours) * 1.5) + ((jobend - jobend.Date.AddHours(9)).TotalHours));
            //    }
            //}
            //if (jobstart > startdate9am && jobend < startdate6pm)
            //{
            //    return (jobend - jobstart).TotalHours;
            //}
            //if (jobstart < startdate9am && (jobend > startdate9am && jobend < startdate6pm))
            //{
            //    return ((startdate9am - jobstart).TotalHours * 1.5) + ((jobend - startdate9am).TotalHours);
            //}
            //if ((jobstart > startdate9am && jobstart < startdate6pm) && jobend > startdate6pm)
            //{
            //    return (startdate6pm - jobstart).TotalHours + ((jobend - startdate6pm).TotalHours * 1.5);
            //}
            //if (jobstart > startdate6pm && jobend < jobstart.Date.AddDays(1).AddHours(9))
            //{
            //    return (jobend - jobstart).TotalHours * 1.5;
            //}

            // ^ check
            #endregion

            double workingcounter = 0.0;
            //same day 
            string pathtraverse = "";
            if (jobstart.Date == jobend.Date)
            {
                if (!(jobstart.DayOfWeek == DayOfWeek.Saturday || jobstart.DayOfWeek == DayOfWeek.Sunday))
                {
                    if (jobstart <= startdate9am && jobend <= startdate9am)
                    {
                        // start and end before 9
                        workingcounter += (jobend - jobstart).TotalHours * multipliers.From12amto9am;
                        Debug.WriteLine((startdate9am - jobstart).TotalHours);
                        pathtraverse = pathtraverse + "1";
                    }
                    else if (jobstart <= startdate9am && jobend <= startdate6pm)
                    {
                        // start before 9, end before 6
                        workingcounter += ((startdate9am - jobstart).TotalHours * multipliers.From12amto9am) + ((jobend - startdate9am).TotalHours);
                        pathtraverse = pathtraverse + "2";
                    }
                    else if (jobstart <= startdate9am && jobend >= startdate6pm)
                    {
                        //start before 9, end after 6
                        workingcounter += ((startdate9am - jobstart).TotalHours * multipliers.From12amto9am) + ((startdate6pm - startdate9am).TotalHours) + ((jobend - startdate6pm).TotalHours * multipliers.After6pmto12am);
                        pathtraverse = pathtraverse + "3";
                    }
                    else if (jobstart >= startdate9am && jobend <= startdate6pm)
                    {
                        //start after 9, end before 6
                        workingcounter += (jobend - jobstart).TotalHours;
                        pathtraverse = pathtraverse + "4";
                    }
                    
                    else if (jobstart >= startdate6pm && jobend >= startdate6pm)
                    {
                        //start and end after 6
                        Debug.WriteLine("I came after 6");
                        workingcounter += (jobend - jobstart).TotalHours * multipliers.After6pmto12am;
                        pathtraverse = pathtraverse + "5";
                    }else if (jobstart >= startdate9am && jobend >= startdate6pm)
                    {
                        //start after 9, end after 6
                        workingcounter += ((startdate6pm - jobstart).TotalHours) + ((jobend - startdate6pm).TotalHours * multipliers.After6pmto12am);
                        pathtraverse = pathtraverse + "6";
                    }
                }else if(jobstart.DayOfWeek == DayOfWeek.Saturday)
                {
                    workingcounter += (jobend - jobstart).TotalHours * multipliers.SaturdayMultiplier;
                    pathtraverse = pathtraverse + "7";
                }
                else if(jobstart.DayOfWeek == DayOfWeek.Sunday)
                {
                    workingcounter += (jobend - jobstart).TotalHours * multipliers.SundayMultiplier;
                    pathtraverse = pathtraverse + "8";
                }
            }else if (jobstart.Date != jobend.Date) //start and end on different dates
            {
                DateTime datecounter = jobstart.Date;
                DateTime timecounter = jobstart;
                TimeRange workinghours = new TimeRange(9, 0, 18, 0);
                DateTime nextday = datecounter.Date.AddDays(1);
                while (datecounter.Date < jobend.Date)
                {
                    //if it monday to friday
                    if (!(datecounter.DayOfWeek == DayOfWeek.Saturday || datecounter.DayOfWeek == DayOfWeek.Sunday))
                    {
                        //counting hours in a day
                        if (jobstart >= startdate6pm)
                        {
                            //DateTime nextday = datecounter.AddDays(1);
                            workingcounter += (nextday - jobstart).TotalHours * multipliers.After6pmto12am;
                            pathtraverse = pathtraverse + "9";
                        }
                        else if (jobstart >= startdate9am && jobstart <= startdate6pm)
                        {
                           // DateTime nextday = datecounter.AddDays(1);
                            workingcounter += (startdate6pm - jobstart).TotalHours + ((nextday - startdate6pm).TotalHours * multipliers.After6pmto12am);
                            pathtraverse = pathtraverse + "10";
                        }
                        else if (jobstart <= startdate9am)
                        {
                            //DateTime nextday = datecounter.AddDays(1);
                            workingcounter += ((startdate9am - jobstart).TotalHours * multipliers.From12amto9am) + ((startdate6pm - startdate9am).TotalHours)
                                + ((nextday - startdate6pm).TotalHours * multipliers.After6pmto12am);
                            pathtraverse = pathtraverse + "11";
                        }
                    }else if(datecounter.DayOfWeek == DayOfWeek.Saturday)
                    {
                        //DateTime nextday = datecounter.AddDays(1);
                        workingcounter += (nextday - jobstart).TotalHours * multipliers.SaturdayMultiplier;
                        pathtraverse = pathtraverse + "12";
                    }
                    else if(datecounter.DayOfWeek == DayOfWeek.Sunday)
                    {
                       // DateTime nextday = datecounter.AddDays(1);
                        workingcounter += (nextday - jobstart).TotalHours * multipliers.SundayMultiplier;
                        pathtraverse = pathtraverse + "13";
                    }
                    startdate6pm = startdate6pm.AddDays(1);
                    startdate9am = startdate9am.AddDays(1);
                    nextday = nextday.Date.AddDays(1);
                    jobstart = jobstart.Date.AddDays(1);
                    datecounter = datecounter.AddDays(1);
                    
                    Debug.WriteLine("boo hoo " + workingcounter);
                    Debug.WriteLine("Kamehameha" + datecounter.Date);
                }
                if (!(jobend.DayOfWeek == DayOfWeek.Sunday || jobend.DayOfWeek == DayOfWeek.Saturday))
                {
                    if (jobend <= startdate9am)
                    {
                        workingcounter += (jobend - datecounter).TotalHours * multipliers.From12amto9am;
                        pathtraverse = pathtraverse + "14";
                    }
                    else if (jobend >= startdate9am && jobend <= startdate6pm)
                    {
                        workingcounter += ((jobend - startdate9am).TotalHours) + ((startdate9am - datecounter).TotalHours * multipliers.From12amto9am);
                        pathtraverse = pathtraverse + "15";
                    }
                    else if (jobend >= startdate6pm)
                    {
                        workingcounter += (startdate6pm - startdate9am).TotalHours + ((jobend - startdate6pm).TotalHours * multipliers.After6pmto12am) +
                            ((startdate9am - datecounter).TotalHours * multipliers.From12amto9am);
                        pathtraverse = pathtraverse + "16";
                    }
                }else if(jobend.DayOfWeek == DayOfWeek.Saturday)
                {
                    workingcounter += (jobend - datecounter).TotalHours * multipliers.SaturdayMultiplier;
                    pathtraverse = pathtraverse + "18";
                }
                else if (jobend.DayOfWeek == DayOfWeek.Sunday)
                {
                    workingcounter += (jobend - datecounter).TotalHours * multipliers.SundayMultiplier;
                    pathtraverse = pathtraverse + "19";
                }

            }
            
            Debug.WriteLine("traverse" + pathtraverse);
            return Math.Round(workingcounter,1);
            
        }

        #region old code for reference? (commented out)
        //public double CalculateMSHUsed(DateTime starttime, DateTime endtime)
        //{
        //    //TimeSpan start = TimeSpan.Parse("22:00"); // 10 PM
        //    //TimeSpan end = TimeSpan.Parse("02:00");   // 2 AM
        //    //TimeSpan now = DateTime.Now.TimeOfDay;

        //    //if (start <= end)
        //    //{
        //    //    // start and stop times are in the same day
        //    //    if (now >= start && now <= end)
        //    //    {
        //    //        // current time is between start and stop
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    // start and stop times are in different days
        //    //    if (now >= start || now <= end)
        //    //    {
        //    //        // current time is between start and stop
        //    //    }
        //    //}
        //    double custom = 0.0;
        //    TimeSpan workingstart = new TimeSpan(9, 0, 0);
        //    TimeSpan workingend = new TimeSpan(18, 0, 0);
        //    TimeSpan jobtimestart = new TimeSpan(starttime.Hour, starttime.Minute, starttime.Second);
        //    TimeSpan jobtimeend = new TimeSpan(endtime.Hour, endtime.Minute, endtime.Second);
        //    TimeSpan span = endtime.Subtract(starttime);

        //    //if long hours, outside + inside + outside
        //    //Monday to friday 9-6
        //    if (!(starttime.DayOfWeek == DayOfWeek.Saturday && !(endtime.DayOfWeek == DayOfWeek.Saturday || endtime.DayOfWeek == DayOfWeek.Sunday) )|| !(starttime.DayOfWeek == DayOfWeek.Sunday && endtime.DayOfWeek == DayOfWeek.Sunday))
        //    {
        //        TimeRange t = null;
        //        t = new TimeRange(9, 0, 18, 0);
        //        if(t.IsIn(new TimeRange(starttime.Hour,starttime.Minute,endtime.Hour, endtime.Minute)))
        //        {
        //            return span.TotalHours;
        //        }else if(t.Clashes(new TimeRange(starttime.Hour, starttime.Minute, endtime.Hour, endtime.Minute)))
        //        {
        //            //if end time is smaller than start time   
        //            if ((jobtimestart > workingstart && jobtimestart < workingend) && jobtimeend > workingend)
        //            {
        //                //if job started in the working hours 9-6 but exceed 6
        //                //start at 10am end at 7pm

        //                DateTime now = endtime;
        //                DateTime endworktime = now.Date.AddHours(18);
        //                custom = (endworktime - starttime).TotalHours + ((endtime - endworktime) * 1.5).TotalHours;
        //                return custom;
        //                #region timespan format?
        //                //TimeSpan hi = workingend.Subtract(jobtimestart);
        //                //TimeSpan bye = jobtimeend.Subtract(workingend);
        //                //custom = hi.TotalHours + (bye.TotalHours * 1.5);
        //                //return custom;
        //                #endregion
        //                #region //friday exception ps i forget
        //                //if (starttime.DayOfWeek == DayOfWeek.Friday && endtime.DayOfWeek == DayOfWeek.Friday)
        //                //{
        //                //    TimeSpan hi = workingend.Subtract(jobtimestart);
        //                //    TimeSpan bye = jobtimeend.Subtract(workingend);
        //                //    custom = hi.TotalHours + (bye.TotalHours * 1.5);
        //                //    return custom;
        //                //}
        //                #endregion
        //            }
        //            else if (jobtimestart < workingstart && (jobtimeend > workingstart && jobtimeend < workingend))
        //            {
        //                //job started before 9 and end between 9-6
        //                //start at 8am end at 2pm
        //                DateTime now = endtime;
        //                DateTime startworktime = now.Date.AddHours(9);
        //                custom = (endtime-startworktime).TotalHours + ((startworktime - starttime) * 1.5).TotalHours;
        //                return custom;

        //                //add monday exception
        //                #region timespan format?
        //                //TimeSpan hi = jobtimeend.Subtract(workingstart);
        //                //TimeSpan bye = workingstart.Subtract(jobtimestart);
        //                //custom = hi.TotalHours + (bye.TotalHours * 1.5);
        //                //return custom;
        //                #endregion
        //            }
        //            else if(jobtimestart > jobtimeend && (jobtimeend > workingstart && jobtimeend < workingend)) // recheck sure wrong
        //            {
        //                //if job time end at 9-6 but start before 9
        //                //example if it start  > end
        //                //start at 11pm end at 10am
        //                DateTime now = endtime;
        //                DateTime startworktime = now.Date.AddHours(9);
        //                custom = (endtime - startworktime).TotalHours + ((startworktime - starttime) * 1.5).TotalHours;
        //                return custom;
        //                //Question if u use 0800 subtract 2300 what you get? (+24 for solve?)
        //                #region timespan format?
        //                //TimeSpan hi = jobtimeend.Subtract(workingstart);
        //                //TimeSpan bye = workingstart.Subtract(jobtimestart);
        //                //custom = hi.TotalHours + (bye.TotalHours * 1.5);
        //                //return custom;
        //                #endregion
        //            }
        //            else if((jobtimestart > workingstart && jobtimestart < workingend) && jobtimeend < jobtimestart)
        //            {
        //                //if job time start at 9-6 but end after 6
        //                //start at 5pm end at 1am
        //                DateTime now = endtime;
        //                DateTime endworktime = now.Date.AddHours(18);
        //                custom = (endworktime-starttime).TotalHours + ((endtime - endworktime)*1.5).TotalHours;
        //                return custom;
        //            }
        //        }
        //        else
        //        {
        //            if (endtime.DayOfWeek == DayOfWeek.Monday && starttime.DayOfWeek == DayOfWeek.Sunday) {

        //                if (new TimeRange(9,0,18,0).Clashes(new TimeRange(starttime.Hour,starttime.Minute,endtime.Hour,endtime.Minute)))
        //                {
        //                    //if time start from sunday and end on monday
        //                }
        //                else
        //                {
        //                    return (span.TotalHours * 1.5);
        //                }
        //            }
        //            if(starttime.DayOfWeek == DayOfWeek.Friday && (endtime.DayOfWeek == DayOfWeek.Saturday || endtime.DayOfWeek == DayOfWeek.Sunday))
        //            {
        //                if (new TimeRange(9, 0, 18, 0).Clashes(new TimeRange(starttime.Hour, starttime.Minute, endtime.Hour, endtime.Minute)))
        //                {
        //                    //if start before 6pm on friday and end later
        //                }
        //                else
        //                {

        //                    return span.TotalHours * 1.5;
        //                }
        //            }
        //            //if start and end on sat/sunday without touching mon - fri 9-6
        //            return (span.TotalHours * 1.5);
        //        }

        //    }
        //    else
        //    {
        //        return (span.TotalHours * 1.5);
        //    }
        //}
        #endregion
        
        public void DeleteReport(int id)
        {
            MySqlConnection conn = GetConnection();
            if(getServiceReport(id) != null || getServiceReport(id).ReportStatus != "Comfirmed" ) {
                try
                {
                    conn.Open();
                    string Query = "delete  from centrics.servicereport where id = @id";

                    MySqlCommand c = new MySqlCommand(Query, conn);
                    c.Parameters.AddWithValue("@id", id);

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
        //this doesnt exist
        public void ModifyContract(Contract model) {

            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "update centrics.contract set email = @email where idcontract =@idcontract";
                MySqlCommand c = new MySqlCommand(query, conn);
                
                c.Parameters.AddWithValue("@email", model.Email);
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

        //check for a duplicating id
        public bool CheckExisitingReportID(int id)
        {
            MySqlConnection conn = GetConnection();
            
            try
            {
                conn.Open();
                string query = "select * from centrics.servicereport";

                MySqlCommand c = new MySqlCommand(query, conn);

                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        if(id == int.Parse(r["id"].ToString()))
                        {
                            return true;
                        }
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
            return false;
        }

        //get the id of the next report to display in add service report page
        public int getReportCounts()
        {
            MySqlConnection conn = GetConnection();
            int count = 0;
            try
            {
                conn.Open();
                string query = "SELECT `AUTO_INCREMENT` FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'centrics' AND TABLE_NAME = 'servicereport'; ";

                MySqlCommand c = new MySqlCommand(query, conn);

                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        count = int.Parse(r["AUTO_INCREMENT"].ToString());
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
        
       
        public ServiceReport getServiceReport(int serial)
        {
            ServiceReport SR = new ServiceReport();
            var empt = SR;
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
                        if (DBNull.Value.Equals(r["id"]))
                        {
                            Debug.WriteLine("i broke");
                            break;
                        }
                        SR = new ServiceReport
                        {
                            SerialNumber = int.Parse(r["id"].ToString()),
                            ClientCompanyName = r["clientcompanyname"].ToString(),
                            ClientAddress = r["clientaddress"].ToString(),
                            ClientContactPerson = r["clientcontactperson"].ToString(),
                            PurposeOfVisit = r["purposeofvisit"].ToString().Split(','),
                            Description = r["description"].ToString(),
                            Remarks = r["remarks"].ToString(),
                            TimeStart = DateTime.Parse(r["timestart"].ToString()),
                            TimeEnd = DateTime.Parse(r["timeend"].ToString()),
                            MSHUsed = double.Parse(r["mshused"].ToString()),
                            AttendedByStaffName = r["attendedbystaffname"].ToString(),
                            AttendedOnDate = DateTime.Parse(r["attendedondate"].ToString()),
                            JobStatus = r["jobstatus"].ToString().Split(','),
                            ReportStatus = r["reportstatus"].ToString(),
                            ReportFrom = r["reportfrom"].ToString()
                        };

                        if (!DBNull.Value.Equals(r["clienttel"]))
                        {

                            SR.ClientTel = int.Parse(r["clienttel"].ToString());
                        }
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
                
                if (SR != empt)
                {
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
                
                string query = "Update centrics.servicereport set purposeofvisit = @purposeofvisit, description = @description, remarks = @remarks, timestart=@timestart, timeend = @timeend,mshused = @mshused, attendedbystaffname = @attendedbystaffname,attendedondate = @attendedondate ,jobstatus = @jobstatus where id = @id";

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
                ClientAddress cA = getOneClient(model.ClientCompanyName);
                c.Parameters.AddWithValue("@id",model.SerialNumber);
                c.Parameters.AddWithValue("@purposeofvisit", combinedpurpose);
                c.Parameters.AddWithValue("@description", model.Description);
                c.Parameters.AddWithValue("@remarks", model.Remarks);
                c.Parameters.AddWithValue("@mshused", CalculateMSH(model.TimeStart, model.TimeEnd));
                c.Parameters.AddWithValue("@attendedondate",model.TimeStart.Date);
                c.Parameters.AddWithValue("@timestart", model.TimeStart);
                c.Parameters.AddWithValue("@timeend", model.TimeEnd);
                c.Parameters.AddWithValue("@attendedbystaffname", model.AttendedByStaffName);
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

        //removing one address from the client
        public void RemoveAddressFromClientList(string name, string address)
        {
            MySqlConnection conn = GetConnection();
            ClientAddress cA = GetClientAddressList(name);
            List<string> aList = cA.Addresslist;
            for(int i =0;i< aList.Count(); i++)
            {
                if(aList[i] == address)
                {
                    aList.Remove(address);
                }
            }
            int hi = aList.Count();
            try
            {
                conn.Open();

                if (hi == 0)
                {
                
                string query = "update centrics.clientaddress set addresslist = @addresslist where companyname = @clientcompany;";
                MySqlCommand c = new MySqlCommand(query, conn);
                address = "";
                c.Parameters.AddWithValue("@clientcompany", name);
                c.Parameters.AddWithValue("@addresslist", DBNull.Value);

                c.ExecuteNonQuery();
                }
                else if (hi > 0)
                {
                if (hi == 5)
                {
                    Debug.WriteLine("Wow system broke");
                }
                if (hi == 1)
                {
                    string query = "update centrics.clientaddress set addresslist = @addresslist where companyname = @clientcompany";
                    MySqlCommand c = new MySqlCommand(query, conn);

                        Debug.WriteLine("here please? ");
                    c.Parameters.AddWithValue("@addresslist", aList[0]);
                    c.Parameters.AddWithValue("@clientcompany", name);

                    c.ExecuteNonQuery();
                }
                else
                {
                    string saver = "";
                    string query = "update centrics.clientaddress set addresslist = @addresslist where companyname = @clientcompany";
                    MySqlCommand c = new MySqlCommand(query, conn);
                    for (int i = 0; i < aList.Count(); i++)
                    {
                        if (i != (aList.Count -1))
                        {
                            saver += aList[i] + "centricsnetworks";
                        }
                        else
                        {
                            saver += aList[i];
                        }
                    }
                    c.Parameters.AddWithValue("@addresslist", saver);
                    c.Parameters.AddWithValue("clientcompany", name);

                    c.ExecuteNonQuery();
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
        }

        //get all client details
        public List<ClientAddress> getAllClientAddress()
        {
            MySqlConnection conn = GetConnection();
            List<ClientAddress> ListCA = new List<ClientAddress>();
            

            try
            {
                conn.Open();
                string query = "Select * from centrics.clientaddress";

                MySqlCommand c = new MySqlCommand(query, conn);

                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        ClientAddress cA = new ClientAddress();
                        List<string> addresslist = new List<string>();
                        List<string> ContactList = new List<string>();
                        List<string> ContactNoList = new List<string>();
                        List<string> EmailList = new List<string>();
                        cA.CustomerID =r["customerid"].ToString();
                        cA.ClientCompany = r["companyname"].ToString();
                        if (!DBNull.Value.Equals(r["addresslist"].ToString()))
                        {
                            string[] spliter = r["addresslist"].ToString().Split("centricsnetworks");
                            for(int i = 0;i < spliter.Length; i++)
                            {
                                addresslist.Add(spliter[i]);
                            }
                        }
                        if (!DBNull.Value.Equals(r["contact"].ToString()))
                        {
                            string[] spliter = r["contact"].ToString().Split("centricsnetworks");
                            for (int i = 0; i < spliter.Length; i++)
                            {
                                ContactList.Add(spliter[i]);
                            }
                        }
                        if (!DBNull.Value.Equals(r["contactno"].ToString()))
                        {
                            string[] spliter = r["contactno"].ToString().Split("centricsnetworks");
                            for (int i = 0; i < spliter.Length; i++)
                            {
                                ContactNoList.Add(spliter[i]);
                            }
                        }
                        if (!DBNull.Value.Equals(r["emailaddress"].ToString()))
                        {
                            string[] spliter = r["emailaddress"].ToString().Split("centricsnetworks");
                            for (int i = 0; i < spliter.Length; i++)
                            {
                                EmailList.Add(spliter[i]);
                            }
                        }

                        cA.EmailList = EmailList;
                        cA.ContactNoList = ContactNoList;
                        cA.ContactList = ContactList;
                        cA.Addresslist = addresslist;
                        
                        ListCA.Add(cA);
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
            return ListCA;
        }

        //get a company detail
        public ClientAddress getOneClient(string name)
        {
            MySqlConnection conn = GetConnection();
            ClientAddress cA = new ClientAddress();

            try
            {
                conn.Open();
                string query = "Select * from centrics.clientaddress where companyname = @name";

                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@name", name);
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        
                        List<string> addresslist = new List<string>();
                        List<string> ContactList = new List<string>();
                        List<string> ContactNoList = new List<string>();
                        List<string> EmailList = new List<string>();
                        cA.CustomerID = r["customerid"].ToString();
                        cA.ClientCompany = r["companyname"].ToString();
                        if (!DBNull.Value.Equals(r["addresslist"].ToString()))
                        {
                            string[] spliter = r["addresslist"].ToString().Split("centricsnetworks");
                            for (int i = 0; i < spliter.Length; i++)
                            {
                                addresslist.Add(spliter[i]);
                            }
                            cA.Addresslist = addresslist;
                        }
                        else
                        {
                            cA.Addresslist = null;
                        }

                        if (!DBNull.Value.Equals(r["contact"].ToString()))
                        {
                            string[] spliter = r["contact"].ToString().Split("centricsnetworks");
                            for (int i = 0; i < spliter.Length; i++)
                            {
                                ContactList.Add(spliter[i]);
                            }
                            cA.ContactList = ContactList;
                        }
                        else
                        {
                            cA.ContactList = null;
                        }
                        if (!DBNull.Value.Equals(r["contactno"].ToString()))
                        {
                            string[] spliter = r["contactno"].ToString().Split("centricsnetworks");
                            for (int i = 0; i < spliter.Length; i++)
                            {
                                ContactNoList.Add(spliter[i]);
                            }
                            cA.ContactNoList = ContactNoList;
                        }
                        else
                        {
                            cA.ContactNoList = null;
                        }
                        if (!DBNull.Value.Equals(r["emailaddress"].ToString()))
                        {
                            string[] spliter = r["emailaddress"].ToString().Split("centricsnetworks");
                            for (int i = 0; i < spliter.Length; i++)
                            {
                                EmailList.Add(spliter[i]);
                            }
                            cA.EmailList = EmailList;
                        }
                        else
                        {
                            cA.EmailList = null;
                        }

                        //cA.EmailList = EmailList;
                        //cA.ContactNoList = ContactNoList;
                        //cA.ContactList = ContactList;
                        //cA.Addresslist = addresslist;

                        return cA;
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
            return cA;
        }

        //search function based on name of the company
        public List<ClientAddress> SearchClientAddress(string name)
        {
            MySqlConnection conn = GetConnection();
            List<ClientAddress> ListCA = new List<ClientAddress>();
            

            try
            {
                conn.Open();
                string query = "Select * from centrics.clientaddress where companyname like @clientcompany";

                MySqlCommand c = new MySqlCommand(query, conn);

                c.Parameters.AddWithValue("@clientcompany", "%" + name + "%");

                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        ClientAddress cA = new ClientAddress();
                        List<string> addresslist = new List<string>();
                        List<string> ContactList = new List<string>();
                        List<string> ContactNoList = new List<string>();
                        List<string> EmailList = new List<string>();
                        cA.CustomerID = r["customerid"].ToString();
                        cA.ClientCompany = r["companyname"].ToString();
                        if (!DBNull.Value.Equals(r["addresslist"].ToString()))
                        {
                            string[] spliter = r["addresslist"].ToString().Split("centricsnetworks");
                            for (int i = 0; i < spliter.Length; i++)
                            {
                                addresslist.Add(spliter[i]);
                            }
                        }
                        if (!DBNull.Value.Equals(r["contact"].ToString()))
                        {
                            string[] spliter = r["contact"].ToString().Split("centricsnetworks");
                            for (int i = 0; i < spliter.Length; i++)
                            {
                                ContactList.Add(spliter[i]);
                            }
                        }
                        if (!DBNull.Value.Equals(r["contactno"].ToString()))
                        {
                            string[] spliter = r["contactno"].ToString().Split("centricsnetworks");
                            for (int i = 0; i < spliter.Length; i++)
                            {
                                ContactNoList.Add(spliter[i]);
                            }
                        }
                        if (!DBNull.Value.Equals(r["emailaddress"].ToString()))
                        {
                            string[] spliter = r["emailaddress"].ToString().Split("centricsnetworks");
                            for (int i = 0; i < spliter.Length; i++)
                            {
                                EmailList.Add(spliter[i]);
                            }
                        }

                        cA.EmailList = EmailList;
                        cA.ContactNoList = ContactNoList;
                        cA.ContactList = ContactList;
                        cA.Addresslist = addresslist;

                        ListCA.Add(cA);
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
            return ListCA;
        }

        //importing method (from excel)
        public void Imported(ClientAddress cA)
        {
            MySqlConnection conn = GetConnection();

            if (GetClientAddressList(cA.ClientCompany).ClientCompany != "")
            {
                try
                {
                    conn.Open();
                    string query = "Delete from centrics.clientaddress where companyname = @companyname";

                    MySqlCommand c = new MySqlCommand(query, conn);
                    c.Parameters.AddWithValue("@companyname", cA.ClientCompany);
                    
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

            //current (delete) -- when deleting exisiting contract, there can exist a data in the contract but not in the client.
            //do a truncate -- excel as the true source, all old data deleted. causes more of a reliance(data) issue with contract
            //do a ignore -- does not cause a error in contract but does contain error if excel changes that row huge margin(old code have this)
            
            //if u want to change the method, just comment the old configuration. For the delete, comment both the delete code blocks.

            //For ignore, go to the importexport controller, uncomment 2 sections of the code of onPostImport 
            //namely the line //if (context.GetClientAddressList(row.GetCell(0).ToString()).ClientCompany != "") continue;
            //and the section starting with //for (int k = 0; k<ListCA.Count; k++)

            //To comment in visual studio do a ctrl+k+c combo keys and uncomment ctrl + k + u combo keys


            //try
            //{
            //    conn.Open();
            //    string truncate = "Truncate TABLE centrics.clientaddress";

            //    MySqlCommand c = new MySqlCommand(truncate, conn);
            //    c.Parameters.AddWithValue("@customerid", cA.CustomerID);

            //    c.ExecuteNonQuery();
            //}catch(MySqlException e)
            //{
            //    Debug.WriteLine(e);
            //}
            //finally
            //{
            //    conn.Close();
            //}



            List<ClientAddress> cList = getAllClientAddress();
            for(int i = 0;i < cList.Count; i++)
            {
            if (cList[i].CustomerID == cA.CustomerID)
            {
                try
                {
                    conn.Open();
                    string query = "Delete from centrics.clientaddress where customerid = @customerid";

                    MySqlCommand c = new MySqlCommand(query, conn);
                    c.Parameters.AddWithValue("@customerid", cA.CustomerID);

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

            ClientAddress getList = GetClientAddressList(cA.ClientCompany);
            if (getList.ClientCompany == "" )
            {

                List<string> Addresslist = new List<string>();
                List<string> ContactNoList = new List<string>();
                List<string> ContactList = new List<string>();
                List<string> EmailList = new List<string>();
                List<string> TitleList = new List<string>();

                if (cA.Addresslist != null)
                {
                    Addresslist = cA.Addresslist;
                }
                if (cA.ContactNoList != null)
                {
                    ContactNoList = cA.ContactNoList;
                }
                if (cA.ContactList != null)
                {
                    ContactList = cA.ContactList;
                }
                if (cA.EmailList != null)
                {
                    EmailList = cA.EmailList;
                }
                if (cA.TitleList != null)
                {
                    TitleList = cA.TitleList;
                }
                
                    if (Addresslist.Count == 1)
                    {
                        cA.Address = Addresslist[0];
                    }
                    else if (Addresslist.Count > 1)
                    {
                        for (int c = 0; c < Addresslist.Count(); c++)
                        {
                            
                            if (c != (Addresslist.Count() -1 ))
                            {
                                cA.Address += Addresslist[c] + "centricsnetworks";
                            }
                            else
                            {
                                cA.Address += Addresslist[c];
                            }
                        }
                    }
                                
                    if (ContactList.Count == 1)
                    {
                        cA.Contact = ContactList[0];
                    }
                    else if (ContactList.Count > 1)
                    {
                        for (int c = 0; c < ContactList.Count(); c++)
                        {
                            if (c != (ContactList.Count -1 ))
                            {
                                cA.Contact += ContactList[c] + "centricsnetworks";
                            }
                            else
                            {
                                cA.Contact += ContactList[c];
                            }
                        }
                    }
                
                
                    if (ContactNoList.Count == 1)
                    {
                        cA.ContactNoString = ContactNoList[0];
                    }
                    else if (ContactNoList.Count > 1)
                    {
                        for (int c = 0; c < ContactNoList.Count(); c++)
                        {
                            if (c != (ContactNoList.Count -1 ))
                            {

                                cA.ContactNoString += ContactNoList[c] + "centricsnetworks";
                            }
                            else
                            {
                                cA.ContactNoString += ContactNoList[c];
                            }
                        }
                    }
                                
                        if (EmailList.Count == 1)
                        {
                            cA.EmailAddress = EmailList[0];
                        }
                        else if (EmailList.Count > 1)
                        {
                            for (int c = 0; c < EmailList.Count(); c++)
                            {
                                if (c != (EmailList.Count -1 ))
                                {
                                    
                                    cA.EmailAddress += EmailList[c] + "centricsnetworks";                                    
                                }
                                else
                                {
                                    
                                    cA.EmailAddress += EmailList[c];                                }
                            }
                        }
                
                try
                {
                    conn.Open();
                    string query = "insert into centrics.clientaddress(customerid,companyname,addresslist,contact,contactno,emailaddress) values(@customerid,@companyname,@addresslist,@contact,@contactno,@emailaddress)";

                    MySqlCommand c = new MySqlCommand(query, conn);
                    
                    c.Parameters.AddWithValue("@customerid", cA.CustomerID);
                    c.Parameters.AddWithValue("@companyname",cA.ClientCompany);
                    c.Parameters.AddWithValue("@addresslist", cA.Address);
                    c.Parameters.AddWithValue("@contact", cA.Contact);
                    c.Parameters.AddWithValue("@contactno", cA.ContactNoString);
                    c.Parameters.AddWithValue("@emailaddress", cA.EmailAddress);

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

        //Add a address to the client existing address list 
        public void AddAdresstoClientAddressList(ClientAddress cA)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                
                ClientAddress newy = GetClientAddressList(cA.ClientCompany);
                List<string> listy = newy.Addresslist;
                string saver = "";
                
                if (listy == null)
                {
                    string query = "update centrics.clientaddress set addresslist = @addresslist where companyname = @clientcompany;";
                    MySqlCommand c = new MySqlCommand(query, conn);

                    c.Parameters.AddWithValue("@clientcompany",cA.ClientCompany);
                    c.Parameters.AddWithValue("@addresslist",cA.Address);
                    
                    c.ExecuteNonQuery();
                }
                else if(listy.Count() > 0)
                {
                    if(listy.Count() == 5)
                    {
                        Debug.WriteLine("Die Die Die"); //.. basically u can't reach here hopefully
                    }
                    if(listy.Count() == 4)
                    {
                        listy.Add(cA.Address);
                        
                        string query = "update centrics.clientaddress set addresslist = @addresslist where companyname = @clientcompany";
                        MySqlCommand c = new MySqlCommand(query, conn);
                        for(int i =0 ; i < listy.Count(); i++)
                        {
                            if (i != (listy.Count - 1))
                            {
                                saver += listy[i] + "centricsnetworks";
                            }
                            else
                            {
                                saver += listy[i];
                            }
                        }

                        c.Parameters.AddWithValue("@addresslist", saver);
                        c.Parameters.AddWithValue("@clientcompany", cA.ClientCompany);

                        c.ExecuteNonQuery();
                    }
                    else {
                        listy.Add(cA.Address);
                        string query = "update centrics.clientaddress set addresslist = @addresslist where companyname = @clientcompany";
                        MySqlCommand c = new MySqlCommand(query, conn);
                        for (int i = 0; i < listy.Count(); i++)
                        {
                            if (i != (listy.Count() - 1))
                            {
                                saver += listy[i] + "centricsnetworks";
                            }
                            else
                            {
                                saver += listy[i];
                            }
                        }
                        c.Parameters.AddWithValue("@addresslist",saver);
                        c.Parameters.AddWithValue("clientcompany", cA.ClientCompany);

                        c.ExecuteNonQuery();
                    }

                   
                }

            }catch(MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
        }

        //Get client address list using the name of the company.
        public ClientAddress GetClientAddressList(string name)
        {
            MySqlConnection conn = GetConnection();
            ClientAddress listy = new ClientAddress();
            string bye = "";
            string company = "";
            try
            {
                conn.Open();
                string query = "Select * from centrics.clientaddress where companyname = @name";

                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@name", name);
                
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        if (!DBNull.Value.Equals(r["companyname"]))
                        {
                            company = r["companyname"].ToString();
                        }
                        if (!DBNull.Value.Equals(r["addresslist"]))
                        {
                            Debug.WriteLine("hi" + r["addresslist"]);
                            bye = (r["addresslist"].ToString());
                        }
                    }
                }
                List<String> hiiie = new List<string>();
                                
                string[] Addresslist = bye.Split("centricsnetworks");

                for (int i = 0; i < Addresslist.Count(); i++)
                {
                    hiiie.Add(Addresslist[i]);
                }
                if(bye == "")
                {
                    hiiie = null;
                }
                listy = new ClientAddress
                {
                    ClientCompany = (company.ToString()),
                    Addresslist = hiiie
                };
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }
            return listy;
        }

        public void AddNewCompany(ClientAddress clientAddress)
        {
            MySqlConnection conn = GetConnection();

            try
            {
                conn.Open();
                string query = "insert into centrics.clientaddress(companyname,addresslist,contact,contactno,emailaddress) values(@clientcompany,@addresslist,@contact,@contactno,@emailaddress)";

                MySqlCommand c = new MySqlCommand(query, conn);
                
                
                c.Parameters.AddWithValue("@clientcompany",clientAddress.ClientCompany );
                c.Parameters.AddWithValue("@addresslist", clientAddress.Address);
                c.Parameters.AddWithValue("@contact", clientAddress.Contact);
                c.Parameters.AddWithValue("@contactno", clientAddress.ContactNo);
                c.Parameters.AddWithValue("@emailaddress", clientAddress.EmailAddress);
                
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

        //Compares 2 strings to see whether they match
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

        //During registration, check if email is already existing in the database, returns false is email already exists
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

        //While Editing User, check if the email is already existing, returns false if email already exists in database
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
        
        //Logs in the user
        public LoginViewModel LoginUser(LoginViewModel user)
        {
            MySqlConnection conn = GetConnection();
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
                        user.UserID = Convert.ToInt32(r["userID"]);
                        user.SuccessfulLogin = CompareHashedPasswords(user.UserPassword, r["password"].ToString());
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

            return user;
        }

        //Sets user as Authenticated after they have keyed in their 2FA for the first time
        public void SetUserAsAuthenticated (String UserLoggingInEmail)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "update users set authenticated = 1 where email = @email";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@email", UserLoggingInEmail);
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

        //Creates a new user in the database
        public void RegisterUser(User model)
        {
            Boolean validEmail = CheckExistingEmail(model);

            if (validEmail)
            {
                MySqlConnection conn = GetConnection();
                try
                {
                    conn.Open();
                    string AddQuery = "insert into users(firstName, lastName,  email, password, userRole, authenticated) values (@firstName, @lastName, @email, @password, @role, @authenticated)";
                    MySqlCommand c = new MySqlCommand(AddQuery, conn);
                    string hashedPassword = HashPassword(model.UserPassword);
                    c.Parameters.AddWithValue("@firstName", model.FirstName);
                    c.Parameters.AddWithValue("@lastName", model.LastName);
                    c.Parameters.AddWithValue("@email", model.UserEmail);
                    c.Parameters.AddWithValue("@password", hashedPassword);
                    c.Parameters.AddWithValue("@role", model.UserRole);
                    c.Parameters.AddWithValue("@authenticated", 0);
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

        //Get a single user by Email, returns a User Object
        public User GetUserByEmail(string email)
        {
            //Initialize User to place returned User object
            User userRetrieved = new User();

            //Establish connection to MySQL Database
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "select * from users where email = @email";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@email", email);
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
                            UserRole = r["userRole"].ToString(),
                            Authenticated = Convert.ToBoolean(r["authenticated"])
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
            return userRetrieved;
        }

        //Get a single user by UserID, returns the user retrieved as a User Object
        public User GetUser(int UserID)
        {
            //Initialize User to place returned User object
            User userRetrieved = new User();

            //Establish connection to MySQL Database
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "select * from users where userID = @userID";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@userID", UserID);
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
                            UserRole = r["userRole"].ToString(),
                            Authenticated = Convert.ToBoolean(r["authenticated"])
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
            return userRetrieved;
        }

        //Get ALL users
        public List<User> GetUsers()
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

        //Deletes User from the database
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

        //Updates to Database the changes made to the user by the admin
        public void EditUser(EditUserViewModel user)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "update users set firstName=@firstName, lastName=@lastName, email=@email, userRole=@role where userID=@userID";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@firstName", user.FirstName);
                c.Parameters.AddWithValue("@lastName", user.LastName);
                c.Parameters.AddWithValue("@email", user.UserEmail);
                c.Parameters.AddWithValue("@role", user.UserRole);
                c.Parameters.AddWithValue("@userID", user.UserID);

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

        //Changes the password of a user by asking them to key in their old password as well as their new password
        public Boolean ChangePassword(string CurrentPassword, string NewPassword, User user)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "select * from users where userID=@userID";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@userID", user.UserID);
                Boolean passwordMatch = false;
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    //Loops for every row of the users table
                    while (r.Read())
                    {
                        //Compare if old password is matching to the one in the database
                        if (CompareHashedPasswords(CurrentPassword, r["password"].ToString()))
                            passwordMatch = true;
                        else return false;
                    }
                    r.Close();
                }
                if (passwordMatch)
                {
                    string updateQuery = "update users set password = @NewPassword where userID = @userID";
                    MySqlCommand c2 = new MySqlCommand(updateQuery, conn);
                    NewPassword = HashPassword(NewPassword);
                    c2.Parameters.AddWithValue("@NewPassword", NewPassword);
                    c2.Parameters.AddWithValue("@userID", user.UserID);
                    c2.ExecuteNonQuery();
                    return true;
                }
                else return false;
            }
            catch (MySqlException e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                conn.Close();
            }

            return false;
        }

        public Boolean SendResetLink(ForgotPasswordViewModel user)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "select * from users where email=@email";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@email", user.UserEmail);
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    if (r.Read())
                    {
                        //EmailAddress.DefaultRenderer = new RazorRenderer();
                        //string ResetID = RandomString(20);
                        //int UserID = Convert.ToInt32(r["userID"]);
                        //SaveResetIDToDB(ResetID, UserID, DateTime.Now);
                        //var template = "Hi @Model.Name, you've recently requested to reset your password. Click on this <a href = '@Model.Link'>link</a> to reset your password. " +
                        //    "<p>Ignore this email if you did not request to reset your password.</p>";

                        //var email = EmailAddress
                        //    .From("johnfoohw@gmail.com")
                        //    .To(r["email"].ToString())
                        //    .Subject("Reset Password")
                        //    .UsingTemplate(template, new { Name = r["firstName"].ToString(), Email = r["email"].ToString(), Link = "http://localhost:57126/Users/ResetPassword?ResetID="+ResetID+"&UserID="+UserID});

                        //email.Send();
                        //return true;

                        SmtpClient client = new SmtpClient("outlook.centricsnetworks.com.sg");
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential("crm@centricsnetworks.com.sg", "Password123");
                        string ResetID = RandomString(20);
                        int UserID = Convert.ToInt32(r["userID"]);
                        SaveResetIDToDB(ResetID, UserID, DateTime.Now);
                        string name = r["firstName"].ToString();
                        string link = "http://localhost:57126/Users/ResetPassword?ResetID=" + ResetID + "&UserID=" + UserID;


                        MailMessage mailMessage = new MailMessage
                        {
                            IsBodyHtml = true,
                            Body = "Hi " + name + ", you've recently requested to reset your password. Click on this <a href = '" + link + "'>link</a> to reset your password. " +
                            "<p>Ignore this email if you did not request to reset your password.</p>",
                            From = new MailAddress("crm@centricsnetworks.com.sg")
                        };

                       //mailMessage.To.Add("wenjie_lee@centricsnetworks.com.sg"); Used for testing
                        mailMessage.To.Add(r["email"].ToString()); //Actual line to send reset link

                        //if (cA.EmailList != null)
                        //{
                        //    mailMessage.To.Add(cA.EmailList[0]);
                        //}

                        //mailMessage.To.Add(contract.Email);
                        //mailMessage.Body = "nobody has no body there nobody is nobody cuz nobody can see him <br /> thanks, from centrics";
                        mailMessage.Subject = "Reset Password";
                        client.Send(mailMessage);
                        return true;
                    }
                    
                    else
                    {
                        return false;
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

            return false;
        }

        public void SaveResetIDToDB(string ResetID, int UserID, DateTime sendDate)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "insert into resetpassword values (@resetID, @userID, @sendDate)";
                MySqlCommand c2 = new MySqlCommand(query, conn);
                c2.Parameters.AddWithValue("@resetID", ResetID);
                c2.Parameters.AddWithValue("@userID", UserID);
                c2.Parameters.AddWithValue("@sendDate", sendDate);
                c2.ExecuteNonQuery();
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

        //Retrieves the Reset ID from database to compare
        public Boolean RetrieveResetIDFromDB(string ResetID)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "select * from resetpassword where resetID = @resetID";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@resetID", ResetID);
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    if (r.Read())
                    {
                        DeleteResetIDInDB(r["resetID"].ToString());
                        return true;
                    }
                    else
                        return false;
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

            return false;
        }

        //Deletes reset ID from database
        public void DeleteResetIDInDB(string ResetID)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "delete from resetpassword where resetID = @resetID";
                MySqlCommand c2 = new MySqlCommand(query, conn);
                c2.Parameters.AddWithValue("@resetID", ResetID);
                c2.ExecuteNonQuery();
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

        public string RandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }

            return res.ToString();
        }

        //Updates database with new password keyed in by user
        public void ResetPassword(ResetPasswordViewModel model)
        {
            MySqlConnection conn = GetConnection();

            try
            {
                conn.Open();
                string query = "update users set password = @NewPassword where userID = @UserID";
                MySqlCommand c2 = new MySqlCommand(query, conn);
                string NewPassword = HashPassword(model.NewPassword);
                c2.Parameters.AddWithValue("@NewPassword", NewPassword);
                c2.Parameters.AddWithValue("@UserID", model.UserID);
                c2.ExecuteNonQuery();   
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

        //Retrieves all logs for admin view
        public List<Log> GetAllLogs()
        {
            MySqlConnection conn = GetConnection();
            List<Log> logList = new List<Log>();
            
            try
            {
                conn.Open();
                string query = "select * from actionlogs";
                MySqlCommand c = new MySqlCommand(query, conn);
                using (MySqlDataReader r = c.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Log retrievedLog = new Log
                        {
                            LogID = Convert.ToInt32(r["logID"]),
                            Type = r["type"].ToString(),
                            UserID = Convert.ToInt32(r["userID"]),
                            UserEmail = r["email"].ToString(),
                            ActionPerformed = r["action"].ToString(),
                            DateTimePerformed = Convert.ToDateTime(r["dateTimePerformed"])
                        };

                        logList.Add(retrievedLog);
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

            return logList;
        }

        //Logs the action based on the type
        public void LogAction(string type, string action, Object user)
        {
            
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                if (type == "Login")
                {
                    LoginViewModel loginUser = (LoginViewModel)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", loginUser.UserID);
                    c2.Parameters.AddWithValue("@email", loginUser.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
                }

                if (type == "Registration")
                {
                    User registeredUser = (User)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", registeredUser.UserID);
                    c2.Parameters.AddWithValue("@email", registeredUser.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
                }

                if (type == "2 Factor Authentication")
                {
                    User loggedUser = (User)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", loggedUser.UserID);
                    c2.Parameters.AddWithValue("@email", loggedUser.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
                }

                if (type == "Password Change")
                {
                    User userChanged = (User)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", userChanged.UserID);
                    c2.Parameters.AddWithValue("@email", userChanged.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
                }

                if (type == "Logout")
                {
                    User userLoggedOut = (User)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", userLoggedOut.UserID);
                    c2.Parameters.AddWithValue("@email", userLoggedOut.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
                }

                if (type == "User Deleted")
                {
                    User userLoggedOut = (User)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", userLoggedOut.UserID);
                    c2.Parameters.AddWithValue("@email", userLoggedOut.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
                }

                if (type == "User Edited")
                {
                    User userWhoEdited = (User)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", userWhoEdited.UserID);
                    c2.Parameters.AddWithValue("@email", userWhoEdited.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
                }

                if (type == "Forgot Password")
                {
                    User userWhoForgotPassword = (User)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", userWhoForgotPassword.UserID);
                    c2.Parameters.AddWithValue("@email", userWhoForgotPassword.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
                }

                if (type == "Client")
                {
                    User modifyingUser = (User)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", modifyingUser.UserID);
                    c2.Parameters.AddWithValue("@email", modifyingUser.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
                }

                if (type == "Contract")
                {
                    User modifyingUser = (User)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", modifyingUser.UserID);
                    c2.Parameters.AddWithValue("@email", modifyingUser.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
                }

                if (type == "Service Report")
                {
                    User modifyingUser = (User)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", modifyingUser.UserID);
                    c2.Parameters.AddWithValue("@email", modifyingUser.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
                }

                if (type == "Import/Export Excel")
                {
                    User userLoggedIn = (User)user;
                    string query = "insert into actionlogs(type, userID, email, action, dateTimePerformed) values (@type, @userID, @email, @action, @dateTime)";
                    MySqlCommand c2 = new MySqlCommand(query, conn);
                    c2.Parameters.AddWithValue("@type", type);
                    c2.Parameters.AddWithValue("@action", action);
                    c2.Parameters.AddWithValue("@userID", userLoggedIn.UserID);
                    c2.Parameters.AddWithValue("@email", userLoggedIn.UserEmail);
                    c2.Parameters.AddWithValue("@dateTime", DateTime.Now);
                    c2.ExecuteNonQuery();
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
        }

        //Deletes all logs, function only works for Super Admin
        public void DeleteAllLogs()
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "delete from actionlogs";
                MySqlCommand c = new MySqlCommand(query, conn);
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

        //Resets 2 Factor Authentication
        public void Reset2FactorAuth(string email)
        {
            MySqlConnection conn = GetConnection();
            try
            {
                conn.Open();
                string query = "update users set authenticated = 0 where email = @email";
                MySqlCommand c = new MySqlCommand(query, conn);
                c.Parameters.AddWithValue("@email", email);
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
}
