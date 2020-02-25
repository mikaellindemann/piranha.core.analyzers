/*
 * Copyright (c) 2020 Mikael Lindemann
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core.analyzers
 *
 */
 
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Piranha.Analyzers.Test
{
    public class DateFieldRegionAnalyzersUnitTests : CodeFixVerifier
    {
        [Fact]
        public async Task NoDiagnosticIfNoCode()
        {
            var test = @"";

            await VerifyCSharpDiagnosticAsync(test);
        }

        [Fact]
        public async Task DiagnosticRegionAppliedToDateFieldProperty()
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
                "        public DateField Date { get; set; }",
                "    }",
                "}");
            var expected = new DiagnosticResult
            {
                Id = "PA0001",
                Message = "DateField should not be used as a single field region",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 9)
                        }
            };

            await VerifyCSharpDiagnosticAsync(test, expected);
        }

        [Fact]
        public async Task NoDiagnosticDateFieldInComplexRegion()
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
                "        public ContentRegion Content { get; set; }",
                "",
                "        public class ContentRegion",
                "        {",
                "            [Field]",
                "            public DateField Date { get; set; }",
                "        }",
                "    }",
                "}");

            await VerifyCSharpDiagnosticAsync(test);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return null;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NonSingleFieldRegionAnalyzer();
        }
    }
}
