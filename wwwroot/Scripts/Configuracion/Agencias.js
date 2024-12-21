// Obtiene el listado de las Agencias
GetAgencias();

function GetAgencias() {
    $(document).ready(function () {
        // MOSTRAMOS TODA LA TABLA DE LAS AGENCIAS
        $('#tbAgencias').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "ajax":
            {
                "url": '/Configuracion/GetAgencias',
                "type": "POST",
                "datatype": "json"
            },
            "deferRender": true,
            "columns": [
                {
                    "data": "codigO_AGENCIA", "render": function (data, type, row, meta) { return row.codigO_AGENCIA }, "name": "CODIGO AGENCIA", "autoWidth": true
                },
                {
                    "data": "nombrE_AGENCIA", "render": function (data, type, row, meta) { return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.nombrE_AGENCIA + "</div>"; }, "name": "NOMBRE AGENCIA", "autoWidth": true, "orderable": false
                },
                {
                    "data": "jefE_AGENCIA", "render": function (data, type, row, meta) { return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.jefE_AGENCIA + "</div>"; }, "name": "NOMBRE JEJE", "autoWidth": true, "orderable": false
                },
                {
                    "data": "codigO_AGENCIA",
                    "render": function (data, type, row, meta) {
                        let buttons = "<div class='optiongrid'>";

                        buttons += "<a href='javascript:void(0)' title='Editar Jefe' onClick='ModificarJefe(\"" + row.codigO_AGENCIA + "\")'> <i class='fas fa-pencil-alt'> </i> Modificar Jefe</a>";

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

//FUNCION PARA CARGAR EL MODAL DE MODIFICAR EL JEFE
//==========================================================================================
function ModificarJefe(codigo) {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetUsuarios',
        dataType: 'json',
        success: function (result) {
            let usuarios = "";

            result.forEach(function (usuario) {
                usuarios = usuarios + "<option value=" + usuario.codigO_USUARIO + ">" + usuario.nombrE_USUARIO + "</option>";
            });

            Swal.fire({
                title: 'Desea modificar el jefe de agencia?',
                html: `
                    <label for="jefeSelect">Selecciona el Jefe:</label>
                    <select id="jefeSelect" class="swal2-input">
                        <option value="" disabled selected>Selecciona</option>
                        `+ usuarios +`
                    </select>
                `,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Guardar',
                cancelButtonText: 'Cancelar',
                preConfirm: () => {
                    const jefeSeleccionado = document.getElementById('jefeSelect');
                    const valorJefe = jefeSeleccionado.value;
                    const textoJefe = jefeSeleccionado.options[jefeSeleccionado.selectedIndex].text;

                    if (!valorJefe) {
                        Swal.showValidationMessage('Por favor, selecciona un jefe de agencia');
                    }
                    return textoJefe;
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    const jefeSeleccionado = result.value;

                    $.ajax({
                        type: 'post',
                        url: '/Configuracion/ModificarJefeAgencia',
                        data: {
                            codigo: codigo,
                            nombreJefe: jefeSeleccionado
                        },
                        dataType: 'json',
                        success: function (result) {
                            if (result == "error") {
                                Swal.fire(
                                    'Error!',
                                    'Ocurrio un error al guardar el registro',
                                    'error'
                                )
                            } else {
                                Swal.fire({
                                    icon: 'success',
                                    title: 'Guardado',
                                    text: `El registro ha sido guardado exitosamente.`,
                                }).then(() => {
                                    $('#tbAgencias').DataTable().ajax.reload();
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
                } else {
                    Swal.fire({
                        icon: 'info',
                        title: 'Cancelado',
                        text: 'No se guardaron los cambios.',
                    });
                }
            });
        },
        error: function (xhr, textStatus, errorThrown) {
            Swal.fire(
                'Error!',
                'Sus agencias no se pueden obtener  ' + xhr.responseText,
                'error'
            )
        }
    });
}