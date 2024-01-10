using ColorHelper;
using CommunityToolkit.Mvvm.Input;
using Gomoku.Core.Helper.Base;
using Gomoku.Core.Helper.Extensions;
using Gomoku.Core.Playground.Interface;
using Gomoku.Core.Rule;
using Gomoku.SocketUtils.Role;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ColorConverter = System.Windows.Media.ColorConverter;
using MenuType = Gomoku.UI.Control.CustomControlEx.MenuButtonEx.ButtonType;
using MultiPlayground = Gomoku.Core.Playground.MultiPlayground;
using Playground = Gomoku.Core.Playground.Playground;
using StoneType = Gomoku.UI.Control.CustomControlEx.StoneButtonEx.ButtonType;


// 汇总
namespace Gomoku.UI.Control.UserControlEx.ClientEx
{
    /// <summary>
    /// 协调 viewmodel 和 Gomoku.Core 之间的的交互
    /// </summary>
    partial class uClient_controller
    {
        private (string[] name, string[] color, int[] arr) roll; //随机属性

        private uClient_viewmodel viewmodel;
        private IPlayground playground;
        private MatchHelper matchHelper;

        public uClient_controller(uClient_viewmodel vm)
        {
            viewmodel = vm;
            playground = Playground.Instance;
            matchHelper = MatchHelper.Instance;

            roll = RandomizeCharacter();

            ChessBoardDelegateInit();
            ChessBoardViewModelInit();

            ChatViewModelInit();
        }
    }
}

