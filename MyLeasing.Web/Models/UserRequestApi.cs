using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyLeasing.Web.Models
{
    public class UserRequestApi
    {
        [Required]
        public string UserId { get; set; }
    }
}
