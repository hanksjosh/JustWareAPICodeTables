using System.Collections.Generic;
using JustWareApiCodeTables.JustWareApi;

namespace JustWareApiCodeTables
{
    public interface ICodeTableCache
    {
        void AddToDictionary<T>(List<T> codeTableList) where T : DataContractBase;
        List<T> QueryCacheCodeTable<T>(string query) where T: DataContractBase;
        bool IsCodeTableCached<T>() where T: DataContractBase;
    }
}