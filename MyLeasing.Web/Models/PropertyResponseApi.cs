using MyLeasing.Web.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLeasing.Web.Models
{
    public class PropertyResponseApi
    {
        public int Id { get; set; }

        public string Neighborhood { get; set; }

        public string Address { get; set; }

        public decimal Price { get; set; }

        public int SquareMeters { get; set; }

        public int Rooms { get; set; }

        public int Stratum { get; set; }

        public bool HasParkingLot { get; set; }


        public bool IsAvailable { get; set; }

        public string Remarks { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public PropertyTypeResponseApi PropertyType { get; set; }

        public OwnerResponseApi Owner { get; set; }

        public ICollection<PropertyImageResponseApi> PropertyImages { get; set; }

        public ICollection<ContractResponseApi> Contracts { get; set; }

        public string FirstImage => PropertyImages == null || PropertyImages.Count == 0
            ? "~/images/Properties/noImage.png"
             : PropertyImages.FirstOrDefault().ImageUrl;

        //"https://myleasinghidalgo.azurewebsites.net/images/Properties/noImage.png"
    }
}
