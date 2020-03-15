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
    public class AudioFieldRegionAnalyzersUnitTests : CodeFixVerifier
    {
        [Fact]
        public async Task NoDiagnosticIfNoCodeAsync()
        {
            var test = @"";

            await VerifyCSharpDiagnosticAsync(test);
        }

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
                "        public AudioField Audio { get; set; }",
                "    }",
                "}");
            var expected = new DiagnosticResult
            {
                Id = "PA0001",
                Message = "AudioField should not be used as a single field region",
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 9)
                        }
            };

            await VerifyCSharpDiagnosticAsync(test, expected);

            var expectedFix = string.Join(Environment.NewLine,
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    class TypeName : Post<TypeName>",
                "    {",
                "        [Region]",
                "        public ComplexRegion MyRegion { get; set; }",
                "",
                "        public class ComplexRegion",
                "        {",
                "            [Field]",
                "            public AudioField Audio { get; set; }",
                "        }",
                "    }",
                "}");

            await VerifyCSharpFixAsync(test, expectedFix);
        }


        [Fact]
        public async Task DiagnosticRegionAppliedToAudioFieldPropertyWithExistingComplexRegion()
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
                "        public AudioField Audio { get; set; }",
                "",
                "        [Region]",
                "        public ComplexRegion Region { get; set; }",
                "",
                "        public class ComplexRegion",
                "        {",
                "        }",
                "    }",
                "}");
            var expected = new DiagnosticResult
            {
                Id = "PA0001",
                Message = "AudioField should not be used as a single field region",
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 9)
                        }
            };

            await VerifyCSharpDiagnosticAsync(test, expected);

            var expectedFix = string.Join(Environment.NewLine,
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    class TypeName : Post<TypeName>",
                "    {",
                "        [Region]",
                "        public ComplexRegion1 MyRegion { get; set; }",
                "",
                "        [Region]",
                "        public ComplexRegion Region { get; set; }",
                "",
                "        public class ComplexRegion",
                "        {",
                "        }",
                "",
                "        public class ComplexRegion1",
                "        {",
                "            [Field]",
                "            public AudioField Audio { get; set; }",
                "        }",
                "    }",
                "}");

            await VerifyCSharpFixAsync(test, expectedFix);
        }

        [Fact]
        public async Task NoDiagnosticAudioFieldInComplexRegionAsync()
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
                "            public AudioField Audio { get; set; }",
                "        }",
                "    }",
                "}");

            await VerifyCSharpDiagnosticAsync(test);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new IntroduceComplexRegionCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NonSingleFieldRegionAnalyzer();
        }
    }
}
