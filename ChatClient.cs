using SocketIOClient;
using SocketIOClient.Common;

namespace Nexo.NET
{
    public class ChatClient
    {
        private SocketIO? _socket;

        private string _username = "";
        private string _password = "";
        private string _room = "";
        private string _authToken = "";
        private string _clientId = "";

        public bool IsConnected =>
            _socket != null && _socket.Connected;

        // Current connection state exposed to the UI
        public string CurrentUsername => _username;
        public string CurrentRoom => _room;

        // Connection events
        public event Action? Connected;
        public event Action<string>? ConnectionError;
        public event Action? Disconnected;
        public event Action<string>? NickStatusReceived;
        public event Action<string>? LoginFailed;
        public event Action<string>? RoomJoined;

        //Private message events
        public event Action<PrivateMessage>? PrivateMessageReceived;

        // User list events
        public event Action<List<UserListItem>>? UserListUpdated;

        // Message events
        public event Action<ChatMessage>? MessageReceived;
        public event Action<List<ChatMessage>>? HistoryReceived;

        public async Task ConnectAsync(
            string username,
            string password,
            string room)
        {
            try

            {

                if (_socket != null)
                {
                    try
                    {
                        if (_socket.Connected)
                            await _socket.DisconnectAsync();

                        _socket.Dispose();
                    }
                    catch
                    {
                    }

                    _socket = null;
                }

                _authToken = "";
                _clientId = "";

                _username = username;
                _password = password;
                _room = room.ToLowerInvariant();

                _clientId = Guid.NewGuid()
                    .ToString("N")
                    .ToLowerInvariant();

                var options = new SocketIOOptions
                {
                    Transport = TransportProtocol.WebSocket,
                    Reconnection = false,
                    ExtraHeaders = new Dictionary<string, string>
                    {
                        { "Origin", "https://elchatlatino.com" },
                        { "User-Agent", "Mozilla/5.0" }
                    }
                };

                _socket = new SocketIO(
                    new Uri("https://elchatlatino.com"),
                    options
                );

                RegisterEvents();

                _socket.OnConnected += async (sender, e) =>
                {
                    Connected?.Invoke();

                    await CheckNickAsync();
                };

                _socket.OnDisconnected += (sender, reason) =>
                {
                    Disconnected?.Invoke();
                };

                _socket.OnError += (sender, error) =>
                {
                    ConnectionError?.Invoke(error);
                };

                // Room history received after joining
                _socket.On("room_history", response =>
                {
                    try
                    {
                        var data = response.GetValue<RoomHistoryResponse>(0);

                        if (data?.messages == null)
                            return Task.CompletedTask;

                        HistoryReceived?.Invoke(data.messages);

                        return Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {
                        ConnectionError?.Invoke(
                            "Error procesando room_history: " + ex.Message
                        );

                        return Task.CompletedTask;
                    }
                });

                // Live messages received from server
                _socket.On("message", response =>
                {
                    try
                    {
                        var data = response.GetValue<ChatMessage>(0);

                        if (data == null)
                            return Task.CompletedTask;

                        MessageReceived?.Invoke(data);

                        return Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {
                        ConnectionError?.Invoke(
                            "Error procesando message: " + ex.Message
                        );

                        return Task.CompletedTask;
                    }
                });
                // End live message handling  

                await _socket.ConnectAsync();
            }
            catch (Exception ex)
            {
                ConnectionError?.Invoke(ex.ToString());
            }
        }

        // Send public chat message
        public async Task SendMessageAsync(
            string message,
            bool bold,
            bool italic,
            bool underline,
            string fontFamily,
            string messageColor)
        {
            if (_socket == null || !_socket.Connected)
                return;

            var payload = new
            {
                id = Guid.NewGuid().ToString("N"),
                username = _username,
                message = message,
                format = new
                {
                    bold,
                    italic,
                    underline,
                    fontFamily,
                    color = messageColor
                },

                vipNickColor = (string?)null,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await _socket.EmitAsync(
                "message",
                new object[]
                {
                    payload
                }
            );
        }

        // End send public chat message        

        // Send private message
        public async Task<bool> SendPrivateMessageAsync(
            string recipient,
            string message)
        {
            if (_socket == null || !_socket.Connected)
                return false;

            recipient = recipient.Trim();
            message = message.Trim();

            if (string.IsNullOrWhiteSpace(recipient) ||
                string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            if (string.Equals(
                recipient,
                _username,
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var payload = new
            {
                room = _room,
                from = _username,
                to = recipient,
                username = _username,
                message = message,
                format = new
                {
                    bold = false,
                    italic = false,
                    underline = false,
                    fontFamily = "Arial",
                    color = "#000000"
                },
                vipNickColor = (string?)null,
                _acceptedLink = false
            };

            try
            {
                await _socket.EmitAsync(
                    "private_message",
                    new object[]
                    {
                        payload
                    }
                );

                return true;
            }
            catch (Exception ex)
            {
                ConnectionError?.Invoke(
                    "Error enviando private_message: " +
                    ex.Message
                );

                return false;
            }
        }

        // End send private message

        private void RegisterEvents()
        {
            if (_socket == null)
                return;

            _socket.On("nick_status", async response =>
            {
                try
                {
                    var data = response.GetValue<NickStatusResponse>(0);

                    if (data == null)
                        return;

                    NickStatusReceived?.Invoke(
                        $"{data.username} registered={data.registered}"
                    );

                    if (data.registered)
                    {
                        // Registered nickname -> password login first
                        await LoginAsync();
                    }
                    else
                    {
                        // Guest nickname -> no password/auth token needed; join room directly
                        _authToken = "";
                        await JoinRoomAsync();
                    }
                }
                catch (Exception ex)
                {
                    ConnectionError?.Invoke(
                        "Error procesando nick_status: " + ex.Message
                    );
                }
            });

            _socket.On("login_result", async response =>
            {
                try
                {
                    var data = response.GetValue<LoginResultResponse>(0);

                    if (data == null)
                        return;

                    if (!data.success)
                    {
                        LoginFailed?.Invoke(
                            string.IsNullOrWhiteSpace(data.message)
                                ? "Login rechazado."
                                : data.message
                        );

                        return;
                    }

                    _authToken = data.authToken ?? "";

                    await JoinRoomAsync();
                }
                catch (Exception ex)
                {
                    ConnectionError?.Invoke(
                        "Error procesando login_result: " + ex.Message
                    );
                }
            });

            _socket.On("user_list", response =>
            {
                try
                {
                    var data = response.GetValue<UserListResponse>(0);

                    if (data?.users == null)
                        return Task.CompletedTask;

                    var users = data.users
                        .Where(u => !string.IsNullOrWhiteSpace(u.username))
                        .ToList();

                    UserListUpdated?.Invoke(users);

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    ConnectionError?.Invoke(
                        "Error procesando user_list: " + ex.Message
                    );

                    return Task.CompletedTask;
                }
            });

            _socket.On("join_room_ok", response =>
            {
                try
                {
                    var data = response.GetValue<JoinRoomResponse>(0);

                    if (data == null)
                        return Task.CompletedTask;

                    RoomJoined?.Invoke(data.room);

                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    ConnectionError?.Invoke(
                        "Error procesando join_room_ok: " + ex.Message
                    );

                    return Task.CompletedTask;
                }
            });

            //Private message handling
            _socket.On("private_message", response =>
            {
                try
                {
                    var data =
                        response.GetValue<PrivateMessage>(0);

                    if (data == null)
                        return Task.CompletedTask;

                    PrivateMessageReceived?.Invoke(data);
                }
                catch (Exception ex)
                {
                    ConnectionError?.Invoke(
                        "Error procesando private_message: " +
                        ex.Message
                    );
                }

                return Task.CompletedTask;
            });
            //Ends private message handling

        }

        private async Task CheckNickAsync()
        {
            if (_socket == null)
                return;

            await _socket.EmitAsync(
                "check_nick",
                new object[]
                {
                    new

                    {
                        username = _username,
                        website = ""
                    }
                }
            );
        }

        private async Task LoginAsync()
        {
            if (_socket == null)
                return;

            await _socket.EmitAsync(
                "login_nick",
                new object[]
                {
                    new
                    {
                        username = _username,
                        password = _password,
                        website = ""
                    }
                }
            );
        }

        private async Task JoinRoomAsync()
        {
            if (_socket == null)
                return;

            await _socket.EmitAsync(
                "join_room",
                new object[]
                {
                    new
                    {
                        room = _room,
                        username = _username,
                        clientId = _clientId,
                        authToken = _authToken
                    }
                }
            );
        }

        public async Task DisconnectAsync()
        {
            if (_socket != null)
            {
                try
                {
                    if (_socket.Connected)
                        await _socket.DisconnectAsync();
                }
                finally
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }

            _authToken = "";
            _clientId = "";
        }
    }

    public class NickStatusResponse
    {
        public string username { get; set; } = "";
        public bool registered { get; set; }
    }

    public class LoginResultResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public string authToken { get; set; } = "";
        public bool mustChangePassword { get; set; }
        public string resetBy { get; set; } = "";
    }

    public class JoinRoomResponse
    {
        public string room { get; set; } = "";
    }

    public class UserListItem
    {
        public string username { get; set; } = "";

        public bool isAdmin { get; set; }

        public string? estado { get; set; }

        public string? role { get; set; }

        public string? nickIcon { get; set; }

        public string? nickColor { get; set; }

        public int? vipLevel { get; set; }

        public string? vipNickColor { get; set; }

        public string? vipIcon { get; set; }

        public string? staffPaintColor { get; set; }

        public string? staffIcon { get; set; }

        public bool isGuardian { get; set; }

        public bool guardianOnHold { get; set; }

        public bool isFrozen { get; set; }

        public bool isMuted { get; set; }

        public bool hidden { get; set; }

        public object? onlineSince { get; set; }

        public override string ToString()
        {
            return username;
        }
    }

    public class UserListResponse
    {
        public List<UserListItem> users { get; set; } = new();
    }

    // Chat message models
    public class ChatMessage
    {
        public string id { get; set; } = "";
        public string username { get; set; } = "";
        public string message { get; set; } = "";
        public ChatFormat format { get; set; } = new();
        public string? vipNickColor { get; set; }
        public long timestamp { get; set; }
    }
    public class RoomHistoryResponse // Room history received when joining
    {
        public List<ChatMessage> messages { get; set; } = new();
    }

    public class ChatFormat // Message formatting sent/received by server
    {
        public bool bold { get; set; }
        public bool italic { get; set; }
        public bool underline { get; set; }
        public string fontFamily { get; set; } = "Verdana";
        public string color { get; set; } = "#000000";
    }

    // Private message models
    public class PrivateMessage
    {
        public string room { get; set; } = "";
        public string from { get; set; } = "";
        public string to { get; set; } = "";
        public string username { get; set; } = "";
        public string message { get; set; } = "";

        public ChatFormat format { get; set; } = new();

        public string? vipNickColor { get; set; }

        public bool _acceptedLink { get; set; }
    }

}
    