using System;
using Gtk;
using System.IO;
using System.Collections;
using System.IO.Ports;
using System.Timers;

namespace PNMail
{
	public partial class MainWindow : Gtk.Window
	{
		Config mConfig = null;
		Gtk.NodeStore store;
		SerialPort sPort = null;
		Timer tTime = null;
		public double dGPSSog, dGPSCog;
		int iGPS;

		public MainWindow () : base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			mConfig = new Config ();
			mConfig.ReadConfig ();
			nvIn.AppendColumn ("Nom du fichier", new Gtk.CellRendererText (), "text", 0);
			nvIn.AppendColumn ("Sujet", new Gtk.CellRendererText (), "text", 1);
			nvIn.AppendColumn ("Provenance", new Gtk.CellRendererText (), "text", 2);
			nvIn.AppendColumn ("Long.", new Gtk.CellRendererText (), "text", 3);
			nvOut.AppendColumn ("Nom du fichier", new Gtk.CellRendererText (), "text", 0);
			nvOut.AppendColumn ("Sujet", new Gtk.CellRendererText (), "text", 1);
			nvOut.AppendColumn ("Destination", new Gtk.CellRendererText (), "text", 2);
			nvOut.AppendColumn ("Long.", new Gtk.CellRendererText (), "text", 3);

			nvIn.NodeStore = StoreIn;
			nvOut.NodeStore = StoreOut;

			nvIn.ShowAll ();
			nvOut.ShowAll ();

			tTime = new Timer (1000);  
			tTime.Elapsed += new ElapsedEventHandler (delegate {
				ontTimerTick ();
			}); 
			tTime.Start ();
			sPort = new SerialPort ();
			sPort.PortName = "/dev/rfcomm0";
			sPort.BaudRate = 4800;
			sPort.Parity = Parity.None;
			sPort.StopBits = StopBits.One;
			iGPS = 0;
			try {
				if (!sPort.IsOpen) {
					sPort.Open ();  //open the port.
					//sPort.ReadExisting (); bloque si il n'y a rien
				}

			} catch (Exception ex) {
				Console.WriteLine (DateTime.UtcNow + " SERIAL: " + ex.Message);
			}  
		
		}

		protected void ontTimerTick ()
		{
			double dGPSLat, dGPSLong;
			string sGps;
			string[] value;
			double dL, dM, dMM, ddM;
			int irep;
			string sCrc;

		/*			nvIn.NodeStore = StoreIn; // rafraichit les listes
			nvOut.NodeStore = StoreOut;
			nvIn.ShowAll ();
			nvOut.ShowAll ();*/
			iGPS -= 1;
			if (iGPS < 0)
				iGPS = 0;

			if (sPort.IsOpen) {      	//when the port is open  
				do {
					sGps = sPort.ReadLine ();
					if (sGps.StartsWith ("$GPRMC")) {
						int GpsCrc = 0;

						for (irep = 1; irep < sGps.Length; irep++) {
							if (sGps [irep] != '*')
								GpsCrc ^= sGps [irep];
							else
								break;
						}
						irep++;
						sCrc = sGps.Substring (irep, 2);
						irep = System.Convert.ToByte (sCrc, 16);
						if (irep == GpsCrc) {
							value = sGps.Split (',');
							System.Globalization.NumberFormatInfo provider = new System.Globalization.NumberFormatInfo ();
							provider.NumberDecimalSeparator = ".";
							if (value [3] == "")
								return;
							dGPSLat = System.Convert.ToDouble (value [3], provider);
							dGPSLat /= 100;
							dL = Math.Floor (dGPSLat);
							dM = (dGPSLat - dL) * 100;
							dMM = Math.Floor (dM);
							ddM = (dM - dMM) * 100;
							ddM = Math.Floor (ddM);
							mConfig.sLat = String.Format ("{0:D2}-{1:D2}.{2:D2}{3}", (int)dL, (int)dMM, (int)ddM, value [4]);

							dGPSLong = System.Convert.ToDouble (value [5], provider);
							dGPSLong /= 100;
							dL = Math.Floor (dGPSLong);
							dM = (dGPSLong - dL) * 100;
							dMM = Math.Floor (dM);
							ddM = (dM - dMM) * 100;
							ddM = Math.Floor (ddM);
							mConfig.sLon = String.Format ("{0:D3}-{1:D2}.{2:D2}{3}", (int)dL, (int)dMM, (int)ddM, value [6]);
														
							dGPSSog = System.Convert.ToDouble (value [7], provider);
							dGPSCog = System.Convert.ToDouble (value [8], provider);
							iGPS = 10;
						//labGPS.Text = "GPS OK   ";

						}
					}
				} while(sPort.BytesToRead > 30);
			} 
		if (iGPS > 1)
				labGPS.Text = "GPS OK   ";
			else
				labGPS.Text = "Pas de GPS   ";
		}


