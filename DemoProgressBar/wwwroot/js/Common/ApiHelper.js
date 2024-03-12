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
    apiHelp.interceptors.request.use(
        function (config) {
            document.getElementById('LodingBoard').style.display = '';
            return config;
        },
        function (error) {
            // 对请求错误做些什么

            return Promise.reject(error);
        }
    );
    // 添加响应拦截器
    apiHelp.interceptors.response.use(
        function (response) {
            document.getElementById('LodingBoard').style.display = 'none';
       
            return response;
        },
        function (error) {
            // 对响应错误做些什么
            return Promise.reject(error);
        }
    );

    return apiHelp;
}




