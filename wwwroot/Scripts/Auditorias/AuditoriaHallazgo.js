//Evitamos que el boton envie el formulario
document.getElementById('cuantitativa').addEventListener('click', function (event) {
    event.preventDefault();
});

//Evitamos que el boton envie el formulario
document.getElementById('cualitativa').addEventListener('click', function (event) {
    event.preventDefault();
});


// Esperar a que el DOM esté completamente cargado
document.addEventListener("DOMContentLoaded", function () {
    // Objeto JavaScript para almacenar los valores del formulario
    const formularioData = {
        hallazgo: "",
        calificacion: "",
        valor_muestra: "",
        muestra_inconsistente: "",
        desviacion_muestra: "",
        selectedRisk: "",
        condicion: "",
        criterio: "",
        causageneral: [
            { id: "causa", causa: "" }  // Causa inicial por defecto
        ]
    };

    console.log(formularioData);

    // Seleccionar todos los inputs y agregar el evento de cambio
    const inputs = document.querySelectorAll('input, textarea');
    inputs.forEach(input => {
        input.addEventListener('change', handleInputChange);
    });

    // Función para manejar el cambio en los inputs
    function handleInputChange(event) {
        const { name, value } = event.target;
        formularioData[name] = value;
        console.log(formularioData);
    }

    // Agregar eventos a los botones para cambiar la calificación
    document.getElementById('cuantitativa').addEventListener('click', function (event) {
        event.preventDefault();
        formularioData.calificacion = "1";  // Cuantitativa es 1
    });

    document.getElementById('cualitativa').addEventListener('click', function (event) {
        event.preventDefault();
        formularioData.calificacion = "2";  // Cualitativa es 2
    });
});


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

    // Crear un botón de eliminar
    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.classList.add('remove-button');
    removeButton.innerHTML = '<i class="bx bx-trash"></i>'; // Usar un ícono de basura si lo deseas

    // Añadir evento para eliminar el input cuando se haga clic en el botón
    removeButton.addEventListener('click', function () {
        inputContainer.remove();
    });

    // Añadir el input y el botón de eliminar al contenedor div
    inputContainer.appendChild(newInput);
    inputContainer.appendChild(removeButton);

    // Añadir el contenedor div al contenedor principal
    const container = document.getElementById('input_causa');
    container.appendChild(inputContainer);

    //Agregar al array de objetos
    const nuevasCausas = formularioData.causageneral;
    nuevasCausas.push({ id: uniqueId, causa: "" });
    // Incrementar el contador
    causaCount++;
});



let efectoCount = 1; // Inicializar un contador

document.getElementById('btn_efecto').addEventListener('click', function () {
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

    // Crear un botón de eliminar
    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.classList.add('remove-button');
    removeButton.innerHTML = '<i class="bx bx-trash"></i>'; // Usar un ícono de basura si lo deseas

    // Añadir evento para eliminar el input cuando se haga clic en el botón
    removeButton.addEventListener('click', function () {
        inputContainer.remove();
    });

    // Añadir el input y el botón de eliminar al contenedor div
    inputContainer.appendChild(newInput);
    inputContainer.appendChild(removeButton);

    // Añadir el contenedor div al contenedor principal
    const container = document.getElementById('input_efecto');
    container.appendChild(inputContainer);

    // Incrementar el contador
    efectoCount++;
});

let recomendacionesCount = 1; // Inicializar un contador

document.getElementById('btn_recomendaciones').addEventListener('click', function () {
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

    // Crear un botón de eliminar
    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.classList.add('remove-button');
    removeButton.innerHTML = '<i class="bx bx-trash"></i>'; // Usar un ícono de basura si lo deseas

    // Añadir evento para eliminar el input cuando se haga clic en el botón
    removeButton.addEventListener('click', function () {
        inputContainer.remove();
    });

    // Añadir el input y el botón de eliminar al contenedor div
    inputContainer.appendChild(newInput);
    inputContainer.appendChild(removeButton);

    // Añadir el contenedor div al contenedor principal
    const container = document.getElementById('input_recomendaciones');
    container.appendChild(inputContainer);

    // Incrementar el contador
    recomendacionesCount++;
});

let comentariosCount = 1; // Inicializar un contador

document.getElementById('btn_comentarios').addEventListener('click', function () {
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

    // Crear un botón de eliminar
    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.classList.add('remove-button');
    removeButton.innerHTML = '<i class="bx bx-trash"></i>'; // Usar un ícono de basura si lo deseas

    // Añadir evento para eliminar el input cuando se haga clic en el botón
    removeButton.addEventListener('click', function () {
        inputContainer.remove();
    });

    // Añadir el input y el botón de eliminar al contenedor div
    inputContainer.appendChild(newInput);
    inputContainer.appendChild(removeButton);

    // Añadir el contenedor div al contenedor principal
    const container = document.getElementById('input_comentarios');
    container.appendChild(inputContainer);

    // Incrementar el contador
    comentariosCount++;
});

let accionesRequeridasCount = 1; // Inicializar un contador

document.getElementById('btn_acciones_requeridas').addEventListener('click', function () {
    // Crear un contenedor div para el nuevo input y el botón de eliminar
    const inputContainer = document.createElement('div');
    inputContainer.classList.add('input-group');

    // Crear un nuevo input
    const newInput = document.createElement('input');
    newInput.placeholder = 'Escribir...';
    newInput.classList.add('additional-input');

    // Asignar un ID único al nuevo input
    const uniqueId = `acciones_requeridas-${accionesRequeridasCount}`;
    newInput.id = uniqueId;

    // Crear un botón de eliminar
    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.classList.add('remove-button');
    removeButton.innerHTML = '<i class="bx bx-trash"></i>'; // Usar un ícono de basura si lo deseas

    // Añadir evento para eliminar el input cuando se haga clic en el botón
    removeButton.addEventListener('click', function () {
        inputContainer.remove();
    });

    // Añadir el input y el botón de eliminar al contenedor div
    inputContainer.appendChild(newInput);
    inputContainer.appendChild(removeButton);

    // Añadir el contenedor div al contenedor principal
    const container = document.getElementById('input_acciones_requeridas');
    container.appendChild(inputContainer);

    // Incrementar el contador
    accionesRequeridasCount++;
});


document.getElementById('selectedRisk').addEventListener('click', function () {
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
        const selectedRisk = document.getElementById('selectedRisk');
        selectedRisk.innerText = this.innerText;
        selectedRisk.className = `card highlight-${riskLevel}`;
        selectedRisk.setAttribute('data-risk', riskLevel);

        // Actualizar el valor del input oculto
        document.getElementById('nivel_riesgo').value = riskLevel;

        // Ocultar las opciones de nivel de riesgo
        document.getElementById('riskLevels').style.display = 'none';
    });
});

