using System.Runtime.InteropServices;

namespace Maankix.Endpoint.Service.Infrastructure.DriverClient;

/// <summary>
/// P/Invoke for <c>fltlib.dll</c> used by MiniSpy.exe.
/// </summary>
internal static class FltLibNative
{
    [DllImport("fltlib.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern int FilterConnectCommunicationPort(
        string lpPortName,
        uint dwOptions,
        nint lpContext,
        ushort wSizeOfContext,
        nint lpSecurityAttributes,
        out nint hPort);

    [DllImport("fltlib.dll", ExactSpelling = true)]
    public static extern int FilterSendMessage(
        nint hPort,
        ref CommandMessage lpInBuffer,
        uint dwInBufferSize,
        [Out] byte[] lpOutBuffer,
        uint dwOutBufferSize,
        out uint lpBytesReturned);

    [DllImport("fltlib.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern int FilterAttach(
        string lpFilterName,
        string lpVolumeName,
        string? lpInstanceName,
        uint dwCreatedInstanceNameLength,
        [Out] char[] lpCreatedInstanceName);

    [DllImport("fltlib.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern int FilterDetach(
        string lpFilterName,
        string lpVolumeName,
        string? lpInstanceName);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(nint hObject);
}
