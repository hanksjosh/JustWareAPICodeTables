using System.Collections.Generic;
using JustWareApiCodeTables.JustWareApi;

namespace JustWareApiCodeTables
{
    public interface ICodeTableCache
    {
        void AddToDictionary<T>(List<T> codeTableList);
        List<T> QueryCacheCodeTable<T>(string query);
        bool IsCodeTableCached<T>();
    }
}