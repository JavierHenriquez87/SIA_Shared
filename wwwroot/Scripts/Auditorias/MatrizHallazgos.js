//FUNCION PARA OBTENER LAS AUDITORIAS INTEGRALES EN PANTALLA INICIAL
//==========================================================================================
GetAuditoriasIntegrales();
async function GetAuditoriasIntegrales() {
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

async function GetAuditoriasIntegralesCompleta() {
    $(document).ready(function () {
        // MOSTRAMOS TODA LA TABLA
        $('#tbMatrizHallazgoCompleta').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "ajax":
            {
                "url": '/Auditorias/GetMatrizHallazgosCompleto',
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
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " Cumplimiento " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " 31 " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " 15/12/2023 " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " Auditoría Agencia La Esperanza " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " Noe Javier Quintanilla Flores " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " AGE108-1 " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " Documentación faltante en expedientes de cuentas de ahorro. " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " Alto" + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " NOTA: SE PRESENTAN 9 CUENTAS QUE NO SE PRESENTARON EN AUDITORIA REALIZADA" + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + "13/2/2024 " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " 13/2/2024 " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " 30 " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " Noe Javier Quintanilla Flores " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + "  " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + "  " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + " Pendiente Fuera de plazo " + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                //{
                //    "data": "listado_hallazgos",
                //    "render": function (data, type, row, meta) {
                //        if (row.listado_hallazgos && row.listado_hallazgos.length > 0) {
                //            // Recorrer los hallazgos y extraer las condiciones
                //            let condiciones = row.listado_hallazgos.map(function (hallazgo) {
                //                return hallazgo.condicion;
                //            }).join(", "); // Une las condiciones en una cadena separada por comas

                //            // Retornar las condiciones de los hallazgos
                //            return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + condiciones + "</div>";
                //        }
                //        return "No existen condiciones agregadas";
                //    },
                //    "name": "CONDICION",
                //    "autoWidth": true,
                //    "orderable": false,
                //    "width": "50%"
                //},
                //{
                //    "data": "listado_hallazgos", "render": function (data, type, row, meta) {
                //        if (!row.listado_hallazgos || row.listado_hallazgos.length === 0) {
                //            return "<div>No hay recomendacion</div>"; // Si no hay hallazgos, mostrar mensaje
                //        }

                //        let recomendaciones = row.listado_hallazgos.map(function (hallazgo) {
                //            return hallazgo.listado_detalles
                //                .filter(function (detalle) {
                //                    return detalle.tipo && detalle.tipo.toLowerCase().includes("recomendacion");
                //                })
                //                .map(function (detalle) {
                //                    return detalle.descripcion;
                //                })
                //                .join(", ");
                //        }).join("<br/>");

                //        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + recomendaciones + "</div>";
                //    },
                //    "name": "RECOMENDACION",
                //    "autoWidth": true,
                //    "orderable": false,
                //    "width": "20%"
                //},
                //{
                //    "data": "numerO_AUDITORIA_INTEGRAL",
                //    "render": function (data, type, row, meta) {
                //        return "";
                //    },
                //    "autoWidth": true,
                //    "orderable": false,
                //    "width": "10%"
                //},
            ],
            "language": {
                "url": "https://cdn.datatables.net/plug-ins/1.10.11/i18n/Spanish.json"
            },
            //"pagingType": "full_numbers",
            //"iDisplayLength": 25,
            //"lengthChange": false,
            //responsive: "true",
        });
    });

};

const tabResumida = document.getElementById('tabResumida');
const tabCompleta = document.getElementById('tabCompleta');
let tablaLLena = false;

// Añade eventos de clic para alternar la clase 'active'
tabResumida.addEventListener('click', function (e) {
    e.preventDefault(); // Evita que se recargue la página
    tabResumida.classList.add('active');
    tabCompleta.classList.remove('active');
    divTablaCompleta.setAttribute('hidden', true);
    divTablaResumida.removeAttribute('hidden');
});

tabCompleta.addEventListener('click', function (e) {
    e.preventDefault(); // Evita que se recargue la página
    tabCompleta.classList.add('active');
    tabResumida.classList.remove('active');
    divTablaResumida.setAttribute('hidden', true);
    divTablaCompleta.removeAttribute('hidden');
    if (tablaLLena == false) {
        GetAuditoriasIntegralesCompleta();
        tablaLLena = true;
    }
});