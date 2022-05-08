using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace natom.varadero.entities
{
    public class Cliente
    {
        public long EF_Id { get; set; }

        public string Codigo { get; set; }

        public string CUIT { get; set; }
        public string RazonSocial { get; set; }
        public string NombreFantasia { get; set; }
        public string CodigoProvincia { get; set; }

        public int ListaPreciosId { get; set; }

        public int? RegionId { get; set; }

        public decimal? LimiteDeCredito { get; set; }
        public decimal? SaldoEnCtaCte { get; set; }

        public bool Activo { get; set; }

        public List<ClienteDireccion> Direcciones { get; set; }


        public decimal GetSaldo()
        {
            return (this.LimiteDeCredito ?? 0) - (this.SaldoEnCtaCte ?? 0);
        }
    }
}