// 与棋盘交互的部分
namespace Gomoku.UI.Control.UserControlEx.ClientEx
{
    /// <summary>
    /// 声明棋盘网格尺寸，网格数量等
    /// </summary>
    partial class uClient_controller
    {
        private const double gridSize = 32;
        private const int horizontalGridCount = 14; // 15根线，14个格，13个有效点
        private const int verticalGridCount = 14;
        private const double stoneSize = gridSize - 1;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    partial class uClient_controller
    {
        private MenuButtonGroupViewModel singlePlayMenuButtonGroupVM;
        private MenuButtonGroupViewModel multiPlayMenuButtonGroupVM;

        private ChessBoardViewModel chessBoardVM;
        private StoneViewModel tempStoneVM;

        private void ChessBoardDelegateInit()
        {
            MediatorAsync.Instance.Register(AsyncMessageType.UIReset, async (para, token) =>
            {
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    chessBoardVM.StoneVMList.Clear();
                });

                return null;
            });
        }
        private void ChessBoardViewModelInit()
        {
            chessBoardVM = viewmodel.ChessBoardViewModel = new(gridSize, horizontalGridCount, verticalGridCount, new(MouseClick), new(MouseMove));
            tempStoneVM = chessBoardVM.TempStoneVM = new(StoneType.RedStone, stoneSize / 2, new(0), false, Visibility.Collapsed);

            singlePlayMenuButtonGroupVM = viewmodel.SinglePlayMenuButtonGroupViewModel = new();
            singlePlayMenuButtonGroupVM.MenuVMList.Add(new("单机对战", roll.color[0], double.NaN, 25, new(StartGame)));
            singlePlayMenuButtonGroupVM.MenuVMList.Add(new("结束对战", roll.color[0], double.NaN, 25, new(StopGame)));
            singlePlayMenuButtonGroupVM.MenuVMList.Add(new("悔棋", roll.color[0], double.NaN, 25, new(UndoMove)));

            multiPlayMenuButtonGroupVM = viewmodel.MultiPlayMenuButtonGroupViewModel = new();
            multiPlayMenuButtonGroupVM.MenuVMList.Add(new("联机对战", roll.color[0], double.NaN, 25, new(SendMatchRequest), MenuType.Custom));
            multiPlayMenuButtonGroupVM.MenuVMList.Add(new("提前退场", roll.color[0], double.NaN, 25, new(AbortOnlineGame), MenuType.Custom));
        }
    }

    /// <summary>
    /// 初始化用的方法
    /// </summary>
    partial class uClient_controller
    {
        // 单机按钮
        private async Task StartGame(object? args)
        {
            if (playground is MultiPlayground) { return; }
            await playground.StartGame(horizontalGridCount, verticalGridCount, async (isWinner) => { await Task.CompletedTask; });
        }
        private async Task StopGame(object? args)
        {
            if (playground is MultiPlayground) { return; }
            await playground.StopGame();
        }
        private async Task UndoMove(object? args)
        {
            if (playground is MultiPlayground) { return; }
            if (playground.IsGameNotStarted) { return; }

            if (await playground.UndoMove() is false) { return; }

            if (chessBoardVM.StoneVMList.LastItem() is not null)
            {
                chessBoardVM.StoneVMList.RemoveLastItem();
                chessBoardVM.StoneVMList.LastItem()?.SetHighLight(true);
            }
        }

        // 单机联机通用拿坐标方法
        private async Task MouseClick(object? para)
        {
            if (playground.IsGameNotStarted) { return; }

            Point relativePoint = Mouse.GetPosition((IInputElement)para!);
            double x = relativePoint.X;
            double y = relativePoint.Y;

            if (IsOutOfBounds(x, y) is false)
            {
                int nx = (int)((x - gridSize / 2) / gridSize);
                int ny = (int)((y - gridSize / 2) / gridSize);

                var chessPoint = new ChessPoint(nx, ny);

                if (await playground.CursorMove(chessPoint) is not ChessMoveStatus.Forbidden)
                {
                    var type = (await playground.CursorClick(chessPoint)) switch
                    {
                        ChessPieceColor.Black => StoneType.BlackStone,
                        ChessPieceColor.White => StoneType.WhiteStone,
                        _ => throw new NotImplementedException()
                    };
                    var diameter = stoneSize;
                    var radius = diameter / 2;

                    chessBoardVM.StoneVMList.LastItem()?.SetHighLight(false);
                    chessBoardVM.StoneVMList.Add(new(type, diameter, new((chessPoint.X + 1) * gridSize - radius, (chessPoint.Y + 1) * gridSize - radius, 0, 0), true));

                    tempStoneVM.Type = StoneType.RedStone;

                    if (matchHelper.IsInGameProgress)
                    {
                        await playground.SetMultiPlayChessPoint(chessPoint); // 己方落子
                        await this.MultiPlayMouseClick(chessPoint);          // 落子消息发送给对方
                    }
                }
            }
        }
        private async Task MouseMove(object? para)
        {
            Point relativePoint = Mouse.GetPosition((IInputElement)para!);
            double x = relativePoint.X;
            double y = relativePoint.Y;

            if (IsOutOfBounds(x, y))
            {
                tempStoneVM.Visibility = Visibility.Collapsed;
            }
            else
            {
                int nx = (int)((x - gridSize / 2) / gridSize);
                int ny = (int)((y - gridSize / 2) / gridSize);

                if (playground.IsGameNotStarted)
                {
                    tempStoneVM.Type = StoneType.RedStone;
                }
                else
                {
                    switch (await playground.CursorMove(new(nx, ny)))
                    {
                        case ChessMoveStatus.Forbidden:
                            {
                                tempStoneVM.Type = StoneType.RedStone;
                                break;
                            }
                        case ChessMoveStatus.AllowBlack:
                            {
                                tempStoneVM.Type = StoneType.BlackStone;
                                break;
                            }
                        case ChessMoveStatus.AllowWhite:
                            {
                                tempStoneVM.Type = StoneType.WhiteStone;
                                break;
                            }
                    }
                }

                var radius = tempStoneVM.Diameter / 2;
                tempStoneVM.Visibility = Visibility.Visible;
                tempStoneVM.Margin = new Thickness((nx + 1) * gridSize - radius, (ny + 1) * gridSize - radius, 0, 0);
            }
        }

        private bool IsOutOfBounds(double inputX, double inputY)
        {
            var radius = gridSize / 2;
            var maxWidth = gridSize * horizontalGridCount;
            var maxHeight = gridSize * verticalGridCount;

            return (inputX < radius || inputY < radius || inputX >= maxWidth - radius || inputY >= maxHeight - radius);
        }
    }
}

