
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Centrics.Models
{

    public class Importer
    {
        [Required(ErrorMessage = "Please specify a file")]
        public IFormFile File { get; set; }

    }
    
}
