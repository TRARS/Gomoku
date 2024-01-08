using Gomoku.Core.Helper.Base;
using Gomoku.SocketUtils.Role;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MenuType = Gomoku.UI.Control.CustomControlEx.MenuButtonEx.ButtonType;
using StoneType = Gomoku.UI.Control.CustomControlEx.StoneButtonEx.ButtonType;

namespace Gomoku.UI.Control.UserControlEx.ClientEx
{
    /// <summary>
    /// <para>继承 ObservableCollection</para>
    /// <para>Add() 带容量限制</para>
    /// <para>Enqueue() 带出队特效</para>
    /// </summary>
    public class LimitedSizeObservableCollection<T> : ObservableCollection<T>
    {
        private object lockObject = new object();
        private int maxItemCount = 20;
        private int itemsToRemove = 12;


        /// <summary>
        /// 常规Add
        /// </summary>
        public new void Add(T item)
        {
            lock (lockObject)
            {
                if (Count >= maxItemCount)
                {
                    for (int i = 0; i < itemsToRemove; i++)
                    {
                        base.RemoveAt(0);
                    }
                }
                base.Add(item);
            }
        }

        public LimitedSizeObservableCollection(int _maxItemCount, int _countToRemove)
        {
            maxItemCount = Math.Clamp(_maxItemCount, 2, int.MaxValue);
            itemsToRemove = Math.Clamp(_countToRemove, 1, _maxItemCount);
        }
    }

    /// <summary>
    /// https://smdn.jp/programming/netfx/tips/set_volume_of_soundplayer/
    /// </summary>
    class WaveStream : Stream
    {
        public override bool CanSeek
        {
            // シークはサポートしない
            get { return false; }
        }

        public override bool CanRead
        {
            get { return !IsClosed; }
        }

        public override bool CanWrite
        {
            // 書き込みはサポートしない
            get { return false; }
        }

        private bool IsClosed
        {
            get { return reader == null; }
        }

        public override long Position
        {
            get { CheckDisposed(); throw new NotSupportedException(); }
            set { CheckDisposed(); throw new NotSupportedException(); }
        }

        public override long Length
        {
            get { CheckDisposed(); throw new NotSupportedException(); }
        }

        public int Volume
        {
            get { CheckDisposed(); return volume; }
            set
            {
                CheckDisposed();

                if (value < 0 || MaxVolume < value)
                    throw new ArgumentOutOfRangeException("Volume",
                                                          value,
                                                          string.Format("0から{0}の範囲の値を指定してください", MaxVolume));

                volume = value;
            }
        }

        public WaveStream(Stream baseStream)
        {
            if (baseStream == null)
                throw new ArgumentNullException("baseStream");
            if (!baseStream.CanRead)
                throw new ArgumentException("読み込み可能なストリームを指定してください", "baseStream");

            this.reader = new BinaryReader(baseStream);

            ReadHeader();
        }

        public override void Close()
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
        }

        // dataチャンクまでのヘッダブロックの内容をバッファに読み込んでおく
        // WAVEFORMAT等のヘッダ内容のチェックは省略
        private void ReadHeader()
        {
            using (var headerStream = new MemoryStream())
            {
                var writer = new BinaryWriter(headerStream);

                // RIFFヘッダ
                var riffHeader = reader.ReadBytes(12);

                writer.Write(riffHeader);

                // dataチャンクまでの内容をwriterに書き写す
                for (; ; )
                {
                    var chunkHeader = reader.ReadBytes(8);

                    writer.Write(chunkHeader);

                    var fourcc = BitConverter.ToInt32(chunkHeader, 0);
                    var size = BitConverter.ToInt32(chunkHeader, 4);

                    if (fourcc == 0x61746164) // 'data'
                        break;

                    writer.Write(reader.ReadBytes(size));
                }

                writer.Close();

                header = headerStream.ToArray();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", offset, "0以上の値を指定してください");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", count, "0以上の値を指定してください");
            if (buffer.Length - count < offset)
                throw new ArgumentException("配列の範囲を超えてアクセスしようとしました", "offset");

            if (header == null)
            {
                // dataチャンクの読み込み
                // WAVEサンプルを読み込み、音量を適用して返す
                // ストリームは16ビット(1サンプル2バイト)と仮定

                // countバイト以下となるよう読み込むサンプル数を決定する
                var samplesToRead = count / 2;
                var bytesToRead = samplesToRead * 2;
                var len = reader.Read(buffer, offset, bytesToRead);

                if (len == 0)
                    return 0; // 終端まで読み込んだ

                // 読み込んだサンプル1つずつにボリュームを適用する
                for (var sample = 0; sample < samplesToRead; sample++)
                {
                    short s = (short)(buffer[offset] | (buffer[offset + 1] << 8));

                    s = (short)(((double)s * volume) / MaxVolume);

                    buffer[offset] = (byte)(s & 0xff);
                    buffer[offset + 1] = (byte)((s >> 8) & 0xff);

                    offset += 2;
                }

                return len;
            }
            else
            {
                // ヘッダブロックの読み込み
                // バッファに読み込んでおいた内容をそのままコピーする
                var bytesToRead = Math.Min(header.Length - headerOffset, count);

                Buffer.BlockCopy(header, headerOffset, buffer, offset, bytesToRead);

                headerOffset += bytesToRead;

                if (headerOffset == header.Length)
                    // ヘッダブロックを全て読み込んだ
                    // (不要になったヘッダのバッファを解放し、以降はdataチャンクの読み込みに移る)
                    header = null;

                return bytesToRead;
            }
        }

