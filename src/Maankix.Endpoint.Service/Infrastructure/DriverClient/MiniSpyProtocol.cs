using System.Runtime.InteropServices;

namespace Maankix.Endpoint.Service.Infrastructure.DriverClient;

/// <summary>
/// MiniSpy user-mode protocol constants and blittable layouts from <c>minispy.h</c>.
/// </summary>
internal static class MiniSpyProtocol
{
    public const int BufferSize = 4096;
    public const int GetMiniSpyLog = 0;
    public const uint RecordTypeFileTag = 0x00000004;

    public const byte IrpMjCreate = 0x00;
    public const byte IrpMjClose = 0x02;
    public const byte IrpMjRead = 0x03;
    public const byte IrpMjWrite = 0x04;
    public const byte IrpMjMdlRead = unchecked((byte)(-15));
    public const byte IrpMjPrepareMdlWrite = unchecked((byte)(-17));
    public const byte IrpMjMdlWriteComplete = unchecked((byte)(-18));

    public static readonly int CommandMessageSize = Marshal.SizeOf<CommandMessage>();
    public static readonly int LogRecordHeaderSize = Marshal.SizeOf<LogRecordHeader>();

    public const int HresultNoMoreItems = unchecked((int)0x80070103);
    public const int HresultInvalidHandle = unchecked((int)0x80070006);
    public const int NtStatusNoMoreEntries = unchecked((int)0x8000001A);
}

/// <summary>
/// <c>COMMAND_MESSAGE</c> without the flexible <c>Data[]</c> tail.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct CommandMessage
{
    public int Command;
    public uint Reserved;
}

/// <summary>
/// <c>RECORD_DATA</c> as laid out for 64-bit user/kernel.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct RecordData
{
    public long OriginatingTime;
    public long CompletionTime;
    public ulong DeviceObject;
    public ulong FileObject;
    public ulong Transaction;
    public ulong ProcessId;
    public ulong ThreadId;
    public ulong Information;
    public int Status;
    public uint IrpFlags;
    public uint Flags;
    public byte CallbackMajorId;
    public byte CallbackMinorId;
    public byte Reserved0;
    public byte Reserved1;
    public ulong Arg1;
    public ulong Arg2;
    public ulong Arg3;
    public ulong Arg4;
    public ulong Arg5;
    public long Arg6;
    public uint EcpCount;
    public uint KnownEcpMask;
}

/// <summary>
/// <c>LOG_RECORD</c> header; <c>Name[]</c> follows immediately.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct LogRecordHeader
{
    public uint Length;
    public uint SequenceNumber;
    public uint RecordType;
    public uint Reserved;
    public RecordData Data;
}