		Gtk.NodeStore StoreIn {
			get {
				store = new Gtk.NodeStore (typeof(MyTreeNode));
				DirectoryInfo dirInfo = new DirectoryInfo (Environment.GetEnvironmentVariable ("HOME") + "/PNMail/InBox");
				try {
					FileInfo[] oldFiles = dirInfo.GetFiles ("*.*", SearchOption.TopDirectoryOnly);
					if (oldFiles.Length == 0)
						throw new IOException ("");
					foreach (FileInfo fi in oldFiles) {
						String sSuj = TrouveSujet (fi);
						String sProv = TrouveProv (fi);
						store.AddNode (new MyTreeNode (fi.Name, sSuj, sProv, (int)fi.Length));
					}
				} catch (IOException e) {
					store.AddNode (new MyTreeNode ("Vide", "", "", 0));
				}

				return store;
			}
		}

		Gtk.NodeStore StoreOut {
			get {
				store = new Gtk.NodeStore (typeof(MyTreeNode));
				DirectoryInfo dirInfo = new DirectoryInfo (Environment.GetEnvironmentVariable ("HOME") + "/PNMail/OutBox");
				try {
					FileInfo[] oldFiles = dirInfo.GetFiles ("*.*", SearchOption.TopDirectoryOnly);
					if (oldFiles.Length == 0)
						throw new IOException ("");
					foreach (FileInfo fi in oldFiles) {
						String sSuj = TrouveSujet (fi);
						String sDest = TrouveDest (fi);
						store.AddNode (new MyTreeNode (fi.Name, sSuj, sDest, (int)fi.Length));
					}
				} catch (IOException e) {
					store.AddNode (new MyTreeNode ("Vide", "", "", 0));
				}
				return store;
			}
		}

		protected void OnBtnMailClicked (object sender, EventArgs e)
		{
			MailDlg mdlgMail = new MailDlg (mConfig);

			mdlgMail.Modal = true;
			mdlgMail.Response += on_dialog_response;
			mdlgMail.Run ();
			mdlgMail.Destroy ();
		}

		protected void OnBtnPosClicked (object sender, EventArgs e)
		{
			if (iGPS == 0) {
				dGPSSog = 0;
				dGPSCog = 0;
			}
			PosDlg mPosDlg = new PosDlg (mConfig, dGPSSog, dGPSCog);

			mPosDlg.Modal = true;
			mPosDlg.Response += on_dialog_response;
			mPosDlg.Run ();
			mPosDlg.Destroy ();
		}

		protected void OnBtnCataClicked (object sender, EventArgs e)
		{
			CatDlg mCatDlg = new CatDlg (mConfig);

			mCatDlg.Modal = true;
			mCatDlg.Response += on_dialog_response;
			mCatDlg.Run ();
			mCatDlg.Destroy ();
		}

		protected void OnBtnGribClicked (object sender, EventArgs e)
		{
			GribDlg mGribDlg = new GribDlg (mConfig);

			mGribDlg.Modal = true;
			mGribDlg.Response += on_dialog_response;
			mGribDlg.Run ();
			mGribDlg.Destroy ();
		}

		void on_dialog_response (object obj, ResponseArgs args)
		{
			nvIn.NodeStore = StoreIn;
			nvOut.NodeStore = StoreOut;
			nvIn.ShowAll ();
			nvOut.ShowAll ();
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			OnQuitterActionActivated (sender, a);
		}

