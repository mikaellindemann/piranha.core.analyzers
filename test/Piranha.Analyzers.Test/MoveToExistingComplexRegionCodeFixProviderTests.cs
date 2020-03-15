using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Piranha.Analyzers.Test
{
    public class MoveToExistingComplexRegionCodeFixProviderTests : CodeFixVerifier
    {
        [Fact]
        public async Task DiagnosticRegionAppliedToAudioFieldPropertyWithExistingComplexRegionAsync()
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
                "        public Region Region { get; set; }",
                "",
                "        [Region]",
                "        public AudioField Audio { get; set; }",
                "    }",
                "}");

            var test2 = string.Join(Environment.NewLine,
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    public class Region",
                "    {",
                "        [Field]",
                "        public StringField Title { get; set; }",
                "    }",
                "}");
            var expected = new DiagnosticResult
            {
                Id = "PA0001",
                Message = "AudioField should not be used as a single field region",
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 9)
                        }
            };

            await VerifyCSharpDiagnosticAsync(new[] { test, test2 }, expected);

            var expectedFix = string.Join(Environment.NewLine,
                "using Piranha.Extend;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    class TypeName : Post<TypeName>",
                "    {",
                "        [Region]",
                "        public Region Region { get; set; }",
                "    }",
                "}");

            var expectedFix2 = string.Join(Environment.NewLine,
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    public class Region",
                "    {",
                "        [Field]",
                "        public StringField Title { get; set; }",
                "",
                "        [Field]",
                "        public AudioField Audio { get; set; }",
                "    }",
                "}");

            await VerifyCSharpFixAsync(new[] { test, test2 }, new[] { expectedFix, expectedFix2 });
        }

        [Fact]
        public async Task DiagnosticRegionAppliedToAudioFieldPropertyWithExistingComplexRegion1Async()
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
                "        public Region Region { get; set; }",
                "",
                "        [Region]",
                "        public AudioField Audio { get; set; }",
                "    }",
                "",
                "    public class Region",
                "    {",
                "        [Field]",
                "        public StringField Title { get; set; }",
                "    }",
                "}");
            var expected = new DiagnosticResult
            {
                Id = "PA0001",
                Message = "AudioField should not be used as a single field region",
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 12, 9)
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
                "        public Region Region { get; set; }",
                "    }",
                "",
                "    public class Region",
                "    {",
                "        [Field]",
                "        public StringField Title { get; set; }",
                "",
                "        [Field]",
                "        public AudioField Audio { get; set; }",
                "    }",
                "}");

            await VerifyCSharpFixAsync(test, expectedFix);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MoveToExistingComplexRegionCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NonSingleFieldRegionAnalyzer();
        }
    }
}
