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
using TestHelper;
using Xunit;

namespace Piranha.Analyzers.Test
{
    public class AudioFieldRegionAnalyzersUnitTests : CodeFixVerifier
    {
        [Fact]
        public void NoDiagnosticIfNoCode()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void DiagnosticRegionAppliedToAudioFieldProperty()
        {
            var test = @"
using Piranha.Extend;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace ConsoleApplication1
{
    class TypeName : Post<TypeName>
    {
        [Region]
        public AudioField Audio { get; set; }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "PA0001",
                Message = "AudioField should not be used as a single field region",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var expectedFix = @"
using Piranha.Extend;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace ConsoleApplication1
{
    class TypeName : Post<TypeName>
    {
        [Region]
        public ComplexRegion MyRegion { get; set; }

        public class ComplexRegion
        {
            [Field]
            public AudioField Audio { get; set; }
        }
    }
}";

            VerifyCSharpFix(test, expectedFix);
        }


        [Fact]
        public void DiagnosticRegionAppliedToAudioFieldPropertyWithExistingComplexRegion()
        {
            var test = @"
using Piranha.Extend;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace ConsoleApplication1
{
    class TypeName : Post<TypeName>
    {
        [Region]
        public AudioField Audio { get; set; }

        [Region]
        public ComplexRegion Region { get; set; }

        public class ComplexRegion
        {
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "PA0001",
                Message = "AudioField should not be used as a single field region",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var expectedFix = @"
using Piranha.Extend;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace ConsoleApplication1
{
    class TypeName : Post<TypeName>
    {
        [Region]
        public ComplexRegion1 MyRegion { get; set; }

        [Region]
        public ComplexRegion Region { get; set; }

        public class ComplexRegion
        {
        }

        public class ComplexRegion1
        {
            [Field]
            public AudioField Audio { get; set; }
        }
    }
}";

            VerifyCSharpFix(test, expectedFix);
        }

        [Fact]
        public void NoDiagnosticAudioFieldInComplexRegion()
        {
            var test = @"
    using Piranha.Extend;
    using Piranha.Extend.Fields;
    using Piranha.Models;

    namespace ConsoleApplication1
    {
        class TypeName : Post<TypeName>
        {
            [Region]
            public ContentRegion Content { get; set; }

            public class ContentRegion
            {
                [Field]
                public AudioField Audio { get; set; }
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
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
