using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace draw_growing_tree
{
    public class Branch
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int GrowTimes { get; set; }
        public Color Color { get; set; }
        public string Text { get; set; }

        private Branch _root;
        public Branch Root
        {
            get
            {
                if (_root != null)
                    return _root.Root;
                return this;
            }
            set
            {
                _root = value;
            }
        }

        public int Branches
        {
            get
            {
                int count = 1;
                var root = _root;
                while (root != null)
                {
                    ++count;
                    root = root._root;
                }

                return count;
            }
        }

        public Branch(Point pt)
        {
            X = pt.X;
            Y = pt.Y;
            GrowTimes = 1;
            Color = Color.Blue;
            Root = null;
            Text = null;
        }
    }
}