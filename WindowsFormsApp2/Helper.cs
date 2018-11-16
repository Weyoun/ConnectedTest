namespace WindowsFormsApp2
{
    internal class Knoten
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
    }
}
