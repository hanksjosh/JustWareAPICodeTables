using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using JustWareApiCodeTables.JustWareApi;

namespace JustWareApiCodeTables
{
    public class CodeLookupHelper : ICodeLookup
    {
        private readonly Object _justWareApi;
        private ICodeTableCache _codeTableCache;
        public static readonly string ALL_CODES_QUERY = "Code == Code";

			/// <summary>
			/// Creates the CodeLookupHelper
			/// </summary>
			/// <param name="justWareApi">Reference to an IJustWareApi for the CodeLookupHelper to access.</param>
			/// <param name="codeTableCache">Object to use for caching.  Can be the CodeTableCache for windows applications or the WebCodeTableCache for Web Applications</param>
        public CodeLookupHelper(Object justWareApi, ICodeTableCache codeTableCache = null)
        {
            if (justWareApi == null)
            {
                throw new ArgumentNullException("justWareApi");
            }
            _justWareApi = justWareApi;
            _codeTableCache = codeTableCache;
        }

			/// <summary>
			/// Returns the description for the given code, returning from the cache if available.
			/// </summary>
			/// <typeparam name="T">Type of the Code Table to search in</typeparam>
			/// <param name="code">Code that specifies which description to return</param>
			/// <returns>Description value of T </returns>
        public string GetCodeDescription<T>(string code)
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

			/// <summary>
			/// Returns a list of CodeTable types that fit the query, returning from the cache if available
			/// </summary>
			/// <typeparam name="T">Type of the Code Table to search in</typeparam>
			/// <param name="query">Query for which codes to return (Same as the API Query language)</param>
			/// <returns>List of the code tables that fit the query.</returns>
        public List<T> QueryCodeTable<T>(string query)
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

			/// <summary>
			/// Returns the entire Code Table, returning from the cache if available.
			/// </summary>
			/// <typeparam name="T">Type of the Code Table requested</typeparam>
			/// <returns>Entire Code Table</returns>
        public List<T> GetCodeTable<T>()
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
			/// <summary>
			/// Clears the cache for the specified Type
			/// </summary>
			/// <typeparam name="T">Type of Code Table to clear.</typeparam>
				public void ClearCache<T>()
				{
					if (_codeTableCache != null)
					{
						_codeTableCache.ClearCache<T>();
					}
				}

			/// <summary>
			/// Clears all of the cached code tables.
			/// </summary>
				public void ClearAllCache()
				{
					if (_codeTableCache != null)
					{
						_codeTableCache.ClearAllCache();
					}
				}
    }
}