// 与聊天窗交互的部分
namespace Gomoku.UI.Control.UserControlEx.ClientEx
{
    /// <summary>
    /// 声明本机服务端、客户端
    /// </summary>
    partial class uClient_controller
    {
        private ChatServer chatServer;
        private ChatClient chatClient;
        private CooldownTimer cooldownTimer;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    partial class uClient_controller
    {
        private ChatRoomJoinerViewModel chatRoomJoinerVM;

        private ChatHistoryViewModel chatHistoryVM;
        private ChatInputViewmodel chatInputVM;
        private ChatServerViewModel chatServerVM;
        private ChatClientViewModel chatClientVM;

        private void ChatViewModelInit()
        {
            cooldownTimer = new(); // 计时器

            chatRoomJoinerVM = viewmodel.ChatRoomJoinerViewModel = new("サキュバス", roll.name[0], roll.color[0], roll.arr[0]);

            chatHistoryVM = viewmodel.ChatHistoryViewModel = new();
            chatInputVM = viewmodel.ChatInputViewModel = new(new(SendMessageToServer), new(StartClient), chatRoomJoinerVM.ClientColor);
            chatServerVM = viewmodel.ChatServerViewModel = new(chatRoomJoinerVM.TryGetLocalAddress(), 0, new(StartServer));
            chatClientVM = viewmodel.ChatClientViewModel = new(chatRoomJoinerVM.TryGetLocalAddress(), "0", new(LoginConfirm), new(LoginCancel));

            chatServer = new ChatServer(chatRoomJoinerVM.ServerName);
            chatClient = new ChatClient(chatRoomJoinerVM.ClientName);
        }
    }

    /// <summary>
    /// 操作 ChatHistoryViewModel、ChatInputViewmodel 等等
    /// </summary>
    partial class uClient_controller
    {
        /// <summary>
        /// 发送消息至服务端（收件人为服务端时，由服务端代理群发）
        /// </summary>
        private async Task SendMessageToServer(object? input)
        {
            chatInputVM.UserMessage = string.Empty;

            // 防呆
            if (chatClient.IsStarted is false)
            {
                chatHistoryVM.SystemMessages.Add(new($"尚未登录"));
                return;
            }

            // 防回车按死
            if (chatClient.IsStarted && cooldownTimer["Z4nd@TBxsJPzL2b&"].Elapsed(50) is false)
            {
                if ($"{input}".Length == 0)
                {
                    chatHistoryVM.SystemMessages.Add(new($"请勿发送空消息"));
                    return;
                }

                // 发送常规消息
                var exPayload = new AdditionalPayload(chatRoomJoinerVM.ClientColor, chatRoomJoinerVM.ClientAvatarIdx);
                await chatClient.SendMessageToClient(chatRoomJoinerVM.ServerName, $"{input}", exPayload);
            }
        }

