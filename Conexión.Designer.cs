namespace Nexo.NET
{
    partial class Conexión
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            txtNickname = new TextBox();
            txtPassword = new TextBox();
            cmbArea = new ComboBox();
            cmbRoom = new ComboBox();
            btnConnect = new Button();
            btnDisconnect = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(32, 12);
            label1.Name = "label1";
            label1.Size = new Size(53, 15);
            label1.TabIndex = 0;
            label1.Text = "Usuario :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 41);
            label2.Name = "label2";
            label2.Size = new Size(73, 15);
            label2.TabIndex = 1;
            label2.Text = "Contraseña :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(48, 70);
            label3.Name = "label3";
            label3.Size = new Size(37, 15);
            label3.TabIndex = 2;
            label3.Text = "Area :";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(48, 99);
            label4.Name = "label4";
            label4.Size = new Size(34, 15);
            label4.TabIndex = 3;
            label4.Text = "Sala :";
            // 
            // txtNickname
            // 
            txtNickname.Location = new Point(91, 9);
            txtNickname.Name = "txtNickname";
            txtNickname.Size = new Size(126, 23);
            txtNickname.TabIndex = 4;
            txtNickname.Text = "NexoVS.NET ";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(91, 38);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(126, 23);
            txtPassword.TabIndex = 5;
            // 
            // cmbArea
            // 
            cmbArea.FormattingEnabled = true;
            cmbArea.Location = new Point(91, 67);
            cmbArea.Name = "cmbArea";
            cmbArea.Size = new Size(126, 23);
            cmbArea.TabIndex = 6;
            cmbArea.Text = "Amistad";
            // 
            // cmbRoom
            // 
            cmbRoom.FormattingEnabled = true;
            cmbRoom.Location = new Point(91, 96);
            cmbRoom.Name = "cmbRoom";
            cmbRoom.Size = new Size(126, 23);
            cmbRoom.TabIndex = 7;
            cmbRoom.Text = "Amigos";
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(41, 134);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(85, 23);
            btnConnect.TabIndex = 8;
            btnConnect.Text = "Conectar";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Location = new Point(132, 134);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new Size(85, 23);
            btnDisconnect.TabIndex = 9;
            btnDisconnect.Text = "Desconectar";
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += btnDisconnect_Click;
            // 
            // Conexión
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(241, 173);
            Controls.Add(btnDisconnect);
            Controls.Add(btnConnect);
            Controls.Add(cmbRoom);
            Controls.Add(cmbArea);
            Controls.Add(txtPassword);
            Controls.Add(txtNickname);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "Conexión";
            Text = "connectFrm";
            Load += Conexión_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private TextBox txtNickname;
        private TextBox txtPassword;
        private ComboBox cmbArea;
        private ComboBox cmbRoom;
        private Button btnConnect;
        private Button btnDisconnect;
    }
}