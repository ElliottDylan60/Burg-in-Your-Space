using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using _881TheBurg;

namespace SharpUpdate
{
    public class SharpUpdater
    {
        private ISharpUpdatable applicationInfo;
        private BackgroundWorker bgWorker;

        public SharpUpdater(ISharpUpdatable applicationInfo) {
            this.applicationInfo = applicationInfo;

            this.bgWorker = new BackgroundWorker();
            this.bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            this.bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BgWorker_RunWorkerCompleted);
        }

        public void DoUpdate() {
            
            if (!this.bgWorker.IsBusy)
                this.bgWorker.RunWorkerAsync(this.applicationInfo);
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e) {
           
            ISharpUpdatable application = (ISharpUpdatable)e.Argument;
            if (!SharpUpdateXML.ExistsOnServer(application.UpdateXmlLocaion))
            {
                Console.WriteLine("XML does not exist");
                e.Cancel = true;
            }
            else {

                Console.WriteLine("XML exists " + application.UpdateXmlLocaion.ToString());
                e.Result = SharpUpdateXML.Parse(application.UpdateXmlLocaion, application.ApplicationId);
            }
            
        }
        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled) {
                
                SharpUpdateXML update = (SharpUpdateXML)e.Result;
                Console.WriteLine("File Exists, Will Not Cancel");
                if (update != null)
                {
                    if (update.IsNewerThan(this.applicationInfo.ApplicationAssembly.GetName().Version))
                    {
                        if (new SharpUpdateAcceptForm(this.applicationInfo, update).ShowDialog(this.applicationInfo.Context) == DialogResult.Yes)
                        {
                            this.DownloadUpdate(update);
                        }
                    }
                    else {
                        Console.WriteLine("No New Update");
                    }
                }
                else {
                    Console.WriteLine("Update is null");
                }
            }
        }

        private void DownloadUpdate(SharpUpdateXML update)
        {
            Console.WriteLine("Downloading Update....");
            SharpUpdateDownloadForm form = new SharpUpdateDownloadForm(update.Uri, update.MD5, this.applicationInfo.ApplicationIcon);
            Console.WriteLine("Download Form opened");
            DialogResult result = form.ShowDialog(this.applicationInfo.Context);
            if (result == DialogResult.OK)
            {
                Console.WriteLine("DialogResult is OK");
                string currentPath = this.applicationInfo.ApplicationAssembly.Location;
                string newPath = Path.GetDirectoryName(currentPath) + "\\" + update.FileName;

                UpdateApplication(form.TempFilePath, currentPath, newPath, update.LaunchArgs);
                Console.WriteLine("Closing Application");
                Application.Exit();
            }
            else if (result == DialogResult.Abort)
            {
                MessageBox.Show("The update download was cenceled. \nThis program has not been modified.", "Update Download Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else {
                MessageBox.Show("There was a problem downloading the update. \nPlease try again later", "Update Download Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
        }

        private void UpdateApplication(string tempFilePath, string currentPath, string newPath, string launchArgs)
        {
            Console.WriteLine("Deleting Previous File & Downloading New File");
            string argument = "/C Choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\" & Choice /C Y /N /D Y /T 2 & Move /Y \"{1}\" \"{2}\" & Start \"\" /D \"{3}\" \"{4}\" {5}";
            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = string.Format(argument, currentPath, tempFilePath, newPath, Path.GetDirectoryName(newPath), Path.GetFileName(newPath), launchArgs);
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.CreateNoWindow = true;
            info.FileName = "cmd.exe";
            Process.Start(info);
        }
    }
}
