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
    public class SiteTypeAttributeOnlyForClassesInheritingSiteContentAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public async Task ClassWithSiteTypeAttributeDirectlyInheritingSiteContent()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [SiteType]",
                "    class TypeName : SiteContent<TypeName>",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    class HeroRegion",
                "    {",
                "        [Field]",
                "        public AudioField Audio { get; set; }",
                "        [Field]",
                "        public AudioField Audio2 { get; set; }",
                "    }",
                "}");

            await VerifyCSharpDiagnosticAsync(test);
        }

        [Fact]
        public async Task ClassWithSiteTypeAttributeIndirectlyInheritingSiteContent()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [SiteType]",
                "    class TypeName : MySiteContent<TypeName>",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    class MySiteContent<T> : SiteContent<T>",
                "    {}",
                "    class HeroRegion",
                "    {",
                "        [Field]",
                "        public AudioField Audio { get; set; }",
                "        [Field]",
                "        public AudioField Audio2 { get; set; }",
                "    }",
                "}");

            await VerifyCSharpDiagnosticAsync(test);
        }

        [Fact]
        public async Task ClassWithSiteTypeAttributeIndirectlyInheritingSiteContent2()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [SiteType]",
                "    class TypeName : MySiteContent2",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    abstract class MySiteContent : SiteContent<TypeName>",
                "    {}",
                "    abstract class MySiteContent2 : MySiteContent",
                "    {}",
                "    class HeroRegion",
                "    {",
                "        [Field]",
                "        public AudioField Audio { get; set; }",
                "        [Field]",
                "        public AudioField Audio2 { get; set; }",
                "    }",
                "}");

            await VerifyCSharpDiagnosticAsync(test);
        }

        [Fact]
        public async Task ClassWithSiteTypeAttributeWithoutSiteContentAsBaseClass()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [SiteType]",
                "    class TypeName",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    class HeroRegion",
                "    {",
                "        [Field]",
                "        public AudioField Audio { get; set; }",
                "        [Field]",
                "        public AudioField Audio2 { get; set; }",
                "    }",
                "}");
            var expected = new DiagnosticResult
            {
                Id = "PA0005",
                Message = "TypeName does not extend SiteContent, but is marked with [SiteType]",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 5)
                }
            };

            await VerifyCSharpDiagnosticAsync(test, expected);
        }



        [Fact]
        public async Task ClassWithSiteTypeAttributeWithCustomSiteContentAsBaseClass()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [SiteType]",
                "    class TypeName : SiteContent<TypeName>",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    class SiteContent<T> where T : SiteContent<T>",
                "    {}",
                "    class HeroRegion",
                "    {",
                "        [Field]",
                "        public AudioField Audio { get; set; }",
                "        [Field]",
                "        public AudioField Audio2 { get; set; }",
                "    }",
                "}");
            var expected = new DiagnosticResult
            {
                Id = "PA0005",
                Message = "TypeName does not extend SiteContent, but is marked with [SiteType]",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 5)
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
            return new SiteTypeAttributeOnlyForClassesInheritingSiteContentAnalyzer();
        }
    }
}
