
const { createApp } = Vue

createApp({
    data() {
        return {
            InputListID: '',
            SearchList: [],
            SelectData: [],
            isShowDownload: false,
            Donloadprogress: {
                progress: 0,
                message: ''
            },
            hub: {
                connection: {}
                , HubConnId: ''
            },
            Settings: window.appSettings,
            apiHelp: null
        }
    },
    created() {
        window.LoginCheck();
    },
    mounted() {
        const thatA = this
        let YTDownloadHubUrl = new URL(thatA.Settings.YTDownloadHubUri, thatA.Settings.API_BASE).href;
        thatA.hub.connection = new signalR.HubConnectionBuilder()
            .withUrl(YTDownloadHubUrl)// 你的 SignalR Hub 地址
            .build();

        //與Server建立連線
        thatA.hub.connection.start().then(function () {
            console.log("連線完成");
        }).catch(function (err) {
            alert('連線錯誤: ' + err.toString());
        });
        // 更新進度
        thatA.hub.connection.on("YoutubeDownloadProgress", function (message, percent) {
            if (percent == 100) {
                thatA.Donloadprogress.message = '檔案壓縮中..';
            } else {
                thatA.Donloadprogress.progress = percent;
                thatA.Donloadprogress.message = message + ' ' + percent + '%';
            }
        });
        let token = window.getTokenCookieBearer();
        thatA.apiHelp = axios.create({
            baseURL: thatA.Settings.API_BASE,
            headers: {
                "Content-Type": "application/json",
                "Authorization": token
            }
        })
    },
    methods: {
        //取得音樂清單
        listget: function () {
            let params = {
                PlaylistId: this.InputListID
            }
            //this.apiHelp.get(this.Settings.YTDownloadPlayListGetUri, { params })
            //    .then(function (response) {
            //       if (response.data.length > 0) {
            //           this.SearchList = response.data;
            //       } 
            //    })
            //    .catch(function (error) {
            //        console.log(error);
            //    });
            let token = window.getTokenCookieBearer();
            let url = new URL(this.Settings.YTDownloadPlayListGetUri, this.Settings.API_BASE).href;
            axios
                .get(url, {
                    headers: {
                        "Authorization": `Bearer ${token}`
                    },
                    params: params
                })
                .then((response) => {
                    if (response.data.length > 0) {
                        this.Data = response.data;
                    }
                })
                .catch(function (error) { // 请求失败处理
                    console.log(error);
                })

        },
        //執行音樂下載
        download: function () {
            document.getElementById('Download_Btn').disabled = true;
            this.isShowDownload = true;
            //篩選有勾選的資料
            let ndata = _.filter(this.SearchList, ['isCheck', true]);
            //只取id 與 title欄位
            this.SelectData = _.map(ndata, obj => _.pick(obj, ['id', 'title']));
            // axios.post('https://localhost:44353/api/YoutubeDownload/Download',
            this.apiHelp.post(this.Settings.YoutubeDownloadUrl,
                this.SelectData,
                {
                    responseType: 'blob'
                })
                .then((response) => this.downloadData(response))
                .catch((error) => console.log(error))
            //axios.post(self.appSettings.YoutubeDownloadUrl,
            //    this.SelectData,
            //    {
            //        responseType: 'blob'
            //    })
            //    .then((response) => this.downloadData(response))
            //    .catch((error) => console.log(error))
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
            //link.setAttribute('download', 'excel.xlsx')

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