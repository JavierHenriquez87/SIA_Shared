//Evitamos que el boton envie el formulario
document.getElementById('cuantitativa').addEventListener('click', function (event) {
    event.preventDefault();
});

//Evitamos que el boton envie el formulario
document.getElementById('cualitativa').addEventListener('click', function (event) {
    event.preventDefault();
});

let formularioData = {};

// Esperar a que el DOM esté completamente cargado
document.addEventListener("DOMContentLoaded", function () {
    // Objeto JavaScript para almacenar los valores del formulario
    formularioData = {
        codigo_hallazgo: "0",
        hallazgo: "",
        calificacion: "1",
        valor_muestra: "",
        muestra_inconsistente: "",
        nivel_riesgo: "1",
        condicion: "",
        criterio: "",
        desviacion_muestra: "",
        causageneral: [
            { id: "causa", causa: "" }
        ],
        efecto: [
            { id: "efecto", efecto: "" }
        ],
        recomendaciones: [
            { id: "recomendaciones", recomendaciones: "" }
        ],
        comentarios: [
            { id: "comentarios", comentarios: "" }
        ],
        adjuntos: [
        ]
    };


    // Seleccionar todos los inputs y agregar el evento de cambio
    const inputs = document.querySelectorAll('input, textarea');
    inputs.forEach(input => {
        input.addEventListener('change', handleInputChange);
    });

    // Agregar eventos a los botones para cambiar la calificación
    document.getElementById('cuantitativa').addEventListener('click', function (event) {
        event.preventDefault();
        formularioData.calificacion = "1";  // Cuantitativa es 1
    });

    document.getElementById('cualitativa').addEventListener('click', function (event) {
        event.preventDefault();
        formularioData.calificacion = "2";  // Cualitativa es 2
    });

    // Cargar la informacion si el registro sera editable
    if (hallazgo !== null) {
        document.getElementById('codigo_hallazgo').value = hallazgo.CODIGO_HALLAZGO;
        formularioData.codigo_hallazgo = hallazgo.CODIGO_HALLAZGO;

        document.getElementById('hallazgo').value = hallazgo.HALLAZGO;
        formularioData.hallazgo = hallazgo.HALLAZGO;

        document.getElementById('valor_muestra').value = hallazgo.VALOR_MUESTRA;
        formularioData.valor_muestra = hallazgo.VALOR_MUESTRA;

        document.getElementById('muestra_inconsistente').value = hallazgo.MUESTRA_INCONSISTENTE;
        formularioData.muestra_inconsistente = hallazgo.MUESTRA_INCONSISTENTE;

        document.getElementById('desviacion_muestra').value = hallazgo.DESVIACION_MUESTRA;
        formularioData.desviacion_muestra = hallazgo.DESVIACION_MUESTRA;

        calculoMuestra();

        document.getElementById('condicion').value = hallazgo.CONDICION;
        formularioData.condicion = hallazgo.CONDICION;

        document.getElementById('criterio').value = hallazgo.CRITERIO;
        formularioData.criterio = hallazgo.CRITERIO;

        if (hallazgo.CALIFICACION == 1) {
            document.getElementById('cuantitativa').click();
        }
        else if (hallazgo.CALIFICACION == 2) {
            document.getElementById('cualitativa').click();
            var riesgo = hallazgo.NIVEL_RIESGO == 1 ? "bajo" : hallazgo.NIVEL_RIESGO == 2 ? "medio" : "alto";
            document.querySelector('#riskLevels .card[data-risk="' + riesgo + '"]').click();
            var textoOrientacion = "";
            hallazgo.OrientacionCalificacion.forEach((orientacion, index) => {
                textoOrientacion += orientacion.ORIENTACION;

                // Agregar salto de línea solo si no es el último elemento
                if (index < hallazgo.OrientacionCalificacion.length - 1) {
                    textoOrientacion += " \n \n";
                }
            });
            document.getElementById('orientacion_calificacion').value = textoOrientacion;
        }

        // Filtrar por tipo 'causa' y 'causa-1'
        var causas = hallazgo.Detalles.filter(detalle => detalle.TIPO.startsWith("causa"));
        var count = 0;
        var id = "causa";

        causas.forEach(causa => {
            if (count > 0) {
                id = "causa-" + count;
                document.getElementById("btn_causa").click();
            }

            document.getElementById(id).value = causa.DESCRIPCION ?? "";
            var causaExistente = formularioData.causageneral.find(e => e.id === id);
            if (causaExistente) {
                causaExistente.causa = causa.DESCRIPCION;
            }
            count++;
        });

        // Filtrar por tipo 'efecto' y 'efecto-1'
        var efectos = hallazgo.Detalles.filter(hallazgo => hallazgo.TIPO.startsWith("efecto"));
        var count = 0;
        var id = "efecto";

        efectos.forEach(efecto => {
            if (count > 0) {
                id = "efecto-" + count;
                document.getElementById("btn_efecto").click();
            }

            document.getElementById(id).value = efecto.DESCRIPCION ?? "";
            var efectoExistente = formularioData.efecto.find(e => e.id === id);
            if (efectoExistente) {
                efectoExistente.efecto = efecto.DESCRIPCION;
            }

            count++;
        });

        // Filtrar por tipo 'recomendacion' y 'recomendaciones'
        var recomendaciones = hallazgo.Detalles.filter(detalle => detalle.TIPO.startsWith("recomendaciones"));
        var count = 0;
        var id = "recomendaciones";

        recomendaciones.forEach(recomendacion => {
            if (count > 0) {
                id = "recomendaciones-" + count;
                document.getElementById("btn_recomendaciones").click();
            }

            document.getElementById(id).value = recomendacion.DESCRIPCION ?? "";
            const recomendacionExistente = formularioData.recomendaciones.find(r => r.id === id);
            if (recomendacionExistente) {
                recomendacionExistente.recomendaciones = recomendacion.DESCRIPCION;
            }

            count++;
        });

        // Filtrar por tipo 'comentario' y 'comentarios'
        var comentarios = hallazgo.Detalles.filter(hallazgo => hallazgo.TIPO.startsWith("comentarios"));
        var count = 0;
        var id = "comentarios";

        comentarios.forEach(comentario => {
            if (count > 0) {
                id = "comentarios-" + count;
                document.getElementById("btn_comentarios").click();
            }

            document.getElementById(id).value = comentario.DESCRIPCION ?? "";

            const comentarioExistente = formularioData.comentarios.find(r => r.id === id);
            if (comentarioExistente) {
                comentarioExistente.comentarios = comentario.DESCRIPCION;
            }

            count++;
        });

    }

    console.log(formularioData);
});

