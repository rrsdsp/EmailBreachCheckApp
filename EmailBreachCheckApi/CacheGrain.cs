using EmailBreachCheckApi.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orleans;
using Orleans.Persistence;
using Orleans.Providers;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;


namespace EmailBreachCheckApi
{
    [Serializable]
    public class EmailAddressState
    {
        public List<string> EmailAddresses { get; set; }
    }

    [StorageProvider(ProviderName = "cacheStorage")]
    
    public class CacheGrain : Grain<EmailAddressState>, ICacheGrain
    {

        private readonly ILogger<EmailsController> _logger;
        private IDisposable timer;
        private readonly IPersistentState<EmailAddressState> _emailState;
        private readonly IClusterClient _clusterClient;

        public CacheGrain(ILogger<EmailsController> logger, IClusterClient clusterClient,
        [PersistentState("emailState","cacheStorage")]
            IPersistentState<EmailAddressState> emailState)
        {
            _logger = logger;
            _emailState = emailState;
            _clusterClient = clusterClient;
        }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

            //// Load state on activation
            await ReadStateAsync();
            // Set a default value if no state is loaded
            if (_emailState.State.EmailAddresses == null)
            {
                List<string> mailList = new List<string>() { "No record" };
                _emailState.State.EmailAddresses = mailList;
            }
                

            

            // Set up a timer to periodically persist state
            timer = RegisterTimer(PersistState, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public override async Task OnDeactivateAsync()
        {
            // Dispose of the timer when the grain deactivates
            timer.Dispose();
            await base.OnDeactivateAsync();
        }

        
        public async Task<bool> GetData(string value)
        {          
            try
            {
                await _emailState.ReadStateAsync();
                var addressStatus = IsInTheList(_emailState.State.EmailAddresses, value);
                if (addressStatus == false)
                {                   
;                   var addr = await _clusterClient.GetGrain<IStorageGrain>("azure").GetData(value);
                    if (addr==false)
                    {                      
                        return false;
                    }                                                        
                }
                var updateList = AddToList(_emailState.State.EmailAddresses, value);
                _emailState.State.EmailAddresses = updateList;
                return true;

            }
            catch(Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return false;
            }          
        }

        public async Task<bool> SaveData(string value)
        {          
            try
            {
                var status = IsInTheList(_emailState.State.EmailAddresses, value);
                if (status == true)
                {
                    return false;
                }
                _emailState.State.EmailAddresses = AddToList(_emailState.State.EmailAddresses, value);
                await _emailState.WriteStateAsync(); 
                return  true;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return false;
            } 
        }

        private async Task PersistState(object _)
        {
            try
            {
                var result = await _clusterClient.GetGrain<IStorageGrain>("azure").GetAllData();
                if (result.Count > 0)
                {
                    await _clusterClient.GetGrain<IStorageGrain>("azure").ClearStorage();
                }
                var addEmailAddressStatus = await _clusterClient.GetGrain<IStorageGrain>("azure").StoreData(_emailState.State.EmailAddresses);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
            }          
        }

        public async Task<bool> ClearStorage()
        {
            try
            {
                await _emailState.ClearStateAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return false;
            }

        }
        private bool IsInTheList(List<string> emaislList, string address)
        {
            try
            {
                var result = emaislList.Contains(address) ? true : false;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return false;
            }
        }
        private List<string> AddToList(List<string> emailsList, string emailAddress)
        {
            try
            {
                var mails = emailsList;
                mails.Add(emailAddress);
                return mails;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return null;
            }
        }

        
    }
}
