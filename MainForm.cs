using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace draw_growing_tree
{
    public partial class MainForm : Form
    {
        private ProgrammerConsole console = null;
        public string CommandList { get { return richTextBox1.Text; } set { richTextBox1.Text = value; } }

        public MainForm()
        {
            InitializeComponent();
            console = new ProgrammerConsole(this);
            console.Show();

            FormClosing += Form1_FormClosing;

            if (File.Exists("commands.txt"))
                richTextBox1.Text = File.ReadAllText("commands.txt");

            button1.Click += runToolStripMenuItem_Click;

            renderSurface1.ProgrammedCommand += (command) =>
                {
                    switch (command.Type)
                    {
                        case CommandType.Move:
                            console.AddCommand(string.Format("move {0} {1}", ((Point)command.Tag).X, ((Point)command.Tag).Y));
                            break;
                        case CommandType.Place:
                            console.AddCommand(string.Format("place {0}", (bool)command.Tag ? 1 : 0));
                            break;
                        case CommandType.Reset:
                            console.AddCommand("reset");
                            break;
                        case CommandType.Speed:
                            console.AddCommand(string.Format("speed {0}", ((float)command.Tag).ToString().Replace(",", ".")));
                            break;
                    }
                };
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText("commands.txt", richTextBox1.Text);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text = richTextBox1.Text;
            // allows to select text in the rich edit and execute only the marked text
            if (richTextBox1.SelectedText.Length > 0)
                text = richTextBox1.SelectedText;

            string[] cmds = text.Replace("\r", "").Split('\n');
            for (int i = 0; i < cmds.Length; ++i)
            {
                string cmd = cmds[i].Trim();
                if (cmd.Length > 0 &&
                    cmd.Contains("//"))
                    cmd = cmd.Substring(0, cmd.IndexOf("//")).Trim();

                if (cmd.Length > 0)
                {
                    if (Regex.IsMatch(cmd, @"^move \d+ \d+$", RegexOptions.IgnoreCase))
                    {
                        string data = cmd.Substring(5);
                        int x, y;
                        if (int.TryParse(data.Substring(0, data.IndexOf(" ")), out x))
                        {
                            data = data.Substring(data.IndexOf(" ") + 1);
                            if (int.TryParse(data, out y))
                                renderSurface1.AddCommand(new Command(CommandType.Move, new Point(x, y)));
                        }
                    }
                    else if (Regex.IsMatch(cmd, @"^place (0|1)$", RegexOptions.IgnoreCase))
                    {
                        string data = cmd.Substring(6);
                        renderSurface1.AddCommand(new Command(CommandType.Place, data == "1"));
                    }
                    else if (Regex.IsMatch(cmd, @"^reset$", RegexOptions.IgnoreCase))
                    {
                        renderSurface1.AddCommand(new Command(CommandType.Reset));
                    }
                    else if (Regex.IsMatch(cmd, @"^speed [-+]?[0-9]*\.?[0-9]+$", RegexOptions.IgnoreCase))
                    {
                        string data = cmd.Substring(6).Replace(".", ",");
                        float speed = 0.0f;
                        if (float.TryParse(data, out speed))
                            renderSurface1.AddCommand(new Command(CommandType.Speed, speed));
                    }
                }
            }
        }
    }
}