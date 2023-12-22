
const vm = Vue.createApp({
    data() {
        return {
            ClientID: window.ClientID,
            apiHelp: null
            //google: self.google
        }
    },
    mounted() {
        this.apiHelp = axios.create({
            baseURL: 'https://localhost:7068',
            headers: {
                "Content-Type": "application/json"
            }
        })
        this.checkLoginStatus();
    },
    methods: {
        //確認登入狀況
        checkLoginStatus() {
            google.accounts.id.disableAutoSelect();
            this.deleteCookie("token");
            this.deleteCookie("username");
            this.deleteCookie("email");
            this.deleteCookie("userInfo");
            /*signoutItemDisplay(false);*/
            var userCookie = getTokenCookie();

            if (userCookie) {
                this.signoutItemDisplay(true);

                // go to new page
                //window.location.replace("/");

            } else {

                this.signoutItemDisplay(false);
                google.accounts.id.initialize({
                    client_id: this.ClientID,
                    callback: this.handleCredentialResponse
                });

                google.accounts.id.renderButton(
                    document.getElementById("buttonDiv"),
                    { type: "standard", shape: "rectangular", theme: "outline", size: "medium", text: "signin_with", logo_alignment: "left" }  // customization attributes
                );

                google.accounts.id.prompt(); // also display the One Tap dialog
            }
        },
        //取得cookie中的token
        getCookie(cookieName) {
            var name = cookieName + "=";
            var decodedCookie = decodeURIComponent(document.cookie);
            var cookieArray = decodedCookie.split(';');

            for (var i = 0; i < cookieArray.length; i++) {
                var cookie = cookieArray[i].trim();
                if (cookie.indexOf(name) === 0) {
                    return cookie.substring(name.length, cookie.length);
                }
            }
            return null;
        },
        //控制登出按鈕顯示
        signoutItemDisplay(display) {
            var signoutItem = document.getElementById("signout_item");
            if (signoutItem) {
                if (display) {
                    signoutItem.style.display = "inline";
                } else {
                    signoutItem.style.display = "none";
                }
            }
        },
        async handleCredentialResponse(response) {
            //呼叫GOOGLE登入api
            let getresult = await this.googleLogin(response.credential);
            if (getresult.code != 1) {
                return;
            }
            /* setGoogleAcessToken(getresult.JWT);*/
            const responsePayload = this.decodeJwtResponse(getresult.JWT);

            setTokenCookie(getresult.JWT, responsePayload.exp * 1000);

            //console.log("ID: " + responsePayload.sub);
            //console.log('Full Name: ' + responsePayload.name);
            ////console.log('Given Name: ' + responsePayload.given_name);
            ////console.log('Family Name: ' + responsePayload.family_name);
            //console.log("Image URL: " + responsePayload.picture);
            //console.log("Email: " + responsePayload.email);
            //console.log("Encoded JWT ID token: " + response.credential);

            // set up to cookie
            //var expirationDate = new Date();
            //var usernameCookie = `username=${responsePayload.given_name}${responsePayload.family_name}; path=/`;
            //expirationDate.setDate(expirationDate.getDate() + 14);
            //usernameCookie += "; expires=" + expirationDate.toUTCString() + " ; secure ;samesite=strict";
            //document.cookie = usernameCookie

            //var emailCookie = "email=" + responsePayload.email.toString() + "; path=/";
            //expirationDate.setDate(expirationDate.getDate() + 14);
            //emailCookie += "; expires=" + expirationDate.toUTCString() + " ; secure ;samesite=strict";
            //document.cookie = emailCookie

            //var tokenCookie = "token=" + response.credential + "; path=/";
            //expirationDate.setDate(expirationDate.getDate() + 14);
            //tokenCookie += "; expires=" + expirationDate.toUTCString() + " ; secure ;samesite=strict";
            //document.cookie = tokenCookie

            //var userData = {
            //    username: `${responsePayload.given_name}${responsePayload.family_name}`,
            //    email: responsePayload.email,
            //    token: response.credential
            //};

            //var userCookie = `userInfo=${JSON.stringify(userData)}; path=/;`;
            //expirationDate.setDate(expirationDate.getDate() + 14);
            //userCookie += `expires=${expirationDate.toUTCString()}; secure; samesite=strict`;
            //document.cookie = userCookie;

            // go to new page
            //window.location.replace("https://localhost:7068/WeatherForecast");

        },
        async googleLogin(token) {
            let result = {
                code: 0,
                JWT: null
            };
            let postdata = {
                credential: token
            }

            await this.apiHelp.post('api/GoogleAuth/Login', postdata)
                .then(function (response) {
                    result.code = 1;
                    result.JWT = response.data;
                })
                .catch(function (error) {

                });
            return result;
        },
        decodeJwtResponse(token) {
            var base64Url = token.split(".")[1];
            var base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
            var jsonPayload = decodeURIComponent(
                atob(base64)
                    .split("")
                    .map(function (c) {
                        return "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2);
                    })
                    .join("")
            );

            return JSON.parse(jsonPayload);
        },
        logOut() {
            google.accounts.id.disableAutoSelect();
            this.deleteCookie("token");
            this.deleteCookie("username");
            this.deleteCookie("email");
            this.deleteCookie("userInfo");
            this.signoutItemDisplay(false);
            window.location.replace("/Login/Login");
        },
        deleteCookie(cookieName) {
            var cookies = document.cookie.split(";");

            for (var i = 0; i < cookies.length; i++) {
                var cookie = cookies[i];
                var eqPos = cookie.indexOf("=");
                var name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
                if (name.trim() === cookieName) {
                    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
                    break;
                }
            }
        }
    }
}).mount('#app')

