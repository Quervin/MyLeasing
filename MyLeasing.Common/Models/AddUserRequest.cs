using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyLeasing.Common.Models
{
    public class AddUserRequest: EditUserRequest
    {

        [Required]
        [StringLength(20, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        public int RoleId { get; set; } // 1: Owner, 2: Lessee
    }
}
