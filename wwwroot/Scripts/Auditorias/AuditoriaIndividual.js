CargarTiposAuditorias();
// Obtiene la información de las auditorias especificas de la auditoria integral
GeAuditoriasEspec();
//Llama a la funcion en helpers.js que obtiene los auditores encargados
CargarAuditoresEncargados();

//FUNCION PARA AGREGAR EL REGISTRO AL OBJETO ANTES DE GUARDAR EN BD
//==========================================================================================
async function AgregarAuditoria() {
    var num_audit_integral = $('#num_audit_integral').val();
    var anio_audit_integral = $('#anio_audit_integral').val();
    var codigo_universo_auditable = $('#codigo_universo_auditable').val();
    var tipo_auditoria = $('#tipoAuditoria').val();
    var selectedText = $('#tipoAuditoria option:selected').text();
    var encargado_auditoria = $('#encargadoAuditoria').val();

    if (this.ValidaraAuditoriaEspecifica() == true) {
        Swal.fire({
            title: 'Datos Incompletos',
            text: "Complete todos los campos requeridos (marcados con *)",
            icon: 'info',
            showCancelButton: false,
            confirmButtonColor: '#2A3042',
            confirmButtonText: 'Ok'
        })
    } else {
        Swal.showLoading();

        $.ajax({
            method: 'POST',
            url: '../GuardarAuditoriaEspecifica',
            data: {
                num_auditoria_integral: num_audit_integral,
                anio_auditoria_integral: anio_audit_integral,
                codigo_universo_auditable: codigo_universo_auditable,
                tipo_auditoria: tipo_auditoria,
                selectedText: selectedText,
                encargado_auditoria: encargado_auditoria
            },
            dataType: 'json',
            success: function (respuesta) {
                if (respuesta == "error") {
                    Swal.fire({
                        title: 'Error',
                        text: "Su registro no se pudo guardar.",
                        icon: 'error',
                        showCancelButton: false,
                        confirmButtonColor: '#2A3042',
                        confirmButtonText: 'Ok'
                    })
                } else if (respuesta == "existe") {
                    Swal.fire({
                        title: 'Auditoría Invalida',
                        text: "Ya existe una auditoría de este tipo.",
                        icon: 'warning',
                        showCancelButton: false,
                        confirmButtonColor: '#2A3042',
                        confirmButtonText: 'Ok'
                    })
                }
                else {
                    Swal.fire({
                        title: 'Guardado!',
                        text: 'Registro guardado con exito.',
                        icon: 'success',
                        didClose: () => {

                        }
                    });

                    // Destruimos la tabla con la informacion cargada de las auditorias especificas
                    $('#tbAuditorias').dataTable().fnDestroy();
                    //Obtenemos nuevamente las auditorias especificas actualizadas
                    GeAuditoriasEspec();
                    //Limpiamos el select del tipo de auditoria que se selecciono
                    $('#tipoAuditoria').val("");
                    //Limpiamos el select del encargado de auditoria que se selecciono
                    $('#encargadoAuditoria').val("");
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

//FUNCION PARA ENVIAR LOS REGISTROS EN VARIABLE DE SESION A LA BD
//==========================================================================================
async function ConfirmarAuditoria() {
    var num_audit_integral = $('#num_audit_integral').val();
    var anio_audit_integral = $('#anio_audit_integral').val();
    var codigo_universo_auditable = $('#codigo_universo_auditable').val();
    var tipo_auditoria = $('#tipoAuditoria').val();

    Swal.showLoading();

    $.ajax({
        method: 'POST',
        url: '../ConfirmarAuditoriasEspecifica',
        data: {
            num_auditoria_integral: num_audit_integral,
            anio_auditoria_integral: anio_audit_integral
        },
        dataType: 'json',
        success: function (respuesta) {
            if (respuesta == "Sin Confirmaciones") {
                Swal.fire({
                    title: 'Error',
                    text: "No existen auditorías pendientes de confirmación!",
                    icon: 'info',
                    showCancelButton: false,
                    confirmButtonColor: '#2A3042',
                    confirmButtonText: 'Ok'
                })
            } else if (respuesta == "error") {
                Swal.fire({
                    title: 'Error',
                    text: "Su registro no se pudo guardar.",
                    icon: 'error',
                    showCancelButton: false,
                    confirmButtonColor: '#2A3042',
                    confirmButtonText: 'Ok'
                })
            } else {
                // Destruimos la tabla con la informacion cargada de las auditorias especificas
                $('#tbAuditorias').dataTable().fnDestroy();
                //Obtenemos nuevamente las auditorias especificas actualizadas
                GeAuditoriasEspec();
                //Limpiamos el select del tipo de auditoria que se selecciono
                $('#tipoAuditoria').val("");

                window.location.href = '/Auditorias/DetalleAuditoria';
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

//OBTENEMOS LAS AUDITORIAS ESPECIFICAS
//==========================================================================================
async function GeAuditoriasEspec() {
    $(document).ready(function () {
        var num_audit_integral = $('#num_audit_integral').val();
        var anio_audit_integral = $('#anio_audit_integral').val();
        // MOSTRAMOS TODA LA TABLA DE LAS AUDITORIAS ESPECIFICAS
        $('#tbAuditorias').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": false,
            "ajax":
            {
                "url": '/Auditorias/GetAuditoriasEspecificas',
                "type": "POST",
                data: {
                    num_auditoria_integral: num_audit_integral,
                    anio_auditoria_integral: anio_audit_integral
                },
                "datatype": "json"
            },
            "deferRender": true,
            "columns": [
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) { return row.codigO_AUDITORIA },
                    "name": "CODIGO",
                    "autoWidth": true
                },
                {
                    "data": "nombrE_AUDITORIA",
                    "render": function (data, type, row, meta) { return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.nombrE_AUDITORIA + "</div>"; },
                    "name": "AUDITORIA",
                    "autoWidth": true,
                    "orderable": false
                },
                {
                    "data": "mg_tipos_de_auditorias.descripcion",
                    "render": function (data, type, row, meta) { return "<div class='tipo_auditoria'>" + row.mg_tipos_de_auditorias.descripcion + "</div>"; }, "name": "TIPO DE AUDITORIA",
                    "autoWidth": true,
                    "orderable": false
                },
                {
                    "data": "encargadO_AUDITORIA",
                    "render": function (data, type, row, meta) { return "<div class=''>" + row.encargadO_AUDITORIA + "</div>"; },
                    "name": "encargadO_AUDITORIA",
                    "autoWidth": true,
                    "orderable": false
                },
                {
                    "data": "numerO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        let buttons = "<div class='optiongrid'>";

                        buttons += "<a style='cursor: pointer; href='#' title='Editar Auditoría' onClick='EditarAuditoriaEspec(\"" + row.numerO_AUDITORIA_INTEGRAL + "\", \"" + row.numerO_AUDITORIA + "\", \"" + row.encargadO_AUDITORIA + "\")'> <i class='far fa-edit'> </i> Editar</a>";

                        if (row.codigO_AUDITORIA == "Pendiente Confirmación") {
                            buttons += " | <a style='cursor: pointer; href='#' title='Borrar Auditoría' onClick='BorrarAuditoriaEspec(\"" + row.numerO_AUDITORIA_INTEGRAL + "\", \"" + row.numerO_AUDITORIA + "\")'> <i class='fas fa-trash' style='color:red;'> </i> Borrar</a>";
                        }

                        buttons += "</div>";

                        return buttons;
                    },
                    "autoWidth": true,
                    "orderable": false
                },
            ],
            "language": {
                "url": "https://cdn.datatables.net/plug-ins/1.10.11/i18n/Spanish.json"
            },
            "pagingType": "full_numbers",
            "iDisplayLength": 25,
            "lengthChange": false,
            "responsive": true,
        });
    });

};

//FUNCION PARA BORRAR UNA AUDITORIA ESPECIFICA QUE NO HA SIDO CONFIRMADA
//==========================================================================================
function BorrarAuditoriaEspec(num_audit_integral, numero_auditoria) {
    $.ajax({
        type: 'ajax',
        method: 'post',
        url: '../BorrarAuditoriaEspecifica',
        data: {
            num_audit_integral: num_audit_integral,
            numero_auditoria: numero_auditoria,
        },
        dataType: 'json',
        success: function (result) {
            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'No se pudo borrar el registro. Contacte con TI si el problema persiste.',
                    'error'
                )
            }
            else {
                // Destruimos la tabla con la informacion cargada de las auditorias especificas
                $('#tbAuditorias').dataTable().fnDestroy();
                //Obtenemos nuevamente las auditorias especificas actualizadas
                GeAuditoriasEspec();
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            Swal.fire(
                'Error!',
                'Su registro no se puede borrar  ' + xhr.responseText,
                'error'
            )
        }
    });
}

//==========================================================================================
//VALIDACIONES
//==========================================================================================
function ValidaraAuditoriaEspecifica() {

    var errorValidacion = false;

    var num_audit_integral = $('#num_audit_integral').val();
    var codigo_universo_auditable = $('#codigo_universo_auditable').val();
    var tipo_auditoria = $('#tipoAuditoria').val();
    var encargado_auditoria = $('#encargadoAuditoria').val();


    if (num_audit_integral.length == 0) {
        errorValidacion = true;
    }

    if (codigo_universo_auditable.length == 0) {
        errorValidacion = true;
    }

    if (tipo_auditoria.length == 0) {
        errorValidacion = true;
    }

    if (encargado_auditoria.length == 0) {
        errorValidacion = true;
    }

    return errorValidacion;
}

function MostrarAuditorias() {
    //Alterna la visibilidad de los div entre display none a block
    $('#divAuditoriaIndividual').toggle();
    $('#divSinAuditoriaIndividual').toggle();
}

function EditarAuditoriaEspec(num_audit_integral, numero_auditoria, encargado_auditoria) {
    CargarAuditoresEditar(encargado_auditoria);

    $('#numAuditIntEdit').val(num_audit_integral);
    $('#numAudiEspEdit').val(numero_auditoria);
    //Abrimos el modal para editar al encargado de la auditoria
    $('#editarEncargadoModal').modal('show');
}

//FUNCION PARA CARGAR AUDITORES Y EL ENCARGADO DE LA AUDITORIA
//==========================================================================================
function CargarAuditoresEditar(encargado_auditoria) {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetAuditores',
        dataType: 'json',
        success: function (result) {
            let SelectAuditores = document.getElementById("editarencargadoAuditoria");
            SelectAuditores.innerHTML = ""; // Limpiar el select antes de llenarlo

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'No se pudieron obtener los auditores',
                    'error'
                )
            }
            else {
                result.forEach(function (data) {
                    let opcion = document.createElement("option");
                    opcion.value = data.codigO_USUARIO;
                    opcion.text = data.nombrE_USUARIO;
                    // Verificar si este es el auditor que debe estar seleccionado por defecto
                    if (data.codigO_USUARIO == encargado_auditoria) {
                        opcion.selected = true;
                    }
                    SelectAuditores.appendChild(opcion);
                });
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            Swal.fire(
                'Error!',
                'Sus tipos de auditorias no se pueden obtener  ' + xhr.responseText,
                'error'
            )
        }
    });
}

//FUNCION PARA EDITAR EL ENCARGADO DE LA AUDITORIA
//==========================================================================================
function EditarEncargado() {
    var num_audit_integral = $('#numAuditIntEdit').val();
    var num_audit = $('#numAudiEspEdit').val();
    var encargado_auditoria = $('#editarencargadoAuditoria').val();

    if (encargado_auditoria.length == 0) {
        Swal.fire({
            title: 'Datos Incompletos',
            text: "Debe seleccionar un encargado de la auditoría.",
            icon: 'info',
            showCancelButton: false,
            confirmButtonColor: '#2A3042',
            confirmButtonText: 'Ok'
        })
    } else {
        Swal.showLoading();

        $.ajax({
            method: 'POST',
            url: '../EditarEncargadoAuditoriaEspec',
            data: {
                num_auditoria_integral: num_audit_integral,
                num_audit: num_audit,
                encargado_auditoria: encargado_auditoria
            },
            dataType: 'json',
            success: function (respuesta) {
                if (respuesta == "error") {
                    Swal.fire({
                        title: 'Error',
                        text: "Su registro no se pudo guardar.",
                        icon: 'error',
                        showCancelButton: false,
                        confirmButtonColor: '#2A3042',
                        confirmButtonText: 'Ok'
                    })
                } else {
                    Swal.fire({
                        title: 'Actualizado!',
                        text: 'Registro actualizado con exito.',
                        icon: 'success'
                    });

                    // Destruimos la tabla con la informacion cargada de las auditorias especificas
                    $('#tbAuditorias').dataTable().fnDestroy();
                    //Obtenemos nuevamente las auditorias especificas actualizadas
                    GeAuditoriasEspec();
                    //Cerramos el modal para editar al encargado de la auditoria
                    $('#editarEncargadoModal').modal('hide');
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