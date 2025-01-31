﻿using System.Collections.Generic;

namespace MyLeasing.Common.Models
{
    public class OwnerResponse
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Document { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
        public ICollection<PropertyResponse> Properties { get; set; }

        public ICollection<ContractResponse> Contracts { get; set; }
        public int RoleId { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}

