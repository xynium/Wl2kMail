using System;
using System.IO;

namespace PNMail
{
	public class mime
	{
		String sBufSortie;
		public int iConstControl;
		String sWkRep = "/PNMail/";
		const int ciMIDMAXLEN = 12;
		const int ciCALLLEN = 6;
		// Call 6 signes sinon tronque
		DateTime now = DateTime.Now;


		//ecrit le fichier du mail
		public mime (String sAdress, String sSujet, String sMsg, String sMyAdress)
		{
			String sMid;
			String sFileName;

			sBufSortie = "Mid: ";
			sMid = generate_mid (sMyAdress);
			sBufSortie += sMid;
			sBufSortie += "\r\n";

			sBufSortie += "Date: ";
			sBufSortie += String.Format ("{0:D4}/{1:D2}/{2:D2} {3:D2}:{4:D2}", now.Year, now.Month, now.Day, now.Hour, now.Minute);
			sBufSortie += "\r\n";

		   /* if ((sAdress == "QTH") || (sAdress == "INQUIRY"))
				sBufSortie += "Type: Radio; Outmail\r\n";
			else
				sBufSortie += "Type: Email; OutMail\r\n";       //"Type: Private\r\n";
            */

			sBufSortie += "From: ";
		/*	if (sMyAdress.IndexOf ("@") > 0)
				sBufSortie += "SMTP:";*/
			sBufSortie += sMyAdress;
			sBufSortie += "\r\n";

			sBufSortie += "To: ";  // gerer si plusieur adresse 
		//	if (sAdress.IndexOf ("@") > 0)			sBufSortie += "SMTP:";
			sBufSortie += sAdress;
			sBufSortie += "\r\n";

			//  sBufSortie+= "Cc: ";  a gerer le CC
			//  sBufSortie+= "Bcc: ";  a gerer le Bcc

			sBufSortie += "Subject: ";
			sBufSortie += sSujet;
			sBufSortie += "\r\n";


		/*	sBufSortie += "Mbo: ";
		//	if (sMyAdress.IndexOf ("@") > 0)				sBufSortie += "SMTP:";
			sBufSortie += sMyAdress;
			sBufSortie += "\r\n";*/

			sBufSortie += "Body: ";
			sBufSortie += String.Format ("{0:D}", sMsg.Length);  //test sMsg.Length+2 effacé le +2 le 18/07 a tester
			sBufSortie += "\r\n";  

			sBufSortie += "\r\n";
			sBufSortie += sMsg;
			sBufSortie += "\r\n";  // crlf pas compte dans le body
			//sBufSortie += "\r\n"; 
						
			sFileName = Environment.GetEnvironmentVariable ("HOME") + sWkRep + "OutBox/" + sMid + ".msg";

			StreamWriter writer = new StreamWriter (sFileName);
			writer.Write (sBufSortie);
			writer.Close ();
		}

		String generate_mid (String sIn)
		{
			String sMid;
			String sRand;
			int itmp;
			//DateTime now = DateTime.Now;

			itmp = sIn.IndexOf ("@");
			if (itmp > ciCALLLEN)
				itmp = ciCALLLEN;
			sMid = sIn.Substring (0, itmp); 

			sRand = String.Format ("{0:D}", (now.Second + ((now.Minute + (now.Hour + (now.Day + now.Month * 31 + (now.Year - 2015) * 370) * 24) * 60) * 60))); 

			sMid = sRand.Substring (sRand.Length + itmp - 11, 11 - itmp) + "_" + sMid;

			return sMid;   
		}
	}
}

