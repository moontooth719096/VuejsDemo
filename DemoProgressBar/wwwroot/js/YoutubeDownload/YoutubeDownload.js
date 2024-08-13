
const { createApp } = Vue

createApp({
    data() {
        return {
            InputListID: null,
            SearchList: [],
            SelectData: [],
            Donloadprogress: {
                progress: 0,
                message: ''
            },
            hub: null,
            UrlType:{
                PlayListType: 2,
                VedioType:1
            }
        }
    },
    created() {
        LoginCheck();
        let YTDownloadHubUrl = new URL(APISettings.YTDownloadHubUri, APISettings.BaseUrl).href;
        const token = window.getTokenCookie();
        this.hub = new signalR.HubConnectionBuilder()
            .withUrl(YTDownloadHubUrl, {
                accessTokenFactory: () => token // 在這裡提供標頭
            }) // 你的 SignalR Hub 地址
            .withAutomaticReconnect()
            .build();
        this.initSignalR(this);
    },
    mounted() {
        this.$refs.urlinput.focus();
    },
    methods: {
        initSignalR(self) {
            //與Server建立連線
            self.hub.start().then(function () {
                console.log("連線完成");
            }).catch(function (err) {
                console.log(err);
                Swal.fire({
                    icon: "error",
                    text: '連線錯誤: ' + err.toString()
                });
            });

            // 更新進度
            self.hub.on("YoutubeDownloadProgress", function (message, percent) {
                if (percent == 100) {
                    self.Donloadprogress.message = '檔案壓縮中..';
                } else {
                    self.Donloadprogress.progress = percent;
                    self.Donloadprogress.message = message + ' ' + percent + '%';
                }
            });
        },
        //取得音樂清單
         listget:async function () {
            this.SearchList = [];
            //檢查傳入資料格式
            let isNotOK = this.listGetCheck(this.InputListID);
            if (isNotOK) {
                return;
            }

            //判斷是ListID 還是 VideoID
            let checkGet = this.getID(this.InputListID);
             if (checkGet.Type == null || checkGet.Type === undefined || checkGet.Type == '') {
                //判斷沒有跳出錯誤訊息
                Swal.fire({
                    icon: "error",
                    text: "請確認您輸入的是合法的網址"
                });
                return;
            } 

            //依照Type呼叫API
            switch (checkGet.Type) {
                case this.UrlType.VedioType:
                    this.videoAPICall(checkGet.ID);
                    break;
                case this.UrlType.PlayListType:
                    this.playListAPICall(checkGet.ID);
                    break;
                default:
                    break;
            }
        },
        listGetCheck(inputdata) {
            //判斷傳入的質是否為空
            if (window.isWhiteSpace(inputdata)) {
                Swal.fire({
                    icon: "error",
                    text: "請輸入網址或是ListID"
                });
                return true;
            }
            //判斷輸入的是不是網址
            if (!isUrlPath(inputdata)) {
                Swal.fire({
                    icon: "error",
                    text: "請輸入網址或是ListID"
                });
                return true;
            }
            return false;
        },
        getUrlParamKey(url,key) {
            let urlParams = new URLSearchParams(new URL(url).search);
            // 獲取 "list" 參數的值
            let listParam = urlParams.get(key);
            return listParam;
        },
        getID(url) {
            let result = {
               Type:null,
                ID: null
            };
            // 獲取 "list" 參數的值
            let listParam = this.getUrlParamKey(url,"list");
            if (listParam !== null && listParam !== undefined && listParam != '') {
                //判斷有list參數 存入變數
                result.Type = this.UrlType.PlayListType;
                result.ID = listParam
                return result;
            };

            // 獲取 "v" 參數的值
            let videoID = this.getUrlParamKey(url,"v");
            if (videoID !== null && videoID !== undefined && videoID != '') {
                //判斷有list參數 存入變數
                result.Type = this.UrlType.VedioType;
                result.ID = videoID
                return result;
            };
            return result;
        },
        async videoAPICall(videoid) {
            let params = {
                VideoID: videoid
            };

            let response = await axiosGet(APISettings.YTDownloadVideoGetUri, {
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
        async playListAPICall(playlistid) {
            let params = {
                PlaylistId: playlistid
            };

            let response = await axiosGet(APISettings.YTDownloadPlayListGetUri, {
                params: params
            })

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
        //執行音樂下載
        download: async function () {
            //篩選有勾選的資料
            let ste = this.SearchList.filter(data => data.isCheck);
            let ndata = _.filter(this.SearchList, ['isCheck', true]);
            //只取id 與 title欄位
            this.SelectData = _.map(ndata, obj => _.pick(obj, ['id', 'title']));
            let result = await axiosPost(APISettings.YoutubeDownloadUri, this.SelectData, 1000000, 'blob');
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
        }
    }
}).mount('#YoutubeDonloadApp')