using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace BookingManagmint.Pages
{
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string? StatusCode { get; set; }
        public string? Message { get; set; }

        public void OnGet(string? code = null)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            StatusCode = code;
            Message = code switch
            {
                "404" => "The page you requested was not found.",
                "400" => "Bad request. Please check your inputs.",
                _ => null
            };
        }
    }
}