//window.onload = function () {
//    checkLoginStatus();
//}


//function checkLoginStatus() {
//        google.accounts.id.disableAutoSelect();
//        deleteCookie("token");
//        deleteCookie("username");
//        deleteCookie("email");
//        deleteCookie("userInfo");
//        /*signoutItemDisplay(false);*/
//    var userCookie = getCookie("token");

//    if (userCookie) {
//        signoutItemDisplay(true);

//        // go to new page
//        //window.location.replace("/Home/List");

//    } else {

//        signoutItemDisplay(false);

//        //google.accounts.id.initialize({
//        //    client_id: window.ClientID,
//        //    login_uri: 'https://localhost:7068/api/GoogleAuth/Login',
//        //    context: 'signin',
//        //    ux_mode: "popup",
//        //    //callback: handleCredentialResponse,
//        //    auto_select: true,
//        //    use_fedcm_for_prompt: true,
//        //    auto_prompt: true
//        //});

//        google.accounts.id.renderButton(
//            document.getElementById("g_id_signin"),
//            { theme: "outline", size: "large" }  // customization attributes
//        );

//        google.accounts.id.prompt(); // also display the One Tap dialog

//    }
//}

// After login success
//function handleCredentialResponse(response) {

//    const responsePayload = decodeJwtResponse(response.credential);
//    //console.log("ID: " + responsePayload.sub);
//    //console.log('Full Name: ' + responsePayload.name);
//    ////console.log('Given Name: ' + responsePayload.given_name);
//    ////console.log('Family Name: ' + responsePayload.family_name);
//    //console.log("Image URL: " + responsePayload.picture);
//    //console.log("Email: " + responsePayload.email);
//    //console.log("Encoded JWT ID token: " + response.credential);

//    // set up to cookie
//    var expirationDate = new Date();
//    var usernameCookie = `username=${responsePayload.given_name}${responsePayload.family_name}; path=/`;
//    expirationDate.setDate(expirationDate.getDate() + 14);
//    usernameCookie += "; expires=" + expirationDate.toUTCString() + " ; secure ;samesite=strict";
//    document.cookie = usernameCookie

//    var emailCookie = "email=" + responsePayload.email.toString() + "; path=/";
//    expirationDate.setDate(expirationDate.getDate() + 14);
//    emailCookie += "; expires=" + expirationDate.toUTCString() + " ; secure ;samesite=strict";
//    document.cookie = emailCookie

//    var tokenCookie = "token=" + response.credential + "; path=/";
//    expirationDate.setDate(expirationDate.getDate() + 14);
//    tokenCookie += "; expires=" + expirationDate.toUTCString() + " ; secure ;samesite=strict";
//    document.cookie = tokenCookie

//    var userData = {
//        username: `${responsePayload.given_name}${responsePayload.family_name}`,
//        email: responsePayload.email,
//        token: response.credential
//    };

//    var userCookie = `userInfo=${JSON.stringify(userData)}; path=/;`;
//    expirationDate.setDate(expirationDate.getDate() + 14);
//    userCookie += `expires=${expirationDate.toUTCString()}; secure; samesite=strict`;
//    document.cookie = userCookie;

//    // go to new page
//    window.location.replace("https://localhost:7068/WeatherForecast");

//}

//function getCookie(cookieName) {
//    var name = cookieName + "=";
//    var decodedCookie = decodeURIComponent(document.cookie);
//    var cookieArray = decodedCookie.split(';');

//    for (var i = 0; i < cookieArray.length; i++) {
//        var cookie = cookieArray[i].trim();
//        if (cookie.indexOf(name) === 0) {
//            return cookie.substring(name.length, cookie.length);
//        }
//    }
//    return null;
//}

// Tool
//function decodeJwtResponse(token) {
//    var base64Url = token.split(".")[1];
//    var base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
//    var jsonPayload = decodeURIComponent(
//        atob(base64)
//            .split("")
//            .map(function (c) {
//                return "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2);
//            })
//            .join("")
//    );

//    return JSON.parse(jsonPayload);
//}

// logout
//const button = document.getElementById('g_id_signin');
//button.onclick = () => {

//    google.accounts.id.disableAutoSelect();
//    deleteCookie("token");
//    deleteCookie("username");
//    deleteCookie("email");
//    deleteCookie("userInfo");
//    signoutItemDisplay(false);
//    window.location.replace("/Home/Index");
//}

//function deleteCookie(cookieName) {
//    var cookies = document.cookie.split(";");

//    for (var i = 0; i < cookies.length; i++) {
//        var cookie = cookies[i];
//        var eqPos = cookie.indexOf("=");
//        var name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
//        if (name.trim() === cookieName) {
//            document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
//            break;
//        }
//    }
//}

//function signoutItemDisplay(display) {
//    var signoutItem = document.getElementById("signout_item");
//    if (signoutItem) {
//        if (display) {
//            signoutItem.style.display = "inline";
//        } else {
//            signoutItem.style.display = "none";
//        }
//    }
//}