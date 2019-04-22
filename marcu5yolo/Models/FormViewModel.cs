using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace marcu5yolo.Models
{
    public class FormViewModel
    {
        [Required] public IFormFile imageUpload { get; set; }
        [Required] public IFormFile styleUpload { get; set; }
    }
}
