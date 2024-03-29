﻿using System;
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

        public Pedido NuevoPedido(Usuario usuario)
        {
            Pedido pedido = null;
            Cliente cliente = new ClienteManager().ObtenerClientePorCUIT(usuario.ClienteCUIT);
            object lockNuevoPedido = new object();
            lock (lockNuevoPedido)
            {
                pedido = new Pedido();
                pedido.Numero = ((db.Pedidos.Max(p => (int?)p.Numero)) ?? 250000) + 1;
                pedido.ClienteCodigo = cliente.Codigo;
                pedido.Cotizacion = CotizacionHelper.ObtenerCotizacion();
                pedido.Moneda = 1;
                pedido.Fecha = DateTime.Now;
                pedido.FechaHoraCreacion = DateTime.Now;
                pedido.PuntoDeVenta = 9999;
                pedido.ListaPreciosId = cliente.ListaPreciosId;
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

        public void MarcarComoPreparado(int pedidoId)
        {
            Pedido pedido = this.db.Pedidos.Find(pedidoId);
            this.db.Entry(pedido).State = EntityState.Modified;
            pedido.FechaHoraPreparado = DateTime.Now;
            this.db.SaveChanges();
        }

        public void MarcarComoCompletado(int pedidoId)
        {
            Pedido pedido = this.db.Pedidos.Find(pedidoId);
            this.db.Entry(pedido).State = EntityState.Modified;
            pedido.FechaHoraCompletado = DateTime.Now;
            this.db.SaveChanges();
        }

        public Cliente ObtenerClientePorCodigo(string codigoCliente)
        {
            return db.Clientes.First(c => c.Codigo.Equals(codigoCliente));
        }

        public PedidoDetalle AgregarItem(int pedidoId, string articuloCodigo, decimal cantidad, bool tienePVP, decimal? pu, decimal porcDesc, decimal puConDesc, decimal porcIVA, decimal puConDescNeto)
        {
            Articulo articulo = this.db.Articulos.FirstOrDefault(a => a.ArticuloCodigo == articuloCodigo);   
            if (articulo == null)
            {
                throw new HandledException("No se encontró el artículo");
            }

            PedidoDetalle item = this.db.PedidosDetalles.FirstOrDefault(p => p.PedidoId == pedidoId && p.Codigo == articuloCodigo);
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

            item.Codigo = articuloCodigo;
            item.Marca = articulo.Marca;


            item.TienePVP = tienePVP;
            item.PorcentajeDescuento = porcDesc;

            item.PorcentajeIVA = porcIVA;

            item.CostoUnitario = articulo.PrecioUnitario ?? 0;

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

        public IEnumerable<Pedido> GetPedidos()
        {
            IEnumerable<Pedido> query = this.db.Pedidos
                                                    .Include(p => p.Detalle)
                                                    .Where(p => !p.AnuladoPorInactividad
                                                                    && p.FechaHoraConfirmacion.HasValue);

            return query;
        }

        public int GetPedidosCount()
        {
            return this.db.Pedidos.Where(p => !p.AnuladoPorInactividad
                                                    && p.FechaHoraConfirmacion.HasValue).Count();
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
            Cliente cliente = new CarritoManager().ObtenerClientePorCodigo(pedido.ClienteCodigo);

            //OBTENEMOS MONTO MINIMO PARA EL PEDIDO
            decimal? montoMinimo = 0;
            if (cliente.RegionId != null)
            {
                var regionMontoMinimo = new RegionMontosMinimosManager().ObtenerMontoMinimoParaHoy(cliente.RegionId.Value);
                if (regionMontoMinimo != null)
                    montoMinimo = regionMontoMinimo.MontoMinimo;
            }

            //DEVOLVEMOS LA DATA
            return new
            {
                detalle = from item in pedido.Detalle select new {
                    pedidoDetalleId = item.PedidoDetalleId,
                    cantidad = item.Cantidad.ToString().Replace(",", "."),
                    tienePVP = item.TienePVP,
                    tieneIVADisc = item.PorcentajeIVA > 0,
                    precioUnitario = item.PrecioUnitario.HasValue ? String.Format("$ {0}", (item.PrecioUnitario.Value.ToString("F").Replace(".", ","))) : null,
                    precioUnitarioTotal = item.PrecioUnitario.HasValue ? String.Format("$ {0}", ((item.PrecioUnitario.Value * item.Cantidad).ToString("F").Replace(".", ","))) : null,
                    //precioUnitarioConDescuento = String.Format("$ {0}", ((item.PrecioUnitarioConDescuento).ToString("F").Replace(".", ","))),
                    //precioUnitarioConDescuentoTotal = String.Format("$ {0}", ((item.PrecioUnitarioConDescuento * item.Cantidad).ToString("F").Replace(".", ","))),
                    precioUnitarioConDescuento = String.Format("$ {0}", ((item.PrecioUnitarioConDescuentoNeto).ToString("F").Replace(".", ","))),
                    precioUnitarioConDescuentoTotal = String.Format("$ {0}", ((item.PrecioUnitarioConDescuentoNeto * item.Cantidad).ToString("F").Replace(".", ","))),
                    porcentajeDescuento = String.Format("% {0}", item.PorcentajeDescuento.ToString().Replace(".", ","))
                },
                subtotal = String.Format("$ {0}", pedido.Detalle.Sum(item => item.PrecioUnitarioConDescuentoNeto * item.Cantidad).ToString("F").Replace(".", ",")),
                iva = String.Format("$ {0}", pedido.Detalle.Sum(item => (item.PrecioUnitarioConDescuento - item.PrecioUnitarioConDescuentoNeto) * item.Cantidad).ToString("F").Replace(".", ",")),
                total = String.Format("$ {0}", pedido.Detalle.Sum(item => item.PrecioUnitarioConDescuento * item.Cantidad).ToString("F").Replace(".", ",")),
                montoMinimo = montoMinimo != null && montoMinimo != 0 ? String.Format("$ {0}", montoMinimo.Value.ToString("F").Replace(".", ",")) : null,
                montoMinimoEnRojo = pedido.Detalle.Sum(item => item.PrecioUnitarioConDescuentoNeto * item.Cantidad) < montoMinimo
            };
        }

        public Pedido ObtenerPedidoAbierto(Cliente cliente)
        {
            return this.db.Pedidos
                            .Include(p => p.Detalle)
                            .FirstOrDefault(c => c.ClienteCodigo == cliente.Codigo
                                                    && !c.FechaHoraAnulacion.HasValue && !c.FechaHoraConfirmacion.HasValue);
        }

        public Pedido AnularPedido(int pedidoId, Cliente cliente)
        {
            Pedido pedido = this.db.Pedidos.FirstOrDefault(p => p.PedidoId == pedidoId
                                                                && p.ClienteCodigo == cliente.Codigo);
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
                            .Where(c => c.ClienteCodigo == cliente.Codigo)
                            .OrderByDescending(c => c.PedidoId)
                            .Take(10)
                            .ToList();
        }

        public Pedido ConfirmarPedidoAbierto(Cliente cliente, long? clienteDireccionId)
        {
            ClienteDireccion direccion = clienteDireccionId.HasValue ? this.db.ClientesDirecciones.FirstOrDefault(c => c.EF_Id == clienteDireccionId) : null;
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

        public static string ObtenerEstadoPedido(Pedido pedido, bool esUsuario = false)
        {
            var limite = Convert.ToDateTime(ConfigurationManager.AppSettings["Dashboard.Ordenes.CircuitoAPartirDe"]);

            if (pedido.FechaHoraAnulacion.HasValue)
                return "ANULADO";

            else if (pedido.FechaHoraConfirmacion.HasValue && pedido.FechaHoraConfirmacion.Value < limite)
                return "COMPLETADO"; //COMPATIBILIDAD PEDIDOS ANTERIORES A ESTE FEATURE

            else if (!pedido.FechaHoraFinSincronizado.HasValue)
                return esUsuario ? "CONFIRMADO *" : "PEND. SINCRONIZACIÓN";
            else if (!pedido.FechaHoraPreparado.HasValue)
                return "CONFIRMADO";
            else if (!pedido.FechaHoraCompletado.HasValue)
                return "PREPARADO";
            else
                return "COMPLETADO";
        }
    }
}