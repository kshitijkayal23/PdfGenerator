using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PdfGeneratorApi.Dtos;
using PdfGeneratorApi.Interfaces;
using PdfGeneratorApi.Models;

namespace PdfGeneratorApi.Repository
{
    public class PdfGeneratorRepository : IPdfGeneratorRepository
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly string PDFGeneratorModelBaseUrl;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PdfGeneratorRepository> _logger;
        public PdfGeneratorRepository(IHttpContextAccessor accessor, IConfiguration configuration, ILogger<PdfGeneratorRepository> logger, IHttpClientFactory httpClientFactory)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            PDFGeneratorModelBaseUrl = _configuration[AppConstants.PDFGeneratorModelBaseUrl] ?? string.Empty;
            _logger = logger;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<byte[]> GetDataFromPDFGeneratorModel(ObjectRequestDto objectRequestDto)
        {
            _logger.LogDebug($"Processing for {nameof(GetDataFromPDFGeneratorModel)} started at {DateTime.Now}");

            try
            {
                var csvFile = objectRequestDto.csv_File;
                var coverImage = objectRequestDto.cover_Image;
                var logoImage = objectRequestDto.logo_Image;

                if (csvFile?.Length > 0 && coverImage?.Length > 0 && logoImage?.Length > 0)
                {
                    var csvPath = Path.Combine(Path.GetTempPath(), csvFile.FileName);
                    using (var fs = new FileStream(csvPath, FileMode.Create))
                    {
                        await csvFile.CopyToAsync(fs);
                    }

                    var coverImagePath = Path.Combine(Path.GetTempPath(), coverImage.FileName);
                    using (var fs = new FileStream(coverImagePath, FileMode.Create))
                    {
                        await csvFile.CopyToAsync(fs);
                    }

                    var logoImagePath = Path.Combine(Path.GetTempPath(), logoImage.FileName);
                    using (var fs = new FileStream(logoImagePath, FileMode.Create))
                    {
                        await csvFile.CopyToAsync(fs);
                    }

                    byte[] generatedPDF = await SendFileToGenPdfAsync(csvPath, coverImagePath, logoImagePath, objectRequestDto);

                    // Delete the temporary file
                    System.IO.File.Delete(csvPath);
                    System.IO.File.Delete(coverImagePath);
                    System.IO.File.Delete(logoImagePath);

                    return generatedPDF;
                }
                else
                {
                    throw new ArgumentException("File is empty or invalid.");
                }

            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.BadRequest)
                {
                    string jsonPayload = System.Text.Json.JsonSerializer.Serialize(objectRequestDto);
                    string errorMessage = ex.Message;
                    _logger.LogError($"Bad request error occurred in {nameof(GetDataFromPDFGeneratorModel)} for input request body {jsonPayload}. Error message: {errorMessage}");
                    throw new HttpRequestException("Bad request error", ex);
                }
                else
                {
                    _logger.LogError($"Error occurred in {nameof(GetDataFromPDFGeneratorModel)}: {ex.Message}");
                    throw;
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error occurred in {nameof(GetDataFromPDFGeneratorModel)}: {ex.Message}");
                throw;
            }
        }

        private async Task<byte[]> SendFileToGenPdfAsync(string csvPath, string coverImagePath, string logoImagePath, ObjectRequestDto objectRequestDto)
        {
            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(objectRequestDto, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using var jsonContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            string url = $"{AppConstants.PDFGeneratorUrl}";  // Ensure this URL points to your Flask API.

            using MultipartFormDataContent multipartContent = new();
            using var httpClient = CreateHttpClient();
            var accessToken = GetAccessToken();
            httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);

            // Add JSON content (site_info, etc.)
            multipartContent.Add(jsonContent, "otherJsonContent");

            // Add the CSV file
            var csvContent = new ByteArrayContent(await File.ReadAllBytesAsync(csvPath));
            csvContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            multipartContent.Add(csvContent, "csvFile", Path.GetFileName(csvPath));

            // Add the cover image
            var coverImageContent = new ByteArrayContent(await File.ReadAllBytesAsync(coverImagePath));
            coverImageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            multipartContent.Add(coverImageContent, "coverImage", Path.GetFileName(coverImagePath));

            // Add the logo image
            var logoContent = new ByteArrayContent(await File.ReadAllBytesAsync(logoImagePath));
            logoContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            multipartContent.Add(logoContent, "logoImage", Path.GetFileName(logoImagePath));

            try
            {
                // Make the POST request to the Flask API
                HttpResponseMessage response = await httpClient.PostAsync(url, multipartContent);
                response.EnsureSuccessStatusCode();
                if(response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine(response.ReasonPhrase);
                }

                // Return the PDF content (assuming it is returned in the response body)
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            
        }

        private HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(PDFGeneratorModelBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }
        private string GetAccessToken()
        {
            string accessToken = _accessor.HttpContext?.Request?.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }
            else
            {
                return accessToken;
            }
        }
    }
}