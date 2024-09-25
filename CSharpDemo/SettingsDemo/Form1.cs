using System;
using System.Diagnostics;
using System.Windows.Forms;
using SettingsDemo.Properties;

namespace SettingsDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            valueLabel.Text = Settings.Default.test;
        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            Settings.Default.Reload();
            valueLabel.Text = Settings.Default.test;
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            Settings.Default.test = inputTextBox.Text;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            Settings.Default.Save();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            Settings.Default.Reset();
            valueLabel.Text = Settings.Default.test;
        }

        private void OpenConfigButton_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\SettingsDemo");
        }

    }
}
