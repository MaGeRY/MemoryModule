#pragma warning disable 0414
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System
{
    //public static ushort IMAGE_DOS_SIGNATURE = 0x5A4D;        

    #region [struct] IMAGE_SECTION_HEADER
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct IMAGE_SECTION_HEADER
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Name;
        //union 
        //{    
        //    DWORD PhysicalAddress;    
        //    DWORD VirtualSize;  
        //} Misc;
        public uint PhysicalAddress;
        //public uint VirtualSize;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLinenumbers;
        public short NumberOfRelocations;
        public short NumberOfLinenumbers;
        public uint Characteristics;
    }
    #endregion

    #region [struct] IMAGE_DOS_HEADER
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct IMAGE_DOS_HEADER
    {
        public UInt16 e_magic;       // Magic number
        public UInt16 e_cblp;        // Bytes on last page of file
        public UInt16 e_cp;          // Pages in file
        public UInt16 e_crlc;        // Relocations
        public UInt16 e_cparhdr;     // Size of header in paragraphs
        public UInt16 e_minalloc;    // Minimum extra paragraphs needed
        public UInt16 e_maxalloc;    // Maximum extra paragraphs needed
        public UInt16 e_ss;          // Initial (relative) SS value
        public UInt16 e_sp;          // Initial SP value
        public UInt16 e_csum;        // Checksum
        public UInt16 e_ip;          // Initial IP value
        public UInt16 e_cs;          // Initial (relative) CS value
        public UInt16 e_lfarlc;      // File address of relocation table
        public UInt16 e_ovno;        // Overlay number
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt16[] e_res1;        // Reserved words
        public UInt16 e_oemid;       // OEM identifier (for e_oeminfo)
        public UInt16 e_oeminfo;     // OEM information; e_oemid specific
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public UInt16[] e_res2;        // Reserved words
        public Int32 e_lfanew;      // File address of new exe header
    }
    #endregion    

    #region [struct] IMAGE_FILE_HEADER
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct IMAGE_FILE_HEADER
    {
        public UInt16 Machine;
        public UInt16 NumberOfSections;
        public UInt32 TimeDateStamp;
        public UInt32 PointerToSymbolTable;
        public UInt32 NumberOfSymbols;
        public UInt16 SizeOfOptionalHeader;
        public UInt16 Characteristics;
    }
    #endregion

    #region [struct] IMAGE_DATA_DIRECTORY
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct IMAGE_DATA_DIRECTORY
    {
        public UInt32 VirtualAddress;
        public UInt32 Size;
    }
    #endregion

    #region [struct] IMAGE_OPTIONAL_HEADER32
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IMAGE_OPTIONAL_HEADER32
    {
        public UInt16 Magic;
        public Byte MajorLinkerVersion;
        public Byte MinorLinkerVersion;
        public UInt32 SizeOfCode;
        public UInt32 SizeOfInitializedData;
        public UInt32 SizeOfUninitializedData;
        public UInt32 AddressOfEntryPoint;
        public UInt32 BaseOfCode;
        public UInt32 BaseOfData;
        public UInt32 ImageBase;
        public UInt32 SectionAlignment;
        public UInt32 FileAlignment;
        public UInt16 MajorOperatingSystemVersion;
        public UInt16 MinorOperatingSystemVersion;
        public UInt16 MajorImageVersion;
        public UInt16 MinorImageVersion;
        public UInt16 MajorSubsystemVersion;
        public UInt16 MinorSubsystemVersion;
        public UInt32 Win32VersionValue;
        public UInt32 SizeOfImage;
        public UInt32 SizeOfHeaders;
        public UInt32 CheckSum;
        public UInt16 Subsystem;
        public UInt16 DllCharacteristics;
        public UInt32 SizeOfStackReserve;
        public UInt32 SizeOfStackCommit;
        public UInt32 SizeOfHeapReserve;
        public UInt32 SizeOfHeapCommit;
        public UInt32 LoaderFlags;
        public UInt32 NumberOfRvaAndSizes;

        public IMAGE_DATA_DIRECTORY ExportTable;
        public IMAGE_DATA_DIRECTORY ImportTable;
        public IMAGE_DATA_DIRECTORY ResourceTable;
        public IMAGE_DATA_DIRECTORY ExceptionTable;
        public IMAGE_DATA_DIRECTORY CertificateTable;
        public IMAGE_DATA_DIRECTORY BaseRelocationTable;
        public IMAGE_DATA_DIRECTORY Debug;
        public IMAGE_DATA_DIRECTORY Architecture;
        public IMAGE_DATA_DIRECTORY GlobalPtr;
        public IMAGE_DATA_DIRECTORY TLSTable;
        public IMAGE_DATA_DIRECTORY LoadConfigTable;
        public IMAGE_DATA_DIRECTORY BoundImport;
        public IMAGE_DATA_DIRECTORY IAT;
        public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
        public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
        public IMAGE_DATA_DIRECTORY Reserved;
    }
    #endregion

    #region [struct] IMAGE_OPTIONAL_HEADER64
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IMAGE_OPTIONAL_HEADER64
    {
        public UInt16 Magic;
        public Byte MajorLinkerVersion;
        public Byte MinorLinkerVersion;
        public UInt32 SizeOfCode;
        public UInt32 SizeOfInitializedData;
        public UInt32 SizeOfUninitializedData;
        public UInt32 AddressOfEntryPoint;
        public UInt32 BaseOfCode;
        public UInt64 ImageBase;
        public UInt32 SectionAlignment;
        public UInt32 FileAlignment;
        public UInt16 MajorOperatingSystemVersion;
        public UInt16 MinorOperatingSystemVersion;
        public UInt16 MajorImageVersion;
        public UInt16 MinorImageVersion;
        public UInt16 MajorSubsystemVersion;
        public UInt16 MinorSubsystemVersion;
        public UInt32 Win32VersionValue;
        public UInt32 SizeOfImage;
        public UInt32 SizeOfHeaders;
        public UInt32 CheckSum;
        public UInt16 Subsystem;
        public UInt16 DllCharacteristics;
        public UInt64 SizeOfStackReserve;
        public UInt64 SizeOfStackCommit;
        public UInt64 SizeOfHeapReserve;
        public UInt64 SizeOfHeapCommit;
        public UInt32 LoaderFlags;
        public UInt32 NumberOfRvaAndSizes;

        public IMAGE_DATA_DIRECTORY ExportTable;
        public IMAGE_DATA_DIRECTORY ImportTable;
        public IMAGE_DATA_DIRECTORY ResourceTable;
        public IMAGE_DATA_DIRECTORY ExceptionTable;
        public IMAGE_DATA_DIRECTORY CertificateTable;
        public IMAGE_DATA_DIRECTORY BaseRelocationTable;
        public IMAGE_DATA_DIRECTORY Debug;
        public IMAGE_DATA_DIRECTORY Architecture;
        public IMAGE_DATA_DIRECTORY GlobalPtr;
        public IMAGE_DATA_DIRECTORY TLSTable;
        public IMAGE_DATA_DIRECTORY LoadConfigTable;
        public IMAGE_DATA_DIRECTORY BoundImport;
        public IMAGE_DATA_DIRECTORY IAT;
        public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
        public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
        public IMAGE_DATA_DIRECTORY Reserved;
    }
    #endregion

    #region [struct] IMAGE_NT_HEADERS32
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_NT_HEADERS32
    {
        public UInt32 Signature;
        public IMAGE_FILE_HEADER FileHeader;
        public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
    }
    #endregion

    #region [struct] IMAGE_NT_HEADERS64
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_NT_HEADERS64
    {
        public UInt32 Signature;
        public IMAGE_FILE_HEADER FileHeader;
        public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
    }
    #endregion

    #region [struct] MEMORYMODULE32
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORYMODULE32
    {
        public IMAGE_NT_HEADERS32 headers;
        public IntPtr codeBase;
        public IntPtr modules;
        public int numModules;
        public int initialized;
    }
    #endregion

    #region [struct] MEMORYMODULE64
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORYMODULE64
    {
        public IMAGE_NT_HEADERS64 headers;
        public IntPtr codeBase;
        public IntPtr modules;
        public int numModules;
        public int initialized;
    }
    #endregion

    #region [struct] IMAGE_IMPORT_DESCRIPTOR
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_IMPORT_DESCRIPTOR
    {
        public uint CharacteristicsOrOriginalFirstThunk;
        public uint TimeDateStamp;
        public uint ForwarderChain;
        public uint Name;
        public uint FirstThunk;
    }
    #endregion

    #region [struct] IMAGE_EXPORT_DIRECTORY
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_EXPORT_DIRECTORY
    {
        public UInt32 Characteristics;
        public UInt32 TimeDateStamp;
        public UInt16 MajorVersion;
        public UInt16 MinorVersion;
        public UInt32 Name;
        public UInt32 Base;
        public UInt32 NumberOfFunctions;
        public UInt32 NumberOfNames;
        public UInt32 AddressOfFunctions;
        public UInt32 AddressOfNames;
        public UInt32 AddressOfNameOrdinals;
    }
    #endregion

    #region [struct] IMAGE_BASE_RELOCATION
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_BASE_RELOCATION
    {
        public uint VirtualAddress;
        public uint SizeOfBlock;
    }
    #endregion

    #region [struct] IMAGE_IMPORT_BY_NAME
    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_IMPORT_BY_NAME
    {
        public short Hint;
        public byte Name;
    }
    #endregion    

    public unsafe class MemoryModule
    {
        private static uint PAGE_NOACCESS          = 0x01;
        private static uint PAGE_READONLY          = 0x02;
        private static uint PAGE_READWRITE         = 0x04;
        private static uint PAGE_WRITECOPY         = 0x08;
        private static uint PAGE_EXECUTE           = 0x10;
        private static uint PAGE_EXECUTE_READ      = 0x20;
        private static uint PAGE_EXECUTE_READWRITE = 0x40;
        private static uint PAGE_EXECUTE_WRITECOPY = 0x80;
        private static uint MEM_COMMIT             = 0x1000;
        private static uint MEM_RESERVE            = 0x2000;
        private static uint MEM_RELEASE            = 0x8000;

        private readonly int[][][] ProtectionFlags = new int[2][][];

        private MEMORYMODULE32 module32;
        private MEMORYMODULE64 module64;

        [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
        private delegate bool DllEntry(IntPtr instance, uint reason, IntPtr reserved);        

        #region [ Native ]
        internal static class Native
        {
            private const string KERNEL32_DLL = "KERNEL32.DLL";            

            [DllImport(KERNEL32_DLL, SetLastError = true)]
            public static extern IntPtr LoadLibrary(string lpFileName);            

            [DllImport(KERNEL32_DLL, SetLastError = true)]
            public static extern IntPtr VirtualAlloc(IntPtr pStartAddr, uint size, uint flAllocationType, uint flProtect);            

            [DllImport(KERNEL32_DLL, SetLastError = true)]
            public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

            [DllImport(KERNEL32_DLL, SetLastError = true)]
            public static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

            [DllImport(KERNEL32_DLL, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr module, IntPtr ordinal);

            [DllImport(KERNEL32_DLL, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport(KERNEL32_DLL)]
            public static extern uint GetLastError();

        }
        #endregion        

        public bool Is64Bit { get; private set; }

        public MemoryModule(byte[] bytes)
        {
            IntPtr address = IntPtr.Zero;
            IntPtr dllEntryPoint = IntPtr.Zero;

            ProtectionFlags[0] = new int[2][];
            ProtectionFlags[1] = new int[2][];
            ProtectionFlags[0][0] = new int[2];
            ProtectionFlags[0][1] = new int[2];
            ProtectionFlags[1][0] = new int[2];
            ProtectionFlags[1][1] = new int[2];
            ProtectionFlags[0][0][0] = 0x01;
            ProtectionFlags[0][0][1] = 0x08;
            ProtectionFlags[0][1][0] = 0x02;
            ProtectionFlags[0][1][1] = 0x04;
            ProtectionFlags[1][0][0] = 0x10;
            ProtectionFlags[1][0][1] = 0x80;
            ProtectionFlags[1][1][0] = 0x20;
            ProtectionFlags[1][1][1] = 0x40;

            IMAGE_DOS_HEADER dosHeader = bytes.ToStruct<IMAGE_DOS_HEADER>();
            IMAGE_NT_HEADERS32 ntHeader = bytes.ToStruct<IMAGE_NT_HEADERS32>((uint)dosHeader.e_lfanew);

            Is64Bit = (ntHeader.OptionalHeader.Magic == 0x020B || ntHeader.FileHeader.Machine == 0x8664);

            #region [ Load x64 Machine Dynamic Link Library ]
            if (Is64Bit)
            {
                IMAGE_NT_HEADERS64 header64 = bytes.ToStruct<IMAGE_NT_HEADERS64>((uint)dosHeader.e_lfanew);

                address = Native.VirtualAlloc((IntPtr)header64.OptionalHeader.ImageBase, header64.OptionalHeader.SizeOfImage, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
                if (address == IntPtr.Zero) address = Native.VirtualAlloc(address, header64.OptionalHeader.SizeOfImage, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

                if (address != IntPtr.Zero)
                {
                    //Log.Console("Memory Address: 0x" + address.ToString("X16"));                                        

                    IntPtr headers = Native.VirtualAlloc(address, header64.OptionalHeader.SizeOfHeaders, MEM_COMMIT, PAGE_READWRITE);
                    Marshal.Copy(bytes, 0, headers, (int)(dosHeader.e_lfanew + header64.OptionalHeader.SizeOfHeaders));

                    CopySections64(bytes, address, header64, headers, dosHeader);

                    int moduleCount = GetModuleCount(address, header64.OptionalHeader.ImportTable);

                    module64 = new MEMORYMODULE64
                    {
                        codeBase = address,
                        numModules = moduleCount,
                        modules = Marshal.AllocHGlobal(moduleCount * sizeof(int)),
                        initialized = 0
                    };

                    module64.headers = headers.ToStruct<IMAGE_NT_HEADERS64>((uint)dosHeader.e_lfanew);
                    module64.headers.OptionalHeader.ImageBase = (ulong)address;                    

                    ulong locationDelta = (ulong)address - header64.OptionalHeader.ImageBase;
                    if (locationDelta != 0) PerformBaseRelocation64(locationDelta);

                    BuildImportTable64(module64);                    

                    FinalizeSections64(address, headers, dosHeader, header64);

                    dllEntryPoint = address.Add(header64.OptionalHeader.AddressOfEntryPoint);
                }
            }
            #endregion

            #region [ Load x86 Machine Dynamic Link Library ]
            else
            {
                IMAGE_NT_HEADERS32 header32 = bytes.ToStruct<IMAGE_NT_HEADERS32>((uint)dosHeader.e_lfanew);

                address = Native.VirtualAlloc((IntPtr)header32.OptionalHeader.ImageBase, header32.OptionalHeader.SizeOfImage, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
                if (address == IntPtr.Zero) address = Native.VirtualAlloc(address, header32.OptionalHeader.SizeOfImage, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

                if (address != IntPtr.Zero)
                {
                    //Log.Console("Memory Address: 0x" + address.ToString("X16"));                    

                    IntPtr headers = Native.VirtualAlloc(address, header32.OptionalHeader.SizeOfHeaders, MEM_COMMIT, PAGE_READWRITE);
                    Marshal.Copy(bytes, 0, headers, (int)(dosHeader.e_lfanew + header32.OptionalHeader.SizeOfHeaders));

                    CopySections32(bytes, address, header32, headers, dosHeader);

                    int moduleCount = GetModuleCount(address, header32.OptionalHeader.ImportTable);

                    module32 = new MEMORYMODULE32
                    {
                        codeBase = address,
                        numModules = 0,
                        modules = new IntPtr(0),
                        initialized = 0
                    };

                    module32.headers = headers.ToStruct<IMAGE_NT_HEADERS32>((uint)dosHeader.e_lfanew);
                    module32.headers.OptionalHeader.ImageBase = (uint)address;

                    ulong locationDelta = (ulong)address - header32.OptionalHeader.ImageBase;
                    if (locationDelta != 0) PerformBaseRelocation32(locationDelta);

                    BuildImportTable32(module32);

                    FinalizeSections32(address, headers, dosHeader, header32);

                    dllEntryPoint = address.Add(header32.OptionalHeader.AddressOfEntryPoint);
                }
            }
            #endregion

            //Log.Console("DllEntryPoint: 0x" + dllEntryPoint.ToString("X16"));
            DllEntry dllEntry = (DllEntry)Marshal.GetDelegateForFunctionPointer(dllEntryPoint, typeof(DllEntry));
            dllEntry(address, 1, IntPtr.Zero);
        }

        #region [this] GetModuleCount(IntPtr codeBase, IMAGE_DATA_DIRECTORY directory)
        private int GetModuleCount(IntPtr codeBase, IMAGE_DATA_DIRECTORY directory)
        {
            int result = 0;            
            if (directory.Size > 0)
            {
                IMAGE_IMPORT_DESCRIPTOR importDesc = codeBase.ToStruct<IMAGE_IMPORT_DESCRIPTOR>(directory.VirtualAddress);

                while (importDesc.Name > 0)
                {
                    string moduleName = Marshal.PtrToStringAnsi(codeBase.Add(importDesc.Name));
                    if (Native.LoadLibrary(moduleName) == IntPtr.Zero) break;

                    result++;
                    importDesc = codeBase.ToStruct<IMAGE_IMPORT_DESCRIPTOR>((uint)(directory.VirtualAddress + (Marshal.SizeOf(typeof(IMAGE_IMPORT_DESCRIPTOR)) * result)));
                }
            }
            return result;
        }
        #endregion

        #region [this] CopySections32(byte[] data, IntPtr codeBase, IMAGE_NT_HEADERS32 header32, IntPtr headers, IMAGE_DOS_HEADER dosHeader)
        private void CopySections32(byte[] data, IntPtr codeBase, IMAGE_NT_HEADERS32 header32, IntPtr headers, IMAGE_DOS_HEADER dosHeader)
        {
            IMAGE_SECTION_HEADER section = headers.ToStruct<IMAGE_SECTION_HEADER>(24 + dosHeader.e_lfanew + header32.FileHeader.SizeOfOptionalHeader);

            for (int i = 0; i < header32.FileHeader.NumberOfSections; i++)
            {
                //ConsoleWindow.Pause("Section " + i + "/"+ (header64.FileHeader.NumberOfSections-1) + " (" + Encoding.UTF8.GetString(section.Name) + ") -> 0x"+ section.VirtualAddress.ToString("X16") + "...");

                IntPtr dest;
                if (section.SizeOfRawData == 0)
                {
                    uint sectionSize = header32.OptionalHeader.SectionAlignment;
                    if (sectionSize > 0)
                    {
                        dest = Native.VirtualAlloc(codeBase.Add(section.VirtualAddress), sectionSize, MEM_COMMIT, PAGE_READWRITE);
                        Marshal.Copy(new byte[sectionSize + 1], 0, dest, (int)sectionSize);

                        //section.PhysicalAddress = (uint)dest;
                        //ConsoleWindow.WriteLine("section(NoRaw).PhysicalAddress=0x" + section.PhysicalAddress.ToString("X16")+", dest=0x" + dest.ToString("X16"));

                        //write = headers.Add(32 + dosHeader.e_lfanew + header64.FileHeader.SizeOfOptionalHeader + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * i));
                        //Marshal.WriteInt64(write, (long)dest);                      
                    }
                }
                else
                {
                    dest = Native.VirtualAlloc(codeBase.Add(section.VirtualAddress), section.SizeOfRawData, MEM_COMMIT, PAGE_READWRITE);
                    Marshal.Copy(data, (int)section.PointerToRawData, dest, (int)section.SizeOfRawData);

                    //section.PhysicalAddress = (uint)dest;
                    //ConsoleWindow.WriteLine("section.PhysicalAddress=0x" + section.PhysicalAddress.ToString("X16") + ", dest=0x" + dest.ToString("X16"));

                    //write = headers.Add(32 + dosHeader.e_lfanew + header64.FileHeader.SizeOfOptionalHeader + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * i));
                    //Marshal.WriteInt64(write, (long)dest);
                }
                section = headers.ToStruct<IMAGE_SECTION_HEADER>(24 + dosHeader.e_lfanew + header32.FileHeader.SizeOfOptionalHeader + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * (i + 1)));
            }
        }
        #endregion

        #region [this] CopySections64(byte[] data, IntPtr codeBase, IMAGE_NT_HEADERS64 header64, IntPtr headers, IMAGE_DOS_HEADER dosHeader)
        private void CopySections64(byte[] data, IntPtr codeBase, IMAGE_NT_HEADERS64 header64, IntPtr headers, IMAGE_DOS_HEADER dosHeader)
        {            
            IMAGE_SECTION_HEADER section = headers.ToStruct<IMAGE_SECTION_HEADER>(24 + dosHeader.e_lfanew + header64.FileHeader.SizeOfOptionalHeader);

            for (int i = 0; i < header64.FileHeader.NumberOfSections; i++)
            {
                //ConsoleWindow.Pause("Section " + i + "/"+ (header64.FileHeader.NumberOfSections-1) + " (" + Encoding.UTF8.GetString(section.Name) + ") -> 0x"+ section.VirtualAddress.ToString("X16") + "...");

                IntPtr dest;                                
                if (section.SizeOfRawData == 0)
                {
                    uint sectionSize = header64.OptionalHeader.SectionAlignment;
                    if (sectionSize > 0)
                    {
                        dest = Native.VirtualAlloc(codeBase.Add(section.VirtualAddress), sectionSize, MEM_COMMIT, PAGE_READWRITE);
                        Marshal.Copy(new byte[sectionSize + 1], 0, dest, (int)sectionSize);

                        //section.PhysicalAddress = (uint)dest;
                        //ConsoleWindow.WriteLine("section(NoRaw).PhysicalAddress=0x" + section.PhysicalAddress.ToString("X16")+", dest=0x" + dest.ToString("X16"));

                        //write = headers.Add(32 + dosHeader.e_lfanew + header64.FileHeader.SizeOfOptionalHeader + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * i));
                        //Marshal.WriteInt64(write, (long)dest);                      
                    }
                }
                else
                {
                    dest = Native.VirtualAlloc(codeBase.Add(section.VirtualAddress), section.SizeOfRawData, MEM_COMMIT, PAGE_READWRITE);
                    Marshal.Copy(data, (int)section.PointerToRawData, dest, (int)section.SizeOfRawData);

                    //section.PhysicalAddress = (uint)dest;
                    //ConsoleWindow.WriteLine("section.PhysicalAddress=0x" + section.PhysicalAddress.ToString("X16") + ", dest=0x" + dest.ToString("X16"));

                    //write = headers.Add(32 + dosHeader.e_lfanew + header64.FileHeader.SizeOfOptionalHeader + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * i));
                    //Marshal.WriteInt64(write, (long)dest);
                }
                section = headers.ToStruct<IMAGE_SECTION_HEADER>(24 + dosHeader.e_lfanew + header64.FileHeader.SizeOfOptionalHeader + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * (i+1)));
            }
        }
        #endregion

        #region [this] PerformBaseRelocation32(ulong delta)
        private void PerformBaseRelocation32(ulong delta)
        {
            //ConsoleWindow.WriteLine("PerformBaseRelocation: Delta="+delta);
            IntPtr codeBase = module64.codeBase;
            int sizeOfBase = Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION));
            IMAGE_DATA_DIRECTORY directory = module32.headers.OptionalHeader.BaseRelocationTable;

            int cnt = 0;
            if (directory.Size > 0)
            {
                var relocation = codeBase.ToStruct<IMAGE_BASE_RELOCATION>(directory.VirtualAddress);
                while (relocation.VirtualAddress > 0)
                {
                    unsafe
                    {
                        var dest = (IntPtr)(codeBase.ToInt32() + (int)relocation.VirtualAddress);
                        var relInfo = (ushort*)(codeBase.ToInt32() + (int)directory.VirtualAddress + sizeOfBase);
                        uint i;
                        for (i = 0; i < ((relocation.SizeOfBlock - Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION))) / 2); i++, relInfo++)
                        {
                            int type = *relInfo >> 12;
                            int offset = (*relInfo & 0xfff);
                            switch (type)
                            {
                                case 0x00:
                                    break;
                                case 0x03:
                                    uint* patchAddrHl = (uint*)((int)dest + offset);
                                    *patchAddrHl += (uint)delta;
                                    break;
                                case 0x0A:
                                    ulong* patchAddrDIR64 = (ulong*)((long)dest + offset);
                                    *patchAddrDIR64 += delta;
                                    break;
                            }
                        }
                    }
                    cnt += (int)relocation.SizeOfBlock;
                    relocation = codeBase.ToStruct<IMAGE_BASE_RELOCATION>((uint)(directory.VirtualAddress + cnt));
                }
            }
        }
        #endregion

        #region [this] PerformBaseRelocation64(ulong delta)
        private void PerformBaseRelocation64(ulong delta)
        {
            //ConsoleWindow.WriteLine("PerformBaseRelocation: Delta="+delta);
            IntPtr codeBase = module64.codeBase;
            int sizeOfBase = Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION));
            IMAGE_DATA_DIRECTORY directory = module64.headers.OptionalHeader.BaseRelocationTable;

            int cnt = 0;
            if (directory.Size > 0)
            {
                var relocation = codeBase.ToStruct<IMAGE_BASE_RELOCATION>(directory.VirtualAddress);
                while (relocation.VirtualAddress > 0)
                {
                    unsafe
                    {
                        var dest = (IntPtr)(codeBase.ToInt32() + (int)relocation.VirtualAddress);
                        var relInfo = (ushort*)(codeBase.ToInt32() + (int)directory.VirtualAddress + sizeOfBase);
                        uint i;
                        for (i = 0; i < ((relocation.SizeOfBlock - Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION))) / 2); i++, relInfo++)
                        {
                            int type = *relInfo >> 12;
                            int offset = (*relInfo & 0xfff);
                            switch (type)
                            {
                                case 0x00:
                                break;
                                case 0x03:
                                    uint* patchAddrHl = (uint*)((int)dest + offset);
                                    *patchAddrHl += (uint)delta;
                                break;
                                case 0x0A:
                                    ulong* patchAddrDIR64 = (ulong*)((long)dest + offset);
                                    *patchAddrDIR64 += delta;
                                break;
                            }
                        }
                    }
                    cnt += (int)relocation.SizeOfBlock;
                    relocation = codeBase.ToStruct<IMAGE_BASE_RELOCATION>((uint)(directory.VirtualAddress + cnt));
                }
            }
        }
        #endregion

        #region [this] BuildImportTable32(MEMORYMODULE64 module64)
        private void BuildImportTable32(MEMORYMODULE32 module32)
        {
            int moduleCount = 0;
            IntPtr codeBase = module32.codeBase;
            IMAGE_DATA_DIRECTORY directory = module32.headers.OptionalHeader.ImportTable;

            if (directory.Size > 0)
            {
                //Log.Console("ImportTable.Size: "+ directory.Size);

                uint* nameRef, funcRef;
                IMAGE_IMPORT_DESCRIPTOR importDesc = codeBase.ToStruct<IMAGE_IMPORT_DESCRIPTOR>(directory.VirtualAddress);

                while (importDesc.Name > 0)
                {
                    string moduleName = Marshal.PtrToStringAnsi(codeBase.Add(importDesc.Name));
                    //Log.Console("Import Module: " + moduleName);

                    IntPtr handle = Native.LoadLibrary(moduleName);
                    if (handle == IntPtr.Zero) break;

                    if (importDesc.CharacteristicsOrOriginalFirstThunk > 0)
                    {
                        nameRef = (uint*)codeBase.Add(importDesc.CharacteristicsOrOriginalFirstThunk);
                        funcRef = (uint*)codeBase.Add(importDesc.FirstThunk);
                    }
                    else
                    {
                        nameRef = (uint*)codeBase.Add(importDesc.FirstThunk);
                        funcRef = (uint*)codeBase.Add(importDesc.FirstThunk);
                    }

                    for (; *nameRef > 0; nameRef++, funcRef++)
                    {
                        //Log.Console("Import: " + nameRef->ToString("X16") + ", 0x" + funcRef->ToString("x16"));

                        if ((*nameRef & 0x8000000000000000) != 0)
                        {
                            *funcRef = (uint)Native.GetProcAddress(handle, new IntPtr(*nameRef & 0xffff));
                        }
                        else
                        {
                            string functionName = Marshal.PtrToStringAnsi(codeBase.Add((*nameRef) + 2));
                            *funcRef = (uint)Native.GetProcAddress(handle, functionName);
                            //Log.Console("Import Function: " + functionName + " -> 0x" + funcRef->ToString("x16"));
                        }

                        //Log.Console("Import nameRef: 0x" + nameRef->ToString("X16"));
                        //Log.Console("Import funcRef: 0x" + funcRef->ToString("X16"));                                                

                        if (*funcRef == 0)
                        {
                            break;
                        }
                    }

                    moduleCount++;
                    importDesc = codeBase.ToStruct<IMAGE_IMPORT_DESCRIPTOR>(directory.VirtualAddress + (uint)(Marshal.SizeOf(typeof(IMAGE_IMPORT_DESCRIPTOR)) * moduleCount));
                }
            }
        }
        #endregion

        #region [this] BuildImportTable64(MEMORYMODULE64 module64)
        private void BuildImportTable64(MEMORYMODULE64 module64)
        {
            int moduleCount = 0;
            IntPtr codeBase = module64.codeBase;
            IMAGE_DATA_DIRECTORY directory = module64.headers.OptionalHeader.ImportTable;

            if (directory.Size > 0)
            {
                //Log.Console("ImportTable.Size: "+ directory.Size);

                ulong* nameRef, funcRef;
                IMAGE_IMPORT_DESCRIPTOR importDesc = codeBase.ToStruct<IMAGE_IMPORT_DESCRIPTOR>(directory.VirtualAddress);                

                while (importDesc.Name > 0)
                {
                    string moduleName = Marshal.PtrToStringAnsi(codeBase.Add(importDesc.Name));
                    //Log.Console("Import Module: " + moduleName);

                    IntPtr handle = Native.LoadLibrary(moduleName);
                    if (handle == IntPtr.Zero) break;

                    if (importDesc.CharacteristicsOrOriginalFirstThunk > 0)
                    {                        
                        nameRef = (ulong*)codeBase.Add(importDesc.CharacteristicsOrOriginalFirstThunk);
                        funcRef = (ulong*)codeBase.Add(importDesc.FirstThunk);
                    }
                    else
                    {
                        nameRef = (ulong*)codeBase.Add(importDesc.FirstThunk);
                        funcRef = (ulong*)codeBase.Add(importDesc.FirstThunk);
                    }

                    for (; *nameRef > 0; nameRef++, funcRef++)
                    {
                        //Log.Console("Import: " + nameRef->ToString("X16") + ", 0x" + funcRef->ToString("x16"));

                        if ((*nameRef & 0x8000000000000000) != 0)
                        {
                            *funcRef = (ulong)Native.GetProcAddress(handle, new IntPtr((long)*nameRef & 0xffff));
                        }
                        else
                        {
                            string functionName = Marshal.PtrToStringAnsi(codeBase.Add((long)(*nameRef) + 2));                        
                            *funcRef = (ulong)Native.GetProcAddress(handle, functionName);
                            //Log.Console("Import Function: " + functionName + " -> 0x" + funcRef->ToString("x16"));
                        }

                        //Log.Console("Import nameRef: 0x" + nameRef->ToString("X16"));
                        //Log.Console("Import funcRef: 0x" + funcRef->ToString("X16"));                                                

                        if (*funcRef == 0)
                        {
                            break;
                        }
                    }

                    moduleCount++;
                    importDesc = codeBase.ToStruct<IMAGE_IMPORT_DESCRIPTOR>(directory.VirtualAddress + (uint)(Marshal.SizeOf(typeof(IMAGE_IMPORT_DESCRIPTOR)) * moduleCount));
                }            
            }
        }
        #endregion

        #region [this] FinalizeSections32(IntPtr codeBase, IntPtr headers, IMAGE_DOS_HEADER dosHeader, IMAGE_NT_HEADERS32 header32)
        private void FinalizeSections32(IntPtr codeBase, IntPtr headers, IMAGE_DOS_HEADER dosHeader, IMAGE_NT_HEADERS32 header32)
        {
            var section = headers.ToStruct<IMAGE_SECTION_HEADER>((uint)(24 + dosHeader.e_lfanew + header32.FileHeader.SizeOfOptionalHeader));
            for (int i = 0; i < header32.FileHeader.NumberOfSections; i++)
            {
                //Console.WriteLine("Finalize section " + Encoding.UTF8.GetString(section.Name));

                if ((section.Characteristics & 0x02000000) > 0)
                {
                    bool aa = Native.VirtualFree(codeBase.Add(section.VirtualAddress), (UIntPtr)section.SizeOfRawData, 0x4000);
                    continue;
                }

                int execute = (section.Characteristics & 0x20000000) != 0 ? 1 : 0;
                int readable = (section.Characteristics & 0x40000000) != 0 ? 1 : 0;
                int writeable = (section.Characteristics & 0x80000000) != 0 ? 1 : 0;

                var protect = (uint)ProtectionFlags[execute][readable][writeable];

                if ((section.Characteristics & 0x04000000) > 0) protect |= 0x200;

                var size = (int)section.SizeOfRawData;

                if (size == 0)
                {
                    if ((section.Characteristics & 0x00000040) > 0)
                    {
                        size = (int)header32.OptionalHeader.SizeOfInitializedData;
                    }
                    else if ((section.Characteristics & 0x00000080) > 0)
                    {
                        size = (int)header32.OptionalHeader.SizeOfUninitializedData;
                    }
                }

                if (size > 0)
                {
                    uint oldProtect;
                    IntPtr physicalAddress = codeBase.Add(section.VirtualAddress);
                    if (!Native.VirtualProtect(physicalAddress, section.SizeOfRawData, protect, out oldProtect))
                    {
                        Log.Error("Error protecting memory page of physical address 0x" + physicalAddress.ToString("X16"));
                    }
                }

                section = headers.ToStruct<IMAGE_SECTION_HEADER>((uint)((24 + dosHeader.e_lfanew + header32.FileHeader.SizeOfOptionalHeader) + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * (i + 1))));
            }

        }
        #endregion

        #region [this] FinalizeSections64(IntPtr codeBase, IntPtr headers, IMAGE_DOS_HEADER dosHeader, IMAGE_NT_HEADERS64 header64)
        private void FinalizeSections64(IntPtr codeBase, IntPtr headers, IMAGE_DOS_HEADER dosHeader, IMAGE_NT_HEADERS64 header64)
        {
            var section = headers.ToStruct<IMAGE_SECTION_HEADER>((uint)(24 + dosHeader.e_lfanew + header64.FileHeader.SizeOfOptionalHeader));
            for (int i = 0; i < header64.FileHeader.NumberOfSections; i++)
            {                                
                //Console.WriteLine("Finalize section " + Encoding.UTF8.GetString(section.Name));

                if ((section.Characteristics & 0x02000000) > 0)
                {
                    bool aa = Native.VirtualFree(codeBase.Add(section.VirtualAddress), (UIntPtr)section.SizeOfRawData, 0x4000);
                    continue;
                }

                int execute = (section.Characteristics & 0x20000000) != 0 ? 1 : 0;
                int readable = (section.Characteristics & 0x40000000) != 0 ? 1 : 0;
                int writeable = (section.Characteristics & 0x80000000) != 0 ? 1 : 0;

                var protect = (uint)ProtectionFlags[execute][readable][writeable];

                if ((section.Characteristics & 0x04000000) > 0) protect |= 0x200;

                var size = (int)section.SizeOfRawData;

                if (size == 0)
                {
                    if ((section.Characteristics & 0x00000040) > 0)
                    {
                        size = (int)header64.OptionalHeader.SizeOfInitializedData;
                    }
                    else if ((section.Characteristics & 0x00000080) > 0)
                    {
                        size = (int)header64.OptionalHeader.SizeOfUninitializedData;
                    }
                }

                if (size > 0)
                {
                    uint oldProtect;
                    IntPtr physicalAddress = codeBase.Add(section.VirtualAddress);
                    if (!Native.VirtualProtect(physicalAddress, section.SizeOfRawData, protect, out oldProtect))                    
                    {
                        Log.Error("Error protecting memory page of physical address 0x" + physicalAddress.ToString("X16"));
                    }
                }

                section = headers.ToStruct<IMAGE_SECTION_HEADER>((uint)((24 + dosHeader.e_lfanew + header64.FileHeader.SizeOfOptionalHeader) + (Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER)) * (i + 1))));
            }

        }
        #endregion

        #region [this] GetProcAddress(string name)
        public IntPtr GetProcAddress(string name)
        {
            unsafe
            {
                IntPtr codeBase;
                IMAGE_DATA_DIRECTORY directory;

                if (Is64Bit)
                {
                    codeBase = module64.codeBase;
                    directory = module64.headers.OptionalHeader.ExportTable;
                }
                else
                {
                    codeBase = module32.codeBase;
                    directory = module32.headers.OptionalHeader.ExportTable;
                }

                if (directory.Size != 0)
                {
                    IMAGE_EXPORT_DIRECTORY exports = codeBase.ToStruct<IMAGE_EXPORT_DIRECTORY>(directory.VirtualAddress);
                    ushort* ordinal = (ushort*)codeBase.Add(exports.AddressOfNameOrdinals);
                    uint* nameRef = (uint*)codeBase.Add(exports.AddressOfNames);

                    int idx = -1;                    
                    for (uint i = 0; i < exports.NumberOfNames; i++, nameRef++, ordinal++)
                    {
                        if (Marshal.PtrToStringAnsi(codeBase.Add(*nameRef)) == name)
                        {
                            idx = *ordinal;
                            break;
                        }
                    }

                    if (idx > -1)
                    {
                        uint* addrs = (uint*)codeBase.Add(exports.AddressOfFunctions + (idx * 4));
                        return codeBase.Add(*addrs);
                    }
                }
                return IntPtr.Zero;         
            }
        }
        #endregion
    }
}
