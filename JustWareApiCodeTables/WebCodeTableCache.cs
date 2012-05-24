using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Web.Caching;

namespace JustWareApiCodeTables
{
    public class WebCodeTableCache : ICodeTableCache
    {
        private Cache _cache;
        public WebCodeTableCache(Cache cache)
        {
            _cache = cache;
        }


        public void AddToDictionary<T>(List<T> codeTableList)
        {
            _cache[typeof(T).Name] = codeTableList;
        }

        public List<T> QueryCacheCodeTable<T>(string query)
        {
            List<T> resultList = new List<T>();
            List<T> cachedList = (List<T>)_cache[typeof (T).Name];
            if (cachedList != null)
            {
                IQueryable<T> currentList = cachedList.AsQueryable();
                resultList = currentList.DynamicWhere(query).ToList();
            }
            return resultList;
        }

        public bool IsCodeTableCached<T>()
        {
            List<T> cachedList = (List<T>)_cache[typeof(T).Name];
            if (cachedList != null)
            {
                return true;
            }
            return false;
        }

				public void ClearCache<T>()
				{
					_cache.Remove(typeof(T).Name);
				}

				public void ClearAllCache()
				{
					throw new NotSupportedException("ClearAllCache is not supported for the WebCodeTableCache");
				}
    }
}
