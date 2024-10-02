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
