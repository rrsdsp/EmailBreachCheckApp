using EmailBreachCheckApi.Controllers;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;


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
            await ReadStateAsync();
            
            
            var dataList = await _clusterClient.GetGrain<IStorageGrain>("azure").GetAllData();
            _emailState.State.EmailAddresses = dataList;
            await _emailState.WriteStateAsync();

            timer = RegisterTimer(PersistState, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public override async Task OnDeactivateAsync()
        {

            timer.Dispose();
            await base.OnDeactivateAsync();
        }


        public async Task<bool> GetData(string value)
        {
            try
            {
                await _emailState.ReadStateAsync();
                var Status = IsInList(_emailState.State.EmailAddresses, value);
                if (Status == false)
                {
                    var addr = await _clusterClient.GetGrain<IStorageGrain>("azure").GetData(value);
                    if (addr == false)
                    {
                        return false;
                    }
                }
                var updateList = AddToList(_emailState.State.EmailAddresses, value);
                _emailState.State.EmailAddresses = updateList;
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error! - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SaveData(string value)
        {
            try
            {
                await _emailState.ReadStateAsync();
                var status = IsInList(_emailState.State.EmailAddresses, value);
                if (status == true)
                {
                    return false;
                }
                var listStatus = AddToList(_emailState.State.EmailAddresses, value);
                if (listStatus != null)
                {
                    _emailState.State.EmailAddresses = AddToList(_emailState.State.EmailAddresses, value);
                    await _emailState.WriteStateAsync();
                }

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
                await _emailState.ReadStateAsync();
                var status = await _clusterClient.GetGrain<IStorageGrain>("azure").StoreData(_emailState.State.EmailAddresses);
                if (status == true)
                {
                    _logger.LogInformation("Succesfully stored to Azure Blob service!");
                }
            }
            catch (Exception ex)
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
        private bool IsInList(List<string> emailsList, string address)
        {
            try
            {
                var result = emailsList.Contains(address) ? true : false;
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
