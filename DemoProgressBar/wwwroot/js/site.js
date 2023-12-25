// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

let actkn = 'mg_T'

function getGoogleAcessToken() {
    return  localStorage.getItem('googleAuthToken');
}

function getAPIGoogleAcessToken() {
    return 'Bearer ' + localStorage.getItem('googleAuthToken');
}

function setGoogleAcessToken(token) {
    localStorage.setItem('googleAuthToken', token);
}

function removeGoogleAcessToken(token) {
    localStorage.removeItem("googleAuthToken");
}

function setCookie(name, value, expirationTimestamp) {
    // 將時間戳記轉換為 Date 物件
    const expirationDate = new Date(expirationTimestamp);

    // 設定 cookie 字符串
    const cookieString = `${name}=${encodeURIComponent(value)}; domain=localhost; secure; expires=${expirationDate.toUTCString()}; path=/`;

    // 設定 cookie
    document.cookie = cookieString;
}
function getCookie(name) {
    const cookieArray = document.cookie.split('; ');

    for (const cookie of cookieArray) {
        const [cookieName, cookieValue] = cookie.split('=');
        if (cookieName === name) {
            return decodeURIComponent(cookieValue);
        }
    }

    return null; // 如果找不到對應名稱的 cookie
}

function deleteCookie(name) {
    document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/; domain=moon719096service.uk; secure`;
}

//設定JWTTokenCookie
function setTokenCookie(value, expirationTimestamp) {
    setCookie(actkn, value, expirationTimestamp);
}
function getTokenCookie() {
    return getCookie(actkn);
}
function getTokenCookieBearer() {
    let result = null;
    let token = getCookie(actkn);
    if (token)
        result = 'Bearer ' + getCookie(actkn);
    return result;
}

