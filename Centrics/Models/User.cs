using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    public class User
    {
        //User Details for Login & Registration
        [Display(Name = "User ID"), Required]
        public int UserID { get; set; }

        [Display(Name = "First Name"), Required, StringLength(20)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name"), Required, StringLength(20)]
        public string LastName { get; set; }

        [Display(Name = "Username"), Required, StringLength(20)]
        public string Username { get; set; }

        [Display(Name = "Email"), Required, StringLength(100), DataType(DataType.EmailAddress)]
        public string UserEmail{ get; set; }

        [Display(Name = "Password"), Required, StringLength(20), DataType(DataType.Password), 
        RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", ErrorMessage = "Password must contain: Minimum 8 characters at least 1 uppercase alphabet, 1 lowercase alphabet and 1 numerical value.")]
        public string UserPassword { get; set; }

        [Display(Name = "Confirm Password"), Required, StringLength(20), Compare("UserPassword", ErrorMessage = "Confirm password and password do not match"), DataType(DataType.Password)]
        public string UserCfmPassword { get; set; }
    }
}
