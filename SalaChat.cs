using System.Drawing.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Http;


namespace Nexo.NET
{
    public partial class SalaChat : Form
    {

        // Message formatting state
        private bool _bold = false;
        private bool _italic = false;
        private bool _underline = false;

        private string _messageColorCode = "#000000";
        // ===============================
        private readonly ChatClient _chatClient;

        public SalaChat(ChatClient chatClient)
        {
            InitializeComponent();
            lstUsers.DrawMode = DrawMode.OwnerDrawFixed;
            lstUsers.ItemHeight = 24;
            lstUsers.DrawItem += lstUsers_DrawItem;

            AcceptButton = btnSend;

            Directory.CreateDirectory(_emojiCacheFolder);
            Directory.CreateDirectory(_userListCacheFolder);


            _chatClient = chatClient;

            _chatClient.RoomJoined += room =>
            {
                BeginInvoke(() =>
                {
                    rtbChat.AppendText(
                        $"* Entraste a la Sala {room} *{Environment.NewLine}"
                    );

                    Text = $"Nexo Chat Usuario: {_chatClient.CurrentUsername} Conectado en: {room}";

                    ScrollChatToBottom();

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
                BeginInvoke(async () =>
                {
                    foreach (var msg in messages)
                    {
                        await AppendChatMessageAsync(msg);
                    }
                });
            };

            // Live messages received from server
            _chatClient.MessageReceived += msg =>
            {
                BeginInvoke(async () =>
                {
                    await AppendChatMessageAsync(msg);
                });
            };
            // End live message handling

            _chatClient.UserListUpdated += users =>
            {
                BeginInvoke(async () =>
                {
                    lstUsers.Items.Clear();

                    foreach (var user in users)
                    {
                        await PreloadUserListImagesAsync(user);
                        lstUsers.Items.Add(user);
                    }

                    lstUsers.Invalidate();
                });
            };
        }

        private async Task AppendChatMessageAsync(ChatMessage msg)
        {
            rtbChat.SelectionStart = rtbChat.TextLength;

            if (!string.IsNullOrWhiteSpace(msg.vipNickColor))
            {
                try
                {
                    rtbChat.SelectionColor =
                        ColorTranslator.FromHtml(msg.vipNickColor);
                }
                catch
                {
                    rtbChat.SelectionColor = rtbChat.ForeColor;
                }
            }
            else
            {
                rtbChat.SelectionColor = rtbChat.ForeColor;
            }

            rtbChat.SelectionFont = rtbChat.Font;

            rtbChat.AppendText(msg.username + ": ");

            var style = FontStyle.Regular;

            if (msg.format.bold)
                style |= FontStyle.Bold;

            if (msg.format.italic)
                style |= FontStyle.Italic;

            if (msg.format.underline)
                style |= FontStyle.Underline;

            try
            {
                rtbChat.SelectionFont = new Font(
                    msg.format.fontFamily,
                    rtbChat.Font.Size,
                    style
                );
            }
            catch
            {
                rtbChat.SelectionFont = new Font(
                    rtbChat.Font.FontFamily,
                    rtbChat.Font.Size,
                    style
                );
            }

            await AppendMessageBodyAsync(msg);

            rtbChat.SelectionColor = rtbChat.ForeColor;
            rtbChat.SelectionFont = rtbChat.Font;

            rtbChat.AppendText(Environment.NewLine);

            ScrollChatToBottom();
        }
        private async Task SendCurrentMessageAsync()
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
                fontFamily,
                _messageColorCode
            );

            txtMessage.Clear();
            txtMessage.Focus();
        }


        // Emoji + message-body rendering
        private async Task AppendMessageBodyAsync(ChatMessage msg)
        {
            var decoded = WebUtility.HtmlDecode(msg.message);

            Color defaultColor;

            try
            {
                defaultColor =
                    ColorTranslator.FromHtml(msg.format.color);
            }
            catch
            {
                defaultColor = rtbChat.ForeColor;
            }

            var spanPattern =
                @"<span\s+style\s*=\s*[""']color\s*:\s*(#[0-9a-fA-F]{6})[""']\s*>(.*?)</span>";

            var matches = Regex.Matches(
                decoded,
                spanPattern,
                RegexOptions.IgnoreCase |
                RegexOptions.Singleline
            );

            if (matches.Count == 0)
            {
                await AppendTextAndEmojisAsync(
                    decoded,
                    defaultColor
                );

                return;
            }

            int position = 0;

            foreach (Match match in matches)
            {
                if (match.Index > position)
                {
                    string before = decoded.Substring(
                        position,
                        match.Index - position
                    );

                    await AppendTextAndEmojisAsync(
                        before,
                        defaultColor
                    );
                }

                Color spanColor;

                try
                {
                    spanColor = ColorTranslator.FromHtml(
                        match.Groups[1].Value
                    );
                }
                catch
                {
                    spanColor = defaultColor;
                }

                await AppendTextAndEmojisAsync(
                    match.Groups[2].Value,
                    spanColor
                );

                position = match.Index + match.Length;
            }

            if (position < decoded.Length)
            {
                await AppendTextAndEmojisAsync(
                    decoded.Substring(position),
                    defaultColor
                );
            }
        }

