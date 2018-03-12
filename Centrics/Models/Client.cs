using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    //A parent can have many childs? a child can have 1 parent.
    //double confirm this
    public class ParentClient
    {
        private CentricsContext context;

        [Display(Name = "Company Name"), Required, StringLength(50)]
        public string ClientCompanyName { get; set; }
    }

    public class Client
    {
        private CentricsContext context;

        //Client Details
        [Display(Name = "Company Name"), Required,StringLength(50)]
        public string ClientCompanyName { get; set; }

        [Display(Name = "Address"), StringLength(200), Required]
        public string ClientAddress { get; set; }


        [Display(Name = "Tel / HP"), Required] //Regular Expression? Max Length 8?
        public int ClientTel { get; set; }

        [Display(Name = "Contact Person"), Required, StringLength(50)]
        public string ClientContactPerson { get; set; }

        [Display(Name = "Email Address"),Required, StringLength(100), DataType(DataType.EmailAddress)]
        public string ClientEmailAddress { get; set; }
    }
}
