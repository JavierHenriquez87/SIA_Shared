// Objeto para almacenar la información de la encuesta
var respuestasUsuario = {};
let preguntasObj = [];

// Cargamos las preguntas del cuestionario
//==========================================================================================
async function GetCuestionario() {

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
                const contenedor = document.getElementById('contenedor-cuestionarios');
                contenedor.innerHTML = ''; // Limpia el contenedor antes de llenarlo

                result.forEach(function (seccion) {
                    // Agrega el título de la sección
                    const titulo = document.createElement('h3');
                    titulo.textContent = seccion.DESCRIPCION_SECCION;
                    titulo.classList.add('text-card-title');
                    contenedor.appendChild(titulo);

                    seccion.sub_secciones.forEach(function (subSeccion) {
                        // Agrega el título de la sub-sección
                        const titulo2 = document.createElement('h5');
                        titulo2.textContent = subSeccion.DESCRIPCION;
                        titulo2.classList.add('text-card-subtitle');
                        contenedor.appendChild(titulo2);

                        // Crea una fila para las tarjetas
                        let row = document.createElement('div');
                        row.classList.add('column');
                        contenedor.appendChild(row);

                        let contador = 0;

                        subSeccion.Preguntas_Cuestionarios.forEach(function (pregunta) {
                            // Crea un div para la tarjeta
                            const card = document.createElement('div');
                            card.classList.add('card-tarjet');
                            var chk1 = '';
                            var chk2 = '';
                            var chk3 = '';
                            var chk4 = '';
                            var classradio = 'status create percentage';
                            var percent = '0%';

                            if (pregunta.RESPUESTA_PREGUNTA.CUMPLE === 1) {
                                chk1 = 'checked';
                                chk2 = '';
                                chk3 = '';
                                chk4 = '';
                                classradio = 'status approved percentage';
                                percent = '100%';
                            } else if (pregunta.RESPUESTA_PREGUNTA.NO_CUMPLE === 1) {
                                chk1 = '';
                                chk2 = 'checked';
                                chk3 = '';
                                chk4 = '';
                                classradio = 'status pending percentage';
                                percent = '0%';
                            } else if (pregunta.RESPUESTA_PREGUNTA.CUMPLE_PARCIALMENTE === 1) {
                                chk1 = '';
                                chk2 = '';
                                chk3 = 'checked';
                                chk4 = '';
                                classradio = 'status partially percentage';
                                percent = '50%';
                            } else if (pregunta.RESPUESTA_PREGUNTA.NO_APLICA === 1) {
                                chk1 = '';
                                chk2 = '';
                                chk3 = '';
                                chk4 = 'checked';
                                classradio = 'status create percentage';
                                percent = '0%';
                            }

                            // Agrega el contenido HTML a la tarjeta
                            card.innerHTML = `
                                <div class="row">
                                    <b for="question">${pregunta.DESCRIPCION}</b>
                                </div>
                                <div class="radio-group">
                                    <input type="radio" id="cumple-${pregunta.CODIGO_PREGUNTA}" name="evaluacion-${pregunta.CODIGO_PREGUNTA}" value="cumple" data-codigo="${pregunta.CODIGO_PREGUNTA}" ${chk1}>
                                    <label for="cumple-${pregunta.CODIGO_PREGUNTA}">Cumple</label>
                                    <input type="radio" id="parcialmente-${pregunta.CODIGO_PREGUNTA}" name="evaluacion-${pregunta.CODIGO_PREGUNTA}" value="parcialmente" data-codigo="${pregunta.CODIGO_PREGUNTA}" ${chk3}>
                                    <label for="parcialmente-${pregunta.CODIGO_PREGUNTA}">Cumple Parcialmente</label>
                                    <input type="radio" id="no-cumple-${pregunta.CODIGO_PREGUNTA}" name="evaluacion-${pregunta.CODIGO_PREGUNTA}" value="no-cumple" data-codigo="${pregunta.CODIGO_PREGUNTA}" ${chk2}>
                                    <label for="no-cumple-${pregunta.CODIGO_PREGUNTA}">No Cumple</label>
                                    <input type="radio" id="na-${pregunta.CODIGO_PREGUNTA}" name="evaluacion-${pregunta.CODIGO_PREGUNTA}" value="na" data-codigo="${pregunta.CODIGO_PREGUNTA}" ${chk4}>
                                    <label for="na-${pregunta.CODIGO_PREGUNTA}">N/A</label>
                                </div>
                                <div class="textarea-container">
                                    <textarea id="textarea-${pregunta.CODIGO_PREGUNTA}" name="textarea-${pregunta.CODIGO_PREGUNTA}" data-codigo="${pregunta.CODIGO_PREGUNTA}">${pregunta.RESPUESTA_PREGUNTA.OBSERVACIONES}</textarea>
                                </div>
                                <div id="divPorcentaje-${pregunta.CODIGO_PREGUNTA}" class="${classradio}">
                                    <b>Puntaje: </b><label id="porcentaje-${pregunta.CODIGO_PREGUNTA}">${percent}</label>
                                </div>
                            `;

                            // Añade la tarjeta a la fila actual
                            row.appendChild(card);

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

                            // Incrementa el contador
                            contador++;

                            // Si el contador es 2, reinicia el contador y crea una nueva fila
                            if (contador === 2) {
                                row = document.createElement('div');
                                row.classList.add('column');
                                contenedor.appendChild(row);
                                contador = 0;
                            }
                        });
                    });
                });
                console.log(preguntasObj);
                // Aquí agregas los listeners después de generar el contenido dinámico
                attachEventListeners();
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

// Función para manejar el cambio en los inputs y textareas
//==========================================================================================
function handleInputChange(event) {
    const input = event.target;
    const codigoPregunta = input.getAttribute('data-codigo');
    const status = document.getElementById(`divPorcentaje-${codigoPregunta}`);
    const porcentajeLabel = document.getElementById(`porcentaje-${codigoPregunta}`);

    //Buscamos la pregunta para modificarla
    var preguntaIndex = preguntasObj.findIndex(function (item) {
        return item.CODIGO_PREGUNTA == parseInt(codigoPregunta, 10);
    });
    6
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

// Función para agregar los event listeners
//==========================================================================================
function attachEventListeners() {
    const radios = document.querySelectorAll('input[type="radio"]');
    const textareas = document.querySelectorAll('textarea');

    radios.forEach(radio => {
        radio.addEventListener('change', handleInputChange);
    });

    textareas.forEach(textarea => {
        textarea.addEventListener('input', debounce(handleInputChange, 2000)); // 2 segundos de espera
    });
}

// Función debounce que evita que en cada pulsacion en el textarea se ejecute el cambio
function debounce(func, delay) {
    let timeout;
    return function (...args) {
        const context = this;
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(context, args), delay);
    };
}

// Llama a GetCuestionario al cargar la página
//==========================================================================================
document.addEventListener('DOMContentLoaded', function () {
    GetCuestionario($('#codigo_cuest').val()); // Pasa el código del cuestionario que deseas cargar
});

//GUARDAR LAS RESPUESTAS A LAS PREGUNTAS
//==========================================================================================
async function saveQuestion() {
    Swal.showLoading();
    return new Promise((resolve, reject) => {
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
        resolve();
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