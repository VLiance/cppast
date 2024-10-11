using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace CwideContext
{
    [Serializable]
    public class Settings
    {
        private Int32 sampleNumber = 69;
        private String sampleText = "This is the CWide plugin.";
        private Keys sampleShortcut = Keys.Control | Keys.F1;
        
        /// <summary> 
        /// Get and sets the sampleText
        /// </summary>
        [Description("A CWide string setting."), DefaultValue("This is a Linx plugin.")]
        public String SampleText 
        {
            get { return this.sampleText; }
            set { this.sampleText = value; }
        }

        /// <summary> 
        /// Get and sets the sampleNumber
        /// </summary>
        [Description("A CWide integer setting."), DefaultValue(69)]
        public Int32 SampleNumber 
        {
            get { return this.sampleNumber; }
            set { this.sampleNumber = value; }
        }

        /// <summary> 
        /// Get and sets the sampleShortcut
        /// </summary>
        [Description("A sample shortcut setting."), DefaultValue(Keys.Control | Keys.F1)]
        public Keys SampleShortcut
        {
            get { return this.sampleShortcut; }
            set { this.sampleShortcut = value; }
        }

    }

}
