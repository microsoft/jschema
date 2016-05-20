// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Json.Schema.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Json.Schema.ValidationSuiteTests
{
    public class ValidationSuiteTests
    {
        [Theory(DisplayName = nameof(ValidationSuite), Skip = "NYI")]
        [ClassData(typeof(ValidationData))]
        public void ValidationSuite(TestData testData)
        {
            testData.ErrorMessage.Should().BeNull();

            var validator = new Validator(testData.Schema);

            string[] errorMessages = validator.Validate(testData.InstanceText);
            errorMessages.Should().BeEmpty();
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
                try
                {
                    List<TestSuite> testSuites = JsonConvert.DeserializeObject<List<TestSuite>>(File.ReadAllText(testFile));
                    foreach (TestSuite testSuite in testSuites)
                    {
                        foreach (TestCase testCase in testSuite.Tests)
                        {
                            string description = $"{testSuite.Description}: {testCase.Description}";
                            _data.Add(new object[]
                            {
                                new TestData
                                {
                                    Description = description,
                                    Schema = testSuite.Schema,
                                    InstanceText = GetInstanceText(testCase.Data),
                                    Valid = testCase.Valid
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _data.Add(new object[]
                    {
                        new TestData
                        {
                            ErrorMessage = $"Error reading {testFile}: {ex.Message}"
                        }
                    });
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

        private string GetInstanceText(JToken data)
        {
            string instanceText = data.ToString();

            switch (data.Type)
            {
                case JTokenType.String:
                    instanceText = '"' + instanceText + '"';
                    break;

                case JTokenType.Boolean:
                    instanceText = instanceText.ToLowerInvariant();
                    break;

                default:
                    break;
            }

            return instanceText;
        }
    }

    public class TestSuite
    {
        public string Description { get; set; }
        public JsonSchema Schema { get; set; } 
        public List<TestCase> Tests { get; set; }
    }

    public class TestCase
    {
        public string Description { get; set; }
        public JToken Data { get; set; }
        public bool Valid { get; set; }
    }

    public class TestData
    {
        public string Description { get; set; }
        public JsonSchema Schema { get; set; }
        public string InstanceText { get; set; }
        public bool Valid { get; set; }
        public string ErrorMessage { get; set; }
    }
}
