/*
 * Copyright (c) 2020 Mikael Lindemann
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core.analyzers
 *
 */

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Piranha.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IncorrectFieldSettingsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PA0006";
        private const string Category = "Usage";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.IncorrectFieldSettingsAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.IncorrectFieldSettingsAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.IncorrectFieldSettingsAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
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

            var attributeType = context.SemanticModel.GetTypeInfo(attribute, context.CancellationToken).Type;
            var fieldSettingsAttributeType = context.Compilation.GetTypeByMetadataName(Constants.Types.Piranha.Extend.FieldSettingsAttribute);

            // Verify that it is a settings attribute.
            var inheritedType = attributeType.BaseType;
            while (inheritedType != null && !fieldSettingsAttributeType.Equals(inheritedType, SymbolEqualityComparer.IncludeNullability))
            {
                inheritedType = inheritedType.BaseType;
            }

            if (inheritedType == null)
            {
                return;
            }

            if (attribute.Parent is not AttributeListSyntax attributeList)
            {
                return;
            }

            if (attributeList.Parent is not PropertyDeclarationSyntax property)
            {
                return;
            }

            var propertyType = context.SemanticModel.GetTypeInfo(property.Type).Type;

            // Check that StringFieldSettingsAttribute is only applied to StringField.
            if (attributeType.MetadataName.StartsWith(propertyType.MetadataName))
            {
                return;
            }
            
            context.ReportDiagnostic(Diagnostic.Create(Rule, attribute.GetLocation(), attributeType.Name, propertyType.Name));
        }
    }
}