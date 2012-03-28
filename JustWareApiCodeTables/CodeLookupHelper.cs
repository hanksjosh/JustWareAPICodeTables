using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using JustWareApiCodeTables.JustWareApi;

namespace JustWareApiCodeTables
{
    public class CodeLookupHelper : ICodeLookup
    {
        private readonly IJustWareApi _justWareApi;
        private ICodeTableCache _codeTableCache;
        public static readonly string ALL_CODES_QUERY = "Code == Code";

        public CodeLookupHelper(IJustWareApi justWareApi, ICodeTableCache codeTableCache = null)
        {
            if (justWareApi == null)
            {
                throw new ArgumentNullException("justWareApi");
            }
            _justWareApi = justWareApi;
            _codeTableCache = codeTableCache;
        }

        public string GetCodeDescription<T>(string code) where T : DataContractBase
        {
            Type entityType = typeof(T);
            VerifyEntityIsCodeTable(entityType);

            PropertyInfo descriptionProperty = entityType.GetPropertiesOfType<string>().SingleOrDefault(t => t.Name.Equals("Description", StringComparison.OrdinalIgnoreCase));

            var searchResults = QueryCodeTable<T>(String.Format("Code == \"{0}\"", code));

            T codeEntity = searchResults.SingleOrDefault();

            if (codeEntity != null)
            {
                return descriptionProperty.GetValue<string>(codeEntity);
            }
            return null;
        }

        public List<T> QueryCodeTable<T>(string query) where T : DataContractBase
        {
            List<T> codeTables = new List<T>();
            if (_codeTableCache == null)
            {
                codeTables = FindCodeTableFromApi<T>(query);
            }
            else
            {
                if (!_codeTableCache.IsCodeTableCached<T>())
                {
                    List<T> fullCodeTable = FindCodeTableFromApi<T>(ALL_CODES_QUERY);
                    _codeTableCache.AddToDictionary<T>(fullCodeTable);
                    
                }
                codeTables = _codeTableCache.QueryCacheCodeTable<T>(query);
            }
            return codeTables;
        }

        private List<T> FindCodeTableFromApi<T>(string query)
        {
            VerifyEntityIsCodeTable(typeof(T));
            MethodInfo method = _justWareApi.GetType().GetReflectionInfo().Methods.Single(m => (typeof(IEnumerable<T>)).IsAssignableFrom(m.ReturnType) && m.Name.StartsWith("Find", StringComparison.OrdinalIgnoreCase));
            return (List<T>)method.Invoke(_justWareApi, parameters: new object[] { query, null });
        }

        public List<T> GetCodeTable<T>() where T : DataContractBase
        {
            List<T> list = new List<T>();
            if (_codeTableCache != null)
            {
                list = _codeTableCache.QueryCacheCodeTable<T>(ALL_CODES_QUERY);
            }
            if (list.Count() == 0)
            {
                list = FindCodeTableFromApi<T>(ALL_CODES_QUERY);
                if (_codeTableCache != null)
                {
                    _codeTableCache.AddToDictionary(list);
                }
            }
            return list;
        }

        private static void VerifyEntityIsCodeTable(Type entityType)
        {
            if (entityType.GetPropertiesOfType<string>().Count(p => p.Name.Equals("Description", StringComparison.OrdinalIgnoreCase)
                                                                    || p.Name.Equals("Code", StringComparison.OrdinalIgnoreCase)) != 2)
            {
                throw new ApplicationException(String.Format("Entity '{0}' is not a code type.", entityType.Name));
            }
        }
    }
}