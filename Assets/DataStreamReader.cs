using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
 * This class is a bit-wise memory output stream implementation
 */
public class DataStreamReader
{
    readonly private int capacity;
    private int head;
    readonly private byte[] buffer;

    public int GetHead()
    {
        return head;
    }
    
    public DataStreamReader(byte[] buffer)
    {
        this.buffer = buffer;
        capacity = buffer.Length;
    }

    public byte ReadByte()
    {
        var byteRet = buffer[head];
        head++;
        return byteRet;
    }
    
    public int ReadInt() {
        var resBytes = buffer.Skip(head).Take(4).ToList();
        head += 4;
        return bytesToInt(resBytes);
    }
    
    public float ReadFloat() {
        var resBytes = buffer.Skip(head).Take(4).ToArray();
        head += 4;
        int intRepresentation = BitConverter.ToInt32(resBytes, 0);
        return BitConverter.ToSingle(intToBytes(intRepresentation, resBytes.Length), 0);
    }
    
    public string ReadString() {
        int bytesCount = ReadByte();
        var resArr = buffer.Skip(head).Take(bytesCount).ToArray();
        head += bytesCount;
        return Encoding.UTF8.GetString(resArr);
    }
    private byte[] intToBytes(int num, int byteCount) {

        byte[] result = new byte[byteCount];
        for (int i = 0; byteCount > 0; i++, byteCount--) {
            result[i] = (byte)(num >> ((byteCount - 1) * 8));
        }
        return result;
    }

    private int bytesToInt(List<byte> num) {

        int result = 0;
        for (int i = 0; i < num.Count; i++) {
            result |= (num[i] << ((num.Count - i - 1) * 8));
        }
        return result;
    }
    
    public int GetRemainingByteCount() {
        return capacity - head;
    }

    public byte[] GetRemainingBytes()
    {
        return buffer.Skip(head-1).ToArray();
    }
    

}
