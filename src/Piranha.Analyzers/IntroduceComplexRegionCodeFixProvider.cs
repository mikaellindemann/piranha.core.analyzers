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
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Piranha.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IntroduceComplexRegionCodeFixProvider)), Shared]
    public class IntroduceComplexRegionCodeFixProvider : ComplexRegionCodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NonSingleFieldRegionAnalyzer.DiagnosticId);
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the faulty property and its containing class
            var faultyProperty = root.FindNode(context.Span).FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            var contentClass = faultyProperty.FirstAncestorOrSelf<ClassDeclarationSyntax>();

            var complexRegionName = ComplexRegionName(contentClass);

            if (complexRegionName == null)
            {
                // Could not find a complex region name.
                return;
            }

            context.RegisterCodeFix(CodeAction.Create(
                "Replace with complex region",
                ct =>
                {
                    // Create complex region with the faulty region as a field.
                    var complexRegionClass = CreateComplexRegionClass(
                        complexRegionName,
                        CreateAutoPropertyWithAttributes(faultyProperty.Type, faultyProperty.Identifier.ValueText, attributeNames: Constants.Types.PiranhaExtendFieldAttribute).WithLeadingTrivia(faultyProperty.GetLeadingTrivia()).WithTrailingTrivia(faultyProperty.GetTrailingTrivia())
                    );

                    // Create a new region property of the new complex region type.
                    var regionProperty = CreateAutoPropertyWithAttributes(SyntaxFactory.ParseTypeName(complexRegionClass.Identifier.ValueText), "MyRegion", attributeNames: Constants.Types.PiranhaExtendRegionAttribute)
                        .WithTrailingTrivia(faultyProperty.GetTrailingTrivia());

                    // Replace the faulty region property with the proper one, add the region class as inner class.
                    var newContentClass = contentClass
                        .WithMembers(contentClass.Members.Replace(faultyProperty, regionProperty))
                        .AddMembers(complexRegionClass);

                    return Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(contentClass, newContentClass)));
                },
                "Replace with complex region"), context.Diagnostics);
        }

        private static string ComplexRegionName(ClassDeclarationSyntax @class)
        {
            string Name(int number)
            {
                if (number <= 0)
                {
                    return "ComplexRegion";
                }

                return $"ComplexRegion{number}";
            }

            for (var number = 0; number < 10; number++)
            {
                var name = Name(number);
                if (!@class.Members.Any(m => (m.GetType().GetProperty("Identifier", BindingFlags.Public | BindingFlags.Instance)?.GetValue(m) as SyntaxToken?)?.ValueText == name))
                {
                    return name;
                }
            }

            // Give up on finding a class name.
            return null;
        }
    }
}
