﻿using System;
using System.Collections.Generic;
using System.Linq;
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


        public void AddToDictionary<T>(List<T> codeTableList) where T : JustWareApi.DataContractBase
        {
            _cache[typeof(T).Name] = codeTableList;
        }

        public List<T> QueryCacheCodeTable<T>(string query) where T : JustWareApi.DataContractBase
        {
            List<T> resultList = new List<T>();
            List<T> cachedList = (List<T>)_cache[typeof (T).Name];
            if (cachedList != null)
            {
                resultList = cachedList;
            }
            return resultList;
        }

        public bool IsCodeTableCached<T>() where T : JustWareApi.DataContractBase
        {
            List<T> cachedList = (List<T>)_cache[typeof(T).Name];
            if (cachedList != null)
            {
                return true;
            }
            return false;
        }
    }
}
