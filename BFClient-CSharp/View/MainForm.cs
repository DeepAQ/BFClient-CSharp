using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BFClient_CSharp.Util;
using Microsoft.VisualBasic;

namespace BFClient_CSharp.View
{
    public partial class MainForm : Form
    {
        private readonly ArrayList _changeList = new ArrayList();
        private int _changeIndex;
        private Thread _saveChangeThread;

        private string _fileName = "";
        private string _fileVersion = "";
        private string _originalCode = "";
        private bool _modified;

        public MainForm()
        {
            InitializeComponent();
            UpdateTitle();
            userToolStripMenuItem.Text = @"Logged in as : " + SessionMgr.Username;
            _changeList.Add("");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CheckSaved();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void textCode_TextChanged(object sender, EventArgs e)
        {
            if (_saveChangeThread != null && _saveChangeThread.IsAlive)
                _saveChangeThread.Interrupt();
            _saveChangeThread = new Thread(SaveChange);
            _saveChangeThread.Start();
            _modified = !textCode.Text.Equals(_originalCode);
            UpdateTitle();
        }

        private void SaveChange()
        {
            try
            {
                Thread.Sleep(500);
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
            if (_changeIndex >= 0 && textCode.Text.Equals(_changeList[_changeIndex]))
                return;
            while (_changeList.Count > _changeIndex + 1)
                _changeList.RemoveAt(_changeIndex + 1);
            _changeList.Add(textCode.Text);
            if (_changeList.Count > 100)
                _changeList.RemoveAt(0);
            _changeIndex = _changeList.Count - 1;
        }

        private void UpdateTitle()
        {
            var modFlag = _modified ? "* " : "";
            var title = string.IsNullOrEmpty(_fileName) ? $"{modFlag}Untitled.bf" : $"{modFlag}{_fileName}.bf ({_fileVersion})";
            title += @" - BrainFuck IDE";
            this.Text = title;
        }

        private bool CheckSaved()
        {
            if (!_modified) return true;
            var result = MessageBox.Show(@"File not saved, save it?", @"Confirmation", MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Information);
            switch (result)
            {
                case DialogResult.Yes:
                    saveToolStripMenuItem.PerformClick();
                    return false;
                case DialogResult.No:
                    return true;
                default:
                    return false;
            }
        }

        // File
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckSaved()) return;
            textCode.Clear();
            _fileName = "";
            _fileVersion = "";
            _modified = false;
            UpdateTitle();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_modified) return;
            if (string.IsNullOrEmpty(_fileName))
                saveAsToolStripMenuItem.PerformClick();
            else
                try
                {
                    var newVersion = SessionMgr.SaveFile(textCode.Text, _fileName);
                    _fileVersion = newVersion;
                    _originalCode = textCode.Text;
                    _modified = false;
                    UpdateTitle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var input = Interaction.InputBox("Filename :", "Save as");
            if (string.IsNullOrEmpty(input)) return;
            _fileName = input;
            _modified = true;
            saveToolStripMenuItem.PerformClick();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = @"BrainFuck source code|.bf";
            dialog.ShowDialog();
            if (string.IsNullOrEmpty(dialog.FileName)) return;
            File.WriteAllText(dialog.FileName, textCode.Text, Encoding.UTF8);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Edit
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_changeIndex <= 0) return;
            _changeIndex--;
            textCode.Text = _changeList[_changeIndex] as string;
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_changeIndex >= _changeList.Count - 1) return;
            _changeIndex++;
            textCode.Text = _changeList[_changeIndex] as string;
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textCode.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textCode.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textCode.Paste();
        }

        // Run
        private void runToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                var result = SessionMgr.Execute(textCode.Text, textInput.Text);
                textOutput.Text = result[0] + "\r\n====================\r\nExecution success, used " + result[1] + "ms";
            }
            catch (Exception ex)
            {
                textOutput.Text = "Execution error:\r\n" + ex.Message;
            }
        }

        // Help
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        // User
        private void refreshSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SessionMgr.RefreshSession();
                MessageBox.Show(@"Refresh success", @"Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckSaved()) return;
            SessionMgr.Logout();
            Hide();
            new LoginForm().Show();
        }
    }
}
