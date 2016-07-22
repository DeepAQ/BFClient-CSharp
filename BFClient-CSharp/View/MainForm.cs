using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;

namespace BFClient_CSharp.View
{
    public partial class MainForm : Form
    {
        private readonly ArrayList _changeList = new ArrayList();
        private int _changeIndex = 0;
        private Thread _saveChangeThread;

        public MainForm()
        {
            InitializeComponent();
            _changeList.Add("");
        }

        private void textCode_TextChanged(object sender, EventArgs e)
        {
            if (_saveChangeThread != null && _saveChangeThread.IsAlive)
                _saveChangeThread.Interrupt();
            _saveChangeThread = new Thread(SaveChange);
            _saveChangeThread.Start();
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

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
