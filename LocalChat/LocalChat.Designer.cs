namespace LocalChat
{
    partial class frmLocalChat
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
            rtbConsole = new RichTextBox();
            label1 = new Label();
            txtInput = new TextBox();
            btnSend = new Button();
            btnExit = new Button();
            bwrReadMessages = new System.ComponentModel.BackgroundWorker();
            SuspendLayout();
            // 
            // rtbConsole
            // 
            rtbConsole.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbConsole.BorderStyle = BorderStyle.FixedSingle;
            rtbConsole.Location = new Point(12, 12);
            rtbConsole.Name = "rtbConsole";
            rtbConsole.ReadOnly = true;
            rtbConsole.Size = new Size(567, 213);
            rtbConsole.TabIndex = 0;
            rtbConsole.Text = "";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(12, 243);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 1;
            label1.Text = "Input:";
            // 
            // txtInput
            // 
            txtInput.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtInput.Location = new Point(56, 239);
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(360, 23);
            txtInput.TabIndex = 2;
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSend.Location = new Point(422, 239);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(75, 23);
            btnSend.TabIndex = 3;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnExit
            // 
            btnExit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExit.Location = new Point(503, 239);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(75, 23);
            btnExit.TabIndex = 4;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // bwrReadMessages
            // 
            bwrReadMessages.DoWork += bwrReadMessages_DoWork;
            // 
            // frmLocalChat
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(592, 280);
            Controls.Add(btnExit);
            Controls.Add(btnSend);
            Controls.Add(txtInput);
            Controls.Add(label1);
            Controls.Add(rtbConsole);
            MinimumSize = new Size(608, 319);
            Name = "frmLocalChat";
            Text = "Local Chat";
            Load += frmLocalChat_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox rtbConsole;
        private Label label1;
        private TextBox txtInput;
        private Button btnSend;
        private Button btnExit;
        private System.ComponentModel.BackgroundWorker bwrReadMessages;
    }
}