        private async Task AppendTextAndEmojisAsync(
            string text,
            Color color)
        {
            var matches = _emojiRegex.Matches(text);

            if (matches.Count == 0)
            {
                rtbChat.SelectionColor = color;
                rtbChat.AppendText(text);
                return;
            }

            int position = 0;

            foreach (Match match in matches)
            {
                if (match.Index > position)
                {
                    rtbChat.SelectionColor = color;

                    rtbChat.AppendText(
                        text.Substring(
                            position,
                            match.Index - position
                        )
                    );
                }

                string code =
                    match.Groups["code"].Value;

                if (int.TryParse(code, out int number) &&
                    number >= 0 &&
                    number <= 67)
                {
                    try
                    {
                        byte[] imageBytes =
                            await GetEmojiAsync(code);

                        InsertEmoji(imageBytes);

                        rtbChat.SelectionColor = color;
                    }
                    catch
                    {
                        rtbChat.SelectionColor = color;
                        rtbChat.AppendText(match.Value);
                    }
                }
                else
                {
                    rtbChat.SelectionColor = color;
                    rtbChat.AppendText(match.Value);
                }

                position = match.Index + match.Length;
            }

            if (position < text.Length)
            {
                rtbChat.SelectionColor = color;

                rtbChat.AppendText(
                    text.Substring(position)
                );
            }
        }
        // End emoji + message-body rendering
        private void SalaChat_Load(object sender, EventArgs e)
        {
            cmbFont.Items.Clear();

            using var fonts = new InstalledFontCollection();

            foreach (var family in fonts.Families)
            {
                cmbFont.Items.Add(family.Name);
            }

            if (cmbFont.Items.Contains("Verdana"))
                cmbFont.SelectedItem = "Verdana";
            else if (cmbFont.Items.Count > 0)
                cmbFont.SelectedIndex = 0;
        }
        private void ScrollChatToBottom()
        {
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;
            rtbChat.ScrollToCaret();
        }
        private void btnConexión_Click(object sender, EventArgs e)
        {
            using var conexion = new Conexión(_chatClient);
            conexion.ShowDialog(this);
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            await SendCurrentMessageAsync();
        }
        private void btnBold_Click(object sender, EventArgs e)
        {
            _bold = !_bold;
            btnBold.BackColor = _bold
                ? SystemColors.ControlDark
                : SystemColors.Control;
        }

        private void btnItalic_Click(object sender, EventArgs e)
        {
            _italic = !_italic;
            btnItalic.BackColor = _italic
                ? SystemColors.ControlDark
                : SystemColors.Control;
        }

        private void btnUnderline_Click(object sender, EventArgs e)
        {
            _underline = !_underline;
            btnUnderline.BackColor = _underline
                ? SystemColors.ControlDark
                : SystemColors.Control;
        }
        private void btnColors_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();

            AddColorItem(menu, "Negro", ColorTranslator.FromHtml("#000000"), "#000000");
            AddColorItem(menu, "Azul", ColorTranslator.FromHtml("#0000C0"), "#0000C0");
            AddColorItem(menu, "Rojo", ColorTranslator.FromHtml("#FF0000"), "#FF0000");
            AddColorItem(menu, "Verde", ColorTranslator.FromHtml("#1F9912"), "#1F9912");
            AddColorItem(menu, "Naranja", ColorTranslator.FromHtml("#FF9900"), "#FF9900");
            AddColorItem(menu, "Rosa", ColorTranslator.FromHtml("#FF41FF"), "#FF41FF");
            AddColorItem(menu, "Morado", ColorTranslator.FromHtml("#A50694"), "#A50694");

            menu.Show(btnColors, new Point(0, btnColors.Height));
        }

