const ContentType = {
    json: 'application/json'
}
function BaseApiBase() {
    let token = getTokenCookieBearer();
    let apiHelp = axios.create({
        baseURL: 'https://localhost:7068/',
        headers: {
            "Content-Type": ContentType.json,
            "Authorization": token
        }
    })
    return apiHelp;
}
