using System;
using System.Collections;
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
        private bool _modified = false;

        public MainForm()
        {
            InitializeComponent();
            UpdateTitle();
            userToolStripMenuItem.Text = @"Logged in as : " + SessionMgr.Username;
            _changeList.Add("");
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

        // File
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

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

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
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
    }
}
