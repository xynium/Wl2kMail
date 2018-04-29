using System;

namespace PNMail
{
	public partial class MailDlg : Gtk.Dialog
	{
		//String sBuff;
		//mime mmime = null;
		String smyAd;

		public MailDlg (Config mConfig)
		{
			this.Build ();
			smyAd = mConfig.sMyAdress;
				for (int inAdr = 0; inAdr < mConfig.nADr; inAdr++) {
				cbAdress.AppendText ((string)mConfig.sAdress [inAdr]);
			}
		}

		protected void OnButtonOkClicked (object sender, EventArgs e){
			mime mmime = new mime (cbAdress.ActiveText, eSujet.Text, tvMail.Buffer.Text, smyAd);
			this.Respond (1);
		}

	}
}

