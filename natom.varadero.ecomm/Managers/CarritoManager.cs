using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using natom.varadero.entities;
using natom.varadero.ecomm.Exceptions;
using System.Configuration;

namespace natom.varadero.ecomm.Managers
{
    public class CarritoManager
    {
        private DbEcommerceContext db = new DbEcommerceContext();

        public Pedido NuevoPedido(Cliente cliente)
        {
            Pedido pedido = null;
            object lockNuevoPedido = new object();
            lock (lockNuevoPedido)
            {
                pedido = new Pedido();
                pedido.Numero = ((db.Pedidos.Max(p => (int?)p.Numero)) ?? 250000) + 1;
                pedido.ClienteId = cliente.PKClienteId;
                pedido.SucursalId = cliente.PKSucursalId;
                pedido.ResponsableId = cliente.ResponsableId;
                pedido.Fecha = DateTime.Now;
                pedido.FechaHoraCreacion = DateTime.Now;
                pedido.PuntoDeVenta = 9999;
                pedido.CondVtaId = cliente.CondVtaId;
                pedido.ListaPreciosId = cliente.ListaPreciosId;
                pedido.PorcentajeIIBB = cliente.PorcentajeIIBB;
                db.Pedidos.Add(pedido);
                db.SaveChanges();
            }
            return pedido;
        }

        public List<Pedido> ObtenerPedidosPendientesDeConfirmacion()
        {
            return this.db.Pedidos
                            .Include(p => p.Detalle)
                            .Where(c => c.FechaHoraConfirmacion.HasValue && !c.FechaHoraAnulacionEnvioEmail.HasValue)
                            .Take(10)
                            .ToList();
        }

        public void AnularPedidosInactivos()
        {
            int dias = Convert.ToInt32(ConfigurationManager.AppSettings["Varadero.Pedidos.AnulacionAutomatica.ToleranciaDiasInactivo"]);
            DateTime fechaHoraTolerado = DateTime.Now.AddDays(-dias);
            List<Pedido> pedidos = this.db.Pedidos.Where(p => !p.FechaHoraAnulacion.HasValue
                                                                && !p.FechaHoraConfirmacion.HasValue
                                                                && p.FechaHoraCreacion < fechaHoraTolerado).ToList();
            foreach (var inactivo in pedidos)
            {
                this.db.Entry(inactivo).State = EntityState.Modified;
                inactivo.FechaHoraAnulacion = DateTime.Now;
                inactivo.AnuladoPorInactividad = true;
            }
            this.db.SaveChanges();
        }

        public PedidoDetalle AgregarItem(int pedidoId, int articuloId, decimal cantidad, bool tienePVP, decimal? pu, decimal porcDesc, decimal puConDesc, decimal porcIVA, decimal puConDescNeto)
        {
            Articulo articulo = this.db.Articulos.FirstOrDefault(a => a.PKArticuloId == articuloId);   
            if (articulo == null)
            {
                throw new HandledException("No se encontró el artículo");
            }

            Marca articuloMarca = this.db.Marcas.FirstOrDefault(m => m.PKMarcaId.Equals(articulo.MarcaId));
            if (articuloMarca == null)
            {
                throw new HandledException("No se encontró la marca del artículo");
            }

            PedidoDetalle item = this.db.PedidosDetalles.FirstOrDefault(p => p.PedidoId == pedidoId && p.ArticuloId == articuloId);
            if (item == null)
            {
                item = new PedidoDetalle();
                this.db.PedidosDetalles.Add(item);
            }
            else
            {
                this.db.Entry(item).State = System.Data.Entity.EntityState.Modified;

                if (item.PrecioUnitario != pu
                        //|| item.PrecioUnitarioConDescuento != puConDesc
                        || item.PorcentajeDescuento != porcDesc
                        || item.PorcentajeIVA != porcIVA
                        /*|| item.PrecioUnitarioConDescuentoNeto != puConDescNeto*/
                        || item.PrecioUnitarioConDescuentoNeto != puConDesc)
                {
                    throw new HandledException("Incoherencia en los precios");
                }
            }
            item.PedidoId = pedidoId;

            item.ArticuloId = articuloId;
            item.Marca = articuloMarca.PKMarcaId;

            item.ArticuloUnidMin = articulo.UnidMin ?? 1;

            item.TienePVP = tienePVP;
            item.PorcentajeDescuento = porcDesc;

            item.PorcentajeIVA = porcIVA;

            item.CostoUnitario = articulo.PrecioUnitario ?? 0;
            item.NroAlicIVA = articulo.NroAlicIVA.Value;

            //item.PrecioUnitario = pu;
            //item.PrecioUnitarioConDescuento = puConDesc;
            //item.PrecioUnitarioConDescuentoNeto = puConDescNeto;

            item.PrecioUnitario = pu;
            item.PrecioUnitarioConDescuentoNeto = puConDesc;
            item.PrecioUnitarioConDescuento = Math.Round(puConDesc * (1 + (item.PorcentajeIVA / 100)), 2);

            item.Codigo = articulo.ArticuloCodigo;
            item.Nombre = articulo.ArticuloNombre;
            item.ArticuloDescripcionAbreviada = articulo.ArticuloDescripcionAbreviada;

            item.Cantidad += cantidad;

            this.db.SaveChanges();

            return item;
        }

