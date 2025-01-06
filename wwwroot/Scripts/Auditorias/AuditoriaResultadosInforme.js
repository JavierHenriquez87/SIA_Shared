
let formularioData = {};

document.addEventListener("DOMContentLoaded", function () {
    formularioData = {};
});

function ModificarTexto(id, tipo) {

    Swal.fire({
        title: "Modificar el texto",
        html: `
         <div style="display: flex; flex-direction: column; align-items: center; width: 100%; text-align: left !important;">
            <div style="margin-top: 5px; width: 95%;">
                <textarea id="html-editor" style="width: 100%; height: 300px; margin: 0px; text-align: left !important;"></textarea>
            </div>
        </div>
    `,
        focusConfirm: false,
        preConfirm: () => {
            const editor = document.getElementById('html-editor').value;

            if (!editor) {
                Swal.showValidationMessage('Debe agregar información');
                return false;
            }

            return { editor };
        },
        showCancelButton: true,
        confirmButtonText: 'Guardar',
        cancelButtonText: 'Cancelar',
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Auditorias/ModificarTextoAuditoriaRI',
                type: 'POST',
                data: {
                    id: id,
                    texto: result.value.editor,
                    tipo: tipo
                },
                success: function (response) {
                    if (response.success) {
                        Swal.fire(
                            'Éxito',
                            'El texto se ha actualizado correctamente.',
                            'success'
                        ).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire(
                            'Error',
                            'Ocurrió un error al actualizar el texto: ' + response.message,
                            'error'
                        );
                    }
                },
                error: function (err) {
                    Swal.fire(
                        'Error',
                        'Ocurrió un error en la solicitud AJAX: ' + err.statusText,
                        'error'
                    );
                }
            });
        }
    });

    $('#html-editor').summernote({
        height: 400,
        codemirror: {
            theme: 'monokai'
        },
        toolbar: [
            ['style', []],
            ['font', ['bold', 'italic', 'underline', 'clear']],
            ['para', ['ul', 'ol']],
            ['insert', []],
            ['view', ['codeview']]
        ]
    });

    const contentDiv = document.getElementById(tipo).innerHTML;

    $('#html-editor').summernote('code', contentDiv);
}

function toggleConclusion(id) {
    var conclusionContent = document.getElementById("conclusionContent");
    var toggle = document.getElementById("toggleConclusion").checked;

    $.ajax({
        url: '/Auditorias/ActivarConclusionAuditoriaRI',
        type: 'POST',
        data: {
            id: id,
            estado: toggle,
        },
        success: function (response) {
            if (response.success) {
                conclusionContent.style.display = toggle ? "block" : "none";
            } else {
                document.getElementById("toggleConclusion").checked = !toggle;
                Swal.fire(
                    'Error',
                    'Ocurrió un error al actualizar el texto: ' + response.message,
                    'error'
                );
            }
        },
        error: function (err) {
            Swal.fire(
                'Error',
                'Ocurrió un error en la solicitud AJAX: ' + err.statusText,
                'error'
            );
        }
    });
}

function ModificarTextoRI(titulo, texto) {
    Swal.fire({
        title: "Modificar el texto",
        html: `
        <div style="display: flex; flex-direction: column; align-items: center; width: 100%; text-align: left !important;">
            <div style="margin-top: 5px; width: 95%;">
                <label for="titulo" style="display: block; font-weight: bold; margin-bottom: 5px;">Título:</label>
                <input id="html-titulo" type="text" style="width: 100%; padding: 8px; font-size: 16px; border: 1px solid #ccc; border-radius: 4px;">
            </div>

            <div style=" width: 95%;">
                <label for="html-editor" style="display: block; font-weight: bold; margin-bottom: 5px;">Contenido:</label>
                <textarea id="html-editor" style="width: 100%; padding: 10px; font-size: 16px; border: 1px solid #ccc; border-radius: 4px; text-align: left !important;"></textarea>
            </div>
        </div>
    `,
        focusConfirm: false,
        didOpen: () => {
            if (titulo) {
                document.getElementById('html-titulo').value = titulo;
                document.getElementById('html-titulo').disabled = true;
            }
        },
        preConfirm: () => {
            const editor = document.getElementById('html-editor').value;
            const titulo = document.getElementById('html-titulo').value;

            if (!editor || !titulo) {
                Swal.showValidationMessage('Debe completar toda la información');
                return false;
            }

            return { editor, titulo };
        },
        showCancelButton: true,
        confirmButtonText: 'Guardar',
        cancelButtonText: 'Cancelar',
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Auditorias/ModificarTextoResultadoInforme',
                type: 'POST',
                data: {
                    texto: result.value.editor,
                    titulo: result.value.titulo
                },
                success: function (response) {
                    if (response.success) {
                        Swal.fire(
                            'Éxito',
                            'El texto se ha actualizado correctamente.',
                            'success'
                        ).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire(
                            'Error',
                            'Ocurrió un error al actualizar el texto: ' + response.message,
                            'error'
                        );
                    }
                },
                error: function (err) {
                    Swal.fire(
                        'Error',
                        'Ocurrió un error en la solicitud AJAX: ' + err.statusText,
                        'error'
                    );
                }
            });
        }
    });

    $('#html-editor').summernote({
        height: 400,
        codemirror: {
            theme: 'monokai'
        },
        toolbar: [
            ['style', []],
            ['font', ['bold', 'italic', 'underline', 'clear']],
            ['para', ['ul', 'ol']],
            ['insert', []],
            ['view', ['codeview']]
        ]
    });

    if (texto != "") {
        const contentDiv = document.getElementById(texto).innerHTML;
        $('#html-editor').summernote('code', contentDiv);
    }
}