// Función para manejar el cambio en los inputs
function handleInputChange(event) {
    const { name, value } = event.target;

    if (name.includes("causa")) {
        const causaId = name; // Asume que el nombre del campo es el mismo que el ID en el array
        const causaObj = formularioData.causageneral.find(obj => obj.id === causaId);
        if (causaObj) {
            causaObj.causa = value;
        }
    }
    else if (name.includes("efecto")) {
        const efectoId = name; // Asume que el nombre del campo es el mismo que el ID en el array
        const efectoObj = formularioData.efecto.find(obj => obj.id === efectoId);
        if (efectoObj) {
            efectoObj.efecto = value;
        }
    }
    else if (name.includes("recomendaciones")) {
        const recomendacionesId = name; // Asume que el nombre del campo es el mismo que el ID en el array
        const recomendacionesObj = formularioData.recomendaciones.find(obj => obj.id === recomendacionesId);
        if (recomendacionesObj) {
            recomendacionesObj.recomendaciones = value;
        }
    }
    else if (name.includes("comentarios")) {
        const comentariosId = name; // Asume que el nombre del campo es el mismo que el ID en el array
        const comentariosObj = formularioData.comentarios.find(obj => obj.id === comentariosId);
        if (comentariosObj) {
            comentariosObj.comentarios = value;
        }
    }
    else {
        formularioData[name] = value;
    }
}

