using Orleans;
using System.Threading.Tasks;

namespace EmailBreachCheckApi
{
    public interface IStorageGrain : IGrainWithStringKey
    {
        Task<bool> GetData(string value);
        Task<List<string>> GetAllData();
        Task<bool> StoreData(List<string> value);
        Task<bool> ClearStorage();
    }
}
