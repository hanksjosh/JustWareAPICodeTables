using System.Collections.Generic;
using JustWareApiCodeTables.JustWareApi;

namespace JustWareApiCodeTables
{
    public interface ICodeLookup
    {
        string GetCodeDescription<T>(string code) where T : DataContractBase;
        List<T> GetCodeTable<T>() where T : DataContractBase;
        List<T> QueryCodeTable<T>(string query) where T : DataContractBase;
    }
}