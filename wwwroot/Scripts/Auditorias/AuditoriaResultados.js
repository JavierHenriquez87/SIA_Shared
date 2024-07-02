const pdfData = [
    {
        "name": "Documento 1",
        "url": "path/to/document1.pdf",
        "size": "1.2 MB",
    },
    {
        "name": "Documento 2",
        "url": "path/to/document2.pdf",
        "size": "850 KB",
    },
    {
        "name": "Documento 3",
        "url": "path/to/document3.pdf",
        "size": "2.5 MB",
    }
];

// Función para renderizar la lista de PDFs
function renderPdfList(pdfData) {
    const container = document.getElementById('pdf-container');

    pdfData.forEach(pdf => {
        const pdfItem = document.createElement('div');
        pdfItem.className = 'pdf-item';

        const pdfThumbnail = document.createElement('img');
        pdfThumbnail.src = "/assets/images/pdf-ico.png";
        pdfThumbnail.alt = pdf.name;

        const pdfDetails = document.createElement('div');

        const pdfLink = document.createElement('a');
        pdfLink.href = pdf.url;
        pdfLink.target = '_blank';
        pdfLink.textContent = pdf.name + ".pdf";

        const pdfSize = document.createElement('p');
        pdfSize.textContent = `${pdf.size}`;

        pdfDetails.appendChild(pdfLink);
        pdfDetails.appendChild(pdfSize);
        pdfItem.appendChild(pdfThumbnail);
        pdfItem.appendChild(pdfDetails);

        container.appendChild(pdfItem);
    });
}

// Llamada a la función para renderizar la lista
renderPdfList(pdfData);

function AgregarHallazgo() {
    window.location.href = '/Auditorias/AuditoriaResultados/AuditoriaHallazgo';
}