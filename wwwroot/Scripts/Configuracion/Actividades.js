// Obtiene la información de las actividades
GetActividades();
CargarTiposAuditoriasAct();

//FUNCION PARA OBTENER LISTADO DE ACTIVIDADES
//==========================================================================================
function GetActividades() {
    $(document).ready(function () {

        // MOSTRAMOS TODA LA TABLA DE LOS USUARIOS
        $('#tbActividadesConf').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "ajax":
            {
                "url": '/Configuracion/GetActividades',
                "type": "POST",
                "data": function (d) {
                    // Pasamos el valor del select al servidor
                    d.tipoAuditoria = $('#tipoAuditoriaAct').val();
                },
                "datatype": "json"
            },
            "deferRender": true,
            "columns": [
                {
                    "data": "codigO_ACTIVIDAD",
                    "render": function (data, type, row, meta) { return row.codigO_ACTIVIDAD },
                    "name": "codigO_ACTIVIDAD",
                    "autoWidth": true,
                    "width": "10%",
                    "orderable": false
                },
                {
                    "data": "nombrE_ACTIVIDAD",
                    "render": function (data, type, row, meta) { return row.nombrE_ACTIVIDAD },
                    "name": "nombrE_ACTIVIDAD",
                    "autoWidth": true,
                    "orderable": true
                },
                {
                    "data": "descripcion",
                    "render": function (data, type, row, meta) { return row.descripcion },
                    "name": "descripcion",
                    "autoWidth": true,
                    "orderable": false
                },
                {
                    "data": "codigO_ESTADO",
                    "render": function (data, type, row, meta) {
                        var estado = "Activa";
                        if (row.codigO_ESTADO == "I") {
                            estado = "Inactiva";
                        }

                        return estado
                    },
                    "name": "codigO_ESTADO",
                    "autoWidth": true,
                    "orderable": false,
                    "width": "10%"
                },
                {
                    "data": "codigO_TIPO_AUDITORIA",
                    "render": function (data, type, row, meta) { return row.codigO_TIPO_AUDITORIA },
                    "name": "codigO_TIPO_AUDITORIA",
                    "autoWidth": true,
                    "orderable": false,
                    "width": "10%"
                },
                {
                    "data": "codigO_ACTIVIDAD",
                    "render": function (data, type, row, meta) {
                        let buttons = "<div class='optiongrid'>";

                        buttons += "<a style='cursor: pointer; href='#' title='Editar Actividad' onClick='ModificarActividad(\"" + row.codigO_ACTIVIDAD + "\")'> <i class='fas fa-edit' style='color: black;'></i></a>";

                        buttons += " | <a style='cursor: pointer; href='#' title='Inactivar Actividad' onClick='InactivarActividad(\"" + row.codigO_ACTIVIDAD + "\", \"" + row.codigO_ESTADO + "\")'> <i class='fas fa-eye' style='color: black;'></i></a>";

                        buttons += "</div>";

                        return buttons;
                    },
                    "autoWidth": true,
                    "orderable": false,
                    "width": "10%"
                }
            ],
            "language": {
                "url": "https://cdn.datatables.net/plug-ins/1.10.11/i18n/Spanish.json"
            },
            "pagingType": "full_numbers",
            "iDisplayLength": 25,
            "lengthChange": false,
            "responsive": true
        });
    });

};

//FUNCION PARA OBTENER LOS TIPOS DE AUDITORIAS SEGUN LA OPCION SELECCIONADA
//==========================================================================================
function GetActividadesChange() {
    $('#tbActividadesConf').dataTable().fnDestroy();
    GetActividades();
}

