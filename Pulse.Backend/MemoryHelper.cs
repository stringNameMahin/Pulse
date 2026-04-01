using System;
using System.Runtime.InteropServices;

public static class MemoryHelper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll")]
    public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    public static (double totalMb, double availableMb) GetMemory()
    {
        var mem = new MEMORYSTATUSEX();
        mem.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));

        GlobalMemoryStatusEx(ref mem);

        double total = mem.ullTotalPhys / (1024.0 * 1024.0);
        double available = mem.ullAvailPhys / (1024.0 * 1024.0);

        return (total, available);
    }
}