        public PedidoDetalle ObtenerPedidoDetalle(int pedidoDetalleId)
        {
            return this.db.PedidosDetalles.FirstOrDefault(p => p.PedidoDetalleId == pedidoDetalleId);
        }

        public int ObtenerTotalItems(int pedidoId)
        {
            return this.db.PedidosDetalles.Where(p => p.PedidoId == pedidoId).Count();
        }

        public PedidoDetalle UpdateCantidadItem(int pedidoDetalleId, decimal cantidad)
        {
            PedidoDetalle item = null;
            object lockUpdate = new object();
            lock (lockUpdate)
            {
                item = this.db.PedidosDetalles.FirstOrDefault(p => p.PedidoDetalleId == pedidoDetalleId);
                if (item == null)
                {
                    throw new HandledException("No se encontró el item");
                }
                this.db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                item.Cantidad = cantidad;
                this.db.SaveChanges();
            }
            return item;
        }

        public PedidoDetalle EliminarItem(int pedidoDetalleId)
        {
            PedidoDetalle item = this.db.PedidosDetalles.FirstOrDefault(p => p.PedidoDetalleId == pedidoDetalleId);
            if (item == null)
            {
                throw new HandledException("No se encontró el item");
            }
            this.db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
            this.db.SaveChanges();

            return item;
        }

        public object ObtenerTotalesPedido(int pedidoId)
        {
            Pedido pedido = this.ObtenerPedido(pedidoId);
            return new
            {
                detalle = from item in pedido.Detalle select new {
                    pedidoDetalleId = item.PedidoDetalleId,
                    cantidad = item.Cantidad.ToString().Replace(",", "."),
                    tienePVP = item.TienePVP,
                    tieneIVADisc = item.PorcentajeIVA > 0,
                    precioUnitario = item.PrecioUnitario.HasValue ? String.Format("$ {0}", (item.PrecioUnitario.Value.ToString("#,##0.00"))) : null,
                    precioUnitarioTotal = item.PrecioUnitario.HasValue ? String.Format("$ {0}", ((item.PrecioUnitario.Value * item.Cantidad).ToString("#,##0.00"))) : null,
                    //precioUnitarioConDescuento = String.Format("$ {0}", ((item.PrecioUnitarioConDescuento).ToString("#,##0.00"))),
                    //precioUnitarioConDescuentoTotal = String.Format("$ {0}", ((item.PrecioUnitarioConDescuento * item.Cantidad).ToString("#,##0.00"))),
                    precioUnitarioConDescuento = String.Format("$ {0}", ((item.PrecioUnitarioConDescuentoNeto).ToString("#,##0.00"))),
                    precioUnitarioConDescuentoTotal = String.Format("$ {0}", ((item.PrecioUnitarioConDescuentoNeto * item.Cantidad).ToString("#,##0.00"))),
                    porcentajeDescuento = String.Format("% {0}", item.PorcentajeDescuento.ToString().Replace(".", ","))
                },
                subtotal = String.Format("$ {0}", pedido.Detalle.Sum(item => item.PrecioUnitarioConDescuentoNeto * item.Cantidad).ToString("#,##0.00")),
                iva = String.Format("$ {0}", pedido.Detalle.Sum(item => (item.PrecioUnitarioConDescuento - item.PrecioUnitarioConDescuentoNeto) * item.Cantidad).ToString("#,##0.00")),
                total = String.Format("$ {0}", pedido.Detalle.Sum(item => item.PrecioUnitarioConDescuento * item.Cantidad).ToString("#,##0.00"))
            };
        }

