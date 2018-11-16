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

            //RemoveAt(1, 1);
            //RemoveAt(3, 1);
            //RemoveAt(2, 1);
        }

        private void CalculateDistance(int x, int y)
        {
            Queue<Knoten> toCheck = new Queue<Knoten>();
            HashSet<Knoten> added = new HashSet<Knoten>();

            toCheck.Enqueue(Graph[x * spalten + y]);

            while (toCheck.Count > 0)
            {
                var current = toCheck.Dequeue();

                if (current.North != null && !toCheck.Contains(current.North) && !added.Contains(current.North))
                {
                    current.North.Pred = current;
                    current.North.Distance = current.Distance + 1;
                    toCheck.Enqueue(current.North);
                }

                if (current.East != null && !toCheck.Contains(current.East) && !added.Contains(current.East))
                {
                    current.East.Pred = current;
                    current.East.Distance = current.Distance + 1;
                    toCheck.Enqueue(current.East);
                }

                if (current.South != null && !toCheck.Contains(current.South) && !added.Contains(current.South))
                {
                    current.South.Pred = current;
                    current.South.Distance = current.Distance + 1;
                    toCheck.Enqueue(current.South);
                }

                if (current.West != null && !toCheck.Contains(current.West) && !added.Contains(current.West))
                {
                    current.West.Pred = current;
                    current.West.Distance = current.Distance + 1;
                    toCheck.Enqueue(current.West);
                }

                added.Add(current);
            }
        }

        private void RemoveAt(int x, int y)
        {

            if (Graph.TryGetAt(x, y, zeilen, out Knoten knoten))
            {
                knoten.Distance = -1;

                if (knoten.North != null && knoten.North.Pred == knoten)
                    Recalculate(knoten.North);

                if (knoten.East != null && knoten.East.Pred == knoten)
                    Recalculate(knoten.East);

                if (knoten.South != null && knoten.South.Pred == knoten)
                    Recalculate(knoten.South);

                if (knoten.West != null && knoten.West.Pred == knoten)
                    Recalculate(knoten.West);
            }
        }

        private void Recalculate(Knoten knoten)
        {
            var distance = int.MaxValue;
            Knoten pred = null;

            if (knoten.North != null)
            {
                if (knoten.North.Distance >= 0 && knoten.North.Distance < distance && knoten.North.Pred != knoten)
                {
                    pred = knoten.North;
                    distance = knoten.North.Distance;
                }
            }

            if (knoten.East != null)
            {
                if (knoten.East.Distance >= 0 && knoten.East.Distance < distance && knoten.East.Pred != knoten)
                {
                    pred = knoten.East;
                    distance = knoten.East.Distance;
                }
            }

            if (knoten.South != null)
            {
                if (knoten.South.Distance >= 0 && knoten.South.Distance < distance && knoten.South.Pred != knoten)
                {
                    pred = knoten.South;
                    distance = knoten.South.Distance;
                }
            }

            if (knoten.West != null)
            {
                if (knoten.West.Distance >= 0 && knoten.West.Distance < distance && knoten.West.Pred != knoten)
                {
                    pred = knoten.West;
                    distance = knoten.West.Distance;
                }
            }

            if (pred != null)
            {
                knoten.Pred = pred;
                knoten.Distance = distance + 1;
            }
            else
            {
                knoten.Pred = null;
                knoten.Distance = -2;
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

                g.DrawEllipse(p, point.X - pointRadius, point.Y - pointRadius, 2* pointRadius, 2* pointRadius);
                g.FillEllipse(sb, point.X - pointRadius, point.Y - pointRadius, 2* pointRadius, 2* pointRadius);
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
