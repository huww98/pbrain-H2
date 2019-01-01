using System;
using System.Collections.Generic;
using System.Text;

namespace Huww98.FiveInARow.Engine
{
    interface ITranspositionTable<TNode>
    {
        void Add(long hash, TNode node);
        TNode GetExistsOrNewNode(long hash);
    }

    class TranspositionTable<TNode> : ITranspositionTable<TNode> where TNode : new()
    {
        readonly Dictionary<long, TNode> table = new Dictionary<long, TNode>();

        public TNode GetExistsOrNewNode(long hash)
        {
            if (!table.TryGetValue(hash, out var newNode))
            {
                newNode = new TNode();
                table.Add(hash, newNode);
            }
            return newNode;
        }

        public void Add(long hash, TNode node)
        {
            if (!table.ContainsKey(hash))
            {
                table.Add(hash, node);
            }
        }
    }

    class NullTranspositionTable<TNode> : ITranspositionTable<TNode> where TNode : new()
    {
        public void Add(long hash, TNode node)
        {
        }

        public TNode GetExistsOrNewNode(long hash)
        {
            return new TNode();
        }
    }
}
