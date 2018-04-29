using System;

namespace PNMail
{
	public partial class CatDlg : Gtk.Dialog
	{
		String smyAd;
		//mime mmime = null;

		public CatDlg (Config mConfig)
		{
			this.Build ();
			smyAd = mConfig.sMyAdress;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			String sMail;
			mime mmime = null;

			sMail = "";
			if (cbListFreq.Active)
				sMail += "PUB_PACTOR\r\n";
			//if (checkbutton4.Active)		sMail += "HAM.2KEPS\r\n";   //pas meteo 
			//if (checkbutton9.Active)				sMail += "AMSAT.KEPS\r\n";   // pas les meteo 
			if (sMail != "")
				mmime = new mime ((String)"INQUIRY", (String)"REQUEST", sMail, smyAd); 


			// envoyé commerequette sailmail
			sMail = "";
			if (cbSatKepler.Active)
				sMail += "Send https://www.celestrak.com/NORAD/elements/noaa.txt\r\n";  // chang titre 4281 char
			if (checkbutton3.Active)
				sMail += "Send http://www.sidc.be/silso/DATA/EISN/EISN_current.txt  \r\n";  // tache solaire obs belgique
			if (checkbutton5.Active)
				sMail += "Send http://services.swpc.noaa.gov/text/27-day-outlook.txt \r\n";   // flux solaire NOAA
			if (checkbutton7.Active)
				sMail += "Send atl.outlook\r\n";
			if (checkbutton9.Active)
				sMail += "Send http://www.nhc.noaa.gov/tafb_latest/atlsfc24_latestBW_sm3.gif \r\n"; //Carte gif sit gene
					
			if (sMail != "")
				mmime = new mime ((String)"query@saildocs.com", (String)"Saildocs Request", sMail, smyAd);

			sMail = "";
			// requette noaa
			if (checkbutton6.Active) {
				sMail = "open\r\ncd fax\r\n";
				sMail += "get PYAA01.TIF";
				sMail += "\r\nquit\r\n";   // 30ko compressé passe pas en gzip
			}
			if (checkbutton8.Active) {
				sMail = "open\r\ncd data\r\ncd hurricane_products\r\ncd atlantic\r\ncd weather\r\n";
				sMail += "get outlook.txt";
				sMail += "\r\nquit\r\n";  
			}
			if (sMail != "")
				mmime = new mime ((String)"NWS.FTPMail.OPS@noaa.gov", (String)"Req", sMail, smyAd);
			
			this.Respond (1);

		}
	}
}

/*

http://www.sidc.be/silso/DATA/EISN/EISN_current.txt    // Sunspot number
http://services.swpc.noaa.gov/text/27-day-outlook.txt  // noaa mais solar flux prevision
*/


/*


The following equations are useful for converting between 10 cm flux (F) and sunspot number (R). The equations are valid on a statistical (ie, average) basis.

	F = 67.0 + 0.572 R + (0.0575 R)2 - (0.0209 R)3

		R = 1.61 FD - (0.0733 FD)2 + (0.0240 FD)3

			where FD = F - 67.0

*/