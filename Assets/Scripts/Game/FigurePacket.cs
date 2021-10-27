using System;
using System.Collections.Generic;
using System.Text;

public class FigurePacket
{
    public string Owner;
    public byte Color;
    public List<IntPoint> Points;

    public FigurePacket(string owner, byte color, List<IntPoint> points)
    {
        Owner = owner;
        Color = color;
        Points = new List<IntPoint>(points);
    }

    public FigurePacket(byte[] bytes)
    {
        var index = 0;
        // Color, IsLittleEndian, Owner.Length, Owner, Points.Count, Points
        Color = bytes[index];
        index++;
        var isLittleEndian = bytes[index] == 1;
        index++;
        var bLen = new byte[sizeof(int)];
        Array.Copy(bytes, index, bLen, 0, bLen.Length);
        index += bLen.Length;
        if (isLittleEndian)
            Array.Reverse(bLen);
        var ownerLength = BitConverter.ToInt32(bLen, 0);
        var bOwner = new byte[ownerLength];
        Array.Copy(bytes, index, bOwner, 0, ownerLength);
        Owner = Encoding.UTF8.GetString(bOwner);
        index += ownerLength;

        bLen = new byte[sizeof(int)];
        Array.Copy(bytes, index, bLen, 0, bLen.Length);
        index += bLen.Length;
        
        if (isLittleEndian) Array.Reverse(bLen);
        var length = BitConverter.ToInt32(bLen, 0);
        Points = new List<IntPoint>(length);
        for (var i = 0; i < length; ++i)
        {
            var x = bytes[index];
            index++;
            var y = bytes[index];
            index++;
            Points.Add(new IntPoint(x, y));
        }
    }

    public byte[] GetData()
    {
        var index = 0;
        // Color, IsLittleEndian, Owner.Length, Owner, Points.Count, Points
        var bytes = new byte[1 + 1 + sizeof(int) + Owner.Length + sizeof(int)+ sizeof(byte)*2*Points.Count];
        bytes[index] = Color;
        index++;
        bytes[index] = BitConverter.IsLittleEndian ? (byte)1 : (byte)0;
        index++;

        var owner = Encoding.UTF8.GetBytes(Owner);
        var ownerLength = BitConverter.GetBytes(owner.Length);
        
        if (BitConverter.IsLittleEndian)
            Array.Reverse(ownerLength);
        Array.Copy(ownerLength, 0, bytes, index, ownerLength.Length);
        index += ownerLength.Length;
        Array.Copy(owner, 0, bytes, index, owner.Length);
        index += owner.Length;

        int n = Points.Count;
        var bLen = BitConverter.GetBytes(n);
        if (BitConverter.IsLittleEndian) Array.Reverse(bLen);
        Array.Copy(bLen, 0, bytes, index, bLen.Length);
        index += bLen.Length;        

        for (var i = 0; i < Points.Count; ++i)
        {
            bytes[index] = Points[i].X;
            index++; 
            bytes[index] = Points[i].Y;
            index++;
        }

        return bytes;
    }
}
