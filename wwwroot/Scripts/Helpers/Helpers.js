//FUNCION PARA CARGAR TODAS LAS AGENCIAS
//==========================================================================================
function CargarAgencias() {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetAgencias',
        dataType: 'json',
        success: function (result) {
            let selectAgencias = document.getElementById("selectAgencias");

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Sus agencias no se pueden obtener',
                    'error'
                )
            }
            else {
                result.forEach(function (agencia) {
                    let opcion = document.createElement("option");
                    opcion.value = agencia.codigO_AGENCIA;
                    opcion.text = agencia.codigO_AGENCIA + " - " + agencia.nombrE_AGENCIA;
                    selectAgencias.appendChild(opcion);
                });
            }
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

//FUNCION PARA CARGAR TODAS LAS ROLES
//==========================================================================================
function CargarRoles() {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetRoles',
        dataType: 'json',
        success: function (result) {
            let selectRoles = document.getElementById("selectRoles");

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Sus roles no se pueden obtener',
                    'error'
                )
            }
            else {
                result.forEach(function (rol) {
                    let opcion = document.createElement("option");
                    opcion.value = rol.codigO_ROL;
                    opcion.text = rol.codigO_ROL + " - " + rol.nombrE_ROL;
                    selectRoles.appendChild(opcion);
                });
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            Swal.fire(
                'Error!',
                'Sus roles no se pueden obtener  ' + xhr.responseText,
                'error'
            )
        }
    });
}

//FUNCION PARA CARGAR TODOS LOS CARGOS
//==========================================================================================
function CargarCargos() {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetCargos',
        dataType: 'json',
        success: function (result) {
            let selectCargos = document.getElementById("selectCargos");

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Sus cargos no se pueden obtener',
                    'error'
                )
            }
            else {
                result.forEach(function (cargo) {
                    let opcion = document.createElement("option");
                    opcion.value = cargo.codigO_CARGO;
                    opcion.text = cargo.descripcion;
                    selectCargos.appendChild(opcion);
                });
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            Swal.fire(
                'Error!',
                'Sus cargos no se pueden obtener  ' + xhr.responseText,
                'error'
            )
        }
    });
}

//FUNCION PARA CARGAR TODAS UNIVERSO AUDITABLE
//==========================================================================================
function CargarUniversoAuditable() {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetUniversoAuditable',
        dataType: 'json',
        success: function (result) {
            let selectAgencias = document.getElementById("universoAuditable");

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Los Universos Auditables no se pueden obtener',
                    'error'
                )
            }
            else {
                result.forEach(function (agencia) {
                    let opcion = document.createElement("option");
                    opcion.value = agencia.codigO_UNIVERSO_AUDITABLE;
                    opcion.text = agencia.codigO_UNIVERSO_AUDITABLE + ' - ' + agencia.nombre;
                    selectAgencias.appendChild(opcion);
                });
            }
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


//FUNCION PARA CARGAR TODAS UNIVERSO AUDITABLE
//==========================================================================================
function CargarTiposAuditorias() {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetTiposAuditoria',
        dataType: 'json',
        success: function (result) {
            let selectTiposAud = document.getElementById("tipoAuditoria");

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

//FUNCION PARA CARGAR TODOS LOS AUDITORES QUE SERAN PARTE DE LA AUDITORIA
//==========================================================================================
function CargarAuditores() {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetAuditores',
        dataType: 'json',
        success: function (result) {
            let SelectAuditores = document.getElementById("equipoAuditores");

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'No se pudieron obtener los auditores',
                    'error'
                )
            }
            else {
                result.forEach(function (tipo) {
                    let opcion = document.createElement("option");
                    opcion.value = tipo.codigO_USUARIO;
                    opcion.text = tipo.nombrE_USUARIO;
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

//FUNCION PARA CARGAR OPCIONES DE AUDITORIA PROGRAMADA O NO PROGRAMADA
//==========================================================================================
function CargarProgramadaNoProgramada() {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetProgramadasNoProgramadas',
        dataType: 'json',
        success: function (result) {
            let auditoriaProgramada = document.getElementById("auditoriaProgramada");

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'No se pudieron obtener los tipos de auditorias',
                    'error'
                )
            }
            else {
                result.forEach(function (tipo) {
                    let opcion = document.createElement("option");
                    opcion.value = tipo.codigO_TIPO_AUDITORIA;
                    opcion.text = tipo.nombrE_TIPO_AUDITORIA;
                    auditoriaProgramada.appendChild(opcion);
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


//FUNCION PARA CARGAR LOS AUDITORES QUE SERAN ENCARGADOS DE UNA AUDITORIA
//==========================================================================================
function CargarAuditoresEncargados() {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetAuditores',
        dataType: 'json',
        success: function (result) {
            let SelectAuditores = document.getElementById("encargadoAuditoria");

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'No se pudieron obtener los auditores',
                    'error'
                )
            }
            else {
                result.forEach(function (tipo) {
                    let opcion = document.createElement("option");
                    opcion.value = tipo.codigO_USUARIO;
                    opcion.text = tipo.nombrE_USUARIO;
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