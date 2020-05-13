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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Piranha.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PostTypeAttributeOnlyForClassesInheritingPostAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PA0003";
        private const string Category = "Usage";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.PostTypeAttributeOnlyForClassesInheritingPostAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.PostTypeAttributeOnlyForClassesInheritingPostAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.PostTypeAttributeOnlyForClassesInheritingPostAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.Attribute);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is AttributeSyntax attribute))
            {
                return;
            }

            var regionAttributeType = context.Compilation.GetTypeByMetadataName(Constants.Types.PiranhaAttributeBuilderPostTypeAttribute);

            if (regionAttributeType == null)
            {
                return;
            }

            var attributeType = context.SemanticModel.GetTypeInfo(attribute, context.CancellationToken);

            if (!regionAttributeType.Equals(attributeType.ConvertedType, SymbolEqualityComparer.IncludeNullability))
            {
                return;
            }

            if (!(attribute.Parent is AttributeListSyntax attributeList))
            {
                return;
            }

            if (!(attributeList.Parent is ClassDeclarationSyntax @class))
            {
                return;
            }

            var postType = context.Compilation.GetTypeByMetadataName($"{Constants.Types.PiranhaModelsPost}`1");

            if (postType == null)
            {
                return;
            }

            var unboundPostType = postType.ConstructUnboundGenericType();

            if (@class.BaseList != null && @class.BaseList.Types.Count != 0)
            {
                foreach (var type in @class.BaseList.Types)
                {
                    if (!(context.SemanticModel.GetTypeInfo(type.Type, context.CancellationToken).ConvertedType is INamedTypeSymbol convertedType))
                    {
                        continue;
                    }

                    if (InheritsPost(convertedType, postType, unboundPostType))
                    {
                        return;
                    }
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, @class.GetLocation(), @class.Identifier.ValueText));
        }

        private static bool InheritsPost(INamedTypeSymbol type, INamedTypeSymbol postType, INamedTypeSymbol unboundPostType)
        {
            if (type == null)
            {
                return false;
            }

            if (type.SpecialType == SpecialType.System_Object)
            {
                return false;
            }

            if (!type.IsGenericType || !unboundPostType.Equals(type.ConstructUnboundGenericType(), SymbolEqualityComparer.IncludeNullability))
            {
                var baseType = type.BaseType;
                if (baseType != null && InheritsPost(baseType, postType, unboundPostType))
                {
                    return true;
                }
            }

            if (type.TypeArguments.Length != 1)
            {
                return false;
            }

            var constructedType = postType.Construct(type.TypeArguments.First());

            if (constructedType.Equals(type, SymbolEqualityComparer.IncludeNullability))
            {
                return true;
            }

            return false;
        }
    }
}
