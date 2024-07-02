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
                    "data": "nombrE_ROL", "render": function (data, type, row, meta) { return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.nombrE_ROL + "</div>"; }, "name": "NOMBRE ROL", "autoWidth": true, "orderable": false
                },
                {
                    "data": "codigO_ROL",
                    "render": function (data, type, row, meta) {
                        let buttons = "<div class='optiongrid'>";

                        buttons += "<a href='javascript:void(0)' title='Editar Rol' onClick='ModificarRol(\"" + row.codigO_ROL + "\", \"" + row.nombrE_ROL + "\")'> <i class='fas fa-pencil-alt'> </i> Modificar</a> ";

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

//==========================================================================================

$('.checkLecEsc').on('change', function () {
    var isChecked = $(this).prop('checked'); // Obtener el estado del checkbox (true o false)
    var codigoSubmenu = parseInt($(this).attr('data')); // Obtener el ID del submenu
    var permissionType = $(this).data('permission-type'); // Obtener el tipo de permiso

    var existingObjectIndex = $accesosSubMenus.findIndex(function (item) {
        return item.CODIGO_SUB_MENU === codigoSubmenu;
    });

    if (existingObjectIndex !== -1) {
        // Actualizar el objeto existente con la nueva propiedad
        $accesosSubMenus[existingObjectIndex][permissionType] = isChecked ? 1 : null;
    }
    else {
        // El checkbox está marcado, por lo que se presionó
        var nuevoRegistro = { CODIGO_SUB_MENU: codigoSubmenu };
        nuevoRegistro[permissionType] = 1;
        // Agregar el nuevo objeto al array
        $accesosSubMenus.push(nuevoRegistro);
    }
});

//==========================================================================================

$('.checkSubMenu').on('click', function () {
    var $fila = $(this).closest('.row'); // Obtener la fila padre
    var isChecked = $(this).prop('checked');
    var codigoSubmenu = parseInt($(this).attr('data')); // Obtener el ID del submenu

    if (!isChecked) {
        var existingObjectIndex = $accesosSubMenus.findIndex(function (item) {
            return item.CODIGO_SUB_MENU === codigoSubmenu;
        });
        $accesosSubMenus.splice(existingObjectIndex, 1);
    }
    // Buscar checkboxes secundarios dentro de la fila y ajustar su estado
    $fila.find('.checkLecEsc').each(function () {
        var $checkbox = $(this);
        if (isChecked) {
            // Activar el checkbox y recordar su estado original
            $checkbox.prop('disabled', $checkbox.hasClass('disabledCheck'));

            var checkboxId = $checkbox.attr('id');
            if (checkboxId == "leer" + codigoSubmenu) {
                $checkbox.prop('checked', true);
                
                $checkbox.prop('disabled', true);
                // El checkbox está marcado, por lo que se presionó
                var nuevoRegistro = { CODIGO_SUB_MENU: codigoSubmenu };
                nuevoRegistro["LECTURA"] = 1;
                // Agregar el nuevo objeto al array
                $accesosSubMenus.push(nuevoRegistro);
            }
            else {
                $checkbox.prop('checked', false);
            }
        } else {
            // Desactivar el checkbox y recordar su estado original
            $checkbox.prop('disabled', true);
            $checkbox.prop('checked', false);
        }
    });


});

//FUNCION PARA GUARDAR EL ROL NUEVO
//==========================================================================================

async function GuardarRolNuevo() {
    $nombreRol = $("#nombreRol").val();
    $valorCodigoRol = $("#codigoRol").val();

    $resp = await validarCodigoYNombreRolDuplicado($valorCodigoRol, $nombreRol);
    $resp2 = await validacionesRol();

    if ($resp == false && $resp2 == false) {
        Swal.showLoading();

        $.ajax({
            method: 'POST',
            url: 'Guardar_Rol_Nuevo',
            data: { NOMBRE_ROL: $nombreRol, ACCESOS: JSON.stringify($accesosSubMenus), CODIGO_ROL: $valorCodigoRol },
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
                    var nombre = menuItem.submenus.nombre;
                    if (nombre != "Index") {
                        //Chequeamos el elemento que tenemos guardado
                        document.getElementById(nombre).checked = true;

                        //Creamos el objeto que guardara nuestro registro
                        var nuevoRegistro = { CODIGO_SUB_MENU: menuItem.codigO_SUB_MENU };

                        //Obtenemos las referencias a los componentes
                        var leer = document.getElementById('leer' + menuItem.codigO_SUB_MENU);
                        var crear = document.getElementById('crear' + menuItem.codigO_SUB_MENU);
                        var modificar = document.getElementById('modificar' + menuItem.codigO_SUB_MENU);
                        var autorizar = document.getElementById('autorizar' + menuItem.codigO_SUB_MENU);
                        var eliminar = document.getElementById('eliminar' + menuItem.codigO_SUB_MENU);

                        //Chequeamos el check necesario y llenamos el objeto
                        if (menuItem.lectura != null) {
                            leer.checked = true;
                            nuevoRegistro['LECTURA'] = 1;
                        }
                        if (menuItem.crear != null) {
                            crear.checked = true;
                            nuevoRegistro['CREAR'] = 1;
                        }
                        if (menuItem.modificar != null) {
                            modificar.checked = true;
                            nuevoRegistro['MODIFICAR'] = 1;
                        }
                        if (menuItem.autorizar != null) {
                            autorizar.checked = true;
                            nuevoRegistro['AUTORIZAR'] = 1;
                        }
                        if (menuItem.eliminar != null) {
                            eliminar.checked = true;
                            nuevoRegistro['ELIMINAR'] = 1;
                        }

                        //Guardamos en el objeto el registro que hemos llenado en nuestro objeto
                        $accesosSubMenus.push(nuevoRegistro);

                        //Habilitamos todos los check
                        leer.disabled = true;
                        crear.disabled = false;
                        modificar.disabled = false;
                        autorizar.disabled = false;
                        eliminar.disabled = false;
                    }
                    else {
                        //document.getElementById('codigoRol').textContent = menuItem.codigO_ROL;
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

    if ($resp == false && $resp2 == false) {
        Swal.showLoading();

        $.ajax({
            method: 'POST',
            url: 'Guardar_Rol_Editado',
            data: { NOMBRE_ROL: $nombreRol, ACCESOS: JSON.stringify($accesosSubMenus), CODIGO_ROL: $valorCodigoRol },
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

function validarCodigoYNombreRolDuplicado($idRol, $nombreRol) {
    if ($idRol != "" && $nombreRol != "") {
        return new Promise((resolve, reject) => {
            //Validamos que el nombre del rol no exista
            $.ajax({
                method: 'POST',
                url: 'VerificarCodigoYNombreRol',
                data: { CODIGO_ROL: $idRol, NOMBRE_ROL: $nombreRol },
                success: function (respuesta) {
                    if (respuesta == 'codigoRol') {
                        $("#validacionRol").text("El codigo del rol ya existe");
                        document.getElementById('validacionRol').style.display = 'block';
                        resolve(true);
                    }
                    else if (respuesta == 'nombreRol') {
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
//==========================================================================================

function validarNombreRolDuplicado($idRol, $nombreRol) {
    if ($nombreRol != "") {
        return new Promise((resolve, reject) => {
            //Validamos que el nombre del rol no exista
            $.ajax({
                method: 'POST',
                url: 'VerificarNombreRol',
                data: { NOMBRE_ROL: $nombreRol },
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

    if ($codigoRol.length == 0) {
        Swal.fire({
            title: "Informacion incompleta",
            text: "Debe digitar el codigo del rol",
            icon: "warning",
            confirmButtonText: 'Aceptar',
            allowEscapeKey: false,
            allowOutsideClick: false,
            showConfirmButton: true
        });

        return true;
    }
    else if ($nombre_rol.length == 0) {
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
    //Contamos el numero de checkbox que han sido seleccionados
    //Si el contador es 0 es porque no ha seleccionado ningun checbox y le pedira al usuario seleccionar al menos uno
    $('.contenedor  input[type=checkbox]:checked').each(function () {
        $contadorSubmenus++;
    });

    if ($contadorSubmenus == 0) {
        Swal.fire({
            title: "Informacion incompleta",
            text: "Seleccione al menos un acceso para el nuevo rol",
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
