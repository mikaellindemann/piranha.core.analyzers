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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System.Linq;

namespace Piranha.Analyzers
{
#pragma warning disable RS1016
    public abstract class ComplexRegionCodeFixProviderBase : CodeFixProvider
#pragma warning restore RS1016
    { 
        protected static PropertyDeclarationSyntax CreateAutoPropertyWithAttributes(TypeSyntax type, string name, params string[] attributeNames)
        {
            return SyntaxFactory
                        .PropertyDeclaration(type, name)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        )
                        .AddAttributeLists(SyntaxFactory.AttributeList(new SeparatedSyntaxList<AttributeSyntax>().AddRange(attributeNames.Select(attr => SyntaxFactory.Attribute(SyntaxFactory.ParseName(attr))))))
                        .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
        }

        protected static ClassDeclarationSyntax CreateComplexRegionClass(string name, params PropertyDeclarationSyntax[] fields)
        {
            var complexRegionClass = SyntaxFactory.ClassDeclaration(name)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddMembers(fields)
                        .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

            return complexRegionClass;
        }
    }
}
