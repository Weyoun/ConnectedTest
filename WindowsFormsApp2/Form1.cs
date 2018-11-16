using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace WindowsFormsApp2
{

    public partial class Form1 : Form
    {
        int zeilen = 5;
        int spalten = 5;
        Knoten[] Graph;

        int middleX, middleY;
        Point centreOfGraph;

        public Form1()
        {
            InitializeComponent();
            Init();

            centreOfGraph = new Point(zeilen / 2, spalten / 2);
            CalculateDistance(centreOfGraph.X, centreOfGraph.Y);

            middleX = GraphView.Size.Width / 2;
            middleY = GraphView.Size.Height / 2;
        }

        private void CalculateDistance(int x, int y)
        {
            Queue<Knoten> toCheck = new Queue<Knoten>();
            HashSet<Knoten> added = new HashSet<Knoten>();

            toCheck.Enqueue(Graph[x * spalten + y]);

            while (toCheck.Count > 0)
            {
                var current = toCheck.Dequeue();

                CheckNeighbor(current.North, current, toCheck, added);
                CheckNeighbor(current.East, current, toCheck, added);
                CheckNeighbor(current.South, current, toCheck, added);
                CheckNeighbor(current.West, current, toCheck, added);

                added.Add(current);
            }
        }

        private void CheckNeighbor(Knoten node, Knoten me, Queue<Knoten> queue, HashSet<Knoten> hashSet)
        {
            if (node != null && !queue.Contains(node) && !hashSet.Contains(node))
            {
                node.Pred = me;
                node.Distance = me.Distance + 1;
                queue.Enqueue(node);
            }
        }

        private void RemoveAt(int x, int y)
        {

            if (Graph.TryGetAt(x, y, zeilen, out Knoten knoten))
            {
                knoten.Distance = -1;

                if (knoten.North != null)
                {
                    knoten.North.South = null;

                    if (knoten.North.Pred == knoten)
                        Recalculate(knoten.North);
                }

                if (knoten.East != null)
                {
                    knoten.East.West = null;

                    if (knoten.East.Pred == knoten)
                        Recalculate(knoten.East);
                }

                if (knoten.South != null)
                {
                    knoten.South.North = null;

                    if (knoten.South.Pred == knoten)
                        Recalculate(knoten.South);
                }

                if (knoten.West != null)
                {
                    knoten.West.East = null;

                    if (knoten.West.Pred == knoten)
                        Recalculate(knoten.West);
                }
            }
        }

        private void Recalculate(Knoten knoten)
        {
            var distance = int.MaxValue;
            Knoten pred = null;
            List<Knoten> possible = new List<Knoten>();

            CheckNeighbor(knoten.North, knoten, possible, ref distance, ref pred);
            CheckNeighbor(knoten.East, knoten, possible, ref distance, ref pred);
            CheckNeighbor(knoten.South, knoten, possible, ref distance, ref pred);
            CheckNeighbor(knoten.West, knoten, possible, ref distance, ref pred);

            if (possible.Count > 0 && pred == null)
            {
                possible.Sort();
                List<Knoten> circle = new List<Knoten>();
                circle.Add(knoten);

                do
                {
                    var node = possible.First(); possible.RemoveAt(0);


                    if (RecalculateRecursive(node, circle))
                    {
                        knoten.Pred = node;
                        knoten.Distance = node.Distance + 1;

                        AdjustDistance(knoten);

                        return;
                    }

                } while (possible.Count > 0);

                // Wir sind wohl Disconnected
            }
            else
            {
                if (pred != null)
                {
                    knoten.Pred = pred;
                    knoten.Distance = distance + 1;

                    AdjustDistance(knoten);
                }
                else
                {
                    knoten.Pred = null;
                    knoten.Distance = -2;
                }
            }
        }

        private void AdjustDistance(Knoten knoten)
        {
            foreach (var succ in knoten.GetSuccessor())
            {
                succ.Distance = knoten.Distance + 1;
                AdjustDistance(succ);
            }
        }

        private bool RecalculateRecursive(Knoten knoten, List<Knoten> circle)
        {
            var foo = knoten.GetNeighbors(circle);

            if (foo.Count == 0)
            {
                return false;
            }
            else
            {
                foo.Sort();
                var best = foo.First(); foo.RemoveAt(0);

                knoten.Pred = best;
                knoten.Distance = best.Distance + 1;
            }

            return true;
        }

        private void CheckNeighbor(Knoten other, Knoten me, List<Knoten> set, ref int dist, ref Knoten pred)
        {
            if (other == null)
                return;

            if (other.Pred == me)
            {
                set.Add(other);
            }
            else if (other.Distance >= 0 && other.Distance < dist)
            {
                pred = other;
                dist = other.Distance;
            }
        }

        private void Init()
        {
            Graph = new Knoten[zeilen*spalten];

            for (int i = 0; i < zeilen; i++)
            {
                for (int j = 0; j < spalten; j++)
                {
                    var knot = new Knoten(i, j, zeilen, spalten);
                    Graph[knot.Index] = knot;
                }
            }

            for (int i = 0; i < zeilen; i++)
            {
                for (int j = 0; j < spalten; j++)
                {
                    var index = i * zeilen + j;

                    if (i != 0)
                        Graph[index].North = Graph[index - zeilen];

                    if (i != zeilen - 1)
                        Graph[index].South = Graph[index + zeilen];

                    if (j != 0)
                        Graph[index].West = Graph[index - 1];

                    if (j != spalten - 1)
                        Graph[index].East = Graph[index + 1];
                }
            }
        }

        private void GraphView_MouseDown(object sender, MouseEventArgs e)
        {
            var p = PointToIndex(e.X, e.Y);

            RemoveAt(p.X, p.Y);

            GraphView.Invalidate();
        }

        private void GraphView_Paint(object sender, PaintEventArgs e)
        {
            DrawDots();
            DrawPred();
        }

        private void DrawPred()
        {
            var g = GraphView.CreateGraphics();

            var p = new Pen(Color.Green,3);
            p.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;

            for (int i = 0; i < Graph.Length; i++)
            {
                if (Graph[i] == null || !Graph[i].TryGetPred(out Knoten pred))
                    continue;

                var pointS = IndexToPoint(i);
                var pointE = IndexToPoint(pred.Index);

                int startX, startY, endX, endY;

                if (pointS.Y == pointE.Y)
                {
                    if (pointS.X > pointE.X)
                    {
                        startY = pointS.X - pointRadius;
                        endY = pointE.X + pointRadius;
                        startX = endX = pointS.Y;
                    }
                    else
                    {
                        startY = pointS.X + pointRadius;
                        endY = pointE.X - pointRadius;
                        startX = endX = pointS.Y;
                    }
                }
                else
                {
                    if (pointS.Y > pointE.Y)
                    {
                        startX = pointS.Y - pointRadius;
                        endX = pointE.Y + pointRadius;
                        startY = endY = pointS.X;
                    }
                    else
                    {
                        startX = pointS.Y + pointRadius;
                        endX = pointE.Y - pointRadius;
                        startY = endY = pointS.X;
                    }
                }

                g.DrawLine(p, startY, startX, endY, endX);
            }
        }

        private void DrawDots()
        {
            var g = GraphView.CreateGraphics();

            var sb = new SolidBrush(Color.Black);
            var p = new Pen(Color.Black);

            for (int i = 0; i < Graph.Length; i++)
            {
                if (Graph[i] == null)
                    continue;

                var point = IndexToPoint(i);

                var x = point.X - pointRadius;
                var y = point.Y - pointRadius;

                g.DrawEllipse(p, x, y, 2* pointRadius, 2* pointRadius);
                g.FillEllipse(sb, x, y, 2* pointRadius, 2* pointRadius);
                g.DrawString(Graph[i].Distance.ToString(), Font, sb, new Point(point.X + 3, point.Y + 3));
            }
        }

        int pointDistance = 50;
        int pointRadius = 5;

        private Point IndexToPoint(int index)
        {
            var p = new Point();

            var x = index / zeilen;
            var y = index % zeilen;

            p.X = x + middleX + (pointDistance * (x-centreOfGraph.X));
            p.Y = y + middleY + (pointDistance * (y-centreOfGraph.Y));

            return p;
        }

        private Point PointToIndex(int x, int y)
        {
            var iX = (x + pointRadius - middleX + (pointDistance* centreOfGraph.X)) /(pointDistance+1);
            var iY = (y + pointRadius - middleY + (pointDistance * centreOfGraph.Y)) / (pointDistance + 1);

            return  new Point(iX, iY);
        }
    }
}