        public Pedido ObtenerPedidoAbierto(Cliente cliente)
        {
            return this.db.Pedidos
                            .Include(p => p.Detalle)
                            .FirstOrDefault(c => c.ClienteId == cliente.PKClienteId && c.SucursalId == cliente.PKSucursalId
                                                    && !c.FechaHoraAnulacion.HasValue && !c.FechaHoraConfirmacion.HasValue);
        }

        public Pedido AnularPedido(int pedidoId, Cliente cliente)
        {
            Pedido pedido = this.db.Pedidos.FirstOrDefault(p => p.PedidoId == pedidoId
                                                                && p.ClienteId == cliente.PKClienteId
                                                                && p.SucursalId == cliente.PKSucursalId);
            if (pedido == null)
            {
                throw new HandledException("El pedido no existe");
            }
            if (pedido.FechaHoraAnulacion.HasValue)
            {
                throw new HandledException("El pedido ya se encuentra anulado");
            }
            this.db.Entry(pedido).State = EntityState.Modified;
            pedido.FechaHoraAnulacion = DateTime.Now;
            this.db.SaveChanges();

            return pedido;
        }

        public Pedido ObtenerPedido(int pedidoId)
        {
            return this.db.Pedidos
                            .Include(p => p.Detalle)
                            .FirstOrDefault(c => c.PedidoId == pedidoId);
        }

        public List<Pedido> ObtenerPedidos(Cliente cliente)
        {
            return this.db.Pedidos
                            .Include(p => p.Detalle)
                            .Where(c => c.ClienteId == cliente.PKClienteId && c.SucursalId == cliente.PKSucursalId)
                            .OrderByDescending(c => c.PedidoId)
                            .Take(10)
                            .ToList();
        }

        public Pedido ConfirmarPedidoAbierto(Cliente cliente, decimal? clienteDireccionId)
        {
            ClienteDireccion direccion = clienteDireccionId.HasValue ? this.db.ClientesDirecciones.FirstOrDefault(c => c.ClienteDireccionId == clienteDireccionId.Value) : null;
            Pedido pedido = this.ObtenerPedidoAbierto(cliente);
            if (pedido == null)
            {
                throw new HandledException("No hay pedido abierto");
            }

            if (pedido.Total > cliente.GetSaldo())
            {
                throw new HandledException("El monto del pedido supera el saldo disponible. Por favor, contáctenos.");
            }

            this.db.Entry(pedido).State = EntityState.Modified;
            pedido.FechaHoraConfirmacion = DateTime.Now;
            pedido.EnvioDireccion = direccion?.Direccion;
            pedido.EnvioCodigoPostal = direccion?.CodigoPostal;
            pedido.EnvioTelefono = direccion?.Telefono;
            this.db.SaveChanges();

            return pedido;
        }

        public void UpdateObservaciones(int pedidoId, string observaciones)
        {
            Pedido pedido = this.db.Pedidos.FirstOrDefault(p => p.PedidoId == pedidoId);
            if (pedido != null)
            {
                this.db.Entry(pedido).State = EntityState.Modified;
                pedido.Observaciones = observaciones;
                if (pedido.Observaciones.Length > 200) {
                    pedido.Observaciones = pedido.Observaciones.Substring(0, 200);
                }
                this.db.SaveChanges();
            }
        }
    }
}