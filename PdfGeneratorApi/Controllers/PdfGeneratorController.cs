using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using PdfGeneratorApi.Dtos;
using PdfGeneratorApi.Interfaces;
using PdfGeneratorApi.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;


namespace PdfGeneratorApi.Controllers
{
    // [Authorize]
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("[controller]")]
    public class PdfGeneratorController : ControllerBase

    {
        private readonly IPdfGeneratorRepository _pdfGeneratorRepository;
        public PdfGeneratorController(IPdfGeneratorRepository pdfGeneratorRepository)
        {
            _pdfGeneratorRepository = pdfGeneratorRepository;
        }

        [Microsoft.AspNetCore.Mvc.Route("/pdfGenerate")]
        [HttpPost]
        public async Task<IActionResult> PdfGenerator([FromForm] ObjectRequestDto objectRequestDto)

        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try{
                byte[] generatedPdf = await _pdfGeneratorRepository.GetDataFromPDFGeneratorModel(objectRequestDto);
                return new FileContentResult(generatedPdf, "application/pdf")
                {
                    FileDownloadName = "report.pdf"
                };
            }
            catch (Exception)
            {
                // ILogger.LogError($"Error occurred in {nameof(PdfGenerator)}: {ex.Message}");
                return StatusCode(500, "An error occurred while generating the PDF.");
            }

        }
    }
}