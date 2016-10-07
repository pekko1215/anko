using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace ankoPlayer {

	/// <summary>
	/// 設定値クラス
	/// </summary>
	[Serializable]
	public class Config {
		public int LocationX { get; set; }
		public int LocationY { get; set; }

		public Config() {
			this.LocationX = 0;
			this.LocationY = 0;
		}

		/// <summary>
		/// 設定値をファイルから読み込みます
		/// </summary>
		/// <param name="path">ファイルパス</param>
		/// <returns></returns>
		public static Config Load(string path) {
			if(!File.Exists(path)){
				return new Config();
			}

			Config configData = null;
			try {
				using(FileStream fs = new FileStream(path, FileMode.Open)) {
					System.Xml.Serialization.XmlSerializer xs = new XmlSerializer(typeof(Config));
					configData = xs.Deserialize(fs) as Config;
				}
			}
			catch { }

			return configData != null ? configData : new Config();
		}

		/// <summary>
		/// 設定値をファイルへ書き込みます
		/// </summary>
		/// <param name="path">ファイルパス</param>
		public void Save(string path) {
			try {
				using(FileStream fs = new FileStream(path, FileMode.Create)) {
					System.Xml.Serialization.XmlSerializer xs = new XmlSerializer(typeof(Config));
					xs.Serialize(fs, this);
				}
			}
			catch { }
		}

	}

}
