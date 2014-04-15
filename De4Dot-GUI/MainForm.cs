using System;
using System.Collections.Generic;
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
			this.rbt64Bit.Enabled = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").Equals("x64");
		}
		
		
		void BtnSearchInputClick(object sender, EventArgs e)
		{
			
			using (OpenFileDialog of = new OpenFileDialog())
			{
				of.Filter = "PE files (*.exe, *.dll)|*.exe;*.dll|All Files|*.*";
				of.CheckFileExists = true;
				of.CheckPathExists = true;
				of.ReadOnlyChecked = false;
				of.RestoreDirectory = true;
				of.Multiselect = false;
				if (of.ShowDialog() == DialogResult.OK)
				{
					Regex regex;
					Match match;
					String filename;
					
					filename = of.FileName;
					txtInput.Text = filename;
					regex = new Regex(@"(?<filename>.+?)(?<extension>\..+?)$", RegexOptions.CultureInvariant);
					match = regex.Match(filename);
					if(match.Groups.Count > 0)
						txtOutput.Text = String.Format("{0}_de4dot{1}", match.Groups["filename"].Value, match.Groups["extension"].Value);
					else
						txtOutput.Text = String.Format("{0}_de4dot", filename);
				}
			}
		}
		
		void BtnSearchOutputClick(object sender, EventArgs e)
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
		
		void BtnWorkClick(object sender, EventArgs e)
		{
			Process de4dot;
			String exeName, arguments;
			
			if(!File.Exists(txtInput.Text)) {
				MessageBox.Show("Input files doesnt exists!", "Err0r!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			if(txtInput.Text == txtOutput.Text) {
				MessageBox.Show("Output file must be different from input file!", "Err0r!", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			if(rbt32Bit.Checked)
				exeName = "de4dot.exe";
			else if (rbt64Bit.Checked)
				exeName = "de4dot-x64.exe";
			else
				exeName = "de4dot.exe";		// Failsafe
			
			arguments = String.Format("-f \"{0}\" -o \"{1}\" {2}", txtInput.Text, txtOutput.Text, txtAdditional.Text);
			
			de4dot = new Process {
				StartInfo = new ProcessStartInfo {
			        FileName = exeName,
			        Arguments = arguments,
			        UseShellExecute = false,
			        RedirectStandardOutput = true,
			        CreateNoWindow = true
			    }
			};
			
			try {
				txtOut.AppendText(String.Format("=== START NEW DEOBFUSCATION ==={0}{0}Command: {1}{0}Output:{0}","\r\n", String.Format("{0} {1}", exeName, arguments)));
				de4dot.Start();
				while(!de4dot.StandardOutput.EndOfStream)
					txtOut.AppendText(String.Format("{0}{1}",de4dot.StandardOutput.ReadLine(), "\r\n"));
			}
			catch (System.ComponentModel.Win32Exception ex) {
				MessageBox.Show("This file must be in the same folder of de4dot", "Err0r!", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally {
				txtOut.AppendText(String.Format("{0}{0}=== END OF DEOBFUSCATION ==={0}{0}", "\r\n"));
			}
		}
		
	}
}
