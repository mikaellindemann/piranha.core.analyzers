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
    public class PageTypeAttributeOnlyForClassesInheritingPageAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public async Task ClassWithPageTypeAttributeDirectlyInheritingPage()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [PageType]",
                "    class TypeName : Page<TypeName>",
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
        public async Task ClassWithPageTypeAttributeIndirectlyInheritingPage()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [PageType]",
                "    class TypeName : MyPage<TypeName>",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    class MyPage<T> : Page<T>",
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
        public async Task ClassWithPageTypeAttributeIndirectlyInheritingPage2()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [PageType]",
                "    class TypeName : MyPage2",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    abstract class MyPage : Page<TypeName>",
                "    {}",
                "    abstract class MyPage2 : MyPage",
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
        public async Task ClassWithPageTypeAttributeWithoutPageAsBaseClass()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [PageType]",
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
                Id = "PA0004",
                Message = "TypeName does not extend Page, but is marked with [PageType]",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 5)
                }
            };

            await VerifyCSharpDiagnosticAsync(test, expected);
        }



        [Fact]
        public async Task ClassWithPageTypeAttributeWithCustomPageAsBaseClass()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [PageType]",
                "    class TypeName : Page<TypeName>",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    class Page<T> where T : Page<T>",
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
                Id = "PA0004",
                Message = "TypeName does not extend Page, but is marked with [PageType]",
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
            return new PageTypeAttributeOnlyForClassesInheritingPageAnalyzer();
        }
    }
}
