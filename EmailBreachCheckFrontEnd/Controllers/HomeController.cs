using EmailBreachCheckFrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System;
using System.Net;

namespace EmailBreachCheckFrontEnd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory? _httpClientFactory;


        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> GetAddressFromApi(EmailAddress addr)
        {
            if (String.IsNullOrEmpty(addr.Address))
            {
                return View("Index", addr);
            }
            addr.Response = "_";
            // Replace "your-api-endpoint" with the actual API endpoint you want to consume
            string apiUrl = $"https://localhost:7272/api/Emails?searchString={addr.Address}";

            // Create an instance of HttpClient using the HttpClientFactory
            using (HttpClient client = new HttpClient())
            {
                // Make a GET request to the API endpoint
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                // Check if the request was successful (status code 200-299)
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response content to a list of emai addresses
                    var address = await response.Content.ReadFromJsonAsync<EmailAddress>();

                    // Pass the list of email addresses to the view
                    return View("Index",address);
                }
                else
                {
                    // Handle the error case
                    return View("Error");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostAddressToApi(EmailAddress addr)
        {
            try
            {
                var emailAddress = new EmailAddress();
                if (String.IsNullOrEmpty(addr.Address))
                {
                    return View("Index", emailAddress);
                }
                addr.Response = "...";
                // Replace "your-api-endpoint" with the actual API endpoint
                string apiUrl = "https://localhost:7272/api/Emails";

                using (HttpClient client = new HttpClient())
                {
                    // Serialize the user input object to JSON
                    string jsonContent = JsonConvert.SerializeObject(addr);

                    // Create the HTTP content with the JSON data
                    HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Send the POST request to the API endpoint
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    // Check if the request was successful (status code 200-299)
                    if (response.IsSuccessStatusCode)
                    {
                        // Optionally, you can handle the response here
                        // For example, you might deserialize the response content
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<EmailAddress>(responseContent);
                        emailAddress.Response = responseObject.Response;
                        
                        // Redirect to a success page or return a success message
                        return View("Index",emailAddress);
                    }
                    else
                    {
                        // Handle the error case
                        // Optionally, you can extract error information from the response
                        string errorMessage = $"Error: {response.StatusCode} - {response.ReasonPhrase}";
                        return View("Error", errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if any
                return View("Error", ex.Message);
            }
        }


        public IActionResult Index(EmailAddress addr)
        {
         
            return View(addr);
        }

        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
