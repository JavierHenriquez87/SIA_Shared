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

let inputCount = 1; // Inicializar un contador

document.getElementById('btn_efecto').addEventListener('click', function () {
    // Crear un nuevo input
    const newInput = document.createElement('input');
    newInput.placeholder = 'Escribir...';
    newInput.classList.add('additional-input');

    // Asignar un ID único al nuevo input
    newInput.id = `efecto-${inputCount}`;

    // Añadir el nuevo input al contenedor
    const container = document.getElementById('input-container');
    container.appendChild(newInput);

    // Incrementar el contador
    inputCount++;
});