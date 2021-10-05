using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace draw_growing_tree
{
    public delegate void ProgrammedCommandEvent(Command command);

    public partial class RenderSurface : UserControl
    {
        public static readonly Random Random = new Random(Environment.TickCount);

        public ProgrammedCommandEvent ProgrammedCommand;

        private readonly Timer tmrUpdate = new Timer();
        private float mx = 0, my = 0;
        private Point target = new Point(0, 0);
        private LinkedList<Branch> branches = new LinkedList<Branch>();
        public const int GridSize = 8;
        public LinkedListNode<Branch>[,] GridData = new LinkedListNode<Branch>[2000 / GridSize, 2000 / GridSize];
        public LinkedList<Branch> RootBranches = new LinkedList<Branch>();
        float Speed = 2;
        float speed_X = 1, speed_Y = 1;
        bool place = false;
        LinkedList<Command> commands = new LinkedList<Command>();
        bool programmerMode = false;
        LinkedList<Point> programmerDots = new LinkedList<Point>();

        int mousegrid_x = 0, mousegrid_y = 0;

        public RenderSurface()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            tmrUpdate.Tick += tmrUpdate_Tick;
            tmrUpdate.Interval = 1;
            tmrUpdate.Start();

            MouseMove += RenderSurface_MouseMove;
            MouseDown += RenderSurface_MouseDown;
            KeyDown += RenderSurface_KeyDown;
        }

        public void AddCommand(Command command)
        {
            commands.AddLast(command);
            if (commands.Count == 1)
                CommandFinished(false);
        }

        void CommandFinished(bool remove = true)
        {
            if (remove &&
                commands.Count > 0)
                commands.RemoveFirst();

            var node = commands.First;
            if (node != null)
            {
                var value = node.Value;
                switch (value.Type)
                {
                    case CommandType.Move:
                        if (value.Tag != null && value.Tag is Point)
                        {
                            SetTarget(((Point)value.Tag).X, ((Point)value.Tag).Y);
                            if (target.X == mx &&
                                target.Y == my)
                                CommandFinished();
                        }
                        else
                            CommandFinished();
                        break;
                    case CommandType.Place:
                        if (value.Tag != null && value.Tag is bool)
                        {
                            if (place = (bool)value.Tag)
                                AddBranch(GetGridIndex((int)mx, (int)my));
                        }
                        CommandFinished();
                        break;
                    case CommandType.Reset:
                        place = false;
                        SetTarget(8, 8);
                        if (target.X == mx &&
                            target.Y == my)
                            CommandFinished();
                        break;
                    case CommandType.Speed:
                        if (value.Tag != null && value.Tag is float)
                            Speed = (float)value.Tag;
                        CommandFinished();
                        break;
                }
            }
        }

        void RenderSurface_KeyDown(object sender, KeyEventArgs e)
        {
            float speed = Speed;

            if (e.KeyCode == Keys.Space)
            {
                place = !place;
                if (programmerMode)
                    ProgrammedCommand(new Command(CommandType.Place, place));
            }
            else if (e.KeyCode == Keys.R)
            {
                if (!programmerMode)
                {
                    place = false;
                    SetTarget(8, 8);
                }
                else
                    ProgrammedCommand(new Command(CommandType.Reset));
            }
            else if (e.KeyCode == Keys.D1)
            {
                speed = 1;
            }
            else if (e.KeyCode == Keys.D2)
            {
                speed = 2;
            }
            else if (e.KeyCode == Keys.D3)
            {
                speed = 3;
            }
            else if (e.KeyCode == Keys.D4)
            {
                speed = 4;
            }
            else if (e.KeyCode == Keys.D5)
            {
                speed = 5;
            }

            if (Speed != speed)
            {
                if (!programmerMode)
                {
                    Speed = speed;
                    SetTarget(target.X, target.Y);
                }
                else
                    ProgrammedCommand(new Command(CommandType.Speed, speed));
            }

            if (e.KeyCode == Keys.P)
            {
                programmerMode = !programmerMode;
                programmerDots.Clear();
                place = false;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up)
                SetTarget(target.X, target.Y - GridSize);
            else if (keyData == Keys.Down)
                SetTarget(target.X, target.Y + GridSize);
            else if (keyData == Keys.Left)
                SetTarget(target.X - GridSize, target.Y);
            else if (keyData == Keys.Right)
                SetTarget(target.X + GridSize, target.Y);

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Gets grid index from mouse.
        /// </summary>
        public static Point GetGridIndex(int x, int y)
        {
            x -= x % GridSize;
            y -= y % GridSize;
            x /= GridSize;
            y /= GridSize;
            return new Point(x, y);
        }

        Branch AddBranch(Point gridIndex)
        {
            if (GridData[gridIndex.X, gridIndex.Y] != null)
                return null;

            var branch = new Branch(gridIndex);
            GridData[gridIndex.X, gridIndex.Y] = branches.AddLast(branch);
            return branch;
        }

        void GrowBranch(Branch branch, GrowDirection direction)
        {
            int x = branch.X;
            int y = branch.Y;

            switch (direction)
            {
                case GrowDirection.North:
                    --y;
                    break;
                case GrowDirection.East:
                    ++x;
                    break;
                case GrowDirection.South:
                    ++y;
                    break;
                case GrowDirection.West:
                    --x;
                    break;
            }

            var branch2 = AddBranch(new Point(x, y));
            if (branch2 != null)
                branch2.Root = branch;
            else
                throw new Exception("Growed out of screen or onto another branch!");
        }

        void SetTarget(int x, int y)
        {
            Point pt = GetGridIndex(x, y);
            pt.X *= GridSize;
            pt.Y *= GridSize;
            target = pt;

            float offset_x = Math.Abs((float)pt.X - mx);
            float offset_y = Math.Abs((float)pt.Y - my);
            if (offset_x != 0 && offset_y != 0)
            {
                float diff = offset_x / offset_y;
                speed_X = Math.Abs(Speed * diff);
                speed_Y = Speed;
                while (speed_X > Speed * 2)
                {
                    speed_Y /= 2;
                    speed_X /= 2;
                }
            }
            else
            {
                speed_X = speed_Y = 1.0f;
            }
        }

        void RenderSurface_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                foreach (var node in branches)
                    GridData[node.X, node.Y] = null;
                branches.Clear();
                programmerDots.Clear();
                Invalidate();
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (programmerMode)
                {
                    Point pt = GetGridIndex(e.Location.X, e.Location.Y);
                    pt.X *= GridSize;
                    pt.Y *= GridSize;
                    programmerDots.AddLast(pt);
                    
                    if (ProgrammedCommand != null)
                        ProgrammedCommand(new Command(CommandType.Move, pt));
                }
                else
                    SetTarget(e.Location.X, e.Location.Y);
            }
        }

        void RenderSurface_MouseMove(object sender, MouseEventArgs e)
        {
            Point pt = GetGridIndex(e.Location.X, e.Location.Y);
            mousegrid_x = pt.X * GridSize;
            mousegrid_y = pt.Y * GridSize;
        }

        bool IsEmptyGridSlot(int gx, int gy)
        {
            return
                ClientRectangle.Contains(gx * GridSize, gy * GridSize) &&
                GridData[gx, gy] == null;
        }

        GrowDirection[] GetPossibleDirections(Branch branch)
        {
            List<GrowDirection> directions = new List<GrowDirection>();

            if (IsEmptyGridSlot(branch.X, branch.Y - 1))
                directions.Add(GrowDirection.North);
            if (IsEmptyGridSlot(branch.X + 1, branch.Y))
                directions.Add(GrowDirection.East);
            if (IsEmptyGridSlot(branch.X, branch.Y + 1))
                directions.Add(GrowDirection.South);
            if (IsEmptyGridSlot(branch.X - 1, branch.Y))
                directions.Add(GrowDirection.West);

            return directions.ToArray();
        }

        GrowDirection GetRandomDirection(Branch branch)
        {
            GrowDirection[] directions = GetPossibleDirections(branch);

            if (directions.Length == 0)
                return GrowDirection.None;

            return directions[Random.Next(directions.Length)];
        }

        void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (!programmerMode)
            {
                bool isMoving = mx != target.X || my != target.Y;

                if (mx < target.X)
                    mx = Math.Min(target.X, mx + speed_X);
                else if (mx > target.X)
                    mx = Math.Max(target.X, mx - speed_X);
                if (my < target.Y)
                    my = Math.Min(target.Y, my + speed_Y);
                else if (my > target.Y)
                    my = Math.Max(target.Y, my - speed_Y);

                if (place)
                    AddBranch(GetGridIndex((int)mx, (int)my));

                if (mx == target.X &&
                    my == target.Y &&
                    isMoving)
                {
                    CommandFinished();
                }
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
            using (var pen = new Pen(new SolidBrush(Color.FromArgb(unchecked((int)0xff303030)))))
            {
                for (int x = 0; x < Width; x += GridSize)
                    e.Graphics.DrawLine(pen, new Point(x, 0), new Point(x, Height));

                for (int y = 0; y < Height; y += GridSize)
                    e.Graphics.DrawLine(pen, new Point(0, y), new Point(Width, y));
            }

            foreach (var node in branches)
                e.Graphics.FillRectangle(new SolidBrush(node.Color), new Rectangle(node.X * GridSize, node.Y * GridSize, GridSize, GridSize));

            foreach (var node in branches)
            {
                if (node.Text != null)
                {
                    using (var font = new Font("Calibri", 14, FontStyle.Bold))
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                        SizeF size = e.Graphics.MeasureString(node.Text, font);
                        int x = node.X * GridSize + GridSize / 2 - (int)size.Width / 2;
                        int y = node.Y * GridSize + GridSize + 5;
                        e.Graphics.DrawString(node.Text, font, Brushes.White, new PointF(x, y));
                    }
                }
            }

            if (programmerMode)
            {
                foreach (var node in programmerDots)
                    e.Graphics.FillRectangle(Brushes.Yellow, new Rectangle(node.X, node.Y, GridSize, GridSize));
            }

            using (var font = new Font("Calibri", 13))
            {
                string text = string.Format("Speed: {0:0.00}\nPos: {1:0}x{2:0}\nCommands: {3}", Speed, mx, my, commands.Count);
                var size = e.Graphics.MeasureString(text, font);
                e.Graphics.DrawString(text, font, Brushes.LimeGreen, new PointF(Width - size.Width - 15, 5));

                if (programmerMode &&
                    Environment.TickCount % 1000 < 750)
                {
                    text = "Programmer Mode";
                    size = e.Graphics.MeasureString(text, font);
                    e.Graphics.DrawString(text, font, Brushes.Lime, new PointF(Width - size.Width, Height - size.Height));
                }
            }

            // lines

            int _x = programmerMode ? mousegrid_x : (int)mx;
            int _y = programmerMode ? mousegrid_y : (int)my;

            using (var pen = new Pen(Brushes.Yellow))
            {
                e.Graphics.DrawLine(pen, new Point(_x + (GridSize / 2), 0), new Point(_x + (GridSize / 2), Height));
                e.Graphics.DrawLine(pen, new Point(0, _y + (GridSize / 2)), new Point(Width, _y + (GridSize / 2)));
            }

            e.Graphics.FillRectangle(Brushes.Red, new Rectangle(_x, _y, GridSize, GridSize));
        }
    }

    [Flags]
    public enum GrowDirection : int
    {
        /// <summary>
        /// No direction
        /// </summary>
        None = 0,
        /// <summary>
        /// Up
        /// </summary>
        North = 1,
        /// <summary>
        /// Right
        /// </summary>
        East = 2,
        /// <summary>
        /// Down
        /// </summary>
        South = 4,
        /// <summary>
        /// Left
        /// </summary>
        West = 8
    }

    public class Command
    {
        public CommandType Type { get; private set; }
        public object Tag { get; private set; }

        public Command(CommandType type)
        {
            Type = type;
            Tag = null;
        }

        public Command(CommandType type, object tag)
        {
            Type = type;
            Tag = tag;
        }
    }

    public enum CommandType
    {
        Move,
        Reset,
        Place,
        Speed
    }
}
