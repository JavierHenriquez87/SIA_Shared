//OBTENEMOS LAS PREGUNTAS DE CUESTIONARIO
//==========================================================================================
async function GetCuestionarios() {
  
};


//GUARDAR LAS RESPUESTAS A LAS PREGUNTAS
//==========================================================================================
async function GuardarCuestionario() {
    
}


//OBTENEMOS LAS PREGUNTAS DE UN CUESTIONARIO PARA MOSTRARLAS
//==========================================================================================
async function VerCuestionario(codigo_cuestionario) {
    $.ajax({
        type: 'ajax',
        method: 'post',
        url: 'ObtenerPreguntasCuestionario',
        data: {
            codigo_cuestionario: codigo_cuestionario
        },
        dataType: 'json',
        success: function (result) {
            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Ocurrio un error al obtener el cuestionario',
                    'error'
                )
            }
            else {
                //Mostramos el modal
                $('#verCuestionarioModal').modal('show');
                // Limpiamos la tabla antes de agregar nuevas filas
                $('#tbVerCuestionarioPreguntas').empty();

                // Recorremos el objeto resultante y llenamos la tabla
                result.forEach(function (seccion) {
                    // Agregamos la sección como un encabezado
                    $('#tbVerCuestionarioPreguntas').append(
                        `<tr class="table-primary">
                            <th colspan="2" style="text-align:center;">${seccion.DESCRIPCION_SECCION}</th>
                        </tr>`
                    );

                    // Iniciamos el contador para el correlativo de preguntas
                    let correlativo = 1;

                    seccion.sub_secciones.forEach(function (subSeccion) {
                        // Agregamos la sub-sección como un subtítulo
                        $('#tbVerCuestionarioPreguntas').append(
                            `<tr class="table-secondary">
                                <th colspan="2">${subSeccion.DESCRIPCION}</th>
                            </tr>`
                        );

                        subSeccion.Preguntas_Cuestionarios.forEach(function (pregunta) {
                            // Agregamos las preguntas de la sub-sección
                            $('#tbVerCuestionarioPreguntas').append(
                                `<tr>
                                    <td>${correlativo++}</td>
                                    <td>${pregunta.DESCRIPCION}</td>
                                </tr>`
                            );
                        });
                    });
                });
                
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            Swal.fire(
                'Error!',
                'Ocurrio un error',
                'error'
            )
        }
    });
};