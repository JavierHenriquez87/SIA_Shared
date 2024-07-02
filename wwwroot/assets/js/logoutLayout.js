function logoutSession() {
    $.ajax({
        type: 'GET',
        url: "/Acceso/Logout",
        dataType: 'json',
        success: function (result) {
        },
        error: function (xhr, status, error) {
            // Manejar errores si es necesario
            console.error(error);
        }
    });
}