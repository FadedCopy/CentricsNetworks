using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Centrics.Models
{
    public class Contract
    {
        [Display(Name = "Company"),Required]
        public string ClientCompany { get; set; }

        [Display(Name = "MSH"),Required]
        public double MSH { get; set; }

        [Display(Name = "Start of Validity"),DataType(DataType.Date),Required, DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        //[RegularExpression(@"(^(((0[1-9]|[12][0-8])[\/](0[1-9]|1[012]))|((29|30|31)[\/](0[13578]|1[02]))|((29|30)[\/](0[4,6,9]|11)))[\/](19|[2-9][0-9])\d\d$)|(^29[\/]02[\/](19|[2-9][0-9])(00|04|08|12|16|20|24|28|32|36|40|44|48|52|56|60|64|68|72|76|80|84|88|92|96)\s$)")]
        public DateTime StartValid{get; set;}

        //reg date makes invalid
        [Display(Name = "End of Validity"),DataType(DataType.Date),Required, DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        //[RegularExpression(@"(^(((0[1-9]|[12][0-8])[\/](0[1-9]|1[012]))|((29|30|31)[\/](0[13578]|1[02]))|((29|30)[\/](0[4,6,9]|11)))[\/](19|[2-9][0-9])\d\d$)|(^29[\/]02[\/](19|[2-9][0-9])(00|04|08|12|16|20|24|28|32|36|40|44|48|52|56|60|64|68|72|76|80|84|88|92|96)\s$)")]
        public DateTime EndValid { get; set; }
    }
}
