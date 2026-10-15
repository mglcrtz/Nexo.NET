namespace Nexo.NET
{
    partial class SalaChat
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            rtbChat = new RichTextBox();
            lstUsers = new ListBox();
            txtMessage = new TextBox();
            btnSend = new Button();
            cmbFont = new ComboBox();
            btnBold = new Button();
            btnItalic = new Button();
            btnUnderline = new Button();
            btnColors = new Button();
            btnConexión = new Button();
            SuspendLayout();
            // 
            // rtbChat
            // 
            rtbChat.Location = new Point(12, 32);
            rtbChat.Name = "rtbChat";
            rtbChat.Size = new Size(610, 516);
            rtbChat.TabIndex = 0;
            rtbChat.Text = "";
            // 
            // lstUsers
            // 
            lstUsers.FormattingEnabled = true;
            lstUsers.Location = new Point(628, 32);
            lstUsers.Name = "lstUsers";
            lstUsers.Size = new Size(196, 514);
            lstUsers.TabIndex = 1;
            lstUsers.SelectedIndexChanged += lstUsers_SelectedIndexChanged;
            // 
            // txtMessage
            // 
            txtMessage.Location = new Point(193, 553);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(348, 23);
            txtMessage.TabIndex = 2;
            txtMessage.TextChanged += txtMessage_TextChanged;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(547, 554);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(75, 23);
            btnSend.TabIndex = 3;
            btnSend.Text = "Enviar";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // cmbFont
            // 
            cmbFont.FormattingEnabled = true;
            cmbFont.Location = new Point(12, 554);
            cmbFont.Name = "cmbFont";
            cmbFont.Size = new Size(66, 23);
            cmbFont.TabIndex = 4;
            // 
            // btnBold
            // 
            btnBold.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnBold.Location = new Point(82, 554);
            btnBold.Name = "btnBold";
            btnBold.Size = new Size(27, 23);
            btnBold.TabIndex = 5;
            btnBold.Text = "N";
            btnBold.UseVisualStyleBackColor = true;
            btnBold.Click += btnBold_Click;
            // 
            // btnItalic
            // 
            btnItalic.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point, 0);
            btnItalic.Location = new Point(108, 554);
            btnItalic.Name = "btnItalic";
            btnItalic.Size = new Size(27, 23);
            btnItalic.TabIndex = 6;
            btnItalic.Text = "I";
            btnItalic.UseVisualStyleBackColor = true;
            btnItalic.Click += btnItalic_Click;
            // 
            // btnUnderline
            // 
            btnUnderline.Font = new Font("Segoe UI", 9F, FontStyle.Underline, GraphicsUnit.Point, 0);
            btnUnderline.Location = new Point(134, 554);
            btnUnderline.Name = "btnUnderline";
            btnUnderline.Size = new Size(27, 23);
            btnUnderline.TabIndex = 7;
            btnUnderline.Text = "S";
            btnUnderline.UseVisualStyleBackColor = true;
            btnUnderline.Click += btnUnderline_Click;
            // 
            // btnColors
            // 
            btnColors.Font = new Font("Segoe UI", 9F, FontStyle.Underline, GraphicsUnit.Point, 0);
            btnColors.Location = new Point(160, 554);
            btnColors.Name = "btnColors";
            btnColors.Size = new Size(27, 23);
            btnColors.TabIndex = 9;
            btnColors.Text = "A";
            btnColors.UseVisualStyleBackColor = true;
            btnColors.Click += btnColors_Click;
            // 
            // btnConexión
            // 
            btnConexión.Location = new Point(749, 553);
            btnConexión.Name = "btnConexión";
            btnConexión.Size = new Size(75, 23);
            btnConexión.TabIndex = 10;
            btnConexión.Text = "Coneccion";
            btnConexión.UseVisualStyleBackColor = true;
            btnConexión.Click += btnConexión_Click;
            // 
            // SalaChat
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(836, 583);
            Controls.Add(btnConexión);
            Controls.Add(btnColors);
            Controls.Add(btnUnderline);
            Controls.Add(btnItalic);
            Controls.Add(btnBold);
            Controls.Add(cmbFont);
            Controls.Add(btnSend);
            Controls.Add(txtMessage);
            Controls.Add(lstUsers);
            Controls.Add(rtbChat);
            Name = "SalaChat";
            Text = "Nexo Chat.NET";
            Load += SalaChat_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox rtbChat;
        private ListBox lstUsers;
        private TextBox txtMessage;
        private Button btnSend;
        private ComboBox cmbFont;
        private Button btnBold;
        private Button btnItalic;
        private Button btnUnderline;
        private Button btnColors;
        private Button btnConexión;
    }
}
