AuditoresEncargados();
GeActividadesAsignadas();
var selectedCheckboxes = [];

//FUNCION PARA REGRESAR PLAN DE TRABAJO
//==========================================================================================
async function RegresarPlanTrabajo() {
    $('#divRegresarTrabajo').modal('show');
}

//FUNCION PARA CARGAR EL MODAL DE ASIGNAR ACTIVIDAD
//==========================================================================================
async function VerActividades() {
    // Selecciona todos los checkboxes dentro de la tabla y los desmarca
    $("#tbActividades tbody input[type='checkbox']").prop("checked", false);

    $('#divAsignarActividades').modal('show');
}

//Aplicar Configuracion a DataTables a la tabla de las actividades
$(function () {
    $("#tbActividades").DataTable({
        "paging": true,
        "lengthChange": true,
        "searching": true,
        "ordering": false,
        "info": true,
        "autoWidth": false,
        "language": {
            "decimal": "",
            "emptyTable": "No hay datos disponibles en la tabla",
            "info": "Mostrando _START_ a _END_ de _TOTAL_ registros",
            "infoEmpty": "Mostrando 0 a 0 de 0 registros",
            "infoFiltered": "(filtrado de _MAX_ registros totales)",
            "lengthMenu": "Mostrar _MENU_ registros",
            "loadingRecords": "Cargando...",
            "processing": "Procesando...",
            "search": "Buscar:",
            "zeroRecords": "No se encontraron registros coincidentes",
            "paginate": {
                "first": "Primero",
                "last": "Último",
                "next": "Siguiente",
                "previous": "Anterior"
            },
            "aria": {
                "sortAscending": ": activar para ordenar la columna de manera ascendente",
                "sortDescending": ": activar para ordenar la columna de manera descendente"
            }
        },
        "search": {
            "search": "",
            "smart": true,
            "caseInsensitive": true,
            "regex": false
        }
    });
});

//Agregamos o quitamos el registro seleccionado 
$("#tbActividades").on('change', 'input[type="checkbox"]', function () {
    var checkboxValue = $(this).val();

    if ($(this).is(':checked')) {
        // Si está marcado, lo agregamos al array
        if (!selectedCheckboxes.includes(checkboxValue)) {
            selectedCheckboxes.push(checkboxValue);
        }
    } else {
        // Si está desmarcado, lo eliminamos del array
        selectedCheckboxes = selectedCheckboxes.filter(function (value) {
            return value !== checkboxValue;
        });
    }
});

