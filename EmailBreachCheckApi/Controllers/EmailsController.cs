﻿using EmailBreachCheckApi.Models;
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


        [HttpGet("/api/Emails")]
        public async Task<ActionResult> Get(string searchString)
        {
            var addressObj = new EmailAddress();

            try
            {
                var status = await _clusterClient.GetGrain<ICacheGrain>("mailAddresses").GetData(searchString);

                addressObj.Response = status == true ? $"Ok - email address: {searchString} found!" : $"NotFound - email address {searchString}";
                addressObj.Address = searchString;

                return Ok(addressObj);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return BadRequest($"Error - GetData");
            }
        }




        [HttpPost]
        public async Task<IActionResult> WriteMailAddress([FromBody] EmailAddress addr)
        {
            try
            {
                var Status = await _clusterClient.GetGrain<ICacheGrain>("mailAddresses").SaveData(addr.Address);
                addr.Response = Status == true ? $"Created - email address: {addr.Address} created successfully." : $"Conflict - email address: {addr.Address} already exists!";

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
