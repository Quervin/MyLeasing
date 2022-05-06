using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyLeasing.Common.Models
{
    public class AddContractRequest
    {
        public int Id { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string Remarks { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public int PropertyTypeId { get; set; }

        [Required]
        public int LesseeId { get; set; }

        [Required]
        public int OwnerId { get; set; }
    }
}