async function AsignarActividad() {
    // Selecciona todos los checkboxes que están marcados
    //var checkboxesActivos = $("#tbActividades tbody input[type='checkbox']:checked");
    var usuario_asignado = $('#EquipoAuditoresAsignados').val();
    var numero_pdt = $('#numero_PDT_pt').val();
    var numeroai = $('#codAI_CU').val();
    var anioai = $('#anioAI_CU').val();
    var numero_auditoria_pt = $('#numero_auditoria_pt').val();

    if (usuario_asignado.length == 0) {
        Swal.fire({
            title: 'Datos Incompletos',
            text: "Seleccione un auditor",
            icon: 'info',
            showCancelButton: false,
            confirmButtonColor: '#2A3042',
            confirmButtonText: 'Ok'
        })
        return false; // Detiene la ejecución si es necesario
    }

    if (selectedCheckboxes.length === 0) {
        // Si no hay ninguno marcado, muestra un mensaje de advertencia
        Swal.fire({
            title: 'Datos Incompletos',
            text: "Seleccione al menos una tarea para asignar",
            icon: 'info',
            showCancelButton: false,
            confirmButtonColor: '#2A3042',
            confirmButtonText: 'Ok'
        })

        return false; // Detiene la ejecución si es necesario
    }

    // Mostrar el loader antes de iniciar la solicitud AJAX
    Swal.fire({
        title: "Guardando...",
        text: "Por favor, espere.",
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    //var valoresCheckboxes = checkboxesActivos.map(function () {
    //    return $(this).val();
    //}).get();

    // Convertir el array en una cadena separada por comas
    //var actividadesString = valoresCheckboxes.join(',');
    var actividadesString = selectedCheckboxes.join(',');
    var data = {
        numeropdt: numero_pdt,
        numeroai: numeroai,
        anioai: anioai,
        numero_auditoria: numero_auditoria_pt,
        codigo_usuario_asignado: usuario_asignado,
        actividades: actividadesString
    };

    $.ajax({
        method: 'POST',
        url: 'Guardar_Actividades_asignadas',
        data: JSON.stringify(data),
        contentType: 'application/json',
        success: function (respuesta) {
            if (respuesta == "error") {
                Swal.fire(
                    'Error!',
                    'Ocurrio un error al guardar las actividades.',
                    'error'
                )
            } else {
                $('#divAsignarActividades').modal('hide');

                Swal.fire({
                    title: 'Guardado!',
                    text: '¡Se guardaron las actividades con exito!',
                    icon: 'success',
                    didClose: () => {
                        window.location.reload();
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


//FUNCION PARA OBTENER LAS ACTIVIDADES ASIGNADAS A LOS AUDITORES EN UN PLAN DE TRABAJO
//==========================================================================================
async function GeActividadesAsignadas() {
    var numero_pdt = $('#numero_PDT_pt').val();
    var codAuditor = $('#SelectEquipoAuditoresAsignados').val();

    $(document).ready(function () {
        // MOSTRAMOS TODA LA TABLA DE LAS AUDITORIAS ESPECIFICAS
        $('#tbProgramaTrabajo').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "ajax":
            {
                "url": '/Auditorias/ActividadesAsignadas',
                "type": "POST",
                data: {
                    numero_pdt: numero_pdt,
                    cod_auditor: codAuditor
                },
                "datatype": "json"
            },
            "deferRender": true,
            "columns": [
                {
                    "data": "nombrE_ACTIVIDAD",
                    "render": function (data, type, row, meta) { return row.nombrE_ACTIVIDAD },
                    "name": "nombrE_ACTIVIDAD",
                    "autoWidth": true
                },
                {
                    "data": "nombrE_USUARIO",
                    "render": function (data, type, row, meta) { return row.nombrE_USUARIO },
                    "name": "nombrE_USUARIO",
                    "autoWidth": true
                },
                {
                    "data": "codigO_ESTADO",
                    "render": function (data, type, row, meta) {
                        var text = "";
                        var status = "";
                        if (row.codigO_ESTADO == 0) {
                            text = "Creada";
                            status = "create";
                        } else if (row.codigO_ESTADO == 1) {
                            text = "Aprobación Pendiente";
                            status = "pendingcreate";
                        } else {
                            text = "En Proceso";
                            status = "process";
                        }
                        return "<div class='statusACT "+ status + "' style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + text + "</div>";
                    },
                    "name": "codigO_ESTADO",
                    "autoWidth": true
                },
                {
                    "data": "codigO_ACTIVIDAD",
                    "render": function (data, type, row, meta) {
                        let buttons = "<div class='optiongrid'>";

                        /*buttons += "<a style='cursor: pointer; href='#' title='Editar Actividad' onClick='EditarActividadAsignada(\"" + row.codigO_ACTIVIDAD + "\", \"" + row.numerO_PDT + "\", \"" + row.codigO_USUARIO_ASIGNADO + "\")'> <i class='fas fa-edit' style='color: black;'></i></a>";*/

                        buttons += " | <a style='cursor: pointer;' href='#' title='Eliminar Actividad' onClick='EliminarActividadAsignada(\"" + row.codigO_ACTIVIDAD + "\", \"" + row.numerO_PDT + "\", \"" + row.codigO_USUARIO_ASIGNADO + "\")'> <i class='fas fa-trash' style='color: black;'></i></a>";

                        buttons += " | <a style='cursor: pointer;' href='#' title='Ir Actividad' onClick='IrActividadAsignada(\"" + row.codigO_ACTIVIDAD + "\", \"" + row.numerO_PDT + "\", \"" + row.codigO_USUARIO_ASIGNADO + "\")'> <i class='fas fa-share' style='color: black;'></i></a>";

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
            "iDisplayLength": 15,
            "lengthChange": false,
            "responsive": true,
        });
    });

};

function EliminarActividadAsignada(codigo_actividad, numero_pdt, codigo_usuario_asignado) {
    var data = {
        codigoActividad: codigo_actividad,
        numeroPdt: numero_pdt,
        codigoUsuarioAsignado: codigo_usuario_asignado
    };

    Swal.fire({
        title: '¿Estás seguro?',
        text: "¡No podrás recuperar esta actividad después de eliminarla!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí, eliminar!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            // Si el usuario confirma la eliminación, procede a eliminar la actividad
            var data = {
                codigoActividad: codigo_actividad,
                numeroPdt: numero_pdt,
                codigoUsuarioAsignado: codigo_usuario_asignado
            };

            $.ajax({
                method: 'POST',
                url: 'Eliminar_Actividad_asignada',
                data: $.param(data),
                success: function (respuesta) {
                    if (respuesta == "error") {
                        Swal.fire(
                            'Error!',
                            'Ocurrió un error al eliminar la actividad.',
                            'error'
                        );
                    } else {
                        $('#divAsignarActividades').modal('hide');

                        Swal.fire({
                            title: 'Eliminado!',
                            text: '¡Se eliminó la actividad con éxito!',
                            icon: 'success',
                            didClose: () => {
                                window.location.reload();
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
    });
}

//FUNCION PARA OBTENER LAS ACTIVIDADES ASIGNADAS A LOS AUDITORES EN UN PLAN DE TRABAJO
//==========================================================================================
async function GeActividadesAsignadasXAuditor() {
    $('#tbProgramaTrabajo').dataTable().fnDestroy();
    GeActividadesAsignadas();
};

//FUNCION PARA IR A LA PANTALLA DE HALLAZGOS DE UNA ACTIVIDAD
//==========================================================================================
function IrActividadAsignada(codigo_actividad, numero_pdt, codigo_usuario_asignado) {
    codigo_actividad = encodeBase64(codigo_actividad);
    numero_pdt = encodeBase64(numero_pdt);
    codigo_usuario_asignado = encodeBase64(codigo_usuario_asignado);
    window.location.href = 'AuditoriaResultados?ca=' + codigo_actividad + '&pdt=' + numero_pdt + '&us=' + codigo_usuario_asignado;
}

//FUNCION PARA CODIFICAR A BASE64 LOS DATOS DE LA URL
function encodeBase64(str) {
    return btoa(unescape(encodeURIComponent(str)));
}