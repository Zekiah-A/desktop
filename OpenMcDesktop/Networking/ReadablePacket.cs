using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenMcDesktop.Networking;

/// <summary>
/// Packet reader, most similar to client/data.js
/// </summary>
public ref struct ReadablePacket
{
    public Span<byte> Data;
    public int Position;
    public int Left => Data.Length - Position;

    public ReadablePacket(byte[] data)
    {
        Data = data;
    }
    
    public T Read<T>(ref T? target) where T : new()
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

        // TODO: Implement array reading
        if (typeof(T).IsArray)
        {
            
        }
        
        // If T is a class, or some other kind of struct or object we use reflection to get the type of each of its
        // properties, and read into that, this is recursive, so regardless how deep the object is, we can keep reading
        // into it until we have populated the entire object with the correct data.
        if (target is null)
        {
            var newInstance = new T();
            var readMethod = ((object) this).GetType().GetMethod(nameof(Read))!;
            
            foreach (var property in typeof(T).GetProperties())
            {
                var readConstructed = readMethod.MakeGenericMethod(property.GetType());
                var propertyValue = readConstructed.Invoke(this, new[] { property.GetValue(newInstance) });
                newInstance.GetType().GetProperty(property.Name)!.SetValue(newInstance, propertyValue);
            }
        }
        else
        {
            var readMethod = ((object) this).GetType().GetMethod(nameof(Read))!;
            foreach (var property in typeof(T).GetProperties())
            {
                var readConstructed = readMethod.MakeGenericMethod(property.GetType());
                var propertyValue = readConstructed.Invoke(this, new[] { property.GetValue(target) });
                target.GetType().GetProperty(property.Name)!.SetValue(target, propertyValue);
            }
        }
        
        throw new InvalidOperationException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte() => Data[Position++];
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadShort() => BinaryPrimitives.ReadInt16BigEndian(Data[((Position += 2) - 2)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUShort() => BinaryPrimitives.ReadUInt16BigEndian(Data[((Position += 2) - 2)..]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt() => BinaryPrimitives.ReadInt32BigEndian(Data[((Position += 4) - 4)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt() => BinaryPrimitives.ReadUInt32BigEndian(Data[((Position += 4) - 4)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble() => BinaryPrimitives.ReadDoubleBigEndian(Data[((Position += 8) - 8)..]);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat() => BinaryPrimitives.ReadSingleBigEndian(Data[((Position += 4) - 4)..]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBool() => Data[Position++] != 0;

    /// <summary>
    /// A variable length integer. Similar to VarInt, made up of UInt6, UInt14 or Uint31, allows range 0-2147483647.
    /// </summary>
    public uint ReadFlexInt()
    {
        var value = (uint) Data[Position];
        if (value >= 64)
        {
            if (value >= 128)
            {
                value = BinaryPrimitives.ReadUInt32BigEndian(Data[Position..]) & 0x7FFFFFFF;
                Position += 4;
            }
            else
            {
                value = (uint) (BinaryPrimitives.ReadUInt16BigEndian(Data[Position..]) & 0x3FFF);
                Position += 2;
            }
        }
        else
        {
            Position++;
        }

        return value;
    }
    
    public byte[] ReadBytes(int count)
    {
        var array = Data[Position..(Position + count)];
        Position += count;
        return array.ToArray();
    }

    /// <summary>
    /// Variable length byte array, the first value will represent a FlexInt of the array length.
    /// </summary>
    public byte[] ReadByteArray()
    {
        var length = ReadFlexInt();
        var array = Data[Position..(int) (Position + length)].ToArray();
        Position += (int) length;
        return array;
    }

    /// <summary>
    /// Variable length string, the first value will represent a FlexInt of the array length.
    /// </summary>
    public string ReadString()
    {
        var subArray = ReadByteArray();
        return Encoding.UTF8.GetString(subArray);
    }
    
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