using natom.varadero.ecomm.Models;
using natom.varadero.entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace natom.varadero.ecomm
{
    public class DbEcommerceContext : DbContext
    {
        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<ArticuloDestacado> ArticulosDestacados { get; set; }
        public DbSet<ArticuloGrupo> ArticulosGrupos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ClienteDireccion> ClientesDirecciones { get; set; }
        public DbSet<CondicionDeVenta> CondicionesDeVenta { get; set; }
        public DbSet<ListaPrecios> ListasDePrecios { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidosDetalles { get; set; }
        public DbSet<Rubro> Rubros { get; set; }
        public DbSet<SubRubro> SubRubros { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<SyncSchedule> SyncSchedules { get; set; }
        public DbSet<SyncSesion> SyncSesions { get; set; }
        public DbSet<eCommerceStatus> eCommerceStatus { get; set; }

        public DbEcommerceContext()
            : base("name=DbEcommerceContext")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<Articulo>().HasKey(i => i.EF_Id);
            modelBuilder.Entity<ArticuloDestacado>().HasKey(i => i.PKArticuloId);
            modelBuilder.Entity<ArticuloGrupo>().HasKey(i => i.EF_Id);
            modelBuilder.Entity<Cliente>().HasKey(i => i.EF_Id);
            modelBuilder.Entity<ClienteDireccion>().HasKey(i => i.EF_Id);
            modelBuilder.Entity<CondicionDeVenta>().HasKey(i => i.EF_Id);
            modelBuilder.Entity<ListaPrecios>().HasKey(i => i.EF_Id);
            modelBuilder.Entity<Marca>().HasKey(i => i.EF_Id);
            modelBuilder.Entity<Pedido>().HasKey(i => i.PedidoId);
            modelBuilder.Entity<PedidoDetalle>().HasKey(i => i.PedidoDetalleId);
            modelBuilder.Entity<Rubro>().HasKey(i => i.EF_Id);
            modelBuilder.Entity<SubRubro>().HasKey(i => i.EF_Id);
            modelBuilder.Entity<Log>().HasKey(i => i.Id);
            modelBuilder.Entity<SyncSchedule>().HasKey(i => i.SyncScheduleId);
            modelBuilder.Entity<SyncSesion>().HasKey(i => i.EjecucionId);
            modelBuilder.Entity<eCommerceStatus>().HasKey(i => i.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}