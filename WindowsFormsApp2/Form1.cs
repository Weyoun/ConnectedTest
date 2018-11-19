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

        #region InternalClasses
        [Flags]
        enum CellType : byte
        {
            North = 1 << 0,
            East = 1 << 1,
            South = 1 << 2,
            West = 1 << 3
        }

        class ShipCell
        {
            readonly ShipCell[] neighbors;
            readonly int x, y, width;
            public CellType CellType { get; private set; }
            public bool Disconnected { get; set; }

            static readonly int north = 0, east = 1, south = 2, west = 3;

            public int Index { get; private set; }
            public ShipCell Pred { get; set; }

            public ShipCell(int x, int y, int width, CellType type)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.CellType = type;
                Index = y * width + x;
                neighbors = new ShipCell[4];
            }

            public void UnlinkNeighbors()
            {
                if ((CellType & CellType.North) != 0)
                    neighbors[north].UnlinkFromNeighbor(CellType.South);

                if ((CellType & CellType.East) != 0)
                    neighbors[east].UnlinkFromNeighbor(CellType.West);

                if ((CellType & CellType.South) != 0)
                    neighbors[south].UnlinkFromNeighbor(CellType.North);

                if ((CellType & CellType.West) != 0)
                    neighbors[west].UnlinkFromNeighbor(CellType.East);
            }

            public void UnlinkFromNeighbor(CellType type)
            {
                switch (type)
                {
                    case CellType.North:
                        neighbors[north] = null;
                        CellType ^= CellType.North;
                        break;
                    case CellType.East:
                        neighbors[east] = null;
                        CellType ^= CellType.East;
                        break;
                    case CellType.South:
                        neighbors[south] = null;
                        CellType ^= CellType.South;
                        break;
                    case CellType.West:
                        neighbors[west] = null;
                        CellType ^= CellType.West;
                        break;
                    default:
                        break;
                }
            }

            public void SetNeighbor(CellType type, ShipCell cell)
            {
                switch (type)
                {
                    case CellType.North:
                        neighbors[north] = cell;
                        break;
                    case CellType.East:
                        neighbors[east] = cell;
                        break;
                    case CellType.South:
                        neighbors[south] = cell;
                        break;
                    case CellType.West:
                        neighbors[west] = cell;
                        break;
                }
            }

            public IEnumerable<ShipCell> GetSuccessors()
            {
                return neighbors.Where(x => x != null && x.Pred == this);
            }

            public IEnumerable<ShipCell> GetSuccessors(HashSet<ShipCell> circle)
            {
                return neighbors.Where(x => x != null && x.Pred == this && !circle.Contains(x));
            }

            public IEnumerable<ShipCell> GetNotSuccessors(HashSet<ShipCell> circle)
            {
                return neighbors.Where(x => x != null && x.Pred != this && !circle.Contains(x) && !circle.Contains(x.Pred));
            }

            public IEnumerable<ShipCell> GetNeighbors()
            {
                return neighbors.Where(x => x != null);
            }
        }
        #endregion

        int width = 9;
        int height = 9;
        ShipCell[] cells;

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
            CalculateSpawningTree(centreOfGraph.X, centreOfGraph.Y);

            middleX = GraphView.Size.Width / 2;
            middleY = GraphView.Size.Height / 2;
        }

        void CalculateSpawningTree(int x, int y)
        {
            Queue<ShipCell> toCheck = new Queue<ShipCell>();
            HashSet<ShipCell> added = new HashSet<ShipCell>();

            toCheck.Enqueue(cells[y * width + x]);

            while (toCheck.Count > 0)
            {
                var current = toCheck.Dequeue();

                foreach (var neighbor in current.GetNeighbors())
                {
                    if (toCheck.Contains(neighbor) || added.Contains(neighbor))
                        continue;

                    neighbor.Pred = current;
                    toCheck.Enqueue(neighbor);
                }

                added.Add(current);
            }
        }

        public bool CheckHit(uint x, uint y)
        {
            var index = y * width + x;

            if (index > cells.Length)
                return false;

            var cell = cells[index];

            if (cell == null)
                return false;

            cell.UnlinkNeighbors();
            cells[index] = null;

            var hasWreck = false;

            foreach (var succ in cell.GetSuccessors())
            {
                if (Recalculate(succ))
                {
                    hasWreck = true;
                }
            }

            return hasWreck;
        }

        bool Recalculate(ShipCell cell)
        {
            var circle = new HashSet<ShipCell>();

            if (!RecalculateRecursive(cell, circle))
            {
                cell.Pred = null;

                SetDisconnected(cell);

                return true;
            }

            return false;
        }

        private void SetDisconnected(ShipCell knoten)
        {
            knoten.Disconnected = true;

            foreach (var other in knoten.GetNeighbors())
            {
                if (other.Disconnected)
                    continue;

                SetDisconnected(other);
            }
        }

        bool RecalculateRecursive(ShipCell cell, HashSet<ShipCell> circle)
        {
            circle.Add(cell);

            var neigh = cell.GetNotSuccessors(circle);

            foreach (var best in neigh)
            {
                if (IsCircle(best.Pred, circle))
                    continue;

                cell.Pred = best;

                return true;
            }

            var succ = cell.GetSuccessors(circle);

            foreach (var best in succ)
            {
                if (RecalculateRecursive(best, circle))
                {
                    cell.Pred = best;

                    return true;
                }
            }

            return false;
        }

        bool IsCircle(ShipCell current, HashSet<ShipCell> circle)
        {
            if (current == null)
                return false;

            do
            {
                if (circle.Contains(current))
                    return true;

                current = current.Pred;
            } while (current != null);

            return false;
        }

        private void Init()
        {
            cells = new ShipCell[width*height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    CellType type = 0;

                    if (y != 0)
                        type |= CellType.North;

                    if (y != height - 1)
                        type |= CellType.South;

                    if (x != 0)
                        type |= CellType.West;

                    if (x != width - 1)
                        type |= CellType.East;

                    var cell = new ShipCell(x, y, width, type);
                    cells[cell.Index] = cell;

                    SetNeighbors(cell);
                }
            }
        }

        void SetNeighbors(ShipCell cell)
        {
            if ((cell.CellType & CellType.North) != 0)
            {
                var other = cells[cell.Index - width];

                cell.SetNeighbor(CellType.North, other);
                other.SetNeighbor(CellType.South, cell);
            }

            if ((cell.CellType & CellType.West) != 0)
            {
                var other = cells[cell.Index - 1];

                cell.SetNeighbor(CellType.West, other);
                other.SetNeighbor(CellType.East, cell);
            }
        }

        private void GraphView_MouseDown(object sender, MouseEventArgs e)
        {
            var p = PointToIndex(e.X, e.Y);

            stopWatch = Stopwatch.StartNew();

            CheckHit((uint)p.X, (uint)p.Y);

            stopWatch.Stop();

            GraphView.Invalidate();
        }

        private void GraphView_Paint(object sender, PaintEventArgs e)
        {
            DrawDots();
            DrawPred();
            //DrawNeighbors();
            //DrawStatistics();
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

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null)
                    continue;

                ShipCell pred = cells[i].Pred;

                if (pred == null)
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

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null)
                    continue;

                foreach (var pred in cells[i].GetNeighbors())
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

            var sbB = new SolidBrush(Color.Black);
            var pB = new Pen(Color.Black);

            var sbR = new SolidBrush(Color.Red);
            var pR = new Pen(Color.Red);

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null)
                    continue;

                var point = IndexToPoint(i);

                var x = point.X - pointRadius;
                var y = point.Y - pointRadius;

                if (cells[i].Disconnected)
                {
                    g.DrawEllipse(pR, x, y, 2 * pointRadius, 2 * pointRadius);
                    g.FillEllipse(sbR, x, y, 2 * pointRadius, 2 * pointRadius);
                }
                else
                {
                    g.DrawEllipse(pB, x, y, 2 * pointRadius, 2 * pointRadius);
                    g.FillEllipse(sbB, x, y, 2 * pointRadius, 2 * pointRadius);
                }

                //if (!withLocation)
                //{
                //    g.DrawString(cells[i].Distance.ToString(), Font, sb, new Point(point.X + 3, point.Y + 3));
                //}
                //else
                //{
                //    g.DrawString(cells[i].Distance.ToString() + "@(" + cells[i].ToString() + ")", Font, sb, new Point(point.X + 3, point.Y + 3));
                //}
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