function calculoMuestra() {
    var valor_muestra = $('#valor_muestra').val();
    var muestra_inconsistente = $('#muestra_inconsistente').val();

    if (valor_muestra == 0) {
        $('#desviacion_muestra').val('0%');
        selectedRisk.classList.remove('highlight-alto');
        selectedRisk.classList.remove('highlight-medio');
        selectedRisk.classList.add('highlight-bajo');
    } else {
        var desviacion = (muestra_inconsistente / valor_muestra) * 100;
        desviacion = desviacion.toFixed(2);

        $('#desviacion_muestra').val(desviacion + "%");

        if (desviacion <= 5.1) {
            selectedRisk.classList.remove('highlight-alto');
            selectedRisk.classList.remove('highlight-medio');
            selectedRisk.classList.add('highlight-bajo');
            selectedRisk.textContent = "Bajo";
        } else if (desviacion > 5.1 && desviacion <= 9.99) {
            selectedRisk.classList.remove('highlight-alto');
            selectedRisk.classList.remove('highlight-bajo');
            selectedRisk.classList.add('highlight-medio');
            selectedRisk.textContent = "Medio";
        } else {
            selectedRisk.classList.remove('highlight-bajo');
            selectedRisk.classList.remove('highlight-medio');
            selectedRisk.classList.add('highlight-alto');
            selectedRisk.textContent = "Alto";
        }
    }
}

const cuantitativaButton = document.getElementById('cuantitativa');
const cualitativaButton = document.getElementById('cualitativa');

cuantitativaButton.addEventListener('click', () => {
    cuantitativaButton.classList.add('active');
    cualitativaButton.classList.remove('active');
});

cualitativaButton.addEventListener('click', () => {
    cualitativaButton.classList.add('active');
    cuantitativaButton.classList.remove('active');
});

document.querySelector('.upload-area').addEventListener('click', function () {
    document.getElementById('documentos').click();
});


let causaCount = 1; // Inicializar un contador

document.getElementById('btn_causa').addEventListener('click', function () {
    // Antes de crear un nuevo input, busca el último ID "causa-x" existente en el DOM
    const existingInputs = document.querySelectorAll('[id^="causa-"]'); // Selecciona todos los inputs con id que empiezan con "causa-"

    if (existingInputs.length > 0) {
        // Obtener el mayor número de los IDs existentes
        existingInputs.forEach(input => {
            const idParts = input.id.split('-'); // Dividir el ID por el guion
            const numberPart = parseInt(idParts[1], 10); // Obtener el número
            if (numberPart >= causaCount) {
                causaCount = numberPart + 1; // Aumentar el contador para evitar duplicados
            }
        });
    }

    // Crear un contenedor div para el nuevo input y el botón de eliminar
    const inputContainer = document.createElement('div');
    inputContainer.classList.add('input-group');

    // Crear un nuevo input
    const newInput = document.createElement('input');
    newInput.placeholder = 'Escribir...';
    newInput.classList.add('additional-input');

    // Asignar un ID único al nuevo input
    const uniqueId = `causa-${causaCount}`;
    newInput.id = uniqueId;
    newInput.name = uniqueId; // Añadir un nombre único al input

    // Crear un botón de eliminar
    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.classList.add('remove-button');
    removeButton.innerHTML = '<i class="bx bx-trash"></i>'; // Usar un ícono de basura si lo deseas

    // Añadir evento para eliminar el input cuando se haga clic en el botón
    removeButton.addEventListener('click', function () {
        inputContainer.remove();
        // Eliminar del array de objetos
        formularioData.causageneral = formularioData.causageneral.filter(causa => causa.id !== uniqueId);
    });

    // Añadir el input y el botón de eliminar al contenedor div
    inputContainer.appendChild(newInput);
    inputContainer.appendChild(removeButton);

    // Añadir el contenedor div al contenedor principal
    const container = document.getElementById('input_causa');
    container.appendChild(inputContainer);

    // Agregar al array de objetos
    formularioData.causageneral.push({ id: uniqueId, causa: "" });

    // Vincular la función handleInputChange al evento input del nuevo input
    newInput.addEventListener('change', handleInputChange);

    // Incrementar el contador para el próximo input
    causaCount++;
});


let efectoCount = 1; // Inicializar un contador

