using FixPoint;
using System;


public class VTransform
{
    public VInt3 position { get; set; }
    public VInt deg { get; set; } // 顺时针

    public void Rotate(VInt degree)
    {
        var res = degree + this.deg;
        this.deg = AbsDeg(res);
    }

    
    public VInt3 forward
    { //等同于2D  up
        get
        {
            VInt s, c;
            var ccwDeg = (0-deg + 90000);
            IntMath.SinCos(out s, out c, ccwDeg*IntMath.Deg2Rad);
            return new VInt3(c.i, 0,s.i);
        }
        set => deg = ToDeg(value);
    }
    public static VInt ToDeg(VInt3 value)
    {
        var ccwDeg = IntMath.Atan2(value.z, value.x)*IntMath.Rad2Deg;
        var deg = 90000 - ccwDeg;
        return AbsDeg(deg);
    }
    public static VInt AbsDeg(VInt deg)
    {
        var rawVal = deg.i % 360000;
        if (rawVal > 180000)
        {
            rawVal = rawVal - 360000;
        }
        else if (rawVal < -180000)
        {
            rawVal = rawVal + 360000;
        }
        return new VInt( rawVal);
    }
    public void Reset()
    {
        position = VInt3.zero;
        deg = VInt.zero;
    }

    public VTransform Clone()
    {
        return new VTransform() 
        {
            position = position, 
            deg = deg
        };
    }
}

