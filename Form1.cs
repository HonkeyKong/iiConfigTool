using System;
using System.IO;
using System.Timers;
using System.Xml;
using System.Xml.Schema;
using System.Diagnostics;
using System.DirectoryServices;
using System.Configuration;
using System.Windows.Forms;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;

namespace iiConfig
{
    public partial class frmMain : Form
    {
        string processOutput = "";
        int processExit = -1;
        private static System.Timers.Timer? deviceTimer;

        private bool multiFiles = false;
        private string pushFiles = "";
        private bool runProcess(string processName, string args)
        {
            Process process = new();
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

        private string previousOutput = "";

        private void refreshDevices()
        {
            int lstDevices = 0;
            if (runProcess("adb", "devices"))
            {
                string newOutput = processOutput;
                if (newOutput != previousOutput)
                {
                    lbDevices.Items.Clear();
                    foreach (string line in newOutput.Split(new char[] { '\r', }))
                    {
                        if (lstDevices > 0) lbDevices.Items.Add(line);
                        lstDevices++;
                    }
                    previousOutput = newOutput;
                }
            }
        }

        private void OnTimedEvent(object? source, ElapsedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => refreshDevices()));
            }
            else
            {
                refreshDevices();
            }
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
            try
            {
                deviceTimer = new System.Timers.Timer(5000);
                deviceTimer.Elapsed += OnTimedEvent;
                deviceTimer.AutoReset = true;
                deviceTimer.Enabled = true;
                deviceTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            refreshDevices();
        }

        private void btnSelectCFG_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);
            if (File.Exists(openFileDialog1.FileName))
            {
                if (validateCFG(openFileDialog1.FileName))
                {
                    lblCFGFile.Text = "File opened: " + openFileDialog1.FileName;
                    pushFiles = $"\"{openFileDialog1.FileName}\"";
                    multiFiles = false;
                }
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

            if ((pushFiles == "") && (multiFiles == false))
            {
                MessageBox.Show("Select a CFG file first.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!runProcess("adb", "root")) { MessageBox.Show("Root failed.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            string strPushCmd = $"-s {strDevice} push {pushFiles} /sdcard/Android/data/org.emulator.arcade/files/cfg/";

            if (runProcess("adb", strPushCmd))
            {
                MessageBox.Show($"{processOutput}", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Error code {processExit}: {processOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!cbLightGun.Checked) return;

            if (multiFiles)
            {
                MessageBox.Show("Sorry, only one light gun game can be configured at a time.");
                return;
            }

            string[] cfgNames = pushFiles.Split('\\');
            string romName = cfgNames[cfgNames.GetLength(0) - 1].Split('.')[0];
            string lgPushCmd = $"-s {strDevice} push lightgun.zip /sdcard/Android/data/org.emulator.arcade/files/artwork/lightgun/{romName}.zip";

            if (!runProcess("adb", lgPushCmd))
            {
                MessageBox.Show($"Error! {processOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string dbLoc = "/data/data/com.iircade.iiconsole/databases/Game.db";

            string dbUpdateCmd = $"update GAME set Reserve1=\'T#0#1\' where GAME.ID=\'{romName}.zip\';";
            string licenseUpdateCmd = $"update Config set Preload=\'1\';";
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
            pushFiles = "";
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                if (validateCFG(files[0]))
                {
                    multiFiles = false;
                    pushFiles = $"\"{files[0]}\"";
                    lblCFGFile.Text = $"File opened: {files[0]}";
                }
            }
            else
            {
                multiFiles = true;
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        if (validateCFG(file)) pushFiles += $"\"{file}\" ";
                        multiFiles = true;
                    }
                }
                lblCFGFile.Text = $"{files.Length} files loaded.";
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

        private static void ValidationCallback(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
                MessageBox.Show($"Warning: {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (e.Severity == XmlSeverityType.Error)
                Console.WriteLine($"Error: {e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static bool validateCFG(string FilePath)
        {
            try
            {
                string xsdData = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\r\n\r\n  <xs:element name=\"mameconfig\">\r\n    <xs:complexType>\r\n      <xs:sequence>\r\n        <xs:element name=\"system\" type=\"SystemType\"/>\r\n      </xs:sequence>\r\n      <xs:attribute name=\"version\" type=\"xs:string\" use=\"required\"/>\r\n    </xs:complexType>\r\n  </xs:element>\r\n\r\n  <xs:complexType name=\"SystemType\">\r\n    <xs:sequence>\r\n      <xs:element name=\"input\" type=\"InputType\"/>\r\n      <xs:element name=\"crosshairs\" type=\"CrosshairsType\"/>\r\n    </xs:sequence>\r\n    <xs:attribute name=\"name\" type=\"xs:string\" use=\"required\"/>\r\n  </xs:complexType>\r\n\r\n  <xs:complexType name=\"InputType\">\r\n    <xs:sequence>\r\n      <xs:element name=\"port\" type=\"PortType\" maxOccurs=\"unbounded\"/>\r\n    </xs:sequence>\r\n  </xs:complexType>\r\n\r\n  <xs:complexType name=\"PortType\">\r\n    <xs:sequence>\r\n      <xs:element name=\"newseq\" type=\"xs:string\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\r\n    </xs:sequence>\r\n    <xs:attribute name=\"tag\" type=\"xs:string\" use=\"required\"/>\r\n    <xs:attribute name=\"type\" type=\"xs:string\" use=\"required\"/>\r\n    <xs:attribute name=\"mask\" type=\"xs:string\" use=\"required\"/>\r\n    <xs:attribute name=\"defvalue\" type=\"xs:string\" use=\"required\"/>\r\n  </xs:complexType>\r\n\r\n  <xs:complexType name=\"CrosshairsType\">\r\n    <xs:sequence>\r\n      <xs:element name=\"crosshair\" type=\"CrosshairType\" maxOccurs=\"unbounded\"/>\r\n    </xs:sequence>\r\n  </xs:complexType>\r\n\r\n  <xs:complexType name=\"CrosshairType\">\r\n    <xs:attribute name=\"player\" type=\"xs:string\" use=\"required\"/>\r\n    <xs:attribute name=\"mode\" type=\"xs:string\" use=\"required\"/>\r\n  </xs:complexType>\r\n\r\n</xs:schema>\r\n";

                // Load and validate the XML document
                XmlReaderSettings settings = new();

                settings.ValidationType = ValidationType.Schema;
                XmlSchemaSet schemas = new();
                schemas.Add(null, XmlReader.Create(new StringReader(xsdData)));
                settings.Schemas = schemas;

                // Ignore whitespace
                settings.IgnoreWhitespace = true;

                settings.ValidationEventHandler += ValidationCallback;

                using XmlReader reader = XmlReader.Create(FilePath, settings);
                while (reader.Read()) { }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Validation failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
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

        private void btnFixLicense_Click(object sender, EventArgs e)
        {
            if ((strDevice == "null") || (strDevice == ""))
            {
                MessageBox.Show("Select a device first.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string dbLoc = "/data/data/com.iircade.iiconsole/databases/Game.db";
            string licenseUpdateCmd = $"update CONFIG set Preload=\'1\';";
            string dbExecCmd = $"shell sqlite3 \\\"{dbLoc}\\\" \\\"{licenseUpdateCmd}\\\"";

            if (runProcess("adb", dbExecCmd)) MessageBox.Show("Licenses fixed successfully!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                MessageBox.Show($"Error! {processOutput}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
    }
}