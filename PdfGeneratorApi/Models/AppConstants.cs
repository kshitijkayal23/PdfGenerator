using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PdfGeneratorApi.Models
{
    public class AppConstants
    {
        public const string PDFGeneratorModelBaseUrl = "EdgeModuleConfiguration:PDFGeneratorModelBaseUrl";
        public const string PDFGeneratorUrl = "/api/generate";
        public const string AuthScheme = "AuthScheme";
        public const string AllowedOrigins = "AllowedOrigins";
        public const string EdgeAuthenticationServerUrl = "EdgeModuleConfiguration:AuthenticationServerUrl";
        public const string AuthenticationServerUrl = "authentication_server_url";
        public const string AbilityB2CAuthScheme = "AbilityB2CAuthScheme";
        public const string AbilityB2CAudience = "AbilityB2C:Audience";
        public const string AbilityB2CInstance = "AbilityB2C:Instance";
        public const string AbilityB2CDomain = "AbilityB2C:Domain";
        public const string AbilityB2CSolution = "AbilityB2C:Solution";
    }
}