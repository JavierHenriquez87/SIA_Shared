// Objeto para almacenar la información de la encuesta
var respuestasUsuario = {};
let preguntasObj = [];

// Cargamos las preguntas del cuestionario
//==========================================================================================
async function GetCuestionario() {
    //Swal.showLoading();
    $.ajax({
        type: 'post',
        url: '../ObtenerRespuestasCuestionario',
        data: {
            codigo_cuestionario: $('#codigo_cuest').val()
        },
        dataType: 'json',
        success: function (result) {
            if (result == "error") {
                Swal.fire(
                    'Error!',
                    'Ocurrio un error al obtener el cuestionario',
                    'error'
                )
            } else {
                result.forEach(function (seccion) {
                    seccion.sub_secciones.forEach(function (subSeccion) {
                        subSeccion.Preguntas_Cuestionarios.forEach(function (pregunta) {
                            if (pregunta.RESPUESTA_PREGUNTA != null) {
                                respuestasUsuario = {
                                    CODIGO_RESPUESTA: pregunta.RESPUESTA_PREGUNTA.CODIGO_RESPUESTA,
                                    CODIGO_PREGUNTA: pregunta.CODIGO_PREGUNTA,
                                    CUMPLE: pregunta.RESPUESTA_PREGUNTA.CUMPLE,
                                    NO_CUMPLE: pregunta.RESPUESTA_PREGUNTA.NO_CUMPLE,
                                    CUMPLE_PARCIALMENTE: pregunta.RESPUESTA_PREGUNTA.CUMPLE_PARCIALMENTE,
                                    NO_APLICA: pregunta.RESPUESTA_PREGUNTA.NO_APLICA,
                                    OBSERVACIONES: pregunta.RESPUESTA_PREGUNTA.OBSERVACIONES,
                                    CALIFICACIONES: pregunta.RESPUESTA_PREGUNTA.CALIFICACIONES,
                                    PUNTAJE: pregunta.RESPUESTA_PREGUNTA.PUNTAJE
                                };

                                preguntasObj.push(respuestasUsuario);
                            }
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
}

// Función para agregar los event listeners
//==========================================================================================
document.addEventListener('DOMContentLoaded', function () {
    // Función debounce para evitar que se ejecute en cada pulsación en el textarea
    function debounce(func, delay) {
        let timeout;
        return function (...args) {
            const context = this;
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(context, args), delay);
        };
    }

    // Función para manejar cambios en radio buttons y textarea
    function handleInputChange(event) {
        const input = event.target;
        const codigoPregunta = input.getAttribute('data-codigo');
        const status = document.getElementById(`divPorcentaje-${codigoPregunta}`);
        const porcentajeLabel = document.getElementById(`porcentaje-${codigoPregunta}`);

        //Buscamos la pregunta para modificarla
        var preguntaIndex = preguntasObj.findIndex(function (item) {
            return item.CODIGO_PREGUNTA == parseInt(codigoPregunta, 10);
        });

        if (preguntaIndex !== -1) {
            var preguntaExistente = preguntasObj[preguntaIndex];
            if (input.type === 'radio') {
                // Elimina todas las clases
                status.className = '';
                if (input.value === 'cumple') {
                    preguntaExistente.CUMPLE = 1;
                    preguntaExistente.NO_CUMPLE = 0;
                    preguntaExistente.CUMPLE_PARCIALMENTE = 0;
                    preguntaExistente.NO_APLICA = 0;
                    preguntaExistente.CALIFICACIONES = 'A';
                    preguntaExistente.PUNTAJE = 100;
                    status.classList.add('status', 'approved', 'percentage');
                    porcentajeLabel.textContent = '100%';
                } else if (input.value === 'parcialmente') {
                    preguntaExistente.CUMPLE = 0;
                    preguntaExistente.NO_CUMPLE = 0;
                    preguntaExistente.CUMPLE_PARCIALMENTE = 1;
                    preguntaExistente.NO_APLICA = 0;
                    preguntaExistente.CALIFICACIONES = 'B';
                    preguntaExistente.PUNTAJE = 50;
                    status.classList.add('status', 'partially', 'percentage');
                    porcentajeLabel.textContent = '50%';
                } else if (input.value === 'no-cumple') {
                    preguntaExistente.CUMPLE = 0;
                    preguntaExistente.NO_CUMPLE = 1;
                    preguntaExistente.CUMPLE_PARCIALMENTE = 0;
                    preguntaExistente.NO_APLICA = 0;
                    preguntaExistente.CALIFICACIONES = 'M';
                    preguntaExistente.PUNTAJE = 0;
                    status.classList.add('status', 'pending', 'percentage');
                    porcentajeLabel.textContent = '0%';
                } else if (input.value === 'na') {
                    preguntaExistente.CUMPLE = 0;
                    preguntaExistente.NO_CUMPLE = 0;
                    preguntaExistente.CUMPLE_PARCIALMENTE = 0;
                    preguntaExistente.NO_APLICA = 1;
                    preguntaExistente.CALIFICACIONES = 'NA';
                    preguntaExistente.PUNTAJE = 0;
                    status.classList.add('status', 'create', 'percentage');
                    porcentajeLabel.textContent = '0%';
                }
            } else if (input.type === 'textarea') {
                preguntaExistente.OBSERVACIONES = input.value;
            }
        }
    }



    // Escuchar eventos de cambio en todos los radio buttons
    const radioButtons = document.querySelectorAll('.radio-group input[type="radio"]');
    radioButtons.forEach(function (radio) {
        radio.addEventListener('change', handleInputChange);
    });



    // Escuchar eventos de cambio en todos los textarea con debounce
    const textareas = document.querySelectorAll('.textarea-container textarea');
    textareas.forEach(function (textarea) {
        textarea.addEventListener('input', debounce(handleInputChange, 300)); // 300 ms de retraso
    });
});


// Llama a GetCuestionario al cargar la página
//==========================================================================================
document.addEventListener('DOMContentLoaded', function () {
    GetCuestionario($('#codigo_cuest').val()); // Pasa el código del cuestionario que deseas cargar
});

//GUARDAR LAS RESPUESTAS A LAS PREGUNTAS
//==========================================================================================
async function saveQuestion() {

    var DataAuditCuest = {
        CODIGO_AUDITORIA_CUESTIONARIO: $('#codigo_cuest').val(),
        FECHA_CUESTIONARIO: $('#fecha_cuestionario').val(),
        AUDITOR_ASIGNADO: $('#EquipoAuditoresAsignados').val(),
        RESPONSABLE: $('#responsable_cuestionario').val(),
        REVISADO_POR: $('#cuest_revisado_por').val()
    };

    $.ajax({
        method: 'POST',
        url: '/Auditorias/Edit_Audi_Cuest',
        data: JSON.stringify(DataAuditCuest),
        contentType: 'application/json',
        success: function (respuesta) {
            if (respuesta == "error") {
                Swal.fire(
                    'Error!',
                    'Ocurrió un error al guardar el cuestionario.',
                    'error'
                )
            } else {
                // Mostrar el loader antes de iniciar la solicitud AJAX
                Swal.fire({
                    title: "Guardando...",
                    text: "Por favor, espere.",
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });

                $.ajax({
                    method: 'POST',
                    url: '/Auditorias/Edit_Cuestionario',
                    data: JSON.stringify(preguntasObj),
                    contentType: 'application/json',
                    success: function (respuesta) {
                        if (respuesta == "error") {
                            Swal.fire(
                                'Error!',
                                'Ocurrió un error al guardar el cuestionario.',
                                'error'
                            )
                        } else {
                            Swal.fire({
                                title: 'Guardado!',
                                text: '¡Cuestionario modificado con exito!.',
                                icon: 'success',
                                didClose: () => {

                                }
                            });

                        }
                    },
                    error: function () {
                        // Mostrar un mensaje de error al usuario si ocurre un error en la solicitud AJAX
                        Swal.fire({
                            title: 'Error!',
                            text: 'Hubo un problema al procesar su solicitud.',
                            icon: 'error',
                            didClose: () => {

                            }
                        });
                    }
                });
            }
        },
        error: function () {
            // Mostrar un mensaje de error al usuario si ocurre un error en la solicitud AJAX
            Swal.fire({
                title: 'Error!',
                text: 'Hubo un problema al procesar su solicitud.',
                icon: 'error',
                didClose: () => {

                }
            });
        }
    });
}
function VerCuestionario(codigo_cuestionario) {
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

function scrollToTop() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth' // Esto hará que el desplazamiento sea suave
    });
}


//Funcion que recalcula los porcentajes cuando se modifica un radio button
function recalcularPromedio(codigoSubSeccion) {
    // Selecciona todos los inputs de tipo radio en la subsección específica
    const subseccionRadios = document.querySelectorAll(`.subseccion-${codigoSubSeccion} .radio-group input[type="radio"]`);
    // Número de preguntas que no están marcadas como "na"
    let numPreguntasValidas = 0; 

    let totalPuntos = 0;


    // Itera sobre los inputs de radio y calcula el total de puntos
    subseccionRadios.forEach(radio => {
        // Verifica si el radio está marcado
        if (radio.checked) {
            let puntaje = 0;

            // Asigna el puntaje según el valor del radio seleccionado
            switch (radio.value) {
                case 'cumple':
                    puntaje = 100;
                    numPreguntasValidas++;
                    break;
                case 'parcialmente':
                    puntaje = 50;
                    numPreguntasValidas++;
                    break;
                case 'no-cumple':
                    puntaje = 0;
                    numPreguntasValidas++;
                    break;
                case 'na':
                    // No sumar esta pregunta al total de puntos ni al número de preguntas válidas
                    return;
            }

            totalPuntos += puntaje;
        }
    });

    // Calcula el promedio
    const promedio = numPreguntasValidas > 0 ? (totalPuntos / numPreguntasValidas) : 0;

    let promedio_subseccion;
    let stilo_percent;

    if (promedio === 0) {
        promedio_subseccion = "No calculado";
        stilo_percent = "status_percent nocalculado";
    }
    else if (promedio < 50.90) {
        promedio_subseccion = "Cumplimiento Bajo: " + promedio.toFixed(2) + "%";
        stilo_percent = "status_percent bajo";
    } else if (promedio < 65.90) {
        promedio_subseccion = "Cumplimiento Medio-Bajo: " + promedio.toFixed(2) + "%";
        stilo_percent = "status_percent mediobajo";
    } else if (promedio < 85.90) {
        promedio_subseccion = "Cumplimiento Medio: " + promedio.toFixed(2) + "%";
        stilo_percent = "status_percent medio";
    } else {
        promedio_subseccion = "Cumplimiento Alto: " + promedio.toFixed(2) + "%";
        stilo_percent = "status_percent alto";
    }

    // Actualiza el promedio en la interfaz
    const elemento = document.getElementById(`cumplimiento-percent-${codigoSubSeccion}`);
    if (elemento) {
        elemento.textContent = promedio_subseccion;
        elemento.className = '';  // Limpia todas las clases existentes
        elemento.className = stilo_percent;  // Aplica el nuevo estilo
    }
}


// Asigna un evento a cada radio button para recalcular el promedio cuando se seleccione una opción
document.querySelectorAll('.radio-group input[type="radio"]').forEach(radio => {
    radio.addEventListener('change', function () {
        const codigoSubSeccion = this.closest('.col-md-6').classList[2].split('-')[1];
        recalcularPromedio(codigoSubSeccion);
    });
});
