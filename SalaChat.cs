namespace Nexo.NET
{
    public partial class SalaChat : Form
    {

        //Estilo de mensajes enviados y recibidos del servidor
        private bool _bold = false;
        private bool _italic = false;
        private bool _underline = false;
        // ===============================
        private readonly ChatClient _chatClient;

        public SalaChat(ChatClient chatClient)
        {
            InitializeComponent();

            _chatClient = chatClient;

            _chatClient.RoomJoined += room =>
            {
                BeginInvoke(() =>
                {
                    rtbChat.AppendText(
                        $"* Entraste a la Sala {room} *{Environment.NewLine}"
                    );
                });
            };

            _chatClient.Disconnected += () =>
            {
                BeginInvoke(() =>
                {
                    rtbChat.AppendText(
                        "* Desconectado del servidor *" +
                        Environment.NewLine
                    );
                });
            };
            // Procesar historial (mensajes recibidos del servidor al unirse a la sala)
            _chatClient.HistoryReceived += messages =>
            {
                BeginInvoke(() =>
                {
                    foreach (var msg in messages)
                    {
                        rtbChat.AppendText(
                            $"{msg.username}: {msg.message}{Environment.NewLine}"
                        );
                    }
                });
            };

            //Procesar mensajes recibidos del servidor
            _chatClient.MessageReceived += msg =>
            {
                BeginInvoke(() =>
                {
                    rtbChat.AppendText(
                        $"{msg.username}: {msg.message}{Environment.NewLine}"
                    );
                });
            };
            //End messages received from the server

            _chatClient.UserListUpdated += users =>
            {
                BeginInvoke(() =>
                {
                    lstUsers.Items.Clear();

                    foreach (var user in users)
                    {
                        lstUsers.Items.Add(user);
                    }
                });
            };
        }

        private void SalaChat_Load(object sender, EventArgs e)
        {
        }

        private void btnConexión_Click(object sender, EventArgs e)
        {
            using var conexion = new Conexión(_chatClient);
            conexion.ShowDialog(this);
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            var text = txtMessage.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            var fontFamily = cmbFont.Text;

            if (string.IsNullOrWhiteSpace(fontFamily))
                fontFamily = "Verdana";

            await _chatClient.SendMessageAsync(
                text,
                _bold,
                _italic,
                _underline,
                fontFamily
            );

            txtMessage.Clear();
            txtMessage.Focus();
        }
    }
}