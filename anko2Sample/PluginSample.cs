using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ankoPlayer {
	public class AnkoPlayer : APlugin {

		private Form1 form;

		void host_Initialized(object sender, EventArgs e) {
			form = new Form1(host);
		}

		#region IPlugin メンバー

		public override string Description { get { return AssemblyDescription; } }

		public override bool IsAlive {
			get { return form == null || form.IsDisposed ? false : form.IsAlive; }
		}

		public override string Name { get { return AssemblyTitle; } }

		public override void Run() {
			if(form == null || form.IsDisposed) {
				form = new Form1(host);
			}

			if(!form.Visible) {
				form.Show((IWin32Window)host.Win32WindowOwner);
			}
			else if(form.WindowState != FormWindowState.Normal) {
				form.WindowState = FormWindowState.Normal;
			}
		}

		public override ankoPlugin2.IPluginHost host {
			set {
				base.host = value;
				host.Initialized += host_Initialized;
			}
		}

		#endregion

		#region アセンブリ属性アクセサー

		public string AssemblyTitle {
			get {
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if(attributes.Length > 0) {
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
					if(titleAttribute.Title != "") {
						return titleAttribute.Title;
					}
				}
				return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		public string AssemblyVersion {
			get {
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		public string AssemblyDescription {
			get {
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				if(attributes.Length == 0) {
					return "";
				}
				return ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		#endregion

	}
    
}
