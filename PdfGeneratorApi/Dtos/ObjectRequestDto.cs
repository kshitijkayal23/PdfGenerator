using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PdfGeneratorApi.Dtos
{
    public class ObjectRequestDto
    {
        [Required]
        public IFormFile? csv_File { get; set; }
        [Required]
        public IFormFile? cover_Image { get; set; }
        [Required]
        public IFormFile? logo_Image { get; set; }
        [Required]
        public string? site_info { get; set; }
    }
}