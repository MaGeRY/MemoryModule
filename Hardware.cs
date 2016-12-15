using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System
{
    public class Hardware
    {
        private static byte[] HWID;
        private static byte[] _MD5;
        private static byte[] _SHA1;
        private static byte[] _SHA256;
        private static byte[] _SHA512;

        public static byte[] BIOS_RSMB { get; private set; }
        public static byte[] BIOS_FIRM { get; private set; }
        public static byte[] BIOS_ACPI { get; private set; }

        #region [extern] Native
        private static class Native
        {
            private const string USER32_DLL = "USER32.dll";
            private const string ADVAPI_DLL = "ADVAPI32.dll";
            private const string KERNEL32_DLL = "KERNEL32.DLL";

            [DllImport(USER32_DLL, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr CallWindowProcW([In] Byte[] bytes, IntPtr hWnd, Int32 msg, [In, Out] Byte[] wParam, IntPtr lParam);

            [DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool VirtualProtect([In] Byte[] bytes, IntPtr size, Int32 newProtect, out Int32 oldProtect);

            [DllImport(KERNEL32_DLL, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern SafeFileHandle CreateFileW([MarshalAs(UnmanagedType.LPWStr)]string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

            [DllImport(KERNEL32_DLL, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool GetVolumeNameForVolumeMountPoint(String mountPoint, StringBuilder name, UInt32 bufferLength);

            [DllImport(KERNEL32_DLL, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
            public static extern bool DeviceIoControl(SafeFileHandle hHandle, uint dwIoControlCode, [MarshalAs(UnmanagedType.AsAny)][In] object lpInBuffer, int lpInBufferSize, [Out] IntPtr lpOutBuffer, [Out] int lpOutBufferSize, ref uint lpBytesReturned, int lpOverlapped);

            [DllImport(KERNEL32_DLL, SetLastError = true)]
            public static extern bool DeviceIoControl(SafeFileHandle hHandle, uint dwIoControlCode, IntPtr lpInBuffer, int lpInBufferSize, [Out] IntPtr lpOutBuffer, [Out] int lpOutBufferSize, ref uint lpBytesReturned, int lpOverlapped);

            [DllImport(KERNEL32_DLL, CallingConvention = CallingConvention.StdCall)]
            public static extern int CloseHandle(int hObject);

            [DllImport(KERNEL32_DLL, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern int EnumSystemFirmwareTables(BiosFirmwareTableProvider providerSignature, IntPtr firmwareTableBuffer, int bufferSize);

            [DllImport(KERNEL32_DLL, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern int GetSystemFirmwareTable(BiosFirmwareTableProvider providerSignature, int dwFirmwareTableID, IntPtr lpTableBuffer, int dwBufferSize);
        }
        #endregion

        // CPU //
        #region [enum] BiosFirmwareTableProvider
        public enum BiosFirmwareTableProvider : int
        {
            ACPI = (byte)'A' << 24 | (byte)'C' << 16 | (byte)'P' << 8 | (byte)'I',
            FIRM = (byte)'F' << 24 | (byte)'I' << 16 | (byte)'R' << 8 | (byte)'M',
            RSMB = (byte)'R' << 24 | (byte)'S' << 16 | (byte)'M' << 8 | (byte)'B'
        }
        #endregion   
             
        #region [enum] SMBIOSTableType
        public enum SMBIOSTableType : sbyte
        {
            BIOSInformation             = 0,
            SystemInformation           = 1,
            BaseBoardInformation        = 2,
            EnclosureInformation        = 3,
            ProcessorInformation        = 4,
            MemoryControllerInformation = 5,
            MemoryModuleInformation     = 6,
            CacheInformation            = 7,
            PortConnectorInformation    = 8,
            SystemSlotsInformation      = 9,
            OnBoardDevicesInformation   = 10,
            OEMStrings                  = 11,
            SystemConfigurationOptions  = 12,
            BIOSLanguageInformation     = 13,
            GroupAssociations           = 14,
            SystemEventLog              = 15,
            PhysicalMemoryArray         = 16,
            MemoryDevice                = 17,
            MemoryErrorInformation      = 18,
            MemoryArrayMappedAddress    = 19,
            MemoryDeviceMappedAddress   = 20,
            EndofTable                  = 127
        }
        #endregion

        #region [class] SMBIOSTableEntry
        public class SMBIOSTableEntry
        {
            private int length;

            public SMBIOSTableHeader Header;
            public byte[] Buffer;            
            public int Index;

            #region [this] Length
            public int Length
            {
                get
                {
                    if (length < 1)
                    {
                        if (this.Buffer != null && this.Index > 0 && this.Buffer.Length > this.Index)
                        {
                            length = Index + Header.Length;
                            while (++length < Buffer.Length)
                            {
                                if (BIOS_RSMB[length - 1] == 0 && BIOS_RSMB[length] == 0)
                                {
                                    break;
                                }
                            };
                            length -= Index;
                        }
                    }
                    return length;
                }
            }
            #endregion

            public SMBIOSTableEntry(byte[] buffer, int index)
            {
                this.Header = buffer.ToStruct<SMBIOSTableHeader>(index);
                this.Buffer = buffer;
                this.Index = index;                
            }            

            #region [public] GetString(int index)
            public string GetString(int index)
            {
                if (index > 0)
                {
                    int i = 0;
                    int pos = this.Index + this.Header.Length;

                    do
                    {
                        string result = this.Buffer.GetString(pos);
                        if (++i == index)
                        {
                            return result;
                        }
                        pos += result.Length;
                    }
                    while (this.Buffer[++pos] != 0);
                }

                return string.Empty;
            }
            #endregion
        }
        #endregion

        #region [struct] RawSMBIOSData
        [StructLayout(LayoutKind.Sequential)]
        public struct RawSMBIOSData
        {
            public byte Used20CallingMethod;
            public byte MajorVersion;
            public byte MinorVersion;
            public byte DmiRevision;
            public uint Length;
        }
        #endregion

        #region [struct] SMBIOSTableHeader
        [StructLayout(LayoutKind.Sequential)]
        public struct SMBIOSTableHeader
        {
            public SMBIOSTableType Type;
            public byte Length;
            public ushort Handle;
        }
        #endregion

        #region [struct] SMBIOSTableInfo
        [StructLayout(LayoutKind.Sequential)]
        public struct SMBIOSTableBiosInfo
        {
            public SMBIOSTableHeader header;
            public byte vendor;
            public byte version;
            public ushort startingSegment;
            public byte releaseDate;
            public byte biosRomSize;
            public ulong characteristics;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] extensionBytes;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SMBIOSTableSystemInfo
        {
            public SMBIOSTableHeader header;
            public byte manufacturer;
            public byte productName;
            public byte version;
            public byte serialNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] UUID;
            public byte wakeUpType;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SMBIOSTableBaseBoardInfo
        {
            public SMBIOSTableHeader header;
            public byte manufacturer;
            public byte productName;
            public byte version;
            public byte serialNumber;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SMBIOSTableEnclosureInfo
        {
            public SMBIOSTableHeader header;
            public byte manufacturer;
            public byte type;
            public byte version;
            public byte serialNumber;
            public byte assetTagNumber;
            public byte bootUpState;
            public byte powerSupplyState;
            public byte thermalState;
            public byte securityStatus;
            public long OEM_Defined;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SMBIOSTableProcessorInfo
        {
            public SMBIOSTableHeader header;
            public byte socketDesignation;
            public byte processorType;
            public byte processorFamily;
            public byte processorManufacturer;
            public ulong processorID;
            public byte processorVersion;
            public byte processorVoltage;
            public ushort externalClock;
            public ushort maxSpeed;
            public ushort currentSpeed;
            public byte status;
            public byte processorUpgrade;
            public ushort L1CacheHandler;
            public ushort L2CacheHandler;
            public ushort L3CacheHandler;
            public byte serialNumber;
            public byte assetTag;
            public byte partNumber;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SMBIOSTableCacheInfo
        {
            public SMBIOSTableHeader header;
            public byte socketDesignation;
            public long cacheConfiguration;
            public ushort maximumCacheSize;
            public ushort installedSize;
            public ushort supportedSRAMType;
            public ushort currentSRAMType;
            public byte cacheSpeed;
            public byte errorCorrectionType;
            public byte systemCacheType;
            public byte associativity;
        }
        #endregion

        #region [private] GetTable(BiosFirmwareTableProvider provider, string/int table)
        private static byte[] GetTable(BiosFirmwareTableProvider provider, string table)
        {
            int id = table[3] << 24 | table[2] << 16 | table[1] << 8 | table[0];
            return GetTable(provider, id);
        }

        private static byte[] GetTable(BiosFirmwareTableProvider provider, int table)
        {
            Byte[] Result = new Byte[0];
            try
            {
                int sizeNeeded = Native.GetSystemFirmwareTable(provider, table, IntPtr.Zero, 0);
                if (sizeNeeded > 0)
                {
                    IntPtr bufferPtr = Marshal.AllocHGlobal(sizeNeeded);
                    Native.GetSystemFirmwareTable(provider, table, bufferPtr, sizeNeeded);
                    if (Marshal.GetLastWin32Error() == 0)
                    {
                        Result = new Byte[sizeNeeded];
                        Marshal.Copy(bufferPtr, Result, 0, sizeNeeded);
                    }
                    Marshal.FreeHGlobal(bufferPtr);
                }
            }
            catch { }
            return Result;
        }
        #endregion

        #region [private] EnumerateTables(BiosFirmwareTableProvider provider)
        private static string[] EnumerateTables(BiosFirmwareTableProvider provider)
        {
            string[] Result = new string[0];
            try
            {
                int sizeNeeded = Native.EnumSystemFirmwareTables(provider, IntPtr.Zero, 0);
                if (sizeNeeded > 0)
                {
                    byte[] buffer = new byte[sizeNeeded];
                    IntPtr bufferPtr = Marshal.AllocHGlobal(sizeNeeded);
                    Native.EnumSystemFirmwareTables(provider, bufferPtr, sizeNeeded);
                    if (Marshal.GetLastWin32Error() == 0)
                    {
                        Result = new string[sizeNeeded / 4];
                        Marshal.Copy(bufferPtr, buffer, 0, sizeNeeded);
                        for (int i = 0; i < Result.Length; i++) Result[i] = Encoding.ASCII.GetString(buffer, 4 * i, 4);
                    }
                    Marshal.FreeHGlobal(bufferPtr);
                }
            }
            catch { }
            return Result;
        }
        #endregion        

        #region [public] Calculate()
        public static void Calculate()
        {
            if (HWID != null && HWID.Length > 0) return;

            using (MemoryStream memory = new MemoryStream())
            {
                using (BinaryWriter Binary = new BinaryWriter(memory))
                {
                    // Get BIOS Information : SMBIOS //
                    string[] RSMB_Tables = EnumerateTables(BiosFirmwareTableProvider.RSMB);

                    if (RSMB_Tables.Length > 0)
                    {
                        BIOS_RSMB = GetTable(BiosFirmwareTableProvider.RSMB, RSMB_Tables[0]);

                        int entryIndex = Marshal.SizeOf(typeof(RawSMBIOSData));                        

                        if (BIOS_RSMB.Length >= entryIndex)
                        {
                            RawSMBIOSData SMBIOS = BIOS_RSMB.ToStruct<RawSMBIOSData>();
                            List<SMBIOSTableEntry> SMBIOS_Tables = new List<SMBIOSTableEntry>();

                            #region [ Search SMBIOS Entries of Tables ]
                            for (int pos = entryIndex; pos < BIOS_RSMB.Length; pos++)
                            {
                                SMBIOSTableType tableType = (SMBIOSTableType)BIOS_RSMB[pos];
                                if (tableType == SMBIOSTableType.EndofTable) break;

                                SMBIOSTableEntry tableEntry = new SMBIOSTableEntry(BIOS_RSMB, pos);                                                              
                                SMBIOS_Tables.Add(tableEntry);
                                pos += tableEntry.Length;
                            }
                            #endregion

                            // Write SMBIOS Data //
                            Binary.Write(SMBIOS.Serialize());

                            //Log.Console("SMBIOS v"+SMBIOS.MajorVersion+"."+SMBIOS.MinorVersion+"."+SMBIOS.DmiRevision+" (Length: "+SMBIOS.Length+" bytes)");
                            //Log.Console($"SMBIOS->Tables Total: {SMBIOS_Tables.Count}");                            

                            foreach (var table in SMBIOS_Tables)
                            {
                                //Log.Console($"SMBIOS->{table.Header.Type} (Length: {table.Length} bytes)");
                                switch (table.Header.Type)
                                {
                                    case SMBIOSTableType.BIOSInformation:
                                        Binary.Write(table.Buffer, table.Index, table.Length);
                                        //SMBIOSTableBiosInfo biosInfo = BIOS_RSMB.ToStruct<SMBIOSTableBiosInfo>(table.Index);
                                        //Log.Console($"Vendor: {table.GetString(biosInfo.vendor)}");
                                        //Log.Console($"Version: {table.GetString(biosInfo.version)}");
                                        //Log.Console($"Release Date: {table.GetString(biosInfo.releaseDate)}");
                                    break;
                                    case SMBIOSTableType.SystemInformation:
                                        Binary.Write(table.Buffer, table.Index, table.Length);
                                        //SMBIOSTableSystemInfo systemInfo = BIOS_RSMB.ToStruct<SMBIOSTableSystemInfo>(table.Index);
                                        //Log.Console($"Manufacturer: {table.GetString(systemInfo.manufacturer)}");
                                        //Log.Console($"Product Name: {table.GetString(systemInfo.productName)}");
                                        //Log.Console($"Serial Number: {table.GetString(systemInfo.serialNumber)}");
                                        //Log.Console($"Version: {table.GetString(systemInfo.version)}");
                                    break;
                                    case SMBIOSTableType.BaseBoardInformation:
                                        Binary.Write(table.Buffer, table.Index, table.Length);
                                        //SMBIOSTableBaseBoardInfo baseBoardInfo = BIOS_RSMB.ToStruct<SMBIOSTableBaseBoardInfo>(table.Index);
                                    break;
                                    case SMBIOSTableType.EnclosureInformation:
                                        Binary.Write(table.Buffer, table.Index, table.Length);
                                        //SMBIOSTableEnclosureInfo enclosureInfo = BIOS_RSMB.ToStruct<SMBIOSTableEnclosureInfo>(table.Index);
                                    break;
                                    case SMBIOSTableType.ProcessorInformation:
                                        Binary.Write(table.Buffer, table.Index, table.Length);
                                        //SMBIOSTableProcessorInfo processorInfo = BIOS_RSMB.ToStruct<SMBIOSTableProcessorInfo>(table.Index);
                                        //Log.Console($"Manufacturer: {table.GetString(processorInfo.processorManufacturer)}");
                                        //Log.Console($"ProcessorID: {processorInfo.processorID}");
                                        //Log.Console($"Version: {table.GetString(processorInfo.processorVersion)}");
                                        //Log.Console($"Family: {table.GetString(processorInfo.processorFamily)}");
                                        //Log.Console($"Socket: {table.GetString(processorInfo.socketDesignation)}");
                                        //Log.Console($"Type: {table.GetString(processorInfo.processorType)}");
                                    break;
                                }
                            }                            
                        }                        
                    }

                    /*
                    // Get BIOS Information : Firmware //
                    string[] FIRM_Tables = EnumerateTables(BiosFirmwareTableProvider.FIRM);
                    if (FIRM_Tables.Length > 0)
                    {
                        BIOS_FIRM = GetTable(BiosFirmwareTableProvider.FIRM, FIRM_Tables[0]);
                        int length = BIOS_FIRM.Length;
                        if (length > 0x400) length = 0x400;
                        if (length > 0) Binary.Write(BIOS_FIRM, 0, length);                        
                    }

                    // Get BIOS Information : ACPI //
                    string[] ACPI_Tables = EnumerateTables(BiosFirmwareTableProvider.ACPI);
                    if (ACPI_Tables.Length > 0)
                    {
                        BIOS_ACPI = GetTable(BiosFirmwareTableProvider.ACPI, ACPI_Tables[0]);
                        int length = BIOS_ACPI.Length;
                        if (length > 0x400) length = 0x400;
                        if (length > 0) Binary.Write(BIOS_ACPI, 0, length);
                    }
                    */

                    // Get bytes from stream of harware id //
                    HWID = memory.ToArray();

                    // Compute hashes //
                    if (HWID.Length > 0)
                    {
                        _MD5 = new MD5CryptoServiceProvider().ComputeHash(HWID);
                        _SHA1 = new SHA1CryptoServiceProvider().ComputeHash(HWID);
                        _SHA256 = new SHA256CryptoServiceProvider().ComputeHash(HWID);
                        _SHA512 = new SHA512CryptoServiceProvider().ComputeHash(HWID);
                    }
                }
            }
        }
        #endregion

        public static byte[] AsBytes
        {
            get
            {
                Calculate();
                return HWID;
            }
        }

        public static byte[] MD5
        {
            get
            {
                Calculate();
                if (_MD5 == null) return new byte[0];
                return _MD5;
            }
        }

        public static byte[] SHA1
        {
            get
            {
                Calculate();
                if (_SHA1 == null) return new byte[0];
                return _SHA1;
            }
        }

        public static byte[] SHA256
        {
            get
            {
                Calculate();
                if (_SHA256 == null) return new byte[0];
                return _SHA256;
            }
        }

        public static byte[] SHA512
        {
            get
            {
                Calculate();
                if (_SHA512 == null) return new byte[0];
                return _SHA512;
            }
        }

        public static string MD5String
        {
            get
            {
                Calculate();
                if (_MD5 == null) return "";
                return BitConverter.ToString(_MD5, 0).Replace("-", "").ToLower();
            }
        }
        public static string SHA1String
        {
            get
            {
                Calculate();
                if (_SHA1 == null) return "";
                return BitConverter.ToString(_SHA1, 0).Replace("-", "").ToLower();
            }
        }
        public static string SHA256String
        {
            get
            {
                Calculate();
                if (_SHA256 == null) return "";
                return BitConverter.ToString(_SHA256, 0).Replace("-", "").ToLower();
            }
        }
        public static string SHA512String
        {
            get
            {
                Calculate();
                if (_SHA512 == null) return "";
                return BitConverter.ToString(_SHA512, 0).Replace("-", "").ToLower();
            }
        }
    }
}
