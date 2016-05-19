// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;
using JsonSchema = Microsoft.Json.Schema.JsonSchema;

namespace Microsoft.Json.Schema.ValidationSuiteTests
{
    public class ValidationSuiteTests
    {
        [Theory(DisplayName = nameof(ValidationSuite), Skip = "NYI")]
        [ClassData(typeof(ValidationData))]
        public void ValidationSuite(TestSuite testSuite)
        {
            testSuite.Description.Length.Should().BeGreaterThan(10);
        }
    }

    public class ValidationData : IEnumerable<object[]>
    {
        private const string TestSuitePath = @"G:\Code\JSON-Schema-Test-Suite\tests\draft4";

        private readonly List<object[]> _data;

        public ValidationData()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new JsonSchemaContractResolver()
            };

            _data = new List<object[]>();

            string[] testFiles = Directory.GetFiles(TestSuitePath, "*.json");
            foreach (string testFile in testFiles)
            {
                List<TestSuite> testSuites = JsonConvert.DeserializeObject<List<TestSuite>>(File.ReadAllText(testFile));
                foreach (TestSuite testSuite in testSuites)
                {
                    _data.Add(new object[] { testSuite });
                }
            }
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class TestSuite
    {
        public string Description { get; set; }
        public JsonSchema Schema { get; set; } 
        public List<TestCase> TestCases { get; set; }
    }

    public class TestCase
    {
        public string Description { get; set; }
        public string Data { get; set; }
        public bool Valid { get; set; }
    }
}
