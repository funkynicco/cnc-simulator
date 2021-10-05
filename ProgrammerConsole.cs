using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace draw_growing_tree
{
    public partial class ProgrammerConsole : Form
    {
        private MainForm _mainForm = null;

        public ProgrammerConsole(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            FormClosing += (sender, e) => e.Cancel = true;
            button1.Click += (sender, e) => richTextBox1.Text = "";
            button2.Click += (sender, e) =>
                {
                    // Fetch
                    richTextBox1.Text = _mainForm.CommandList;
                    richTextBox1.Select(richTextBox1.Text.Length, 0);
                    richTextBox1.ScrollToCaret();
                };
            button3.Click += (sender, e) =>
                {
                    // Send
                    _mainForm.CommandList = richTextBox1.Text;
                };
        }

        public void AddCommand(string text)
        {
            if (richTextBox1.Text.Length > 0)
                richTextBox1.Text += "\n";
            richTextBox1.Text += text;
            richTextBox1.Select(richTextBox1.Text.Length, 0);
            richTextBox1.ScrollToCaret();
        }
    }
}
