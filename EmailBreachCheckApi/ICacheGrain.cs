using Orleans;
using System.Threading.Tasks;

namespace EmailBreachCheckApi
{
    public interface ICacheGrain : IGrainWithStringKey
    {
        Task<bool> GetData(string value);
        Task<bool> SaveData(string value);
        Task<bool> ClearStorage();
        


    }
}
