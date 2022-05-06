using System;
using System.Collections.Generic;
using System.Text;

namespace MyLeasing.Common.Models
{
    public class AddPropertyRequest: PropertyRequest
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
