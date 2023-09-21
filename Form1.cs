using System;
using System.IO;
using System.Diagnostics;
using System.DirectoryServices;

namespace iiConfig
{
    public partial class frmMain : Form
    {
        public string strDevice = "null";
        public frmMain()
        {
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            strDevice = lbDevices.Items[lbDevices.SelectedIndex].ToString().Split(new char[] { '\t' })[0].ToString().Split(new char[] { ':' })[0].Split(new char[] { '\n' })[1];
            label1.Text = "Selected Device: " + strDevice;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = "devices";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            int lstDevices = 0;
            string output = process.StandardOutput.ReadToEnd();
            lbDevices.Items.Clear();
            foreach (string line in output.Split(new char[] { '\r', }))
            {
                if (lstDevices > 0) lbDevices.Items.Add(line);
                lstDevices++;
            }

        }

        private void btnSelectCFG_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);
            if (File.Exists(openFileDialog1.FileName))
            {
                lblCFGFile.Text = "File opened: " + openFileDialog1.FileName;
            }
        }

        private void btnPushCFG_Click(object sender, EventArgs e)
        {
            if ((strDevice == "null") || (strDevice == ""))
            {
                MessageBox.Show("Select a device first.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(openFileDialog1.FileName))
            {
                MessageBox.Show("Select a CFG file first.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Process rootProcess = new Process();
            rootProcess.StartInfo.FileName = "adb";
            rootProcess.StartInfo.Arguments = "root";
            rootProcess.StartInfo.RedirectStandardOutput = true;
            rootProcess.StartInfo.RedirectStandardError = true;
            rootProcess.StartInfo.UseShellExecute = false;
            rootProcess.StartInfo.CreateNoWindow = true;

            rootProcess.Start();
            string rootOutput = rootProcess.StandardOutput.ReadToEnd();
            int exitCode = rootProcess.ExitCode;
            if (exitCode != 0) { MessageBox.Show("Root failed.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            string[] cfgNames = openFileDialog1.FileName.Split('\\');
            string romName = cfgNames[cfgNames.GetLength(0) - 1].Split('.')[0];

            string strPushCmd = $"-s {strDevice} push \"{openFileDialog1.FileName}\" /sdcard/Android/data/org.emulator.arcade/files/cfg/";
            Process pushProcess = new Process();
            pushProcess.StartInfo.FileName = "adb";
            pushProcess.StartInfo.Arguments = strPushCmd;
            pushProcess.StartInfo.RedirectStandardOutput = true;
            pushProcess.StartInfo.RedirectStandardError = true;
            pushProcess.StartInfo.UseShellExecute = false;
            pushProcess.StartInfo.CreateNoWindow = true;

            pushProcess.Start();

            string pushOutput = pushProcess.StandardOutput.ReadToEnd();
            exitCode = pushProcess.ExitCode;

            if (exitCode == 0)
            {
                MessageBox.Show($"{pushOutput}", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Error code {exitCode}: {pushOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!cbLightGun.Checked) return;

            string lgPushCmd = $"-s {strDevice} push lightgun.zip /sdcard/Android/data/org.emulator.arcade/files/artwork/lightgun/{romName}.zip";
            //MessageBox.Show(lgPushCmd);

            Process lgProcess = new Process();
            lgProcess.StartInfo.FileName = "adb";
            lgProcess.StartInfo.Arguments = lgPushCmd;
            lgProcess.StartInfo.RedirectStandardOutput = true;
            lgProcess.StartInfo.RedirectStandardError = true;
            lgProcess.StartInfo.UseShellExecute = false;
            lgProcess.StartInfo.CreateNoWindow = true;

            lgProcess.Start();

            string lgOutput = lgProcess.StandardOutput.ReadToEnd();
            exitCode = lgProcess.ExitCode;

            if (exitCode != 0)
            {
                MessageBox.Show($"Error! {lgOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string dbLoc = "/data/data/com.iircade.iiconsole/databases/Game.db";

            string dbUpdateCmd = $"update GAME set Reserve1=\'T#0#1\' where GAME.ID=\'{romName}.zip\';";
            //MessageBox.Show(dbUpdateCmd);
            Process dbProcess = new Process();
            dbProcess.StartInfo.FileName = "adb";
            dbProcess.StartInfo.Arguments = $"shell sqlite3 \\\"{dbLoc}\\\" \\\"{dbUpdateCmd}\\\"";
            dbProcess.StartInfo.RedirectStandardOutput = true;
            dbProcess.StartInfo.RedirectStandardError = true;
            dbProcess.StartInfo.UseShellExecute = false;
            dbProcess.StartInfo.CreateNoWindow = true;

            dbProcess.Start();
            string dbOutput = dbProcess.StandardOutput.ReadToEnd();
            exitCode = dbProcess.ExitCode;

            if (exitCode == 0) MessageBox.Show("Gun configured successfully!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                MessageBox.Show($"Error! {dbOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        private void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy; // Indicate that files can be copied here
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }

        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                Console.WriteLine(file);  // Or handle the file path as needed
                openFileDialog1.FileName = file;
                if (openFileDialog1.CheckFileExists)
                {
                    lblCFGFile.Text = "File opened: " + openFileDialog1.FileName;
                }
            }

        }

        private void btnReboot_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = "reboot";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            int exitCode = process.ExitCode;

            if (exitCode == 0)
            {
                MessageBox.Show("Reboot successful.", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Error code {exitCode}: {output}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFixClock_Click(object sender, EventArgs e)
        {
            string formattedDateTime = DateTime.Now.ToString("MMddHHmmyyyy.ss");
            MessageBox.Show(formattedDateTime, "Date/Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}