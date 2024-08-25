using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace RhoLoader
{
    public partial class AboutMe : Form
    {
        private CheckResult Result = CheckResult.Checking;

        public AboutMe()
        {
            InitializeComponent();
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            this.version.Text = $"Version : {(ver.Revision == 0 ? "" : ver.Revision == 1 ? "Beta " : ver.Revision == 2 ? "Dev " : ver.Revision == 3 ? "Unstable " : "Custom ")}{ver.Major}.{ver.Minor}.{ver.Build}";
        }


        private enum CheckResult
        {
            UpToDate,NewVersionReleased,NoNetworkConnection,UnknownError,Checking
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("If this program is running incorrectly,\r\nYou can report to RhoReader github page.\r\nThank you for reporting!");
        }

        private void ChangeUpdateStatus(CheckResult status, int _invoke_recursion_counter = 0)
        {
            if (this.InvokeRequired)
            {
                Action<CheckResult, int> dele = new Action<CheckResult, int>(ChangeUpdateStatus);
                this.Invoke(dele, status, _invoke_recursion_counter + 1);
            }
            else
            {
                try
                {
                    Result = status;
                    switch (Result)
                    {
                        case CheckResult.UpToDate:
                            checkUpdateStatus.Text = "about_UpToDate".GetStringBag();
                            checkUpdateStatus.ForeColor = Color.BlueViolet;
                            break;
                        case CheckResult.NewVersionReleased:
                            checkUpdateStatus.Text = "about_NewVersion".GetStringBag();
                            checkUpdateStatus.ForeColor = Color.DodgerBlue;
                            break;
                        case CheckResult.NoNetworkConnection:
                            checkUpdateStatus.Text = "about_NoInternet".GetStringBag();
                            checkUpdateStatus.ForeColor = Color.Red;
                            break;
                        case CheckResult.UnknownError:
                            checkUpdateStatus.Text = "about_UnknowError".GetStringBag();
                            checkUpdateStatus.ForeColor = Color.Red;
                            break;
                        case CheckResult.Checking:
                            checkUpdateStatus.Text = "about_Checking".GetStringBag();
                            checkUpdateStatus.ForeColor = Color.Gray;
                            break;
                    }
                }
                catch(Exception ex)
                {
                    // Ensure this function will running at the thread that process window events.
                    if (_invoke_recursion_counter >= 5)
                        throw ex;
                    Action<CheckResult, int> dele = new Action<CheckResult, int>(ChangeUpdateStatus);
                    this.Invoke(dele, status, _invoke_recursion_counter + 1);
                }
                
            }
        }
    }
}
