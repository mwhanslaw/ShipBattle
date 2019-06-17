using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Linq;

namespace ShipBattle
{
    public partial class FrmGame : Form
    {
        //player variables
        public String playerName, PortNum, opponentName;
        public int yourScore = 0, opponentScore = 0;
        public bool Ishost = false;
        public bool IsGoingFirst = false;
        //tcp-ip variables
        IPAddress ipAddress;
        TcpListener listener;
        TcpClient client;
        NetworkStream ns;

        //Threads
        Thread t1 = null; // chat thread 
        delegate void SetTextCallBack(string text);

        //temp variables to set up the ship before creating ship objects 
        int carrierSpots = 5;
        int battleshipSpots = 4;
        int cruiserSpots = 3;
        int destroyerSpots = 2;
        string[] carrierLoc = new string[5];
        string[] battleshipLoc = new string[4];
        string[] cruiserLoc = new string[3];
        string[] destroyerLoc = new string[2];
        // u = undecided, h = horizontal, v = vertical 
        char carrierOrientation = 'u';        
        char battleshipOrientation = 'u';
        char cruiserOrientation = 'u';
        char destroyerOrientation = 'u';
        //ship variables 
        string[] EnemyShips = new string[14]; //pull in from opponent netstream to set up ships
        public ship s2, s3, s4, s5; //my ships
        public ship s2E, s3E, s4E, s5E; //enemy ships
        //sound variables
        bool IsInBattle = false;
        System.Media.SoundPlayer sp;


        public FrmGame(string player, string ip, string port, bool host) // initialize variables
        {
            InitializeComponent();

            playerName = player;
            ipAddress = IPAddress.Parse(ip); // already validated in first form
            PortNum = port;
            Ishost = host;

            lblYaxisE.Text = "A\n\nB\n\nC\n\nD\n\nE\n\nF\n\nG\n\nH\n\nI\n\nJ"; //y axis labels
            lblYaxis.Text = "A\n\nB\n\nC\n\nD\n\nE\n\nF\n\nG\n\nH\n\nI\n\nJ"; //y axis labels
            txtbMoves.Text = "x";
            
        }

        private void FrmGame_Shown(object sender, EventArgs e)//set up board on form show
        {
            gbxEnemyBoard.Enabled = false;
            this.Refresh();
            Thread.Sleep(100);

            if (Ishost == true)// Server machine
            {
                try
                {
                    txtbBattleLog.AppendText("Waiting for a player to join . . ." + Environment.NewLine);
                    listener = new TcpListener(IPAddress.Any, Convert.ToInt32(PortNum));
                    listener.Start();
                    client = listener.AcceptTcpClient();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error has occured ... " + Environment.NewLine + ex);
                }                             
            }
            else // client machine
            {
                try
                {
                    client = new TcpClient("localhost", Convert.ToInt32(PortNum));
                }
                catch (Exception)
                {
                    MessageBox.Show("Your connection has timed out! " + Environment.NewLine + " Please make sure you have the correct connection details.");                    
                    Application.Exit();
                }
            }
            try
            {
                ns = client.GetStream();

                byte[] byteTime = Encoding.ASCII.GetBytes(playerName + ", is now connected to the game!" + Environment.NewLine);
                ns.Write(byteTime, 0, byteTime.Length);
                ns.FlushAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to Connect to your opponent. . . closing application");
                Application.Exit();
            }
        

            t1 = new Thread(GetLine); //thread for chat messaging
            t1.Priority = ThreadPriority.Highest;
            t1.Start();

            lblYourName.Text = playerName;
            lblPlayer1Score.Text = Convert.ToString(yourScore);
            lblPlayer2Score.Text = Convert.ToString(opponentScore);
            gbxEnemyBoard.Enabled = false;
            lblInstructions.Text = "[->] : Place your Carrier on your battle board (5 spots, vertical or horizontal)";
            
            //music set up             
            axWMPsetup.URL = "War Drums.wav";
            if (cbxSound.Checked == true) 
                axWMPsetup.Ctlcontrols.play();

            if (Ishost == true)
            {
                IsGoingFirst = true;
            }
        }

