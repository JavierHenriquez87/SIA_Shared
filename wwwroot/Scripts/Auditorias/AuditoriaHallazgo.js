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