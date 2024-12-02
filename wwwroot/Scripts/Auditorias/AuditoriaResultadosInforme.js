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