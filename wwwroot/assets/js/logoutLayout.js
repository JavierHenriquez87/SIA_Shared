function logoutSession() {
    event.preventDefault();
    $.ajax({
        type: 'GET',
        url: "/Acceso/Logout",
        dataType: 'json',
        success: function (result) {
            const newUrl = window.location.origin + window.location.pathname;
            window.history.pushState({ path: newUrl }, '', newUrl);
        },
        error: function (xhr, status, error) {
            // Manejar errores si es necesario
            console.error(error);
        }
    });
}