        public override void SetLength(long @value)
        {
            CheckDisposed();

            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();

            throw new NotSupportedException();
        }

        public override void Flush()
        {
            CheckDisposed();

            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            throw new NotSupportedException();
        }

        private void CheckDisposed()
        {
            if (IsClosed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        private BinaryReader reader;
        private byte[] header;
        private int headerOffset = 0;
        private int volume = MaxVolume;
        private const int MaxVolume = 100;
    }
}

namespace Gomoku.UI.Control.UserControlEx.ClientEx
{
    /// <summary>
    /// 按钮VM
    /// </summary>
    public partial class MenuButtonViewModel : NotificationObject
    {
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value) { return; }
                _text = value;
                NotifyPropertyChanged();
            }
        }

        private string _color;
        public string Color
        {
            get => _color;
            set
            {
                if (_color != value) { return; }
                _color = value;
                NotifyPropertyChanged();
            }
        }

        private double _width;
        public double Width
        {
            get => _width;
            set
            {
                if (_width != value) { return; }
                _width = value;
                NotifyPropertyChanged();
            }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set
            {
                if (_height != value) { return; }
                _height = value;
                NotifyPropertyChanged();
            }
        }

        private AsyncRelayCommand _command;
        public AsyncRelayCommand Command
        {
            get => _command;
            set
            {
                _command = value;
                NotifyPropertyChanged();
            }
        }

        private MenuType _type;
        public MenuType Type
        {
            get => _type;
            set
            {
                if (_type != value) { return; }
                _type = value;
                NotifyPropertyChanged();
            }
        }

        public MenuButtonViewModel(string text, string color, double width, double height, AsyncRelayCommand command, MenuType type = MenuType.Noamal)
        {
            _text = text;
            _color = color;
            _width = width;
            _height = height;
            _command = command;

            _type = type;
        }
    }

    /// <summary>
    /// 按钮组VM
    /// </summary>
    public partial class MenuButtonGroupViewModel : NotificationObject
    {
        public ObservableCollection<MenuButtonViewModel> MenuVMList { get; init; }

        public MenuButtonGroupViewModel()
        {
            MenuVMList = new();
        }
    }

    /// <summary>
    /// 棋子VM
    /// </summary>
    public partial class StoneViewModel : NotificationObject
    {
        private StoneType _type;
        public StoneType Type
        {
            get => _type;
            set
            {
                if (_type == value) { return; }
                _type = value;
                NotifyPropertyChanged();
            }
        }

        private double _diameter;
        public double Diameter
        {
            get => _diameter;
            set
            {
                if (_diameter == value) { return; }
                _diameter = value;
                NotifyPropertyChanged();
            }
        }

        private Thickness _margin;
        public Thickness Margin
        {
            get => _margin;
            set
            {
                if (_margin == value) { return; }
                _margin = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _visibility;
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (_visibility == value) { return; }
                _visibility = value;
                NotifyPropertyChanged();
            }
        }

        private bool _enablehighLight;
        public bool EnableHighLight
        {
            get => _enablehighLight;
            set
            {
                if (_enablehighLight == value) { return; }
                _enablehighLight = value;
                NotifyPropertyChanged();
            }
        }

        private AsyncRelayCommand _SoundCommand;
        public AsyncRelayCommand SoundCommand
        {
            get => _SoundCommand;
            set
            {
                if (_SoundCommand == value) { return; }
                _SoundCommand = value;
                NotifyPropertyChanged();
            }
        }

        private SoundPlayer? soundPlayer;
        private static string? assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        public StoneViewModel(StoneType type, double diameter, Thickness margin, bool enablehighLight = false, Visibility visibility = Visibility.Visible)
        {
            _type = type;
            _diameter = diameter;
            _margin = margin;
            _enablehighLight = enablehighLight;
            _visibility = visibility;
            _SoundCommand = new AsyncRelayCommand(async _ =>
            {
                var path = type switch
                {
                    StoneType.BlackStone => $"pack://application:,,,/{assemblyName};component/Control/AudioEx/drop.wav",
                    StoneType.WhiteStone => $"pack://application:,,,/{assemblyName};component/Control/AudioEx/drop.wav",
                    _ => throw new NotSupportedException()
                };

                //
                //soundPlayer = soundPlayer ?? new();
                //var sri = Application.GetResourceStream(new Uri(path));
                //soundPlayer.Stream = sri.Stream;
                //soundPlayer.Play();

                await Task.Run(() =>
                {
                    using (var stream = new WaveStream(Application.GetResourceStream(new Uri(path)).Stream))
                    {
                        // 0 から 100の範囲で音量を指定する
                        stream.Volume = 20;

                        // WaveStreamから音声を読み込みSoundPlayer.PlaySyncメソッドで再生
                        using (var player = new SoundPlayer(stream))
                        {
                            player.Play();
                        }
                    }
                });
            });
        }

        public void SetHighLight(bool flag)
        {
            this.EnableHighLight = flag;
        }
    }

    /// <summary>
    /// 棋盘VM
    /// </summary>
    public partial class ChessBoardViewModel : NotificationObject
    {
        private double _gridSize;
        public double GridSize
        {
            get => _gridSize;
            set
            {
                if (_gridSize == value) { return; }
                _gridSize = value;
                NotifyPropertyChanged();
            }
        }

        private double _horizontalGridCount;
        public double HorizontalGridCount
        {
            get => _horizontalGridCount;
            set
            {
                if (_horizontalGridCount == value) { return; }
                _horizontalGridCount = value;
                NotifyPropertyChanged();
            }
        }

        private double _verticalGridCount;
        public double VerticalGridCount
        {
            get => _verticalGridCount;
            set
            {
                if (_verticalGridCount == value) { return; }
                _verticalGridCount = value;
                NotifyPropertyChanged();
            }
        }

        private Point _chessBoardSize;
        public Point ChessBoardSize
        {
            get => _chessBoardSize;
            set
            {
                if (_chessBoardSize == value) { return; }
                _chessBoardSize = value;
                NotifyPropertyChanged();
            }
        }

        private Brush _backgroundBrush;
        public Brush BackgroundBrush
        {
            get => _backgroundBrush;
            set
            {
                if (_backgroundBrush == value) { return; }
                _backgroundBrush = value;
                NotifyPropertyChanged();
            }
        }


        public StoneViewModel TempStoneVM { get; set; }
        public ObservableCollection<StoneViewModel> StoneVMList { get; init; }

        public AsyncRelayCommand MouseClickCommand { get; set; }
        public AsyncRelayCommand MouseMoveCommand { get; set; }

        public ChessBoardViewModel(double gridSize, double horizontalGridCount, double verticalGridCount)
        {
            _gridSize = gridSize;
            _horizontalGridCount = horizontalGridCount;
            _verticalGridCount = verticalGridCount;

            _backgroundBrush = CreateGridTile(gridSize, horizontalGridCount, verticalGridCount);
            _chessBoardSize = new(_gridSize * _horizontalGridCount, _gridSize * _verticalGridCount);

            StoneVMList = new();
        }

        private DrawingBrush CreateGridTile(double gSize, double hCount, double vCount)
        {
            var hSize = gSize * hCount;
            var vSize = gSize * vCount;

            Rect bounds = new Rect(0, 0, hSize, vSize); //屏幕矩形
            double sideLength = gSize;                      //方形单位尺寸
            double thickness = 1;                           //笔头粗细度
            bool isSlash = false;                           //是否斜线
            SolidColorBrush penColor = new((Color)ColorConverter.ConvertFromString("#3F4B57"));

            var gridColor = penColor;
            var gridThickness = isSlash ? thickness : thickness; //+0.1
            var tileRect = new Rect(new Size(sideLength, sideLength));

            var gridTile = new DrawingBrush
            {
                Stretch = Stretch.None,
                TileMode = TileMode.Tile,
                Viewport = tileRect,
                ViewportUnits = BrushMappingMode.Absolute,
                Drawing = new GeometryDrawing
                {
                    Pen = new Pen(gridColor, gridThickness),
                    Geometry = new GeometryGroup
                    {
                        Children = isSlash ?
                        new GeometryCollection
                        {//斜线
                            new LineGeometry(tileRect.TopLeft, tileRect.BottomRight),
                            new LineGeometry(tileRect.BottomLeft, tileRect.TopRight)
                        } :
                        new GeometryCollection
                        { //横竖线
                            new LineGeometry(tileRect.TopLeft, tileRect.TopRight),
                            new LineGeometry(tileRect.TopRight, tileRect.BottomRight),
                            new LineGeometry(tileRect.BottomRight, tileRect.BottomLeft),
                            new LineGeometry(tileRect.BottomLeft, tileRect.TopLeft)
                        }
                    }
                }
            };

            var offsetGrid = new DrawingBrush
            {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                Transform = new TranslateTransform(bounds.Left, bounds.Top),
                Drawing = new GeometryDrawing
                {
                    Geometry = new RectangleGeometry(new Rect(bounds.Size)),
                    Brush = gridTile
                }
            };

            return offsetGrid;
        }
    }
}

