using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JustWareApiCodeTables.JustWareApi;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Dynamic;

namespace JustWareApiCodeTables
{
    public class CodeTableCache : ICodeTableCache
    {
        public Dictionary<Type, Object> CodeTableDictionary { get; private set; }

        public CodeTableCache()
        {
            CodeTableDictionary = new Dictionary<Type, object>();
        }
        public void AddToDictionary<T>(List<T> codeTableList)
        {
            Type currentType = typeof(T);
            CodeTableDictionary[currentType] = codeTableList;
        }

        public List<T> QueryCacheCodeTable<T>(string query)
        {
            Type currentType = typeof(T);
            List<T> resultList = new List<T>();
            if (IsCodeTableCached<T>())
            {
                IQueryable<T> currentList = ((List<T>)CodeTableDictionary[currentType]).AsQueryable();
                resultList = currentList.DynamicWhere(query).ToList();
            }

            return resultList;
        }

        public bool IsCodeTableCached<T>()
        {
            return CodeTableDictionary.ContainsKey(typeof (T));
        }

				public void ClearCache<T>()
				{
					CodeTableDictionary.Remove(typeof(T));
				}

				public void ClearAllCache()
				{
					CodeTableDictionary.Clear();
				}

    }


}