document.getElementById('btn_efecto').addEventListener('click', function () {
    // Antes de crear un nuevo input, busca el último ID "efecto-x" existente en el DOM
    const existingInputs = document.querySelectorAll('[id^="efecto-"]'); // Selecciona todos los inputs con id que empiezan con "efecto-"

    if (existingInputs.length > 0) {
        // Obtener el mayor número de los IDs existentes
        existingInputs.forEach(input => {
            const idParts = input.id.split('-'); // Dividir el ID por el guion
            const numberPart = parseInt(idParts[1], 10); // Obtener el número
            if (numberPart >= efectoCount) {
                efectoCount = numberPart + 1; // Aumentar el contador para evitar duplicados
            }
        });
    }

    // Crear un contenedor div para el nuevo input y el botón de eliminar
    const inputContainer = document.createElement('div');
    inputContainer.classList.add('input-group');

    // Crear un nuevo input
    const newInput = document.createElement('input');
    newInput.placeholder = 'Escribir...';
    newInput.classList.add('additional-input');

    // Asignar un ID único al nuevo input
    const uniqueId = `efecto-${efectoCount}`;
    newInput.id = uniqueId;
    newInput.name = uniqueId;

    // Crear un botón de eliminar
    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.classList.add('remove-button');
    removeButton.innerHTML = '<i class="bx bx-trash"></i>'; // Usar un ícono de basura si lo deseas

    // Añadir evento para eliminar el input cuando se haga clic en el botón
    removeButton.addEventListener('click', function () {
        inputContainer.remove();
        // Eliminar del array de objetos
        formularioData.efecto = formularioData.efecto.filter(efecto => efecto.id !== uniqueId);
    });

    // Añadir el input y el botón de eliminar al contenedor div
    inputContainer.appendChild(newInput);
    inputContainer.appendChild(removeButton);

    // Añadir el contenedor div al contenedor principal
    const container = document.getElementById('input_efecto');
    container.appendChild(inputContainer);

    //Agregar al array de objetos
    formularioData.efecto.push({ id: uniqueId, efecto: "" });

    // Vincular la función handleInputChange al evento input del nuevo input
    newInput.addEventListener('change', handleInputChange);

    // Incrementar el contador
    efectoCount++;
});

let recomendacionesCount = 1; // Inicializar un contador

document.getElementById('btn_recomendaciones').addEventListener('click', function () {
    // Antes de crear un nuevo input, busca el último ID "recomendaciones-x" existente en el DOM
    const existingInputs = document.querySelectorAll('[id^="recomendaciones-"]'); // Selecciona todos los inputs con id que empiezan con "recomendaciones-"

    if (existingInputs.length > 0) {
        // Obtener el mayor número de los IDs existentes
        existingInputs.forEach(input => {
            const idParts = input.id.split('-'); // Dividir el ID por el guion
            const numberPart = parseInt(idParts[1], 10); // Obtener el número
            if (numberPart >= recomendacionesCount) {
                recomendacionesCount = numberPart + 1; // Aumentar el contador para evitar duplicados
            }
        });
    }

    // Crear un contenedor div para el nuevo input y el botón de eliminar
    const inputContainer = document.createElement('div');
    inputContainer.classList.add('input-group');

    // Crear un nuevo input
    const newInput = document.createElement('input');
    newInput.placeholder = 'Escribir...';
    newInput.classList.add('additional-input');

    // Asignar un ID único al nuevo input
    const uniqueId = `recomendaciones-${recomendacionesCount}`;
    newInput.id = uniqueId;
    newInput.name = uniqueId;

    // Crear un botón de eliminar
    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.classList.add('remove-button');
    removeButton.innerHTML = '<i class="bx bx-trash"></i>'; // Usar un ícono de basura si lo deseas

    // Añadir evento para eliminar el input cuando se haga clic en el botón
    removeButton.addEventListener('click', function () {
        inputContainer.remove();
        // Eliminar del array de objetos
        formularioData.recomendaciones = formularioData.recomendaciones.filter(recomendaciones => recomendaciones.id !== uniqueId);
    });

    // Añadir el input y el botón de eliminar al contenedor div
    inputContainer.appendChild(newInput);
    inputContainer.appendChild(removeButton);

    // Añadir el contenedor div al contenedor principal
    const container = document.getElementById('input_recomendaciones');
    container.appendChild(inputContainer);

    //Agregar al array de objetos
    formularioData.recomendaciones.push({ id: uniqueId, recomendaciones: "" });

    // Vincular la función handleInputChange al evento input del nuevo input
    newInput.addEventListener('change', handleInputChange);

    // Incrementar el contador
    recomendacionesCount++;
});

let comentariosCount = 1; // Inicializar un contador

