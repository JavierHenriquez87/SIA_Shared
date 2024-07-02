//FUNCION PARA OBTENER LAS AUDITORIAS INTEGRALES EN PANTALLA INICIAL
//==========================================================================================
GeAuditoriasIntegrales();
async function GeAuditoriasIntegrales() {
    $(document).ready(function () {
        // MOSTRAMOS TODA LA TABLA DE LAS AUDITORIAS ESPECIFICAS
        $('#tbAuditoriasIntegrales').DataTable({
            "processing": true,
            "serverSide": true,
            "filter": true,
            "ajax":
            {
                "url": '/Auditorias/GetAuditoriasIntegrales',
                "type": "POST",
                "datatype": "json"
            },
            "deferRender": true,
            "columns": [
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) { return row.codigO_AUDITORIA },
                    "name": "CODIGO",
                    "autoWidth": true,
                    "width": "20%"
                },
                {
                    "data": "nombrE_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        var text = "";
                        if (row.codigO_TIPO_AUDITORIA == 1) {
                            if (row.cantidaD_AUD_ESPEC == 0) {
                                text = "Auditoría";
                            } else if (row.cantidaD_AUD_ESPEC == 1) {
                                text = "Auditoría";
                            } else {
                                text = "Auditoría Integral";
                            }
                        } else {
                            text = "Auditoría Especial";
                        }
                        return "<div style='width: 100%; height: 100%; overflow: hidden; white-space: normal;'>" + text + " " + row.nombrE_AUDITORIA + " (" + row.cantidaD_AUD_ESPEC + ")</div>";
                    },
                    "name": "AUDITORIA",
                    "autoWidth": true,
                    "orderable": false,
                    "width": "50%"
                },
                {
                    "data": "codigO_ESTADO", "render": function (data, type, row, meta) {
                        if (row.codigO_ESTADO == 1) {
                            estado = "Auditoría Creada";
                            cssStatus = " statusAI create";
                        }
                        else if (row.codigO_ESTADO == 2) {
                            estado = "Creando Memorándum de Planificación";
                            cssStatus = " statusAI pendingcreate";
                        }
                        else if (row.codigO_ESTADO == 3) {
                            estado = "Pendiente Aprobación de Memorándum de Planificación";
                            cssStatus = " statusAI pending";
                        }
                        else if (row.codigO_ESTADO == 4) {
                            estado = "Creando Programas de Trabajo";
                            cssStatus = " statusAI pendingcreate";
                        }
                        else if (row.codigO_ESTADO == 5) {
                            estado = "Pendiente Aprobación de  Programas de Trabajo";
                            cssStatus = " statusAI pending";
                        }
                        else if (row.codigO_ESTADO == 6) {
                            estado = "Auditoría Revisada";
                            cssStatus = " statusAI create";
                        }
                        else if (row.codigO_ESTADO == 7) {
                            estado = "Pendiente Confirmación de Carta de Ingreso";
                            cssStatus = " statusAI pending";
                        } else if (row.codigO_ESTADO == 8) {
                            estado = "En Proceso";
                            cssStatus = " statusAI pendingcreate";
                        } else if (row.codigO_ESTADO == 9) {
                            estado = "Pendiente Recibir Informe Preliminar";
                            cssStatus = " statusAI pending";
                        } else if (row.codigO_ESTADO == 10) {
                            estado = "En Seguimiento de Informe Preliminar";
                            cssStatus = " statusAI pendingcreate";
                        } else if (row.codigO_ESTADO == 11) {
                            estado = "Evaluación y Revisión de Seguimiento de Informe Preliminar";
                            cssStatus = " statusAI evaluation";
                        } else if (row.codigO_ESTADO == 12) {
                            estado = "Auditoría en Seguimiento";
                            cssStatus = " statusAI success";
                        } else if (row.codigO_ESTADO == 13) {
                            estado = "Auditoría Ejecutada";
                            cssStatus = " statusAI success";
                        } else if (row.codigO_ESTADO == 14) {
                            estado = "Auditoría Anulada";
                            cssStatus = " statusAI pendingcreate";
                        }
                        return "<div class='tipo_auditoria " + cssStatus + "'>" + estado + "</div>";
                    },
                    "name": "ESTADO",
                    "autoWidth": true,
                    "orderable": false,
                    "width": "20%"
                },
                {
                    "data": "codigO_AUDITORIA",
                    "render": function (data, type, row, meta) {
                        let buttons = "<div class='optiongrid'>";

                        buttons += "<a title='Ver Auditoría' style='cursor:pointer;' onClick=\"AsignarNumAudInteSession("
                            + row.numerO_AUDITORIA_INTEGRAL + "," + row.cantidaD_AUD_ESPEC + "," + row.aniO_AI + ");\">"
                            + "<i class='fas fa-eye' style='color: gray;'></i></a>";


                        buttons += "</div>";

                        return buttons;
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

function AsignarNumAudInteSession(num_audit_integral, cant_audit_espec, anioAI) {
    $.ajax({
        method: 'POST',
        url: '/Auditorias/AsigAudIntegSession',
        data: {
            num_audit_integral: num_audit_integral,
            anio_audit_integral: anioAI
        },
        dataType: 'json',
        success: function (respuesta) {
            if (cant_audit_espec == 0) {
                window.location.href = 'Auditorias/ProgramarAuditoria/AuditoriaIndividual?first=false';
            } else {
                window.location.href = 'Auditorias/DetalleAuditoria';
            }
        },
        error: function () {

        }
    });
}