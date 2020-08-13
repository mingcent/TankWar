using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class PlayerInputCmd
{
    public byte[] cmd;
}


public class ServerFrame
{
    public int tick;
    public PlayerInputCmd[] inputs;
}

public class MsgServerFrames : MsgBase
{
    public MsgServerFrames() { protoName = "MsgServerFrames"; }
    public int startTick;                                         // 开始帧的帧号
    public ServerFrame[] frames;
}


public class MsgClientInputs : MsgBase
{
    public MsgClientInputs() { protoName = "MsgClientInputs"; }
    public int lastTick;                                        // 最后一帧的帧号
    public PlayerInputCmd[] inputs;
}

public class MsgMissFrame: MsgBase
{
    public MsgMissFrame() { protoName = "MsgMissFrame"; }
    public int tick;
}



public class MsgMissingServerFrames : MsgBase
{
    public MsgMissingServerFrames() { protoName = "MsgMissingServerFrames"; }
    public int startTick;
    public ServerFrame[] frames;
}



