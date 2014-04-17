using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace De4DotGUI
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		public MainForm()
		{
		    InitializeComponent();
		    string environmentVariable = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
		    if (environmentVariable != null)
		        rbt64Bit.Enabled = environmentVariable.Equals("x64");
		}

        private void btnSearchInput_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog of = new OpenFileDialog())
            {
                of.Filter = "PE files (*.exe, *.dll)|*.exe;*.dll|All Files|*.*";
                of.CheckFileExists = true;
                of.CheckPathExists = true;
                of.ReadOnlyChecked = false;
                of.RestoreDirectory = true;
                of.Multiselect = false;

                if (of.ShowDialog() != DialogResult.OK) return;
                string filename = of.FileName;
                txtInput.Text = filename;
                Regex regex = new Regex(@"(?<filename>.+?)(?<extension>\..+?)$", RegexOptions.CultureInvariant);
                Match match = regex.Match(filename);
                txtOutput.Text = match.Groups.Count > 0 ? String.Format("{0}_de4dot{1}", match.Groups["filename"].Value, match.Groups["extension"].Value) : String.Format("{0}_de4dot", filename);
            }
        }

        private void btnSearchOutput_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sf = new SaveFileDialog())
            {
                sf.Filter = "All Files|*.*";
                sf.CheckFileExists = false;
                sf.CheckPathExists = true;
                sf.RestoreDirectory = true;
                sf.FileName = txtOutput.Text;

                if (sf.ShowDialog() == DialogResult.OK)
                    txtOutput.Text = sf.FileName;
            }
        }

        private void btnWork_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtInput.Text))
            {
                MessageBox.Show("Input file doesn't exists!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtInput.Text == txtOutput.Text)
            {
                MessageBox.Show("Output file must be different from input file!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string exeName = "de4dot.exe";
            if (rbt64Bit.Checked)
                exeName = "de4dot-x64.exe";

            if (!File.Exists(exeName))
            {
                MessageBox.Show(exeName + " not found! de4dot must be in the same directory of this program.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string arguments = String.Format("-f \"{0}\" -o \"{1}\" {2}", txtInput.Text, txtOutput.Text, txtAdditional.Text);

// ReSharper disable InconsistentNaming
            Process de4dot = new Process
// ReSharper restore InconsistentNaming
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exeName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            try
            {
                txtOut.AppendText(String.Format("=== START NEW DEOBFUSCATION ==={0}{0}Command: {1}{0}Output:{0}", "\r\n", String.Format("{0} {1}", exeName, arguments)));
                de4dot.Start();
                while (!de4dot.StandardOutput.EndOfStream)
                    txtOut.AppendText(String.Format("{0}{1}", de4dot.StandardOutput.ReadLine(), "\r\n"));
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("This program must be in the same folder of de4dot", "Err0r!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                txtOut.AppendText(String.Format("{0}{0}=== END OF DEOBFUSCATION ==={0}{0}", "\r\n"));
            }
        }
	}
}
