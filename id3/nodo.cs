using System;
using System.Collections.Generic;
using System.Text;

namespace id3
{
    class nodo
    {
        public nodo(string name, int tableIndex, atributo nodeAttribute, string edge)
        {
            nombre = name;
            TableIndex = tableIndex;
            NodeAttribute = nodeAttribute;
            hijes = new List<nodo>();
            Edge = edge;
        }

        public nodo(bool isleaf, string name, string edge)
        {
            IsLeaf = isleaf;
            nombre = name;
            Edge = edge;
        }

        public string nombre { get; }

        public string Edge { get; }

        public atributo NodeAttribute { get; }

        public List<nodo> hijes { get; }

        public int TableIndex { get; }

        public bool IsLeaf { get; }
    }
}

