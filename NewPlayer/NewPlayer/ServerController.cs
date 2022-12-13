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
        string ServerAddress; // http://96.126.117.25/Music
        string currentPlaylist; // Long Playlist
        string sweepersFolder; // Sweepers
        public ServerController(string ServerAddress) {
            this.ServerAddress = ServerAddress;
        }
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
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        #region playlistInformation
        /// <summary>
        /// get playlists in current server
        /// </summary>
        public List<string> GetAllPlaylists() {
            try
            {
                List<string> AllPlaylists = new List<string>();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServerAddress);

                var cache = new CredentialCache();
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
                return AllPlaylists;
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
                return null;
            }
        }
        
        /// <summary>
        /// get files in specific directory
        /// </summary>
        public List<SongProperties> GetPlaylist(string directory) {
            try
            {
                List<SongProperties> playlist = new List<SongProperties>();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServerAddress + "/" + directory);

                var cache = new CredentialCache();
                request.Credentials = cache;
                request.PreAuthenticate = true;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();

                        Regex regex = new Regex(GetDirectoryListingRegexForUrl(ServerAddress + "/" + directory));
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

                                        playlist.Add(new SongProperties { 
                                            Title = System.Net.WebUtility.HtmlDecode(match.Groups["name"].ToString()), 
                                            index = index++, 
                                            URL = ServerAddress + "/" + directory + "/" + System.Net.WebUtility.HtmlDecode(match.Groups["name"].ToString()) });
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
                return playlist;
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
                return null;
            }
        }

        /// <summary>
        /// Get sweepers in current playlist
        /// </summary>
        public string GetSubDirectory(string directory) {
            try
            {
                List<string> Sweepers = new List<string>();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServerAddress + "/" + directory);

                var cache = new CredentialCache();
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

                                    if (match.Groups["name"].ToString().EndsWith("/"))
                                    {
                                        Sweepers.Add(System.Net.WebUtility.HtmlDecode(match.Groups["name"].ToString().TrimEnd('/')));
                                    }

                                }

                            }
                        }
                    }
                }

                if (Sweepers.Count > 0) {
                    return Sweepers[0];
                }
                return "Does Not Exist";
            }
            catch (Exception err) {
                ErrorMessage(err, "1");
                return "Does Not Exist";
            }
        }
        #endregion
        /// <summary>
        /// reads all files in given directory
        /// </summary>
        /// <param name="url">location of playlist</param>
        /// <returns>returns name of file/song</returns>
        private string GetDirectoryListingRegexForUrl(string url)
        {
            /*
                I didn't write this code, i got it from here:
                https://social.msdn.microsoft.com/Forums/vstudio/en-US/a7432290-c643-4d84-84df-61ce4c63a563/get-files-list-from-url?forum=csharpgeneral
            */
            try
            {

                return "<a href=\".*\">(?<name>.*)</a>";

            }
            catch (NotSupportedException e)
            {
                return "Could Not Connect To External Server";
            }
        }
    }
}
