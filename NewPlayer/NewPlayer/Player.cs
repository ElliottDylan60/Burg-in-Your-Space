using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using NAudio.Wave;

namespace NewPlayer
{
    class Player
    {
        /*
            Audio Player Variables
         */
        private List<SongProperties> Playlist = new List<SongProperties>();
        private List<SongProperties> ShuffledPlaylist = new List<SongProperties>();
        private List<SongProperties> Sweepers = new List<SongProperties>();

        private MediaFoundationReader mf;
        private WaveOutEvent wo;
        private int CurrentIndex = 0;
        /*
            Server Variables
         */
        private string ServerLocation;
        ServerController Controller;
        /*
            Events
        */
        public event EventHandler Changed;

        /*
            Error Messages
         */
        /// <summary>
        /// Format all Error Messages to look alike
        /// </summary>
        /// <param name="ErrorCode">Error Code, can be looked up at end of Form1.cs</param>
        /// <param name="LineNumber">LineNumber of where the error occured</param>
        private void ErrorMessage(Exception a, string ErrorCode, [CallerLineNumber] int LineNumber = 0)
        {
            try
            {
                

                MessageBox.Show(a.Message + "\n" +
                                "Error Code: " + ErrorCode + "\n" +
                                "Line: " + LineNumber +
                                a.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public Player(string ServerLocation) {
            try
            {
                Controller = new ServerController(ServerLocation);
                this.ServerLocation = ServerLocation;
                wo = new WaveOutEvent();
                wo.PlaybackStopped += new EventHandler<StoppedEventArgs>(StoppedEventHandler);
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
            }

        }
        public void Initialize(string PlaylistName) {
            setSweepers(PlaylistName);
            setPlaylist(Controller.GetPlaylist(PlaylistName));
            
        }
        private void StoppedEventHandler(object sender, EventArgs e)
        {
            Next();
        }
        #region playerControls
        /// <summary>
        /// If playing     -> pause
        /// If paused      -> play
        /// </summary>
        public void PlayPause() {
            try
            {
                mf = new MediaFoundationReader(ShuffledPlaylist[CurrentIndex].URL);
                if (wo.PlaybackState == PlaybackState.Paused)
                {
                    wo.Play();
                }
                else if (wo.PlaybackState == PlaybackState.Playing)
                {
                    wo.Pause();
                }
                else
                {
                    wo.Init(mf);
                    wo.Play();
                }
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
            }
        }
        /// <summary>
        /// Play next song
        /// </summary>
        public void Next() {
            
            try
            {
                if (!isStopped())
                {
                    wo.Stop();
                    return;
                }
                
                if ((CurrentIndex + 1) >= ShuffledPlaylist.Count) {
                    CurrentIndex = -1;
                    RefreshPlaylist();
                }
                
                mf = new MediaFoundationReader(ShuffledPlaylist[++CurrentIndex].URL);
                wo.Init(mf);
                wo.Play();
                Changed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception) {
                Next();
            }
        }
        /// <summary>
        /// Play previous song
        /// </summary>
        public void Previous() {
            try
            {
                
                wo.Stop();
                if ((CurrentIndex - 1) < 0) 
                    CurrentIndex = ShuffledPlaylist.Count() - 1;
                mf = new MediaFoundationReader(ShuffledPlaylist[--CurrentIndex].URL);
                wo.Init(mf);
                wo.Play();
                
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
            }
        }
        #endregion
        #region Playlistcontrols
        /// <summary>
        /// Shuffles playlist and initializes first media
        /// </summary>
        /// <param name="Playlist"></param>
        private void setPlaylist(List<SongProperties> Playlist)
        {
            try
            {
                // Keep Current Playlist
                
                wo.Stop();
                this.Playlist = Playlist;

                // Refresh Playlist
                RefreshPlaylist();
                Form1.ShuffledPlaylist.Clear();
                Form1.ShuffledPlaylist = ShuffledPlaylist.ToList();

                // Play First Index
                mf = new MediaFoundationReader(ShuffledPlaylist[CurrentIndex].URL);
                wo.Init(mf);
                wo.Play();
                
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Playlist is Empty, please select a new one");
            }
            catch (Exception err)
            {
                ErrorMessage(err, "1");
            }
        }
        /// <summary>
        /// Adds sweepers to playlist
        /// </summary>
        /// <param name="PlaylistLocation">playlist location</param>
        private void setSweepers(string PlaylistLocation)
        {
            try
            {
                string SweepersFolder = Controller.GetSubDirectory(PlaylistLocation);
                if (!SweepersFolder.Equals("Does Not Exist"))
                {
                    Sweepers = Controller.GetPlaylist(PlaylistLocation + "/" + SweepersFolder);
                }
                else
                {
                    Sweepers = new List<SongProperties>();
                }
            }
            catch (Exception err)
            {
                ErrorMessage(err, "1");
            }

        }
        /// <summary>
        /// Reset and Shuffle new Playlist
        /// </summary>
        private void RefreshPlaylist() {
            try
            {
                List<SongProperties> temp = Playlist.ToList();
                ShuffledPlaylist.Clear();

                int n = temp.Count;
                Random rand = new Random();
                while (n > 0)
                {
                    n--;
                    int k = rand.Next(n + 1);
                    ShuffledPlaylist.Add(temp[k]);
                    temp.RemoveAt(k);
                }
                AddSweepers();
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
            }
        }
        /// <summary>
        /// Add sweepers to shuffled playlist
        /// </summary>
        private void AddSweepers() {
            try
            {
                int sourceindex = 0;
                int insertIndex = 0;
                int totalInsert = Sweepers.Count;
                int step = 4;
                if (totalInsert < 1) {
                    return;
                }
                List<SongProperties> temp = ShuffledPlaylist.ToList(); // We need a temp because you cannot modify list when using it in foreach statement
                SongProperties insertSong;
                foreach (SongProperties song in ShuffledPlaylist)
                {
                    // Every 'step' song
                    if (sourceindex % step == 0)
                    {
                        // Loop back to begining
                        if (insertIndex == totalInsert)
                            insertIndex = 0;
                        // insert song at index
                        insertSong = new SongProperties { Title = Sweepers[insertIndex].Title, URL = Sweepers[insertIndex++].URL };
                        temp.Insert(sourceindex++, insertSong);
                    }
                    sourceindex++;
                }
                // Clear and add playlist
                ShuffledPlaylist.Clear();
                foreach (SongProperties song in temp)
                {
                    ShuffledPlaylist.Add(song);
                }
            }
           
            catch (Exception err) {
                ErrorMessage(err, "1");
            }
        }

        /// <summary>
        /// Clear all lists as new ones have been selected
        /// </summary>
        public void ClearPlaylists() {
            try
            {
                CurrentIndex = 0;
                this.ShuffledPlaylist.Clear();
                this.Playlist.Clear();
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
            }
        }
        #endregion
        #region Getters
        public int getCurrentIndex() {
            return this.CurrentIndex;
        }
        #endregion
        #region checkers
        public bool isPlaying()
        {
            if(wo.PlaybackState == PlaybackState.Playing)
                return true;
            return false;
        }
        public bool isPaused()
        {
            if (wo.PlaybackState == PlaybackState.Paused)
                return true;
            return false;
        }
        public bool isStopped()
        {
            if (wo.PlaybackState == PlaybackState.Stopped)
                return true;
            return false;
        }
        #endregion
    }
}