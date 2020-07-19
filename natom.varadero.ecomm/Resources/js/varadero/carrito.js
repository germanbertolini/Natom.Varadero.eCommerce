var Carrito = {
    VerAbierto: function () {
        location.href = "/eCommerce/VerPedidoAbierto";
    },
    Ver: function (id) {
        location.href = "/eCommerce/VerPedido?id=" + id;
    },
    ActualizarTotalizador: function (data) {
        var cantidad = 0;
        if (data === undefined) {
            cantidad = parseInt($("#PedidoItemsCount").val());
        }
        else {
            cantidad = data.pedidoItemsCount;
        }
        if (cantidad === undefined) {
            cantidad = 0;
        }
        if (cantidad === 0) {
            $(".itemsCounter").text("").hide();
        }
        else {
            $(".itemsCounter").text(cantidad).show();
        }
    },
    NuevaOrden: function () {
        eCommerce.DoPOST("eCommerce", "NuevaOrden", {}, function (response) {
            if (response.success) {
                location.href = "/eCommerce/Gondola";
            }
            else {
                Mensajes.MostrarError(response.error);
            }
        }, false);
    }, 
    CancelarOrden: function (pedidoId) {
        var _pedidoId = pedidoId;
        Mensajes.MostrarSiNo("¿Desea CANCELAR la orden?", function () { 
            eCommerce.DoPOST("eCommerce", "AnularPedido", { pedidoId: _pedidoId }, function (response) {
                if (response.success) {
                    location.href = "/eCommerce/Principal";
                }
                else {
                    Mensajes.MostrarError(response.error);
                }
            }, false);
        });
    },
    EliminarItem: function (pedidoDetalleId, callbackSuccess) {
        var _pedidoDetalleId = pedidoDetalleId;
        var _callbackSuccess = callbackSuccess;
        Mensajes.MostrarSiNo("¿Desea ELIMINAR el item?", function () {
            eCommerce.DoPOST("eCommerce", "EliminarItem", { pedidoDetalleId: pedidoDetalleId }, function (response) {
                if (response.success) {
                    _callbackSuccess(response);
                }
                else {
                    Mensajes.MostrarError(response.error);
                }
            }, true);
        });
    },
    UpdateCantidadItem: function (pedidoDetalleId, cantidad, callbackSuccess, callbackError) {
        eCommerce.DoPOST("eCommerce", "UpdateCantidadItem", { pedidoDetalleId: pedidoDetalleId, cantidad: cantidad }, function (response) {
            if (response.success) {
                callbackSuccess(response);
            }
            else {
                Mensajes.MostrarError(response.error);
                if (callbackError !== undefined) {
                    callbackError(response);
                }
            }
        }, true);
    },
    AgregarItem: function (articuloId, cantidad, tienePVP, pu, porcDesc, puConDesc, porcIVA, puConDescNeto, callbackSuccess) {
        eCommerce.DoPOST("eCommerce", "AgregarItem", { articuloId: articuloId, cantidad: cantidad, tienePVP: tienePVP, pu: pu, porcDesc: porcDesc, puConDesc: puConDesc, porcIVA: porcIVA, puConDescNeto: puConDescNeto }, function (response) {
            if (response.success) {
                if (callbackSuccess !== undefined) {
                    callbackSuccess(response);
                }
            }
            else {
                Mensajes.MostrarError(response.error);
            }
        }, true);
    },
    ObtenerTotalesPedido: function (pedidoId, callbackSuccess) {
        eCommerce.DoPOST("eCommerce", "ObtenerTotalesPedido", { pedidoId: pedidoId }, function (response) {
            if (response.success) {
                if (callbackSuccess !== undefined) {
                    callbackSuccess(response);
                }
            }
            else {
                Mensajes.MostrarError(response.error);
            }
        }, true);
    }
};