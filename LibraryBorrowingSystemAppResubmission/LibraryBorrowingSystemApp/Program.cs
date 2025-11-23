using System;
using System.Windows.Forms;

namespace LibraryManagementSystem
{
    internal static class ApplicationMain
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BookBorrowingForm());
        }
    }
}