using System;
using MsgBase = Google.Protobuf.IMessage;
public class Player {
	//id
	public string id = "";
    public int frameid;

	public int frameCount = 1; // 每帧数据的冗余度

	//指向ClientState
	public ClientState state;
	//构造函数
	public Player(ClientState state){
		this.state = state;
	}
	//坐标和旋转
	public int x; 
	public int y; 
	public int z;
	public int ex; 
	public int ey; 
	public int ez;

	//在哪个房间
	public int roomId = -1;

	// 标识该玩家在房间中的序号，用于在每帧数据中区分不同玩家
	public byte localId;
	//阵营
	public int camp = 1;
	//坦克生命值
	public int hp = 100;

	//数据库数据
	public PlayerData data;

	//发送信息
	public void Send(MsgBase msgBase ,bool reliable = true){
		if (reliable == true)
		{
			NetManager.Send(state, msgBase, ENet.PacketFlags.Reliable);
		}
		else
		{
			NetManager.Send(state, msgBase, ENet.PacketFlags.Unsequenced);
		}
		
	}

	public void SendPlayerPing(MsgBase msgBase)
	{
		NetManager.SendPlayerPing(state, msgBase);
	}

}


