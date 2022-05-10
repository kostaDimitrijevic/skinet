using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IResponseCacheService
    {
        Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive);//kes ce imati kljuc i response timeToLive koliko ce biti u redisu
        Task<string> GetCachedResponseAsync(string cacheKey);
    }
}