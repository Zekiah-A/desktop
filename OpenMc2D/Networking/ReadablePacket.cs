using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Unicode;

namespace OpenMc2D.Networking;

/// <summary>
/// Packet reader, most similar to client/data.js
/// </summary>
public ref struct ReadablePacket
{
    public Span<byte> Data;
    public int Position;

    public ReadablePacket(byte[] data)
    {
        Data = data;
    }
    
    public T Read<T>()
    {
        if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte)) return (T) (object) ReadByte();
        if (typeof(T) == typeof(short)) return (T) (object) ReadShort();
        if (typeof(T) == typeof(ushort)) return (T) (object) ReadUShort();
        if (typeof(T) == typeof(int)) return (T) (object) ReadInt();
        if (typeof(T) == typeof(uint)) return (T) (object) ReadUInt();
        if (typeof(T) == typeof(double)) return (T) (object) ReadDouble();
        if (typeof(T) == typeof(float)) return (T) (object) ReadFloat();
        if (typeof(T) == typeof(bool)) return (T) (object) ReadBool();
        if (typeof(T) == typeof(string)) return (T) (object) ReadString();
        if (typeof(T) == typeof(byte[])) return (T) (object) ReadByteArray();
        /* TODO: This
        if (typeof(T) == typeof(Item))
        {
            return ReadItem(target)
        }

        if (typeof(T).IsArray)
        {
            
        }
        */
        
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    byte ReadByte() => Data[Position++];
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    short ReadShort() => BinaryPrimitives.ReadInt16BigEndian(Data[(Position += 2)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    ushort ReadUShort() => BinaryPrimitives.ReadUInt16BigEndian(Data[(Position += 2)..]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int ReadInt() => BinaryPrimitives.ReadInt32BigEndian(Data[(Position += 4)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    uint ReadUInt() => BinaryPrimitives.ReadUInt32BigEndian(Data[(Position += 4)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    double ReadDouble() => BinaryPrimitives.ReadDoubleBigEndian(Data[(Position += 8)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    float ReadFloat() => BinaryPrimitives.ReadSingleBigEndian(Data[(Position += 4)..]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool ReadBool() => Data[Position++] != 0;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    string ReadString() => Encoding.UTF8.GetString(Data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    byte[] ReadByteArray() => Data.ToArray();


    /*Item ReadItem()
    {
        
    }*/
    
    public static implicit operator ReadablePacket(byte[] data)
    {
        return new ReadablePacket(data);
    }

    public static implicit operator byte[](ReadablePacket packet)
    {
        return packet.Data.ToArray();
    }
    
    public static implicit operator Span<byte>(ReadablePacket packet)
    {
        return packet.Data;
    }

    public byte this[int index]
    {
        get => Data[index];
        set => Data[index] = value;
    }
    
}