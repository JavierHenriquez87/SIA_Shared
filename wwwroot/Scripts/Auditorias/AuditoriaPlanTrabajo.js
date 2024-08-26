function handleClick(element) {
    var codigoPdt = element.getAttribute('data-codigo-pdt');
    codigoPdtEncode = encodeBase64(codigoPdt);
    window.location.href = 'ProgramaTrabajo?pdt=' + codigoPdtEncode;
}

//FUNCION PARA CODIFICAR A BASE64 LOS DATOS DE LA URL
function encodeBase64(str) {
    return btoa(unescape(encodeURIComponent(str)));
}