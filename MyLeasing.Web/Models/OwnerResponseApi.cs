using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLeasing.Web.Models
{
    public class OwnerResponseApi
    {
        public int Id { get; set; }

        public UserResponseApi User { get; set; }

        public ICollection<PropertyResponseApi> Properties { get; set; }

        public ICollection<ContractResponseApi> Contracts { get; set; }
    }
}
