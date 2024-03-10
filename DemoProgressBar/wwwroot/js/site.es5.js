'use strict';

var _slicedToArray = (function () { function sliceIterator(arr, i) { var _arr = []; var _n = true; var _d = false; var _e = undefined; try { for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) { _arr.push(_s.value); if (i && _arr.length === i) break; } } catch (err) { _d = true; _e = err; } finally { try { if (!_n && _i['return']) _i['return'](); } finally { if (_d) throw _e; } } return _arr; } return function (arr, i) { if (Array.isArray(arr)) { return arr; } else if (Symbol.iterator in Object(arr)) { return sliceIterator(arr, i); } else { throw new TypeError('Invalid attempt to destructure non-iterable instance'); } }; })();

var actkn = 'mg_T';
function setCookie(name, value, expirationTimestamp) {
    // 將時間戳記轉換為 Date 物件
    var expirationDate = new Date(expirationTimestamp);

    // 設定 cookie 字符串
    var cookieString = name + '=' + encodeURIComponent(value) + '; domain=localhost; secure; expires=' + expirationDate.toUTCString() + '; path=/';

    // 設定 cookie
    document.cookie = cookieString;
}
function getCookie(name) {
    var cookieArray = document.cookie.split('; ');

    var _iteratorNormalCompletion = true;
    var _didIteratorError = false;
    var _iteratorError = undefined;

    try {
        for (var _iterator = cookieArray[Symbol.iterator](), _step; !(_iteratorNormalCompletion = (_step = _iterator.next()).done); _iteratorNormalCompletion = true) {
            var cookie = _step.value;

            var _cookie$split = cookie.split('=');

            var _cookie$split2 = _slicedToArray(_cookie$split, 2);

            var cookieName = _cookie$split2[0];
            var cookieValue = _cookie$split2[1];

            if (cookieName === name) {
                return decodeURIComponent(cookieValue);
            }
        }
    } catch (err) {
        _didIteratorError = true;
        _iteratorError = err;
    } finally {
        try {
            if (!_iteratorNormalCompletion && _iterator['return']) {
                _iterator['return']();
            }
        } finally {
            if (_didIteratorError) {
                throw _iteratorError;
            }
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
    var result = null;
    var token = getCookie(actkn);
    if (token) result = 'Bearer ' + getCookie(actkn);
    return result;
}

function removeTokenCookie() {
    deleteCookie(actkn);
}

