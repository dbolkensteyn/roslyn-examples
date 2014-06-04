using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Diagnostics;

namespace Tests
{
    [TestClass]
    public class ControlFlowGraphTest
    {
        [TestMethod]
        public void SimpleAssignmentExpression()
        {
            SyntaxNode node = ParseStatement("a = 0;");

            ControlFlowGraph cfg = ControlFlowGraph.Create(node);

            Assert.AreEqual(1, cfg.BasicBlocks.Count);
            Assert.AreEqual(1, cfg.BasicBlocks[0].Statements.Count);
        }

        [TestMethod]
        public void ReturnStatement()
        {
            SyntaxNode node = ParseStatement("return foo;");

            ControlFlowGraph cfg = ControlFlowGraph.Create(node);

            Assert.AreEqual(1, cfg.BasicBlocks.Count);
            Assert.AreEqual(1, cfg.BasicBlocks[0].Statements.Count);
        }

        [TestMethod]
        public void EqualityExpression()
        {
            SyntaxNode node = ParseStatement("{ if (a == 0); }");

            ControlFlowGraph cfg = ControlFlowGraph.Create(node);

            Assert.AreEqual(SyntaxKind.EqualsExpression, cfg.BasicBlocks[0].Statements[0].CSharpKind());
        }

        [TestMethod]
        public void NotEqualsExpression()
        {
            SyntaxNode node = ParseStatement("{ if (a != 0); }");

            ControlFlowGraph cfg = ControlFlowGraph.Create(node);

            Assert.AreEqual(SyntaxKind.NotEqualsExpression, cfg.BasicBlocks[0].Statements[0].CSharpKind());
        }

        [TestMethod]
        public void Block()
        {
            SyntaxNode node = ParseStatement("{ a = 0; a = 0; }");

            ControlFlowGraph cfg = ControlFlowGraph.Create(node);

            Assert.AreEqual(1, cfg.BasicBlocks.Count);
            Assert.AreEqual(2, cfg.BasicBlocks[0].Statements.Count);
        }

        [TestMethod]
        public void IfStatement()
        {
            SyntaxNode node = ParseStatement("{ if (false) { a = 0; b = 0; } c = 0; }");

            ControlFlowGraph cfg = ControlFlowGraph.Create(node);

            Assert.AreEqual(3, cfg.BasicBlocks.Count);

            Assert.AreEqual(node.DescendantNodes().OfType<IfStatementSyntax>().Single(), cfg.BasicBlocks[0].Terminator);
            Assert.AreEqual(0, cfg.BasicBlocks[0].Statements.Count);
            Assert.AreEqual(2, cfg.BasicBlocks[0].Successors.Count);
            Assert.AreEqual(cfg.BasicBlocks[1], cfg.BasicBlocks[0].Successors[0]);
            Assert.AreEqual(cfg.BasicBlocks[2], cfg.BasicBlocks[0].Successors[1]);

            Assert.AreEqual(2, cfg.BasicBlocks[1].Statements.Count);
            Assert.AreEqual(1, cfg.BasicBlocks[2].Statements.Count);
        }

        private SyntaxNode ParseStatement(String statement)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText("namespace A { class A() { A() { " + statement + " } } }");

            return tree.GetCompilationUnitRoot()
                .Members.OfType<NamespaceDeclarationSyntax>().Single()
                .Members.OfType<ClassDeclarationSyntax>().Single()
                .Members.OfType<ConstructorDeclarationSyntax>().Single()
                .Body
                .Statements.Single();
        }
    }
}
