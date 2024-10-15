// Obtiene el listado de los cuestionarios
let seccionesArray = [];
GetCuestionarios();

function GetCuestionarios() {
    $(document).ready(function () {
        // MOSTRAMOS TODA LA TABLA DE LAS AUDITORIAS ESPECIFICAS
        $('#tbCuestionarios').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "ajax":
            {
                "url": '/Configuracion/GetCuestionarios',
                "type": "POST",
                "datatype": "json"
            },
            "deferRender": true,
            "columns": [
                {
                    "data": "codigO_CUESTIONARIO", "render": function (data, type, row, meta) { return row.codigO_CUESTIONARIO }, "name": "CODIGO CUESTIONARIO", "autoWidth": true
                },
                {
                    "data": "nombrE_CUESTIONARIO", "render": function (data, type, row, meta) { return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.nombrE_CUESTIONARIO + "</div>"; }, "name": "NOMBRE CUESTIONARIO", "autoWidth": true, "orderable": false
                },
                {
                    "data": "codigO_CUESTIONARIO",
                    "render": function (data, type, row, meta) {
                        let buttons = "<div class='optiongrid'>";

                        buttons += "<a href='javascript:void(0)' title='Editar Cuestionario' onClick='ModificarCuestionario(\"" + row.codigO_CUESTIONARIO + "\", \"" + row.nombrE_CUESTIONARIO + "\")'> <i class='fas fa-pencil-alt'> </i> Modificar</a> ";

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
            responsive: "true",
        });
    });
};

//FUNCION PARA CARGAR EL MODAL DE NUEVO CUESTIONARIO
//==========================================================================================

function NuevoCuestionario() {
    //Limpiamos los arreglos
    seccionesArray = [];
    subseccionesSeleccionadas = [];

    $(document).ready(function () {
        $.ajax({
            type: 'post',
            url: '/Configuracion/ObtenerPreguntasCuestionario',
            data: {},
            dataType: 'json',
            success: function (result) {
                if (result == "error") {
                    Swal.fire(
                        'Error!',
                        'Ocurrio un error al obtener el cuestionario',
                        'error'
                    );
                } else {
                    // Limpiamos la tabla antes de agregar nuevas filas
                    $('#tbVerCuestionarioPreguntas').empty();
                    // Mostramos el modal
                    $('#verCuestionarioModal').modal('show');

                    const selectSeccion = document.getElementById('selectSeccion');
                    const opcionInicial = selectSeccion.options[0];

                    // Limpiar todas las opciones del select
                    selectSeccion.innerHTML = '';
                    // Volver a agregar solo la opción inicial
                    selectSeccion.appendChild(opcionInicial);

                    // Recorremos el objeto resultante y llenamos la tabla
                    result.forEach(function (seccion, index) {
                        const option = document.createElement('option');
                        option.value = seccion.CODIGO_SECCION;  
                        option.text = seccion.DESCRIPCION_SECCION;
                        selectSeccion.appendChild(option);
                    });
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                Swal.fire(
                    'Error!',
                    'Ocurrio un error',
                    'error'
                );
            }
        });
    });

    //Ocultamos el boton de editar y mostramos guardar
    document.getElementById('btnGuardarCuestionario').style.display = 'block';
    document.getElementById('btnEditarCuestionario').style.display = 'none';

    $('#nombreCuestionario').val("");

    var modalTitle = document.getElementById('titleNuevoCuestionario');
    modalTitle.textContent = 'Nuevo Cuestionario';

    //Mostramos el modal
    $('#divCuestionario').modal('show');
}

// Event listener para agregar subsecciones
$(document).on('click', '.btnAddSubsection', function () {
    const sectionId = $(this).data('section');
    const subsectionCount = $(`#subsections-${sectionId} .subsection-container`).length + 1;

    const newSubsectionHtml = `
        <div id="subsection-${sectionId}-${subsectionCount}" class="subsection-container">
            <input type="text" placeholder="Nombre de la Subseccion" class="form-control subsection-title">
            <table class="table">
                <thead>
                    <tr>
                        <th style="width: 85%">Pregunta</th>
                        <th style="width: 15%">Acciones</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
            <button type="button" class="btnAddPregunta btn btn-secondary" data-section="${sectionId}" data-subsection="subsection-${sectionId}-${subsectionCount}">Agregar Pregunta</button>
            <button type="button" class="btnEliminarSubseccion btn btn-danger" data-subsection="subsection-${sectionId}-${subsectionCount}">Eliminar Subsección</button>
            <hr>
        </div>
    `;

    $(`#subsections-${sectionId}`).append(newSubsectionHtml);
});

// Evento para eliminar la subsección
$(document).on('click', '.btnEliminarSubseccion', function () {
    const subsectionId = $(this).data('subsection');
    $(`#${subsectionId}`).remove();
});

// Event listener para agregar preguntas
$(document).on('click', '.btnAddPregunta', function () {
    const subsectionId = $(this).data('subsection');

    const newPreguntaHtml = `
        <tr>
            <td>
                <input type="text" placeholder="Escribir Pregunta" class="form-control question-input">
            </td>
            <td>
                <button type="button" class="btnRemovePregunta btn btn-danger">Eliminar</button>
            </td>
        </tr>
    `;

    $(`#${subsectionId} tbody`).append(newPreguntaHtml);
});

// Event listener para eliminar preguntas
$(document).on('click', '.btnRemovePregunta', function () {
    $(this).closest('tr').remove();
});

//FUNCION PARA CARGAR LA INFORMACION DEL CUESTIONARIO
//==========================================================================================

function ModificarCuestionario(id, nombre) {
    //Ocultamos el boton de editar y mostramos guardar
    document.getElementById('btnGuardarCuestionario').style.display = 'none';
    document.getElementById('btnEditarCuestionario').style.display = 'block';

    $('#nombreCuestionario').val("");

    var modalTitle = document.getElementById('titleNuevoCuestionario');
    modalTitle.textContent = 'Editar Cuestionario';

    //Mostramos el modal
    $('#divCuestionario').modal('show');
}

//FUNCION PARA VALIDAR EL NOMBRE DEL CUESTIONARIO
//==========================================================================================
function validarNombreCuestionarioDuplicado($id, $nombreCuestionario) {
    if ($nombreCuestionario != "") {
        return new Promise((resolve, reject) => {
            //Validamos que el nombre del cuestionario no exista
            $.ajax({
                method: 'POST',
                url: 'VerificarNombreCuestionario',
                data: { NOMBRE_CUESTIONARIO: $nombreCuestionario },
                success: function (respuesta) {
                    if (respuesta == 'true') {
                        $("#validacionCuestionario").text("El nombre del cuestionario ya existe");
                        document.getElementById('validacionCuestionario').style.display = 'block';
                        resolve(true);
                    }
                    else {
                        $("#validacionCuestionario").text("");
                        document.getElementById('validacionCuestionario').style.display = 'none';
                        resolve(false);
                    }
                }
            });
        });
    }
    else {
        return false;
    }
}

//FUNCION PARA AGREGAR O ELIMINAR SECCIONES EN EL MANTENIMIENTO
//==========================================================================================
function MantenimientoSecciones() {
    $(document).ready(function () {
        $.ajax({
            type: 'post',
            url: '/Configuracion/ObtenerSecciones',
            data: {},
            dataType: 'json',
            success: function (result) {
                if (result == "error") {
                    Swal.fire(
                        'Error!',
                        'Ocurrio un error al obtener las secciones',
                        'error'
                    );
                } else {
                    // Limpiamos la tabla antes de agregar nuevas filas
                    $('#tbVerSecciones').empty();

                    // Recorremos el objeto resultante y llenamos la tabla
                    result.forEach(function (seccion, index) {
                        var index = index + 1;

                        // Agregar la sección principal
                        $('#tbVerSecciones').append(
                            `<tr id="section-${index}" class="table table-bordered dt-responsive nowrap w-100">
                                <td style="width: 5%; text-align:center;">${index}</td> <!-- Columna para el índice -->
                                <th colspan="1" style="text-align:center;">${seccion.DESCRIPCION_SECCION}</th>
                                <td style="width: 10%; text-align:center;">
                                    <button type="button" class="btn btn-danger btn-sm" 
                                            id="btnEliminar-${seccion.CODIGO_SECCION}"
                                            onclick="eliminarSeccion('${seccion.CODIGO_SECCION}')">
                                        Eliminar
                                    </button>
                                </td> 
                            </tr>`
                        );
                        if (seccion.EXISTE_SUB_SECCION) {
                            $(`#btnEliminar-${seccion.CODIGO_SECCION}`).prop('disabled', true);
                        }
                    });
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                Swal.fire(
                    'Error!',
                    'Ocurrio un error',
                    'error'
                );
            }
        });
    });

    $('#nombreSeccion').val("");
    $('#divSeccion').modal('show');

}

// AGREGAR SECCIONES EN LA ADMINISTRACIÓN DE SECCIONES
//==========================================================================================
function btnAgregarSeccion() {
    // Obtener el valor del input
    var nombreSeccion = $('#nombreSeccion').val();

    // Validar que no esté vacío
    if (nombreSeccion.trim() === "") {
        Swal.fire(
            'Error!',
            'El nombre de la sección no puede estar vacío.',
            'error'
        );
        return;
    }

    $.ajax({
        method: 'POST',
        url: 'AgregarSeccion',
        data: { nombre: nombreSeccion },
        success: function (respuesta) {
            if (respuesta.success) {
                MantenimientoSecciones();

            } else {
                Swal.fire(
                    'Error!',
                    respuesta.message,
                    'error'
                );
            }
        },
        error: function () {
            Swal.fire(
                'Error!',
                'Ocurrió un error al agregar la solicitud.',
                'error'
            );
        }
    });
};

// ELIMINAR UNA SECCION DE LA ADMINISTRACION DE SECCIONES
//==========================================================================================
function eliminarSeccion(codigo) {
    $.ajax({
        method: 'POST',
        url: 'EliminarSeccion',
        data: { codigo: codigo },
        success: function (respuesta) {
            if (respuesta.success) {
                MantenimientoSecciones();

            } else {
                Swal.fire(
                    'Error!',
                    respuesta.message,
                    'error'
                );
            }
        },
        error: function () {
            Swal.fire(
                'Error!',
                'Ocurrió un error al agregar la solicitud.',
                'error'
            );
        }
    });
};

//AGREGAR UNA SECCION EN EL PANEL DE NUEVO CUESTIONARIO
//==========================================================================================
function btSeleccionarSeccion() {
    const select = document.getElementById('selectSeccion');

    if (select.value == '0') {
        Swal.fire(
            'Error!',
            'Por favor, seleccione una sección antes de continuar.',
            'error'
        );
    }
    else {
        let sectionId = `section-${select.value}`;
        // Agregar la sección principal
        $('#tbVerCuestionarioPreguntas').append(
            `<tr class="table-primary">
            <th colspan="2" style="text-align:center;">${select.options[select.selectedIndex].text}</th>
        </tr>
        <tr>
            <td colspan="2">
                <div id="${sectionId}" class="section-container">
                                
                    <div id="subsections-${sectionId}" class="subsections-container">
                                    
                    </div>
                    <button type="button" class="btnAddSubsection btn btn-add" data-section="${sectionId}"><span class="plus-sign">+</span> <span class="add-section">AGREGAR SUB SECCIÓN</span></button>
                    <hr>
                </div>
            </td>
        </tr>`
        );

        // Agregamos la nueva seccion al array
        let nuevaSeccion = {
            CODIGO_SECCION: parseInt(select.value),
            DESCRIPCION_SECCION: select.options[select.selectedIndex].text
        };
        seccionesArray.push(nuevaSeccion);

        // Quitar la opción seleccionada del select
        const optionToRemove = select.options[select.selectedIndex];
        select.remove(optionToRemove.index);

        // Seleccionar la opción predeterminada (posición 0)
        select.selectedIndex = 0;
    }
}

//FUNCION PARA GUARDAR EL CUESTIONARIO NUEVO
//==========================================================================================

async function GuardarCuestionarioNuevo() {
    let subseccionesSeleccionadas = [];

    const nombreCuestionario = $('#nombreCuestionario').val().trim();
    if (nombreCuestionario === '') {
        Swal.fire(
            'Error!',
            'Por favor, ingrese un nombre para el cuestionario.',
            'error'
        );
        return false;
    }

    $('.subsection-container').each(function () {
        let preguntas = [];

        const sectionId = $(this).closest('.section-container').attr('id');
        const subsectionTitle = $(this).find('.subsection-title').val();
        const codigoSeccion = sectionId.replace('section-', '');

        // Si el campo no está vacío, agregarlo al arreglo
        if (subsectionTitle.trim() !== '') {
            $(this).find('.question-input').each(function () {
                const descripcionPregunta = $(this).val().trim();

                // Si la pregunta no está vacía, agregarla al arreglo de preguntas
                if (descripcionPregunta !== '') {
                    preguntas.push({
                        DESCRIPCION: descripcionPregunta,
                        CODIGO_SUB_SECCION: 0
                    });
                }
                else {
                    Swal.fire(
                        'Error!',
                        'Debe llenar completamente todas las preguntas agregadas o eliminarlas.',
                        'error'
                    );
                    return false;
                }
            });

            subseccionesSeleccionadas.push({
                CODIGO_SUB_SECCION: 0,
                DESCRIPCION: subsectionTitle,
                CODIGO_SECCION: codigoSeccion,
                Preguntas_Cuestionarios: preguntas,
                NOMBRE_CUESTIONARIO: nombreCuestionario
            });
        }
        else {
            Swal.fire(
                'Error!',
                'Debe llenar completamente todas las sub secciones agregadas o eliminarlas',
                'error'
            );
            return false;
        }
    });

    // Mostrar el loader antes de iniciar la solicitud AJAX
    Swal.fire({
        title: "Guardando...",
        text: "Por favor, espere.",
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    const cuestionario = {
        NOMBRE_CUESTIONARIO: nombreCuestionario,
        SUB_SECCIONES: subseccionesSeleccionadas
    };

    $.ajax({
        method: 'POST',
        url: 'GuardarCuestionarioNuevo',
        data: JSON.stringify(cuestionario),
        contentType: 'application/json',
        success: function (respuesta) {
            if (respuesta.success == false) {
                Swal.fire(
                    'Error!',
                    respuesta.message,
                    'error'
                )
            } else {
                Swal.fire({
                    title: 'Guardado!',
                    text: respuesta.message,
                    icon: 'success',
                    didClose: () => {
                        location.reload();
                    }
                });
            }
        },
        error: function () {
            // Mostrar un mensaje de error al usuario si ocurre un error en la solicitud AJAX
            Swal.fire({
                title: 'Error!',
                text: 'Hubo un problema al procesar su solicitud.',
                icon: 'error',
                didClose: () => {

                }
            });
        }
    });

    console.log(subseccionesSeleccionadas);
}