document.getElementById('btn_comentarios').addEventListener('click', function () {
    // Antes de crear un nuevo input, busca el último ID "comentarios-x" existente en el DOM
    const existingInputs = document.querySelectorAll('[id^="comentarios-"]'); // Selecciona todos los inputs con id que empiezan con "comentarios-"

    if (existingInputs.length > 0) {
        // Obtener el mayor número de los IDs existentes
        existingInputs.forEach(input => {
            const idParts = input.id.split('-'); // Dividir el ID por el guion
            const numberPart = parseInt(idParts[1], 10); // Obtener el número
            if (numberPart >= comentariosCount) {
                comentariosCount = numberPart + 1; // Aumentar el contador para evitar duplicados
            }
        });
    }

    // Crear un contenedor div para el nuevo input y el botón de eliminar
    const inputContainer = document.createElement('div');
    inputContainer.classList.add('input-group');

    // Crear un nuevo input
    const newInput = document.createElement('input');
    newInput.placeholder = 'Escribir...';
    newInput.classList.add('additional-input');

    // Asignar un ID único al nuevo input
    const uniqueId = `comentarios-${comentariosCount}`;
    newInput.id = uniqueId;
    newInput.name = uniqueId;

    // Crear un botón de eliminar
    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.classList.add('remove-button');
    removeButton.innerHTML = '<i class="bx bx-trash"></i>'; // Usar un ícono de basura si lo deseas

    // Añadir evento para eliminar el input cuando se haga clic en el botón
    removeButton.addEventListener('click', function () {
        inputContainer.remove();
        // Eliminar del array de objetos
        formularioData.comentarios = formularioData.comentarios.filter(comentarios => comentarios.id !== uniqueId);
    });

    // Añadir el input y el botón de eliminar al contenedor div
    inputContainer.appendChild(newInput);
    inputContainer.appendChild(removeButton);

    // Añadir el contenedor div al contenedor principal
    const container = document.getElementById('input_comentarios');
    container.appendChild(inputContainer);

    //Agregar al array de objetos
    formularioData.comentarios.push({ id: uniqueId, comentarios: "" });

    // Vincular la función handleInputChange al evento input del nuevo input
    newInput.addEventListener('change', handleInputChange);

    // Incrementar el contador
    comentariosCount++;
});


let accionesRequeridasCount = 1; // Inicializar un contador

document.getElementById('selectedRisk2').addEventListener('click', function () {
    const riskLevels = document.getElementById('riskLevels');
    riskLevels.style.display = riskLevels.style.display === 'none' ? 'block' : 'none';
});

document.querySelectorAll('.risk-level-options .card').forEach(function (element) {
    element.addEventListener('click', function () {
        // Remover la clase highlight de todos los elementos
        document.querySelectorAll('.risk-level-options .card').forEach(function (el) {
            el.classList.remove('highlight-bajo', 'highlight-medio', 'highlight-alto');
        });

        // Agregar la clase highlight al elemento seleccionado
        const riskLevel = this.getAttribute('data-risk');
        this.classList.add(`highlight-${riskLevel}`);

        // Actualizar el elemento que muestra el nivel seleccionado
        const selectedRisk = document.getElementById('selectedRisk2');
        selectedRisk.innerText = this.innerText;
        selectedRisk.className = `card highlight-${riskLevel}`;
        selectedRisk.setAttribute('data-risk', riskLevel);

        // Actualizar el valor del input oculto
        formularioData.nivel_riesgo = riskLevel === 'alto' ? "3" : (riskLevel === 'medio' ? "2" : "1");

        // Ocultar las opciones de nivel de riesgo
        document.getElementById('riskLevels').style.display = 'none';

        OrientacionCalificacion();
        //console.log(formularioData);
    });
});



