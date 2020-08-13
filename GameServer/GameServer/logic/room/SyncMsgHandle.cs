using System;
using System.Threading.Tasks;
using Pb;
using MsgBase = Google.Protobuf.IMessage;
public partial class MsgHandler {

	//同步位置协议
	public static void MsgTankHp(ClientState c, MsgBase msgBase){
		MsgTankHp msg = (MsgTankHp)msgBase;
		Player player = c.player;
		if(player == null) return;
		//room
		Room room = RoomManager.GetRoom(player.roomId);
		if(room == null){
			return;
		}
		//status
		if(room.status != Room.Status.FIGHT){
			return;
		}
		//更新信息
		player.hp = msg.Hp;

	}

	public static void MsgPlayerPing(ClientState c, MsgBase msgBase)
	{
		MsgPlayerPing msg = (MsgPlayerPing)msgBase;
		Player player = c.player;
		if (player == null) return;
		//room
		Room room = RoomManager.GetRoom(player.roomId);
		if (room == null)
		{
			return;
		}
		//status
		if (room.status != Room.Status.FIGHT)
		{
			return;
		}
		//更新信息
		room.game.SendPlayerPing(player, msg);

	}

	public static void MsgClientInputs(ClientState c, MsgBase msgBase)
    {
		//Console.WriteLine("reveive client frame");

		MsgClientInputs msg = (MsgClientInputs)msgBase;
		Player player = c.player;
		if (player == null) 
			return;
		//room
		Room room = RoomManager.GetRoom(player.roomId);
		if (room == null)
		{
			return;
		}
		//status
		if (room.status != Room.Status.FIGHT)
		{
			return;
		}
		room.game.PushPlayerInput(player, msg);
	}


	public static void MsgMissFrame(ClientState c, MsgBase msgBase) // 处理丢帧
	{
		MsgMissFrame msg = msgBase as MsgMissFrame;
		Player player = c.player;
		if (player == null)
			return;
		//room
		Room room = RoomManager.GetRoom(player.roomId);
		if (room == null)
		{
			return;
		}
		//status
		if (room.status != Room.Status.FIGHT)
		{
			return;
		}
		int missTick = msg.Tick;
		room.game.SendMissingFrame(player, missTick);

	}

}


