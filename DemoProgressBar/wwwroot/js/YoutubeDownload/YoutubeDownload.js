
const { createApp } = Vue

createApp({
    data() {
        return {
            InputListID: null,
            SearchList: [],
            SelectData: [],
            isShowDownload: false,
            Donloadprogress: {
                progress: 0,
                message: ''
            },
            hub: null,
        }
    },
    created() {
        window.LoginCheck();
    },
    mounted() {
        const self = this
        let YTDownloadHubUrl = new URL(APISettings.YTDownloadHubUri, APISettings.BaseUrl).href;
        const token = window.getTokenCookie();
        self.hub = new signalR.HubConnectionBuilder()
            .withUrl(YTDownloadHubUrl, {
                accessTokenFactory: () => token // 在這裡提供標頭
            }) // 你的 SignalR Hub 地址
            .withAutomaticReconnect()
            .build();

        //thatA.apiHelp = axios.create({
        //    baseURL: APISettings.BaseUrl,
        //    headers: {
        //        "Content-Type": "application/json",
        //        "Authorization": token
        //    }
        //})
    },
    methods: {
        initSignalR(self = this) {
            //與Server建立連線
            self.hub.start().then(function () {
                console.log("連線完成");
            }).catch(function (err) {
                alert('連線錯誤: ' + err.toString());
            });
            // 更新進度
            self.hub.on("YoutubeDownloadProgress", function (message, percent) {
                if (percent == 100) {
                    thatA.Donloadprogress.message = '檔案壓縮中..';
                } else {
                    thatA.Donloadprogress.progress = percent;
                    thatA.Donloadprogress.message = message + ' ' + percent + '%';
                }
            });
        },
        //取得音樂清單
        listget: function () {
            this.SearchList = [];
            let params = {
                PlaylistId: null
            }
            //檢查傳入資料格式
            let checkresult = this.listGetCheck(this.InputListID);
            if (checkresult) {
                return;
            }
            //取得ListID
            let nlistid = this.getListID(this.InputListID);
            if (!nlistid) {
                return;
            }

            params.PlaylistId = nlistid;
            //呼叫api查詢清單
            let apiHelp = window.BaseApiBase();
            apiHelp
                .get(APISettings.YTDownloadPlayListGetUri, {
                    params: params
                })
                .then((response) => {
                    let datas = response.data;
                    if (datas !== null && datas.length > 0) {
                        this.SearchList = response.data;
                    } else {
                        Swal.fire({
                            icon: "error",
                            text: "查無資料"
                        });
                    }
                })
                .catch(function (error) {
                    console.log(error);
                    Swal.fire({
                        icon: "error",
                        text: "發生錯誤!"
                    });
                })

        },
        listGetCheck(inputdata) {
            let isOK = false;
            //判斷傳入的質是否為空
            if (window.isWhiteSpace(inputdata)) {
                Swal.fire({
                    icon: "error",
                    text: "請輸入網址或是ListID"
                });
                isOK = true;
            }
            return isOK;
        },
        checkUrlPath(urlpath) {
            // 定義簡單的URL正規表達式
            var urlPattern = /^(https?:\/\/)?([\w-]+(\.[\w-]+)+\/?)([\w-./?%&=]*)?$/;

            // 使用正規表達式進行匹配
            return urlPattern.test(urlpath);
        },
        getListString(url) {
            let urlParams = new URLSearchParams(new URL(url).search);

            // 獲取 "list" 參數的值
            let listParam = urlParams.get("list");

            return listParam;
        },
        getListID(inputdata) {
            //先判斷書入的是不是網址
            if (!this.checkUrlPath(inputdata)) {
                //否 擷取為list參數存入變數
                return inputdata;
            }

            // 獲取 "list" 參數的值
            let listParam = this.getListString(inputdata);
            if (listParam == null || listParam === undefined || listParam == '') {
                //判斷沒有跳出錯誤訊息
                Swal.fire({
                    icon: "error",
                    text: "請確認您輸入的是合法的網址"
                });
                return;
            } else {
                //判斷有list參數 存入變數
                return listParam;
            }
        },
        //執行音樂下載
        download: function () {
            //document.getElementById('Download_Btn').disabled = true;
            //this.isShowDownload = true;
            //篩選有勾選的資料
            let ndata = _.filter(this.SearchList, ['isCheck', true]);
            //只取id 與 title欄位
            this.SelectData = _.map(ndata, obj => _.pick(obj, ['id', 'title']));
            let apiHelp = window.BaseApiBase();
            apiHelp.post(APISettings.YoutubeDownloadUri,
                this.SelectData,
                {
                    responseType: 'blob'
                })
                .then((response) => this.downloadData(response))
                .catch((error) => console.log(error))
        },
        //壓縮完後 下載檔案
        downloadData: function (data) {
            if (!data) {
                return
            }
            let url = window.URL.createObjectURL(new Blob([data.data]))
            let link = document.createElement('a')
            link.style.display = 'none'
            link.href = url

            let timestamp = new Date().getTime();
            link.download = `${timestamp}.zip`;
            document.body.appendChild(link);
            link.click();
            this.downloadComplate();
            document.getElementById('Download_Btn').disabled = false;
        },
        //下載完成後設定
        downloadComplate: function () {
            this.Donloadprogress.message = '檔案下載完成';
            this.Donloadprogress.progress = 0;
            this.Donloadprogress.message = '';
            this.isShowDownload = false;
        }
    }
}).mount('#YoutubeDonloadApp')