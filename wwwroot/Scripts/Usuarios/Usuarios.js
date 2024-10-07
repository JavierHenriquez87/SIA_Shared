// Obtiene la información de los usuarios
GetUsuarios();
CargarAgencias();
CargarRoles()
CargarCargos()

function GetUsuarios() {
    $(document).ready(function () {
        // MOSTRAMOS TODA LA TABLA DE LOS USUARIOS
        $('#tbUsuarios').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "ajax":
            {
                "url": '/Usuarios/GetUsuarios',
                "type": "POST",
                "datatype": "json"
            },
            "deferRender": true,
            "columns": [
                {
                    "data": "codigO_USUARIO", "render": function (data, type, row, meta) { return row.codigO_USUARIO }, "name": "CODIGO USUARIO", "autoWidth": true, "orderable": false
                },
                {
                    "data": "nombre", "render": function (data, type, row, meta) { return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.nombre + "</div>"; }, "name": "NOMBRE USUARIO", "autoWidth": true, "orderable": true
                },
                {
                    "data": "nombrE_ROL", "render": function (data, type, row, meta) { return row.nombrE_ROL }, "name": "NOMBRE ROL", "autoWidth": true, "orderable": false
                },
                {
                    "data": "codigO_ESTADO", "render": function (data, type, row, meta) { return row.codigO_ESTADO == 1 ? "ACTIVO" : "INACTIVO" }, "name": "ESTADO", "autoWidth": true, "orderable": false
                },
                {
                    "data": "codigO_USUARIO",
                    "render": function (data, type, row, meta) {
                        let buttons = "<div class='optiongrid'>";

                        buttons += "<a href='#' title='Editar Usuario' onClick='ModificarUsuario(\"" + row.codigO_USUARIO + "\", \"" + row.nombre + "\")'> <i class='fas fa-pencil-alt'> </i> Modificar</a> |";
                        
                        buttons += "<a href='#' title='Firma' onClick='ModificarFirma(\"" + row.codigO_USUARIO + "\", \"" + row.nombre + "\")'> <i class='fas fa-pencil-alt'> </i> Firma</a> ";

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
            responsive: "true",
        });
    });

};

//FUNCION PARA CARGAR EL MODAL DE NUEVO USUARIO
//==========================================================================================

function NuevoUsuario() {
    LimpiarModalUsuario();

    //Ocultamos el boton de guardar y mostramos editar
    document.getElementById('btnGuardarUsuario').style.display = 'block';
    document.getElementById('btnEditarUsuario').style.display = 'none';

    $('#codigoUsuarioEdit').prop('disabled', false);

    var modalTitle = document.getElementById('titleNuevoUsuario');
    modalTitle.textContent = 'Nuevo Usuario';

    //Mostramos el modal
    $('#divUsuario').modal('show');
}


//FUNCION PARA GUARDAR EL USUARIO NUEVO
//==========================================================================================

function GuardarUsuarioNuevo() {
    var codigoUsuario = $('#codigoUsuarioEdit').val();
    var nombreUsuario = $('#inputNombre').val();
    var claveUsuario = $('#inputClave').val();
    var emailUsuario = $('#inputEmail').val();
    var numeroIdentidad = $('#inputNumeroIdentidad').val();
    var agenciaSeleccionada = $('#selectAgencias').val();
    var rolSeleccionado = $('#selectRoles').val();
    var cargoSeleccionado = $('#selectCargos').val();
    var estadoSeleccionado = $('#selectEstado').val();

    // Objeto que contiene los datos a enviar
    var userData = {
        CODIGO_USUARIO: codigoUsuario,
        CODIGO_AGENCIA: agenciaSeleccionada,
        CODIGO_ROL: rolSeleccionado,
        CODIGO_CARGO: cargoSeleccionado,
        ESTADO: estadoSeleccionado,
        NUMERO_IDENTIDAD: numeroIdentidad,
        NOMBRE_USUARIO: nombreUsuario,
        EMAIL: emailUsuario,
        CLAVE_USUARIO: claveUsuario
    };

    $.ajax({
        url: 'Guardar_Usuario',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(userData),
        success: function (result) {
            $('#divUsuario').modal('hide');

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Su registro no se puede editar',
                    'error'
                )
            }
            else {
                $('#tbUsuarios').dataTable().fnDestroy();
                GetUsuarios();

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

//FUNCION PARA CARGAR LA INFORMACION DEL USUARIO
//==========================================================================================

function ModificarUsuario(idUsuario, nombreUsuario) {
    $.ajax({
        type: 'ajax',
        method: 'post',
        url: 'Informacion_Usuario',
        data: {
            idUsuario: idUsuario,
        },
        dataType: 'json',
        success: function (result) {
            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Su registro no se puede editar',
                    'error'
                )
            }
            else {
                LimpiarModalUsuario();

                //Ocultamos el boton de guardar y mostramos editar
                document.getElementById('btnGuardarUsuario').style.display = 'none';
                document.getElementById('btnEditarUsuario').style.display = 'block';

                $('#codigoUsuarioEdit').prop('disabled', true);

                var modalTitle = document.getElementById('titleNuevoUsuario');
                modalTitle.textContent = 'Editar Usuario';

                //Limpiamos y agregamos la informacion
                $('#codigoUsuarioEdit').val(idUsuario);
                $('#inputNombre').val(nombreUsuario);
                //$('#inputEmail').val(result.email);
                //$('#inputNumeroIdentidad').val(result.numerO_IDENTIDAD);
                //$('#selectAgencias').val(result.codigO_AGENCIA);
                $('#selectRoles').val(result.codigO_ROL);
                //$('#selectCargos').val(result.codigO_CARGO);
                $('#selectEstado').val(result.codigO_ESTADO);

                //Mostramos el modal
                $('#divUsuario').modal('show');
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

//FUNCION PARA GUARDAR EL USUARIO QUE SE HA EDITADO
//==========================================================================================

function GuardarUsuarioEditado() {
    var codigoUsuario = $('#codigoUsuarioEdit').val();
    var nombreUsuario = $('#inputNombre').val();
    var claveUsuario = $('#inputClave').val();
    var emailUsuario = $('#inputEmail').val();
    var numeroIdentidad = $('#inputNumeroIdentidad').val();
    var agenciaSeleccionada = $('#selectAgencias').val();
    var rolSeleccionado = $('#selectRoles').val();
    var cargoSeleccionado = $('#selectCargos').val();
    var estadoSeleccionado = $('#selectEstado').val();

    // Objeto que contiene los datos a enviar
    var userData = {
        CODIGO_USUARIO: codigoUsuario,
        //CODIGO_AGENCIA: agenciaSeleccionada,
        CODIGO_ROL: rolSeleccionado,
        //CODIGO_CARGO: cargoSeleccionado,
        //CODIGO_ESTADO: estadoSeleccionado,
        //NUMERO_IDENTIDAD: numeroIdentidad,
        //NOMBRE_USUARIO: nombreUsuario,
        //EMAIL: emailUsuario,
        //CLAVE_USUARIO: claveUsuario
    };

    $.ajax({
        url: 'Editar_Usuario',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(userData),
        success: function (result) {
            $('#divUsuario').modal('hide');

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Su registro no se puede editar',
                    'error'
                )
            }
            else {
                $('#tbUsuarios').dataTable().fnDestroy();
                GetUsuarios();

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

//FUNCION PARA LIMPIAR EL FORMULARIO DE NUEVO USUARIO O EDITAR USUARIO
//==========================================================================================

function LimpiarModalUsuario() {
    $('#codigoUsuarioEdit').val("");
    $('#inputNombre').val("");
    $('#inputClave').val("");
    $('#inputEmail').val("");
    $('#inputNumeroIdentidad').val("");
    $('#selectAgencias').val("");
    $('#selectRoles').val("");
    $('#selectCargos').val("");
    $('#selectEstado').val(1);
}

//FUNCION PARA SUBIR LA FIRMA
//==========================================================================================

function ModificarFirma(idUsuario, nombreUsuario) {
    $.ajax({
        type: 'ajax',
        method: 'post',
        url: 'Informacion_Usuario',
        data: {
            idUsuario: idUsuario,
        },
        dataType: 'json',
        success: function (result) {
            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Su registro no se puede editar',
                    'error'
                )
            }
            else {
                //Limpiamos y agregamos la informacion
                $('#codigoUsuarioFirma').val(idUsuario);
                $('#inputNombreFirma').val(nombreUsuario);

                var firmaImg = document.getElementById('firmaValidador');

                if (result.firma == null)
                    firmaImg.src = '/assets/images/sinfirma.png';
                else
                    firmaImg.src = result.firma + '?' + new Date().getTime();

                //Mostramos el modal
                $('#divUsuarioFirma').modal('show');
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

//OCULTAR Y MOSTRAR LA CONTRASEÑA
//==========================================================================================

function togglePasswordVisibility() {
    var inputClave = document.getElementById('inputClave');
    var showHidePasswordButton = document.getElementById('showHidePassword');

    if (inputClave.type === 'password') {
        inputClave.type = 'text';
        showHidePasswordButton.textContent = 'Ocultar';
    } else {
        inputClave.type = 'password';
        showHidePasswordButton.textContent = 'Mostrar';
    }
}

//SELECCIONAR LA FIRMA
//==========================================================================================

function seleccionarFirma() {
    var fileInput = document.getElementById('fileInput');
    fileInput.click();
}

//MOSTRAR LA FIRMA EN EL CAMPO
//==========================================================================================

function mostrarFirma(event) {
    var file = event.target.files[0];
    if (file) {
        var firmaImg = document.getElementById('firmaValidador');
        firmaImg.src = URL.createObjectURL(file);
    }
}

//MOSTRAR LA FIRMA EN EL CAMPO
//==========================================================================================

function GuardarFirma() {
    // Obtener la URL de la imagen
    var urlImagen = document.getElementById('firmaValidador').src;
    if (urlImagen.endsWith('/assets/images/sinfirma.png')) {
        Swal.fire(
            'Error!',
            'Registro no guardado debes cargar una firma.',
            'error'
        )
    }
    else {
        // Crear un objeto FormData
        var formData = new FormData();
        // Agregar la imagen al objeto FormData
        formData.append('firma', $('#fileInput')[0].files[0]); 
        formData.append('codigoUsuarioFirma', $('#codigoUsuarioFirma').val());

        $.ajax({
            method: 'POST',
            url: 'GuardarFirmaUsuario',
            data: formData,
            processData: false,
            contentType: false,
            success: function (respuesta) {
                if (respuesta == "success") {
                    $('#tbUsuarios').dataTable().fnDestroy();
                    GetUsuarios();

                    $('#divUsuarioFirma').modal('hide');
                    Swal.fire(
                        'Registro guardado!',
                        'Registro guardado con exito.',
                        'success'
                    )
                }
                else {
                    Swal.fire(
                        'Error!',
                        'Su registro no se pudo guardar',
                        'error'
                    )
                }
            }
        });
    }
}