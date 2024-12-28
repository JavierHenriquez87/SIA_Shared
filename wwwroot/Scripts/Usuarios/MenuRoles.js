// Obtiene la información de los usuarios
GetRoles();

function GetRoles() {
    $(document).ready(function () {
        // MOSTRAMOS TODA LA TABLA DE LOS USUARIOS
        $('#tbRoles').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "ajax":
            {
                "url": '/Usuarios/GetRoles',
                "type": "POST",
                "datatype": "json"
            },
            "deferRender": true,
            "columns": [
                {
                    "data": "codigO_ROL", "render": function (data, type, row, meta) { return row.codigO_ROL }, "name": "CODIGO ROL", "autoWidth": true
                },
                {
                    "data": "nombre", "render": function (data, type, row, meta) { return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.nombre + "</div>"; }, "name": "NOMBRE ROL", "autoWidth": true, "orderable": false
                },
                {
                    "data": "codigO_ROL",
                    "render": function (data, type, row, meta) {
                        let buttons = "<div class='optiongrid'>";

                        buttons += "<a href='javascript:void(0)' title='Editar Rol' onClick='ModificarRol(\"" + row.codigO_ROL + "\", \"" + row.nombre + "\")'> <i class='fas fa-pencil-alt'> </i> Modificar</a> ";

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

function NuevoRol() {

    //Ocultamos el boton de guardar y mostramos editar
    document.getElementById('btnGuardarRol').style.display = 'block';
    document.getElementById('btnEditarRol').style.display = 'none';

    $('#codigoRol').prop('disabled', false);

    var modalTitle = document.getElementById('titleNuevoRol');
    modalTitle.textContent = 'Nuevo Rol';

    //Ponemos todos los check como inactivos
    document.querySelectorAll('.contenedor input[type=checkbox]').forEach(function (checkElement) {
        checkElement.checked = false;
    });

    //Deshabilitamos todos los checkbox de permisos
    document.querySelectorAll('.disabledCheck input[type=checkbox]').forEach(function (checkElementSubmenu) {
        checkElementSubmenu.disabled = true;
    });

    //Ocultamos el boton de guardar y mostramos editar
    document.getElementById('btnGuardarRol').style.display = 'block';
    document.getElementById('btnEditarRol').style.display = 'none';

    $('#nombreRol').val("");
    $('#codigoRol').val("");

    $("#validacionRol").text("");
    document.getElementById('validacionRol').style.display = 'none';

    //Mostramos el modal
    $('#divRol').modal('show');
}


//FUNCION PARA GUARDAR EL ROL NUEVO
//==========================================================================================

async function GuardarRol() {
    $nombreRol = $("#nombreRol").val();

    $resp = await validarNombreRolDuplicado($nombreRol);
    $resp2 = await validacionesRol();

    var menuSeleccionados = [];
    var checkboxes = document.querySelectorAll('.checkSubMenu');

    checkboxes.forEach(function (checkbox) {
        if (checkbox.checked) {
            menuSeleccionados.push({
                CODIGO_APLICACION: "SIA",
                CODIGO_MENU: checkbox.getAttribute('data'),
            });
        }
    });

    if ($resp == false && $resp2 == false) {
        Swal.showLoading();

        $.ajax({
            method: 'POST',
            url: 'GuardarMenuRol',
            data: { NOMBRE_ROL: $nombreRol, ACCESOS: JSON.stringify(menuSeleccionados) },
            success: function (respuesta) {
                if (respuesta == "agregado") {
                    //Ocultamos el modal
                    $('#divRol').modal('hide');

                    //Borramos la data del dataTable
                    $('#tbRoles').dataTable().fnDestroy();
                    //Volvemos a cargar la tabla con el nuevo dato
                    GetRoles();

                    Swal.fire(
                        'Guardado!',
                        'Registro guardado con exito.',
                        'success'
                    )
                } else {
                    Swal.fire(
                        'Error!',
                        'Su registro no se pudo guardar.',
                        'error'
                    )
                }
            }
        });
    }
}


//FUNCION PARA CARGAR LA INFORMACION DEL ROL
//==========================================================================================

function ModificarRol(idRol, nombreRol) {
    $.ajax({
        type: 'ajax',
        method: 'post',
        url: 'Editar_Rol',
        data: {
            idRol: idRol,
        },
        dataType: 'json',
        success: function (result) {
            if (result.message == "success") {
                var modalTitle = document.getElementById('titleNuevoRol');
                modalTitle.textContent = 'Editar Rol ' + nombreRol;

                //Array donde se guardaran todos los accesos que asignen al rol
                $accesosSubMenus = [];

                //Ponemos todos los check como inactivos
                document.querySelectorAll('.contenedor input[type=checkbox]').forEach(function (checkElement) {
                    checkElement.checked = false;
                });

                //Deshabilitamos todos los checkbox de permisos
                document.querySelectorAll('.disabledCheck input[type=checkbox]').forEach(function (checkElementSubmenu) {
                    checkElementSubmenu.disabled = true;
                });

                //Recorremos el menu que nos retorna
                var menu = result.menu;
                for (var i = 0; i < menu.length; i++) {
                    var menuItem = menu[i];
                    // Buscar el checkbox por el código de menú y marcarlo
                    var checkbox = document.querySelector('[data="' + menuItem.codigO_MENU + '"]');

                    if (checkbox) {
                        checkbox.checked = true;
                    }
                }

                //Ocultamos el boton de guardar y mostramos editar
                document.getElementById('btnGuardarRol').style.display = 'none';
                document.getElementById('btnEditarRol').style.display = 'block';

                //Limpiamos y agregamos el nombre del rol y mostramos el modal
                $('#nombreRol').val(nombreRol);
                $('#codigoRol').val(idRol);
                $('#nombreRolEdit').val(nombreRol);

                var codigoRolInput = document.getElementById('codigoRol');
                codigoRolInput.disabled = true;

                $("#validacionRol").text("");
                document.getElementById('validacionRol').style.display = 'none';

                $('#divRol').modal('show');
            }
            else {
                Swal.fire(
                    'Error!',
                    'Su registro no se puede editar',
                    'error'
                )
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            Swal.fire(
                'Error!',
                'Error! ' + xhr.responseText,
                'error'
            )
        }
    });
}


//FUNCION PARA GUARDAR EL ROL QUE SE HA EDITADO
//==========================================================================================

async function GuardarRolEditado() {
    $nombreRol = $("#nombreRol").val();
    $valorCodigoRol = $("#codigoRol").val();
    $nombreRolEdit = $("#nombreRolEdit").val();

    $resp = false;
    if ($nombreRol != $nombreRolEdit) {
        $resp = await validarNombreRolDuplicado($valorCodigoRol, $nombreRol);
    }

    $resp2 = await validacionesRol();

    var menuSeleccionados = [];
    var checkboxes = document.querySelectorAll('.checkSubMenu');

    checkboxes.forEach(function (checkbox) {
        if (checkbox.checked) {
            menuSeleccionados.push({
                CODIGO_APLICACION: "SIA",
                CODIGO_MENU: checkbox.getAttribute('data'),
                CODIGO_ROL: $valorCodigoRol
            });
        }
    });

    if ($resp == false && $resp2 == false) {
        Swal.showLoading();

        $.ajax({
            method: 'POST',
            url: 'GuardarRolEditado',
            data: { NOMBRE_ROL: $nombreRol, ACCESOS: JSON.stringify(menuSeleccionados), CODIGO_ROL: $valorCodigoRol },
            success: function (respuesta) {
                if (respuesta == "agregado") {
                    //Ocultamos el modal
                    $('#divRol').modal('hide');
                    //Borramos los array donde se guardaron los submenus
                    $accesosSubMenus = [];
                    //Borramos la data del dataTable
                    $('#tbRoles').dataTable().fnDestroy();
                    //Volvemos a cargar la tabla con el nuevo dato
                    GetRoles();

                    Swal.fire(
                        'Guardado!',
                        'Registro guardado con exito.',
                        'success'
                    )
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

function validarNombreRolDuplicado($nombreRol) {
    if ($nombreRol != "") {
        return new Promise((resolve, reject) => {
            //Validamos que el nombre del rol no exista
            $.ajax({
                method: 'POST',
                url: 'VerificarNombreRolMenu',
                data: { NOMBRE: $nombreRol },
                success: function (respuesta) {
                    if (respuesta == 'nombreRol') {
                        $("#validacionRol").text("El nombre del rol ya existe");
                        document.getElementById('validacionRol').style.display = 'block';
                        resolve(true);
                    }
                    else {
                        $("#validacionRol").text("");
                        document.getElementById('validacionRol').style.display = 'none';
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

function validacionesRol() {
    //Obtenemos el valor del input del nombre del rol y si esta vacio se le solicita al usuario agregar un nombre de rol
    $codigoRol = $("#codigoRol").val();
    $nombre_rol = $("#nombreRol").val();
    $contadorSubmenus = 0;

    if ($nombre_rol.length == 0) {
        Swal.fire({
            title: "Informacion incompleta",
            text: "Debe digitar el nombre del rol",
            icon: "warning",
            confirmButtonText: 'Aceptar',
            allowEscapeKey: false,
            allowOutsideClick: false,
            showConfirmButton: true
        });

        return true;
    }

    var seleccionados = 0;
    var checkboxes = document.querySelectorAll('.checkSubMenu');

    checkboxes.forEach(function (checkbox) {
        if (checkbox.checked) {
            seleccionados++;
        }
    });

    if (seleccionados == 0) {
        Swal.fire({
            title: "Informacion incompleta",
            text: "Seleccione al menos un acceso para el rol",
            icon: "warning",
            confirmButtonText: 'Aceptar',
            allowEscapeKey: false,
            allowOutsideClick: false,
            showConfirmButton: true
        });

        return true;
    }

    return false;
}
