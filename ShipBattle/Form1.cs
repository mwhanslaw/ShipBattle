using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;

namespace ShipBattle
{
    public partial class FrmLaunchGame : Form
    {
        
        string PlayerName, IpAdd, Port;
        bool IsHost = false;
        System.Media.SoundPlayer sp = new System.Media.SoundPlayer(@"War Drums.wav");

        public FrmLaunchGame()
        {
            InitializeComponent();

            //default settings for standalone single pc use
            txtbIP.Text = "127.0.0.1";
            txtbName.Text = "Player1";
            txtbPort.Text = "4545";

            sp.PlayLooping();
        }

        bool ValidateInput(string ip, string port)
        {
            IPAddress check;
            int portNum;

            if ((IPAddress.TryParse(ip, out check) == true) && (port.Length == 4) && (int.TryParse(port, out portNum) == true))
                return true;
            else
            {
                txtbPort.Clear();
                txtbIP.Clear();
                txtbIP.Focus();
                return false;
            }
        }

        private void btnHost_Click(object sender, EventArgs e)
        {
            if ((txtbIP.Text != "" && txtbName.Text != "" && txtbPort.Text != "") && (ValidateInput(txtbIP.Text,txtbPort.Text)==true))
            {
                IsHost = true;
                PlayerName = txtbName.Text;
                IpAdd = txtbIP.Text; 
                Port = txtbPort.Text;

                FrmGame newGame = new FrmGame(PlayerName, IpAdd, Port, IsHost); //(string player, string ip, string port, bool host)
              
                sp.Stop();
                newGame.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Please fill in all required fields, and check values are valid");

            }
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            if ((txtbIP.Text != "" && txtbName.Text != "" && txtbPort.Text != "")&& (ValidateInput(txtbIP.Text, txtbPort.Text) == true))
            {
                PlayerName = txtbName.Text;
                IpAdd = txtbIP.Text;
                Port = txtbPort.Text;

                FrmGame newGame = new FrmGame(PlayerName, IpAdd, Port, IsHost); //(string player, string ip, string port, bool host)

                sp.Stop();
                newGame.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Please fill in all required fields, and check values are valid");
            }
        }
    }
}
