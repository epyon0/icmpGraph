using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace icmpGraph
{
    public class debug
    {
        public static Form verboseForm = new Form();
        public static RichTextBox verboseRichTextBox = new RichTextBox();
        public static void createForm()
        {
            int width = 1000;
            int height = 500;


            verboseRichTextBox.Width = width;
            verboseRichTextBox.Height = height-22;
            verboseRichTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            verboseRichTextBox.Location = new System.Drawing.Point(0, 0);
            verboseRichTextBox.Font = new System.Drawing.Font("Consolas", 8, System.Drawing.FontStyle.Regular);
            verboseRichTextBox.ForeColor = System.Drawing.Color.LightGreen;
            verboseRichTextBox.BackColor = System.Drawing.Color.Black;
            Form verboseForm = new Form();

            verboseForm.Name = "verboseForm";
            verboseForm.Text = "Verbose Output";
            verboseForm.Width = width;
            verboseForm.Height = height;
            verboseForm.Controls.Add(verboseRichTextBox);
            
            Thread verboseThread = new Thread(() =>
            {
                Application.Run(verboseForm);
            });

            verboseThread.SetApartmentState(ApartmentState.STA);
            verboseThread.Start();

            verboseForm.SendToBack();
            verboseForm.Invalidate();
        }
        public static void verbose(string msg)
        {
            foreach (Form tmpForm in Application.OpenForms)
            {
                if (tmpForm.Name == "verboseForm") {
                    verboseRichTextBox.AppendText(msg + '\n');
                    verboseRichTextBox.SelectionStart = verboseRichTextBox.Text.Length;
                    verboseRichTextBox.ScrollToCaret();
                    break;
                }
            }
            
        }
    }
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string[] args = Environment.GetCommandLineArgs();
            

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg)
                {
                    case "-v":
                    case "--verbose":
                        debug.createForm();
                        break;
                }
            }

            Application.Run(new Form1());


        }
    }
}
