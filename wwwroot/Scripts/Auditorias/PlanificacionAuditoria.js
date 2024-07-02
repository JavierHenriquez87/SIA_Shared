CargarAuditores();

//FUNCION PARA GUARDAR EL REGISTRO
//==========================================================================================
async function solicitarAprobacionMP() {
    var TEXT_TIPO_AUDITORIA = $('#texttipoauditoria').val();
    var TEXT_EQUIPO_AUDITORES = $('#textequipoauditores').val();
    var TEXT_TIEMPO_AUDITORIA = $('#texttiempoauditoria').val();
    var OBJETIVO_AUDITORIA = $('#objAuditoria').val();
    var EQUIPO_AUDITORES = $('#equipoAuditores').val();
    var RECURSOS = $('#recursos').val();


    if (this.ValidaraAgregarMDP() == true) {
        Swal.fire({
            title: 'Datos Incompletos',
            text: "Complete todos los campos requeridos (marcados con *)",
            icon: 'warning',
            showCancelButton: false,
            confirmButtonColor: '#2A3042',
            confirmButtonText: 'Ok'
        })
    } else {
        //Convertimos los auditores a string separado por ,
        // Convertir el array a un string separado por comas
        var equipoAuditoresString = EQUIPO_AUDITORES.join(',');

        Swal.showLoading();

        $.ajax({
            method: 'POST',
            url: '../../solicitarAprobacionMP',
            data: {
                text_tipo_auditoria: TEXT_TIPO_AUDITORIA,
                objetivo_auditoria: OBJETIVO_AUDITORIA,
                equipo_auditores: equipoAuditoresString,
                recursos: RECURSOS,
                text_equipo_auditores: TEXT_EQUIPO_AUDITORES,
                text_tiempo_auditoria: TEXT_TIEMPO_AUDITORIA
            },
            dataType: 'json',
            success: function (respuesta) {
                if (respuesta == "Ok") {
                    Swal.fire({
                        title: 'Guardado!',
                        text: 'Registro guardado con exito.',
                        icon: 'success',
                        didClose: () => {
                            window.location.href = '/Auditorias/ProgramarAuditoria';
                        }
                    });
                } else {
                    Swal.fire(
                        'Error!',
                        'Su registro no se pudo guardar.',
                        'error'
                    )
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

//==========================================================================================
//VALIDACIONES
//==========================================================================================
function ValidaraAgregarMDP() {

    var errorValidacion = false;
    var texttipoauditoria = $('#texttipoauditoria').val();
    var objAuditoria = $('#objAuditoria').val();
    var equipoAuditores = $('#equipoAuditores').val();
    var recursos = $('#recursos').val();
    var textequipoAuditores = $('#textequipoauditores').val();
    var texttiempoauditoria = $('#texttiempoauditoria').val();


    if (texttipoauditoria.length == 0) {
        errorValidacion = true;
    }

    if (textequipoAuditores.length == 0) {
        errorValidacion = true;
    }

    if (texttiempoauditoria.length == 0) {
        errorValidacion = true;
    }

    if (objAuditoria.length == 0) {
        errorValidacion = true;
    }

    if (equipoAuditores.length == 0) {
        errorValidacion = true;
    }

    if (recursos.length == 0) {
        errorValidacion = true;
    }


    return errorValidacion;
}