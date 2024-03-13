const ContentType = {
    json: 'application/json'
}
function BaseApiBase(timeoutset = 3000) {
    let token = getTokenCookieBearer();
    let apiHelp = axios.create({
        baseURL: APISettings.BaseUrl,
        timeout: timeoutset,
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

            return Promise.resolve(response);
        },
        function (error) {
            document.getElementById('LodingBoard').style.display = 'none';
            return Promise.reject(error);
        }
    );

    return apiHelp;
}

function axiosGet(url, params,timeoutset = 3000) {
    //apiHelp.get(url, params)
    //    .then((response) => { return Promise.resolve(response);})
    //    .catch(function (error) {
    //        errorprocess(error.response);
    //    })
    return new Promise((resolve) => {
        let apiHelp = BaseApiBase(timeoutset);
        apiHelp.get(url, params)
            .then(response => {
                resolve(response);
            })
            .catch(error => {
                errorprocess(error.response);
                //reject(error);
            });
    });
}

function axiosPost(url, params, timeoutset = 3000, responsetype = 'application/json') {
    //apiHelp.get(url, params)
    //    .then((response) => { return Promise.resolve(response);})
    //    .catch(function (error) {
    //        errorprocess(error.response);
    //    })
    return new Promise((resolve) => {
        let apiHelp = BaseApiBase(timeoutset);
        apiHelp.post(url, params,{
            responseType: responsetype
            })
            .then((response) =>resolve(response))
            .catch((error) => errorprocess(error.response))
    });
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