namespace Gomoku.UI.Control.UserControlEx.ClientEx
{
    /// <summary>
    /// 聊天气泡VM
    /// </summary>
    public partial class ChatMessageViewModel : NotificationObject
    {
        private bool _Dequeue;
        public bool Dequeue
        {
            get { return _Dequeue; }
            set
            {
                _Dequeue = value;
                NotifyPropertyChanged();
            }
        }

        public bool _isOwnMessage;
        public bool IsOwnMessage
        {
            get { return _isOwnMessage; }
            set
            {
                _isOwnMessage = value;
                NotifyPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyPropertyChanged();
            }
        }

        private SolidColorBrush _borderColor;
        public SolidColorBrush BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                NotifyPropertyChanged();
            }
        }

        private string _imageSource;//UI上显示的头像
        public string ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                NotifyPropertyChanged();
            }
        }

        private string _senderName;
        public string SenderName
        {
            get { return _senderName; }
            set
            {
                _senderName = value;
                NotifyPropertyChanged();
            }
        }

        private Action<string> _callBack;
        public Action<string> CallBack
        {
            get { return _callBack; }
            set
            {
                _callBack = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 聊天气泡VM
        /// </summary>
        public ChatMessageViewModel()
        {
            _message = string.Empty;
            _senderName = string.Empty;
            _imageSource = string.Empty;
            _borderColor = new SolidColorBrush(Colors.Brown);

            _callBack = (para) =>
            {
                Debug.WriteLine($"-> {para}");
            };
        }
    }

    /// <summary>
    /// 系统消息气泡VM
    /// </summary>
    public partial class SystemMessageViewModel : NotificationObject
    {
        private bool _Dequeue;
        public bool Dequeue
        {
            get { return _Dequeue; }
            set
            {
                _Dequeue = value;
                NotifyPropertyChanged();
            }
        }

        private AdditionalPayload _exPayload;
        public AdditionalPayload ExPayload
        {
            get { return _exPayload; }
            set
            {
                _exPayload = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 系统消息气泡VM
        /// </summary>
        public SystemMessageViewModel(AdditionalPayload exPayload)
        {
            _exPayload = exPayload;
        }

        public SystemMessageViewModel(string msg)
        {
            _exPayload = new("#FFFF0000", 0)
            {
                ExMessageType = ExMessageType.SystemAlert,
                ExMessage = msg
            };

        }
    }

    /// <summary>
    /// 对战消息气泡VM
    /// </summary>
    public partial class GameMessageViewModel : NotificationObject
    {
        private bool _Dequeue;
        public bool Dequeue
        {
            get { return _Dequeue; }
            set
            {
                _Dequeue = value;
                NotifyPropertyChanged();
            }
        }

        private AdditionalPayload _exPayload;
        public AdditionalPayload ExPayload
        {
            get { return _exPayload; }
            set
            {
                _exPayload = value;
                NotifyPropertyChanged();
            }
        }

        private AsyncRelayCommand _acceptCommand;
        public AsyncRelayCommand AcceptCommand
        {
            get => _acceptCommand;
            set
            {
                _acceptCommand = value;
                NotifyPropertyChanged();
            }
        }

        private AsyncRelayCommand _rejectCommand;
        public AsyncRelayCommand RejectCommand
        {
            get => _rejectCommand;
            set
            {
                _rejectCommand = value;
                NotifyPropertyChanged();
            }
        }

        public GameMessageViewModel(AdditionalPayload exPayload)
        {
            _exPayload = exPayload;
        }
    }

    /// <summary>
    /// 结算消息气泡VM
    /// </summary>
    public partial class WinnerMessagesViewModel : NotificationObject
    {
        private bool _Dequeue;
        public bool Dequeue
        {
            get { return _Dequeue; }
            set
            {
                _Dequeue = value;
                NotifyPropertyChanged();
            }
        }

        private AdditionalPayload _exPayload;
        public AdditionalPayload ExPayload
        {
            get { return _exPayload; }
            set
            {
                _exPayload = value;
                NotifyPropertyChanged();
            }
        }

        public WinnerMessagesViewModel(AdditionalPayload exPayload)
        {
            _exPayload = exPayload;
        }
    }

    /// <summary>
    /// 聊天记录VM
    /// </summary>
    public partial class ChatHistoryViewModel : NotificationObject
    {
        public LimitedSizeObservableCollection<ChatMessageViewModel> ChatMessages { get; init; }
        public LimitedSizeObservableCollection<SystemMessageViewModel> SystemMessages { get; init; }
        public LimitedSizeObservableCollection<GameMessageViewModel> GameMessages { get; init; }
        public LimitedSizeObservableCollection<WinnerMessagesViewModel> WinnerMessages { get; init; }

        /// <summary>
        /// 聊天记录VM
        /// </summary>
        public ChatHistoryViewModel()
        {
            ChatMessages = new(20, 11);
            SystemMessages = new(8, 5);
            GameMessages = new(8, 5);
            WinnerMessages = new(6, 4);
        }
    }

    /// <summary>
    /// 聊天输入VM
    /// </summary>
    public partial class ChatInputViewmodel : NotificationObject
    {
        private string _userMessage;
        public string UserMessage
        {
            get => _userMessage;
            set
            {
                _userMessage = value;
                NotifyPropertyChanged();
            }
        }

        private string _clientColor;
        public string ClientColor
        {
            get => _clientColor;
            init
            {
                _clientColor = value;
                NotifyPropertyChanged();
            }
        }

        private AsyncRelayCommand _sendcommand;
        public AsyncRelayCommand SendCommand
        {
            get => _sendcommand;
            set
            {
                _sendcommand = value;
                NotifyPropertyChanged();
            }
        }

        private AsyncRelayCommand _loginCommand;
        public AsyncRelayCommand LoginCommand
        {
            get => _loginCommand;
            set
            {
                _loginCommand = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 聊天输入VM
        /// </summary>
        public ChatInputViewmodel(AsyncRelayCommand sendcommand, AsyncRelayCommand loginCommand, string clientColor)
        {
            _userMessage = string.Empty;
            _clientColor = clientColor;
            _sendcommand = sendcommand;
            _loginCommand = loginCommand;
        }
    }

    /// <summary>
    /// 聊天服务端VM
    /// </summary>
    public partial class ChatServerViewModel : NotificationObject
    {
        private string _serverAddress;
        public string ServerAddress
        {
            get => _serverAddress;
            set
            {
                _serverAddress = value;
                NotifyPropertyChanged();
            }
        }

        private int _serverPort;
        public int ServerPort
        {
            get => _serverPort;
            set
            {
                _serverPort = value;
                NotifyPropertyChanged();
            }
        }

        private AsyncRelayCommand _createServerCommand;
        public AsyncRelayCommand CreateServerCommand
        {
            get => _createServerCommand;
            set
            {
                _createServerCommand = value;
                NotifyPropertyChanged();
            }
        }

        private bool _serverLock;
        public bool ServerLock
        {
            get => _serverLock;
            set
            {
                _serverLock = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 聊天服务端VM
        /// </summary>
        public ChatServerViewModel(string serverAddress, int serverPort, AsyncRelayCommand createCommand)
        {
            _serverAddress = serverAddress;
            _serverPort = serverPort;
            _createServerCommand = createCommand;
            _serverLock = false;
        }
    }

    /// <summary>
    /// 聊天客户端VM
    /// </summary>
    public partial class ChatClientViewModel : NotificationObject
    {
        private string _serverAddress;
        public string ServerAddress
        {
            get => _serverAddress;
            set
            {
                _serverAddress = value;
                NotifyPropertyChanged();
            }
        }

        private string _serverPort;
        public string ServerPort
        {
            get => _serverPort;
            set
            {
                _serverPort = value;
                NotifyPropertyChanged();
            }
        }

        private AsyncRelayCommand _confirmCommand;
        public AsyncRelayCommand ConfirmCommand
        {
            get => _confirmCommand;
            set
            {
                _confirmCommand = value;
                NotifyPropertyChanged();
            }
        }

        private AsyncRelayCommand _cancelCommand;
        public AsyncRelayCommand CancelCommand
        {
            get => _cancelCommand;
            set
            {
                _cancelCommand = value;
                NotifyPropertyChanged();
            }
        }

        private Visibility _confirmWindowVisibility;
        public Visibility ConfirmWindowVisibility
        {
            get => _confirmWindowVisibility;
            set
            {
                _confirmWindowVisibility = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 聊天客户端VM
        /// </summary>
        public ChatClientViewModel(string loginServerAddress, string loginServerPort, AsyncRelayCommand loginConfirmCommand, AsyncRelayCommand loginCancelCommand)
        {
            _serverAddress = loginServerAddress;
            _serverPort = loginServerPort;
            _confirmCommand = loginConfirmCommand;
            _cancelCommand = loginCancelCommand;
            _confirmWindowVisibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// 本机代表VM
    /// </summary>
    public partial class ChatRoomJoinerViewModel : NotificationObject
    {
        /// <summary>
        /// 设置或获取本机创建的服务端的名称
        /// </summary>
        public string ServerName
        {
            get => _serverName;
            set
            {
                _serverName = value;
                NotifyPropertyChanged();
            }
        }
        private string _serverName;

        /// <summary>
        /// 设置或获取本机作为客户端时的名称（读取时带盐以防重复）
        /// </summary>
        public string ClientName
        {
            get => _clientName + _feature;
            set
            {
                _clientName = value;
                NotifyPropertyChanged();
            }
        }
        private string _clientName;

        /// <summary>
        /// 本机roll到的颜色
        /// </summary>
        public string ClientColor
        {
            get => _clientColor;
            init
            {
                _clientColor = value;
                NotifyPropertyChanged();
            }
        }
        private string _clientColor;

        /// <summary>
        /// 本机roll到的头像下标
        /// </summary>
        public int ClientAvatarIdx
        {
            get => _clientAvatarIdx;
            init
            {
                _clientAvatarIdx = value;
                NotifyPropertyChanged();
            }
        }
        private int _clientAvatarIdx;

        /// <summary>
        /// 头像路径
        /// </summary>
        private string _clientAvatarPath;
        public string ClientAvatarPath
        {
            get { return _clientAvatarPath; }
            set
            {
                _clientAvatarPath = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 本机作为客户端时名称（原始）
        /// </summary>
        public string ClientRealName => _clientName;

        /// <summary>
        /// 本机代表VM
        /// </summary>
        public ChatRoomJoinerViewModel(string serverName, string clientName, string clientColor, int clientAvatarIdx)
        {
            _feature = $"@{GenerateRandomString(8)}";
            _serverName = serverName;
            _clientName = clientName;
            _clientColor = clientColor;
            _clientAvatarIdx = clientAvatarIdx;
        }

        /// <summary>
        /// 获取首个本机地址
        /// </summary>
        /// <returns></returns>
        public string TryGetLocalAddress()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            var ipv4 = localIPs.Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(x => x);

            return ipv4.Any() ? $"{ipv4.ToList()[0]}" : $"127.0.0.1";
        }

        /// <summary>
        /// 脱盐
        /// </summary>
        public string TryGetClientRealName(string name)
        {
            if (name == _serverName) { return _serverName; } // 服务端名字没盐

            if (name.Length - _feature.Length > 0)
            {
                return name.Substring(0, name.Length - _feature.Length);
            }
            return name;
        }


        private string _feature { get; init; }
        private Random random = new Random();
        /// <summary>
        /// 加盐
        /// </summary>
        private string GenerateRandomString(int length)
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
