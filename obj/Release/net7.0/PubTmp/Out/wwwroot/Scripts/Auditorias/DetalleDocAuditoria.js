GeAuditoriasEspecEdit();
//Llama a la funcion en helpers.js que obtiene los auditores encargados
CargarAuditoresEncargados();
CargarTiposAuditorias();
CargarAuditoresAsignados();

async function AgregarComentario() {
    // Objeto que contiene los datos a enviar
    var DataAI = {
        NOTA: $('#comentarioMDP').val()
    };

    var val = false;

    if (val == true) {
        Swal.fire({
            title: 'Error',
            text: "Para agregar un comentario debe digitar una nota.",
            icon: 'warning',
            showCancelButton: false,
            confirmButtonColor: '#2A3042',
            confirmButtonText: 'Ok'
        })

    } else {
        Swal.showLoading();

        $.ajax({
            method: 'POST',
            url: '/Auditorias/Guardar_ComentarioMDP',
            data: JSON.stringify(DataAI),
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
                        title: "Se guardo el comentario con éxito."
                    });


                    $('#comentarios-list').html(respuesta);
                    $('#comentarioMDP').val('');
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

async function AprobarMDP() {
    var CODIGO_MEMORANDUM = $('#codigoMemorandum').val();

    Swal.fire({
        title: 'Aprobación de Memorándum',
        text: "¿Desea aprobar este memorándum de planificación?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#34c38f',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Aprobar',
        cancelButtonText: `Cancelar`,
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.showLoading();

            $.ajax({
                method: 'POST',
                url: '../AprobarMDP',
                data: {
                    codigo_memorandum: CODIGO_MEMORANDUM
                },
                dataType: 'json',
                success: function (respuesta) {
                    if (respuesta == "error") {
                        Swal.fire({
                            title: 'Error',
                            text: "Ocurrió un error al intentar aprobar el memorandum de planificacion",
                            icon: 'error',
                            showCancelButton: false,
                            confirmButtonColor: '#2A3042',
                            confirmButtonText: 'Ok'
                        })
                    } else {
                        Swal.fire({
                            title: 'Guardado!',
                            text: 'Se aprobó el memorándum de planificación con éxito!',
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
    })
}

async function RegresarMDP() {
    var CODIGO_MEMORANDUM = $('#codigoMemorandum').val();

    Swal.fire({
        title: 'Regresar Memorándum',
        text: "¿Desea regresar al auditor el memorándum de planificación para que realice cambios? \n\n\n Asegurese de haber agregado comentarios en la parte inferior de esta ventana, para que los auditores conozcan los cambios que solicita.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#f1b44c',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Aceptar',
        cancelButtonText: `Cancelar`,
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.showLoading();

            $.ajax({
                method: 'POST',
                url: '../RegresarMDP',
                data: {
                    codigo_memorandum: CODIGO_MEMORANDUM
                },
                dataType: 'json',
                success: function (respuesta) {
                    if (respuesta == "error") {
                        Swal.fire({
                            title: 'Error',
                            text: "Ocurrió un error al intentar regresar el memorándum de planificación",
                            icon: 'error',
                            showCancelButton: false,
                            confirmButtonColor: '#2A3042',
                            confirmButtonText: 'Ok'
                        })
                    } else {
                        Swal.fire({
                            title: 'Guardado!',
                            text: 'Se regreso el memorándum de planificación con éxito!',
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
    })
}

function openSidebar() {
    var sidebar = document.getElementById("SidebarEditMDP");
    if (window.innerWidth <= 600) {
        sidebar.style.width = "90%";
    } else {
        sidebar.style.width = "800px";
    }
    document.getElementById("overlay").style.display = "block";
}

async function closeSidebarBtnSave() {
    var text = $('#texttipoauditoriaEdit').val();

    if (text.length == 0) {
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
            icon: "error",
            title: "Complete el campo de texto para la auditoría."
        });

        return
    } else {
        try {
            await EditarTextoAuditoria();
        } catch (error) {
            return;
        }
    }

        document.getElementById("SidebarEditMDP").style.width = "0";
        document.getElementById("overlay").style.display = "none";
    setTimeout(() => {
        window.location.reload();
    }, 800);


}

async function closeSidebar() {
    document.getElementById("SidebarEditMDP").style.width = "0";
    document.getElementById("overlay").style.display = "none";
    setTimeout(() => {
        window.location.reload();
    }, 500);


}

function openSidebarObjRec() {
    var sidebar = document.getElementById("sidebarEditObjRecMP");
    if (window.innerWidth <= 600) {
        sidebar.style.width = "90%";
    } else {
        sidebar.style.width = "800px";
    }
    document.getElementById("overlayObjRec").style.display = "block";
}

async function closeSidebarObjRecBtnSave() {
    var OBJETIVO_AUDITORIA = $('#objetivoAuditoriaEdit').val();
    var EQUIPO_TRABAJO = $('#equipoTrabajoEdit').val();
    var EQUIPO_AUDITORES = $('#selectequipoTrabajoEdit').val();
    var RECURSOS = $('#recursosEdit').val();
    var numeroMDPEdit = $('#numeroMDPEdit').val();
    var tiempoEdit = $('#tiempoEdit').val();
    var fechaInicioEdit = $('#fechaInicioEdit').val();
    var fechaFinEdit = $('#fechaFinEdit').val();

    if (objetivoAuditoriaEdit.length == 0 || equipoTrabajoEdit.length == 0 || recursosEdit.length == 0 || EQUIPO_AUDITORES.length == 0 || tiempoEdit.length == 0 || fechaInicioEdit.length == 0 || fechaFinEdit.length == 0) {
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
            icon: "error",
            title: "No puede dejar vacio el Objetivo, Equipo de Trabajo o Recursos de la Auditoría."
        });

        return
    } else {
        try {
            //Convertimos los auditores a string separado por ,
            // Convertir el array a un string separado por comas
            var equipoAuditoresString = EQUIPO_AUDITORES.join(',');

            Swal.showLoading();

            $.ajax({
                method: 'POST',
                url: '/Auditorias/editarObjEqRecursosMP',
                data: {
                    objetivo_auditoria: OBJETIVO_AUDITORIA,
                    equipo_trabajo: EQUIPO_TRABAJO,
                    equipo_auditores: equipoAuditoresString,
                    recursos: RECURSOS,
                    numeromdp: numeroMDPEdit,
                    tiempo_edit: tiempoEdit,
                    fecha_inicio: fechaInicioEdit,
                    fecha_fin: fechaFinEdit
                },
                dataType: 'json',
                success: function (respuesta) {
                    if (respuesta == "Ok") {
                        Swal.fire({
                            title: 'Guardado!',
                            text: 'Se actualizo con exito.',
                            icon: 'success',
                            didClose: () => {
                                document.getElementById("sidebarEditObjRecMP").style.width = "0";
                                document.getElementById("overlayObjRec").style.display = "none";
                                window.location.reload();
                            }
                        });
                    } else {
                        Swal.fire(
                            'Error!',
                            'Su registro no se pudo actualizar.',
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
        } catch (error) {
            return;
        }
    }
}

async function closeSidebarObjRecBtn() {
    document.getElementById("sidebarEditObjRecMP").style.width = "0";
    document.getElementById("overlayObjRec").style.display = "none";
    setTimeout(() => {
        window.location.reload();
    }, 500);
}

//OBTENEMOS LAS AUDITORIAS ESPECIFICAS PARA EDITAR
//==========================================================================================
async function GeAuditoriasEspecEdit() {
    $(document).ready(function () {
        var num_audit_integral = $('#numAudInteg').val();
        var anio_audit_integral = $('#anioAudInteg').val();
        // MOSTRAMOS TODA LA TABLA DE LAS AUDITORIAS ESPECIFICAS
        $('#tbEditAuditorias').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": false,
            "ajax":
            {
                "url": '/Auditorias/GetAuditoriasEspecificas',
                "type": "POST",
                data: {
                    num_auditoria_integral: num_audit_integral,
                    anio_auditoria_integral: anio_audit_integral
                },
                "datatype": "json"
            },
            "deferRender": true,
            "columns": [
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) { return row.codigO_AUDITORIA },
                    "name": "CODIGO",
                    "autoWidth": true
                },
                {
                    "data": "mg_tipos_de_auditorias.descripcion",
                    "render": function (data, type, row, meta) { return "<div class='tipo_auditoria'>" + row.mg_tipos_de_auditorias.descripcion + "</div>"; },
                    "name": "TIPO DE AUDITORIA",
                    "autoWidth": true,
                    "orderable": false
                },
                {
                    "data": "encargadO_AUDITORIA",
                    "render": function (data, type, row, meta) { return "<div class=''>" + row.encargadO_AUDITORIA + "</div>"; },
                    "name": "encargadO_AUDITORIA",
                    "autoWidth": true,
                    "orderable": false
                },
                {
                    "data": "numerO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        let buttons = "<div class='optiongrid'>";

                        buttons += "<a style='cursor: pointer; href='#' title='Editar Auditoría' onClick='EditarAuditoriaEspecEdit(\"" + row.numerO_AUDITORIA_INTEGRAL + "\", \"" + row.numerO_AUDITORIA + "\", \"" + row.encargadO_AUDITORIA + "\")'> <i class='far fa-edit'> </i> Editar</a>";

                        buttons += " | <a style='cursor: pointer; href='#' title='Borrar Auditoría' onClick='BorrarAuditoriaEspecEdit(\"" + row.numerO_AUDITORIA_INTEGRAL + "\", \"" + row.numerO_AUDITORIA + "\")'> <i class='fas fa-trash' style='color:red;'> </i> Borrar</a>";


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


//FUNCION PARA BORRAR UNA AUDITORIA ESPECIFICA CUANDO SE REGRESA EL MDP
//==========================================================================================
function BorrarAuditoriaEspecEdit(num_audit_integral, numero_auditoria) {
    Swal.fire({
        title: "¿Eliminar Auditoría?",
        text: "Esta seguro/a de eliminar la auditoría. La eliminación no se podrá revertir.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Si, borrar!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: 'ajax',
                method: 'post',
                url: '../BorrarAuditoriaEspecifica',
                data: {
                    num_audit_integral: num_audit_integral,
                    numero_auditoria: numero_auditoria,
                },
                dataType: 'json',
                success: function (result) {
                    if (result == "error") {
                        Swal.fire(
                            'Error!',
                            'No se pudo borrar el registro. Contacte con TI si el problema persiste.',
                            'error'
                        )
                    }
                    else {
                        // Destruimos la tabla con la informacion cargada de las auditorias especificas
                        $('#tbEditAuditorias').dataTable().fnDestroy();
                        //Obtenemos nuevamente las auditorias especificas actualizadas
                        GeAuditoriasEspecEdit();
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

async function EditarTextoAuditoria() {
    return new Promise((resolve, reject) => {
        var textAudit = $('#texttipoauditoriaEdit').val();
        var codigo_memo = $('#codigoMemorandum').val();

        $.ajax({
            method: 'POST',
            url: '../editarMP',
            data: {
                textAudit: textAudit,
                codigo_memo: codigo_memo,
            },
            dataType: 'json',
            success: function (respuesta) {

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


//FUNCION PARA CARGAR AUDITORES Y EL ENCARGADO DE LA AUDITORIA
//==========================================================================================
function CargarAuditoresMDPEditar(encargado_auditoria) {
    $.ajax({
        type: 'GET',
        url: '/Helpers/GetAuditores',
        dataType: 'json',
        success: function (result) {
            let SelectAuditores = document.getElementById("encargadoAuditoria");
            SelectAuditores.innerHTML = ""; // Limpiar el select antes de llenarlo

            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'No se pudieron obtener los auditores',
                    'error'
                )
            }
            else {
                result.forEach(function (data) {
                    let opcion = document.createElement("option");
                    opcion.value = data.codigO_USUARIO;
                    opcion.text = data.nombrE_USUARIO;
                    // Verificar si este es el auditor que debe estar seleccionado por defecto
                    if (data.codigO_USUARIO == encargado_auditoria) {
                        opcion.selected = true;
                    }
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

//FUNCION PARA CARGAR TODOS LOS AUDITORES QUE SERAN PARTE DE LA AUDITORIA
//==========================================================================================
function CargarAuditoresAsignados() {
    var num_audit_integral = $('#numAudInteg').val();
    var anio_audit_integral = $('#anioAudInteg').val();

    $.ajax({
        type: 'GET',
        url: '/Auditorias/GetAuditoresAsignados',
        data: {
            num_audit_integral: num_audit_integral,
            anio_audit_integral: anio_audit_integral
        },
        dataType: 'json',
        success: function (result) {
            let SelectAuditores = document.getElementById("selectequipoTrabajoEdit");

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
                    // Marcar la opción como seleccionada si debe estarlo
                    if (tipo.selected) {  // Verifica que el objeto devuelto incluya esta propiedad
                        opcion.selected = true;
                    }   
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