let archivosSeleccionados = [];

// Agregar archivos sin eliminar los existentes
function agregarArchivos(input) {
    const nuevosArchivos = Array.from(input.files);

    nuevosArchivos.forEach((file) => {
        // Verifica que no se agregue un archivo repetido
        const existe = archivosSeleccionados.some(f => f.name === file.name);
        if (!existe) {
            archivosSeleccionados.push(file);
        }
    });

    mostrarArchivos();
    input.value = '';  // Limpia el input para permitir subir el mismo archivo de nuevo
}

// Mostrar archivos en la lista
function mostrarArchivos() {
    const lista = document.getElementById('lista-archivos');
    lista.innerHTML = '';

    archivosSeleccionados.forEach((file, index) => {
        const item = document.createElement('div');
        item.innerHTML = `
                <p>
                    ${index + 1}. ${file.name} 
                    (${(file.size / 1024 / 1024).toFixed(2)} MB)
                    <button onclick="eliminarArchivo(${index})" style="margin-left: 10px;">Eliminar</button>
                </p>
            `;
        lista.appendChild(item);
    });
}

// Eliminar archivo individualmente
function eliminarArchivo(index) {
    archivosSeleccionados.splice(index, 1);
    mostrarArchivos();
}

function EliminarSeccion(codigoSecInf) {
    Swal.fire({
        title: '¿Estás seguro?',
        text: "Esta acción eliminará la sección de forma permanente.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Auditorias/EliminarSeccion', 
                type: 'POST',
                data: { codigoSecInf: codigoSecInf },
                success: function (response) {
                    if (response.success) {
                        Swal.fire(
                            'Eliminado',
                            'La sección ha sido eliminada correctamente.',
                            'success'
                        ).then(() => {
                            location.reload(); 
                        });
                    } else {
                        Swal.fire('Error', 'No se pudo eliminar la sección: ' + response.message, 'error');
                    }
                },
                error: function (err) {
                    Swal.fire('Error', 'Ocurrió un error al eliminar la sección: ' + err.statusText, 'error');
                }
            });
        }
    });
}


// Extensiones de archivos permitidos
const tiposPermitidos = [
    'application/pdf',  // PDF
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document', // Word (DOCX)
    'application/msword', // Word (DOC)
    'application/vnd.ms-excel', // Excel (XLS)
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // Excel (XLSX)
    'image/jpeg', // Imagen JPEG
    'image/png', // Imagen PNG
    'image/gif'  // Imagen GIF
];

document.querySelectorAll('.upload-area').forEach(function (area) {
    area.addEventListener('click', function () {
        var idUnico = this.querySelector('input[type="file"]').id;
        document.getElementById(idUnico).click();
    });
});

function agregarArchivo(id) {
    var input = document.getElementById(id);
    var archivo = input.files[0];
    var codigoHallazgo = id.split('_')[1];  // Extraer el ID del hallazgo

    if (archivo) {
        // Crear una lista para cada hallazgo si no existe
        if (!formularioData[codigoHallazgo]) {
            formularioData[codigoHallazgo] = {
                adjuntos: []
            };
        }

        formularioData[codigoHallazgo].adjuntos.push({
            nombre: archivo.name,
            tipo: archivo.type,
            tamaño: archivo.size,
            archivo: archivo
        });

        actualizarListaDeArchivos(codigoHallazgo);  // Pasar solo el sufijo del ID
        input.value = "";
    }
}

function actualizarListaDeArchivos(codigoHallazgo) {
    var lista = document.getElementById(`archivosAgregados_${codigoHallazgo}`);
    lista.innerHTML = "";  // Limpiar lista antes de actualizar

    var archivos = formularioData[codigoHallazgo]?.adjuntos || [];

    archivos.forEach((archivo, index) => {
        var li = document.createElement('li');
        var icono = obtenerIcono(archivo.tipo);  // Obtener ícono según el tipo de archivo

        li.innerHTML = `
                ${icono} ${archivo.nombre} 
                <i style="color:red; cursor: pointer;" title="Eliminar archivo" class="fa fa-trash" onclick="eliminarArchivo(${index}, '${codigoHallazgo}')"></i>
            `;
        lista.appendChild(li);
    });
}

