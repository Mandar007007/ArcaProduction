using System.Runtime.InteropServices;

using Maankix.Endpoint.Service.Domain;

namespace Maankix.Endpoint.Service.Infrastructure.DriverClient;

/// <summary>
/// Walks a tightly packed MiniSpy output buffer the same way <c>RetrieveLogRecords</c> does.
/// </summary>
internal static class MiniSpyRecordParser
{
    public static List<FileIoObservedEventArgs> Parse(ReadOnlySpan<byte> buffer)
    {
        List<FileIoObservedEventArgs> results = [];
        int headerSize = MiniSpyProtocol.LogRecordHeaderSize;
        int used = 0;

        while (used + headerSize <= buffer.Length)
        {
            ReadOnlySpan<byte> remaining = buffer[used..];
            LogRecordHeader header = MemoryMarshal.Read<LogRecordHeader>(remaining);

            if (header.Length < (uint)(headerSize + sizeof(char)))
            {
                break;
            }

            int recordLength = (int)header.Length;
            if (used + recordLength > buffer.Length)
            {
                break;
            }

            used += recordLength;

            if ((header.RecordType & MiniSpyProtocol.RecordTypeFileTag) != 0)
            {
                continue;
            }

            if (!TryMapKind(header.Data.CallbackMajorId, out FileIoKind kind))
            {
                continue;
            }

            int nameBytes = recordLength - headerSize;
            if (nameBytes < sizeof(char))
            {
                continue;
            }

            ReadOnlySpan<char> nameChars = MemoryMarshal.Cast<byte, char>(
                remaining.Slice(headerSize, nameBytes));
            int terminator = nameChars.IndexOf('\0');
            string path = terminator >= 0
                ? nameChars[..terminator].ToString()
                : nameChars.ToString();

            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            results.Add(new FileIoObservedEventArgs(
                kind,
                path,
                header.SequenceNumber,
                header.Data.ProcessId,
                header.Data.Status));
        }

        return results;
    }

    internal static bool TryMapKind(byte majorFunction, out FileIoKind kind)
    {
        switch (majorFunction)
        {
            case MiniSpyProtocol.IrpMjCreate:
                kind = FileIoKind.Create;
                return true;
            case MiniSpyProtocol.IrpMjRead:
            case MiniSpyProtocol.IrpMjMdlRead:
                kind = FileIoKind.Read;
                return true;
            case MiniSpyProtocol.IrpMjWrite:
            case MiniSpyProtocol.IrpMjPrepareMdlWrite:
            case MiniSpyProtocol.IrpMjMdlWriteComplete:
                kind = FileIoKind.Write;
                return true;
            case MiniSpyProtocol.IrpMjClose:
                kind = FileIoKind.Close;
                return true;
            default:
                kind = default;
                return false;
        }
    }
}
