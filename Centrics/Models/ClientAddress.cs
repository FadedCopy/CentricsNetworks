using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    public class ClientAddress
    {

        public string CustomerID { get; set; }

        [Display(Name = "Company Name"), Required(ErrorMessage = "Please enter a Company  Name")]
        public string ClientCompany { get; set; }

        [Required(ErrorMessage = "Please enter a Address")]
        public string Address { get; set; }

        public string Contact { get; set; }

        public int ContactNo { get; set; }

        public string EmailAddress { get; set; }

        public string Title { get; set; }

        public List<string> Addresslist { get; set; }

        public List<string> ContactNoList { get; set; }

        public List<string> ContactList { get; set; }

        public List<string> EmailList { get; set; }

        public List<string> TitleList { get; set; }

        public string ContactNoString { get; set; }


    }

    public class searcher {
        [Display(Name = "Search for company")]
        public string searchvalue { get; set; }
    }
}
