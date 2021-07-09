using System;
using System.Collections.Generic;
using System.Text;

/**
 * This class is a bit-wise memory output stream implementation
 */
public class DataStreamWriter
{
    public List<byte> buffer;
    
    public DataStreamWriter() {
        buffer = new List<byte>();
    }

    public void WriteInt(int data) {
        var byteCount = 4;
        for (int i = 0; byteCount > 0; i++, byteCount--)
        {
            buffer.Add((byte)(data >> ((byteCount - 1) * 8)));
        }
    }

    public void WriteFloat(float data) {
        
        var byteCount = 4;
        var d = BitConverter.ToInt32(BitConverter.GetBytes(data), 0);
        for (int i = 0; byteCount > 0; i++, byteCount--)
        {
            buffer.Add((byte)(d >> ((byteCount - 1) * 8)));
        }
        
    }

    public void WriteByte(byte data) {
        buffer.Add(data);
    }
    
    public void WriteString(string data) {
        byte[] bytes = Encoding.UTF8.GetBytes(data);
        WriteByte((byte) bytes.Length);
        foreach (byte element in bytes) {
            WriteByte(element);
        }
    }

}