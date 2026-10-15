namespace Nexo.NET
{
    partial class PmForm
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
            rtbPmChat = new RichTextBox();
            txtPmMessage = new TextBox();
            btnPmSend = new Button();
            btnPmBuzz = new Button();
            SuspendLayout();
            // 
            // rtbPmChat
            // 
            rtbPmChat.Location = new Point(5, 6);
            rtbPmChat.Name = "rtbPmChat";
            rtbPmChat.Size = new Size(365, 198);
            rtbPmChat.TabIndex = 0;
            rtbPmChat.Text = "";
            // 
            // txtPmMessage
            // 
            txtPmMessage.Location = new Point(7, 208);
            txtPmMessage.Name = "txtPmMessage";
            txtPmMessage.Size = new Size(273, 23);
            txtPmMessage.TabIndex = 1;
            // 
            // btnPmSend
            // 
            btnPmSend.Location = new Point(322, 208);
            btnPmSend.Name = "btnPmSend";
            btnPmSend.Size = new Size(50, 23);
            btnPmSend.TabIndex = 2;
            btnPmSend.Text = "Enviar";
            btnPmSend.UseVisualStyleBackColor = true;
            // 
            // btnPmBuzz
            // 
            btnPmBuzz.Location = new Point(282, 208);
            btnPmBuzz.Name = "btnPmBuzz";
            btnPmBuzz.Size = new Size(39, 23);
            btnPmBuzz.TabIndex = 3;
            btnPmBuzz.Text = "Buzz";
            btnPmBuzz.UseVisualStyleBackColor = true;
            // 
            // PmForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(377, 236);
            Controls.Add(btnPmBuzz);
            Controls.Add(btnPmSend);
            Controls.Add(txtPmMessage);
            Controls.Add(rtbPmChat);
            Name = "PmForm";
            Text = "PmForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox rtbPmChat;
        private TextBox txtPmMessage;
        private Button btnPmSend;
        private Button btnPmBuzz;
    }
}