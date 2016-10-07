using System;
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
using System.Runtime.Remoting.Channels;;

using System.Runtime.Remoting.Channels.Ipc;
namespace ankoPlayer {
	internal partial class Form1 : Form {
        
        private readonly ankoPlugin2.IPluginHost host;

		private Config config;
        public SharedMemory Smem_1;
		public bool IsAlive { get; private set; }

		public Form1(ankoPlugin2.IPluginHost host) {
			InitializeComponent();

			this.host = host;
			host.ReceiveContentStatus += host_ReceiveContentStatus;
			host.ConnectedServer += host_ConnectedServer;
			host.DisconnectedServer += host_DisconnectedServer;
			host.ReceiveChat += host_ReceiveChat;
            

            ConfigLoad();
		}

		private void Form1_Load(object sender, EventArgs e) {
            // 設定値をフォームに反映
            Smem_1 = new SharedMemory(1024, "ankoPLayer");
            SetData(1234);
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
        public unsafe void SetData(Int32 data)
        {
            Byte[] ByteArray = BitConverter.GetBytes(data);
            Smem_1.OpenMapView_Write();
            for(int i = 0; i < ByteArray.Length; i++)
            {
                Smem_1.Address[i] = ByteArray[i];
            }
            Smem_1.CloseMapView();
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
    // ************************************
    // 共有メモリ管理クラス
    // ************************************
    public partial class SharedMemory : IDisposable
    {

        // ************************************
        // WindowsAPIのインポート
        // ************************************

        [System.Runtime.InteropServices.DllImport("Kernel32")]
        public static extern IntPtr CreateFileMapping(
            IntPtr hFile,
            IntPtr pAttributes,
            FileProtection flProtect,
            UInt32 dwMaximumSizeHigh,
            UInt32 dwMaximumSizeLow,
            String pName);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenFileMapping(
            FileMapAccess dwDesiredAccess,
            //FileRights dwDesiredAccess,
            bool bInheritHandle,
            string lpName);
 

        [System.Runtime.InteropServices.DllImport("Kernel32")]
        public static extern Boolean CloseHandle(IntPtr handle);

        [System.Runtime.InteropServices.DllImport("Kernel32")]
        public static extern IntPtr MapViewOfFile(
            IntPtr hFileMappingObject,
            FileMapAccess dwDesiredAccess,
            UInt32 dwFileOffsetHigh, UInt32 dwFileOffsetLow,
            IntPtr dwNumberOfBytesToMap);

        [System.Runtime.InteropServices.DllImport("Kernel32")]
        public static extern Boolean UnmapViewOfFile(IntPtr address);
 
        // ************************************
        // メンバー変数
        // ************************************

        //========================
        // WindowsAPI関連の属性値
        //========================
        public enum FileProtection : uint
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400,
            SEC_FILE = 0x800000,
            SEC_IMAGE = 0x1000000,
            SEC_RESERVE = 0x4000000,
            SEC_COMMIT = 0x8000000,
            SEC_NOCACHE = 0x10000000
        }

        public enum FileMapAccess
        {
            FILE_MAP_COPY = 0x0001,
            FILE_MAP_WRITE = 0x0002,
            FILE_MAP_READ = 0x0004,
            FILE_MAP_ALL_ACCESS = 0x000F001F
        }

        public readonly IntPtr InvalidHandleValue = new IntPtr(-1);

        enum FileRights : uint          // constants from WinBASE.h
        {
            Read = 4,
            Write = 2,
            ReadWrite = Read + Write
        }

        //========================
        //  オブジェクトをファイル、又はメモリにマッピングするための変数
        //========================

        // マッピングオブジェクトのハンドル
        private IntPtr m_hFileMap = IntPtr.Zero;

        // マッピングオブジェクトの名前
        private String m_name;

        // マップされたビューの開始アドレス
        private IntPtr m_address = IntPtr.Zero;

        // マッピングオブジェクトのサイズ
        private int MaximumSize;

        // 要素の現在位置
        public int CurrentPosition = 0;
 
        //========================
        // メンバー変数＜プロパティ＞
        //========================
 

        // ==============
        // アドレス取得プロパティ
        // マップされたビューの開始アドレスを取得します。
        public unsafe Byte* Address
        {
            get
            {
                return (Byte*)m_address;
            }
        }

        // ==============
        // インデクサ
        //  --- 配列やリストは、その各要素を a[0]、a[1]、a[2]、--- と表現
        //      することが可能ですが、ユーザー定義クラスにおいても、その
        //      ような表現を可能にするように定義することが可能です。
        public unsafe Byte this[int Elem_Index]
        {
            get
            {
                // 配列添え字番号が、オーバーしていない場合（正常系）
                if ((Elem_Index < MaximumSize)
                    && (Elem_Index >= 0))
                {
                    return Address[Elem_Index];
                }
                // 配列添え字番号が、オーバーしている場合のエラー処理
                else
                {
                    return 0;
                }
            }
            set
            {
                // 配列添え字番号が、オーバーしていない場合（正常系）
                if ((Elem_Index < MaximumSize)
                    && (Elem_Index >= 0))
                {
                    Address[Elem_Index] = value;
                }
                // 配列添え字番号が、オーバーしている場合のエラー処理
                else
                {
                    // 未処理
                }
            }
        }
 
 
        // ************************************
        // コンストラクタ等
        // ************************************
 
        //========================
        // コンストラクタ＜マッピングオブジェクトを生成＞
        //========================

        // ==============
        // コンストラクタ
        // ファイルマッピングオブジェクトを生成する。
        // かつ、
        // プロセスのアドレス空間に、ファイルのビューをマップする。
        // 第１引数: ファイルマッピングオブジェクトのサイズ（バイト）
        // 第２引数: マッピングオブジェクトの名前
        // 第３引数: 読み込みか、書き込みかの指示（true: 読み込み / false: 書き込み）
        public SharedMemory(Int32 size, String name, bool ReadOnly)
        {
            // ファイルマッピングオブジェクト生成
            // （要するに、メモリマップドファイルを管理制御するための
            //   根本的なオブジェクトを生成します）
            CreateSharedMemory(size, name);

            // 「読み込み」が指定されている場合
            if (ReadOnly == true)
            {
                // マップビューオープン＜読み込み用＞
                OpenMapView_Read();
            }
            // 「書き込み」が指定されている場合
            else
            {
                // マップビューオープン＜書き込み用＞
                OpenMapView_Write();
            }
        }

        // ==============
        // コンストラクタ
        // ファイルマッピングオブジェクトを生成する。
        // なお、プロセスのアドレス空間に、ファイルのビューをマップしない。
        // （後に、ファイルのビューをマップします）
        // 第１引数: ファイルマッピングオブジェクトのサイズ（バイト）
        // 第２引数: マッピングオブジェクトの名前
        public SharedMemory(Int32 size, String name)
        {
            // ファイルマッピングオブジェクト生成
            // （要するに、メモリマップドファイルを管理制御するための
            //   根本的なオブジェクトを生成します）
            CreateSharedMemory(size, name);
        }
 
        //========================
        // コンストラクタ＜マッピングオブジェクトをオープン＞
        //========================

        // ==============
        // コンストラクタ
        // ファイルマッピングオブジェクトをオープンする。
        // かつ、
        // プロセスのアドレス空間に、ファイルのビューをマップする。
        // 第１引数: マッピングオブジェクトの名前
        // 第２引数: 読み込みか、書き込みかの指示（true: 読み込み / false: 書き込み）
        public SharedMemory(String name, bool ReadOnly)
        {
            // ファイルマッピングオブジェクト生成
            // （要するに、メモリマップドファイルを管理制御するための
            //   根本的なオブジェクトを生成します）
            OpenSharedMemory(name);

            // 「読み込み」が指定されている場合
            if (ReadOnly == true)
            {
                // マップビューオープン＜読み込み用＞
                OpenMapView_Read();
            }
            // 「書き込み」が指定されている場合
            else
            {
                // マップビューオープン＜書き込み用＞
                OpenMapView_Write();
            }
        }

        // ==============
        // コンストラクタ
        // ファイルマッピングオブジェクトをオープンする。
        // なお、プロセスのアドレス空間に、ファイルのビューをマップしない。
        // （後に、ファイルのビューをマップします）
        // 第１引数: マッピングオブジェクトの名前
        public SharedMemory(String name)
        {
            // ファイルマッピングオブジェクト生成
            // （要するに、メモリマップドファイルを管理制御するための
            //   根本的なオブジェクトを生成します）
            OpenSharedMemory(name);
        }
 
        //========================
        // ファイナライザ
        //========================

        // ==============
        // ファイナライザ
        ~SharedMemory()
        {
            // メモリマップドファイルに関連するオブジェクトのクリーン
            // アップ（後始末）をする。
            cleanup();
        }

    }
    // ************************************
    // 共有メモリ管理クラス
    // ************************************
    public partial class SharedMemory : IDisposable
    {

        // ************************************
        // メンバーメソッド
        // ************************************

        // ==============
        // ファイルマッピングオブジェクト生成
        // ファイルマッピングオブジェクトを生成します。
        // なお、CreateFileMapping()で、ファイルマッピングオブジェクトが既に生
        // 成済みの場合は、本メソッドではなく、関連メソッドのOpenSharedMemory()
        // を実行して下さい。すなわち、関連メソッドのほうは、ファイルマッピング
        // オブジェクトの生成ではなく、オープンになります。
        // 第１引数: ファイルマッピングオブジェクトのサイズ（バイト）
        // 第２引数: マッピングオブジェクトの名前
        public void CreateSharedMemory(Int32 size, String SharedMemoryName)
        {
            // ファイルマッピングオブジェクトの生成
            // （要するに、メモリマップドファイルを管理制御するための
            //   根本的なオブジェクトを生成します）
            m_hFileMap = CreateFileMapping(
                InvalidHandleValue,
                IntPtr.Zero,
                FileProtection.PAGE_READWRITE,
                0, unchecked((UInt32)size), SharedMemoryName);

            // 失敗時の処理
            if (m_hFileMap == IntPtr.Zero)
                throw new Exception("Open/create error: " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());

            // マッピングオブジェクトの名前をメンバー変数へ設定
            m_name = SharedMemoryName;

            // マッピングオブジェクトのサイズをメンバー変数へ設定
            MaximumSize = size;
        }

        // ==============
        // ファイルマッピングオブジェクトオープン
        // ファイルマッピングオブジェクトをオープンします。
        // CreateFileMapping()で、ファイルマッピングオブジェクトが既に生成済み
        // の場合は、本メソッドでファイルマッピングオブジェクトをオープンします。
        // これに対して、ファイルマッピングオブジェクトがまだ生成していない場合
        // は、本メソッドではなく、関連メソッドのCreateSharedMemory()を実行して
        // 下さい。
        // 第１引数: マッピングオブジェクトの名前
        public void OpenSharedMemory(String SharedMemoryName)
        {
            // ファイルマッピングオブジェクトのオープン
            // （要するに、メモリマップドファイルを管理制御するための
            //   根本的なオブジェクトをオープンします）
            m_hFileMap = OpenFileMapping(
                FileMapAccess.FILE_MAP_ALL_ACCESS, false, SharedMemoryName);

            // 失敗時の処理
            if (m_hFileMap == IntPtr.Zero)
                throw new Exception("Open/create error: " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());

            // マッピングオブジェクトの名前をメンバー変数へ設定
            m_name = SharedMemoryName;
        }


        // ==============
        // マップビューオープン＜書き込み用＞
        // プロセスのアドレス空間に、ファイルのビューをマップします。
        // （要するに、メモリマップドファイルによるメモリ領域にアクセス
        //  が可能な状態にする）
        public void OpenMapView_Write()
        {
            // 前に使っていたマップビューの解放を忘れていた場合
            if (m_address != IntPtr.Zero)
            {
                // マップビューのクローズ
                CloseMapView();
            }

            // プロセスのアドレス空間に、ファイルのビューをマップする。
            m_address = MapViewOfFile(m_hFileMap,
                FileMapAccess.FILE_MAP_WRITE,
                0, 0, IntPtr.Zero);

            // 失敗時の処理
            if (m_address == IntPtr.Zero)
                throw new Exception("MapViewOfFile error: " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
        }


        // ==============
        // マップビューオープン＜読み込み用＞
        // プロセスのアドレス空間に、ファイルのビューをマップします。
        // （要するに、メモリマップドファイルによるメモリ領域にアクセス
        //  が可能な状態にする）
        public void OpenMapView_Read()
        {
            // 前に使っていたマップビューの解放を忘れていた場合
            if (m_address != IntPtr.Zero)
            {
                // マップビューのクローズ
                CloseMapView();
            }

            // プロセスのアドレス空間に、ファイルのビューをマップする。
            m_address = MapViewOfFile(m_hFileMap,
                FileMapAccess.FILE_MAP_READ,
                0, 0, IntPtr.Zero);

            // 失敗時の処理
            if (m_address == IntPtr.Zero)
                throw new Exception("MapViewOfFile error: " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
        }

        // ==============
        // マップビュークローズ
        // ファイルのビューを、プロセスのアドレス空間から解放します。
        // （今まで、メモリマップドファイルによるメモリ領域にアクセス
        //  が可能な状態であったならば、そのアクセス終了時に行う処理
        //  です）
        public void CloseMapView()
        {
            if (m_address != IntPtr.Zero)
            {
                // ファイルのビューを、プロセスのアドレス空間から解放する。
                UnmapViewOfFile(m_address);
                m_address = IntPtr.Zero;
            }
        }

        // ==============
        // クリーンアップ
        //  --- メモリマップドファイルに関連するオブジェクトのクリーン
        //      アップ（後始末）をするメソッド
        public void cleanup()
        {
            if (m_address != IntPtr.Zero)
            {
                UnmapViewOfFile(m_address);
                m_address = IntPtr.Zero;
            }

            if (m_hFileMap != IntPtr.Zero)
            {
                CloseHandle(m_hFileMap);
                m_hFileMap = IntPtr.Zero;
            }
        }

        // ==============
        // アンマネージ リソース解放メソッド
        //  --- アンマネージ（非.NET）のリソースの解放を定義します。
        //      ここで、「アンマネージのリソース」とは、メモリマップドフ
        //      ァイルに関連するオブジェクトのことです。
        // -------
        // IDisposableインターフェイスを実装した場合は、本メソッドを必ず
        // 定義するように定められています。
        public void Dispose()
        {
            // クリーンアップ
            cleanup();
        }

    }
}