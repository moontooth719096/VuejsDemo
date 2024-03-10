let actkn = 'mg_T'
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

function deleteCookie(cookieName) {
    var cookies = document.cookie.split(";");

    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i];
        var eqPos = cookie.indexOf("=");
        var name = eqPos > -1 ? cookie.substring(0, eqPos) : cookie;
        if (name.trim() === cookieName) {
            document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
            break;
        }
    }
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

function removeTokenCookie() {
    deleteCookie(actkn);
}


