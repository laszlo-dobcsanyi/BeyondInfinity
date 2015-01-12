using System;
using System.Windows.Forms;

namespace BeyondInfinity_Editor
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            MainForm MainForm = new MainForm();
            MainForm.Show();

            while (MainForm.Created)
            {
                MainForm.Render();
                Application.DoEvents();

                if (MainForm.ContainsFocus) { }
                else System.Threading.Thread.Sleep(5);
            }
        }
    }
}
