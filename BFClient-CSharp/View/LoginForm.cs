﻿using System;
using System.Windows.Forms;
using BFClient_CSharp.Properties;
using BFClient_CSharp.Util;

namespace BFClient_CSharp.View
{
    internal partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            textHost.Text = Settings.Default.host;
            textUsername.Focus();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            var username = textUsername.Text;
            var password = textPassword.Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) return;
            try
            {
                SessionMgr.Host = textHost.Text;
                SessionMgr.Login(username, password);
                if (checkRemember.Checked)
                    SessionMgr.SaveLoginInfo(username, password);
                else
                    SessionMgr.SaveLoginInfo("", "");
                new MainForm().Show();
                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Login failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}