// Función para enviar el formulario con FormData
const guardarBtn = document.getElementById('guardar-btn');
guardarBtn.addEventListener('click', function () {
    var respuesta = ValidarObjeto(formularioData);
    var pathSave = '';

    if (respuesta == false) {
        Swal.fire(
            'Advertencia',
            'El campo Hallazgo no puede quedar vacio al guardar.',
            'warning'
        )
    } else {
        Swal.showLoading();
        // Crear un objeto FormData
        let formData = new FormData();

        // Agregar los campos al FormData
        formData.append("codigo_hallazgo", formularioData.codigo_hallazgo);
        formData.append("hallazgo", formularioData.hallazgo);
        formData.append("calificacion", formularioData.calificacion);
        formData.append("valor_muestra", formularioData.valor_muestra);
        formData.append("muestra_inconsistente", formularioData.muestra_inconsistente);
        formData.append("nivel_riesgo", formularioData.nivel_riesgo);
        formData.append("condicion", formularioData.condicion);
        formData.append("criterio", formularioData.criterio);
        formData.append("desviacion_muestra", formularioData.desviacion_muestra);

        // Agregar los arreglos (como 'causageneral', 'efecto', etc.)
        formData.append("causageneral", JSON.stringify(formularioData.causageneral));
        formData.append("efecto", JSON.stringify(formularioData.efecto));
        formData.append("recomendaciones", JSON.stringify(formularioData.recomendaciones));
        formData.append("comentarios", JSON.stringify(formularioData.comentarios));

        // Agregar los archivos al FormData
        formularioData.adjuntos.forEach(function (archivo, index) {
            formData.append(`adjuntos[${index}]`, archivo.archivo); // Aquí usamos el archivo original
        });

        if (formularioData.codigo_hallazgo == "0") {
            pathSave = '/Auditorias/GuardarHallazgos';
        } else {
            pathSave = '/Auditorias/EditarHallazgo';
        }

        // Realizar la solicitud de envío de los datos con fetch
        fetch(pathSave, {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                Swal.fire({
                    title: "Guardado",
                    text: "Se guardo la información con éxito",
                    icon: "success",
                    showCancelButton: false,
                    confirmButtonColor: "#3085d6",
                    confirmButtonText: "Aceptar!"
                }).then((result) => {
                    if (result.isConfirmed) {
                        var params_base = $("#paramsah_base64").val();
                        window.location.href = '/Auditorias/AuditoriaResultados' + params_base;
                    }
                });
            })
            .catch(error => {
                console.error('Error al enviar los datos:', error);
            });
    }
});

