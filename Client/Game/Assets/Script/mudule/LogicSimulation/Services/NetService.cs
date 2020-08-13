using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pb;

public class NetService
{
    public static World world;

    public static void SendMissFrameReq(int MaxContinueServerTick)
    {
        MsgMissFrame msg = new MsgMissFrame();
        msg.Tick = MaxContinueServerTick;
        NetManager.Send(msg);
    }

    public static void SendPlayerPing(long _sendTime)
    {
        MsgPlayerPing msg = new MsgPlayerPing() { SendTime = _sendTime };
        NetManager.SendPlayerPing(msg);
    }
}

