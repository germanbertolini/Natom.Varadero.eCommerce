var ListaProductos = {
    _domTable: null,
    _domTableTBODY: null,
    _currentFilters: null,
    _currentPage: 1,
    _itemsPerPage: 10,
    _totalPages: 0,

    Init: function (currentPage, itemsPerPage) {
        ListaProductos._domTable = $($(".tblListaProductos")[0]);
        ListaProductos._domTableTBODY = $($(ListaProductos._domTable).find("tbody")[0]);
        if (currentPage !== undefined) ListaProductos._currentPage = currentPage;
        if (itemsPerPage !== undefined) ListaProductos._itemsPerPage = itemsPerPage;
        ListaProductos.Consultar();

        $("#btnAplicarFiltros").click(function () {
            var filtros = ListaProductos.ObtenerFiltrosDelModal();
            ListaProductos.ApplyFilters(filtros);
            $("#modalFiltroListaProducto").modal("hide");
        });

        $("#btnLimpiarFiltros").click(function () {
            $("#modalFiltroListaProducto input[type='text']").val("");
            $("#modalFiltroListaProducto input[type='checkbox']").removeAttr("checked");
            $("#btnAplicarFiltros").click();
        });

        var esCatalogo = $("#EsCatalogo").length > 0 && $("#EsCatalogo").val() == "True";
        if (esCatalogo) {
            $("[hidden-para-catalogo]").hide();
        }

        $('#modalFiltroListaProducto').on('shown.bs.modal', function () {
            setTimeout(function () {
                $($(".txtFiltroField")[0]).focus();
                $(".txtFiltroField").on('keyup', function (e) {
                    if (e.keyCode === 13) {
                        $("#btnAplicarFiltros").click();
                    }
                });
            }, 500);
        });
    },

    MostrarFiltros: function () {
        $("#modalFiltroListaProducto").modal("show");
        $("#txtFiltros").blur();
    },

    PaginaSiguiente: function () {
        if (ListaProductos._currentPage < ListaProductos._totalPages) {
            ListaProductos._currentPage++;
            ListaProductos.Consultar();
        }
    },

    PaginaAnterior: function () {
        if (ListaProductos._currentPage > 1) {
            ListaProductos._currentPage--;
            ListaProductos.Consultar();
        }
    },

    PaginaInicio: function () {
        if (ListaProductos._totalPages > 0 && ListaProductos._currentPage !== 1) {
            ListaProductos._currentPage = 1;
            ListaProductos.Consultar();
        }
    },

    PaginaFin: function () {
        if (ListaProductos._totalPages > 0 && ListaProductos._currentPage !== ListaProductos._totalPages) {
            ListaProductos._currentPage = ListaProductos._totalPages;
            ListaProductos.Consultar();
        }
    },

    ActualizarPaginador: function () {
        $("#txtNumPage").unbind();
        $("#txtNumPage").on('keyup', function (e) {
            if (e.keyCode === 13) {
                var pagina = parseInt($(this).val());
                ListaProductos.IrAPagina(pagina);
            }
        });
        $("#txtNumPage").on('blur', function (e) {
            var pagina = parseInt($(this).val());
            ListaProductos.IrAPagina(pagina);
        });
        $("#txtNumPage").ForceNumericOnly();
        $("#txtNumPage").attr("placeholder", ListaProductos._currentPage + " / " + ListaProductos._totalPages);
    },

    ApplyFilters: function (filters) {
        ListaProductos._currentFilters = filters;
        ListaProductos._currentPage = 1;
        ListaProductos.Consultar();
    },

    IrAPagina: function (pagina) {
        pagina = parseInt(pagina);
        if (isNaN(pagina) || pagina <= 0 || pagina > ListaProductos._totalPages) {
            $("#txtNumPage").val("");
        }
        else {
            ListaProductos._currentPage = pagina;
            ListaProductos.Consultar();
        }
    },

    Consultar: function () {
        var destacados = false;
        if ($("#Destacados").length > 0) {
            destacados = $("#Destacados").val().toLowerCase() == "true";
        }
        var filters = ListaProductos._currentFilters;
        if (filters === null || filters.length === 0) {
            filters = [];
            filters.push("NONE");
            $("#txtFiltros").val("");
        }
        else {
            $("#txtFiltros").val(filters.length + " filtros aplicados");
        }
        eCommerce.DoGET("ListaProductos", "Get", {
            prodfilters: JSON.stringify(filters),
            soloDestacados: destacados,
            itemsPerPage: ListaProductos._itemsPerPage,
            numPage: ListaProductos._currentPage
        }, function (response) {
                if (response == "SESION_FINALIZADA") {
                    location.href = "/eCommerce/Login?error=La%20sesión%20caducó.%20Vuelva%20a%20identificarse%20por%20favor";
                    return;
                }
            $(ListaProductos._domTableTBODY).html(response);
            ListaProductos._totalPages = parseInt(parseInt($("#RowsCount").val()) / ListaProductos._itemsPerPage) + 1;
            ListaProductos.ActualizarPaginador();
            $(".tblListaProductos .cantidad").change(function () {
                var cantidad = parseFloat($(this).val());
                if (isNaN(cantidad) || cantidad < 0) {
                    $(this).val("0");
                }
            });
            $(".tblListaProductos .cantidad").on('keyup', function (e) {
                if (e.keyCode === 13) {
                    var articuloId = $(this).attr("articuloId");
                    $(".tblListaProductos .btnAgregarCarrito[articuloId='" + articuloId + "']").click();
                }
            });
            $(".tblListaProductos .btnAgregarCarrito").click(function () {
                var articuloId = $(this).attr("articuloId");
                var porcentaje = parseFloat($(".porcentaje[articuloId='" + articuloId + "']").text().replace("% ", "").replace(",", "."));
                var porcentajeIVA = parseFloat($(".porcentajeIVA[articuloId='" + articuloId + "']").text().replace("% ", "").replace(",", "."));
                var cantidad = parseInt($(".cantidad[articuloId='" + articuloId + "']").val());
                var precioUnitario = ObtenerDeMoneda($(".precioUnitario[articuloId='" + articuloId + "']").text().replace("¡", "").replace("!", "")).valor;
                var precioUnitarioConDescuento = ObtenerDeMoneda($(".precioUnitarioConDescuento[articuloId='" + articuloId + "']").text().replace("¡", "").replace("!", "")).valor;
                var precioUnitarioConDescuentoNeto = ObtenerDeMoneda($(".precioUnitarioConDescuentoNeto[articuloId='" + articuloId + "']").text().replace("¡", "").replace("!", "")).valor;
                var tienePVP = $(".tienePVP[articuloId='" + articuloId + "']").html() == "true";

                if (cantidad == 0) {
                    Mensajes.MostrarError("La cantidad no puede ser cero.");
                    return;
                }

                Carrito.AgregarItem(articuloId, cantidad, tienePVP, precioUnitario, porcentaje, precioUnitarioConDescuento, porcentajeIVA, precioUnitarioConDescuentoNeto, function (data) {
                    $(".cantidad[articuloId='" + data.articuloId + "']").val("0").blur();
                    Mensajes.MostrarOK("Se agregó correctamente al carrito.");
                    Carrito.ActualizarTotalizador(data);
                });
                
            });

            var esCatalogo = $("#EsCatalogo").length > 0 && $("#EsCatalogo").val() == "True";
            if (esCatalogo) {
                $("[hidden-para-catalogo]").hide();
            }
        }, true);
    },

    ObtenerFiltrosDelModal: function () {
        var filtros = [];

        //PRIMERO LOS INPUT 'TEXT'
        $('.txtFiltroField').filter(function () {
            return !!this.value;
        }).each(function (i, input) {
            var operator = $(input).attr("query-operator").toUpperCase();
            var field = $(input).attr("query-field");
            var searchParams = field + "??:??" + operator + "??:??" + $(input).val();
            filtros.push(searchParams);
            });

        //DESPUES LOS INPUT 'CHECKBOX'
        $('.chkFiltroField').filter(function () {
            var match = false;
            if ($(this).is("[send-only-if-checked]")) {
                return $(this).is(":checked");
            }
            else {
                return true;
            }
        }).each(function (i, input) {
            var operator = $(input).attr("query-operator").toUpperCase();
            var field = $(input).attr("query-field");
            var searchParams = field + "??:??" + operator + "??:??" + ($(input).is(":checked") ? "1" : "0");
            filtros.push(searchParams);
        });
        return filtros;
    }
};