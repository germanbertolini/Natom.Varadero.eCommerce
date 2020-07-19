var eCommerce = {
    DoGET: function (controller, action, data, callback, hideProcessing) {
        eCommerce.MostrarProcesando();
        $.get(
            "/" + controller + "/" + action,
            data,
            function (data, status) {
                if (status == "success") {
                    callback(data);
                }
                else {
                    alert("Se ha producido un error. Comuníquese con el administrador.");
                }
                if (hideProcessing == undefined || hideProcessing == true) {
                    eCommerce.OcultarProcesando();
                }
            }
        );
    },
    DoPOST: function (controller, action, data, callback, hideProcessing) {
        var token = "";
        eCommerce.MostrarProcesando();
        $.ajax({
            url: "/" + controller + "/" + action,
            type: "POST",
            data: JSON.stringify(data),
            dataType: "json",
            contentType: "application/json",
            success: function (data, status) {
                if (status == "success") {
                    callback(data);
                }
                else {
                    alert("Se ha producido un error. Comuníquese con el administrador.");
                }
                if (hideProcessing == undefined || hideProcessing == true) {
                    eCommerce.OcultarProcesando();
                }
            }
        });
    },
    DoHiddenPOST: function (controller, action, data, callback) {
        var token = "";
        $.ajax({
            url: "/" + controller + "/" + action,
            type: "POST",
            data: JSON.stringify(data),
            dataType: "json",
            contentType: "application/json",
            success: function (data, status) {
                if (status == "success") {
                    if (callback !== undefined) {
                        callback(data);
                    }
                }
                else {
                    alert("Se ha producido un error. Comuníquese con el administrador.");
                }
            }
        });
    },
    MostrarProcesando: function (texto, dom) {
        $("#loading").fadeIn(200);
    },
    OcultarProcesando: function (dom) {
        $("#loading").fadeOut(200);
    },
    MostrarModalView: function (controller, action, params, onShownCallback) {

        var _onShownCallback = onShownCallback;

        eCommerce.DoGET(controller, action, params, function (html) {
            $("#modalView").html(html);
            $("#modalView").modal("show");

            $('#modalView [data-toggle="tooltip"]').tooltip();
            $('#modalView .chosen-select').chosen({ width: '100%' });

            if (_onShownCallback != undefined) {
                _onShownCallback();
            }
        });
    },
    OcultarModalView: function () {
        $("#modalView").modal("hide");
        $.noty.closeAll();
    },
};

var Mensajes = {
    MostrarError: function (mensaje) {
        noty({
            layout: 'center',
            text: mensaje,
            closeWith: [],
            modal: true,
            buttons: [
                {
                    addClass: 'btn btn-danger', text: 'OK', onClick: function ($noty) {
                        $noty.close();
                    }
                }
            ]
        });
        $("#noty_center_layout_container").css("top", "200px");
        $(window).resize(function () { $("#noty_center_layout_container").css("top", "200px"); });
    },
    MostrarOK: function (mensaje) {
        noty({
            layout: 'center',
            text: mensaje,
            closeWith: [],
            modal: true,
            buttons: [
                {
                    addClass: 'btn btn-success', text: 'OK', onClick: function ($noty) {
                        $noty.close();
                    }
                }
            ]
        });
        $("#noty_center_layout_container").css("top", "200px");
        $(window).resize(function () { $("#noty_center_layout_container").css("top", "200px"); });
    },
    MostrarSiNo: function (mensaje, callbackSI, callbackNO) {
        noty({
            layout: 'center',
            text: mensaje,
            closeWith: [],
            modal: true,
            buttons: [
                {
                    addClass: 'btn btn-success', text: 'SI', onClick: function ($noty) {
                        if (callbackSI != undefined) {
                            callbackSI();
                        }
                        $noty.close();
                    }
                },
                {
                    addClass: 'btn btn-danger', text: 'NO', onClick: function ($noty) {
                        if (callbackNO != undefined) {
                            callbackNO();
                        }
                        $noty.close();
                    }
                }
            ]
        });
        $("#noty_center_layout_container").css("top", "200px");
        $(window).resize(function () { $("#noty_center_layout_container").css("top", "200px"); });
    },
    MostrarNotificacionIzquierda: function (mensaje, type) {
        noty({
            layout: 'topLeft',
            text: mensaje,
            closeWith: ['click'],
            modal: false,
            type: type
        });
    },
    MostrarNotificacionDerecha: function (mensaje, type) {
        noty({
            layout: 'topRight',
            text: mensaje,
            closeWith: ['click'],
            modal: false,
            type: type
        });
    }
};

function Redondear(v) {
    return parseFloat((v).toFixed(2));
}

function FormatearAMoneda(valor, moneda) {
    if (moneda == undefined) {
        moneda = "$";
    }
    return moneda + " " + valor.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&~').replace(".", ",").replace(new RegExp('~', 'g'), '.');
}

function ObtenerDeMoneda(valor) {
    var data = valor.split(" ");
    var moneda = undefined;
    valor = 0;

    if (data.length == 2) {
        moneda = data[0];
        valor = data[1].replace(/\./g, '').replace(",", ".");
    }
    else {
        valor = data[0].replace(/\./g, '').replace(",", ".");
    }

    return {
        moneda: moneda,
        valor: parseFloat(valor)
    };
}

function GetPlainText(text) {
    return text.replace(/<\/?[^>]+(>|$)/g, "");
}

jQuery.fn.ForceNumericOnly =
    function () {
        return this.each(function () {
            $(this).keydown(function (e) {
                var key = e.charCode || e.keyCode || 0;
                // allow backspace, tab, delete, arrows, numbers and keypad numbers ONLY
                return (
                    key == 8 ||
                    key == 9 ||
                    key == 46 ||
                    (key >= 37 && key <= 40) ||
                    (key >= 48 && key <= 57) ||
                    (key >= 96 && key <= 105));
            })
        })
    };