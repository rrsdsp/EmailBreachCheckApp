using EmailBreachCheckApi.Controllers;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;

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

            // Set up a timer to periodically persist state
            timer = RegisterTimer(PersistState, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public override async Task OnDeactivateAsync()
        {
            // Dispose of the timer when the grain deactivates
            timer.Dispose();
            await base.OnDeactivateAsync();
        }

        public async Task<List<string>> GetAllData()
        {
            try
            {
                await _emailState.ReadStateAsync();
                return _emailState.State.EmailAddresses;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return null;
            }
        }
        public async Task<bool> GetData(string value)
        {
            try
            {
                await _emailState.ReadStateAsync();
                return IsInList(_emailState.State.EmailAddresses, value) == true ? true : false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StoreData(List<string> valueList)
        {
            try
            {
                await _emailState.ReadStateAsync();
                _emailState.State.EmailAddresses = valueList;
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
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return false;
            }
        }

        private async Task PersistState(object _)
        {
            try
            {
                await _emailState.WriteStateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
            }
        }

        private bool IsInList(List<string> emaislList, string address)
        {
            try
            {
                return emaislList.Contains(address) ? true : false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return false;
            }
        }
    }
}
