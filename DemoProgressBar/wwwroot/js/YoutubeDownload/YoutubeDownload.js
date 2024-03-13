
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
        LoginCheck();
    },
    mounted() {
        let YTDownloadHubUrl = new URL(APISettings.YTDownloadHubUri, APISettings.BaseUrl).href;
        const token = window.getTokenCookie();
        this.hub = new signalR.HubConnectionBuilder()
            .withUrl(YTDownloadHubUrl, {
                accessTokenFactory: () => token // 在這裡提供標頭
            }) // 你的 SignalR Hub 地址
            .withAutomaticReconnect()
            .build();
        this.initSignalR();
        this.$refs.urlinput.focus();
    },
    methods: {
        initSignalR() {
            //與Server建立連線
            this.hub.start().then(function () {
                console.log("連線完成");
            }).catch(function (err) {
                console.log(err);
                Swal.fire({
                    icon: "error",
                    text: '連線錯誤: ' + err.toString()
                });
            });

            // 更新進度
            this.hub.on("YoutubeDownloadProgress", function (message, percent) {
                if (percent == 100) {
                    this.Donloadprogress.message = '檔案壓縮中..';
                } else {
                    this.Donloadprogress.progress = percent;
                    this.Donloadprogress.message = message + ' ' + percent + '%';
                }
            });
        },
        //取得音樂清單
        listget:async function () {

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

            let response = await axiosGet(APISettings.YTDownloadPlayListGetUri, {
                params: params
            });

            //判斷回傳是否有值
            if (response != null && response.data !== null && response.data.length > 0) {
                this.SearchList = response.data;
                this.$refs.downloadbtn.focus();
            } else {
                Swal.fire({
                    icon: "error",
                    text: "查無資料"
                });
            }

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
        getListString(url) {
            let urlParams = new URLSearchParams(new URL(url).search);

            // 獲取 "list" 參數的值
            let listParam = urlParams.get("list");

            return listParam;
        },
        getListID(inputdata) {
            //先判斷書入的是不是網址
            if (!isUrlPath(inputdata)) {
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
        download: async function () {
            //篩選有勾選的資料
            let ndata = _.filter(this.SearchList, ['isCheck', true]);
            //只取id 與 title欄位
            this.SelectData = _.map(ndata, obj => _.pick(obj, ['id', 'title']));
            let result = await axiosPost(APISettings.YoutubeDownloadUri, this.SelectData, 300000, 'blob');
            if (result !== null && result.data !== null) {
                this.downloadData(result);
            }
        },
        //壓縮完後 下載檔案
        downloadData: function (data) {
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