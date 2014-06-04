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

        public static ControlFlowGraph Create(SyntaxNode node)
        {
            return new ControlFlowGraphBuilder().Build(node);
        }

        private class ControlFlowGraphBuilder : SyntaxWalker
        {
            private ImmutableHashSet<SyntaxKind> supportedKinds = new SyntaxKind[]
            {
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.ReturnStatement
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
                if (supportedKinds.Contains(node.CSharpKind()))
                {
                    currentBasicBlock.Statements.Add(node);
                }
                else if (node.IsKind(SyntaxKind.IfStatement))
                {
                    IfStatementSyntax ifNode = (IfStatementSyntax)node;

                    ControlFlowBasicBlock conditionBasicBlock = currentBasicBlock;
                    base.Visit(ifNode.Condition);

                    ControlFlowBasicBlock ifTrueBasicBlock = new ControlFlowBasicBlock();
                    SetCurrentBasicBlock(ifTrueBasicBlock);
                    base.Visit(ifNode.Statement);

                    ControlFlowBasicBlock afterIfBasicBlock = new ControlFlowBasicBlock();
                    SetCurrentBasicBlock(afterIfBasicBlock);
                }
                else
                {
                    base.Visit(node);
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
