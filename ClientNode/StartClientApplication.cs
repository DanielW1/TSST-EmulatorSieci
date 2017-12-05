using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientNode
{
    public partial class StartClientApplication : Form
    {
        public static StartClientApplication _StartClientApplication;
        string ClientIP;
        string ClientPort;
        string CloudPort;

        public StartClientApplication()
        {
            InitializeComponent();
            _StartClientApplication = this;
        }

        private void buttonStartClient_Click(object sender, EventArgs e)
        {
            ClientIP = textBoxClientIP.Text;
            ClientPort = textBoxClientPort.Text;
            CloudPort = textBoxCloudPort.Text;


            _StartClientApplication.Hide();
            var ClientApplicationForm = new ClientApplication(ClientIP, ClientPort, CloudPort);
            ClientApplicationForm.Closed += (s, args) => _StartClientApplication.Close();
            ClientApplicationForm.Show();
        }
    }
}
