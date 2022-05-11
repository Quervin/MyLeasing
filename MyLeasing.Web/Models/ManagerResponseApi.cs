using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLeasing.Web.Models
{
    public class ManagerResponseApi
    {
        public int Id { get; set; }

        public UserResponseApi User { get; set; }
    }
}
