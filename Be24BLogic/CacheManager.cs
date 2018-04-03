using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;


namespace Be24BLogic
{
    public    class CacheManager
    {


        private static   IMemoryCache _cache;
        private static TimeSpan _tsp;

        public   CacheManager(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            int hval = 1;
           if( int.TryParse(SettingsManager.CacheTimeSpan,out hval))
             _tsp = new TimeSpan(hval, 0, 0); //время кэша (час, мин, сек)
            else
            {
                _tsp = new TimeSpan(0, 0, 10);
            }
        }

        public static bool Remove(string id )
        {
            _cache.Remove(id );
            return true;
        }


        public static bool Put(string id, object item)
        {
            _cache.Set(id, item);
            return true;
        }


        public static bool Put(string id, object item, TimeSpan T)
        {
            _cache.Set(id, item,T);
            return true;
        }


        public static bool PutT(string id, object item)
        {
            _cache.Set(id, item, _tsp);
            return true;
        }


        public static bool Contains(string id)
        {
            object val;
            return _cache.TryGetValue(id,out val);

        }


        public static object Get (string id)
        {
            return _cache.Get(id);
        }


    }
}
