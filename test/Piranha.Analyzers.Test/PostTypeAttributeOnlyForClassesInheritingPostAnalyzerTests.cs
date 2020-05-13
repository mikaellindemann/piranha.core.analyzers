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
    public class PostTypeAttributeOnlyForClassesInheritingPostAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public async Task ClassWithPostTypeAttributeDirectlyInheritingPost()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [PostType]",
                "    class TypeName : Post<TypeName>",
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
        public async Task ClassWithPostTypeAttributeIndirectlyInheritingPost()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [PostType]",
                "    class TypeName : MyPost<TypeName>",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    class MyPost<T> : Post<T>",
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
        public async Task ClassWithPostTypeAttributeIndirectlyInheritingPost2()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [PostType]",
                "    class TypeName : MyPost2",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    abstract class MyPost : Post<TypeName>",
                "    {}",
                "    abstract class MyPost2 : MyPost",
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
        public async Task ClassWithPostTypeAttributeWithoutPostAsBaseClass()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [PostType]",
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
                Id = "PA0003",
                Message = "TypeName does not extend Post, but is marked with [PostType]",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 8, 5)
                }
            };

            await VerifyCSharpDiagnosticAsync(test, expected);
        }



        [Fact]
        public async Task ClassWithPostTypeAttributeWithCustomPostAsBaseClass()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [PostType]",
                "    class TypeName : Post<TypeName>",
                "    {",
                "        [Region]",
                "        public HeroRegion Hero { get; set; }",
                "    }",
                "    class Post<T> where T : Post<T>",
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
                Id = "PA0003",
                Message = "TypeName does not extend Post, but is marked with [PostType]",
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
            return new PostTypeAttributeOnlyForClassesInheritingPostAnalyzer();
        }
    }
}