        public void GetLine() // read data
        {
            byte[] bytes = new byte[1024];

            while (true)
            {
                try
                {
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    this.SetText(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                }
                catch (Exception) { }
            }

        }

        private void FrmGame_FormClosing(object sender, FormClosingEventArgs e)//end threads before close
        {            
            try
            {
                if (t1 != null)
                    t1.IsBackground = true; // set  thread to back ground 

                axWMPsetup.Ctlcontrols.stop(); // stop the music 

                byte[] byteTime = Encoding.ASCII.GetBytes("exitCode");
                ns.Write(byteTime, 0, byteTime.Length);
                Thread.Sleep(1000);
            }
            catch(Exception) { }
            finally
            {
                Environment.Exit(-1);
                Application.Exit();
            }
        }

        private void cbxSound_CheckedChanged(object sender, EventArgs e)
        {

            if (cbxSound.Checked == false)
            {
                if (IsInBattle == false)
                    axWMPsetup.Ctlcontrols.pause();
                else
                    axWMPbattle.Ctlcontrols.pause();
            }
            else
            {
                if (IsInBattle == false)
                    axWMPsetup.Ctlcontrols.play();
                else
                    axWMPbattle.Ctlcontrols.play();
            } 
            
        }

        private void SetText(string text) 
        {            
            if (this.txtbBattleLog.InvokeRequired)
            {
                SetTextCallBack del = new SetTextCallBack(SetText);
                this.Invoke(del, new object[] { text });
            }
            else
            {
                if (text == "exitCode")
                {
                    MessageBox.Show("Your Opponent has left the game! closing session . . . ");
                    t1.IsBackground = true;
                    Application.Exit();
                }
                else if (text == "reset")
                {
                    txtbMoves.Text = "InitializeEnemyShips";
                }
                else if (txtbMoves.Text == "")
                {
                    this.txtbMoves.Text = text;
                    gbxEnemyBoard.Enabled = true;
                    lblInstructions.Text = "[->] : your turn";
                    //check your ships for damage
                    string num = text.Replace(System.Environment.NewLine, string.Empty); // remove \r\n from reply 

                    if (s2.IsHit(num) == true)
                    {
                        gbxYourBoard.Controls["btn" + num].Enabled = true;
                        gbxYourBoard.Controls["btn" + num].BackColor = Color.DarkRed;
                        gbxYourBoard.Controls["btn" + num].Text = "X";
                        gbxYourBoard.Controls["btn" + num].Enabled = false;

                        if (s2.IsSunk == true)// was it sunk?
                        {
                            txtbBattleLog.AppendText("[log] : Your Opponent Fired at " + num + " and Sunk a Destroyer!" + Environment.NewLine);
                            SoundEffects('s');
                            pboxMy2.Image = Properties.Resources.destroyerX;
                            pboxMy2.Refresh();
                        }
                        else// just a hit
                        {
                            txtbBattleLog.AppendText("[log] : Your Opponent Fired at " + num + " and Hit!" + Environment.NewLine);
                            SoundEffects('s');
                            SoundEffects('h');
                        }
                    }
                    else if (s3.IsHit(num) == true)
                    {
                        gbxYourBoard.Controls["btn" + num].Enabled = true;
                        gbxYourBoard.Controls["btn" + num].BackColor = Color.DarkRed;
                        gbxYourBoard.Controls["btn" + num].Text = "X";
                        gbxYourBoard.Controls["btn" + num].Enabled = false;

                        if (s3.IsSunk == true)// was it sunk?
                        {
                            txtbBattleLog.AppendText("[log] : Your Opponent Fired at " + num + " and Sunk a Cruiser!" + Environment.NewLine);
                            SoundEffects('s');
                            pboxMy3.Image = Properties.Resources.cruiserX;
                            pboxMy3.Refresh();
                        }
                        else// just a hit
                        {
                            txtbBattleLog.AppendText("[log] : Your Opponent Fired at " + num + " and Hit!" + Environment.NewLine);
                            SoundEffects('h');
                        }
                    }
                    else if (s4.IsHit(num) == true)
                    {
                        gbxYourBoard.Controls["btn" + num].Enabled = true;
                        gbxYourBoard.Controls["btn" + num].BackColor = Color.DarkRed;
                        gbxYourBoard.Controls["btn" + num].Text = "X";
                        gbxYourBoard.Controls["btn" + num].Enabled = false;

                        if (s4.IsSunk == true)// was it sunk?
                        {
                            txtbBattleLog.AppendText("[log] : Your Opponent Fired at " + num + " and Sunk a Battleship!" + Environment.NewLine);
                            SoundEffects('s');
                            pboxMy4.Image = Properties.Resources.battleshipX1;
                            pboxMy4.Refresh();
                        }
                        else// just a hit
                        {
                            txtbBattleLog.AppendText("[log] : Your Opponent Fired at " + num + " and Hit!" + Environment.NewLine);
                            SoundEffects('h');
                        }
                    }
                    else if (s5.IsHit(num) == true)
                    {
                        gbxYourBoard.Controls["btn" + num].Enabled = true;
                        gbxYourBoard.Controls["btn" + num].BackColor = Color.DarkRed;
                        gbxYourBoard.Controls["btn" + num].Text = "X";
                        gbxYourBoard.Controls["btn" + num].Enabled = false;

                        if (s5.IsSunk == true)// was it sunk?
                        {
                            txtbBattleLog.AppendText("[log] : Your Opponent Fired at " + num + " and Sunk a Carrier!" + Environment.NewLine);
                            SoundEffects('s');
                            pboxMy5.Image = Properties.Resources.carrierX;
                            pboxMy5.Refresh();
                        }
                        else// just a hit
                        {
                            txtbBattleLog.AppendText("[log] : Your Opponent Fired at " + num + " and Hit!" + Environment.NewLine);
                            SoundEffects('h');
                        }
                    }
                    else // miss
                    {
                        txtbBattleLog.AppendText("[log] : Your Opponent Fired at " + num + " and Missed!" + Environment.NewLine);
                        gbxYourBoard.Controls["btn" + num].Enabled = true;
                        gbxYourBoard.Controls["btn" + num].BackColor = Color.LightBlue;
                        gbxYourBoard.Controls["btn" + num].Text = "O";
                        gbxYourBoard.Controls["btn" + num].Enabled = false;
                        SoundEffects('m');
                    }

                    //check for defeat condition 
                    if (s2.IsSunk == true && s3.IsSunk == true && s4.IsSunk == true && s5.IsSunk == true)
                    {
                        txtbBattleLog.AppendText("[log] : Your Opponent Fired at " + num + " and Defeated you!" + Environment.NewLine);
                        SoundEffects('s');
                        Thread.Sleep(2500);
                        SoundEffects('d');
                        MessageBox.Show("You have been defeated!");
                        opponentScore++;
                        lblPlayer2Score.Text = Convert.ToString(opponentScore);
                        lblInstructions.Text = "[->] : Place your Carrier on your battle board (5 spots, vertical or horizontal)";
                        ResetGame();

                    }
                }
                else if (txtbMoves.Text == "InitializeEnemyShips")
                {
                    //import all enemy ship placements
                    EnemyShips = text.Split(',');
                    // create enemy ship objects 
                    string[] temp = new string[2];
                    temp[0] = EnemyShips[0];
                    temp[1] = EnemyShips[1];
                    s2E = new ship(2, temp);

                    temp = new string[3];
                    temp[0] = EnemyShips[2];
                    temp[1] = EnemyShips[3];
                    temp[2] = EnemyShips[4];
                    s3E = new ship(3, temp);

                    temp = new string[4];
                    temp[0] = EnemyShips[5];
                    temp[1] = EnemyShips[6];
                    temp[2] = EnemyShips[7];
                    temp[3] = EnemyShips[8];
                    s4E = new ship(4, temp);

                    temp = new string[5];
                    temp[0] = EnemyShips[9];
                    temp[1] = EnemyShips[10];
                    temp[2] = EnemyShips[11];
                    temp[3] = EnemyShips[12];
                    temp[4] = EnemyShips[13];
                    s5E = new ship(5, temp);
                    txtbMoves.Clear();
                    if (Ishost == true)
                        btnStartGame.Visible = true;
                }

                else
                {   //get opponents name during game connection handshake 
                    string[] temp = text.Split();
                    lblOpponentName.Text = temp[0].Remove(temp[0].Length - 1, 1);
                    opponentName = temp[0].Remove(temp[0].Length - 1, 1);
                    txtbBattleLog.Text += text;
                    txtbMoves.Text = "InitializeEnemyShips";
                }
            }
        }

        private void ShipSetup(object sender, EventArgs e) //set up all ship locations and variables
        {

            Button num = (Button)sender;
            string shipLoc = num.Name.Substring(3); //remove btn from button name 
            

            if (carrierSpots == 5)
            {
                if (ValidateInitialShipPlacement(5, shipLoc, "carrier") == true)
                {
                    SoundEffects('p');
                    carrierLoc[4] = shipLoc;
                    num.BackColor = Color.Green;
                    num.Enabled = false;
                    carrierSpots--;
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Ship Placement, please try another spot.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }

            }
            else if (carrierSpots == 4)
            {
                Array.Sort(carrierLoc); // sort array to find the outer values of the ship, nulls go first 
                char test = CheckAdditionalShipSpots(carrierOrientation, carrierLoc[4], carrierLoc[4], shipLoc);
                if (test != '\0')
                {
                    SoundEffects('p');
                    carrierLoc[3] = shipLoc;
                    num.BackColor = Color.Green;
                    num.Enabled = false;
                    carrierSpots--;
                    carrierOrientation = test; // set the orientation of the ship for future checks

                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Placement, Must be connected horizontally or vertically.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }
            }
            else if (carrierSpots == 3)
            {
                Array.Sort(carrierLoc); // sort array to find the outer values of the ship, nulls go first 
                char test = CheckAdditionalShipSpots(carrierOrientation, carrierLoc[3], carrierLoc[4], shipLoc);
                if (test != '\0')
                {
                    SoundEffects('p');
                    carrierLoc[2] = shipLoc;
                    num.BackColor = Color.Green;
                    num.Enabled = false;
                    carrierSpots--;
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Placement, Must be connected to your carrier ship.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }
            }
            else if (carrierSpots == 2)
            {
                Array.Sort(carrierLoc); // sort array to find the outer values of the ship, nulls go first 
                char test = CheckAdditionalShipSpots(carrierOrientation, carrierLoc[2], carrierLoc[4], shipLoc);
                if (test != '\0')
                {
                    SoundEffects('p');
                    carrierLoc[1] = shipLoc;
                    num.BackColor = Color.Green;
                    num.Enabled = false;
                    carrierSpots--;
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Placement, Must be connected to your carrier ship.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }
            }
            else if (carrierSpots == 1)
            {
                Array.Sort(carrierLoc); // sort array to find the outer values of the ship, nulls go first 
                char test = CheckAdditionalShipSpots(carrierOrientation, carrierLoc[1], carrierLoc[4], shipLoc);
                if (test != '\0')
                {
                    SoundEffects('p');
                    carrierLoc[0] = shipLoc;
                    num.BackColor = Color.Green;
                    num.Enabled = false;
                    carrierSpots--;
                    lblInstructions.Text = "[->] : Place your BattleShip on your battle board (4 spots, vertical or horizontal)";
                    s5 = new ship(5, carrierLoc);
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Placement, Must be connected to your carrier ship.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }
            }
            else if (battleshipSpots == 4)
            {
                if (ValidateInitialShipPlacement(4, shipLoc, "battleship") == true)
                {
                    SoundEffects('p');
                    battleshipLoc[0] = shipLoc;
                    num.BackColor = Color.Yellow;
                    num.Enabled = false;
                    battleshipSpots--;
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Ship Placement, please try another spot.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }
            }
            else if (battleshipSpots == 3)
            {
                Array.Sort(battleshipLoc);
                char test = CheckAdditionalShipSpots(battleshipOrientation, battleshipLoc[3], battleshipLoc[3], shipLoc);
                if (test != '\0')
                {
                    SoundEffects('p');
                    battleshipLoc[2] = shipLoc;
                    num.BackColor = Color.Yellow;
                    num.Enabled = false;
                    battleshipSpots--;
                    battleshipOrientation = test;
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Placement, Must be connected to your battle ship.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }

            }
            else if (battleshipSpots == 2)
            {
                Array.Sort(battleshipLoc);
                char test = CheckAdditionalShipSpots(battleshipOrientation, battleshipLoc[2], battleshipLoc[3], shipLoc);
                if (test != '\0')
                {
                    SoundEffects('p');
                    battleshipLoc[1] = shipLoc;
                    num.BackColor = Color.Yellow;
                    num.Enabled = false;
                    battleshipSpots--;
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Placement, Must be connected to your battle ship.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }
            }
            else if (battleshipSpots == 1)
            {
                Array.Sort(battleshipLoc);
                char test = CheckAdditionalShipSpots(battleshipOrientation, battleshipLoc[1], battleshipLoc[3], shipLoc);
                if (test != '\0')
                {
                    SoundEffects('p');
                    battleshipLoc[0] = shipLoc;
                    num.BackColor = Color.Yellow;
                    num.Enabled = false;
                    battleshipSpots--;
                    lblInstructions.Text = "[->] : Place your Cruiser on your battle board (3 spots, vertical or horizontal)";
                    s4 = new ship(4, battleshipLoc);
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Placement, Must be connected to your battle ship.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }
            }
            else if (cruiserSpots == 3)
            {
                if (ValidateInitialShipPlacement(3, shipLoc, "cruiser") == true)
                {
                    SoundEffects('p');
                    cruiserLoc[0] = shipLoc;
                    num.BackColor = Color.Red;
                    num.Enabled = false;
                    cruiserSpots--;
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Ship Placement, please try another spot.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }
            }
            else if (cruiserSpots == 2)
            {
                Array.Sort(cruiserLoc);
                char test = CheckAdditionalShipSpots(cruiserOrientation, cruiserLoc[2], cruiserLoc[2], shipLoc);
                if (test != '\0')
                {
                    SoundEffects('p');
                    cruiserLoc[1] = shipLoc;
                    num.BackColor = Color.Red;
                    num.Enabled = false;
                    cruiserSpots--;
                    cruiserOrientation = test;
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Ship Placement, please try another spot.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }
            }
            else if (cruiserSpots == 1)
            {
                Array.Sort(cruiserLoc);
                char test = CheckAdditionalShipSpots(cruiserOrientation, cruiserLoc[1], cruiserLoc[2], shipLoc);
                if (test != '\0')
                {
                    SoundEffects('p');
                    cruiserLoc[0] = shipLoc;
                    num.BackColor = Color.Red;
                    num.Enabled = false;
                    cruiserSpots--;
                    lblInstructions.Text = "[->] : Place your Destroyer on your battle board (2 spots, vertical or horizontal)";
                    s3 = new ship(3, cruiserLoc);
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Ship Placement, please try another spot.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                } 
            }
            else if (destroyerSpots == 2)
            {
                if (ValidateInitialShipPlacement(2, shipLoc, "destroyer") == true)
                {
                    SoundEffects('p');
                    destroyerLoc[0] = shipLoc;
                    num.BackColor = Color.Orange;
                    num.Enabled = false;
                    destroyerSpots--;
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Ship Placement, please try another spot.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }
            }
            else if (destroyerSpots == 1)
            {
                Array.Sort(destroyerLoc);
                char test = CheckAdditionalShipSpots(destroyerOrientation, destroyerLoc[1], destroyerLoc[1], shipLoc);
                if (test != '\0')
                {
                    SoundEffects('p');
                    destroyerLoc[0] = shipLoc;
                    num.BackColor = Color.Orange;
                    num.Enabled = false;
                    destroyerSpots--;
                    destroyerOrientation = test;
                    lblInstructions.Text = "[->] : All ship are set up! To begin game, click the Battle button!";
                    s2 = new ship(2, destroyerLoc);                    
                    gbxYourBoard.Enabled = false;
                    if (Ishost == false)
                        btnStartGame.Visible = true;
                }
                else
                {
                    SoundEffects('e');
                    string temp = lblInstructions.Text;
                    lblInstructions.Text = "[->] : Invalid Ship Placement, please try another spot.";
                    lblInstructions.Refresh();
                    Thread.Sleep(500);
                    lblInstructions.Text = temp;
                }
            }
        }

        public bool ValidateInitialShipPlacement(int shipSize, string initLoc, string shipName)// checks initial ship placement , vertical and horizontal
        {
            int availableSpotsH = 0;
            int availableSpotsV = 0;
            
            bool stopCount1 = false;
            bool stopCount2 = false;

            int x = Convert.ToInt32(initLoc.Substring(1, 1));
            String y = initLoc.Substring(0, 1);
            

            //horizontal check

            for (int i = 1; i < shipSize; i++)
            {                
                if (x + i < 11)
                {                   
                        if ((gbxYourBoard.Controls["btn" + y + (x + i)].Enabled == true) && (stopCount1 == false)) { availableSpotsH++; }
                        else { stopCount1 = true; }                    
                }
                if (x - i > 0)
                {                  
                        if ((gbxYourBoard.Controls["btn" + y + (x - i)].Enabled == true) && (stopCount2 == false)) { availableSpotsH++; }
                        else { stopCount2 = true; }                    
                }                
            }            

            //vertical check
            
            stopCount1 = false;
            stopCount2 = false;
            int YasInt = Convert.ToInt32(Convert.ToChar(y));

            for (int j=1; j < shipSize; j++) //ascii values A-J = 65 - 74
            {
                if (YasInt + j < 75) 
                {
                    if ((gbxYourBoard.Controls["btn" + Convert.ToChar(YasInt + j) + x].Enabled == true) && (stopCount1 == false)) { availableSpotsV++; }
                    else { stopCount1 = true; }
                }
                if (YasInt - j > 64)
                {
                    if ((gbxYourBoard.Controls["btn" + Convert.ToChar(YasInt - j) + x].Enabled == true) && (stopCount2 == false)) { availableSpotsV++; }
                    else { stopCount2 = true; }
                }
            }


            //if ((availableSpotsH > shipSize-2) || (availableSpotsV > shipSize - 2)){ return true; }
            //else { return false; }
            if ((availableSpotsH > shipSize - 2) && !(availableSpotsV > shipSize - 2))//if horizontal but not vertical, assign orientation
            {
                if (shipName == "carrier")
                    carrierOrientation = 'h';
                else if
                     (shipName == "battleship")
                    battleshipOrientation = 'h';
                else if (shipName == "cruiser")
                    cruiserOrientation = 'h';
                else if (shipName == "destroyer")
                    destroyerOrientation = 'h';

                return true;
            }
            else if (!(availableSpotsH > shipSize - 2) && (availableSpotsV > shipSize - 2))//if vertical but not horizontal, assign orientation
            {
                if (shipName == "carrier")
                    carrierOrientation = 'v';
                else if
                     (shipName == "battleship")
                    battleshipOrientation = 'v';
                else if (shipName == "cruiser")
                    cruiserOrientation = 'v';
                else if (shipName == "destroyer")
                    destroyerOrientation = 'v';

                return true;
            }
            else if ((availableSpotsH > shipSize - 2) && (availableSpotsV > shipSize - 2))//if both vertical and horizontal, dont assign orientation
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        public char CheckAdditionalShipSpots(char o, string shipLocationMin, string shipLocationMax, string checkSpot)//makes sure ship spots are horizontal or vertical and connected
        {
            Char Orientation = '\0';

            int x1 = Convert.ToInt32(shipLocationMin.Substring(1));
            int x2 = Convert.ToInt32(checkSpot.Substring(1));
            int x3 = Convert.ToInt32(shipLocationMax.Substring(1));
            String y1 = shipLocationMin.Substring(0, 1);
            String y2 = checkSpot.Substring(0, 1);
            String y3 = shipLocationMax.Substring(0, 1);
            int y1AsInt = Convert.ToInt32(Convert.ToChar(y1));
            int y2AsInt = Convert.ToInt32(Convert.ToChar(y2));
            int y3AsInt = Convert.ToInt32(Convert.ToChar(y3));

            if (x1 == 10) //array.sort puts 10 infront of 2-9 
            {
                int temp = x2; //vs2017 doesnt include system.ValueTuple , temp variable for swap 
                x2 = x1; x1 = temp; 
            }
                

            if (o == 'u') //check initial spot against new spot, declares ship orientation
            {
                if (y1AsInt == y2AsInt) // vertical
                {
                    if ((x2 == x1 + 1) || (x2 == x1 - 1)) // bug, check each spot length of ship
                    {
                        Orientation = 'h';
                    }
                }
                if (x1 == x2) // horizontal
                {
                    if ((y2AsInt == y1AsInt + 1) || (y2AsInt == y1AsInt - 1)) // bug, check each spot length of ship
                    {
                        Orientation = 'v';
                    }
                }
            }
            else
            {
                if ((o == 'h') && (y1AsInt == y2AsInt)&&(y2AsInt == y3AsInt))  // check new spot against lowest value spot and highest value horizontally
                {
                    if ((x2 == x1 - 1) || (x2 == x3 + 1)) //if the check spot is equal to the left most spot minus 1 or the right most spot plus 1
                    {
                        Orientation = 'h';
                    }
                    else
                    {
                        Orientation = '\0';
                    }
                }
                else if ((o == 'v')&& (x1 == x2)&& (x2 == x3))// check new spot against lowest spot and last highest spot vertically
                {
                    if ((y2AsInt == y1AsInt - 1) || (y2AsInt == y3AsInt + 1)) //if the check spot is equal to the highest spot minus 1 or the lowest spot plus 1
                    {
                        Orientation = 'v';
                    }
                    else
                    {
                        Orientation = '\0';
                    }

                }
                else { Orientation = '\0'; }
            }

            return Orientation;
        }

        private void btnStartGame_Click(object sender, EventArgs e) // start game thread
        {

            lblInstructions.Text = "";
            btnStartGame.Visible = false;
            lblInstructions.Refresh();
            IsInBattle = true;

            //send all ship locations to opponent 
            byte[] byteTime = Encoding.ASCII.GetBytes(s2.shipLocation[0] + "," + s2.shipLocation[1] + "," +s3.shipLocation[0]+"," + s3.shipLocation[1]+ "," + s3.shipLocation[2]+ "," + s4.shipLocation[0] + "," + s4.shipLocation[1] + "," + s4.shipLocation[2] + "," + s4.shipLocation[3] + "," + s5.shipLocation[0] + "," + s5.shipLocation[1] + "," + s5.shipLocation[2] + "," + s5.shipLocation[3] + "," + s5.shipLocation[4]);
            ns.Write(byteTime, 0, byteTime.Length);
            Thread.Sleep(200);
            ns.FlushAsync();

            lblInstructions.Text = "[->] : Game Starting . . . ";
            Thread.Sleep(200);
            
            if (IsGoingFirst == true) //host goes first 
            {
                gbxEnemyBoard.Enabled = true;
                lblInstructions.Text = "[->] : your turn";
                
            }
            else
            {
                gbxEnemyBoard.Enabled = false;
                lblInstructions.Text = "[->] : " + opponentName + "'s turn";
            }

            /*        File too big for Git, please convert the attached mp3 back to wav and un comment this code 
            axWMPsetup.Ctlcontrols.stop();
            axWMPbattle.URL = "battle_music_royalty_free.wav";//credit www.bensound.com/royalty-free-music/track/epic  
            axWMPbattle.Ctlcontrols.stop();
            if (cbxSound.Checked == true)
            {                
                axWMPbattle.Ctlcontrols.play();
            }
            */
        }


        public void SoundEffects(char effect)// plays sound effects based on character sent to function
        {
            
            if (cbxSound.Checked == true)
            {
                if (effect == 'p')
                {
                    sp = new System.Media.SoundPlayer(@"placeShip.wav"); //system doesnt like this wav :< 
                    sp.Play();
                }
                else if (effect == 'f') //fire
                {
                    sp = new System.Media.SoundPlayer(@"fire.wav"); //system doesnt like this wav :< 
                    sp.Play();
                    Thread.Sleep(850);
                }
                else if (effect == 'h') // hit
                {
                    sp = new System.Media.SoundPlayer(@"hit.wav"); //system doesnt like this wav :< 
                    sp.Play();
                }
                else if (effect == 'm') // miss
                {
                    sp = new System.Media.SoundPlayer(@"miss.wav"); //system doesnt like this wav :< 
                    sp.Play();
                }
                else if (effect == 's')//sink
                {
                    sp = new System.Media.SoundPlayer(@"sink.wav"); //system doesnt like this wav :< 
                    sp.Play();
                }
                else if (effect == 'v')//victory
                {
                    sp = new System.Media.SoundPlayer(@"victory.wav"); //system doesnt like this wav :< 
                    sp.Play();
                }
                else if (effect == 'd')//defeat
                {
                    sp = new System.Media.SoundPlayer(@"defeat.wav"); //system doesnt like this wav :< 
                    sp.Play();
                }
                else if (effect == 'e')
                {
                    sp = new System.Media.SoundPlayer(@"error.wav"); //system doesnt like this wav :< 
                    sp.Play();
                } 
            }
        }

        private void TakeTurn(object sender, EventArgs e) //when you click on opponents battle board
        {            
            if (txtbMoves.Text == "InitializeEnemyShips") { txtbMoves.Clear(); }

            Button num = (Button)sender;
            String EnemyshipLoc = num.Name.Substring(3); //remove btn from button name 
            EnemyshipLoc = EnemyshipLoc.Remove(EnemyshipLoc.Length-1); // remove the E at the end of button name

            byte[] byteTime = Encoding.ASCII.GetBytes(EnemyshipLoc);
            ns.Write(byteTime, 0, byteTime.Length);


            SoundEffects('f');

            txtbMoves.Clear();
            gbxEnemyBoard.Enabled = false;
            lblInstructions.Text = "[->] : " + opponentName + "'s turn";
            lblInstructions.Refresh();

            //check enemy ships for damage
            if (s2E.IsHit(EnemyshipLoc) == true)
            {
                num.Enabled = true;
                num.BackColor = Color.DarkRed;
                num.Text = "X";
                num.Enabled = false;

                if (s2E.IsSunk == true)// was it sunk?
                {
                    txtbBattleLog.AppendText("[log] : You Fired at " + EnemyshipLoc + " and Sunk a Destroyer!" + Environment.NewLine);
                    SoundEffects('s');
                    pboxEnemy2.Image = Properties.Resources.destroyerX;
                    pboxEnemy2.Refresh();
                }
                else// just a hit
                {
                    txtbBattleLog.AppendText("[log] : You Fired at " + EnemyshipLoc + " and Hit!" + Environment.NewLine);
                    SoundEffects('h');
                }
            }
            else if (s3E.IsHit(EnemyshipLoc) == true)
            {
                num.Enabled = true;
                num.BackColor = Color.DarkRed;
                num.Text = "X";
                num.Enabled = false;

                if (s3E.IsSunk == true)// was it sunk?
                {
                    txtbBattleLog.AppendText("[log] : You Fired at " + EnemyshipLoc + " and Sunk a Cruiser!" + Environment.NewLine);
                    SoundEffects('s');
                    SoundEffects('s');
                    pboxEnemy3.Image = Properties.Resources.cruiserX;
                    pboxEnemy3.Refresh();
                }
                else// just a hit
                {
                    txtbBattleLog.AppendText("[log] : You Fired at " + EnemyshipLoc + " and Hit!" + Environment.NewLine);
                    SoundEffects('h');
                }
            }
            else if (s4E.IsHit(EnemyshipLoc) == true)
            {
                num.Enabled = true;
                num.BackColor = Color.DarkRed;
                num.Text = "X";
                num.Enabled = false;

                if (s4E.IsSunk == true)// was it sunk?
                {
                    txtbBattleLog.AppendText("[log] : You Fired at " + EnemyshipLoc + " and Sunk a BattleShip!" + Environment.NewLine);                    
                    SoundEffects('s');
                    pboxEnemy4.Image = Properties.Resources.battleshipX1;
                    pboxEnemy4.Refresh();
                }
                else// just a hit
                {
                    txtbBattleLog.AppendText("[log] : You Fired at " + EnemyshipLoc + " and Hit!" + Environment.NewLine);
                    SoundEffects('h');
                }
            }
            else if (s5E.IsHit(EnemyshipLoc) == true)
            {
                num.Enabled = true;
                num.BackColor = Color.DarkRed;
                num.Text = "X";
                num.Enabled = false;

                if (s5E.IsSunk == true)// was it sunk?
                {
                    txtbBattleLog.AppendText("[log] : You Fired at " + EnemyshipLoc + " and Sunk a Carrier!" + Environment.NewLine);                   
                    SoundEffects('s');
                    pboxEnemy5.Image = Properties.Resources.carrierX;
                    pboxEnemy5.Refresh();
                }
                else// just a hit
                {
                    txtbBattleLog.AppendText("[log] : You Fired at " + EnemyshipLoc + " and Hit!" + Environment.NewLine);
                    SoundEffects('h');
                }
            }
            else // miss
            {
                txtbBattleLog.AppendText("[log] : You Fired at " + EnemyshipLoc + " and Missed!" + Environment.NewLine);
                num.Enabled = true;
                num.BackColor = Color.LightBlue;
                num.Text = "O";
                num.Enabled = false;
                SoundEffects('m');
            }

            //check for win condition 
            if (s2E.IsSunk == true && s3E.IsSunk == true && s4E.IsSunk == true && s5E.IsSunk == true)
            {
                txtbBattleLog.AppendText("[log] : You Defeated your opponent! Victory is yours!" + Environment.NewLine);
                SoundEffects('s');
                Thread.Sleep(2500);                
                SoundEffects('v');
                MessageBox.Show("Victory is Yours!");
                yourScore++;
                lblPlayer1Score.Text = Convert.ToString(yourScore);
                lblInstructions.Text = "[->] : Place your Carrier on your battle board (5 spots, vertical or horizontal)";
                byteTime = Encoding.ASCII.GetBytes("reset");
                ns.Write(byteTime, 0, byteTime.Length);
                ResetGame();
            }            
        }

        private void ResetGame()
        {
            //reset variables
            carrierSpots = 5;
            battleshipSpots = 4;
            cruiserSpots = 3;
            destroyerSpots = 2;
            carrierLoc = new string[5];
            battleshipLoc = new string[4];
            cruiserLoc = new string[3];
            destroyerLoc = new string[2];
            carrierOrientation = 'u';
            battleshipOrientation = 'u';
            cruiserOrientation = 'u';
            destroyerOrientation = 'u';            
            EnemyShips = new string[14];                        

            s2 = new ship(2, destroyerLoc);
            s3 = new ship(3, cruiserLoc);
            s4 = new ship(4, battleshipLoc);
            s5 = new ship(5, carrierLoc);

            s2E = new ship(2, destroyerLoc);
            s3E = new ship(3, cruiserLoc);
            s4E = new ship(4, battleshipLoc);
            s5E = new ship(5, carrierLoc);

            //reset buttons
            gbxEnemyBoard.Enabled = true;
            gbxYourBoard.Enabled = true;
            foreach (var button in gbxYourBoard.Controls.OfType<Button>())
            {
                button.Enabled = true;
                button.BackColor = Color.Transparent;                
                button.Text = "";
            }
            foreach (var button in gbxEnemyBoard.Controls.OfType<Button>())
            {
                button.Enabled = true;
                button.BackColor = Color.Transparent;
                button.Text = "";
            }
            gbxEnemyBoard.Enabled = false;

            //reset controls 
            txtbBattleLog.Clear();
            IsInBattle = false;

            pboxEnemy2.Image = Properties.Resources.destroyer;
            pboxEnemy3.Image = Properties.Resources.cruiser;
            pboxEnemy4.Image = Properties.Resources.battleship;
            pboxEnemy5.Image = Properties.Resources.carrier;
            pboxMy2.Image = Properties.Resources.destroyer;
            pboxMy3.Image = Properties.Resources.cruiser;
            pboxMy4.Image = Properties.Resources.battleship;
            pboxMy5.Image = Properties.Resources.carrier;
            
            axWMPbattle.Ctlcontrols.stop();

            if (cbxSound.Checked == true)
                axWMPsetup.Ctlcontrols.play();

            txtbMoves.Text = "InitializeEnemyShips";
            this.Refresh();

            //swap whos taking the first turn every game 
            if (IsGoingFirst == true)            
                IsGoingFirst = false;            
            else            
                IsGoingFirst = true;            
        }

    }

    public class ship
    {
        public int shipSize { get; set; }
        public string[] shipLocation;
        public bool IsSunk = false;
        public int ShipHealth;

        public ship(int size,string[] loc) // initialize ship locations, size and health
        {
            shipSize = size;
            ShipHealth = size;

            shipLocation = new string[shipSize];           
            shipLocation = loc;            
        }

        public bool IsHit(String spotFiredAt) // checks if shot hit the ship
        {
            bool hit = false;

            for (int i = 0; i < shipSize; i++)
            {
                if (spotFiredAt == shipLocation[i])
                {
                    hit = true;
                    ShipHealth--;

                    if (ShipHealth == 0)
                        IsSunk = true;
                }                
            }
            return hit;              
        }
    }
}
