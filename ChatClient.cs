using SocketIOClient;
using SocketIOClient.Common;
using System.Text.Json;

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
        
        // Conection events
        public event Action? Connected;
        public event Action<string>? ConnectionError;
        public event Action? Disconnected;
        public event Action<string>? NickStatusReceived;
        public event Action<string>? LoginFailed;
        public event Action<string>? RoomJoined;

        //User list events
        public event Action<List<string>>? UserListUpdated;
        
        //Message events
        public event Action<ChatMessage>? MessageReceived;
        public event Action<List<ChatMessage>>? HistoryReceived;


        public async Task ConnectAsync(
            string username,
            string password,
            string room)
        {
            try
            {
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

                // Historial de Mensajes de Chat (recibidos al unirse a la sala)
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

                // Procesador de mensajes recibidos
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
                //Message Ends  


                await _socket.ConnectAsync();
            }
            catch (Exception ex)
            {
                ConnectionError?.Invoke(ex.ToString());
            }
        }

        // Procesador de formato mensajes enviados
        public async Task SendMessageAsync(
            string message,
            bool bold,
            bool italic,
            bool underline,
            string fontFamily)
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
                    fontFamily
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

            // Procesador de mensajes enviados Fin        
      

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
                        //Registered Nickname -> password login first
                        await LoginAsync();
                    }
                    else
                    {
                        // Guest nickname ->no ppassword/auth token needed, join room directly
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

                    var names = data.users
                        .Select(u => u.username)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToList();

                    UserListUpdated?.Invoke(names);

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
                await _socket.DisconnectAsync();
            }
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
    }

    public class UserListResponse
    {
        public List<UserListItem> users { get; set; } = new();
    }

    //Cadena de mensajes Enviados y recibidos del servidor
    public class ChatMessage 
    {
        public string id { get; set; } = "";
        public string username { get; set; } = "";
        public string message { get; set; } = "";
        public ChatFormat format { get; set; } = new();
        public string? vipNickColor { get; set; }
        public long timestamp { get; set; }
    }
    public class RoomHistoryResponse //Historial de mensajes recibidos al unirse a la sala
    {
        public List<ChatMessage> messages { get; set; } = new();
    }

    public class ChatFormat //Formato de mensajes enviados y recibidos del servidor
    {
        public bool bold { get; set; }
        public bool italic { get; set; }
        public bool underline { get; set; }
        public string fontFamily { get; set; } = "Verdana";
    }


}