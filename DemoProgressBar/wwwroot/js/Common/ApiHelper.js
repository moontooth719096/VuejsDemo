const ContentType = {
    json: 'application/json'
}
function BaseApiBase() {
    let token = getTokenCookieBearer();
    let apiHelp = axios.create({
        baseURL: APISettings.BaseUrl,
        headers: {
            "Content-Type": ContentType.json,
            "Authorization": token
        }
    })
    return apiHelp;
}
