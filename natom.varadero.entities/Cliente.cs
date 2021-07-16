using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace natom.varadero.entities
{
    public class Cliente
    {
        public long EF_Id { get; set; }
        public decimal ClienteId { get; set; }
    
	    public int PKSucursalId { get; set; }
        public int PKClienteId { get; set; }

        public double? CUIT { get; set; }
        public string RazonSocial { get; set; }
        public string NombreFantasia { get; set; }
        public string CodigoProvincia { get; set; }
        public string Moneda { get; set; }

        public string UsuarioAlias { get; set; }
        public string UsuarioClave { get; set; }
        public string UsuarioEmail { get; set; }

        public int CondVtaId { get; set; }
        public int ListaPreciosId { get; set; }

        public int? RegionId { get; set; }
        public Region Region { get; set; }

        public decimal? LimiteDeCredito { get; set; }
        public decimal? SaldoEnCtaCte { get; set; }

        public decimal PorcentajeIIBB { get; set; }

        public int? ResponsableId { get; set; }

        public string SesionIP { get; set; }
        public string SesionAgent { get; set; }
        public string SesionToken { get; set; }
        public DateTime? SesionInicio { get; set; }
        public DateTime? SesionUltimaAccion { get; set; }


        public decimal GetSaldo()
        {
            return (this.LimiteDeCredito ?? 0) - (this.SaldoEnCtaCte ?? 0);
        }
    }
}
