using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NewPlayer
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Player and Server 
        /// </summary>
        ServerController Controller = new ServerController("http://96.126.117.25/Music");
        Player player = new Player("http://96.126.117.25/Music");
        /// <summary>
        /// Move form
        /// </summary>
        private bool mouseDown;
        private Point lastLocation;
        
        /*
            Error Messages
         */
        /// <summary>
        /// Format all Error Messages to look alike
        /// </summary>
        /// <param name="ErrorCode">Error Code, can be looked up at end of Form1.cs</param>
        /// <param name="LineNumber">LineNumber of where the error occured</param>
        public void ErrorMessage(Exception a, string ErrorCode, [CallerLineNumber] int LineNumber = 0)
        {
            try
            {
                MessageBox.Show(a.Message + "\n" +
                                "Error Code: " + ErrorCode + "\n" +
                                "Line: " + LineNumber +
                                a.ToString());
                player.Next();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        /// <summary>
        /// Initialize important elements before form oad
        /// </summary>
        public Form1()
        {
            player.Changed += ChangedEventHandler;
            InitializeComponent();
            initialize();
        }
        /// <summary>
        /// Initialize imporant elements before form load
        /// </summary>
        private void initialize() {
            try
            {
                // Get Playlists From Web Server
                foreach (string Playlist in Controller.GetAllPlaylists())
                {
                    PlaylistDropDown.Items.Add(Playlist);
                }

                // Set First Playlist
                PlaylistDropDown.SelectedIndex = 0;
                // Start lLog

            }
            catch (Exception err)
            {
                ErrorMessage(err, "1");
            }
        }
        #region Move Form
        /// <summary>
        /// When user presses mouse down
        /// </summary>
        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                mouseDown = true;// Mouse is pressed down: start moving form
                lastLocation = e.Location; // get location of mouse
            }
            catch (Exception a)
            {
                ErrorMessage(a, "480 \nCould Not Move Window");
            }
        }
        /// <summary>
        /// When user mouse is up
        /// </summary>
        private void Main_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                mouseDown = false;// Mouse is no longer pressed down: stop moving form
            }
            catch (Exception a)
            {
                ErrorMessage(a, "480 \nCould Not Move Window");
            }
        }
        /// <summary>
        /// When user moves mouse
        /// </summary>
        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (mouseDown) // Mouse is pressed down: when mouse starts moving
                {

                    this.Location = new Point(
                        (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);// set new location of mouse

                    this.Update();// Moves form locatio to mouse location
                }
            }
            catch (Exception a)
            {
                ErrorMessage(a, "480 \nCould Not Move Window");
            }
        }
        #endregion
        #region FormButtons
        /// <summary>
        /// Special update information
        /// </summary>
        private void btnInfo_Click(object sender, EventArgs e)
        {
            Information info = new Information();
            info.Show();
        }
        /// <summary>
        /// Close the form
        /// </summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// Play previous song in playlist
        /// </summary>
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            try
            {
                player.Previous();
                updateUserInterface();
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
            }
        }
        /// <summary>
        /// If playing     -> pause
        /// If paused      -> play
        /// </summary>
        private void btnPlayPause_Click(object sender, EventArgs e)
        {
            try
            {
                if (player.isPlaying())
                {
                    var bmp = (Bitmap)Properties.Resources.ResourceManager.GetObject("play");
                    btnPlayPause.Image = bmp;
                }
                else {
                    var bmp = (Bitmap)Properties.Resources.ResourceManager.GetObject("pause");
                    btnPlayPause.Image = bmp;
                }
                player.PlayPause();
                updateUserInterface();
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
            }
        }
        /// <summary>
        /// Play next song in playlist
        /// </summary>
        private void btnNext_Click(object sender, EventArgs e)
        {
            try
            {

                player.Next();
                updateUserInterface();
                
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
            }
        }
        /// <summary>
        /// New Playlist Selected, Get Songs from Selected Playlist
        /// </summary>
        private void PlaylistDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                player.ClearPlaylists();
                // Gets Songs From location and passes to player

                player.Initialize(PlaylistDropDown.SelectedItem.ToString());
                updateUserInterface();

            }
            catch (Exception err)
            {
                ErrorMessage(err, "1");
            }
        }
        #endregion
        #region HelperFucntions
        /// <summary>
        /// Update User interface with content from given index
        /// </summary>
        /// <param name="index">index location of Shuffled Playlist</param>
        private void updateUserInterface()
        {
            try
            {
                txtTitle.Text = player.getMediaTitle();
                toolTip1.SetToolTip(txtTitle, player.getMediaTitle());
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            catch (Exception err)
            {
                ErrorMessage(err, "1");
            }
        }
        /// <summary>
        /// On Song Change
        /// 
        /// Update Current playling Title
        /// </summary>
        public void ChangedEventHandler(object sender, EventArgs e)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate {
                    txtTitle.Text = player.getMediaTitle();
                    toolTip1.SetToolTip(txtTitle, player.getMediaTitle());
                });
            }
            catch (Exception err)
            {
                ErrorMessage(err, "1");
            }
        }
        #endregion

    }
}
