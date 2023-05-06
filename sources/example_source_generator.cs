using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Text;

public class HelloWorldGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is SyntaxReceiver receiver)
        {
            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                var className = classDeclaration.Identifier.Text;
                var namespaceName = (classDeclaration.Parent as NamespaceDeclarationSyntax)?.Name.ToString() ?? "Global";

                var sourceBuilder = new StringBuilder($@"
namespace {namespaceName}
{{
    partial class {className}
    {{
        public void HelloWorld()
        {{
            Console.WriteLine(""Hello, World!"");
        }}
    }}
}}
");
                context.AddSource($"{className}_generated", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }
        }
    }

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration && classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                CandidateClasses.Add(classDeclaration);
            }
        }
    }
}