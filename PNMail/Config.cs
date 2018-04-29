using System;
using System.IO;
using System.Collections;

namespace PNMail
{
	public class Config
	{

		// prefs default
		public String sMyAdress="FM4PN@Winlink.org";  // doit commencer par le call
		public String sMyPass="NOUBLIPAS";
		public String[] sAdress;
		public String sLat="14-36.04N";
		public String  sLon="061-36.04W";
		public String sMyLoc="FK98nk";
		public int nADr=0;
		public int nMaxAdress=40;

		String sFileName;

		public Config ()
		{
			sAdress = new String[nMaxAdress];
			for (int it=0;it<nMaxAdress;it++)
				sAdress [it] = "";
					

		}

		public void ReadConfig(){
			StreamReader srPref = null;
			String sLine;
			int idt,idx;

			sFileName = Environment.GetEnvironmentVariable ("HOME") + "/PNMail/PNMail.conf";
								
			try {
				srPref = new StreamReader (sFileName);
				//bCree = true;
			} catch (DirectoryNotFoundException e) {
				// créée le repertoire
				Directory.CreateDirectory (Environment.GetEnvironmentVariable ("HOME") + "/PNMail");
				Directory.CreateDirectory (Environment.GetEnvironmentVariable ("HOME") + "/PNMail/OutBox");
				Directory.CreateDirectory (Environment.GetEnvironmentVariable ("HOME") + "/PNMail/InBox");
				Directory.CreateDirectory (Environment.GetEnvironmentVariable ("HOME") + "/PNMail/Trash");
				Console.WriteLine ("Les repertoires n'existaient pas: {0}", e.Message);
				return;
			}
			catch (FileNotFoundException e) {
				Console.WriteLine("Le fichier n'existe pas: {0}",e.Message);
				if (srPref!=null)	srPref.Close ();
				WritetheConf();
				return;
			}

			try {
				nADr=0;
				do {                                  // lit le fichier et affecte les valeurs
					sLine=srPref.ReadLine();
					idx=sLine.Length;
					idt=sLine.IndexOf(':')+1;
					if (sLine.StartsWith("CallAdress")) sMyAdress=sLine.Substring(idt,idx-idt); 
					if (sLine.StartsWith("PassWord"))  sMyPass=sLine.Substring(idt,idx-idt); 
					if (sLine.StartsWith("Locator"))  sMyLoc=sLine.Substring(idt,idx-idt); 
					if (sLine.StartsWith("Longitude"))  sLon=sLine.Substring(idt,idx-idt); 
					if (sLine.StartsWith("Latitude"))  sLat=sLine.Substring(idt,idx-idt); 
					if (sLine.StartsWith("Adress"))  sAdress[nADr++]=sLine.Substring(idt,idx-idt); 
				}while(!srPref.EndOfStream);
				srPref.Close ();
				return;
			} catch (Exception e) {
				// Probleme inconnu
				Console.WriteLine("Probleme lecture fichier: {0}",e.Message);
				if (srPref!=null)	srPref.Close ();
				WritetheConf ();
				return;
			}
			if (srPref!=null)	srPref.Close ();
			return;
		}

		public void WritetheConf(){
			String sTmp;

			StreamWriter sw = new StreamWriter (sFileName);
					
			sw.WriteLine ("#PNMail Fichier de configuration");
			sTmp = String.Format("CallAdress :{0}",sMyAdress);
			sw.WriteLine (sTmp);
			sTmp = String.Format("PassWord :{0}",sMyPass);
			sw.WriteLine (sTmp);
			sTmp = String.Format ("Locator :{0}", sMyLoc);
			sw.WriteLine (sTmp);
			sTmp = String.Format ("Latitude :{0}", sLat);
			sw.WriteLine (sTmp);
			sTmp = String.Format ("Longitude :{0}", sLon);
			sw.WriteLine (sTmp);

			nADr = 0;
			for (int it=0;it<nMaxAdress;it++){
				if (sAdress [it].Length > 5) {
					sTmp = String.Format ("Adress :{0}", sAdress [it]);
					sw.WriteLine (sTmp);
					nADr++;
				}
			}
		
			sw.Close ();

		}
	}
}

