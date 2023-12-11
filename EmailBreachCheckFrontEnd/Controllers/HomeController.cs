using EmailBreachCheckFrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

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
            
            string apiUrl = $"https://localhost:7272/api/Emails?searchString={addr.Address}";
          
            using (HttpClient client = new HttpClient())
            {               
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var address = await response.Content.ReadFromJsonAsync<EmailAddress>();
                    return View("Index", address);
                }
                else
                {
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
                
                string apiUrl = "https://localhost:7272/api/Emails";

                using (HttpClient client = new HttpClient())
                {                  
                    string jsonContent = JsonConvert.SerializeObject(addr);
                  
                    HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                 
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<EmailAddress>(responseContent);
                        emailAddress.Response = responseObject.Response;

                        
                        return View("Index", emailAddress);
                    }
                    else
                    {
                        string errorMessage = $"Error: {response.StatusCode} - {response.ReasonPhrase}";
                        return View("Error", errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }


        public IActionResult Index(EmailAddress addr)
        {
            return View(addr);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
