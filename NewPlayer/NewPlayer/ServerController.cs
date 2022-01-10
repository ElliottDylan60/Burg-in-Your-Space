using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.CompilerServices;
using NAudio.Wave;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;
using System.Net.Mail;

namespace NewPlayer
{
    class ServerController
    {
        string ServerAddress;
        List<string> AllPlaylists = new List<string>();
        List<string> Sweepers = new List<string>();
        string CurrentPlaylist;
        List<SongProperties> playlist = new List<SongProperties>();

        public ServerController(string ServerAddress) {
            this.ServerAddress = ServerAddress;
        }
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
                
                SendEmail(a.Message + "\n" +
                                "Error Code: " + ErrorCode + "\n" +
                                "Line: " + LineNumber +
                                a.ToString());

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
        /// <summary>
        /// Return list of all playlists in server address
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllPlaylists() {
            try
            {
                BackgroundWorker FindPlaylists = new BackgroundWorker(); // Create Background Worker
                FindPlaylists.DoWork += new DoWorkEventHandler(this.GetAllPlaylists_DoWork); // Assign Do Work event
                FindPlaylists.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.GetAllPlaylists_RunWorkerComplete);
                FindPlaylists.RunWorkerAsync();
                while (FindPlaylists.IsBusy)// wait for background worker to stop
                {
                    Application.DoEvents();
                }
                return AllPlaylists;
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
                return null;
            }
        }
        private void GetAllPlaylists_DoWork(object sender, DoWorkEventArgs e) {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServerAddress);
                
                var cache = new CredentialCache();
                cache.Add(new Uri(ServerAddress), "Basic", new NetworkCredential("admin", "9Automation9"));
                request.Credentials = cache;
                request.PreAuthenticate = true;
                
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();
                        Regex regex = new Regex(GetDirectoryListingRegexForUrl(ServerAddress));
                        MatchCollection matches = regex.Matches(html);
                        if (matches.Count > 0)
                        {
                            foreach (Match match in matches)
                            {
                                if (match.Success)
                                {
                                    AllPlaylists.Add(match.Groups["name"].ToString().TrimEnd('/'));
                                }
                            }
                        }
                        else
                        {
                            // No Files in Directory
                        }
                        if (AllPlaylists[0].Equals("Description"))
                        {
                            AllPlaylists.RemoveAt(1); // Remove Parent Directory
                            AllPlaylists.RemoveAt(0); // Remove Description
                        }
                        else
                        {
                            AllPlaylists.RemoveAt(0); // Remove Parent Directory
                        }
                    }
                }
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
            }
        }
        private void GetAllPlaylists_RunWorkerComplete(object sender, RunWorkerCompletedEventArgs e) { 
        
        }
        /// <summary>
        /// Returns List of All songs in given Playlist
        /// </summary>
        /// <returns></returns>
        public List<SongProperties> GetPlaylist(string playlist) {
            try
            {
                this.CurrentPlaylist = playlist;
                BackgroundWorker GetPlaylist = new BackgroundWorker();
                GetPlaylist.DoWork += new DoWorkEventHandler(BackgroundGetPlaylist_DoWork);
                GetPlaylist.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundGetPlaylist_RunWorkerComplete);
                GetPlaylist.RunWorkerAsync();
                while (GetPlaylist.IsBusy)
                {
                    Application.DoEvents();
                }
                return this.playlist;
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
                return null;
            }
        }

        private void BackgroundGetPlaylist_DoWork(object sender, DoWorkEventArgs e) {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServerAddress + "/" + CurrentPlaylist);
                
                var cache = new CredentialCache();
                cache.Add(new Uri(ServerAddress), "Basic", new NetworkCredential("admin", "9Automation9"));
                request.Credentials = cache;
                request.PreAuthenticate = true;
                
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();

                        Regex regex = new Regex(GetDirectoryListingRegexForUrl(ServerAddress + "/" + CurrentPlaylist));
                        MatchCollection matches = regex.Matches(html);
                        if (matches.Count > 0)
                        {
                            int index = 0;
                            foreach (Match match in matches)
                            {
                                if (match.Success)
                                {
                                    // If match is not directory
                                    if (!match.Groups["name"].ToString().EndsWith("/"))
                                    {

                                        playlist.Add(new SongProperties { Title = System.Net.WebUtility.HtmlDecode(match.Groups["name"].ToString()), index = index++, URL = ServerAddress + "/" + CurrentPlaylist + "/" + System.Net.WebUtility.HtmlDecode(match.Groups["name"].ToString()) });
                                    }

                                }
                            }
                        }
                        else
                        {
                            // no songs in given playlist
                        }

                        // Remove items from list
                        if (playlist[0].Title.Contains("Description"))
                        {
                            playlist.RemoveAt(0);
                            playlist.RemoveAt(0);
                        }
                        else
                        {
                            playlist.RemoveAt(0);
                        }
                    }
                }
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
            }
        }
        private void BackgroundGetPlaylist_RunWorkerComplete(object sender, RunWorkerCompletedEventArgs e) { 
        }

        public List<string> GetSweepersFolder() {
            try
            {
                BackgroundWorker FindSweepers = new BackgroundWorker(); // Create Background Worker
                FindSweepers.DoWork += new DoWorkEventHandler(this.GetSweepers_DoWork); // Assign Do Work event
                FindSweepers.RunWorkerAsync();
                while (FindSweepers.IsBusy)// wait for background worker to stop
                {
                    Application.DoEvents();
                }
                return Sweepers;
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
                return null;
            }
        }
        private void GetSweepers_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServerAddress);
                
                var cache = new CredentialCache();
                cache.Add(new Uri(ServerAddress), "Basic", new NetworkCredential("admin", "9Automation9"));
                request.Credentials = cache;
                request.PreAuthenticate = true;
                
                Console.WriteLine(ServerAddress);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {

                        string html = reader.ReadToEnd();
                        Regex regex = new Regex(GetDirectoryListingRegexForUrl(ServerAddress));
                        MatchCollection matches = regex.Matches(html);

                        if (matches.Count > 0)
                        {
                            foreach (Match match in matches)
                            {
                                if (match.Success)
                                {

                                    if (match.Groups["name"].ToString().EndsWith("/"))
                                    {
                                        Sweepers.Add(System.Net.WebUtility.HtmlDecode(match.Groups["name"].ToString().TrimEnd('/')));
                                    }

                                }

                            }
                        }

                    }
                }
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
            }
        }
        /// <summary>
        /// reads all files in given directory
        /// </summary>
        /// <param name="url">location of playlist</param>
        /// <returns>returns name of file/song</returns>
        private string GetDirectoryListingRegexForUrl(string url)
        {
            try
            {
                if (url.Equals(url))
                {
                    return "<a href=\".*\">(?<name>.*)</a>";
                }
                return "Could Not Connect To External Server";
            }
            catch (NotSupportedException e)
            {
                return "Could Not Connect To External Server";
            }
        }
        /*
            Send Email Notification
         */
        private void SendEmail(string Error)
        {
            
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential("fatalerrorservernotification@gmail.com", "Elliott050703*"),
                    EnableSsl = true,
                };
                smtpClient.Send("fatalerrorservernotification@gmail.com", "elliottdy@cwu.edu", "Player Died", Error);
            }
            catch (Exception err) {
                MessageBox.Show(err.ToString());
            }
            
        }
    }
}
