using natom.ecomm.sync.kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace natom.ecomm.sync.background
{
    public static class Program
    {
        public static string EjecucionId = Ejecutador.GenerateEjecucionId();

        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
