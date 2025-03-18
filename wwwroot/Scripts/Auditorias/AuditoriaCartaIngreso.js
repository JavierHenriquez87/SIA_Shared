//FUNCION PARA MODIFICAR LA FECHA DE INICIO
//==========================================================================================
function ModificarFecha(id, tipo) {
    var textoTipo = "";
    if (tipo == "visita")
    {
        textoTipo = 'Modificar Fecha Visita';
    }
    else {
        textoTipo = 'Modificar Fecha Revisión';
    }

    Swal.fire({
        title: textoTipo,
        html: `
         <div style="display: flex; flex-direction: column; align-items: center;">
            <div style="margin-top: 20px;">
                <label for="start-date" style="display: block; text-align: center;">Fecha de inicio:</label>
                <input type="date" id="start-date" class="swal2-input" style="margin: 0px !important;">
            </div>
            <div style="margin-top: 20px; margin-bottom: 10px;">
                <label for="end-date" style="display: block; text-align: center;">Fecha de fin:</label>
                <input type="date" id="end-date" class="swal2-input" style="margin: 0px !important;">
            </div>
        </div>
    `,
        focusConfirm: false,
        preConfirm: () => {
            const startDate = document.getElementById('start-date').value;
            const endDate = document.getElementById('end-date').value;

            if (!startDate || !endDate) {
                Swal.showValidationMessage('Debe seleccionar ambas fechas');
                return false;
            }

            // Validación: la fecha de fin no puede ser igual o menor a la de inicio
            if (new Date(endDate) <= new Date(startDate)) {
                Swal.showValidationMessage('La fecha de fin no puede ser igual o menor que la fecha de inicio');
                return false;
            }

            return { startDate, endDate };
        },
        showCancelButton: true,
        confirmButtonText: 'Guardar',
        cancelButtonText: 'Cancelar',
    }).then((result) => {
        if (result.isConfirmed) {
            const startDate = result.value.startDate;
            const endDate = result.value.endDate;
            $.ajax({
                url: '/Auditorias/ModificarFechaIntegral',
                type: 'POST',
                data: {
                    id: id,
                    inicio: startDate,
                    fin: endDate,
                    tipo: tipo
                },
                success: function (response) {
                    if (response.success) {
                        Swal.fire(
                            'Éxito',
                            'Las fechas se han actualizado correctamente.',
                            'success'
                        ).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire(
                            'Error',
                            'Ocurrió un error al actualizar las fechas: ' + response.message,
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
}

//FUNCION PARA MODIFICAR EL TEXTO
//==========================================================================================
function ModificarTextoPdf(id, tipo) {
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

    $.ajax({
        url: 'ObtenerTextoCarta',
        type: 'POST',
        data: {
            id: id,
            tipo: tipo
        },
        success: function (response) {
            if (response.success) {
                // Establecer el texto en el editor
                $('#html-editor').summernote('code', response.message);
            } else {
                Swal.fire(
                    'Error',
                    'Ocurrió un error al mostrar el texto: ' + response.message,
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

//FIRMAR LA CARTA
function AgregarFirma(id, tipo) {
    Swal.fire({
        title: 'Ingrese sus credenciales',
        html: `
           <div>
                <label for="usuario" style="font-weight: bold;" style="width: 30%;">Usuario:</label>
                <input type="text" id="usuario" class="swal2-input" placeholder="Ingrese su usuario" style="width: 43%;">

            </div>
           <div>
                <label for="password" style="font-weight: bold;" style="width: 30%;">Contraseña:</label>
                <input type="password" id="password" class="swal2-input" placeholder="Ingrese su contraseña" style="width: 40%;">
            </div>
        `,
        showCancelButton: true,
        confirmButtonText: 'Firmar',
        cancelButtonText: 'Cancelar',
        preConfirm: () => {
            const usuario = document.getElementById('usuario').value;
            const password = document.getElementById('password').value;

            if (!usuario || !password) {
                Swal.showValidationMessage('Por favor, ingrese usuario y contraseña');
                return false;
            }

            return { usuario, password };
        }
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Auditorias/EnviarFirma',
                type: 'POST',
                data: {
                    id: id,
                    usuario: result.value.usuario,
                    password: result.value.password,
                    tipo: tipo
                },
                success: function (response) {
                    if (response.success) {
                        Swal.fire(
                            'Firma exitosa',
                            'Su firma ha sido registrada correctamente.',
                            'success'
                        ).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire(
                            'Error',
                            'Ocurrió un error al validar la firma',
                            'error'
                        );
                    }
                },
                error: function (err) {
                    Swal.fire(
                        'Error',
                        'Ocurrió un error al realizar la firma: ' + err.statusText,
                        'error'
                    );
                }
            });
            
        }
    });
}
