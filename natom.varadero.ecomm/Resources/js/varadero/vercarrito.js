$(document).ready(function () {
    ActualizarTotales();
    
    $(".cantidad").change(function () {
        var pedidoDetalleId = parseInt($(this).attr("pedidoDetalleId"));
        var cantidad = parseFloat($(this).val().replace(",", "."));
        var cantidadOriginal = $(this).attr("original-value");
        if (isNaN(cantidad)) {
            $(this).val($(this).attr("original-value"));
            return;
        }
        if (cantidad <= 0) {
            $(this).val("1").trigger("change");
            return;
        }
        $(this).attr("original-value", cantidad);

        var domCantidad = $(this);
        
        Carrito.UpdateCantidadItem(pedidoDetalleId, cantidad, function (data) {
            ActualizarTotales(data.totalesPedido);
        }, function () {
            $(domCantidad[0]).val(cantidadOriginal);
        });
    });

    $(".btnContinuar").click(asjl);

    $("#observaciones").change(function () {
        eCommerce.DoHiddenPOST("eCommerce", "UpdateObservaciones", { pedidoId: $("#PedidoId").val(), observaciones: $(this).val() });
    });
});

function EliminarItem(pedidoDetalleId) {
    Carrito.EliminarItem(pedidoDetalleId, function (data) {
        ActualizarTotales(data.totalesPedido);
        Carrito.ActualizarTotalizador(data);
        $("tr[pedidoDetalleId='" + pedidoDetalleId + "']").remove();
        if ($(".tblPedido tbody tr").length == 0) {
            $(".btnContinuar").remove();
        }
    });
}

function ActualizarTotales(datosTotales) {
    var callback = function (data) {
        $.each(data.detalle, function (i, item) {
            var domTR = $("tr[pedidoDetalleId='" + item.pedidoDetalleId + "']");
            $(domTR).find(".cantidad").val(item.cantidad).attr("original-value", item.cantidad);
            $(domTR).find(".porcentajeDescuento").html(item.tienePVP && !item.tieneIVADisc ? item.porcentajeDescuento : "");
            //$(domTR).find(".precioUnitario").html("P.U.: " + item.precioUnitario);
            $(domTR).find(".precioUnitarioConDescuento").html("P.U.: " + item.precioUnitarioConDescuento);
            $(domTR).find(".precioUnitarioTotalConDescuento").html(item.precioUnitarioConDescuentoTotal);
            //$(domTR).find(".precioUnitarioTotal").html(item.precioUnitarioTotal);
            $(domTR).find(".precioUnitarioTotal").html(item.tienePVP && !item.tieneIVADisc ? item.precioUnitario : "");
        });
        $(".iva").html(data.iva);
        $(".subtotal").html(data.subtotal);
        $(".total").html(data.total);

        $(".dMontoMinimo").removeAttr("total-por-debajo-minimo");

        if (data.montoMinimo != null) {
            $(".dMontoMinimo").show();
            $(".monto-minimo").html(data.montoMinimo);
            if (data.montoMinimoEnRojo == true) {
                $(".dMontoMinimo").css("color", "red");
                $(".dMontoMinimo").attr("total-por-debajo-minimo", "total-por-debajo-minimo");
            }
            else {
                $(".dMontoMinimo").css("color", "black");
            }
        }
        else {
            $(".dMontoMinimo").hide();
        }
    };
    if (datosTotales === undefined) {
        var pedidoId = $("#PedidoIdCerrado").length == 0 ? parseInt($("#PedidoId").val()) : parseInt($("#PedidoIdCerrado").val());
        Carrito.ObtenerTotalesPedido(pedidoId, function (data) {
            if (data.success) {
                callback(data.totalesPedido);
            }
        });
    }
    else {
        callback(datosTotales);
    }
}

function asjl() {
    Mensajes.MostrarSiNo("¿Desea CONFIRMAR la orden?", function () {
        var callbackConfirmar = function () {
            var entregaEn = $("#entregaEn").val();
            if (entregaEn == "-2") {
                Mensajes.MostrarError("Debe seleccionar el domicilio de Entrega");
                return;
            }
            if (entregaEn == "-1") {
                entregaEn = null;
            }
            eCommerce.DoPOST("eCommerce", "ConfirmarPedido", { clienteDireccionId: entregaEn }, function (response) {
                if (response.success) {
                    location.href = "/eCommerce/PedidoConfirmado?id=" + response.pedidoId;
                }
                else {
                    Mensajes.MostrarError(response.error);
                    eCommerce.OcultarProcesando();
                }
            }, false);
        };

        if ($(".dMontoMinimo").is("[total-por-debajo-minimo]"))
            Mensajes.MostrarSiNo("<b>ATENCIÓN:</b> El subtotal del pedido se encuentra por debajo del monto mínimo. Esto implica que, en caso de continuar con la confirmación, un vendedor estará contactandose con usted para terminar de confirmar el pedido.<br/>¿Desea continuar?", callbackConfirmar);
        else
            callbackConfirmar();
    });
}