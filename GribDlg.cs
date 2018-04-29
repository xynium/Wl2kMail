using System;

namespace PNMail
{
	public partial class GribDlg : Gtk.Dialog
	{
		String smyAd;

		public GribDlg (Config mConfig)
		{
			int iVal;
			//String sLettre;

			this.Build ();
			smyAd = mConfig.sMyAdress;
			iVal = Val (mConfig.sLat);
			eReqSud.Text = String.Format ("{0}{1}", iVal - 1, Let (mConfig.sLat));
			eReqNord.Text = String.Format ("{0}{1}", iVal + 1, Let (mConfig.sLat));
			iVal = Val (mConfig.sLon);
			eReqOuest.Text = String.Format ("{0}{1}", iVal + 1, Let (mConfig.sLon));
			eReqEst.Text = String.Format ("{0}{1}", iVal - 1, Let (mConfig.sLon));

			eLat.Text = String.Format ("{0:D2}.{1:D1}{2}", Val (mConfig.sLat), ValD (mConfig.sLat), Let (mConfig.sLat));  // Spot en Degre et fraction
			eLong.Text = String.Format ("{0:D3}.{1:D1}{2}", Val (mConfig.sLon), ValD (mConfig.sLon), Let (mConfig.sLon));

		}

		// TODO faire un compteur de volume 1.5 octet par donné

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			String sMail;
			int iTot, iDure;
			int iCt;
			Gtk.TreeIter Giter;

			if (nbMain.Page == 0) {
				sMail = "send GFS:" + eReqSud.Text + "," + eReqNord.Text + "," + eReqOuest.Text + "," + eReqEst.Text + "|";
				sMail += cbGPas.ActiveText + "," + cbGPas.ActiveText + "|";
				int iPas = Convert.ToUInt16 (cbDPas.ActiveText);
			
				if (cbExpo.Active) {   // TODO la version exponentielle
					iDure = Convert.ToUInt16 (eDure.Text);
					if (iDure > 360)
						iDure = 360;  // si plus que 15jours
					iTot = iPas;

					cbDPas.Model.GetIterFromString (out Giter, cbDPas.Active.ToString ());

					do {
						sMail += String.Format ("{0},{1}..", iTot, iTot + iPas);
						iTot += 4 * iPas;
						//iPas *= 2;
						GLib.Value thisRow = new GLib.Value ();
						cbDPas.Model.GetValue (Giter, 0, ref thisRow);
						if (cbDPas.Model.IterNext (ref Giter))
							iPas = Convert.ToUInt16 ((String)cbDPas.Model.GetValue (Giter, 0));
					} while (iTot < iDure);
					iPas /= 2;
					iCt = 0;
					do {
						iCt++;
						iTot -= iPas;
					} while (iTot > iDure);
					switch (iCt) {
					case 1:
					case 2:
						sMail += String.Format ("{0}", iTot + iPas);
						break;
					case 3: //supprime les ... 
						sMail = sMail.TrimEnd ('.');
						sMail += String.Format (",{0}", iTot + iPas);
						break;
					case 4:
					case 5:
						sMail = sMail.TrimEnd ('.');
						break;
					}
				} else
					sMail += String.Format ("{0},{1}..{2}", iPas, 2 * iPas, eDure.Text);
				sMail += "|WIND";
				if (cbCAPE.Active == true)
					sMail += ",CAPE";
				if (cbVague.Active == true)
					sMail += ",WAVES";
				if (cbRain.Active == true)
					sMail += ",RAIN";


			} else { //	if (nbMain.Page ==1) 
				sMail = "send Spot:" + eLat.Text + "," + eLong.Text + "|";
				sMail += eDure1.Text + "," + cbDPas1.ActiveText + "|";
				//if (cbVent.Active == true)
				sMail += "WIND,";
				if (cbPress.Active == true)
					sMail += "PRMSL,";
				if (cbWaves.Active == true)
					sMail += "WAVES,";
				if (cbRain1.Active == true)
					sMail += "RAIN,";
				if (cbCape1.Active == true)
					sMail += "CAPE,";
				if (cbSurfTmp.Active == true)
					sMail += "SFCTMP,";
				if (cbLFTX.Active == true)
					sMail += "LFTX,";
				sMail = sMail.TrimEnd (',');
		
			}

			mime mmime = new mime ((String)"query@saildocs.com", (String)"Saildocs Request", sMail, smyAd);
			this.Respond (1);
		}

		int Val (String s)
		{ //retrouve la valeur dans chaine type :  14-36.04N (il y a un espace devant)
			int iR;

			iR = s.IndexOf ("-");
			String sMid = s.Substring (0, iR); 
			iR = Convert.ToInt16 (sMid);

			return iR;
		}

		// formatage po spot fraction en dixieme degre
		int ValD (String s)
		{ //retrouve la valeur dans chaine type :  14-36.04N (il y a un espace devant)
			int iR;

			iR = s.IndexOf ("-");
			String sMid = s.Substring (iR + 1, 2); 
			try {
				double d = Convert.ToDouble (sMid);
				iR = (int)(d / 6.0);
			} catch {
				iR = 0;
			}
			return iR;
		}

		String Let (String s)
		{ //retrouve la valeur dans chaine type :  14-36.04N (il y a un espace devant)
			String sR;

			sR = s.Substring (s.Length - 1, 1); 

			return sR;
		}
	}
}

