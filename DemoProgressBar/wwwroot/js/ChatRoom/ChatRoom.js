const vm = Vue.createApp({
    data() {
        return {
            chatlist: [],//聊天對象清單
            connectionid: null,//自己的連線ID
            signalRconnect: null,
            nowtalkid: null,//當前聊天對象ID
            talklist: [],//聊天內容清單
            nowtalk: [],//當前聊天對象內容
            keyonmessage: '',//對話框輸入內容
            config: window.appSettings,
        }
    },
    created() {
        var self = this;
        self.signalRconnect = new signalR.HubConnectionBuilder()
            .withUrl(self.config.ChatHubUrl, {
                accessTokenFactory: () => getTokenCookie(), // 在這裡提供標頭
            }) // 你的 SignalR Hub 地址
            .withAutomaticReconnect()
            .build();
        self.initSigmalR(self);
    },
    methods: {
       async initSigmalR(self = this) {
            await self.signalRconnect.start()
                .then(() => {
                    self.connectionid = self.signalRconnect.connectionId;
                    console.log('SignalR 连接已建立');
                })
                .catch((error) => {
                    console.error('SignalR 连接失败:', error);
                });

            //監聽上線者清單
            self.signalRconnect.on("OnlineList", function (onlineusers) {
                self.chatlist = onlineusers;
                ////將新加入的使用者新增到聊天對象清單
                //if (onlineuser.connectionID != self.connectionid)
                //    self.chatlist.push(onlineuser);
            });

            //監聽有使用者連線
            self.signalRconnect.on("UserConnected", function (onlineuser) {
                //將新加入的使用者新增到聊天對象清單
                if (onlineuser.connectionID != self.connectionid)
                    self.chatlist.push(onlineuser);
            });
            //監聽使用者離線
            self.signalRconnect.on("UserDisconnected", function (offlineuserid) {
                let nowusers = self.chatlist;
                //確認聊天對象清單是否存在該使用者
                let user = _.find(nowusers, function (o) { return o.connectionID == offlineuserid });

                //如果有的話就取得不包含該離線者的資料重新放入聊天對象清單
                if (user != null && user !== undefined)
                    self.chatlist = _.filter(nowusers, function (o) { return o.connectionID != user.connectionID; })

                //如果離線的是目前聊天的對象，則將當前聊天對象ID清空
                if (self.nowtalkid == offlineuserid) {
                    self.talkselect(null);
                }

            });
            //監聽私訊訊息
            self.signalRconnect.on("PrivateMessage", function (senduser, message) {
                let nowusers = self.chatlist;
                //抓取目前清單裡有沒有這個人
                let user = _.find(nowusers, function (o) { return o.connectionID == senduser.connectionID });

                //判斷使用者清單沒有這個人就加上
                if (user == null || user == undefined) {
                    senduser.lastMesage = message;
                    self.chatlist.push(senduser);
                } else {
                    if (self.nowtalkid != user.connectionID)
                        user.noReadCount = user.noReadCount + 1;
                    user.lastMesage = message;
                }

                //將收到的訊息加到對話清單裡
                self.addTalk(senduser.connectionID, senduser.connectionID, message);

                

                //判斷目前沒有選擇跟任何人聊天,就給目前私訊你的人
                if (self.nowtalkid == null || self.nowtalkid == undefined) {
                    self.talkselect(senduser.connectionID);
                }
            });
            
        },
        //設定要聊天的人
        talkselect(selectid) {
            this.nowtalkid = selectid;
            let nowusers = this.chatlist;
            let user = _.find(nowusers, function (o) { return o.connectionID == selectid });
            //判斷使用者清單沒有這個人就加上
            if (user != null && user != undefined) {
                if (user.noReadCount>0)
                    user.noReadCount = 0;
            }
            this.refreshChat();
        },
        //發送訊息
        sendmessage() {
            let nowusers = this.chatlist;
            let nowtalk = this.nowtalkid;
            if (this.isWhiteSpace(this.keyonmessage))
                return;
            this.signalRconnect.invoke("PrivateMessage", this.nowtalkid, this.keyonmessage).catch(function (err) {
                alert('傳送錯誤: ' + err.toString());
                return;
            });

            let user = _.find(nowusers, function (o) { return o.connectionID == nowtalk });

            if (user != null && user != undefined) {
                user.lastMesage = this.keyonmessage;
            }

            this.addTalk(this.nowtalkid, this.connectionid, this.keyonmessage);
            // this.talklist.push({ talkid: this.nowtalkid, message: this.keyonmessage });
            this.keyonmessage = '';
        },

        addTalk(talkid, sayid, message) {
            //將收到的訊息加到對話清單裡
            let talk = _.find(this.talklist, function (o) { return o.talkid == talkid });
            if (talk != null && talk != undefined) {
                talk.talks.push({ talkid: sayid, message: message });
            }
            else {
                talk = { talkid: talkid, talks: [{ talkid: sayid, message: message }] };
                this.talklist.push(talk);
            }

            if (talk.talkid == this.nowtalkid)
                this.refreshChat();
        },
        //刷新當前對話內容
        refreshChat() {
            //查詢聊天內容清單中是否有這個聊天對象的聊天室
            let nowtalkid = this.nowtalkid;
            let nowtalks = _.find(this.talklist, function (o) { return o.talkid == nowtalkid });//o.talkid.includes(nowtalkid)

            //如果抓出聊天內容清單中有這個聊天室ID 就將目前聊天的內容設定為這個聊天室的內容
            if (nowtalks != null && nowtalks != undefined) {
                this.nowtalk = _.cloneDeep(nowtalks.talks);
            }
            else {
                this.nowtalk = [];
            }

            // 使用Vue.nextTick確保對話資料渲染完畢後觸發scrollbar保持在最下變
            this.$nextTick(() => {
                this.autoScrollToBottom();
            });
        },
        //將對話框的Scrollbar保持在最下方
        autoScrollToBottom() {
            // 抓取scrollbar的區塊
            let container = this.$refs.scrollContainer;
            if (container.scrollHeight == undefined)
                return;
            container.scrollTop = container.scrollHeight;
        },
        //判斷是否有空白
        isWhiteSpace(text) {
            const regex = /^\s*$/;
            return regex.test(text);
        }

    },
    //watch: {
    //    // 監聽 當前聊天對象ID 這個變數是否有變化
    //    nowtalkid: function () {
    //        //變化時直接刷新當前對話內容
    //        this.refreshChat();
    //    }
    //}
}).mount('#app')