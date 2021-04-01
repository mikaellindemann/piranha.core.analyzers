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
    public class SiteTypeAttributeOnlyForClassesInheritingSiteContentAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PA0005";
        private const string Category = "Usage";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.SiteTypeAttributeOnlyForClassesInheritingSiteAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.SiteTypeAttributeOnlyForClassesInheritingSiteAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.SiteTypeAttributeOnlyForClassesInheritingSiteAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.Attribute);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not AttributeSyntax attribute)
            {
                return;
            }

            var regionAttributeType = context.Compilation.GetTypeByMetadataName(Constants.Types.Piranha.AttributeBuilder.SiteTypeAttribute);

            if (regionAttributeType == null)
            {
                return;
            }

            var attributeType = context.SemanticModel.GetTypeInfo(attribute, context.CancellationToken);

            if (!regionAttributeType.Equals(attributeType.ConvertedType, SymbolEqualityComparer.IncludeNullability))
            {
                return;
            }

            if (attribute.Parent is not AttributeListSyntax attributeList)
            {
                return;
            }

            if (attributeList.Parent is not ClassDeclarationSyntax @class)
            {
                return;
            }

            var siteType = context.Compilation.GetTypeByMetadataName($"{Constants.Types.Piranha.Models.SiteContent}`1");

            if (siteType == null)
            {
                return;
            }

            var unboundSiteType = siteType.ConstructUnboundGenericType();

            if (@class.BaseList != null && @class.BaseList.Types.Count != 0)
            {
                foreach (var type in @class.BaseList.Types)
                {
                    if (context.SemanticModel.GetTypeInfo(type.Type, context.CancellationToken).ConvertedType is not INamedTypeSymbol convertedType)
                    {
                        continue;
                    }

                    if (InheritsSite(convertedType, siteType, unboundSiteType))
                    {
                        return;
                    }
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, @class.GetLocation(), @class.Identifier.ValueText));
        }

        private static bool InheritsSite(INamedTypeSymbol type, INamedTypeSymbol siteType, INamedTypeSymbol unboundSiteType)
        {
            if (type == null)
            {
                return false;
            }

            if (type.SpecialType == SpecialType.System_Object)
            {
                return false;
            }

            if (!type.IsGenericType || !unboundSiteType.Equals(type.ConstructUnboundGenericType(), SymbolEqualityComparer.IncludeNullability))
            {
                var baseType = type.BaseType;
                if (baseType != null && InheritsSite(baseType, siteType, unboundSiteType))
                {
                    return true;
                }
            }

            if (type.TypeArguments.Length != 1)
            {
                return false;
            }

            var constructedType = siteType.Construct(type.TypeArguments.First());

            if (constructedType.Equals(type, SymbolEqualityComparer.IncludeNullability))
            {
                return true;
            }

            return false;
        }
    }
}
