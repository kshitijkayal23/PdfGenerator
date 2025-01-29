using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PdfGeneratorApi.Models
{
    public class PdfGenertorResponse
    {
        public IFormFile File { get; set; }
        public string csv_path { get; set; }
        public string pdf_path { get; set; }
        public string cover_image_path { get; set; }
        public string logo_path { get; set; }
        public string site_info { get; set; }
        public string Extension { get; set; }
    }
}