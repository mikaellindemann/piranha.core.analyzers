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
using Microsoft.CodeAnalysis.Simplification;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Piranha.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MoveToExistingComplexRegionCodeFixProvider)), Shared]
    public class MoveToExistingComplexRegionCodeFixProvider : ComplexRegionCodeFixProviderBase
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NonSingleFieldRegionAnalyzer.DiagnosticId);
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;


        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = (CompilationUnitSyntax)await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the faulty property and its containing class
            var faultyProperty = root.FindNode(context.Span).FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            var contentClass = faultyProperty.FirstAncestorOrSelf<ClassDeclarationSyntax>();

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            var compilation = await context.Document.Project.GetCompilationAsync(context.CancellationToken);
            var regionAttribute = compilation.GetTypeByMetadataName(Constants.Types.Piranha.Extend.RegionAttribute);

            if (regionAttribute == null)
            {
                return;
            }

            var faultyPropertyType = semanticModel.GetTypeInfo(faultyProperty.Type).Type;

            var newFieldProperty = CreateAutoPropertyWithAttributes(SyntaxFactory.ParseTypeName(faultyPropertyType.Name), faultyProperty.Identifier.ValueText, attributeNames: Constants.Types.Piranha.Extend.FieldAttribute).WithLeadingTrivia(faultyProperty.GetLeadingTrivia()).WithTrailingTrivia(faultyProperty.GetTrailingTrivia());
            var contentClassWithoutFaultyProperty = contentClass.WithMembers(contentClass.Members.Remove(faultyProperty));

            var changedRoot = root.ReplaceNode(contentClass, contentClassWithoutFaultyProperty).WithAdditionalAnnotations(Simplifier.Annotation);

            var changedSolution = await RemoveUnusedUsings(context.Document.Project.Solution.WithDocumentSyntaxRoot(
                context.Document.Id,
                changedRoot
            ), context.Document.Id, context.CancellationToken).ConfigureAwait(false);

            // Refetch content class and semantic model from changed solution.
            var changedDocument = changedSolution.GetDocument(context.Document.Id);
            changedRoot = (CompilationUnitSyntax)await changedDocument.GetSyntaxRootAsync().ConfigureAwait(false);
            contentClassWithoutFaultyProperty = changedRoot.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>().Single(c => c.Identifier.ValueText == contentClassWithoutFaultyProperty.Identifier.ValueText);
            semanticModel = await changedSolution.GetDocument(context.Document.Id).GetSemanticModelAsync().ConfigureAwait(false);

            // Find other region properties on the class.
            var regionProperties = contentClassWithoutFaultyProperty.Members
                .Where(m => m != faultyProperty)
                .OfType<PropertyDeclarationSyntax>()
                .Where(m => m.AttributeLists.Any(l => l.Attributes.Any(a => regionAttribute.Equals(semanticModel.GetTypeInfo(a).ConvertedType, SymbolEqualityComparer.IncludeNullability))));

            foreach (var regionProperty in regionProperties)
            {
                var references = semanticModel.GetTypeInfo(regionProperty.Type).Type.DeclaringSyntaxReferences;

                if (references.IsEmpty)
                {
                    continue;
                }

                var reference = references.First();

                var node = await reference.GetSyntaxAsync(context.CancellationToken);

                if (!(node is ClassDeclarationSyntax regionClassDeclaration))
                {
                    continue;
                }

                context.RegisterCodeFix(CodeAction.Create(
                    $"Move {faultyProperty.Identifier.ValueText} to {regionProperty.Type}...",
                    async ct =>
                    {
                        var regionDocument = changedSolution.GetDocument(node.SyntaxTree);
                        var regionDocumentRoot = (CompilationUnitSyntax)await regionDocument.GetSyntaxRootAsync(ct);

                        var newRoot = regionDocumentRoot.ReplaceNode(node, regionClassDeclaration.AddMembers(newFieldProperty))
                            .WithAdditionalAnnotations(Simplifier.Annotation);

                        return changedSolution.WithDocumentSyntaxRoot(
                            regionDocument.Id,
                            newRoot
                                .WithUsings(new SyntaxList<UsingDirectiveSyntax>(newRoot.Usings.Union(new[] { UsingNamespace(faultyPropertyType.ContainingNamespace) }, new UsingDirectiveSyntaxEqualityComparer())))
                        );
                    },
                    $"Move {faultyProperty.Identifier.ValueText} to {regionProperty.Type}..."), context.Diagnostics);
            }
        }

        public UsingDirectiveSyntax UsingNamespace(INamespaceSymbol @namespace)
        {
            var nsString = @namespace.Name;
            var ns = @namespace.ContainingNamespace;
            while (!ns.IsGlobalNamespace)
            {
                nsString = $"{ns.Name}.{nsString}";
                ns = ns.ContainingNamespace;
            }

            return SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(nsString));
        }

        public async Task<Solution> RemoveUnusedUsings(Solution solution, DocumentId id, CancellationToken ct)
        {
            var document = solution.GetDocument(id);
            var changedProject = document.Project;
            var changedCompilation = await changedProject.GetCompilationAsync(ct).ConfigureAwait(false);
            var unusedUsings = changedCompilation.GetDiagnostics().Where(d => d.Id == "CS8019").ToArray();

            foreach (var unused in unusedUsings)
            {
                try
                {
                    var anotherRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);
                    var node = (UsingDirectiveSyntax)anotherRoot.FindNode(unused.Location.SourceSpan);

                    if (node.Name.ToFullString() != Constants.Namespaces.PiranhaExtendFields)
                    {
                        continue;
                    }

                    var nodeWithoutUsing = anotherRoot.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);

                    solution = solution.WithDocumentSyntaxRoot(id, nodeWithoutUsing);
                }
                catch
                {
                    // Could be that the diagnostics was not from this document.
                }
            }

            return solution;
        }
    }


    public class UsingDirectiveSyntaxEqualityComparer : IEqualityComparer<UsingDirectiveSyntax>
    {
        public bool Equals(UsingDirectiveSyntax x, UsingDirectiveSyntax y)
        {
            return x.Name.IsEquivalentTo(y.Name);
        }

        public int GetHashCode(UsingDirectiveSyntax obj)
        {
            return obj.Name.ToString().GetHashCode();
        }
    }
}