        private void AddColorItem(
            ContextMenuStrip menu,
            string name,
            Color color,
            string code)
        {
            var item = new ToolStripMenuItem();

            item.Text = name;

            var swatch = new Bitmap(16, 16);

            using (var g = Graphics.FromImage(swatch))
            {
                using var brush = new SolidBrush(color);
                g.FillRectangle(brush, 0, 0, 16, 16);
                g.DrawRectangle(Pens.Gray, 0, 0, 15, 15);
            }

            item.Image = swatch;

            item.Click += (sender, e) =>
            {
                _messageColorCode = code;
                btnColors.ForeColor = color;
                txtMessage.ForeColor = color;
            };

            menu.Items.Add(item);
        }


        // Persistent emoji cache
        private async Task<byte[]> GetEmojiAsync(string code)
        {
            var localPath =
                Path.Combine(_emojiCacheFolder, $"{code}.png");

            if (File.Exists(localPath))
            {
                try
                {
                    return await File.ReadAllBytesAsync(localPath);
                }
                catch
                {
                    try
                    {
                        File.Delete(localPath);
                    }
                    catch
                    {
                    }
                }
            }

            var url =
                $"https://elchatlatino.com/imagenes/iconoschat/{code}.png";

            var bytes =
                await _emojiHttp.GetByteArrayAsync(url);

            await File.WriteAllBytesAsync(localPath, bytes);

            return bytes;
        }

        private void InsertEmoji(byte[] pngBytes)
        {
            using var stream = new MemoryStream(pngBytes);
            using var image = Image.FromStream(stream);

            const int targetHeight = 38;

            int targetWidth = (int)Math.Round(
                image.Width *
                (targetHeight / (double)image.Height)
            );

            // RTF uses twips. At 96 DPI, ~15 twips = 1 pixel.
            int widthTwips = targetWidth * 15;
            int heightTwips = targetHeight * 15;

            string hex = Convert.ToHexString(pngBytes);

            string rtf =
                @"{\rtf1\ansi" +
                @"{\pict\pngblip" +
                $@"\picw{image.Width}" +
                $@"\pich{image.Height}" +
                $@"\picwgoal{widthTwips}" +
                $@"\pichgoal{heightTwips} " +
                hex +
                "}}";

            rtbChat.SelectedRtf = rtf;
        }


        private static readonly HttpClient _emojiHttp = new();

