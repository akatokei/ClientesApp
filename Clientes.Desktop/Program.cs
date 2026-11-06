using System;
using System.Windows.Forms;

namespace ClientesApp
{
    static class Program
    {
        // Aquí empieza todo: se abre la ventana principal
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new FormPrincipal());
        }
    }
}