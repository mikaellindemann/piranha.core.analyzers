/*
 * Copyright (c) 2020 Mikael Lindemann
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core.analyzers
 *
 */

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Piranha.Analyzers.Test
{
    public class InvalidSingleFieldComplexRegionAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public async Task InvalidSingleFieldComplexRegionInSameSource()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    class TypeName : Post<TypeName>",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    class HeroRegion",
                "    {",
                "        [Field]",
                "        public AudioField Audio { get; set; }",
                "    }",
                "}");
            var expected = new DiagnosticResult
            {
                Id = "PA0002",
                Message = "Piranha does not have support for single field complex regions. Use a single field region instead.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 9, 9)
                }
            };

            await VerifyCSharpDiagnosticAsync(test, expected);
        }

        [Fact]
        public async Task InvalidSingleFieldComplexRegionInMultipleSources()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.Extend;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    class TypeName : Post<TypeName>",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "}");

            var test2 = string.Join(Environment.NewLine,
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    class HeroRegion",
                "    {",
                "        [Field]",
                "        public AudioField Audio { get; set; }",
                "    }",
                "}");

            var expected = new DiagnosticResult
            {
                Id = "PA0002",
                Message = "Piranha does not have support for single field complex regions. Use a single field region instead.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 9)
                }
            };

            await VerifyCSharpDiagnosticAsync(new[] { test, test2 }, expected);
        }


        /// <summary>
        /// Tests that a complex region type with multiple fields in different partial classes are not recognized as a complex single field region.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task NoDiagnosticsComplexRegionInMultipleFiles()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.Extend;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    class TypeName : Post<TypeName>",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "}");

            var test2 = string.Join(Environment.NewLine,
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    partial class HeroRegion",
                "    {",
                "        [Field]",
                "        public AudioField Audio { get; set; }",
                "    }",
                "}");

            var test3 = string.Join(Environment.NewLine,
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    partial class HeroRegion",
                "    {",
                "        [Field]",
                "        public CheckBoxField CheckBox { get; set; }",
                "    }",
                "}");

            await VerifyCSharpDiagnosticAsync(new[] { test, test2, test3 });
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return null;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new InvalidSingleFieldComplexRegionAnalyzer();
        }
    }
}
