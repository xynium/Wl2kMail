using System;
using System.IO;
using Gtk;

namespace PNMail
{
	
	public partial class DlgPrefs : Gtk.Dialog
	{
		Config mDCfg = null;
		int inAdr;

		public DlgPrefs (Config mCfg)
		{
			this.Build ();
			mDCfg = mCfg;
			eCall.Text = (String)mDCfg.sMyAdress;
			eCode.Text = (String)mDCfg.sMyPass;
			eLocator.Text = (String)mDCfg.sMyLoc;

			for (inAdr = 0; inAdr < mDCfg.nADr; inAdr++) {
				cbAdress.AppendText ((string)mDCfg.sAdress [inAdr]);
			}
		}

		protected void OnBtnAddClicked (object sender, EventArgs e)
		{
			if ((cbAdress.ActiveText.Length > 5) && (mDCfg.nADr < mDCfg.nMaxAdress)) {
				mDCfg.sAdress [mDCfg.nADr] = cbAdress.ActiveText;
				cbAdress.AppendText ((String)mDCfg.sAdress [mDCfg.nADr]);
				mDCfg.nADr += 1;
			}
		}

		//Sort des lignes du combo
		//TODO
		protected void OnBtnEffClicked (object sender, EventArgs e)
		{
		}

		//TODO button witelist

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			mDCfg.sMyAdress = eCall.Text;
			mDCfg.sMyPass = eCode.Text;
			mDCfg.sMyLoc = eLocator.Text;
		
			mDCfg.WritetheConf ();
		}

	}
}

