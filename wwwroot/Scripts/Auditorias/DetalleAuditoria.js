// Función que será llamada al hacer clic en un documento
//==========================================================================================
function validarMemoExiste() {
    var num_auditoria_integral = $('#codAI_DA').val();
    var anio_auditoria_integral = $('#anioAI_DA').val();
    $.ajax({
        method: 'POST',
        url: '/Auditorias/validarMemorandumExiste',
        data: {
            num_audit_integral: num_auditoria_integral,
            anio_audit_integral: anio_auditoria_integral
        },
        dataType: 'json',
        success: function (respuesta) {
            if (respuesta == true) {
                window.location.href = '/Auditorias/DetalleAuditoria/DetalleDocAuditoria';
            } else {
                Swal.fire({
                    title: 'Aún no se ha creado un Memorándum de Planificación',
                    text: 'Es necesario crear el Memorándum de Planificación para habilitar las opciones de este apartado.',
                    icon: 'warning',
                    showCancelButton: true,
                    reverseButtons: true,
                    cancelButtonText: 'Cancelar',
                    confirmButtonText: 'Crear Memorándum',
                    buttonsStyling: true,
                    customClass: {
                        confirmButton: 'btn btn-primary',
                        cancelButton: 'btn btn-secondary'
                    }
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = '/Auditorias/ProgramarAuditoria/AuditoriaIndividual/PlanificacionAuditoria';
                    } else if (result.isDismissed) {

                    }
                });
            }
        }
    });


}

// Función que será llamada al hacer clic en un folder
//==========================================================================================
function handleFolderClick(doc) {
    document.getElementById('buttonAdd').style.display = 'none';
    document.getElementById('divButtons').style.display = 'none';

    const documents = [
        { type: 'doc', title: 'MEMORANDUM DE PLANIFICACION', status: null },
        { type: 'folder', title: 'PROGRAMAS DE TRABAJO', status: null },
        { type: 'folder', title: 'CUESTIONARIOS DE TRABAJO', status: null }
    ];

    Documents(documents);
}


// Función que llevara al usuario de detalle auditoria a agregar mas auditorias especificas
//==========================================================================================
function AgregarAuditoriasEspecificas() {
    var codigo_auditoria_integral = $('#codAI_DA').val();
    var anio_auditoria_integral = $('#anioAI_DA').val();
    $.ajax({
        method: 'POST',
        url: '/Auditorias/AsigAudIntegSession',
        data: {
            num_audit_integral: codigo_auditoria_integral,
            anio_audit_integral: anio_auditoria_integral
        },
        dataType: 'json',
        success: function (respuesta) {
            window.location.href = '/Auditorias/ProgramarAuditoria/AuditoriaIndividual?first=false';
        },
        error: function () {

        }
    });
}

async function AnularAuditoria() {
    var NUMERO_AUDITORIA_INTEGRAL = $('#codAI_DA').val();
    var anio_auditoria_integral = $('#anioAI_DA').val();

    Swal.fire({
        title: 'Anular',
        text: "¿Desea anular la auditoría? \n\n Tome en cuenta que la anulación no se podra revertir.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#f1b44c',
        cancelButtonColor: '#d33',
        cancelButtonText: `Cancelar`,
        confirmButtonText: 'Anular',
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.showLoading();

            $.ajax({
                method: 'POST',
                url: '/Auditorias/AnularAuditoria',
                data: {
                    num_audit_integral: NUMERO_AUDITORIA_INTEGRAL,
                    anio_audit_integral: anio_auditoria_integral
                },
                dataType: 'json',
                success: function (respuesta) {
                    if (respuesta == "error") {
                        Swal.fire({
                            title: 'Error',
                            text: "Ocurrió un error al intentar anular la auditoría.",
                            icon: 'error',
                            showCancelButton: false,
                            confirmButtonColor: '#2A3042',
                            confirmButtonText: 'Ok'
                        })
                    } else {
                        Swal.fire({
                            title: 'Guardado!',
                            text: 'Se anuló la auditoría con éxito!',
                            icon: 'success',
                            didClose: () => {
                                window.location.href = '/Auditorias';
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
    })
}

function MostrarOpcionesDA() {
    //Alterna la visibilidad de los div entre display none a block
    $('#opcion1DA').toggle();
    $('#opcion2DA').toggle();

    $('#regresarBtn1').toggle();
    $('#regresarBtn2').toggle();
}

// Función que será llamada al hacer clic en un documento
//==========================================================================================
function IrCuestionarioAudi() {
    window.location.href = '/Auditorias/CuestionariosAuditoria';
}

function IrPlanTrabajo() {
    window.location.href = '/Auditorias/ProgramasDeTrabajo';
}

function IrCartaSalida(codigo_auditoria) {
    window.location.href = '/Auditorias/AuditoriaCartaSalida?id=' + codigo_auditoria;
}

function IrCartaIngreso(codigo_auditoria) {
    window.location.href = '/Auditorias/AuditoriaCartaIngreso?id=' + codigo_auditoria;
}

function IrInformePreliminar() {
    window.location.href = '/Auditorias/AuditoriaResultados/AuditoriaResultadosInforme'
}