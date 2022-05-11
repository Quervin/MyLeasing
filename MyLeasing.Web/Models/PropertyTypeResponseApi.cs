using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLeasing.Web.Models
{
    public class PropertyTypeResponseApi
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<PropertyResponseApi> Properties { get; set; }
    }
}
