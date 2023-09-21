using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Diagnostics;
using System.DirectoryServices;
using System.Configuration;

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
            if (lbDevices.SelectedIndex > 0)
            {
                strDevice = lbDevices.Items[lbDevices.SelectedIndex].ToString().Split(new char[] { '\t' })[0].ToString().Split(new char[] { ':' })[0].Split(new char[] { '\n' })[1];
                label1.Text = "Selected Device: " + strDevice;
            }
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
                if (validateCFG(openFileDialog1.FileName)) lblCFGFile.Text = "File opened: " + openFileDialog1.FileName;
                else openFileDialog1.FileName = "";
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
            rootProcess.StartInfo.Arguments = $"-s {strDevice} root";
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
            dbProcess.StartInfo.Arguments = $"-s {strDevice} shell sqlite3 \\\"{dbLoc}\\\" \\\"{dbUpdateCmd}\\\"";
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
                    if (validateCFG(openFileDialog1.FileName)) lblCFGFile.Text = "File opened: " + openFileDialog1.FileName;
                    else openFileDialog1.FileName = "";
                }
            }

        }

        private void btnReboot_Click(object sender, EventArgs e)
        {
            if ((strDevice == "null") || (strDevice == ""))
            {
                MessageBox.Show("Select a device first.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Process process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = $"-s {strDevice} reboot";
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

        private bool validateCFG(string FilePath)
        {
            try
            {
                // Load and validate the XML document
                string schemaFilePath = "mameconfig.xsd"; // Replace with your schema file path
                XmlReaderSettings settings = new XmlReaderSettings();

                settings.ValidationType = ValidationType.Schema;
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add(null, schemaFilePath);
                settings.Schemas = schemas;

                settings.ValidationEventHandler += ValidationCallback;

                using (XmlReader reader = XmlReader.Create(FilePath, settings))
                {
                    while (reader.Read()) { }
                }

                //MessageBox.Show("Validation successful!");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Validation failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static void ValidationCallback(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
                MessageBox.Show($"Warning: {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (e.Severity == XmlSeverityType.Error)
                Console.WriteLine($"Error: {e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnFixClock_Click(object sender, EventArgs e)
        {
            if ((strDevice == "null") || (strDevice == ""))
            {
                MessageBox.Show("Select a device first.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string formattedDateTime = DateTime.Now.ToString("MMddHHmmyyyy.ss");
            // MessageBox.Show(formattedDateTime, "Date/Time", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Process process1 = new Process();
            process1.StartInfo.FileName = "adb";
            process1.StartInfo.Arguments = $"-s {strDevice} shell toybox date";
            process1.StartInfo.RedirectStandardOutput = true;
            process1.StartInfo.RedirectStandardError = true;
            process1.StartInfo.UseShellExecute = false;
            process1.StartInfo.CreateNoWindow = true;

            process1.Start();

            string output = process1.StandardOutput.ReadToEnd();
            int exitCode = process1.ExitCode;

            if (exitCode != 0)
            {
                MessageBox.Show($"Error code {exitCode}: {output}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Process process2 = new Process();
            process2.StartInfo.FileName = "adb";
            process2.StartInfo.Arguments = $"-s {strDevice} shell \"su 0 toybox date {formattedDateTime}\"";
            process2.StartInfo.RedirectStandardOutput = true;
            process2.StartInfo.RedirectStandardError = true;
            process2.StartInfo.UseShellExecute = false;
            process2.StartInfo.CreateNoWindow = true;

            process2.Start();

            output = process2.StandardOutput.ReadToEnd();
            exitCode = process2.ExitCode;

            if (exitCode != 0)
            {
                MessageBox.Show($"Error code {exitCode}: {output}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Date and time updated successfully!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
    }
}