function ValidarObjeto(data) {
    var hallazgo = $('#hallazgo').val();
    var valor_muestra = $('#valor_muestra').val();
    var muestra_inconsistente = $('#muestra_inconsistente').val();

    if (hallazgo.length == 0) {
        return "hallazgo"; // Si el campo de Hallazgo esta vacio devolvemos false
    }

    // Verificar la relación entre valor_muestra y muestra_inconsistente
    if (valor_muestra.length == 0 && muestra_inconsistente.length != 0) {
        return "muestras"; // Si valor_muestra está vacío y muestra_inconsistente no, devolvemos false
    }

    if (valor_muestra.length != 0 && muestra_inconsistente.length == 0) {
        return "muestras"; // Si valor_muestra tiene un valor y muestra_inconsistente está vacío, devolvemos false
    }

    return true // Si el campo de Hallazgo no esta vacio devolvemos true
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

// Cuando un archivo es seleccionado desde el input o se arrastra
function agregarArchivo() {
    var input = document.getElementById('documentos');
    var archivo = input.files[0]; // Obtener el archivo seleccionado

    if (archivo) {
        // Validar el tipo de archivo
        if (tiposPermitidos.includes(archivo.type)) {
            // Agregar el archivo al objeto formularioData
            formularioData.adjuntos.push({
                nombre: archivo.name,
                tipo: archivo.type,
                tamaño: archivo.size,
                archivo: archivo
            });

            // Actualizar la lista de archivos visualmente
            actualizarListaDeArchivos();

            // Limpiar el input para permitir otra selección
            input.value = ""; // Limpiar el campo input
            document.getElementById('uploadText').textContent = 'Click para subir documento o arrastre y suelte el archivo aquí';
        } else {
            Swal.fire(
                'Advertencia',
                'Tipo de archivo no permitido. Solo se permiten PDF, Word, Excel e imágenes.',
                'warning'
            )
        }
    } else {
        Swal.fire(
            'Advertencia',
            'No se ha seleccionado ningún archivo.',
            'warning'
        )
    }
}

// Manejador para arrastrar y soltar archivos
function dropHandler(event) {
    event.preventDefault(); // Prevenir la acción por defecto del navegador
    var input = document.getElementById('documentos');
    var archivo = event.dataTransfer.files[0]; // Obtener el archivo arrastrado

    if (archivo) {
        // Agregar el archivo al array y actualizar lista
        formularioData.adjuntos.push({
            nombre: archivo.name,
            tipo: archivo.type,
            tamaño: archivo.size,
            archivo: archivo
        });
        actualizarListaDeArchivos();
        document.getElementById('uploadText').textContent = 'Archivo arrastrado: ' + archivo.name;
    }
}

// Función para actualizar visualmente la lista de archivos agregados
function actualizarListaDeArchivos() {
    var lista = document.getElementById('archivosAgregados');
    lista.innerHTML = ""; // Limpiar la lista antes de renderizar

    formularioData.adjuntos.forEach(function (archivo, index) {
        var li = document.createElement('li');
        var icono = obtenerIcono(archivo.tipo); // Obtener el ícono según el tipo de archivo

        // Crear el contenido del archivo con nombre, ícono y botón para eliminar
        li.innerHTML = `
            ${icono} ${archivo.nombre} 
            <i style="color:red; cursor: pointer;" title="Eliminar archivo" class="fa fa-trash" onclick="eliminarArchivo(${index})"></i>
        `;

        lista.appendChild(li);
    });
}

// Evitar que el navegador abra el archivo cuando es arrastrado
function dragOverHandler(event) {
    event.preventDefault();
}

// Eliminar archivo del array y actualizar la lista
function eliminarArchivo(index) {
    formularioData.adjuntos.splice(index, 1); // Eliminar el archivo del array
    actualizarListaDeArchivos(); // Actualizar la lista visual
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


function MostrarCualitativo() {

    $('#divcuantitativo').hide();
    $('#divcualitativo').show();
    OrientacionCalificacion();
}

function MostrarCuantitativo() {

    $('#divcuantitativo').show();
    $('#divcualitativo').hide();
}

function OrientacionCalificacion() {
    const valorRiesgo = document.getElementById('selectedRisk2').getAttribute('data-risk');
    let calificacion = valorRiesgo === 'bajo' ? 1 : valorRiesgo === 'medio' ? 2 : 3;
    $.ajax({
        url: '/Auditorias/OrientacionCalificacion',
        type: 'POST',
        data: {
            calificacion
        },
        success: function (response) {
            if (response.success) {
                let textoOrientacion = "-" + response.data.join('\n-');

                // Llenar el textarea
                $('#orientacion_calificacion').val(textoOrientacion);
            } else {
                // Mostrar un mensaje de error
                Swal.fire(
                    'Error',
                    'Error al obtener las orientaciones: ' + response.message,
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
                url: '/Auditorias/EliminarDocumento',
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
                        );
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


// Función para agregar un input basado en causa.TIPO
function agregarInputCausa(causa) {
    // Crear un contenedor div para el nuevo input y el botón de eliminar
    const inputContainer = document.createElement('div');
    inputContainer.classList.add('input-group');

    // Crear un nuevo input
    const newInput = document.createElement('input');
    newInput.placeholder = 'Escribir...';
    newInput.classList.add('additional-input');

    // Usar el id de causa.TIPO para el nuevo input
    newInput.id = causa.TIPO; // Usar el id del objeto `causa`
    newInput.name = causa.TIPO; // Asignar el mismo valor al atributo name

    // Asignar el valor si ya existe una descripción
    newInput.value = causa.DESCRIPCION || '';

    // Crear un botón de eliminar
    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.classList.add('remove-button');
    removeButton.innerHTML = '<i class="bx bx-trash"></i>'; // Usar un ícono de basura si lo deseas

    // Añadir evento para eliminar el input cuando se haga clic en el botón
    removeButton.addEventListener('click', function () {
        inputContainer.remove();
        // Eliminar del array de objetos
        formularioData.causageneral = formularioData.causageneral.filter(c => c.id !== causa.TIPO);
    });

    // Añadir el input y el botón de eliminar al contenedor div
    inputContainer.appendChild(newInput);
    inputContainer.appendChild(removeButton);

    // Añadir el contenedor div al contenedor principal
    const container = document.getElementById('input_causa');
    container.appendChild(inputContainer);

    // Verificar si ya existe la causa en el array, sino, agregarla
    const causaExistente = formularioData.causageneral.find(c => c.id === causa.TIPO);
    if (!causaExistente) {
        formularioData.causageneral.push({ id: causa.TIPO, causa: newInput.value });
    }

    // Vincular la función handleInputChange al evento input del nuevo input
    newInput.addEventListener('change', handleInputChange);
}