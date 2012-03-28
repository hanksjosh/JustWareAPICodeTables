using System.Collections.Generic;
using JustWareApiCodeTables.JustWareApi;

namespace JustWareApiCodeTables
{
    public interface ICodeLookup
    {
        string GetCodeDescription<T>(string code);
        List<T> GetCodeTable<T>();
        List<T> QueryCodeTable<T>(string query);
    }
}