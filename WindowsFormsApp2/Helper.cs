using System;
using System.Collections.Generic;
using System.Linq;

namespace WindowsFormsApp2
{
    internal class Knoten : IComparable<Knoten>
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Index { get; private set; }
        public Knoten Pred { get; set; }
        public int Distance { get; set; }

        private readonly int zeilen, spalten;

        public Knoten North { get; set; }
        public Knoten East { get; set; }
        public Knoten South { get; set; }
        public Knoten West { get; set; }

        public Knoten(int x, int y, int z, int s)
        {
            X = x;
            Y = y;
            zeilen = z;
            spalten = s;
            Index = x * zeilen + y;
            Pred = null;
            Distance = 0;
        }

        //public override string ToString()
        //{
        //    return @"Knoten an Stelle (${X}, ${Y}) mit Distanz ${Distance} und Vorgänger bei (${Pred.X}, ${Pred.Y})";
        //}

        public override string ToString()
        {
            return X + "," + Y;
        }

        public int CompareTo(Knoten other)
        {
            return Distance.CompareTo(other.Distance);
        }

        public List<Knoten> GetSuccessor()
        {
            var rv = new List<Knoten>();

            AddSuccessor(North, rv);
            AddSuccessor(East, rv);
            AddSuccessor(South, rv);
            AddSuccessor(West, rv);

            return rv;
        }

        private void AddSuccessor(Knoten knoten, List<Knoten> liste)
        {
            if (knoten != null && knoten.Pred == this)
                liste.Add(knoten);
        }

        public List<Knoten> GetNeighbors(List<Knoten> list)
        {
            var rv = new List<Knoten>();

            AddToNeighbors(rv, North, list);
            AddToNeighbors(rv, East, list);
            AddToNeighbors(rv, South, list);
            AddToNeighbors(rv, West, list);

            return rv;
        }

        private void AddToNeighbors(List<Knoten> add, Knoten knoten, List<Knoten> check)
        {
            if (knoten != null && knoten.Pred != this && !check.Contains(knoten))
                add.Add(knoten);
        }

        public List<Knoten> GetAllNeighbors()
        {
            var list = new List<Knoten>();

            AddToAllNeighbors(North, list);
            AddToAllNeighbors(East, list);
            AddToAllNeighbors(South, list);
            AddToAllNeighbors(West, list);

            return list;
        }

        private void AddToAllNeighbors(Knoten knoten, List<Knoten> add)
        {
            if (knoten != null && knoten.Distance >= 0)
                add.Add(knoten);
        }

        public void RemoveLinks()
        {
            if (North != null)
                North.South = null;

            if (East != null)
                East.West = null;

            if (South != null)
                South.North = null;

            if (West != null)
                West.East = null;
        }
    }

    internal class KnotenComparer : IComparer<Knoten>
    {
        public int Compare(Knoten x, Knoten y)
        {
            return x.Distance.CompareTo(y.Distance);
        }
    }

    internal static class Helper
    {
        internal static bool TryGetAt(this Knoten[] graph, int x, int y, int zeilen, out Knoten knoten)
        {
            var index = x * zeilen + y;
            knoten = null;

            if (graph[index] != null)
            {
                knoten = graph[index];
                graph[index] = null;

                return true;
            }

            return false;
        }

        internal static bool TryGetPred(this Knoten node, out Knoten pred)
        {
            pred = null;

            if (node.Pred == null)
                return false;

            pred = node.Pred;

            return true;
        }

        internal static Knoten Pop(this List<Knoten> list)
        {
            var knoten = list.First();
            list.RemoveAt(0);
            return knoten;
        }
    }
}
