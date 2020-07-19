using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;

namespace natom.varadero.ecomm.Managers
{
    public static class BackgroundManager
    {
        private static System.Timers.Timer timer = null;

        public static void Init()
        {
            if (timer == null)
            {
                timer = new System.Timers.Timer();
                timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                timer.Interval = 60000;
                timer.Enabled = true;
            }
            Rutina();
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Rutina();
        }

        private static void Rutina()
        {
            //EnviarMailsConfirmacionPedido();
            AnularPedidosInactivos();
        }

        private static void EnviarMailsConfirmacionPedido()
        {
            var carritoMgr = new CarritoManager();
            List<Pedido> pedidosPendientes = carritoMgr.ObtenerPedidosPendientesDeConfirmacion();
            foreach (var pedido in pedidosPendientes)
            {

            }
        }

        private static void AnularPedidosInactivos()
        {
            var carritoMgr = new CarritoManager();
            carritoMgr.AnularPedidosInactivos();
        }
    }
}