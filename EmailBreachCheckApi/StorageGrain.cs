using EmailBreachCheckApi.Controllers;
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
    public class EmailAddressPersistenceState
    {
        public List<string> EmailAddresses { get; set; }
    }

    [StorageProvider(ProviderName = "azureStorage")]
    public class StorageGrain : Grain<EmailAddressPersistenceState>, IStorageGrain
    {
        private readonly ILogger<EmailsController> _logger;
        private IDisposable timer;
        private readonly IPersistentState<EmailAddressState> _emailState;

        public StorageGrain(ILogger<EmailsController> logger,
        [PersistentState("emailState","azureStorage")]
            IPersistentState<EmailAddressState> emailState)
        {
            _logger = logger;
            _emailState = emailState;
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
        }

        public override async Task OnDeactivateAsync()
        {
            await base.OnDeactivateAsync();
        }

        public async Task<List<string>> GetAllData()
        {
            await _emailState.ReadStateAsync();
            return _emailState.State.EmailAddresses;
        }
        public async Task<bool> GetData(string value)
        {
            try
            {
                await _emailState.ReadStateAsync();
                return IsInTheList(_emailState.State.EmailAddresses, value) == true ? true : false;    
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StoreData(List<string> value)
        {
            try
            {
                _emailState.State.EmailAddresses = value;
                await _emailState.WriteStateAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ClearStorage()
        {
            try
            {
                await _emailState.ClearStateAsync();
                return true;
            }
            catch(Exception ex)
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
    }
}
