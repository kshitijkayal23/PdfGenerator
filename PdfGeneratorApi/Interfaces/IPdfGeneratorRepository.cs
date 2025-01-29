using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PdfGeneratorApi.Dtos;
using PdfGeneratorApi.Models;

namespace PdfGeneratorApi.Interfaces
{
    public interface IPdfGeneratorRepository
    {
        Task<byte[]> GetDataFromPDFGeneratorModel(ObjectRequestDto objectRequestDto);

    }
}