		protected void OnQuitterActionActivated (object sender, EventArgs e)
		{
			sPort.Close ();
			store.Dispose ();
			StoreOut.Dispose ();
			StoreIn.Dispose ();
			mConfig = null;
			//this.Destroy ();
			Application.Quit ();
		}

		protected void OnInformationActionActivated (object sender, EventArgs e)
		{
			MessageDialog msgAbout = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Close, "PNMail de FM4PN 2016");
			msgAbout.Title = "Information";
			msgAbout.Run ();
			msgAbout.Destroy ();
		}

		protected void OnPreferencesAction2Activated (object sender, EventArgs e)
		{
			DlgPrefs mdlgPrefs = new DlgPrefs (mConfig);

			mdlgPrefs.Modal = true;
			mdlgPrefs.Response += on_dialogpref_response;
			mdlgPrefs.Run ();
			mdlgPrefs.Destroy ();
		}

		void on_dialogpref_response (object obj, ResponseArgs args)
		{
			if (args.ResponseId == ResponseType.Ok)
				mConfig.ReadConfig ();
		}

		//Trouve le sujet dans le fichier
		String TrouveSujet (FileInfo fi)
		{
			String sTmp, sRet;
			int idx, idt;
			StreamReader sr = new StreamReader (fi.DirectoryName + "/" + fi.Name);

			sRet = "";
			do {                                  // lit le fichier et affecte les valeurs
				sTmp = sr.ReadLine ();
				if (sTmp==null) break;
				idx = sTmp.Length;
				idt = sTmp.IndexOf (':') + 1;
				if (sTmp.StartsWith ("Subject")) {
					sRet = sTmp.Substring (idt, idx - idt);
					break;
				}
			} while(!sr.EndOfStream);

			return sRet;
		}

		//Trouve la provenance
		String TrouveProv (FileInfo fi)
		{
			String sTmp, sRet;
			int idx, idt;
			StreamReader sr = new StreamReader (fi.DirectoryName + "/" + fi.Name);

			sRet = "";
			do {                                  // lit le fichier et affecte les valeurs
				sTmp = sr.ReadLine ();
				if (sTmp==null) break;
				idx = sTmp.Length;
				idt = sTmp.IndexOf (':') + 1;
				if (sTmp.StartsWith ("From")) {
					sRet = sTmp.Substring (idt, idx - idt);
					break;
				}
			} while(!sr.EndOfStream);

			return sRet;
		}

		//Trouve la Destination
		String TrouveDest (FileInfo fi)
		{
			String sTmp, sRet;
			int idx, idt;
			StreamReader sr = new StreamReader (fi.DirectoryName + "/" + fi.Name);

			sRet = "";
			do {                                  // lit le fichier et affecte les valeurs
				sTmp = sr.ReadLine ();
				if (sTmp==null) break;
				idx = sTmp.Length;
				idt = sTmp.IndexOf (':') + 1;
				if (sTmp.StartsWith ("To")) {
					sRet = sTmp.Substring (idt, idx - idt);
					break;
				}
			} while(!sr.EndOfStream);

			return sRet;
		}

		// une ligne est afficher lecture msg
		protected void OnNvInRowActivated (object o, RowActivatedArgs args)
		{
			MyTreeNode item = (MyTreeNode)nvIn.NodeStore.GetNode (args.Path);
			String sFileName = Environment.GetEnvironmentVariable ("HOME") + "/PNMail/InBox/" + item.CNom;
			StreamReader srMsg = new StreamReader (sFileName);
			String sTxt = srMsg.ReadToEnd ();
			LecDlg msgRdMsg = new LecDlg (item.CNom, sTxt);
			msgRdMsg.Run ();
			msgRdMsg.Destroy ();
		}

		protected void OnNvInKeyPressEvent (object o, KeyPressEventArgs args)
		{
			Gtk.NodeView selection = (Gtk.NodeView)o;
			MyTreeNode node = (MyTreeNode)selection.NodeSelection.SelectedNode;

			if ((args.Event.Key == Gdk.Key.Delete) || (args.Event.Key == Gdk.Key.s) || (args.Event.Key == Gdk.Key.d)) { // Efface
				String msgTxt = "Comfirmer l'effacage de\n" + node.CNom; 
				MessageDialog msgConf = new MessageDialog (this, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, msgTxt);
				ResponseType rtD = (Gtk.ResponseType)msgConf.Run ();
				msgConf.Destroy ();
				if (rtD == ResponseType.Yes) { //efface
					String sTrash = Environment.GetEnvironmentVariable ("HOME") + "/PNMail/Trash/" + node.CNom; 
					String sProc = Environment.GetEnvironmentVariable ("HOME") + "/PNMail/InBox/" + node.CNom;
					try {
						File.Copy (sProc, sTrash);  // Move marche pas ????
						File.Delete (sProc);
					} catch (IOException e) {
						Console.WriteLine ("PB: {0}", e.Message);
					}

					nvIn.NodeStore = StoreIn;
					nvIn.ShowAll ();
				}
			} else { // Raffraichit
				nvIn.NodeStore = StoreIn;
				nvOut.NodeStore = StoreOut;
				nvIn.ShowAll ();
				nvOut.ShowAll ();	
			}
		}

		protected void OnNvOutRowActivated (object o, RowActivatedArgs args)
		{
			MyTreeNode item = (MyTreeNode)nvOut.NodeStore.GetNode (args.Path);
			String sFileName = Environment.GetEnvironmentVariable ("HOME") + "/PNMail/OutBox/" + item.CNom;
			StreamReader srMsg = new StreamReader (sFileName);
			String sTxt = srMsg.ReadToEnd ();
			LecDlg msgRdMsg = new LecDlg (item.CNom, sTxt);
			msgRdMsg.Run ();
			msgRdMsg.Destroy ();
		}



		protected void OnNvOutKeyPressEvent (object o, KeyPressEventArgs args)
		{
			Gtk.NodeView selection = (Gtk.NodeView)o;
			MyTreeNode node = (MyTreeNode)selection.NodeSelection.SelectedNode;

			if ((args.Event.Key == Gdk.Key.Delete) || (args.Event.Key == Gdk.Key.s) || (args.Event.Key == Gdk.Key.d)) { // Efface
				String msgTxt = "Comfirmer l'effacage de\n" + node.CNom; 
				MessageDialog msgConf = new MessageDialog (this, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, msgTxt);
				ResponseType rtD = (Gtk.ResponseType)msgConf.Run ();
				msgConf.Destroy ();
				if (rtD == ResponseType.Yes) { //efface
					String sTrash = Environment.GetEnvironmentVariable ("HOME") + "/PNMail/Trash/" + node.CNom; 
					String sProc = Environment.GetEnvironmentVariable ("HOME") + "/PNMail/OutBox/" + node.CNom;
					try {
						File.Copy (sProc, sTrash);  // Move marche pas ????
						File.Delete (sProc);
					} catch (IOException e) {
						Console.WriteLine ("PB: {0}", e.Message);
					}

					nvOut.NodeStore = StoreOut;
					nvOut.ShowAll ();
				}
			} else { // Raffraichit
				nvIn.NodeStore = StoreIn;
				nvOut.NodeStore = StoreOut;
				nvIn.ShowAll ();
				nvOut.ShowAll ();	
			}
		}
	}


	// Classe po la structure des box
	[Gtk.TreeNode (ListOnly = true)]
	public class MyTreeNode : Gtk.TreeNode
	{
		
		public MyTreeNode (string Nom, string Sujet, string Dest, int iLong)
		{
			CNom = Nom;
			CSujet = Sujet;
			CDest = Dest;
			CLong = iLong;
		}

		[Gtk.TreeNodeValue (Column = 0)]
		public string CNom;

		[Gtk.TreeNodeValue (Column = 1)]
		public string CSujet;

		[Gtk.TreeNodeValue (Column = 2)]
		public string CDest;

		[Gtk.TreeNodeValue (Column = 3)]
		public int CLong;
	}

}

