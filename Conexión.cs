namespace Nexo.NET
{
    public partial class Conexión : Form
    {
        private readonly ChatClient _chatClient;


        // Keeps WinForms Designer working
        public Conexión() : this(new ChatClient())
        {
        }

        // Runtime constructor - uses the shared ChatClient
        public Conexión(ChatClient chatClient)
        {
            InitializeComponent();

            _chatClient = chatClient;

            _chatClient.ConnectionError += ChatClient_ConnectionError;
            _chatClient.Disconnected += ChatClient_Disconnected;
            _chatClient.NickStatusReceived += ChatClient_NickStatusReceived;
            _chatClient.LoginFailed += ChatClient_LoginFailed;
            _chatClient.RoomJoined += ChatClient_RoomJoined;

            FormClosed += Conexion_FormClosed;
        }

        private void ChatClient_ConnectionError(string error)
        {
            SafeUI(() =>
            {
                MessageBox.Show(
                    error,
                    "Error de conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            });
        }

        private void ChatClient_Disconnected()
        {
            SafeUI(() =>
            {
                Text = "Conexión - Desconectado";
            });
        }

        private void ChatClient_NickStatusReceived(string status)
        {
            SafeUI(() =>
            {
                Text = "Conexión - " + status;
            });
        }

        private void ChatClient_LoginFailed(string error)
        {
            SafeUI(() =>
            {
                MessageBox.Show(
                    error,
                    "Login rechazado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            });
        }

        private void ChatClient_RoomJoined(string room)
        {
            SafeUI(() =>
            {
                DialogResult = DialogResult.OK;
                Close();
            });
        }

        private void SafeUI(Action action)
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
                return;

            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }

        private void Conexion_FormClosed(object? sender, FormClosedEventArgs e)
        {
            // Unsubscribe this form from the shared ChatClient
            _chatClient.ConnectionError -= ChatClient_ConnectionError;
            _chatClient.Disconnected -= ChatClient_Disconnected;
            _chatClient.NickStatusReceived -= ChatClient_NickStatusReceived;
            _chatClient.LoginFailed -= ChatClient_LoginFailed;
            _chatClient.RoomJoined -= ChatClient_RoomJoined;
        }

        private void Conexión_Load(object sender, EventArgs e)
        {
            // Keep your real Area / Room population here.
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;

            try
            {
                await _chatClient.ConnectAsync(
                    txtNickname.Text.Trim(),
                    txtPassword.Text,
                    cmbRoom.Text.Trim()
                );
            }
            finally
            {
                if (!IsDisposed)
                    btnConnect.Enabled = true;
            }
        }

        private async void btnDisconnect_Click(object sender, EventArgs e)
        {
            await _chatClient.DisconnectAsync();
        }
    }
}