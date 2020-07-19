using natom.ecomm.sync.kernel;
using natom.ecomm.sync.kernel.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace natom.ecomm.sync.background
{
    public partial class Form1 : Form
    {
        public int Minutos = 0;
        public SyncScheduleInfo scheduleInfo = new SyncScheduleInfo();

        public Form1()
        {
            InitializeComponent();
        }

        private void timerMinutero_Tick(object sender, EventArgs e)
        {
            Minutos++;
            if (Minutos == 60)
                Minutos = 0;
            DoTasks();
        }

        private void DoTasks(bool isLoad = false)
        {
            try
            {
                if (Minutos % 10 == 0) GetSchedules();
                if (Minutos % 60 == 0) LogManager.DeleteOldLogs("background", Program.EjecucionId);
                ExecuteRoutines();
                LogManager.SyncToHosting("background", Program.EjecucionId);
            }
            catch (Exception ex)
            {
                LogManager.LogException("background", Program.EjecucionId, "Form1.DoTasks", ex);
            }
        }

        private void ExecuteRoutines()
        {
            //SI NO TENGO CRONOGRAMA ESTABLECIDO, NO HAGO NADA
            if (scheduleInfo == null || scheduleInfo.Schedules == null || scheduleInfo.Schedules.Count == 0)
                return;

            //OBTENGO EL CURRENTTIME Y LAS RUTINAS A EJECUTAR EN ESTE MOMENTO
            string currentTime = GetCurrentTime();
            var routineToExecute = scheduleInfo.Schedules
                                                    .Where(s => s.Hora.Equals(currentTime))
                                                    .Select(s => s.Rutina)
                                                    .ToList();
            
            if (routineToExecute.Count > 0)
            {
                //AVISO SI NO TENGO RUTINAS
                LogManager.LogInfo("background", Program.EjecucionId, "Form1.ExecuteRoutines", "SE HA ENCONTRADO " + routineToExecute.Count + " RUTINAS A EJECUTAR PARA LA HORA " + currentTime + ".");

                //LAS EJECUTO
                foreach (var routine in routineToExecute)
                {
                    LogManager.LogInfo("background", Program.EjecucionId, "Form1.ExecuteRoutines", "SE LANZA EJECUTADOR PARA LA RUTINA " + routine + ".");
                    Ejecutador.Ejecutar("background", Program.EjecucionId, routine);
                }
            }
        }

        private string GetCurrentTime()
        {
            int currentHour = DateTime.Now.Hour;
            int currentMinute = DateTime.Now.Minute;
            string currentTime = currentHour.ToString().PadLeft(2, '0') + ":" + currentMinute.ToString().PadLeft(2, '0') + ":00";
            return currentTime;
        }

        private void GetSchedules()
        {
            LogManager.LogInfo("background", Program.EjecucionId, "Form1.GetSchedules", "INICIO PROCESO GetSchedules");

            try
            {
                LogManager.LogInfo("background", Program.EjecucionId, "Form1.GetSchedules", "CONECTANDO AL HOSTING PARA OBTENER EL CRONOGRAMA...", new { serviceUrl = "/SyncSchedule/Get?aplicativo=background&ejecucionId=" + Program.EjecucionId });

                var task = ServiceAccess.DoGet<SyncScheduleInfo>("/SyncSchedule/Get?aplicativo=background&ejecucionId=" + Program.EjecucionId);
                Task.WaitAll(task);

                LogManager.LogInfo("background", Program.EjecucionId, "Form1.GetSchedules", "SE OBTUVIERON DATOS. PROCESANDO DATOS...", new { Response = task.Result });

                if (!task.Result.Success)
                {
                    LogManager.LogInfo("background", Program.EjecucionId, "Form1.GetSchedules", "EL SERVIDOR HA DEVUELTO UN ERROR: " + task.Result.ErrorMessage);
                }
                else
                {
                    scheduleInfo = task.Result.Data;
                    Ejecutador.CancellationTokenMS = scheduleInfo.CancellationTokenMS;

                    LogManager.LogInfo("background", Program.EjecucionId, "Form1.GetSchedules", "CRONOGRAMA PROCESADO CORRECTAMENTE");

                    string settingsPath = Environment.CurrentDirectory + "\\sync.settings.json";
                    if (File.Exists(settingsPath))
                    {
                        File.Delete(settingsPath);
                        LogManager.LogInfo("background", Program.EjecucionId, "Form1.GetSchedules", "SE ELIMINA ARCHIVO EXISTENTE sync.settings.json");
                    }

                    LogManager.LogInfo("background", Program.EjecucionId, "Form1.GetSchedules", "COMIENZO CREACIÓN ARCHIVO sync.settings.json");
                    File.WriteAllText(settingsPath, JsonConvert.SerializeObject(scheduleInfo));
                    LogManager.LogInfo("background", Program.EjecucionId, "Form1.GetSchedules", "FIN CREACIÓN ARCHIVO sync.settings.json");
                }
            }
            catch (Exception ex)
            {
                LogManager.LogException("background", Program.EjecucionId, "Form1.GetSchedules", ex);
            }
            LogManager.LogInfo("background", Program.EjecucionId, "Form1.GetSchedules", "FIN PROCESO GetSchedules");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string thisProgramPath = Environment.CurrentDirectory + "\\natom.ecomm.sync.background.exe";
            if (Ejecutador.ProgramRunningCount(thisProgramPath) > 1)
            {
                MessageBox.Show("Ya hay una instancia de 'natom.ecomm.sync.background.exe' en ejecución.");
                Application.Exit();
                return;
            }
            DoTasks(isLoad: true);
        }
    }
}