// Arrastrar y soltar múltiples archivos
function dropHandler(event, codigoHallazgo) {
    event.preventDefault();
    var archivos = Array.from(event.dataTransfer.files);  // Obtener todos los archivos arrastrados

    // Crear una lista para el hallazgo si no existe
    if (!formularioData[codigoHallazgo]) {
        formularioData[codigoHallazgo] = {
            adjuntos: []
        };
    }

    archivos.forEach(archivo => {
        if (tiposPermitidos.includes(archivo.type)) {
            const existe = formularioData[codigoHallazgo].adjuntos.some(f => f.nombre === archivo.name);
            if (!existe) {
                formularioData[codigoHallazgo].adjuntos.push({
                    nombre: archivo.name,
                    tipo: archivo.type,
                    tamaño: archivo.size,
                    archivo: archivo
                });
            }
        }
    });

    actualizarListaDeArchivos(codigoHallazgo);  // Actualizar lista específica
}

//// Actualizar visualmente la lista de archivos
//function actualizarListaDeArchivos() {
//    var lista = document.getElementById('archivosAgregados');
//    lista.innerHTML = "";  // Limpiar lista antes de actualizar

//    formularioData.adjuntos.forEach((archivo, index) => {
//        var li = document.createElement('li');
//        var icono = obtenerIcono(archivo.tipo);  // Obtener ícono según el tipo de archivo

//        li.innerHTML = `
//                ${icono} ${archivo.nombre} 
//                <i style="color:red; cursor: pointer;" title="Eliminar archivo" class="fa fa-trash" onclick="eliminarArchivo(${index})"></i>
//            `;
//        lista.appendChild(li);
//    });
//}

// Eliminar archivo de la lista
function eliminarArchivo(index) {
    formularioData.adjuntos.splice(index, 1);
    actualizarListaDeArchivos();
}

// Función para determinar el ícono basado en el tipo de archivo
function obtenerIcono(tipo) {
    if (tipo.includes('image')) {
        return '🖼️'; // Ícono para imágenes
    } else if (tipo.includes('pdf')) {
        return '📄'; // Ícono para PDF
    } else if (tipo.includes('word') || tipo.includes('wordprocessingml.document')) {
        return '📝'; // Ícono para documentos de Word
    } else if (tipo.includes('excel') || tipo.includes('spreadsheet')) {
        return '📊'; // Ícono para Excel
    } else {
        return '📁'; // Ícono genérico
    }
}

function EliminarDocumento(codigoHallazgoDocumento) {
    Swal.fire({
        title: '¿Estás seguro?',
        text: "¡No podrás revertir esto!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminarlo!',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Auditorias/EliminarDocumentoInforme',
                type: 'POST',
                data: {
                    codigo: codigoHallazgoDocumento
                },
                success: function (response) {
                    if (response.success) {
                        // Mostrar el mensaje de éxito
                        Swal.fire(
                            '¡Eliminado!',
                            'Documento eliminado con éxito.',
                            'success'
                        ).then((result) => {
                            if (result.isConfirmed) {
                                location.reload();
                            }
                        });

                        $('#documento-' + codigoHallazgoDocumento).hide(); // Ocultar el documento
                    } else {
                        // Mostrar un mensaje de error
                        Swal.fire(
                            'Error',
                            'Error al eliminar el documento: ' + response.message,
                            'error'
                        );
                    }
                },
                error: function (xhr, status, error) {
                    Swal.fire(
                        'Error',
                        'Error en la solicitud: ' + error,
                        'error'
                    );
                }
            });
        }
    });
}


function AgregarComentarios(codigo) {
    var comentario = document.getElementById(`comentarioAUI_${codigo}`).value;
    var formData = new FormData();

    // Verificar si hay archivos para el hallazgo específico
    if (formularioData[codigo]?.adjuntos) {
        formularioData[codigo].adjuntos.forEach(function (archivo) {
            formData.append("archivos", archivo.archivo);  // "archivos" es el nombre esperado por el backend
        });
    }

    // Agregar el comentario y código del hallazgo
    formData.append("comentario", comentario);
    formData.append("codigo", codigo);

    // Enviar los datos con AJAX
    $.ajax({
        method: 'POST',
        url: '/Auditorias/GuardarComentario',
        data: formData,
        processData: false,
        contentType: false,
        success: function (respuesta) {
            if (respuesta.success) {
                Swal.fire('Éxito', respuesta.message, 'success').then((result) => {
                    if (result.isConfirmed) {
                        location.reload();
                    }
                });
                
            } else {
                Swal.fire('Error', respuesta.message, 'error');
            }
        },
        error: function () {
            Swal.fire('Error', 'Hubo un problema al guardar los datos', 'error');
        }
    });
}