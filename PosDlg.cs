using System;

namespace PNMail
{
	public partial class PosDlg : Gtk.Dialog
	{
		//String smyAd;
		Config mPosCfg;

		public PosDlg (Config mConfig, double dSog, double dCog)
		{
			DateTime now = DateTime.Now;

			this.Build ();
			//smyAd = mConfig.sMyAdress;
			mPosCfg = mConfig;
			eDatps.Text = String.Format ("{0:D4}/{1:D2}/{2:D2} {3:D2}:{4:D2}", now.Year, now.Month, now.Day, now.Hour, now.Minute);
			eLat.Text = mConfig.sLat;
			eLong.Text = mConfig.sLon;
			if (dSog > 0.3) {
				System.Globalization.NumberFormatInfo provider = new System.Globalization.NumberFormatInfo ();
				provider.NumberDecimalSeparator = ".";
				eVit.Text = String.Format (provider, "{0:F1}", dSog);
				eCap.Text = String.Format ("{0:D3}T", (int)dCog);  //TODO erreur dans le formatage
			} else {
				eVit.Text = "0.0";
				eCap.Text = "0T";
			}
			tvComment.Buffer.Text = "73";
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			String sMail;
			String U = "ABCDEFGHIJKLMNOPQRSTUVWX";
			String L = U.ToLower ();

			double dvit = double.Parse (eVit.Text, System.Globalization.CultureInfo.InvariantCulture);
			sMail = eCap.Text.Substring (eCap.Text.Length - 1, 1);
			if (sMail != "T")
				eCap.Text += "T";

			sMail = "TIME: " + eDatps.Text + "\r\nLATITUDE: " + eLat.Text + "\r\nLONGITUDE: " + eLong.Text;
			if (dvit > 0.2)
				sMail += "\r\nCOURSE: " + eCap.Text + "\r\nSPEED: " + eVit.Text;
			sMail += "\r\nCOMMENT: " + tvComment.Buffer.Text;
			sMail += "\r\n";

			mime mmime = new mime ((String)"QTH@winlink.org", (String)"POSITION REPORT", sMail, mPosCfg.sMyAdress);

			// recalc le locator
			int idx = eLat.Text.IndexOf ("-");
			String sMid = eLat.Text.Substring (0, idx); 
			double dLat = Convert.ToDouble (sMid);
			sMid = eLat.Text.Substring (idx + 1, eLat.Text.Length - idx - 2); 
			double dmin = double.Parse (sMid, System.Globalization.CultureInfo.InvariantCulture);  // Minute et fraction
			dLat += dmin / 100;
			if (eLat.Text.Substring (eLat.Text.Length - 1, 1) == "S")
				dLat = -dLat;
			idx = eLong.Text.IndexOf ("-");
			sMid = eLong.Text.Substring (0, idx); 
			double dLong = Convert.ToDouble (sMid);
			sMid = eLong.Text.Substring (idx + 1, eLong.Text.Length - idx - 2); 
			dmin = double.Parse (sMid, System.Globalization.CultureInfo.InvariantCulture);  // Minute et fraction
			dLong += dmin / 100;
			if (eLong.Text.Substring (eLong.Text.Length - 1, 1) == "W")
				dLong = -dLong;
			
			dLat += 90;
			dLong += 180;
			String GLat = U.Substring ((int)(dLat / 10), 1);
			String GLon = U.Substring ((int)(dLong / 20), 1);
			String nLat = "" + (int)(dLat % 10);
			String nLon = "" + (int)((dLong / 2) % 10);
			double rLat = (dLat - (int)(dLat)) * 60;
			double rLon = (dLong - 2 * (int)(dLong / 2)) * 60;
			String gLat = L.Substring ((int)(rLat / 2.5), 1);
			String gLon = L.Substring ((int)(rLon / 5), 1);
			mPosCfg.sMyLoc = "" + GLon + GLat + nLon + nLat + gLon + gLat;

			//ecrit la config
			mPosCfg.sLat = eLat.Text;
			mPosCfg.sLon = eLong.Text;
			mPosCfg.WritetheConf ();

			this.Respond (1);

		}
	}
}

