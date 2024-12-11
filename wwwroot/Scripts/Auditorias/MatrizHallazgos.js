//FUNCION PARA OBTENER LAS AUDITORIAS INTEGRALES EN PANTALLA INICIAL
//==========================================================================================
GeAuditoriasIntegrales();
async function GeAuditoriasIntegrales() {
    $(document).ready(function () {
        // MOSTRAMOS TODA LA TABLA
        $('#tbMatrizHallazgo').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "ajax":
            {
                "url": '/Auditorias/GetMatrizHallazgos',
                "type": "POST",
                "datatype": "json"
            },
            "deferRender": true,
            "columns": [
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.codigO_AUDITORIA + "-" + row.numerO_AUDITORIA_INTEGRAL + "</div>";
                    },
                    "name": "CODIGO",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "listado_hallazgos",
                    "render": function (data, type, row, meta) {
                        if (row.listado_hallazgos && row.listado_hallazgos.length > 0) {
                            // Recorrer los hallazgos y extraer las condiciones
                            let condiciones = row.listado_hallazgos.map(function (hallazgo) {
                                return hallazgo.condicion;
                            }).join(", "); // Une las condiciones en una cadena separada por comas

                            // Retornar las condiciones de los hallazgos
                            return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + condiciones + "</div>";
                        }
                        return "No existen condiciones agregadas";
                    },
                    "name": "CONDICION",
                    "autoWidth": true,
                    "orderable": false,
                    "width": "50%"
                },
                {
                    "data": "listado_hallazgos", "render": function (data, type, row, meta) {
                        if (!row.listado_hallazgos || row.listado_hallazgos.length === 0) {
                            return "<div>No hay recomendacion</div>"; // Si no hay hallazgos, mostrar mensaje
                        }

                        let recomendaciones = row.listado_hallazgos.map(function (hallazgo) {
                            return hallazgo.listado_detalles
                                .filter(function (detalle) {
                                    return detalle.tipo && detalle.tipo.toLowerCase().includes("recomendacion");
                                })
                                .map(function (detalle) {
                                    return detalle.descripcion;
                                })
                                .join(", ");
                        }).join("<br/>"); 

                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + recomendaciones + "</div>";
                    },
                    "name": "RECOMENDACION",
                    "autoWidth": true,
                    "orderable": false,
                    "width": "20%"
                },
                {
                    "data": "numerO_AUDITORIA_INTEGRAL",
                    "render": function (data, type, row, meta) {
                       

                        return "";
                    },
                    "autoWidth": true,
                    "orderable": false,
                    "width": "10%"
                },
            ],
            "language": {
                "url": "https://cdn.datatables.net/plug-ins/1.10.11/i18n/Spanish.json"
            },
            "pagingType": "full_numbers",
            "iDisplayLength": 25,
            "lengthChange": false,
            responsive: "true",
        });
    });

};

const tabResumida = document.getElementById('tabResumida');
const tabCompleta = document.getElementById('tabCompleta');

// Añade eventos de clic para alternar la clase 'active'
tabResumida.addEventListener('click', function (e) {
    e.preventDefault(); // Evita que se recargue la página
    tabResumida.classList.add('active');
    tabCompleta.classList.remove('active');
});

tabCompleta.addEventListener('click', function (e) {
    e.preventDefault(); // Evita que se recargue la página
    tabCompleta.classList.add('active');
    tabResumida.classList.remove('active');
});