        private readonly string _emojiCacheFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                ),
                "Nexo.NET",
                "emoji-cache"
            );

        private static readonly Regex _emojiRegex =
            new Regex(
                @"(?<!/)/{1,2}(?<code>\d{2})(?!\d)",
                RegexOptions.Compiled
            );
        // End persistent emoji cache

        //Userlist custom drawing

        private readonly string _userListCacheFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                ),
                "Nexo.NET",
                "user-list-cache"
            );

        private readonly Dictionary<string, Image> _userListImageCache = new();

        private string GetUserIconUrl(UserListItem user)
        {
            var role = (user.role ?? "")
                .Trim()
                .ToLowerInvariant();

            return role switch
            {
                "superadmin" =>
                    "https://elchatlatino.com/imagenes/staff/superadmin.png?v=ad0088f22c",

                "admin" =>
                    "https://elchatlatino.com/imagenes/staff/admin.png?v=ad0088f22c",

                "operador" =>
                    "https://elchatlatino.com/imagenes/staff/operator.png?v=ad0088f22c",

                "moderador" =>
                    "https://elchatlatino.com/imagenes/staff/mod.png?v=ad0088f22c",

                "modtemporal" =>
                    "https://elchatlatino.com/imagenes/staff/modtemp.png?v=ad0088f22c",

                _ =>
                    "https://elchatlatino.com/imagenes/iconos/EmojiUser.png?v=ad0088f22c"
            };
        }

        private string GetStatusIconUrl(string? estado)
        {
            var status = (estado ?? "nodisponible")
                .Trim()
                .ToLowerInvariant();

            return status switch
            {
                "disponible" or "available" =>
                    "https://elchatlatino.com/imagenes/estados/disponible.png?v=ef342a93a0",

                "ocupado" or "busy" =>
                    "https://elchatlatino.com/imagenes/estados/ocupado.png?v=ef342a93a0",

                "ausente" or "away" =>
                    "https://elchatlatino.com/imagenes/estados/ausente.png?v=ef342a93a0",

                "nodisponible" or "invisible" =>
                    "https://elchatlatino.com/imagenes/estados/nodisponible.png?v=ef342a93a0",

                _ =>
                    "https://elchatlatino.com/imagenes/estados/nodisponible.png?v=ef342a93a0"
            };
        }

        private async Task PreloadUserListImagesAsync(UserListItem user)
        {
            var userIconUrl = GetUserIconUrl(user);
            var statusUrl = GetStatusIconUrl(user.estado);

            var roleKey =
                string.IsNullOrWhiteSpace(user.role)
                    ? "default"
                    : user.role.Trim().ToLowerInvariant();

            var statusKey =
                string.IsNullOrWhiteSpace(user.estado)
                    ? "nodisponible"
                    : user.estado.Trim().ToLowerInvariant();

            await GetUserListImageAsync(
                $"user_{roleKey}",
                userIconUrl
            );

            await GetUserListImageAsync(
                $"status_{statusKey}",
                statusUrl
            );
        }

        private async Task<Image?> GetUserListImageAsync(
            string key,
            string url)
        {
            if (_userListImageCache.TryGetValue(
                key,
                out var cachedImage))
            {
                return cachedImage;
            }

            var localPath =
                Path.Combine(
                    _userListCacheFolder,
                    $"{key}.png"
                );

            try
            {
                byte[] bytes;

                if (File.Exists(localPath))
                {
                    bytes =
                        await File.ReadAllBytesAsync(localPath);
                }
                else
                {
                    bytes =
                        await _emojiHttp.GetByteArrayAsync(url);

                    await File.WriteAllBytesAsync(
                        localPath,
                        bytes
                    );
                }

                using var stream =
                    new MemoryStream(bytes);

                using var original =
                    Image.FromStream(stream);

                var copy =
                    new Bitmap(original);

                _userListImageCache[key] = copy;

                return copy;
            }
            catch
            {
                try
                {
                    if (File.Exists(localPath))
                        File.Delete(localPath);
                }
                catch
                {
                }

                return null;
            }
        }
        private void lstUsers_DrawItem(
            object? sender,
            DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            if (lstUsers.Items[e.Index] is not UserListItem user)
                return;

            e.DrawBackground();

            var roleKey =
                string.IsNullOrWhiteSpace(user.role)
                    ? "default"
                    : user.role.Trim().ToLowerInvariant();

            var statusKey =
                string.IsNullOrWhiteSpace(user.estado)
                    ? "nodisponible"
                    : user.estado.Trim().ToLowerInvariant();

            if (_userListImageCache.TryGetValue(
                $"user_{roleKey}",
                out var userIcon))
            {
                e.Graphics.DrawImage(
                    userIcon,
                    new Rectangle(
                        e.Bounds.Left + 3,
                        e.Bounds.Top + 4,
                        16,
                        16
                    )
                );
            }

            Color userNameColor = lstUsers.ForeColor;
            FontStyle userNameStyle = FontStyle.Regular;

            string? paintValue =
                !string.IsNullOrWhiteSpace(user.staffPaintColor)
                    ? user.staffPaintColor
                    : !string.IsNullOrWhiteSpace(user.nickColor)
                        ? user.nickColor
                        : !string.IsNullOrWhiteSpace(user.vipNickColor)
                            ? user.vipNickColor
                            : null;

            if (!string.IsNullOrWhiteSpace(paintValue))
            {
                string[] parts = paintValue.Split('|', 2);

                string colorCode = parts[0].Trim();

                try
                {
                    userNameColor =
                        ColorTranslator.FromHtml(colorCode);
                }
                catch
                {
                    userNameColor = lstUsers.ForeColor;
                }

                if (parts.Length > 1)
                {
                    string flags =
                        parts[1].Trim().ToLowerInvariant();

                    if (flags.Contains('b'))
                        userNameStyle |= FontStyle.Bold;

                    if (flags.Contains('i'))
                        userNameStyle |= FontStyle.Italic;

                    if (flags.Contains('u'))
                        userNameStyle |= FontStyle.Underline;
                }
            }

            using var userNameFont =
                new Font(
                    lstUsers.Font,
                    userNameStyle
                );

            TextRenderer.DrawText(
                e.Graphics,
                user.username,
                userNameFont,
                new Point(
                    e.Bounds.Left + 24,
                    e.Bounds.Top + 4
                ),
                userNameColor
            );

            if (_userListImageCache.TryGetValue(
                $"status_{statusKey}",
                out var statusIcon))
            {
                e.Graphics.DrawImage(
                    statusIcon,
                    new Rectangle(
                        e.Bounds.Right - 21,
                        e.Bounds.Top + 3,
                        18,
                        18
                    )
                );
            }

            e.DrawFocusRectangle();
        }

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
