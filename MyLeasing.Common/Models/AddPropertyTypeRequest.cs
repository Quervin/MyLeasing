using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyLeasing.Common.Models
{
    public class AddPropertyTypeRequest
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
