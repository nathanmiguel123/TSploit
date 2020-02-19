﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TheSuper_Injector
{
	public class TheSuperAPI
	{
		// Token: 0x06000001 RID: 1
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool WaitNamedPipe(string name, int timeout);

		// Token: 0x06000002 RID: 2 RVA: 0x00002050 File Offset: 0x00000250
		public static bool NamedPipeExist(string pipeName)
		{
			bool result;
			try
			{
				int timeout = 0;
				if (!TheSuperAPI.WaitNamedPipe(Path.GetFullPath(string.Format("\\\\.\\pipe\\{0}", pipeName)), timeout))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (lastWin32Error == 0)
					{
						return false;
					}
					if (lastWin32Error == 2)
					{
						return false;
					}
				}
				result = true;
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000020A8 File Offset: 0x000002A8
		private void SMTP(string pipe, string input)
		{
			if (TheSuperAPI.NamedPipeExist(pipe))
			{
				try
				{
					using (NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", pipe, PipeDirection.Out))
					{
						namedPipeClientStream.Connect();
						using (StreamWriter streamWriter = new StreamWriter(namedPipeClientStream))
						{
							streamWriter.Write(input);
							streamWriter.Dispose();
						}
						namedPipeClientStream.Dispose();
					}
					return;
				}
				catch (IOException)
				{
					MessageBox.Show("Error occured sending message to the game!", "Connection Failed!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message.ToString());
					return;
				}
			}
			MessageBox.Show("Error occured. Did the dll properly inject?", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002174 File Offset: 0x00000374
		private string ReadURL(string url)
		{
			return this.client.DownloadString(url);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002184 File Offset: 0x00000384
		private string GetLatestData()
		{
			string text = this.ReadURL("https://cdn.wearedevs.net/software/exploitapi/latestdata.txt");
			if (text.Length > 0)
			{
				return text;
			}
			string text2 = this.ReadURL("https://pastebin.com/raw/Ly9mJwH7");
			if (text2.Length > 0)
			{
				return text2;
			}
			return "";
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000021C4 File Offset: 0x000003C4
		public bool IsUpdated()
		{
			bool result = false;
			string latestData = this.GetLatestData();
			if (latestData.Length > 0)
			{
				result = Convert.ToBoolean(latestData.Split(new char[]
				{
					' '
				})[0]);
			}
			else
			{
				MessageBox.Show("Could not check for the latest version. Did your fireall block us?", "Error");
			}
			return result;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002210 File Offset: 0x00000410
		private bool DownloadLatestVersion()
		{
			if (File.Exists("exploit-main.dll"))
			{
				File.Delete("exploit-main.dll");
			}
			string latestData = this.GetLatestData();
			if (latestData.Length > 0)
			{
				this.client.DownloadFile(latestData.Split(new char[]
				{
					' '
				})[1], "exploit-main.dll");
			}
			return File.Exists("exploit-main.dll");
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002275 File Offset: 0x00000475
		public bool isAPIAttached()
		{
			return TheSuperAPI.NamedPipeExist(this.cmdpipe);
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002288 File Offset: 0x00000488
		public bool LaunchExploit()
		{
			if (TheSuperAPI.NamedPipeExist(this.cmdpipe))
			{
				MessageBox.Show("Dll already injected", "No problems");
			}
			else if (this.IsUpdated())
			{
				if (this.DownloadLatestVersion())
				{
					if (this.injector.InjectDLL())
					{
						return true;
					}
					MessageBox.Show("DLL failed to inject", "Error");
				}
				else
				{
					MessageBox.Show("Could not download the latest version! Did your firewall block us?", "Error");
				}
			}
			else
			{
				MessageBox.Show("Exploit is currently patched... Please wait for the developers to fix it! Meanwhile, check wearedevs.net for updates/info.", "Error");
			}
			return false;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002308 File Offset: 0x00000508
		public void SendCommand(string Command)
		{
			this.SMTP(this.cmdpipe, Command);
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002317 File Offset: 0x00000517
		[Obsolete("SendScript is deprecated, please use SendLuaCScript instead.")]
		public void SendScript(string script)
		{
			this.SendLuaCScript(script);
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002320 File Offset: 0x00000520
		public void SendLuaCScript(string Script)
		{
			foreach (string input in Script.Split("\r\n".ToCharArray()))
			{
				try
				{
					this.SMTP(this.luacpipe, input);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message.ToString());
				}
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002384 File Offset: 0x00000584
		[Obsolete("SendLimitedLuaScript is deprecated, please use SendLuaScript instead.")]
		public void SendLimitedLuaScript(string script)
		{
			this.SendLuaScript(script);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x0000238D File Offset: 0x0000058D
		public void SendLuaScript(string Script)
		{
			this.SMTP(this.luapipe, Script);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x0000239C File Offset: 0x0000059C
		public void LuaC_getglobal(string service)
		{
			this.SendScript("getglobal " + service);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000023AF File Offset: 0x000005AF
		public void LuaC_getfield(int index, string instance)
		{
			this.SendScript(string.Concat(new object[]
			{
				"getglobal ",
				index,
				" ",
				instance
			}));
		}

		// Token: 0x06000011 RID: 17 RVA: 0x000023DF File Offset: 0x000005DF
		public void LuaC_setfield(int index, string property)
		{
			this.SendScript(string.Concat(new object[]
			{
				"setfield ",
				index,
				" ",
				property
			}));
		}

		// Token: 0x06000012 RID: 18 RVA: 0x0000240F File Offset: 0x0000060F
		public void LuaC_pushvalue(int index)
		{
			this.SendScript("pushvalue " + index);
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002427 File Offset: 0x00000627
		public void LuaC_pushstring(string text)
		{
			this.SendScript("pushstring " + text);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x0000243A File Offset: 0x0000063A
		public void LuaC_pushnumber(int number)
		{
			this.SendScript("pushnumber " + number);
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002454 File Offset: 0x00000654
		public void LuaC_pcall(int numberOfArguments, int numberOfResults, int ErrorFunction)
		{
			this.SendScript(string.Concat(new object[]
			{
				"pushnumber ",
				numberOfArguments,
				" ",
				numberOfResults,
				" ",
				ErrorFunction
			}));
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000024A5 File Offset: 0x000006A5
		public void LuaC_settop(int index)
		{
			this.SendScript("settop " + index);
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000024BD File Offset: 0x000006BD
		public void LuaC_pushboolean(string value = "false")
		{
			this.SendScript("pushboolean " + value);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000024D0 File Offset: 0x000006D0
		public void LuaC_gettop()
		{
			this.SendScript("gettop");
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000024DD File Offset: 0x000006DD
		public void LuaC_pushnil()
		{
			this.SendScript("pushnil");
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000024EA File Offset: 0x000006EA
		public void LuaC_next(int index)
		{
			this.SendScript("next");
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000024F7 File Offset: 0x000006F7
		public void LuaC_pop(int quantity)
		{
			this.SendScript("pop " + quantity);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000250F File Offset: 0x0000070F
		public void DoBTools(string username = "me")
		{
			this.SendCommand("btools " + username);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002522 File Offset: 0x00000722
		public void DoKill(string username = "me")
		{
			this.SendCommand("kill " + username);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002535 File Offset: 0x00000735
		public void CreateForceField(string username = "me")
		{
			this.SendCommand("ff " + username);
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002548 File Offset: 0x00000748
		public void RemoveForceField(string username = "me")
		{
			this.SendCommand("noff " + username);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000255B File Offset: 0x0000075B
		public void DoFloat(string username = "me")
		{
			this.SendCommand("float " + username);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000256E File Offset: 0x0000076E
		public void DoNoFloat(string username = "me")
		{
			this.SendCommand("nofloat " + username);
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002581 File Offset: 0x00000781
		public void RemoveLimbs(string username = "me")
		{
			this.SendCommand("nolimbs " + username);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002594 File Offset: 0x00000794
		public void RemoveArms(string username = "me")
		{
			this.SendCommand("noarms " + username);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000025A7 File Offset: 0x000007A7
		public void RemoveLegs(string username = "me")
		{
			this.SendCommand("nolegs " + username);
		}

		// Token: 0x06000025 RID: 37 RVA: 0x000025BA File Offset: 0x000007BA
		public void AddFire(string username = "me")
		{
			this.SendCommand("fire " + username);
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000025CD File Offset: 0x000007CD
		public void RemoveFire(string username = "me")
		{
			this.SendCommand("nofire " + username);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x000025E0 File Offset: 0x000007E0
		public void AddSparkles(string username = "me")
		{
			this.SendCommand("sparkles " + username);
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000025F3 File Offset: 0x000007F3
		public void RemoveSparkles(string username = "me")
		{
			this.SendCommand("nosparkles " + username);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002606 File Offset: 0x00000806
		public void AddSmoke(string username = "me")
		{
			this.SendCommand("smoke " + username);
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002619 File Offset: 0x00000819
		public void DoBlockHead(string username = "me")
		{
			this.SendCommand("blockhead " + username);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x0000262C File Offset: 0x0000082C
		public void ForceBubbleChat(string username = "me", string text = "WeAreDevs Website")
		{
			this.SendCommand("chat " + username + " " + text);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002645 File Offset: 0x00000845
		public void ConsolePrint(string text = "WeAreDevs Website")
		{
			this.SendCommand("print " + text);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002658 File Offset: 0x00000858
		public void ConsoleWarn(string text = "meWeAreDevs Website")
		{
			this.SendCommand("warn " + text);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x0000266B File Offset: 0x0000086B
		public void SetWalkSpeed(string username = "me", int value = 100)
		{
			this.SendCommand("speed " + username + " " + value.ToString());
		}

		// Token: 0x0600002F RID: 47 RVA: 0x0000268A File Offset: 0x0000088A
		public void ToggleClickTeleport()
		{
			this.SendCommand("toggleclickteleport");
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002697 File Offset: 0x00000897
		public void SetFogEnd(int value = 0)
		{
			this.SendCommand("fogend " + value);
		}

		// Token: 0x06000031 RID: 49 RVA: 0x000026AF File Offset: 0x000008AF
		public void SetJumpPower(int value = 100)
		{
			this.SendCommand("jumppower " + value);
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000026C7 File Offset: 0x000008C7
		public void TeleportMyCharacterTo(string target_username = "me")
		{
			this.SendCommand("teleport " + target_username);
		}

		// Token: 0x06000033 RID: 51 RVA: 0x000026DA File Offset: 0x000008DA
		public void PlaySoundInGame(string assetid = "1071384374")
		{
			this.SendCommand("music " + assetid);
		}

		// Token: 0x06000034 RID: 52 RVA: 0x000026ED File Offset: 0x000008ED
		public void SetSkyboxImage(string assetid = "2143522")
		{
			this.SendCommand("skybox " + assetid);
		}

		// Token: 0x04000001 RID: 1
		private WebClient client = new WebClient();

		// Token: 0x04000002 RID: 2
		private TheSuperAPI.BasicInject injector = new TheSuperAPI.BasicInject();

		// Token: 0x04000003 RID: 3
		private string cmdpipe = "WeAreDevsPublicAPI_CMD";

		// Token: 0x04000004 RID: 4
		private string luacpipe = "WeAreDevsPublicAPI_LuaC";

		// Token: 0x04000005 RID: 5
		private string luapipe = "WeAreDevsPublicAPI_Lua";

		// Token: 0x02000003 RID: 3
		private class BasicInject
		{
			// Token: 0x06000036 RID: 54
			[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
			internal static extern IntPtr LoadLibraryA(string lpFileName);

			// Token: 0x06000037 RID: 55
			[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
			internal static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

			// Token: 0x06000038 RID: 56
			[DllImport("kernel32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool FreeLibrary(IntPtr hModule);

			// Token: 0x06000039 RID: 57
			[DllImport("kernel32.dll")]
			internal static extern IntPtr OpenProcess(TheSuperAPI.BasicInject.ProcessAccess dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

			// Token: 0x0600003A RID: 58
			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
			internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

			// Token: 0x0600003B RID: 59
			[DllImport("kernel32.dll", SetLastError = true)]
			internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

			// Token: 0x0600003C RID: 60
			[DllImport("kernel32.dll")]
			internal static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, UIntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

			// Token: 0x0600003D RID: 61
			[DllImport("kernel32.dll", SetLastError = true)]
			internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

			// Token: 0x0600003E RID: 62 RVA: 0x00002740 File Offset: 0x00000940
			public bool InjectDLL()
			{
				if (Process.GetProcessesByName("RobloxPlayerBeta").Length == 0)
				{
					return false;
				}
				Process process = Process.GetProcessesByName("RobloxPlayerBeta")[0];
				byte[] bytes = new ASCIIEncoding().GetBytes(AppDomain.CurrentDomain.BaseDirectory + "exploit-main.dll");
				IntPtr hModule = TheSuperAPI.BasicInject.LoadLibraryA("kernel32.dll");
				UIntPtr procAddress = TheSuperAPI.BasicInject.GetProcAddress(hModule, "LoadLibraryA");
				TheSuperAPI.BasicInject.FreeLibrary(hModule);
				if (procAddress == UIntPtr.Zero)
				{
					return false;
				}
				IntPtr intPtr = TheSuperAPI.BasicInject.OpenProcess(TheSuperAPI.BasicInject.ProcessAccess.AllAccess, false, process.Id);
				if (intPtr == IntPtr.Zero)
				{
					return false;
				}
				IntPtr intPtr2 = TheSuperAPI.BasicInject.VirtualAllocEx(intPtr, (IntPtr)0, (uint)bytes.Length, 12288U, 4U);
				UIntPtr uintPtr;
				IntPtr intPtr3;
				return !(intPtr2 == IntPtr.Zero) && TheSuperAPI.BasicInject.WriteProcessMemory(intPtr, intPtr2, bytes, (uint)bytes.Length, out uintPtr) && !(TheSuperAPI.BasicInject.CreateRemoteThread(intPtr, (IntPtr)0, 0U, procAddress, intPtr2, 0U, out intPtr3) == IntPtr.Zero);
			}

			// Token: 0x02000004 RID: 4
			[Flags]
			public enum ProcessAccess
			{
				// Token: 0x04000007 RID: 7
				AllAccess = 1050235,
				// Token: 0x04000008 RID: 8
				CreateThread = 2,
				// Token: 0x04000009 RID: 9
				DuplicateHandle = 64,
				// Token: 0x0400000A RID: 10
				QueryInformation = 1024,
				// Token: 0x0400000B RID: 11
				SetInformation = 512,
				// Token: 0x0400000C RID: 12
				Terminate = 1,
				// Token: 0x0400000D RID: 13
				VMOperation = 8,
				// Token: 0x0400000E RID: 14
				VMRead = 16,
				// Token: 0x0400000F RID: 15
				VMWrite = 32,
				// Token: 0x04000010 RID: 16
				Synchronize = 1048576
			}
		}
	}
}
