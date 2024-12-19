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
                    "data": "codigO_HALLAZGO",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.codigO_HALLAZGO + "</div>";
                    },
                    "name": "CODIGO HALLAZGO",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "condicion",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.condicion + "</div>";
                    },
                    "name": "CONDICION",
                    "autoWidth": true,
                    "orderable": false,
                    "width": "50%"
                },
                {
                    "data": "recomendacion", "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.recomendacion + "</div>";
                    },
                    "name": "RECOMENDACION",
                    "autoWidth": true,
                    "orderable": false,
                    "width": "20%"
                },
                {
                    "data": "anexos",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.anexos + "</div>";
                    },
                    "name": "ANEXOS",
                    "autoWidth": true,
                    "orderable": false,
                    "width": "10%"
                },
                {
                    "data": "accioneS_PREV_CORR",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.accioneS_PREV_CORR + "</div>";
                    },
                    "name": "ACCIONES_PREV_CORR",
                    "autoWidth": true,
                    "orderable": false,
                    "width": "10%"
                },
                {
                    "data": "evidencia",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.evidencia + "</div>";
                    },
                    "name": "EVIDENCIA",
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
                    "data": "correlativo",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.correlativo + "</div>";
                    },
                    "name": "correlativo",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "tipO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.tipO_AUDITORIA + "</div>";
                    },
                    "name": "TIPO_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "numerO_INFORME",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.numerO_INFORME + "</div>";
                    },
                    "name": "NUMERO_INFORME",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "fechA_EMISION_INF_FINAL",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.fechA_EMISION_INF_FINAL + "</div>";
                    },
                    "name": "FECHA_EMISION_INF_FINAL",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "unidaD_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.unidaD_AUDITORIA + "</div>";
                    },
                    "name": "UNIDAD_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "responsablE_UNI_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.responsablE_UNI_AUDITORIA + "</div>";
                    },
                    "name": "RESPONSABLE_UNI_AUDITORIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "codigO_HALLAZGO",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.codigO_HALLAZGO + "</div>";
                    },
                    "name": "CODIGO_HALLAZGO",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "descripcion",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.descripcion + "</div>";
                    },
                    "name": "DESCRIPCION",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "niveL_RIESGO",
                    "render": function (data, type, row, meta) {
                        let riesgoEstatus = row.niveL_RIESGO == 1 ? "Bajo" : row.niveL_RIESGO == 2 ? "Medio" : row.niveL_RIESGO == 2 ? "Alto" : "";

                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal; text-align: center;'>" + riesgoEstatus + "</div>";
                    },
                    "name": "NIVELL_RIESGO",
                    "autoWidth": true,
                    "width": "20%",
                    "createdCell": function (td, cellData, rowData, row, col) {
                        if (rowData.niveL_RIESGO == 1) {
                            $(td).addClass('highlight-bajo');
                        } else if (rowData.niveL_RIESGO == 2) {
                            $(td).addClass('highlight-medio');
                        } else if (rowData.niveL_RIESGO == 3) {
                            $(td).addClass('highlight-alto');
                        }
                    }
                },
                {
                    "data": "proceso",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.proceso + "</div>";
                    },
                    "name": "PROCESO",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "objetivO_CONTROL_INTERNO",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.objetivO_CONTROL_INTERNO + "</div>";
                    },
                    "name": "OBJECTIVO_CONTROL_INTERNO",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "objetivO_ESTRATEGICO",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.objetivO_ESTRATEGICO + "</div>";
                    },
                    "name": "OBJECTIVO_ESTRATEGICO",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "accioneS_PREV_CORRE",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.accioneS_PREV_CORRE + "</div>";
                    },
                    "name": "ACCIONES_PREV_CORRE",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "fechA_SOLUCION",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.fechA_SOLUCION + "</div>";
                    },
                    "name": "FECHA_SOLUCION",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "fechA_SOLUCIONO",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.fechA_SOLUCIONO + "</div>";
                    },
                    "name": "FECHA_SOLUCIONO",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "diaS_ATRAZO",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.diaS_ATRAZO + "</div>";
                    },
                    "name": "DIAS_ATRAZO",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "responsable",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.responsable + "</div>";
                    },
                    "name": "RESPONSABLE",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "evidencia",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.evidencia + "</div>";
                    },
                    "name": "EVIDENCIA",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "unidaD_APOYO",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.unidaD_APOYO + "</div>";
                    },
                    "name": "UNIDAD_APOYO",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "estatus",
                    "render": function (data, type, row, meta) {
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + row.estatus + "</div>";
                    },
                    "name": "ESTATUS",
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