﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLeasing.Web.Models
{
    public class ImageRequestApi
    {
        public int Id { get; set; }

        public int PropertyId { get; set; }

        public IFormFile ImageFile { get; set; }
    }
}
