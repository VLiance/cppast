using System;
using System.Text;
using System.Collections.Generic;
using PluginCore;
using System.IO;

namespace CwideContext
{
    public class CwLibGroup
    {
        public string sName;
	    public string sWritePath;
	    public string sReadPath;
        public List<CwLib> aLib = new List<CwLib>(); //NX2

/*
        public CwLibGroup( List<Nx2LibGroup> _aList, Nx2Lib _oLib)
        {

            //bool _bExist = false;
            foreach (Nx2LibGroup _oLibGroup in _aList)
            {
                foreach (Nx2Lib _oCmpLib in _oLibGroup.aLib)
                {
                    if (_oLib.sName == _oCmpLib.sName)
                    {
                        _oLibGroup.aLib.Add(_oLib);
                        return;
                    }
                }
            }

            sName = _oLib.sName;
            aLib.Add(_oLib);
            _aList.Add(this);
        }*/

		public CwLibGroup(string _sName, string _sReadPath, string _sWritePath )
		{
			sName = _sName;
			sWritePath = _sWritePath.Replace('\\','/');
			sReadPath = _sReadPath.Replace('\\','/');
		}



		public void fAddLib(CwLib _oLib) {
			  aLib.Add(_oLib);
		}
		



    }
}
