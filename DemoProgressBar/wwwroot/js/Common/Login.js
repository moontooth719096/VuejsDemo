var userinfo;

const PageUri = {
    "LoginPageUri": "/Login/Login",
    "HomePage": "/"

}
async function LoginCheck() {
    let checkresult = await AppLoginCheck();
    let isLiginPage = (window.location.pathname == PageUri.LoginPageUri);
    if (checkresult) {
        if (isLiginPage) {
            //如果token合法 但是當前在登入頁時導向首頁
            window.location.replace(PageUri.HomePage);
        }
    } else if (!isLiginPage) {
        //如果token不合法，且現在不在登入頁則跳至登入頁
        window.location.replace(PageUri.LoginPageUri);
    }
}

async function AppLoginCheck() {
    var userCookie = getTokenCookieBearer();
    let result = false;
    if (userCookie) {
        //驗證token是否過期
        let apihelper = BaseApiBase();
        await apihelper.get(APISettings.LoginCheck)
            .then((response) => {
                //console.log(response);
                result = true;
            })
            .catch((error) => {
                console.error(error);
            });
    } else {
        console.log("NoAppToken");
    }
    return result;
}

function AppLogOut() {
    AppLogOutClear();
    let isLiginPage = (window.location.pathname == PageUri.LoginPageUri);
    //如果登出時當下不在登入頁，則轉向登入頁
    if (!isLiginPage) {
        location.replace(PageUri.LoginPageUri);
    }
}

function AppLogOutClear() {
    //deleteCookie("token");
    //deleteCookie("username");
    //deleteCookie("email");
    //deleteCookie("userInfo");
    userinfo = null;
    removeTokenCookie();
}