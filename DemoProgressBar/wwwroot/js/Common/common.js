//判斷字串為空
function isWhiteSpace(text) {
    const regex = /^\s*$/;
    return regex.test(text);
}

function isUrlPath(urlpath) {
    // 定義簡單的URL正規表達式
    const regex = /^(https?:\/\/)?([\w-]+(\.[\w-]+)+\/?)([\w-./?%&=]*)?$/;
    // 使用正規表達式進行匹配
    return regex.test(urlpath);
}