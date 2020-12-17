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
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using TestHelper;
using Xunit;
using Microsoft.CodeAnalysis;

namespace Piranha.Analyzers.Test
{
    public class IncorrectFieldSettingsAnalyzerTests : CodeFixVerifier
    {
        [Fact]
        public async Task StringFieldSettingsAppliedToStringField()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Extend.Fields.Settings;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [SiteType]",
                "    class TypeName : SiteContent<TypeName>",
                "    {",
                "        [StringFieldSettings(Length=200)]",
                "        [Field]",
                "        public StringField Title { get; set; }",
                "    }",
                "}");

            await VerifyCSharpDiagnosticAsync(test);
        }

        [Fact]
        public async Task StringFieldSettingsAppliedToAudioField()
        {
            var test = string.Join(Environment.NewLine,
                "using Piranha.AttributeBuilder;",
                "using Piranha.Extend;",
                "using Piranha.Extend.Fields;",
                "using Piranha.Extend.Fields.Settings;",
                "using Piranha.Models;",
                "",
                "namespace ConsoleApplication1",
                "{",
                "    [SiteType]",
                "    class TypeName : SiteContent<TypeName>",
                "    {",
                "        [StringFieldSettings(MaxLength=200)]",
                "        [Field]",
                "        public AudioField BackgroundMusic { get; set; }",
                "    }",
                "}");

            var expected = new DiagnosticResult{
                Id = "PA0006",
                Message = "StringFieldSettingsAttribute does not apply to field of type AudioField",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 12, 10)
                }
            };

            await VerifyCSharpDiagnosticAsync(test, expected);
        }

        [Fact]
        public async Task ColorFieldSettingsAppliedToColorField()
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
                "        [ColorFieldSettings(DisallowInput = true)]",
                "        [Field]",
                "        public ColorField Color { get; set; }",
                "    }",
                "}");

            await VerifyCSharpDiagnosticAsync(test);
        }

        [Fact]
        public async Task ColorFieldSettingsAppliedToAudioField()
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
                "        [ColorFieldSettings(DisallowInput = true)]",
                "        [Field]",
                "        public AudioField BackgroundMusic { get; set; }",
                "    }",
                "}");

            var expected = new DiagnosticResult{
                Id = "PA0006",
                Message = "ColorFieldSettingsAttribute does not apply to field of type AudioField",
                Severity = DiagnosticSeverity.Error,
                Locations = new[]
                {
                    new DiagnosticResultLocation("Test0.cs", 11, 10)
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
            return new IncorrectFieldSettingsAnalyzer();
        }
    }
}
