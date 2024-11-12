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
                url: 'ModificarTexto',
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

    //$.ajax({
    //    url: 'ObtenerTextoCarta',
    //    type: 'POST',
    //    data: {
    //        id: id,
    //        tipo: tipo
    //    },
    //    success: function (response) {
    //        if (response.success) {
    //            // Establecer el texto en el editor
    //            $('#html-editor').summernote('code', response.message);
    //        } else {
    //            Swal.fire(
    //                'Error',
    //                'Ocurrió un error al mostrar el texto: ' + response.message,
    //                'error'
    //            );
    //        }
    //    },
    //    error: function (err) {
    //        Swal.fire(
    //            'Error',
    //            'Ocurrió un error en la solicitud AJAX: ' + err.statusText,
    //            'error'
    //        );
    //    }
    //});

}