using Microsoft.AspNetCore.Http;
using MyLeasing.Web.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyLeasing.Web.Models
{
    public class PropertyImageViewModel : PropertyImage
    {
        [Display(Name = "Image")]
        public IFormFile ImageFile { get; set; }
    }
}

