using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Diagnostics;
using System.DirectoryServices;
using System.Configuration;
using System.Windows.Forms;

namespace iiConfig
{
    public partial class frmMain : Form
    {
        string processOutput = "";
        int processExit = -1;

        private bool runProcess(string processName, string args)
        {
            Process process = new Process();
            process.StartInfo.FileName = processName;
            process.StartInfo.Arguments = $"-s {strDevice} " + args;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            processOutput = process.StandardOutput.ReadToEnd();
            processExit = process.ExitCode;
            if (processExit == 0) { return true; } else { return false; }
        }

        public string strDevice = "null";
        public frmMain()
        {
            InitializeComponent();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbDevices.SelectedIndex >= 0)
            {
                strDevice = lbDevices.Items[lbDevices.SelectedIndex].ToString().Split(new char[] { '\t' })[0].ToString().Split(new char[] { ':' })[0].Split(new char[] { '\n' })[1];
                label1.Text = "Selected Device: " + strDevice;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            int lstDevices = 0;

            if (runProcess("adb", "devices"))
            {
                lbDevices.Items.Clear();
                foreach (string line in processOutput.Split(new char[] { '\r', }))
                {
                    if (lstDevices > 0) lbDevices.Items.Add(line);
                    lstDevices++;
                }
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

            if (!runProcess("adb", "root")) { MessageBox.Show("Root failed.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            string[] cfgNames = openFileDialog1.FileName.Split('\\');
            string romName = cfgNames[cfgNames.GetLength(0) - 1].Split('.')[0];

            string strPushCmd = $"-s {strDevice} push \"{openFileDialog1.FileName}\" /sdcard/Android/data/org.emulator.arcade/files/cfg/";

            if (runProcess("adb", strPushCmd))
            {
                MessageBox.Show($"{processOutput}", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Error code {processExit}: {processOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!cbLightGun.Checked) return;

            string lgPushCmd = $"-s {strDevice} push lightgun.zip /sdcard/Android/data/org.emulator.arcade/files/artwork/lightgun/{romName}.zip";

            if (!runProcess("adb", lgPushCmd))
            {
                MessageBox.Show($"Error! {processOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string dbLoc = "/data/data/com.iircade.iiconsole/databases/Game.db";

            string dbUpdateCmd = $"update GAME set Reserve1=\'T#0#1\' where GAME.ID=\'{romName}.zip\';";
            string dbExecCmd = $"shell sqlite3 \\\"{dbLoc}\\\" \\\"{dbUpdateCmd}\\\"";

            if (runProcess("adb", dbExecCmd)) MessageBox.Show("Gun configured successfully!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                MessageBox.Show($"Error! {processOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if (runProcess("adb", "reboot"))
            {
                MessageBox.Show("Reboot successful.", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Error code {processExit}: {processOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool validateCFG(string FilePath)
        {
            try
            {
                // Load and validate the XML document
                string schemaFilePath = "mameconfig.xsd";
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

            if (!runProcess("adb", "shell toybox date"))
            {
                MessageBox.Show($"Error code {processExit}: {processOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!runProcess("adb", $"shell \"su 0 toybox date {formattedDateTime}\""))
            {
                MessageBox.Show($"Error code {processExit}: {processOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Date and time updated successfully!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
    }
}