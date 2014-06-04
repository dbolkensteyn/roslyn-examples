﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Diagnostics
{
    public class ControlFlowBasicBlock
    {
        public ControlFlowBasicBlock()
        {
            Statements = new List<SyntaxNode>();
            Successors = new List<ControlFlowGraph>();
        }

        public IList<SyntaxNode> Statements { get; private set; }

        public SyntaxNode Terminator;

        public IList<ControlFlowGraph> Successors { get; private set; }
    }
}
