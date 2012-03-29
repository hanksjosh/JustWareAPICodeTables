using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using JustWareApiCodeTables;
using JustWareApiCodeTables.JustWareApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JustWareApiCodeTablesTests
{
    [TestClass]
    public class CodeLookupHelperTests
    {
        [TestMethod]
        public void QueryCodeTableTest_NoCache()
        {
            Mock<IJustWareApi> api = new Mock<IJustWareApi>();
            List<CaseType> ct = new List<CaseType>();
            ct.Add(new CaseType {Code = "C", Description = "D"});
            api.Setup(a => a.FindCaseTypes(It.IsAny<string>(), null)).Returns(ct);

            CodeLookupHelper helper = new CodeLookupHelper(api.Object);

            List<CaseType> returned = helper.QueryCodeTable<CaseType>("1==1");
            Assert.AreSame(ct, returned, "CaseTypes not returned correctly.");
            api.Verify(a=>a.FindCaseTypes(It.IsAny<string>(), null), Times.Once(), "Api FindCaseTypes not called.");
        }

        [TestMethod]
        public void QueryCodeTableTest_PutsCodeTableInCache()
        {
            Mock<IJustWareApi> api = new Mock<IJustWareApi>();
            Mock<ICodeTableCache> cache = new Mock<ICodeTableCache>();
            List<CaseType> ct = new List<CaseType>();
            ct.Add(new CaseType { Code = "C", Description = "D" });
            cache.Setup(c => c.IsCodeTableCached<CaseType>()).Returns(false);
            cache.Setup(c => c.QueryCacheCodeTable<CaseType>(It.IsAny<string>())).Returns(ct);
            
            api.Setup(a => a.FindCaseTypes(It.IsAny<string>(), null)).Returns(ct);

            CodeLookupHelper helper = new CodeLookupHelper(api.Object, cache.Object);

            string QUERY = "1==1";
            List<CaseType> returned = helper.QueryCodeTable<CaseType>(QUERY);
            Assert.AreSame(ct, returned, "CaseTypes not returned correctly.");
            api.Verify(a => a.FindCaseTypes(It.IsAny<string>(), null), Times.Once(), "Api FindCaseTypes not called.");
            cache.Verify(c=>c.AddToDictionary(It.IsAny<List<CaseType>>()), Times.Once(), "AddToDictionary was not called.");
        }

        [TestMethod]
        public void QueryCodeTableTest_QueriesFromCache()
        {
            Mock<IJustWareApi> api = new Mock<IJustWareApi>();
            Mock<ICodeTableCache> cache = new Mock<ICodeTableCache>();
            List<CaseType> ct = new List<CaseType>();
            ct.Add(new CaseType { Code = "C", Description = "D" });
            cache.Setup(c => c.IsCodeTableCached<CaseType>()).Returns(true);
            cache.Setup(c => c.QueryCacheCodeTable<CaseType>(It.IsAny<string>())).Returns(ct);

            CodeLookupHelper helper = new CodeLookupHelper(api.Object, cache.Object);

            string QUERY = "1==1";
            List<CaseType> returned = helper.QueryCodeTable<CaseType>(QUERY);
            Assert.AreSame(ct, returned, "CaseTypes not returned correctly.");
            api.Verify(a => a.FindCaseTypes(It.IsAny<string>(), null), Times.Never(), "Api FindCaseTypes called when it should not have been.");
            cache.Verify(c => c.AddToDictionary(It.IsAny<List<CaseType>>()), Times.Never(), "AddToDictionary was called when it should not have been.");
            cache.Verify(c => c.QueryCacheCodeTable<CaseType>(QUERY), Times.Once(), "QueryCachedCodeTable was not called");
        }


        [TestMethod]
        public void GetCodeTableTest()
        {
            Mock<IJustWareApi> api = new Mock<IJustWareApi>();
            Mock<ICodeTableCache> cache = new Mock<ICodeTableCache>();
            List<CaseType> ct = new List<CaseType>();
            ct.Add(new CaseType {Code = "C", Description = "D"});
            api.Setup(a => a.FindCaseTypes(It.IsAny<string>(), null)).Returns(ct);
            cache.Setup(c => c.QueryCacheCodeTable<CaseType>(It.IsAny<string>())).Returns(new List<CaseType>());

            CodeLookupHelper helper = new CodeLookupHelper(api.Object, cache.Object);

            List<CaseType> returned = helper.GetCodeTable<CaseType>();
            Assert.AreSame(ct, returned, "CaseTypes not returned correctly.");
            api.Verify(a => a.FindCaseTypes(It.Is<string>(s => s == CodeLookupHelper.ALL_CODES_QUERY), null), Times.Once(), "FindCaseTypes was not called correctly");
            cache.Verify(c=>c.AddToDictionary(It.Is<List<CaseType>>(l=>l == ct)), Times.Once(), "Code Table was not cached");
        }

        [TestMethod]
        public void GetCodeTable_GetsFromCacheIfThere()
        {
            Mock<IJustWareApi> api = new Mock<IJustWareApi>();
            Mock<ICodeTableCache> cache = new Mock<ICodeTableCache>();
            List<CaseType> ct = new List<CaseType>();
            ct.Add(new CaseType { Code = "C", Description = "D" });
            cache.Setup(c => c.QueryCacheCodeTable<CaseType>(It.IsAny<string>())).Returns(ct);
            api.Setup(a => a.FindCaseTypes(It.IsAny<string>(), null)).Returns(ct);

            CodeLookupHelper helper = new CodeLookupHelper(api.Object, cache.Object);

            List<CaseType> returned = helper.GetCodeTable<CaseType>();
            Assert.AreSame(ct, returned, "CaseTypes not returned correctly.");
            api.Verify(a => a.FindCaseTypes(It.Is<string>(s => s == CodeLookupHelper.ALL_CODES_QUERY), null), Times.Never(), "FindCaseTypes was called when it should not have been");
            cache.Verify(c => c.QueryCacheCodeTable<CaseType>(It.Is<string>(s => s == CodeLookupHelper.ALL_CODES_QUERY)), Times.Once(), "Code Table was not cached");
        }


        [TestMethod]
        public void GetCodeTable_NoCache_DoesntBlowup()
        {
            Mock<IJustWareApi> api = new Mock<IJustWareApi>();
            List<CaseType> ct = new List<CaseType>();
            ct.Add(new CaseType { Code = "C", Description = "D" });
            api.Setup(a => a.FindCaseTypes(It.IsAny<string>(), null)).Returns(ct);

            CodeLookupHelper helper = new CodeLookupHelper(api.Object);

            List<CaseType> returned = helper.GetCodeTable<CaseType>();
            Assert.AreSame(ct, returned, "CaseTypes not returned correctly.");
            api.Verify(a => a.FindCaseTypes(It.Is<string>(s => s == CodeLookupHelper.ALL_CODES_QUERY), null), Times.Once(), "FindCaseTypes was not called correctly");
        }


        [TestMethod]
        public void GetCodeDescriptionTest()
        {
            Mock<IJustWareApi> api = new Mock<IJustWareApi>();
            List<CaseType> ct = new List<CaseType>();
            ct.Add(new CaseType { Code = "C", Description = "D" });
            api.Setup(a => a.FindCaseTypes(It.IsAny<string>(), null)).Returns(ct);

            CodeLookupHelper helper = new CodeLookupHelper(api.Object);

            string description = helper.GetCodeDescription<CaseType>("C");
            Assert.AreEqual("D", description, "The correct caseType was not returned");
            api.Verify(a => a.FindCaseTypes(It.Is<string>(s => s == "Code == \"C\""), null), Times.Once(), "FindCaseTypes was not called correctly");
        }


        [TestMethod]
        public void GetCodeDescription_FillsCache()
        {
            Mock<IJustWareApi> api = new Mock<IJustWareApi>();
            Mock<ICodeTableCache> cache = new Mock<ICodeTableCache>();
            List<CaseType> ct = new List<CaseType>();
            ct.Add(new CaseType { Code = "C", Description = "D" });
            ct.Add(new CaseType { Code = "C2", Description = "D2" });
            api.Setup(a => a.FindCaseTypes(It.IsAny<string>(), null)).Returns(ct);


            List<CaseType> singleItemList = new List<CaseType>();
            singleItemList.Add(new CaseType(){Code = "C", Description = "D"});

            cache.Setup(c => c.QueryCacheCodeTable<CaseType>(It.IsAny<string>())).Returns(singleItemList);
            cache.Setup(c => c.IsCodeTableCached<CaseType>()).Returns(false);
            
            CodeLookupHelper helper = new CodeLookupHelper(api.Object, cache.Object);
            string description = helper.GetCodeDescription<CaseType>("C");

            Assert.AreEqual("D", description, "The correct caseType was not returned");
            api.Verify(a => a.FindCaseTypes(It.Is<string>(s => s == CodeLookupHelper.ALL_CODES_QUERY), null), Times.Once(), "FindCaseTypes was not called correctly");
            cache.Verify(c=>c.AddToDictionary(It.Is<List<CaseType>>(l=>l.Count() == 2)), Times.Once(), "Full CodeTable was not cached.");
        }


				[TestMethod]
				public void ClearCache_CallsClearCacheOnCodeTableCache()
				{
					Mock<IJustWareApi> api = new Mock<IJustWareApi>();
					Mock<ICodeTableCache> cache = new Mock<ICodeTableCache>();
					CodeLookupHelper helper = new CodeLookupHelper(api.Object, cache.Object);

					helper.ClearCache<CaseType>();

					cache.Verify(c => c.ClearCache<CaseType>(), Times.Once(), "ClearCache was not called on the CodeTableCache");
				}

				[TestMethod]
				public void ClearCache_HandlesNoCodeTableCache()
				{
					Mock<IJustWareApi> api = new Mock<IJustWareApi>();
					Mock<ICodeTableCache> cache = new Mock<ICodeTableCache>();
					CodeLookupHelper helper = new CodeLookupHelper(api.Object);

					helper.ClearCache<CaseType>();

					//As long as we didn't crash because we have no cache, everything is happy.
				}


				[TestMethod]
				public void ClearAllCache_CallsClearAllCacheOnCodeTableCache()
				{
					Mock<IJustWareApi> api = new Mock<IJustWareApi>();
					Mock<ICodeTableCache> cache = new Mock<ICodeTableCache>();
					CodeLookupHelper helper = new CodeLookupHelper(api.Object, cache.Object);

					helper.ClearAllCache();

					cache.Verify(c => c.ClearAllCache(), Times.Once(), "ClearAllCache was not called on the CodeTableCache");
				}

				[TestMethod]
				public void ClearAllCache_HandlesNoCodeTableCache()
				{
					Mock<IJustWareApi> api = new Mock<IJustWareApi>();
					Mock<ICodeTableCache> cache = new Mock<ICodeTableCache>();
					CodeLookupHelper helper = new CodeLookupHelper(api.Object);

					helper.ClearAllCache();

					//As long as we didn't crash because we have no cache, everything is happy.
				}

    }
}
