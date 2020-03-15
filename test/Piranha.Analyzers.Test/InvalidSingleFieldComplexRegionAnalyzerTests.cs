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
        public async Task DiagnosticRegionAppliedToAudioFieldProperty()
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
