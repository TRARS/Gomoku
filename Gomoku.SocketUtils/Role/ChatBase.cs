using Gomoku.SocketUtils.Helper.Extensions;
using Gomoku.SocketUtils.Helper.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;

namespace Gomoku.SocketUtils.Role
{
    // 非同期処理でソケット情報を保持する為のオブジェクト
    public class StateObject
    {
        // 受信バッファサイズ
        public const int BufferSize = 1024;

        // 受信バッファ
        public byte[] buffer = new byte[BufferSize];

        // 受信データ
        public StringBuilder sb = new StringBuilder();

        // ソケット
        public Socket workSocket = null;

        // 粘包flag
        public bool packetSticking = false;
    }

    // 启动标志
    public enum StartState
    {
        Started = 0,
        Starting,
        StartFalse,
        None
    }

    // 作为客户端，向服务端报道时，的信件格式
    public record ClientInitialAuthentication([property: JsonPropertyName("Name")] string Name,
                                              [property: JsonPropertyName("Age")] string Age);


    // 发消息，转发消息，收消息，使用通用格式
    public record ClientMessage([property: JsonPropertyName("SenderName")] string SenderName,
                                [property: JsonPropertyName("ReceiverName")] string ReceiverName,
                                [property: JsonPropertyName("Message")] string Message,
                                [property: JsonPropertyName("AdditionalPayload")] AdditionalPayload AdditionalPayload);

    // 追加消息
    public class AdditionalPayload
    {
        [JsonPropertyName("SenderColor")]
        public string SenderColor { get; set; }

        [JsonPropertyName("SenderAvatarIdx")]
        public int SenderAvatarIdx { get; set; }

        [JsonPropertyName("ExMessageType")]
        public ExMessageType ExMessageType { get; set; }

        [JsonPropertyName("ExMessage")]
        public string ExMessage { get; set; }

        [JsonPropertyName("ExObject")]
        public object? ExObject { get; set; }

        public AdditionalPayload(string senderColor, int senderAvatarIdx)
        {
            // 必填
            SenderColor = senderColor;
            SenderAvatarIdx = senderAvatarIdx;

            // 选填
            ExMessageType = ExMessageType.Chat;
            ExMessage = string.Empty;
            ExObject = null;
        }
    }

    // 追加消息的类型
    [JsonConverter(typeof(JsonStringEnumConverter<ExMessageType>))]
    public enum ExMessageType
    {
        Chat = 0,          //聊天消息
        SystemAlert,       //系统公告 ExMessage
        SystemReply,       //首登回复 ExMessage
        GameMatching,      //发起对战 ExObject
        GameConfirm,       //确认对战 ExObject
        GameInProgress,    //正在对战 ExObject
        WinnerAlert,       //结算公告 ExObject
    }
}

namespace Gomoku.SocketUtils.Role
{
    public partial class ChatBase
    {
        protected StartState StartState = StartState.None;
        protected string CharacterName { get; init; }
        protected string CharacterAge { get; init; }
        protected readonly string EOF = "<EOF>";


        private readonly IJsonService _jsonService;
        private readonly IDeflateService _deflateService;

        public ChatBase()
        {
            _jsonService = new ServiceCollection().TryAddAllService()
                                                  .BuildServiceProvider()
                                                  .GetRequiredService<IJsonService>();
            _deflateService = new ServiceCollection().TryAddAllService()
                                                     .BuildServiceProvider()
                                                     .GetRequiredService<IDeflateService>();
        }


        // 序列化
        protected string JsonSerialize<T>(T jsonObject)
        {
            return _jsonService.JsonSerialize(jsonObject);
        }
        // 反序列化
        protected T? JsonDeserialize<T>(string jsonText)
        {
            return _jsonService.JsonDeserialize<T>(jsonText);
        }

        // 压缩
        protected byte[] DeflateCompressText(string text)
        {
            return _deflateService.CompressText(text);
        }
        protected byte[] DeflateCompressText(byte[] text)
        {
            return _deflateService.CompressText(text);
        }
        // 解压缩
        protected string DeflateDecompressData(byte[] compressedData)
        {
            return _deflateService.DecompressData(compressedData);
        }
    }

    public partial class ChatBase
    {
        private Random random = new Random();
        protected string GenerateRandomString(int length)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                stringBuilder.Append(chars[index]);
            }

            return stringBuilder.ToString();
        }
    }
}