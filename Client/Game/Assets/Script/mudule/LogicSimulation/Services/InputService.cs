using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FixPoint;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Pb;
using Google.Protobuf.WellKnownTypes;

[Serializable]
public class PlayerInput
{

    // 玩家输入
    public VInt2 inputUV = VInt2.zero;
    public bool isInputFire = false;

    public void Reset()
    {
        inputUV = VInt2.zero;
        isInputFire = false;
    }

    public bool Equals(PlayerInput other)
    {
        if (other == null) return false;
        if (inputUV != other.inputUV) return false;
        if (isInputFire != other.isInputFire) return false;
        return true;
        
    }

}


public class InputService
{

    public static PlayerInput currentInput = new PlayerInput();

    public static void ExecuteCmd(Tank tank, PlayerInputCmd InputCmd)
    {
       
        if (InputCmd.Cmd == null || InputCmd.Cmd.Length == 0) // 命令为空
        {
            return;
        }

        Pb.PlayerInput input = Pb.PlayerInput.Parser.ParseFrom(InputCmd.Cmd);

        
        tank.input.inputUV.x = input.U;
        tank.input.inputUV.y = input.V;
        tank.input.isInputFire = input.Fire;

    }


    public static PlayerInputCmd GetInputCmd() // 将输入编码成字符串
    {
        Pb.PlayerInput input = new Pb.PlayerInput();
        input.U = currentInput.inputUV.x;
        input.V = currentInput.inputUV.y;
        input.Fire = currentInput.isInputFire;
        return new PlayerInputCmd()
        {
            IsMissing = false,
            Cmd = input.ToByteString()
        };
    }
    public static PlayerInputCmd EmptyInputCmd = new PlayerInputCmd();
}


