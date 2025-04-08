using Discord;
using Discord.WebSocket;
using System.Linq;

namespace DemoProgressBarAPI.Services
{
    public class DiscordService
    {
        private readonly DiscordSocketClient _client;
        private readonly string _botId;

        public DiscordService(string botid,string token)
        {
            // 使用包含 MessageContent 意圖的設定
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };
            _botId = botid;
            _client = new DiscordSocketClient(config);
            _client.Log += LogAsync;
            // 註冊事件，當收到訊息時觸發 MessageReceived
            _client.MessageReceived += OnMessageReceivedAsync;
            // 將 await 改為同步呼叫方法，因為建構子無法是 async
            InitializeAsync(token).GetAwaiter().GetResult();
        }

        public async Task InitializeAsync(string token)
        {
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(ulong channelId, string message)
        {
            var channel = _client.GetChannel(channelId) as IMessageChannel;
            if (channel != null)
            {
                await channel.SendMessageAsync(message);
            }
        }

        /// <summary>
        /// 接收訊息事件，當有新訊息時觸發。
        /// </summary>
        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            // 避免機器人自身回覆自己的訊息
            
            if (message.Author.IsBot|| !message.MentionedUsers.Any(user => user.Id.ToString() == _botId))
                return;
            // 嘗試取得發送訊息的使用者
            var guildUser = message.Author as SocketGuildUser;
            if (guildUser == null)
                return;
            // 檢查使用者是否具有管理員權限
            bool isAdministrator = guildUser.GuildPermissions.Administrator;
            // 使用正則表達式移除提及部分，取得訊息正文
            string contentWithoutMentions = RemoveMentionsFromContent(message);
            // 基本指令範例：當收到 "!ping" 回覆 "Pong!"
            if (contentWithoutMentions.Equals("!ping", StringComparison.OrdinalIgnoreCase))
            {
                await message.Channel.SendMessageAsync("Pong!");
            }
            else
            {
                if (isAdministrator)
                {
                    await message.Channel.SendMessageAsync($"{message.Author.Mention} 管理員逼嘴");
                }
                else { 
                    await message.Channel.SendMessageAsync($"{message.Author.Mention} 逼嘴");
                }
            }

           

            // 更多互動：
            // 您可以在此根據不同指令或條件增加不同回應邏輯，
            // 例如：辨識 !help 指令，自動傳送相關說明內容。
        }

        /// <summary>
        /// 使用正則表達式移除訊息中的提及部分，返回純正文內容。
        /// </summary>
        /// <param name="message">收到的訊息</param>
        /// <returns>移除提及後的訊息內容</returns>
        private string RemoveMentionsFromContent(SocketMessage message)
        {
            string content = message.Content;

            // 正則表達式匹配 <@UserId> 格式的提及
            string mentionPattern = @"<@!?(\d+)>";

            // 使用正則表達式移除所有提及部分
            content = System.Text.RegularExpressions.Regex.Replace(content, mentionPattern, string.Empty);

            // 移除多餘的空格
            return content.Trim();
        }
    }
}
