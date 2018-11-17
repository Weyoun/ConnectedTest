using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace WindowsFormsApp2
{

    public partial class Form1 : Form
    {
        int width = 9;
        int height = 9;
        Knoten[] Graph;

        int middleX, middleY;
        Point centreOfGraph;

        int recalculating, checkCircle;
        Stopwatch stopWatch = new Stopwatch();

        private bool withLocation = false;

        public Form1()
        {
            InitializeComponent();
            Init();

            centreOfGraph = new Point(width / 2, height / 2);
            CalculateDistance(centreOfGraph.X, centreOfGraph.Y);

            middleX = GraphView.Size.Width / 2;
            middleY = GraphView.Size.Height / 2;
        }

        private void CalculateDistance(int x, int y)
        {
            Queue<Knoten> toCheck = new Queue<Knoten>();
            HashSet<Knoten> added = new HashSet<Knoten>();

            toCheck.Enqueue(Graph[y * width + x]);

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

            if (Graph.TryGetAt(x, y, width, out Knoten knoten))
            {
                knoten.Distance = -1;
                knoten.RemoveLinks();

                recalculating = 0;
                checkCircle = 0;

                if (knoten.North != null && knoten.North.Pred == knoten)
                {
                    Recalculate(knoten.North);
                }

                if (knoten.East != null && knoten.East.Pred == knoten)
                {
                    Recalculate(knoten.East);
                }

                if (knoten.South != null && knoten.South.Pred == knoten)
                {
                    Recalculate(knoten.South);
                }

                if (knoten.West != null && knoten.West.Pred == knoten)
                {
                    Recalculate(knoten.West);
                }
            }
        }

        private void Recalculate(Knoten knoten)
        {
            var circle = new List<Knoten>();

            if (!RecalculateRecursive(knoten, circle))
            {
                knoten.Pred = null;
                SetDisconnected(knoten);
            }

            //var distance = int.MaxValue;
            //Knoten pred = null;
            //List<Knoten> possible = new List<Knoten>();

            //CheckNeighbor(knoten.North, knoten, possible, ref distance, ref pred);
            //CheckNeighbor(knoten.East, knoten, possible, ref distance, ref pred);
            //CheckNeighbor(knoten.South, knoten, possible, ref distance, ref pred);
            //CheckNeighbor(knoten.West, knoten, possible, ref distance, ref pred);


            //if (pred != null)
            //{
            //    knoten.Pred = pred;
            //    knoten.Distance = distance + 1;

            //    AdjustDistance(knoten);
            //}
            //else if (possible.Count > 0)
            //{
            //    possible.Sort();
            //    List<Knoten> circle = new List<Knoten>();
            //    circle.Add(knoten);

            //    do
            //    {
            //        var node = possible.Pop();

            //        if (RecalculateRecursive(node, circle))
            //        {
            //            knoten.Pred = node;
            //            knoten.Distance = node.Distance + 1;

            //            //AdjustDistance(knoten);

            //            return;
            //        }

            //    } while (possible.Count > 0);

            //    // Wir sind wohl Disconnected
            //    knoten.Pred = null;
            //    SetDisconnected(knoten);
            //}
            //else
            //{
            //    knoten.Pred = null;
            //    SetDisconnected(knoten);
            //}
        }

        private void SetDisconnected(Knoten knoten)
        {
            knoten.Distance = -2;

            foreach (var other in knoten.GetAllNeighbors())
            {
                SetDisconnected(other);
            }
        }

        private void AdjustDistance(Knoten knoten)
        {
            if (knoten == null)
                return;

            foreach (var succ in knoten.GetSuccessor())
            {
                succ.Distance = knoten.Distance + 1;
                AdjustDistance(succ);
            }
        }

        private bool RecalculateRecursive(Knoten knoten, List<Knoten> circle)
        {
            recalculating++;

            circle.Add(knoten);
            var foo = knoten.GetNeighbors(circle).Where(x => !circle.Contains(x.Pred)).ToList();
            foo.Sort();

            while  (foo.Count > 0)
            {
                var best = foo.Pop();

                if (IsCricle(best.Pred, circle))
                    continue;

                knoten.Pred = best;
                knoten.Distance = best.Distance + 1;

                return true;
            }

            var succ = knoten.GetSuccessor().Where(x => !circle.Contains(x)).ToList();
            succ.Sort();

            while (succ.Count > 0)
            {
                var current = succ.Pop();

                if (RecalculateRecursive(current, new List<Knoten>(circle)))
                {
                    knoten.Pred = current;
                    knoten.Distance = current.Distance + 1;

                    return true;
                }
            }

            return false;
        }

        private bool IsCricle(Knoten current, List<Knoten> list)
        {
            if (current == null)
                return false;

            do
            {
                checkCircle++;

                if (list.Contains(current))
                    return true;

                current = current.Pred;
            } while (current != null);

            return false;
        }

        private bool CheckCircle(Knoten knoten, List<Knoten> circle)
        {
            return !circle.Contains(knoten);
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
            Graph = new Knoten[width*height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var knot = new Knoten(x, y, width);
                    Graph[knot.Index] = knot;

                    if (x != 0)
                    {
                        knot.West = Graph[knot.Index - 1];
                        knot.West.East = knot;
                    }

                    if (y != 0)
                    {
                        knot.North = Graph[knot.Index - width];
                        knot.North.South = knot;
                    }
                }
            }
        }

        private void GraphView_MouseDown(object sender, MouseEventArgs e)
        {
            var p = PointToIndex(e.X, e.Y);

            stopWatch = Stopwatch.StartNew();

            RemoveAt(p.X, p.Y);

            stopWatch.Stop();

            GraphView.Invalidate();
        }

        private void GraphView_Paint(object sender, PaintEventArgs e)
        {
            DrawDots();
            DrawPred();
            //DrawNeighbors();
            DrawStatistics();
        }

        private void DrawStatistics()
        {
            var g = GraphView.CreateGraphics();

            var sb = new SolidBrush(Color.Black);

            var str = $"Recalculating: {recalculating}\nCheck Circle: {checkCircle}\nTime: {stopWatch.Elapsed}";

            g.DrawString(str, Font, sb, new Point(3, 3));
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

        private void DrawNeighbors()
        {
            var g = GraphView.CreateGraphics();

            var p = new Pen(Color.Blue, 3);
            p.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;

            for (int i = 0; i < Graph.Length; i++)
            {
                if (Graph[i] == null)
                    continue;

                foreach (var pred in Graph[i].GetAllNeighbors())
                {
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

                if (!withLocation)
                {
                    g.DrawString(Graph[i].Distance.ToString(), Font, sb, new Point(point.X + 3, point.Y + 3));
                }
                else
                {
                    g.DrawString(Graph[i].Distance.ToString() + "@(" + Graph[i].ToString() + ")", Font, sb, new Point(point.X + 3, point.Y + 3));
                }
            }
        }

        int pointDistance = 50;
        int pointRadius = 5;

        private Point IndexToPoint(int index)
        {
            var p = new Point();

            var x = index % width;
            var y = index / width;

            p.X = x + middleX + (pointDistance * (x-centreOfGraph.X));
            p.Y = y + middleY + (pointDistance * (y-centreOfGraph.Y));

            return p;
        }

        private void GraphView_Resize(object sender, EventArgs e)
        {
            middleX = GraphView.Size.Width / 2;
            middleY = GraphView.Size.Height / 2;
            GraphView.Invalidate();
        }

        private Point PointToIndex(int x, int y)
        {
            var iX = (x + pointRadius - middleX + (pointDistance* centreOfGraph.X)) /(pointDistance+1);
            var iY = (y + pointRadius - middleY + (pointDistance * centreOfGraph.Y)) / (pointDistance + 1);

            return  new Point(iX, iY);
        }
    }
}
