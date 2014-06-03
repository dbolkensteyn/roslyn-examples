using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Diagnostics
{
    public class ControlFlowBasicBlock
    {
        public IList<SyntaxNode> Statements;

        public SyntaxNode Terminator;

        public IList<ControlFlowGraph> Successors;
    }
}
