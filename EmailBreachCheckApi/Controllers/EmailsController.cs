using EmailBreachCheckApi.Models;
using Microsoft.AspNetCore.Mvc;
using Orleans;


namespace EmailBreachCheckApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class EmailsController : ControllerBase
    {
        private readonly IClusterClient _clusterClient;
        private readonly ILogger<EmailsController> _logger;

        public EmailsController(ILogger<EmailsController> logger, IClusterClient clusterClient)
        {
            _logger = logger;
            _clusterClient = clusterClient;
        }

        // GET: api/<EmailsController>/Parameter
        [HttpGet("/api/Emails")]
        public async Task<ActionResult> Get(string searchString)
        {
            var addressObj = new EmailAddress();
            
            try
            {
                var status = await _clusterClient.GetGrain<ICacheGrain>("mailAddresses").GetData(searchString);
       
                addressObj.Response = status==true?$"Ok - Email address {searchString} found!": $" NotFound - Email address {searchString}";
                addressObj.Address = searchString;
                
                return Ok(addressObj);
            } 
            catch(Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return BadRequest($"Error - GetData");
            }
        }

        

        // POST api/<EmailsController>
        [HttpPost]
        public async Task<IActionResult> WriteMailAddress([FromBody] EmailAddress addr)
        {         
            try
            {
                var Status = await _clusterClient.GetGrain<ICacheGrain>("mailAddresses").SaveData(addr.Address);
                addr.Response = Status==true?$"Created - Email address: {addr.Address} created successfully.": $"Conflict - Email address: {addr.Address} already exists!";
                
                return Ok(addr);
                          
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return BadRequest($"Error - SaveData");
            }
  
        }

        

        
    }
}
