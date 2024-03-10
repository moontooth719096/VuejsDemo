
const vm = Vue.createApp({
    data() {
        return {
            ClientID: GoogleOAuth.ClientID,
            //apiHelp: null
            //google: self.google
        }
    },
    created: function () {
        window.LoginCheck();
    },
    mounted() {
        this.initLoginPage();
    },
    methods: {
        initLoginPage() {
            this.logOut();
            //初始化google登入按鈕
            google.accounts.id.initialize({
                client_id: this.ClientID,
                callback: this.handleCredentialResponse,
                use_fedcm_for_prompt: true,
            });

            google.accounts.id.renderButton(
                document.getElementById("buttonDiv"),
                { type: "standard", shape: "rectangular", theme: "filled_black", size: "medium", text: "signin", logo_alignment: "left" }  // customization attributes
            );

            google.accounts.id.prompt(); // also display the One Tap dialog
        },
        //google登入按鈕的callback
        async handleCredentialResponse(response) {
            //呼叫GOOGLE登入api
            let getresult = await this.googleLogin(response.credential);
            if (getresult.code != 1) {
                return;
            }
            const responsePayload = this.decodeJwtResponse(getresult.JWT);
            //token寫入Cookie
            setTokenCookie(getresult.JWT, responsePayload.exp * 1000);
            //再回去檢查一次token
            window.LoginCheck();

        },
        async googleLogin(token) {
            let result = {
                code: 0,
                JWT: null
            };
            let postdata = {
                credential: token
            }
            let apiHelp = window.BaseApiBase();
            await apiHelp.post(APISettings.GoogleLoginUri, postdata)
                .then(function (response) {

                    result.code = 1;
                    result.JWT = response.data;
                })
                .catch(function (error) {
                    console.error(error);
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
            window.AppLogOutClear();
        },
       
    }
}).mount('#app')