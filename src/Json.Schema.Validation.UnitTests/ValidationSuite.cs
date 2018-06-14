// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Json.Schema.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Json.Schema.Validation.UnitTests
{
    public class ValidationSuiteTests
    {
        // This test runs the official JSON Schema Test Suite. Since this
        // implementation of JSON Schema is not quite complete, a few of the tests
        // in the suite fail. If you want to fill in some of the missing pieces,
        // uncomment the next two lines, and then run BuildAndTest.cmd with the
        // /run-json-schema-test-suite option to verify your changes.
        //  When you're done, if the implementation is still incomplete, comment
        // these two lines out again.
        //
        //[Theory(DisplayName = nameof(ValidationSuite))]
        //[ClassData(typeof(ValidationData))]
        public void ValidationSuite(TestData testData)
        {
            testData.ErrorMessage.Should().BeNull();

            var validator = new Validator(testData.Schema);

            Result[] results = validator.Validate(testData.InstanceText, testData.FileName);

            if (testData.Valid)
            {
                results.Should().BeEmpty($"test \"{testData.Description}\" should pass");
            }
            else
            {
                results.Should().NotBeEmpty($"test \"{testData.Description}\" should pass");
            }
        }
    }

    public class ValidationData : IEnumerable<object[]>
    {
        // This assumes that we are building, or have built at least once, from
        // the command line by using the script BuildAndTest.cmd, which clones
        // the JSON-Schema-Test-Suite repo into a location adjacent to the JSchema repo.
        private const string TestSuitePath = @"..\..\..\..\..\..\JSON-Schema-Test-Suite\tests\draft4";

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
                                    FileName = Path.GetFileName(testFile),
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
                            FileName = Path.GetFileName(testFile),
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

                case JTokenType.Null:
                    instanceText = "null";
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
        public string FileName { get; set; }
        public string Description { get; set; }
        public JsonSchema Schema { get; set; }
        public string InstanceText { get; set; }
        public bool Valid { get; set; }
        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Description)
                ? FileName
                : $"{FileName}: {Description}";
        }
    }
}
