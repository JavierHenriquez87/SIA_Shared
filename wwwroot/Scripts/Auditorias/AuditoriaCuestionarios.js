function openSidebarCuest() {
    var sidebar = document.getElementById("SidebarAddCuestionario");
    if (window.innerWidth <= 600) {
        sidebar.style.width = "90%";
    } else {
        sidebar.style.width = "800px";
    }
    document.getElementById("overlayCuest").style.display = "block";

    GetCuestionarios();
}

async function closeSidebarCuest() {
    document.getElementById("SidebarAddCuestionario").style.width = "0";
    document.getElementById("overlayCuest").style.display = "none";
    window.location.reload();
}


//OBTENEMOS LOS CUESTIONARIOS DISPONIBLES PARA PODER AGREGARSE A UNA AUDITORIA
//==========================================================================================
async function GetCuestionarios() {
    $(document).ready(function () {
        var num_audit_integral = $('#codAI_CU').val();
        var anio_audit_integral = $('#anioAI_CU').val();

        $('#tbCuestionarios').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "ajax":
            {
                "url": '/Auditorias/ObtenerCuestionarios',
                "type": "POST",
                "datatype": "json"
            },
            "deferRender": true,
            "columns": [
                {
                    "data": "codigO_TIPO_AUDITORIA",
                    "render": function (data, type, row, meta) { return row.codigO_TIPO_AUDITORIA },
                    "name": "codigO_TIPO_AUDITORIA",
                    "autoWidth": true
                },
                {
                    "data": "nombrE_CUESTIONARIO",
                    "render": function (data, type, row, meta) { return row.nombrE_CUESTIONARIO },
                    "name": "nombrE_CUESTIONARIO",
                    "autoWidth": true
                },
                {
                    "data": "numerO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        let buttons = "<div class='optiongrid'>";

                        buttons += "<a style='cursor: pointer; href='#' title='Ver Cuestionario' onClick='VerCuestionario(\"" + row.codigO_CUESTIONARIO + "\")'> <i class='fas fa-eye' style='color:green;'> </i> Ver</a>";

                        buttons += " | <a style='cursor: pointer; href='#' title='Agregar Cuestionario' onClick='AgregarCuestionarioAuditoria(\"" + num_audit_integral + "\", \"" + anio_audit_integral + "\", \"" + row.codigO_CUESTIONARIO + "\", \"" + row.codigO_TIPO_AUDITORIA + "\")'> <i class='fas fa-plus' style='color:green;'> </i> Agregar</a>";


                        buttons += "</div>";

                        return buttons;
                    },
                    "autoWidth": true,
                    "orderable": false
                },
            ],
            "language": {
                "url": "https://cdn.datatables.net/plug-ins/1.10.11/i18n/Spanish.json",
                "info": ""
            },
            "iDisplayLength": 25,
            "lengthChange": false,
            "responsive": false
        });
    });

};


