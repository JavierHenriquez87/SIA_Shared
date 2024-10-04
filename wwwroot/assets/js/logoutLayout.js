function logoutSession() {
    sessionStorage.clear();
    localStorage.clear();
    const newUrl = window.location.origin;
    window.history.pushState({ path: newUrl }, '', newUrl);
}