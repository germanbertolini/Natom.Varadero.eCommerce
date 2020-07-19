using natom.ecomm.sync.kernel;
using natom.ecomm.sync.kernel.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace natom.ecomm.sync.console
{
    public class Program
    {
        public static SyncScheduleInfo scheduleInfo = new SyncScheduleInfo();
        public static string EjecucionId = Ejecutador.GenerateEjecucionId();

        static void Main(string[] args)
        {
            Timer t = new Timer(TimerCallback, null, 60000, 60000); //CADA 1 MINUTO Y DENTRO DE 1 MINUTO
            TimerCallback(null);    //LA PRIMERA VEZ LO EJECUTAMOS A MANO PORQUE SINO SE VA EN OTRO HILO
        }

        private static void TimerCallback(Object o)
        {
            try
            {
                LogManager.LogAlive("console", Program.EjecucionId);
            }
            catch (Exception ex) { }
            ReadSyncSettingsJSON();
            PrintMainScreen();
        }

        private static void ReadSyncSettingsJSON()
        {
            string settingsPath = Environment.CurrentDirectory + "\\sync.settings.json";
            string contentSettings = null;
            if (File.Exists(settingsPath))
            {
                contentSettings = File.ReadAllText(settingsPath);
                scheduleInfo = JsonConvert.DeserializeObject<SyncScheduleInfo>(contentSettings);
                Ejecutador.CancellationTokenMS = scheduleInfo.CancellationTokenMS;
            }
        }

        public static void PrintMainScreen()
        {
            string backgroundProcessFullPath = Environment.CurrentDirectory + "\\natom.ecomm.sync.background.exe";
            bool backgroundRunning = Ejecutador.ProgramIsRunning(backgroundProcessFullPath);

            Console.Clear();
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine("%%                VARADERO ECOMMERCE             %%");
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            Console.WriteLine(">> STATUS BACKGROUND PROCESS:   " + (backgroundRunning ? "EN EJECUCIÓN" : "APAGADO"));
            Console.WriteLine("                                                   ");
            Console.WriteLine("                                        PROX. SYNC.");
            Console.WriteLine("1)  Sync LISTA DE PRECIOS                 {0}hs", backgroundRunning ? GetNextExecutionTime("ListaPreciosRoutine") : "--:--");
            Console.WriteLine("2)  Sync CONDICIONES DE VENTA             {0}hs", backgroundRunning ? GetNextExecutionTime("CondicionDeVentaRoutine") : "--:--");
            Console.WriteLine("3)  Sync CLIENTES DIRECCIONES             {0}hs", backgroundRunning ? GetNextExecutionTime("ClienteDireccionRoutine") : "--:--");
            Console.WriteLine("4)  Sync CLIENTES                         {0}hs", backgroundRunning ? GetNextExecutionTime("ClienteRoutine") : "--:--");
            Console.WriteLine("5)  Sync ARTICULOS                        {0}hs", backgroundRunning ? GetNextExecutionTime("ArticuloRoutine") : "--:--");
            Console.WriteLine("6)  Sync RUBROS                           {0}hs", backgroundRunning ? GetNextExecutionTime("RubroRoutine") : "--:--");
            Console.WriteLine("7)  Sync SUBRUBROS                        {0}hs", backgroundRunning ? GetNextExecutionTime("SubRubroRoutine") : "--:--");
            Console.WriteLine("8)  Sync GRUPOS                           {0}hs", backgroundRunning ? GetNextExecutionTime("ArticuloGrupoRoutine") : "--:--");
            Console.WriteLine("9)  Sync MARCAS                           {0}hs", backgroundRunning ? GetNextExecutionTime("MarcaRoutine") : "--:--");
            Console.WriteLine("10) Sync PEDIDOS                          {0}hs", backgroundRunning ? GetNextExecutionTime("PedidoRoutine") : "--:--");
            Console.WriteLine("                                                   ");
            Console.Write("Ingrese la opcion a syncronizar: ");

            int operacion;
            string value;
            do
            {
                value = Console.ReadLine();
            }
            while (!int.TryParse(value, out operacion));

            switch (operacion)
            {
                case 1:
                    Console.WriteLine("SE LANZA EJECUCIÓN ListaPreciosRoutine.");
                    Ejecutador.Ejecutar("console", Program.EjecucionId, "ListaPreciosRoutine");
                    Console.WriteLine("FIN EJECUCIÓN ListaPreciosRoutine.");
                    break;
                case 2:
                    Console.WriteLine("SE LANZA EJECUCIÓN CondicionDeVentaRoutine.");
                    Ejecutador.Ejecutar("console", Program.EjecucionId, "CondicionDeVentaRoutine");
                    Console.WriteLine("FIN EJECUCIÓN CondicionDeVentaRoutine.");
                    break;
                case 3:
                    Console.WriteLine("SE LANZA EJECUCIÓN ClienteDireccionRoutine.");
                    Ejecutador.Ejecutar("console", Program.EjecucionId, "ClienteDireccionRoutine");
                    Console.WriteLine("FIN EJECUCIÓN ClienteDireccionRoutine.");
                    break;
                case 4:
                    Console.WriteLine("SE LANZA EJECUCIÓN ClienteRoutine.");
                    Ejecutador.Ejecutar("console", Program.EjecucionId, "ClienteRoutine");
                    Console.WriteLine("FIN EJECUCIÓN ClienteRoutine.");
                    break;
                case 5:
                    Console.WriteLine("SE LANZA EJECUCIÓN ArticuloRoutine.");
                    Ejecutador.Ejecutar("console", Program.EjecucionId, "ArticuloRoutine");
                    Console.WriteLine("FIN EJECUCIÓN ArticuloRoutine.");
                    break;
                case 6:
                    Console.WriteLine("SE LANZA EJECUCIÓN RubroRoutine.");
                    Ejecutador.Ejecutar("console", Program.EjecucionId, "RubroRoutine");
                    Console.WriteLine("FIN EJECUCIÓN RubroRoutine.");
                    break;
                case 7:
                    Console.WriteLine("SE LANZA EJECUCIÓN SubRubroRoutine.");
                    Ejecutador.Ejecutar("console", Program.EjecucionId, "SubRubroRoutine");
                    Console.WriteLine("FIN EJECUCIÓN SubRubroRoutine.");
                    break;
                case 8:
                    Console.WriteLine("SE LANZA EJECUCIÓN ArticuloGrupoRoutine.");
                    Ejecutador.Ejecutar("console", Program.EjecucionId, "ArticuloGrupoRoutine");
                    Console.WriteLine("FIN EJECUCIÓN ArticuloGrupoRoutine.");
                    break;
                case 9:
                    Console.WriteLine("SE LANZA EJECUCIÓN MarcaRoutine.");
                    Ejecutador.Ejecutar("console", Program.EjecucionId, "MarcaRoutine");
                    Console.WriteLine("FIN EJECUCIÓN MarcaRoutine.");
                    break;
                case 10:
                    Console.WriteLine("SE LANZA EJECUCIÓN PedidoRoutine.");
                    Ejecutador.Ejecutar("console", Program.EjecucionId, "PedidoRoutine");
                    Console.WriteLine("FIN EJECUCIÓN PedidoRoutine.");
                    break;
                default:
                    break;
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Presione una tecla para volver al menu <<");
            Console.Read();

            PrintMainScreen();
        }

        private static string GetNextExecutionTime(string routine)
        {
            if (scheduleInfo == null || scheduleInfo.Schedules == null || scheduleInfo.Schedules.Count == 0)
                return "--:--";

            routine = routine.ToLower();

            int currentTime = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            var schedulesRoutine = scheduleInfo.Schedules
                                                .Where(s => s.Rutina.ToLower().Equals(routine))
                                                .OrderBy(s => GetTimeEnFormatoINT(s.Hora))
                                                .ToList();
            
            var todaySchedulesRoutine = schedulesRoutine
                                                .Where(s => GetTimeEnFormatoINT(s.Hora) >= currentTime)
                                                .ToList();

            var next = todaySchedulesRoutine.FirstOrDefault() ?? schedulesRoutine.FirstOrDefault();

            return next == null ? "--:--" : next.Hora.Substring(0, 5);
        }

        private static int GetTimeEnFormatoINT(string hora)
        {
            string[] dateParts = hora.Split(':');
            int h = Convert.ToInt32(dateParts[0]);
            int m = Convert.ToInt32(dateParts[1]);
            return (h * 60) + m;
        }
    }
}
