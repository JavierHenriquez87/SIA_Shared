CargarUniversoAuditable();
CargarProgramadaNoProgramada();

//FUNCION PARA GUARDAR AUDITORIA INTEGRAL
//==========================================================================================

async function GuardarAuditoriaIntegral() {
    // Objeto que contiene los datos a enviar
    var DataAI = {
        CODIGO_UNIVERSO_AUDITABLE: $('#universoAuditable').val(),
        CODIGO_TIPO_AUDITORIA: $('#auditoriaProgramada').val(),
        SOLICITADA_POR: $('#solicitadaPor').val(),
        FECHA_INICIO_VISITA: $('#fechaInicioVisita').val(),
        FECHA_FIN_VISITA: $('#fechaFinVisita').val(),
        PERIODO_INICIO_REVISION: $('#periodoInicioRevision').val(),
        PERIODO_FIN_REVISION: $('#periodoFinRevision').val()
    };

    var val = this.ValidacionGuardarAudInteg();

    if (val == true) {
        Swal.fire({
            title: 'Datos Incompletos',
            text: "Complete todos los campos requeridos (marcados con *)",
            icon: 'warning',
            showCancelButton: false,
            confirmButtonColor: '#2A3042',
            confirmButtonText: 'Ok'
        })

    } else if (val == "errorF1") {
        Swal.fire({
            title: 'Error',
            text: "La fecha de Finalización de visita no debe ser menor a la Fecha de Inicio",
            icon: 'warning',
            showCancelButton: false,
            confirmButtonColor: '#2A3042',
            confirmButtonText: 'Ok'
        })
    } else if (val == "errorF2") {
        Swal.fire({
            title: 'Error',
            text: "La fecha de Finalización de revisión no debe ser menor a la Fecha de Inicio del periodo",
            icon: 'warning',
            showCancelButton: false,
            confirmButtonColor: '#2A3042',
            confirmButtonText: 'Ok'
        })
    } else if (val == "errorF3") {
        Swal.fire({
            title: 'Error',
            text: "La fecha de Inicio del periodo no puede ser menor al dia actual",
            icon: 'warning',
            showCancelButton: false,
            confirmButtonColor: '#2A3042',
            confirmButtonText: 'Ok'
        })
    } else {
        Swal.showLoading();

        $.ajax({
            method: 'POST',
            url: 'Guardar_Auditoria_Integral',
            data: JSON.stringify(DataAI),
            contentType: 'application/json',
            success: function (respuesta) {
                if (respuesta == "error") {
                    Swal.fire(
                        'Error!',
                        'Su registro no se pudo guardar.',
                        'error'
                    )
                } else {
                    Swal.fire({
                        title: 'Guardado!',
                        text: 'Registro guardado con exito.',
                        icon: 'success',
                        didClose: () => {
                            window.location.href = 'ProgramarAuditoria/AuditoriaIndividual?first=true';
                        }
                    });
                }
            },
            error: function () {
                // Mostrar un mensaje de error al usuario si ocurre un error en la solicitud AJAX
                Swal.fire(
                    'Error!',
                    'Hubo un problema al procesar su solicitud.',
                    'error'
                );
            }
        });
    }
}

function ValidacionGuardarAudInteg() {
    var errorValidacion = false;

    var universoAuditable = $('#universoAuditable').val();
    var auditoriaProgramada = $('#auditoriaProgramada').val();
    var fechaInicioVisita = $('#fechaInicioVisita').val();
    var fechaFinVisita = $('#fechaFinVisita').val();
    var periodoInicioRevision = $('#periodoInicioRevision').val();
    var periodoFinRevision = $('#periodoFinRevision').val();

    if (universoAuditable.length == 0) {
        errorValidacion = true;
    }

    if (auditoriaProgramada.length == 0) {
        errorValidacion = true;
    } else {
        if (auditoriaProgramada == 2) {
            var solicitadaPor = $('#solicitadaPor').val();
            if (solicitadaPor.length == 0) {
                errorValidacion = true;
            }
        }
    }

    if (fechaInicioVisita.length == 0) {
        errorValidacion = true;
    }

    if (fechaFinVisita.length == 0) {
        errorValidacion = true;
    }

    if (periodoInicioRevision.length == 0) {
        errorValidacion = true;
    }

    if (periodoFinRevision.length == 0) {
        errorValidacion = true;
    }

    if (errorValidacion == false) {
        // Validar que la fecha de fin no sea menor que la de inicio
        if (new Date(fechaFinVisita) < new Date()) {
            return "errorF3";
        }

        // Validar que la fecha de fin no sea menor que la de inicio
        if (new Date(fechaFinVisita) < new Date(fechaInicioVisita)) {
            return "errorF1";
        }

        // Validar que la fecha de fin no sea menor que la de inicio
        if (new Date(periodoFinRevision) < new Date(periodoInicioRevision)) {
            return "errorF2";
        }
    }

    return errorValidacion;
}

function mostrarSolicitadaPor(){
    var auditoriaProgramada = $('#auditoriaProgramada').val();

    if (auditoriaProgramada == 1) {
        $('#solicPor').hide();
        $('#solicitadaPor').val("");
    } else if (auditoriaProgramada == 2) {
        $('#solicPor').show();
    } else {
        $('#solicPor').hide();
        $('#solicitadaPor').val("");
    }
}
