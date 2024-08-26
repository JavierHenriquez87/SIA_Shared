// Obtiene el listado de los cuestionarios
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
                    // Mostramos el modal
                    $('#verCuestionarioModal').modal('show');
                    // Limpiamos la tabla antes de agregar nuevas filas
                    $('#tbVerCuestionarioPreguntas').empty();

                    // Recorremos el objeto resultante y llenamos la tabla
                    result.forEach(function (seccion, index) {
                        let sectionId = `section-${index + 1}`;

                        // Agregar la sección principal
                        $('#tbVerCuestionarioPreguntas').append(
                            `<tr class="table-primary">
                        <th colspan="2" style="text-align:center;">${seccion.DESCRIPCION_SECCION}</th>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <div id="${sectionId}" class="section-container">
                                
                                <div id="subsections-${sectionId}" class="subsections-container">
                                    
                                </div>
                                <button type="button" class="btnAddSubsection btn btn-primary" data-section="${sectionId}">Agregar Subseccion</button>
                                <hr>
                            </div>
                        </td>
                    </tr>`
                        );
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
            <hr>
        </div>
    `;

    $(`#subsections-${sectionId}`).append(newSubsectionHtml);
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


//FUNCION PARA GUARDAR EL CUESTIONARIO QUE SE HA EDITADO
//==========================================================================================

async function GuardarCuestionarioEditado() {
    
}

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