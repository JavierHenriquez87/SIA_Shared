// Objeto para almacenar la información de la encuesta
const respuestasUsuario = {};

// Cargamos las preguntas del cuestionario
//==========================================================================================
async function GetCuestionario(codigo_cuestionario) {
    $.ajax({
        type: 'post',
        url: '../ObtenerPreguntasCuestionario',
        data: {
            codigo_cuestionario: '1'
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
                        const titulo2 = document.createElement('h3');
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

                            // Agrega el contenido HTML a la tarjeta
                            card.innerHTML = `
                                <div class="row">
                                    <b for="question">${pregunta.DESCRIPCION}</b>
                                </div>
                                <div class="radio-group">
                                    <input type="radio" id="cumple-${pregunta.CODIGO_PREGUNTA}" name="evaluacion-${pregunta.CODIGO_PREGUNTA}" value="cumple" data-codigo="${pregunta.CODIGO_PREGUNTA}">
                                    <label for="cumple-${pregunta.CODIGO_PREGUNTA}">Cumple</label>
                                    <input type="radio" id="parcialmente-${pregunta.CODIGO_PREGUNTA}" name="evaluacion-${pregunta.CODIGO_PREGUNTA}" value="parcialmente" data-codigo="${pregunta.CODIGO_PREGUNTA}">
                                    <label for="parcialmente-${pregunta.CODIGO_PREGUNTA}">Cumple Parcialmente</label>
                                    <input type="radio" id="no-cumple-${pregunta.CODIGO_PREGUNTA}" name="evaluacion-${pregunta.CODIGO_PREGUNTA}" value="no-cumple" data-codigo="${pregunta.CODIGO_PREGUNTA}">
                                    <label for="no-cumple-${pregunta.CODIGO_PREGUNTA}">No Cumple</label>
                                    <input type="radio" id="na-${pregunta.CODIGO_PREGUNTA}" name="evaluacion-${pregunta.CODIGO_PREGUNTA}" value="na" data-codigo="${pregunta.CODIGO_PREGUNTA}">
                                    <label for="na-${pregunta.CODIGO_PREGUNTA}">N/A</label>
                                </div>
                                <div class="textarea-container">
                                    <textarea id="textarea-${pregunta.CODIGO_PREGUNTA}" name="textarea-${pregunta.CODIGO_PREGUNTA}" data-codigo="${pregunta.CODIGO_PREGUNTA}"></textarea>
                                </div>
                                <div id="divPorcentaje-${pregunta.CODIGO_PREGUNTA}" class="status create percentage">
                                    <b>Puntaje: </b><label id="porcentaje-${pregunta.CODIGO_PREGUNTA}">0%</label>
                                </div>
                            `;

                            // Añade la tarjeta a la fila actual
                            row.appendChild(card);

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

    if (!respuestasUsuario[codigoPregunta]) {
        respuestasUsuario[codigoPregunta] = {};
    }

    if (input.type === 'radio') {
        // Elimina todas las clases
        status.className = '';
        if (input.value === 'cumple') {
            respuestasUsuario[codigoPregunta].cumple = 1;
            respuestasUsuario[codigoPregunta].no_cumple = 0;
            respuestasUsuario[codigoPregunta].cumple_parcialmente = 0;
            respuestasUsuario[codigoPregunta].na = 0;
            respuestasUsuario[codigoPregunta].calificacion = 'A';
            respuestasUsuario[codigoPregunta].puntaje = 100;
            status.classList.add('status', 'approved', 'percentage');
            porcentajeLabel.textContent = '100%';
        } else if (input.value === 'parcialmente') {
            respuestasUsuario[codigoPregunta].cumple = 0;
            respuestasUsuario[codigoPregunta].no_cumple = 0;
            respuestasUsuario[codigoPregunta].cumple_parcialmente = 1;
            respuestasUsuario[codigoPregunta].na = 0;
            respuestasUsuario[codigoPregunta].calificacion = 'B';
            respuestasUsuario[codigoPregunta].puntaje = 50;
            status.classList.add('status', 'partially', 'percentage');
            porcentajeLabel.textContent = '50%';
        } else if (input.value === 'no-cumple') {
            respuestasUsuario[codigoPregunta].cumple = 0;
            respuestasUsuario[codigoPregunta].no_cumple = 1;
            respuestasUsuario[codigoPregunta].cumple_parcialmente = 0;
            respuestasUsuario[codigoPregunta].na = 0;
            respuestasUsuario[codigoPregunta].calificacion = 'M';
            respuestasUsuario[codigoPregunta].puntaje = 0;
            status.classList.add('status', 'pending', 'percentage');
            porcentajeLabel.textContent = '0%';
        } else if (input.value === 'na') {
            respuestasUsuario[codigoPregunta].cumple = 0;
            respuestasUsuario[codigoPregunta].no_cumple = 0;
            respuestasUsuario[codigoPregunta].cumple_parcialmente = 0;
            respuestasUsuario[codigoPregunta].na = 1;
            respuestasUsuario[codigoPregunta].calificacion = 'NA';
            respuestasUsuario[codigoPregunta].puntaje = 0;
            status.classList.add('status', 'create', 'percentage');
            porcentajeLabel.textContent = '0%';
        }
    } else if (input.type === 'textarea') {
        respuestasUsuario[codigoPregunta].comentario = input.value;
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
        textarea.addEventListener('input', handleInputChange);
    });
}

// Llama a GetCuestionario al cargar la página
//==========================================================================================
document.addEventListener('DOMContentLoaded', function () {
    GetCuestionario('1'); // Pasa el código del cuestionario que deseas cargar
});

//GUARDAR LAS RESPUESTAS A LAS PREGUNTAS
//==========================================================================================
async function GuardarCuestionario() {
    
}
