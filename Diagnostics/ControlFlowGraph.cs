using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Diagnostics
{
    public class ControlFlowGraph
    {
        private ControlFlowGraph(IList<ControlFlowBasicBlock> basicBlocks)
        {
            this.BasicBlocks = basicBlocks;
        }

        public IList<ControlFlowBasicBlock> BasicBlocks;

        public static ControlFlowGraph Create(SyntaxNode node)
        {
            return new ControlFlowGraphBuilder().Build(node);
        }

        private class ControlFlowGraphBuilder : SyntaxWalker
        {
            private ControlFlowBasicBlock currentBasicBlock = new ControlFlowBasicBlock();
            private IList<ControlFlowBasicBlock> basicBlocks = new List<ControlFlowBasicBlock>();

            public ControlFlowGraph Build(SyntaxNode node)
            {
                Visit(node);
                return new ControlFlowGraph(basicBlocks);
            }

            public override void Visit(SyntaxNode node)
            {
                if (node.IsKind(SyntaxKind.SimpleAssignmentExpression))
                {
                    currentBasicBlock.Statements.Add(node);
                }

                base.Visit(node);
            }
        }
    }
}
