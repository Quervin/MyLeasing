using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLeasing.Web.Models
{
    public class PropertyImageResponseApi
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; }

        // TODO: Change the path when publish
        public string ImageFullPath => string.IsNullOrEmpty(ImageUrl)
            ? null
            : $"https://myleasinghidalgo.azurewebsites.net{ImageUrl.Substring(1)}";

        public PropertyResponseApi Property { get; set; }
    }
}