//FUNCION PARA CARGAR LOS TIPOS DE AUDITORIAS
//==========================================================================================
function CargarTiposAuditoriasAct() {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetTiposAuditoria',
        dataType: 'json',
        success: function (result) {
            let selectTiposAud = document.getElementById("tipoAuditoriaAct");
            let selectTiposAudNew = document.getElementById("tipoAuditoriaActNew");

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Los tipos de auditorias no se pueden obtener',
                    'error'
                )
            }
            else {
                result.forEach(function (tipo) {
                    let opcion = document.createElement("option");
                    opcion.value = tipo.codigO_TIPO_AUDITORIA;
                    opcion.text = tipo.descripcion;
                    selectTiposAud.appendChild(opcion);
                });

                result.forEach(function (tipo) {
                    let opcion = document.createElement("option");
                    opcion.value = tipo.codigO_TIPO_AUDITORIA;
                    opcion.text = tipo.descripcion;
                    selectTiposAudNew.appendChild(opcion);
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

//FUNCION PARA CARGAR EL MODAL DE NUEVA ACTIVIDAD
//==========================================================================================
function NuevaActividad() {
    LimpiarModalActividad();

    //Ocultamos el boton de guardar y mostramos editar
    document.getElementById('btnGuardarActividad').style.display = 'block';
    document.getElementById('GuardarActividadEditada').style.display = 'none';

    $('#codigoActividadEdit').prop('disabled', false);

    var modalTitle = document.getElementById('titleNuevaActividad');
    modalTitle.textContent = 'Nueva Actividad';

    //Mostramos el modal
    $('#divActividad').modal('show');
}

//FUNCION PARA LIMPIAR EL MODAL DE AGREGAR ACTIVIDAD
//==========================================================================================
function LimpiarModalActividad() {
    $('#codigoActividadEdit').val("");
    $('#inputNombreActividad').val("");
    $('#inputDescripActividad').val("");
    $('#tipoAuditoriaActNew').val("");
}

//FUNCION PARA GUARDAR NUEVA ACTIVIDAD
//==========================================================================================
function GuardarActividadNueva() {
    var response = validarInfoActividad();

    if (response == true) {
        Swal.fire(
            'Advertencia!',
            'Complete todos los campos.',
            'warning'
        )
    } else {

        var inputNombreActividad = $('#inputNombreActividad').val();
        var inputDescripActividad = $('#inputDescripActividad').val();
        var tipoAuditoriaActNew = $('#tipoAuditoriaActNew').val();

        // Objeto que contiene los datos a enviar
        var userData = {
            NOMBRE_ACTIVIDAD: inputNombreActividad,
            DESCRIPCION: inputDescripActividad,
            CODIGO_TIPO_AUDITORIA: tipoAuditoriaActNew
        };

        $.ajax({
            url: 'Guardar_Actividad',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(userData),
            success: function (result) {
                $('#divActividad').modal('hide');

                if (result == "error") {
                    Swal.fire(
                        'Error!',
                        'Ocurrió un error al intentar guardar',
                        'error'
                    )
                }
                else {
                    $('#tbActividadesConf').dataTable().fnDestroy();
                    GetActividades();

                    Swal.fire(
                        'Guardado!',
                        'Registro guardado con exito.',
                        'success'
                    )
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                Swal.fire(
                    'Error!',
                    'Su registro no se pudo guardar  ' + xhr.responseText,
                    'error'
                )
            }
        });
    }
}

//FUNCION PARA CARGAR LA INFORMACION DE LA ACTIVIDAD
//==========================================================================================
function ModificarActividad(codigoActividad) {
    $.ajax({
        type: 'ajax',
        method: 'post',
        url: 'Informacion_Actividad',
        data: {
            codigoActividad: codigoActividad,
        },
        dataType: 'json',
        success: function (result) {
            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Ocurrió un error al obtener la información',
                    'error'
                )
            }
            else {
                LimpiarModalActividad();

                //Ocultamos el boton de guardar y mostramos editar
                document.getElementById('btnGuardarActividad').style.display = 'none';
                document.getElementById('GuardarActividadEditada').style.display = 'block';

                $('#codigoActividadEdit').prop('disabled', true);

                var modalTitle = document.getElementById('titleNuevaActividad');
                modalTitle.textContent = 'Editar Actividad';

                //agregamos la informacion
                $('#codigoActividadEdit').val(codigoActividad);
                $('#inputNombreActividad').val(result.nombrE_ACTIVIDAD);
                $('#inputDescripActividad').val(result.descripcion);
                $('#tipoAuditoriaActNew').val(result.codigO_TIPO_AUDITORIA);

                //Mostramos el modal
                $('#divActividad').modal('show');
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            Swal.fire(
                'Error!',
                'Su registro no se puede editar  ' + xhr.responseText,
                'error'
            )
        }
    });
}

//FUNCION PARA GUARDAR LA ACTIVIDAD QUE SE HA EDITADO
//==========================================================================================
function GuardarActividadEditada() {
    var response = validarInfoActividad();

    if (response == true) {
        Swal.fire(
            'Advertencia!',
            'Complete todos los campos.',
            'warning'
        )
    } else {

        var inputCodigoActividad = $('#codigoActividadEdit').val();
        var inputNombreActividad = $('#inputNombreActividad').val();
        var inputDescripActividad = $('#inputDescripActividad').val();
        var tipoAuditoriaActNew = $('#tipoAuditoriaActNew').val();

        // Objeto que contiene los datos a enviar
        var userData = {
            CODIGO_ACTIVIDAD: inputCodigoActividad,
            NOMBRE_ACTIVIDAD: inputNombreActividad,
            DESCRIPCION: inputDescripActividad,
            CODIGO_TIPO_AUDITORIA: tipoAuditoriaActNew
        };

        $.ajax({
            url: 'Editar_Actividad',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(userData),
            success: function (result) {
                $('#divActividad').modal('hide');

                if (result == "error") {
                    Swal.fire(
                        'Error!',
                        'Su registro no se puede editar',
                        'error'
                    )
                }
                else {
                    $('#tbActividadesConf').dataTable().fnDestroy();
                    GetActividades();

                    Swal.fire(
                        'Guardado!',
                        'Registro guardado con exito.',
                        'success'
                    )
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                Swal.fire(
                    'Error!',
                    'Su registro no se puede editar  ' + xhr.responseText,
                    'error'
                )
            }
        });
    }
}

//FUNCION PARAINACTIVAR UNA ACTIVIDAD
//==========================================================================================
function InactivarActividad(codigoActividad, codigoEstado) {
    let textoEstado = codigoEstado == "A" ? "inactivar" : "activar";

    const swalWithBootstrapButtons = Swal.mixin({
        customClass: {
            confirmButton: "btn btn-success mx-2", // mx-2 agrega margen horizontal
            cancelButton: "btn btn-danger mx-2"
        },
        buttonsStyling: false // Esto es necesario para que SweetAlert2 no sobrescriba los estilos de Bootstrap
    });

    swalWithBootstrapButtons.fire({
        title: "Modificar Actividad",
        text: "¿Desea " + textoEstado + " esta actividad?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí",
        cancelButtonText: "Cancelar",
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: 'ajax',
                method: 'post',
                url: 'Inactivar_Actividad',
                data: {
                    codigoActividad: codigoActividad,
                },
                dataType: 'json',
                success: function (result) {
                    if (result == "error") {
                        Swal.fire(
                            'Error!',
                            'Ocurrió un error al ' + textoEstado + ' la actividad',
                            'error'
                        )
                    }
                    else {
                        $('#tbActividadesConf').dataTable().fnDestroy();
                        GetActividades();

                        Swal.fire(
                            'Guardado!',
                            'Actividad modificada con éxito.',
                            'success'
                        )
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    Swal.fire(
                        'Error!',
                        'Su registro no se puede modificar  ' + xhr.responseText,
                        'error'
                    )
                }
            });
        }
    });
}

function validarInfoActividad() {
    var errorValidacion = false;

    var inputNombreActividad = $('#inputNombreActividad').val();
    var inputDescripActividad = $('#inputDescripActividad').val();
    var tipoAuditoriaActNew = $('#tipoAuditoriaActNew').val();

    if (inputNombreActividad.length == 0) {
        errorValidacion = true;
    }

    if (inputDescripActividad.length == 0) {
        errorValidacion = true;
    }

    if (tipoAuditoriaActNew.length == 0) {
        errorValidacion = true;
    }

    return errorValidacion;
}