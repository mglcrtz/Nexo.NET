namespace Nexo.NET
{
    public partial class PmForm : Form
    {
        private readonly ChatClient _chatClient;
        private readonly string _otherUser;

        public string OtherUser => _otherUser;

        public PmForm(
            ChatClient chatClient,
            string otherUser)
        {
            InitializeComponent();

            btnPmSend.Click -= btnPmSend_Click;
            btnPmSend.Click += btnPmSend_Click;

            _chatClient = chatClient;
            _otherUser = otherUser;

            Text = $"Al Privado con: {_otherUser}";
            AcceptButton = btnPmSend;
        }

        private async void btnPmSend_Click(
            object sender,
            EventArgs e)
        {
            var text = txtPmMessage.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            btnPmSend.Enabled = false;

            try
            {
                bool sent =
                    await _chatClient.SendPrivateMessageAsync(
                        _otherUser,
                        text
                    );

                if (!sent)
                {
                    MessageBox.Show(
                        "No se pudo enviar el mensaje privado.",
                        "Mensaje privado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                AppendMessage(
                    _chatClient.CurrentUsername,
                    text
                );

                txtPmMessage.Clear();
                txtPmMessage.Focus();
            }
            finally
            {
                if (!IsDisposed)
                    btnPmSend.Enabled = true;
            }
        }

        public void ReceiveMessage(
            PrivateMessage message)
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
                return;

            void AppendIncoming()
            {
                AppendMessage(
                    string.IsNullOrWhiteSpace(message.from)
                        ? _otherUser
                        : message.from,
                    message.message
                );
            }

            if (InvokeRequired)
                BeginInvoke(AppendIncoming);
            else
                AppendIncoming();
        }

        private void AppendMessage(
            string username,
            string message)
        {
            rtbPmChat.SelectionStart =
                rtbPmChat.TextLength;

            rtbPmChat.SelectionLength = 0;
            rtbPmChat.SelectionColor =
                rtbPmChat.ForeColor;
            rtbPmChat.SelectionFont =
                rtbPmChat.Font;

            rtbPmChat.AppendText(
                username +
                ": " +
                message +
                Environment.NewLine
            );

            rtbPmChat.SelectionStart =
                rtbPmChat.TextLength;
            rtbPmChat.ScrollToCaret();
        }

        private void btnPmBuzz_Click(
            object sender,
            EventArgs e)
        {
            // Buzz gets wired after basic PM send/receive.
        }
    }
}