        /// <summary>
        /// 接收来自服务端的消息
        /// </summary>
        private void OnReceiveServerMessage(ClientMessage obj)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                switch (obj.AdditionalPayload.ExMessageType)
                {
                    case ExMessageType.SystemAlert:
                    case ExMessageType.SystemReply:
                        {
                            // 系统消息
                            chatHistoryVM.SystemMessages.Add(new(obj.AdditionalPayload));
                            break;
                        }
                    case ExMessageType.GameMatching:
                        {
                            // 已开始对战时，拒收对战请求消息
                            if (matchHelper.IsInGameProgress) { break; }

                            // 先反序列化
                            var temp = matchHelper.JsonDeserialize<MatchObject>($"{obj.AdditionalPayload.ExObject}")!;

                            // 本机发送的消息不需要显示 
                            if (temp.ChallengerClientName == chatRoomJoinerVM.ClientName) { break; }

                            // 接收对战请求消息
                            var matchObject = temp;

                            // 接收并储存外机发给本机的对战请求消息
                            matchHelper.ChallengeRequest = matchObject;
                            obj.AdditionalPayload.ExObject = matchObject;


                            // 判断是否有人应战
                            if (matchObject.RequesterClientName == chatRoomJoinerVM.ClientName && matchObject.ResponseToChallenge)
                            {
                                // 开始对战
                                _ = this.ConfirmMatchRequest(null);
                            }

                            // 可能需要显示的对战请求消息
                            chatHistoryVM.GameMessages.Add(new(obj.AdditionalPayload)
                            {
                                AcceptCommand = new(AcceptMatch),
                                RejectCommand = new(RejectMatch)
                            });
                            break;
                        }
                    case ExMessageType.GameConfirm:
                        {
                            // 接收一条回复
                            var matchObject = matchHelper.JsonDeserialize<MatchObject>($"{obj.AdditionalPayload.ExObject}")!;

                            // 已开始对战时，不更新 matchHelper.ChallengeRequest
                            if (matchHelper.IsInGameProgress is false) { matchHelper.ChallengeRequest = matchObject; }

                            // 接收外机发给本机的对战确认消息
                            obj.AdditionalPayload.ExObject = matchObject;

                            // 本机发送的消息不需要显示 
                            if (matchObject.ChallengerClientName == chatRoomJoinerVM.ClientName) { break; }

                            // 若对方离场，己方也离场
                            if (matchObject.AbortGame && matchObject.ChallengerClientName == matchHelper.ChallengeRequest.ChallengerClientName)
                            {
                                _ = AbortOnlineGame(null);
                            }

                            // 可能需要显示的对战确认消息（IsChallengeRequest = false 时不显示 接受/拒绝 按钮）
                            chatHistoryVM.GameMessages.Add(new(obj.AdditionalPayload)
                            {
                                AcceptCommand = new(AcceptMatch),
                                RejectCommand = new(RejectMatch)
                            });

                            break;
                        }
                    case ExMessageType.GameInProgress:
                        {
                            if (matchHelper.IsInGameProgress is false) { return; }

                            // 接收落子消息，先反序列化
                            var matchObject = matchHelper.JsonDeserialize<MatchObject>($"{obj.AdditionalPayload.ExObject}")!;

                            // 接收并储存外机发给本机的落子消息
                            Task.Run(async () =>
                            {
                                var chessPoint = matchObject.ChessPoint;

                                var type = (await playground.CursorClick(chessPoint)) switch
                                {
                                    ChessPieceColor.Black => StoneType.BlackStone,
                                    ChessPieceColor.White => StoneType.WhiteStone,
                                    _ => throw new NotImplementedException()
                                };
                                var diameter = stoneSize;
                                var radius = diameter / 2;

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    chessBoardVM.StoneVMList.LastItem()?.SetHighLight(false);
                                    chessBoardVM.StoneVMList.Add(new(type, diameter, new((chessPoint.X + 1) * gridSize - radius, (chessPoint.Y + 1) * gridSize - radius, 0, 0), true));

                                    tempStoneVM.Type = StoneType.RedStone;
                                });

                                await playground.SetMultiPlayChessPoint(chessPoint);
                            });

