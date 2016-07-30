using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using BFClient_CSharp.Util;
using Newtonsoft.Json.Linq;

namespace BFClient_CSharp.View
{
    public partial class FileOpenForm : Form
    {
        private readonly Dictionary<string, ArrayList> _fileversions = new Dictionary<string, ArrayList>();
        private MainForm _mainForm;

        public FileOpenForm(MainForm main)
        {
            this._mainForm = main;
            InitializeComponent();
        }

        private void FileOpenForm_Shown(object sender, EventArgs e)
        {
            try
            {
                var fileList = SessionMgr.FileList();
                var jsonFiles = JArray.Parse(fileList);
                listFile.BeginUpdate();
                foreach (var jsonFile in jsonFiles)
                {
                    var filename = (string) jsonFile["filename"];
                    var versions = new ArrayList();
                    var jsonVersions = (JArray) jsonFile["versions"];
                    foreach (var version in jsonVersions)
                        versions.Add((string) version);
                    _fileversions.Add(filename, versions);
                    // Update ListView
                    listFile.Items.Add(filename, filename + ".bf", 0);
                    listFile.Items[filename].SubItems.Add((string) versions[versions.Count - 1]);
                }
                listFile.EndUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            var filename = listFile.SelectedItems[0].Name;
            var version = listVersion.SelectedItems[0].Name;
            var code = SessionMgr.FileContent(filename, version);
            _mainForm.newToolStripMenuItem_Click(sender, e);
            _mainForm.FileName = filename;
            _mainForm.FileVersion = version;
            _mainForm.OriginalCode = code;
            _mainForm.textCode.Text = code;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            listVersion.Items.Clear();
            if (listFile.SelectedIndices.Count <= 0) return;
            var versions = _fileversions[listFile.SelectedItems[0].Name];
            listVersion.BeginUpdate();
            foreach (string version in versions)
                listVersion.Items.Add(version, version, 0);
            listVersion.EndUpdate();
            listVersion.Items[0].Selected = true;
        }

        private void listVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonOpen.Enabled = listVersion.SelectedIndices.Count > 0;
        }

        private void listFile_DoubleClick(object sender, EventArgs e)
        {
            listVersion.Items[0].Selected = true;
            buttonOpen.PerformClick();
        }

        private void listVersion_DoubleClick(object sender, EventArgs e)
        {
            buttonOpen.PerformClick();
        }
    }
}
