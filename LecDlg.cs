using System;
using System.Diagnostics;
using System.IO;
using System.Collections;

namespace PNMail
{
	public partial class LecDlg : Gtk.Dialog
	{
		String LTitre;

		public LecDlg (String Titre, String Txt)
		{
			this.Build ();
			this.Title = Titre;
			tvMsg.Buffer.Text = Txt;
			LTitre = Titre;
		}


		// Les Pieces jointes ne se separent pas avec l'instruction munpack
		// Cas ou il n'y as qu'une piece

		protected void OnBtnDepackClicked (object sender, EventArgs e)
		{
			StreamReader srMail = null;
			String sLine;
			int idx, idt;
			int iBodylon, iFilelon;
			String[] sSplited = null;
			int iOff;
			byte[] bbuff = null;

			String sMelFile = Environment.GetEnvironmentVariable ("HOME") + "/PNMail/InBox/" + LTitre;
			srMail = new StreamReader (sMelFile);
			sSplited = new String[3]{ "", "", "" };
			iBodylon = 0;
			iOff = 0;
					
			do {
				sLine = srMail.ReadLine ();
				idx = sLine.Length;
				iOff += idx + 2;  // suppose un \r\n
				idt = sLine.IndexOf (':') + 1;
				if (sLine.StartsWith ("Body:"))
					iBodylon = Convert.ToInt16 (sLine.Substring (idt, idx - idt)); 
				if (sLine.StartsWith ("File:"))
					sSplited = sLine.Split (new Char[] { ' ' }, 3);
			} while (sLine != String.Empty); 

			iFilelon = Convert.ToInt16 (sSplited [1]);
			iOff += iBodylon + 2;
			srMail.BaseStream.Seek (iOff, SeekOrigin.Begin);
			bbuff = new byte[iFilelon];
			srMail.BaseStream.Read (bbuff, 0, iFilelon);

			String sPFile = Environment.GetEnvironmentVariable ("HOME") + "/PNMail/InBox/" + sSplited [2];
			try {
				FileStream fs = new FileStream (sPFile, FileMode.Create,FileAccess.Write);
				BinaryWriter bwPFile = new BinaryWriter (fs);
				bwPFile.Write (bbuff, 0, iFilelon);
				bwPFile.Close ();
				fs.Close();
			} catch (IOException ex) {
				Console.WriteLine ("PB creation piece: {0}", ex.Message);
			}
						

			srMail.Close ();


		}
	}
}