                            // 展示落子坐标
                            //obj.AdditionalPayload.ExObject = matchObject;
                            //chatHistoryVM.GameMessages.Enqueue(new(obj.AdditionalPayload));
                            break;
                        }
                    case ExMessageType.WinnerAlert:
                        {
                            // 接收结算消息，先反序列化
                            var matchObject = matchHelper.JsonDeserialize<MatchObject>($"{obj.AdditionalPayload.ExObject}")!;

                            obj.AdditionalPayload.ExObject = matchObject;
                            chatHistoryVM.WinnerMessages.Add(new(obj.AdditionalPayload));

                            break;
                        }
                    case ExMessageType.Chat:
                        {
                            // 一般聊天消息
                            chatHistoryVM.ChatMessages.Add(new()
                            {
                                IsOwnMessage = obj.SenderName == chatRoomJoinerVM.ClientName,
                                Message = $"{obj.Message}",
                                SenderName = chatRoomJoinerVM.TryGetClientRealName(obj.SenderName),
                                BorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString($"{obj.AdditionalPayload.SenderColor}")),
                                ImageSource = $"./Control/IconEx/{obj.AdditionalPayload.SenderAvatarIdx + 1:00}.png" // 图片01起步所以+1
                            });
                            break;
                        }
                    default: { throw new NotImplementedException(); }
                }
            });

        }

        /// <summary>
        /// 启动服务端
        /// </summary>
        private async Task StartServer(object? args)
        {
            if (chatServerVM.ServerPort > 0)
            {
                Debug.WriteLine("服务端已启动");
            }
            else
            {
                var address = chatServerVM.ServerAddress;
                var port = chatServerVM.ServerPort;
                var flag = await chatServer.StartListening(address, port, (port) =>
                {
                    chatServerVM.ServerPort = port;
                    chatServerVM.ServerLock = true;

                    chatClientVM.ServerAddress = address;
                    chatClientVM.ServerPort = $"{port}";
                });

                if (flag is false)
                {
                    IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                    var ipv4 = localIPs.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(x => x);
                    var info = string.Join(",\n", ipv4);
                    await Task.Run(() => MessageBox.Show($"创建服务端失败，请填入正确本机IP。\n本机局域网IP地址列表:\n{info}"));

                    chatServerVM.ServerLock = false;
                }
            }
        }

        /// <summary>
        /// 启动客户端
        /// </summary>
        [RelayCommand]
        private async Task StartClient(object? args)
        {
            await Task.CompletedTask;

            // 显示确认窗口
            if (chatClient.IsStarted is false)
            {
                chatClientVM.ConfirmWindowVisibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 登录确认
        /// </summary>
        private async Task LoginConfirm(object? args)
        {
            var address = chatClientVM.ServerAddress;
            var port = chatClientVM.ServerPort;
            var flag = false;

            if (int.TryParse(port, out var _port))
            {
                flag = await chatClient.StartClient(address, _port, OnReceiveServerMessage);
            }

            if (flag)
            {
                // 登录成功，发送一条系统消息。
                var exPayload = new AdditionalPayload(chatRoomJoinerVM.ClientColor, chatRoomJoinerVM.ClientAvatarIdx)
                {
                    ExMessageType = ExMessageType.SystemAlert,
                    ExMessage = $"<{chatRoomJoinerVM.ClientRealName}> 上线了！"
                };
                await chatClient.SendMessageToClient(chatRoomJoinerVM.ServerName, string.Empty, exPayload);
            }
            else
            {
                MessageBox.Show("客户端启动失败。");
            }

            chatClientVM.ConfirmWindowVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 登录取消
        /// </summary>
        private async Task LoginCancel(object? args)
        {
            await Task.CompletedTask;

            chatClientVM.ConfirmWindowVisibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// 随机设置客户端名称等
    /// </summary>
    partial class uClient_controller
    {
        /// <summary>
        /// 随机
        /// </summary>
        private (string[] name, string[] color, int[] arr) RandomizeCharacter()
        {
            // 预设名字
            string[] cat_name = new string[40] { "みぃこ", "るな", "ねずこ", "ことみ", "ゆずは", "みお", "あやの", "さやか", "れいこ", "ほのか", "みゆき", "あかね", "れいか", "さゆり", "みやび", "ゆずき", "あやか", "さとみ", "あすか", "かほ", "みさき", "あいこ", "かれん", "ゆきな", "かおり", "まゆみ", "かのん", "はなこ", "あかり", "かすみ", "かなで", "みわこ", "くれは", "あいか", "ひなた", "あんな", "えりこ", "れいな", "ももこ", "ましろ" };

            // 初始化数组
            int[] arr = Enumerable.Range(0, 40).ToArray();
            // 初始化随机数生成器
            Random rand = new Random(new Random().Next());

            // 预设边框颜色
            string[] cat_colors = ColorInit();

            // 使用 Fisher–Yates 洗牌算法打乱数组
            for (int i = arr.Length - 1; i > 0; i--)
            {
                Swap(cat_name, i, rand.Next(0, i + 1));
                Swap(arr, i, rand.Next(0, i + 1));
                Swap(cat_colors, i, rand.Next(0, i + 1));
            }

            //尝试检测
            bool hasAdjacentPairs = true;
            while (hasAdjacentPairs)
            {
                hasAdjacentPairs = false;
                for (int i = 0; i < arr.Length - 1; i++)
                {
                    if (Math.Abs(arr[i] - arr[i + 1]) < 11) // 相邻两数差值小于11
                    {
                        int j = i + (Math.Abs(arr[i] - arr[i + 1]) + new Random().Next(0, 15));
                        if (j > arr.Length - 1) { j -= (arr.Length); }

                        Swap(arr, i, j); // 交换这两个数字
                        Swap(cat_colors, i, j); // 通过洗arr数组，顺便洗颜色
                        hasAdjacentPairs = true;
                    }
                }
            }

            return (cat_name, cat_colors, arr);
        }

        /// <summary>
        /// 成员交换
        /// </summary>
        private void Swap<T>(T[] array, int i, int j)
        {
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }

        /// <summary>
        /// 色相滚动
        /// </summary>
        private string[] ColorInit()
        {
            string[] result = new string[40];

            Color color = Colors.Brown;//基础色

            for (int i = 0; i < 40; i++)
            {
                HSV hsv = ColorHelper.ColorConverter.RgbToHsv(new RGB(color.R, color.G, color.B));
                hsv.H += (i * 9);
                RGB rgb = ColorHelper.ColorConverter.HsvToRgb(hsv);

                result[i] = $"#{rgb.R:x2}{rgb.G:x2}{rgb.B:x2}";
            }

            return result;
        }
    }

}

// 对战交互的部分
namespace Gomoku.UI.Control.UserControlEx.ClientEx
{
    /// <summary>
    /// 对战消息处理
    /// </summary>
    partial class uClient_controller
    {
        /// <summary>
        /// 对战消息封装（请求对战、开始对战）
        /// </summary>
        public sealed class MatchObject
        {
            // 请求对战阶段
            [JsonPropertyName("ChallengerClientName")]
            public string ChallengerClientName { get; set; }

            [JsonPropertyName("ChallengerClientRealName")]
            public string ChallengerClientRealName { get; set; }

            [JsonPropertyName("ChallengerMessage")]
            public string ChallengerMessage { get; set; }

            [JsonPropertyName("ChallengerColor")]
            public string ChallengerColor { get; set; }

            [JsonPropertyName("RequesterClientName")]
            public string RequesterClientName { get; set; }

            [JsonPropertyName("IsChallengeRequest")]
            public bool IsChallengeRequest { get; set; }

            [JsonPropertyName("AbortGame")]
            public bool AbortGame { get; set; }

            [JsonIgnore]
            public bool ResponseToChallenge => !IsChallengeRequest;

            // 开始对战阶段
            [JsonPropertyName("ChessPoint")]
            public ChessPoint ChessPoint { get; set; }

            [JsonPropertyName("ChessColor")]
            public ChessPieceColor ChessColor { get; set; }


            //
            public MatchObject()
            {
                IsChallengeRequest = true;
            }
        }

        /// <summary>
        /// 对战消息处理辅助类
        /// </summary>
        private sealed class MatchHelper
        {
            private static readonly Lazy<MatchHelper> lazyObject = new(() => new());
            public static MatchHelper Instance => lazyObject.Value;

            private JsonSerializerOptions options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            private MatchHelper() { }

            public MatchObject? JsonDeserialize<MatchObject>(string jsonText)
            {
                try
                {
                    return JsonSerializer.Deserialize<MatchObject>(jsonText, options);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"JsonDeserialize Error—{ex.Message}");
                    return default;
                }
            }

            public MatchObject ChallengeRequest; //接收对战消息用
            public bool IsInMatchingGame = true;
            public bool IsInGameProgress => !IsInMatchingGame;
        }

        /// <summary>
        /// 发起对战请求（发消息）
        /// </summary>
        private async Task SendMatchRequest(object? args)
        {
            // 防呆
            if (chatClient.IsStarted is false)
            {
                chatHistoryVM.SystemMessages.Add(new($"尚未登录"));
                return;
            }

            // 防呆
            if (matchHelper.IsInMatchingGame is false)
            {
                chatHistoryVM.SystemMessages.Add(new($"正在对战中"));
                return;
            }

            // 群发对战请求
            var exPayload = CreateGameMatchingMessage("申请对战", ChessPieceColor.Black);
            await chatClient.SendMessageToClient(chatRoomJoinerVM.ServerName, "123", exPayload);

            // 系统提示
            chatHistoryVM.SystemMessages.Add(new($"已发起对战请求"));
        }

        /// <summary>
        /// 提前退出对战
        /// </summary>
        private async Task AbortOnlineGame(object? args)
        {
            // 对战结束，改回单机广场
            if (matchHelper.IsInGameProgress)
            {
                await playground.StopGame(); // 结束联机广场的游戏

                var exPayload = CreateGameConfirmMessage("已经离场", isAbortGame: true);
                await chatClient.SendMessageToClient(matchHelper.ChallengeRequest.ChallengerClientName, string.Empty, exPayload);

                // 清除缓存
                matchHelper.ChallengeRequest = null;

                // 回归单机广场
                matchHelper.IsInMatchingGame = true;
                playground = Playground.Instance;
            }
        }

        /// <summary>
        /// 接受对战请求（发消息）
        /// </summary>
        private async Task AcceptMatch(object? args)
        {
            // 清空所有请求对战消息
            chatHistoryVM.GameMessages.Clear();

            // 已经开始对战，不需要再发送"接受对战"消息
            if (matchHelper.IsInGameProgress) { return; }

            // 发送"接受对战"消息
            var exPayload = CreateGameMatchingMessage("响应了你的对战请求", ChessPieceColor.White, matchHelper.ChallengeRequest.ChallengerClientName, false);
            await chatClient.SendMessageToClient(matchHelper.ChallengeRequest.ChallengerClientName, string.Empty, exPayload);

            // 设置flag，表示应战后不再接收对战请求消息
            matchHelper.IsInMatchingGame = false;

            // 开始对战
            await this.MultiPlayStartGame(ChessPieceColor.White);// 应战方为白色
        }

        /// <summary>
        /// 拒绝对战请求
        /// </summary>
        private async Task RejectMatch(object? args)
        {
            // 清空所有请求对战消息
            chatHistoryVM.GameMessages.Clear();

            await Task.CompletedTask;
        }


        /// <summary>
        /// 确认对战（发消息）
        /// </summary>
        private async Task ConfirmMatchRequest(object? args)
        {
            // 清空所有请求对战消息
            chatHistoryVM.GameMessages.Clear();

            // 已经开始对战，不需要再发送"确认对战"消息
            if (matchHelper.IsInGameProgress) { return; }

            Debug.WriteLine($"有人应战: {matchHelper.ChallengeRequest.ChallengerClientName}");

            // 发送"接受对战"消息
            var exPayload = CreateGameConfirmMessage("接受了你的挑战");
            await chatClient.SendMessageToClient(matchHelper.ChallengeRequest.ChallengerClientName, string.Empty, exPayload);

            // 设置flag，表示应战后不再接收对战请求消息
            matchHelper.IsInMatchingGame = false;

            // 开始对战
            await this.MultiPlayStartGame(ChessPieceColor.Black);// 主办方为黑色
        }

        /// <summary>
        /// 联机落子（发消息）
        /// </summary>
        private async Task MultiPlayMouseClick(ChessPoint pt)
        {
            // 发送"我方落子"消息
            var exPayload = new AdditionalPayload(chatRoomJoinerVM.ClientColor, chatRoomJoinerVM.ClientAvatarIdx)
            {
                ExMessageType = ExMessageType.GameInProgress,
                ExObject = new MatchObject()
                {
                    ChallengerClientName = chatRoomJoinerVM.ClientName,          //带盐
                    ChallengerClientRealName = chatRoomJoinerVM.ClientRealName,  //原始
                    ChallengerColor = chatRoomJoinerVM.ClientColor,              //颜色
                    ChallengerMessage = " (落子) ",                              //文本
                    RequesterClientName = matchHelper.ChallengeRequest.ChallengerClientName, //对方名字
                    IsChallengeRequest = false,                                  //应战，非对战请求

                    ChessPoint = pt,
                    ChessColor = ChessPieceColor.None
                }
            };
            await chatClient.SendMessageToClient(matchHelper.ChallengeRequest.ChallengerClientName, string.Empty, exPayload);
        }

        /// <summary>
        /// 开始联机对战
        /// </summary>
        private async Task MultiPlayStartGame(ChessPieceColor color)
        {
            await playground.StopGame(); // 结束单机广场的游戏

            playground = MultiPlayground.Instance; // 切换至联机广场

            await playground.StartGame(horizontalGridCount, verticalGridCount, async (isWinner) =>
            {
                await Task.Run(async () =>
                {
                    if (isWinner)
                    {
                        // 对战结束，发送一条结算息。
                        var exPayload = new AdditionalPayload(chatRoomJoinerVM.ClientColor, chatRoomJoinerVM.ClientAvatarIdx)
                        {
                            ExMessageType = ExMessageType.WinnerAlert,
                            ExObject = new MatchObject()
                            {
                                ChallengerClientName = chatRoomJoinerVM.ClientName,          //带盐
                                ChallengerClientRealName = chatRoomJoinerVM.ClientRealName,  //原始
                                ChallengerColor = chatRoomJoinerVM.ClientColor,              //颜色
                                ChallengerMessage = $" {(isWinner ? "杀死了比赛" : "...")}", //文本
                                ChessColor = ChessPieceColor.Black
                            }
                        };
                        await chatClient.SendMessageToClient(chatRoomJoinerVM.ServerName, string.Empty, exPayload);
                    }

                    // 对战结束，改回单机广场
                    matchHelper.IsInMatchingGame = true;
                    playground = Playground.Instance;
                });
            });
            await playground.SetMultiPlayChessColor(color);

            Debug.WriteLine(color);
        }
    }

    /// <summary>
    /// 对战消息处理
    /// </summary>
    partial class uClient_controller
    {
        /// <summary>
        /// <para>使用ExMessage的消息类型：SystemAlert</para>
        /// <para>使用ExObject的消息类型：GameMatching、GameConfirm、GameInProgress、WinnerAlert</para>
        /// </summary>
        private AdditionalPayload CreateGameMatchingMessage(string msg, ChessPieceColor color = ChessPieceColor.Black, string remotePlayerClientName = "", bool isChallengeRequest = true)
        {
            var exPayload = new AdditionalPayload(chatRoomJoinerVM.ClientColor, chatRoomJoinerVM.ClientAvatarIdx)
            {
                ExMessageType = ExMessageType.GameMatching,
                ExObject = new MatchObject()
                {
                    ChallengerClientName = chatRoomJoinerVM.ClientName,          //带盐
                    ChallengerClientRealName = chatRoomJoinerVM.ClientRealName,  //原始
                    ChallengerColor = chatRoomJoinerVM.ClientColor,              //颜色
                    ChallengerMessage = $"{msg}",                                //文本
                    ChessColor = color,
                    RequesterClientName = remotePlayerClientName, //对方名字
                    IsChallengeRequest = isChallengeRequest,                                  //应战，非对战请求
                }
            };

            return exPayload;
        }

        /// <summary>
        /// <para>使用ExMessage的消息类型：SystemAlert</para>
        /// <para>使用ExObject的消息类型：GameMatching、GameConfirm、GameInProgress、WinnerAlert</para>
        /// </summary>
        private AdditionalPayload CreateGameConfirmMessage(string msg, ChessPieceColor color = ChessPieceColor.Black, bool isAbortGame = false)
        {
            var exPayload = new AdditionalPayload(chatRoomJoinerVM.ClientColor, chatRoomJoinerVM.ClientAvatarIdx)
            {
                ExMessageType = ExMessageType.GameConfirm,
                ExObject = new MatchObject()
                {
                    ChallengerClientName = chatRoomJoinerVM.ClientName,          //带盐
                    ChallengerClientRealName = chatRoomJoinerVM.ClientRealName,  //原始
                    ChallengerColor = chatRoomJoinerVM.ClientColor,              //颜色
                    ChallengerMessage = $"{msg}",                                //文本
                    ChessColor = color,
                    RequesterClientName = matchHelper.ChallengeRequest.ChallengerClientName, //对方名字
                    IsChallengeRequest = false,                                  //应战，非对战请求
                    AbortGame = isAbortGame
                }
            };

            return exPayload;
        }
    }
}
