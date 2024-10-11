using System;
using System.Text;
using System.Collections.Generic;
using PluginCore;
using System.IO;

namespace CwideContext
{
    public class CwLib
    {
       
		public enum Type {Rc, Lib};

		public Type eType = Type.Lib;

		public string sName;
        public string sIdName;
        public bool bReadOnly;
        public string sReadPath;
        public string sWriteName;
        public string sFullWritePath;
        public string sWritePath;
        public string sEmbedRc;


		public CwLibGroup oGroup;
    }
}
