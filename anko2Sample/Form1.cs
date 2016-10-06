﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace movieplay {
	internal partial class Form1 : Form {

		private readonly ankoPlugin2.IPluginHost host;

		private Config config;
        public bool IsAlive { get; private set; }

		public Form1(ankoPlugin2.IPluginHost host) {
			InitializeComponent();

			this.host = host;
			host.ReceiveContentStatus += host_ReceiveContentStatus;
			host.ConnectedServer += host_ConnectedServer;
			host.DisconnectedServer += host_DisconnectedServer;
			host.ReceiveChat += host_ReceiveChat;

			// 設定値の読込
			ConfigLoad();
		}

		private void Form1_Load(object sender, EventArgs e) {
			// 設定値をフォームに反映
			ConfigToForm();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			// フォームから設定値へ
			ConfigFromForm();
			// 設定値の保存
			ConfigSave();

			if(e.CloseReason == CloseReason.UserClosing) {
				e.Cancel = true;
				this.Hide();
			}
		}

		void host_ReceiveContentStatus(object sender, ankoPlugin2.ReceiveContentStatusEventArgs e) {
			if(e.Status.archive == 1) {
				// タイムシフト

				return;
			}
			// 生放送の情報取得

		}

		void host_ConnectedServer(object sender, ankoPlugin2.ReceiveContentStatusEventArgs e) {
			if(e.Status.archive == 1) {
				// タイムシフト

				return;
			}
			// 生放送の接続

		}

		void host_DisconnectedServer(object sender, ankoPlugin2.ConnectStreamEventArgs e) {
			// 生放送の切断

		}

		void host_ReceiveChat(object sender, ankoPlugin2.ReceiveChatEventArgs e) {
            // コメント取得
            
        }
    
		#region 設定値

		private string ConfigPath() {			 
			return Path.Combine(host.ApplicationDataFolder, string.Format("{0}.xml", this.GetType().Namespace));
		}

		private void ConfigToForm() {
			if(this.InvokeRequired) {
				Invoke(new Action(ConfigToForm));
				return;
			}

			// 設定値をフォームに反映
			if(0 < config.LocationX && 0 < config.LocationY) {
				this.Location = new Point(config.LocationX, config.LocationY);
			}
		}

		private void ConfigFromForm() {
			if(this.InvokeRequired) {
				Invoke(new Action(ConfigFromForm));
				return;
			}

			// フォームから設定値へ
			config.LocationX = this.Location.X;
			config.LocationY = this.Location.Y;
		}

		private void ConfigLoad() {
			// 設定値の読込
			config = Config.Load(ConfigPath());
		}

		private void ConfigSave() {
			// 設定値の保存
			config.Save(ConfigPath());
		}

		#endregion

	}
}