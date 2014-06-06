using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Diagnostics
{
    public class ControlFlowGraph
    {
        private ControlFlowGraph(IList<ControlFlowBasicBlock> basicBlocks)
        {
            BasicBlocks = basicBlocks;
        }

        public IList<ControlFlowBasicBlock> BasicBlocks { get; private set; }

        public String ToGraph()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("digraph finite_state_machine {");
            sb.AppendLine("  rankdir=TB;");

            foreach (ControlFlowBasicBlock basicBlock in BasicBlocks)
            {
                sb.AppendLine("  node [shape = circle, label = \"" + Statements(basicBlock.Statements) + "\"]; B" + BasicBlocks.IndexOf(basicBlock) + ";");
            }

            foreach (ControlFlowBasicBlock predecessor in BasicBlocks)
            {
                bool isFirst = true;
                foreach (ControlFlowBasicBlock successor in predecessor.Successors)
                {
                    String cause;
                    if (predecessor.Terminator != null)
                    {
                        cause = Statement(predecessor.Terminator);
                        cause += Environment.NewLine;
                        cause += " " + isFirst;
                    }
                    else
                    {
                        cause = "";
                    }

                    sb.AppendLine(" B" + BasicBlocks.IndexOf(predecessor) + " -> B" + BasicBlocks.IndexOf(successor) + " [label = \"" + cause + "\"]");

                    isFirst = false;
                }
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static String Statements(IList<SyntaxNode> statements)
        {
            StringBuilder sb = new StringBuilder();

            foreach (SyntaxNode statement in statements)
            {
                sb.AppendLine(Statement(statement));
            }

            return sb.ToString();
        }

        private static String Statement(SyntaxNode statement)
        {
            const int max = 15;

            String text = statement.GetText().ToString().Replace("\"", "\\\"");
            String result = text.Length > max ? text.Substring(0, max - 3) + "..." : text;
            return " " + result + " ";
        }

        public static ControlFlowGraph Create(SyntaxNode node)
        {
            return new ControlFlowGraphBuilder().Build(node);
        }

        private class ControlFlowGraphBuilder : SyntaxWalker
        {
            private ImmutableHashSet<SyntaxKind> supportedKinds = new SyntaxKind[]
            {
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.ReturnStatement,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression
            }.ToImmutableHashSet();

            private ControlFlowBasicBlock currentBasicBlock = new ControlFlowBasicBlock();
            private IList<ControlFlowBasicBlock> basicBlocks = new List<ControlFlowBasicBlock>();

            public ControlFlowGraph Build(SyntaxNode node)
            {
                Visit(node);

                basicBlocks.Add(currentBasicBlock);

                return new ControlFlowGraph(basicBlocks);
            }

            public override void Visit(SyntaxNode node)
            {
                // FIXME How comes a node can be null here?
                if (node != null)
                {
                    if (supportedKinds.Contains(node.CSharpKind()))
                    {
                        currentBasicBlock.Statements.Add(node);
                    }
                    else if (node.IsKind(SyntaxKind.IfStatement))
                    {
                        IfStatementSyntax ifNode = (IfStatementSyntax)node;

                        ControlFlowBasicBlock conditionBasicBlock = currentBasicBlock;
                        ControlFlowBasicBlock ifTrueBasicBlock = new ControlFlowBasicBlock();
                        ControlFlowBasicBlock afterIfBasicBlock = new ControlFlowBasicBlock();

                        conditionBasicBlock.Terminator = ifNode;
                        Visit(ifNode.Condition);

                        ifTrueBasicBlock.Successors.Add(afterIfBasicBlock);
                        SetCurrentBasicBlock(ifTrueBasicBlock);
                        Visit(ifNode.Statement);

                        SetCurrentBasicBlock(afterIfBasicBlock);

                        conditionBasicBlock.Successors.Add(ifTrueBasicBlock);
                        conditionBasicBlock.Successors.Add(afterIfBasicBlock);
                    }
                    else
                    {
                        base.Visit(node);
                    }
                }
            }

            private void SetCurrentBasicBlock(ControlFlowBasicBlock newCurrentBasicBlock)
            {
                basicBlocks.Add(currentBasicBlock);
                currentBasicBlock = newCurrentBasicBlock;
            }
        }
    }
}
