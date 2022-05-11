using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLeasing.Web.Models
{
    public class ContractResponseApi
    {
        public int Id { get; set; }
        public string Remarks { get; set; }

        public decimal Price { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime StartDateLocal => StartDate.ToLocalTime();

        public DateTime EndDateLocal => EndDate.ToLocalTime();

        public OwnerResponseApi Owner { get; set; }
        public LesseeResponseApi Lessee { get; set; }
        public PropertyResponseApi Property { get; set; }
    }
}
