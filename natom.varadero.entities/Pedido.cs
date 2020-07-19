using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;

namespace natom.varadero.entities
{
    public class Pedido
    {
        public int PedidoId { get; set; }
    
        public int PuntoDeVenta { get; set; }
        public int Numero { get; set; }

        public DateTime Fecha { get; set; }

        public int SucursalId { get; set; }
        public int ClienteId { get; set; }
        public int? ResponsableId { get; set; }

        public int CondVtaId { get; set; }
        public int ListaPreciosId { get; set; }

        public DateTime? FechaHoraCreacion { get; set; }
        public DateTime? FechaHoraConfirmacion { get; set; }
        public DateTime? FechaHoraConfirmacionEnvioEmail { get; set; }
        public DateTime? FechaHoraAnulacion { get; set; }
        public DateTime? FechaHoraAnulacionEnvioEmail { get; set; }
        public DateTime? FechaHoraInicioSincronizado { get; set; }
        public DateTime? FechaHoraFinSincronizado { get; set; }

        public string EnvioDireccion { get; set; }
        public long? EnvioCodigoPostal { get; set; }
        public string EnvioTelefono { get; set; }

        public string Observaciones { get; set; }

        public decimal PorcentajeIIBB { get; set; }

        public List<PedidoDetalle> Detalle { get; set; }

        public bool AnuladoPorInactividad { get; set; }

        public decimal Total
        {
            get
            {
                return Detalle.Sum(d => d.Cantidad * d.PrecioUnitarioConDescuento);
            }
        }
    }
}
