﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using JustWareApiCodeTables;
using JustWareApiCodeTables.JustWareApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JustWareApiCodeTablesTests
{
    [TestClass]
    public class CodeTableCacheTest
    {
        [TestMethod]
        public void AddToDictionaryTest()
        {
            CodeTableCache cache = new CodeTableCache();
            List<CaseStatusType> csList = new List<CaseStatusType>();
            csList.Add(new CaseStatusType() {Code = "C", Description = "Desc"});

            Assert.IsFalse(cache.CodeTableDictionary.ContainsKey(typeof(CaseStatusType)), "CaseStatusType was not null to start with");

            cache.AddToDictionary(csList);

            Assert.AreSame(csList, cache.CodeTableDictionary[typeof(CaseStatusType)], "List was not added to the dictionary");
        }

        [TestMethod]
        public void AddToDictionary_OverwritesWhatsAlreadyCached()
        {
            CodeTableCache cache = new CodeTableCache();
            List<CaseStatusType> csList = new List<CaseStatusType>();
            csList.Add(new CaseStatusType() { Code = "C", Description = "Desc" });

            Assert.IsFalse(cache.CodeTableDictionary.ContainsKey(typeof(CaseStatusType)), "CaseStatusType was not null to start with");

            cache.AddToDictionary(csList);
            Assert.AreSame(csList, cache.CodeTableDictionary[typeof(CaseStatusType)], "List was not added to the dictionary");
            List<CaseStatusType> newList = new List<CaseStatusType>();
            newList.Add(new CaseStatusType() { Code = "C", Description = "Desc" });
            newList.Add(new CaseStatusType() { Code = "C2", Description = "Desc2" });

            cache.AddToDictionary(newList);

            List<CaseStatusType> dictList = (List<CaseStatusType>)cache.CodeTableDictionary[typeof (CaseStatusType)];
            Assert.AreEqual(2, dictList.Count(), "There isn't two items.");
            Assert.IsNotNull(dictList.FirstOrDefault(c=>c.Code == "C"), "Code C was not in the list");
            Assert.IsNotNull(dictList.FirstOrDefault(c=>c.Code == "C2"), "Code C2 was not in the list");
        }

        [TestMethod]
        public void QueryCacheCodeTableTest_NotInDictionary()
        {
            CodeTableCache cache = new CodeTableCache();
            List<CaseType> ct = cache.QueryCacheCodeTable<CaseType>("1==1");
            Assert.IsNotNull(ct, "Returned list was null.");
            Assert.AreEqual(0, ct.Count(), "There should not be any rows");
        }

        [TestMethod]
        public void QueryCacheCodeTableTest()
        {
            CodeTableCache cache = new CodeTableCache();
            List<CaseStatusType> csList = new List<CaseStatusType>();
            csList.Add(new CaseStatusType() { Code = "C", Description = "Desc" });
            cache.AddToDictionary(csList);

            List<CaseStatusType> result = cache.QueryCacheCodeTable<CaseStatusType>("1==1");
            Assert.AreEqual(1, result.Count(), "Result did not have one item");
            Assert.IsNotNull(result.FirstOrDefault(ct=>ct.Code == "C"), "Result did not have the right data");
        }

        [TestMethod]
        public void QueryCacheCodeTableTest_WithQuery()
        {
            CodeTableCache cache = new CodeTableCache();
            List<CaseStatusType> csList = new List<CaseStatusType>();
            csList.Add(new CaseStatusType() { Code = "C", Description = "Desc" });
            csList.Add(new CaseStatusType() { Code = "C2", Description = "Desc2" });
            cache.AddToDictionary(csList);

            List<CaseStatusType> result = cache.QueryCacheCodeTable<CaseStatusType>("Code == \"C2\"");
            Assert.AreEqual(1, result.Count(), "Result did not have one item");
            Assert.IsNotNull(result.FirstOrDefault(ct => ct.Code == "C2"), "Result did not have the right data");
        }

    }
}
