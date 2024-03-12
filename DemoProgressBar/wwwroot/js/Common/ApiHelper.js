const ContentType = {
    json: 'application/json'
}
function BaseApiBase() {
    let token = getTokenCookieBearer();
    let apiHelp = axios.create({
        baseURL: APISettings.BaseUrl,
        timeout: 3000,
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
            document.getElementById('LodingBoard').style.display = 'none';
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
            document.getElementById('LodingBoard').style.display = 'none';
            return Promise.reject(error);
        }
    );

    return apiHelp;
}

function axiosGet(url, params) {
    let apiHelp = window.BaseApiBase();
    apiHelp.get(url, {
            params: params
        })
        .then((response) => { return response; })
        .catch(function (error) {
            errorprocess(error.response);
        })
}

function errorprocess(response) {
    console.log(response);
    switch (response.status) {
        case 401:
            Swal.fire({
                icon: "warning",
                text: "登入資訊已過期，請重新登入"
            }).then(()=>{
                location.replace(PageUri.LoginPageUri);
            });
            
            break;
        default:
            Swal.fire({
                icon: "error",
                text: "發生錯誤!"
            });
            break
    }
  
}