//GUARDA EN LA AUDITORIA UN CUESTIONARIO
//==========================================================================================
async function AgregarCuestionarioAuditoria(num_audit_integral, anio_audit_integral, codigo_cuestionario, tipo_auditoria) {
    // Objeto que contiene los datos a enviar
    var DataCuest = {
        NUMERO_AUDITORIA_INTEGRAL: num_audit_integral,
        ANIO: anio_audit_integral,
        CODIGO_CUESTIONARIO: codigo_cuestionario,
        CODIGO_AUD_CUEST: tipo_auditoria
    };

    Swal.showLoading();

    return new Promise((resolve, reject) => {
        $.ajax({
            method: 'POST',
            url: '/Auditorias/Guardar_Audit_Cuestionario',
            data: JSON.stringify(DataCuest),
            contentType: 'application/json',
            success: function (respuesta) {
                if (respuesta == "error") {
                    Swal.fire(
                        'Error!',
                        'Su registro no se pudo guardar.',
                        'error'
                    )
                } else {
                    const Toast = Swal.mixin({
                        toast: true,
                        position: "top-end",
                        showConfirmButton: false,
                        timer: 3000,
                        timerProgressBar: true,
                        didOpen: (toast) => {
                            toast.onmouseenter = Swal.stopTimer;
                            toast.onmouseleave = Swal.resumeTimer;
                        }
                    });
                    Toast.fire({
                        icon: "success",
                        title: "Se guardo el cuestionario para la auditoria con éxito."
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
        resolve();
    });
}


//OBTENEMOS LAS PREGUNTAS DE UN CUESTIONARIO PARA MOSTRARLAS
//==========================================================================================
async function VerCuestionario(codigo_cuestionario) {
    $(document).ready(function () {
        $.ajax({
            type: 'ajax',
            method: 'post',
            url: 'ObtenerPreguntasCuestionario',
            data: {
                codigo_cuestionario: codigo_cuestionario
            },
            dataType: 'json',
            success: function (result) {
                if (result == "error") {
                    Swal.fire(
                        'Error!',
                        'Ocurrio un error al obtener el cuestionario',
                        'error'
                    )
                }
                else {
                    //Mostramos el modal
                    $('#verCuestionarioModal').modal('show');
                    // Limpiamos la tabla antes de agregar nuevas filas
                    $('#tbVerCuestionarioPreguntas').empty();

                    // Recorremos el objeto resultante y llenamos la tabla
                    result.forEach(function (seccion) {
                        // Agregamos la sección como un encabezado
                        $('#tbVerCuestionarioPreguntas').append(
                            `<tr class="table-primary">
                                <th colspan="2" style="text-align:center;">${seccion.DESCRIPCION_SECCION}</th>
                            </tr>`
                        );

                        // Iniciamos el contador para el correlativo de preguntas
                        let correlativo = 1;

                        seccion.sub_secciones.forEach(function (subSeccion) {
                            // Agregamos la sub-sección como un subtítulo
                            $('#tbVerCuestionarioPreguntas').append(
                                `<tr class="table-secondary">
                                    <th colspan="2">${subSeccion.DESCRIPCION}</th>
                                </tr>`
                            );

                            subSeccion.Preguntas_Cuestionarios.forEach(function (pregunta) {
                                // Agregamos las preguntas de la sub-sección
                                $('#tbVerCuestionarioPreguntas').append(
                                    `<tr>
                                        <td>${correlativo++}</td>
                                        <td>${pregunta.DESCRIPCION}</td>
                                    </tr>`
                                );
                            });
                        });
                    });

                }
            },
            error: function (xhr, textStatus, errorThrown) {
                Swal.fire(
                    'Error!',
                    'Ocurrio un error',
                    'error'
                )
            }
        });
    });

};

function handleClickCuestionario(element) {
    var codigoCuest = element.getAttribute('data-codigo-cuest');
    window.location.href = 'CuestionariosAuditoria/CuestionarioTrabajo?dc=' + codigoCuest;
}

//FUNCION PARA BORRAR UN CUESTIONARIO
//==========================================================================================
function borrarCuestionario(event) {
    event.stopPropagation(); // Detiene la propagación del evento para que no se dispare el onclick del contenedor principal.

    // Obtener el elemento contenedor principal (div) desde el ícono de borrar
    const parentDiv = event.currentTarget.closest('[data-codigo-cuest]');
    const codigoCuest = parentDiv.getAttribute('data-codigo-cuest');

    console.log("Eliminar cuestionario con código:", codigoCuest);
    const swalWithBootstrapButtons = Swal.mixin({
        customClass: {
            confirmButton: "btn btn-success mx-2", // mx-2 agrega margen horizontal
            cancelButton: "btn btn-danger mx-2"
        },
        buttonsStyling: false // Esto es necesario para que SweetAlert2 no sobrescriba los estilos de Bootstrap
    });

    swalWithBootstrapButtons.fire({
        title: "Borrar Cuestionario",
        text: "¿Desea borrar este cuestionario de la auditoria?",
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
                url: '/Auditorias/BorrarCuestionario',
                data: {
                    codigo_cuestionario: codigoCuest
                },
                dataType: 'json',
                success: function (result) {
                    if (result == "error") {
                        Swal.fire(
                            'Error!',
                            'No se pudo borrar el cuestionario. Contacte con TI si el problema persiste.',
                            'error'
                        )
                    }
                    else {
                        window.location.reload();
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
    });
}
