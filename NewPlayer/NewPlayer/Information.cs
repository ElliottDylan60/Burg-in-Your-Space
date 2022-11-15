using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewPlayer
{
    public partial class Information : Form
    {
        /*
            Update Information
         */
        private Uri xmlLocation;

        public string ApplicationName
        {
            get { return "881TheBurg"; }
        }

        public string ApplicationId
        {
            get { return "881TheBurg"; }
        }

        public Assembly ApplicationAssembly
        {
            get { return Assembly.GetEntryAssembly(); }
        }

        public Icon ApplicationIcon
        {
            get { return this.Icon; }
        }

        public Uri UpdateXmlLocaion
        {
            get { return new Uri("http://69.164.202.213/update/update.xml"); }
        }

        public Form Context
        {
            get { return this; }
        }
        /*
            Form Moving Variables
         */
        private bool mouseDown;
        private Point lastLocation;
        public Information()
        {
            InitializeComponent();
            this.lbVersion.Text = "Version " + this.ApplicationAssembly.GetName().Version.ToString();
            this.xmlLocation = new Uri("http://69.164.202.213/update/update.xml");
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
        private void Information_Load(object sender, EventArgs e)
        {

        }
        /*
            Drag and Drop
         */
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
        /*
            Buttons
         */

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_Update_Click(object sender, EventArgs e)
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    //MessageBox.Show("Admin Privileges Required to Update");
                    //return;
                }
            }
        }
    }
}
