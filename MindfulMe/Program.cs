using MindfulMe.Forms;
using MindfulMe.Services;

namespace MindfulMe
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            // Show login form
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK && loginForm.LoggedInUser != null)
                {
                    // Login successful, show main form
                    Application.Run(new MainForm(loginForm.LoggedInUser));
                }
                else
                {
                    MessageBox.Show("Login required to access MindfulMe.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}