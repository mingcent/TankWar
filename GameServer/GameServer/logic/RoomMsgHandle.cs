using System;
using Pb;
using MsgBase = Google.Protobuf.IMessage;
public partial class MsgHandler {
	
	//查询战绩
	public static void MsgGetAchieve(ClientState c, MsgBase msgBase){
		MsgGetAchieve msg = (MsgGetAchieve)msgBase;
		Player player = c.player;
		if(player == null) return;

		msg.Win = player.data.win;
		msg.Lost = player.data.lost;

		player.Send(msg);
	}


	//请求房间列表
	public static void MsgGetRoomList(ClientState c, MsgBase msgBase){
		MsgGetRoomList msg = (MsgGetRoomList)msgBase;
		Player player = c.player;
		if(player == null) return;

		player.Send(RoomManager.ToMsg());
	}

	//创建房间
	public static void MsgCreateRoom(ClientState c, MsgBase msgBase){
		MsgCreateRoom msg = (MsgCreateRoom)msgBase;
		Player player = c.player;
		if(player == null) return;
		//已经在房间里
		if(player.roomId >=0 ){
			msg.Result = 1;
			player.Send(msg);
			return;
		}
		//创建
		Room room = RoomManager.AddRoom();
		room.AddPlayer(player.id);

		msg.Result = 0;
		player.Send(msg);
	}

	//进入房间
	public static void MsgEnterRoom(ClientState c, MsgBase msgBase){
		MsgEnterRoom msg = (MsgEnterRoom)msgBase;
		Player player = c.player;
		if(player == null) return;
		//已经在房间里
		if(player.roomId >=0 ){
			msg.Result = 1;
			player.Send(msg);
			return;
		}
		//获取房间
		Room room = RoomManager.GetRoom(msg.Id);
		if(room == null){
			msg.Result = 1;
			player.Send(msg);
			return;
		}
		//进入
		if(!room.AddPlayer(player.id)){
			msg.Result = 1;
			player.Send(msg);
			return;
		}
		//返回协议	
		msg.Result = 0;
		player.Send(msg);
	}


	//获取房间信息
	public static void MsgGetRoomInfo(ClientState c, MsgBase msgBase){
		MsgGetRoomInfo msg = (MsgGetRoomInfo)msgBase;
		Player player = c.player;
		if(player == null) return;

		Room room = RoomManager.GetRoom(player.roomId);
		if(room == null){
			player.Send(msg);
			return;
		}

		player.Send(room.ToMsg());
	}

	//离开房间
	public static void MsgLeaveRoom(ClientState c, MsgBase msgBase){
		MsgLeaveRoom msg = (MsgLeaveRoom)msgBase;
		Player player = c.player;
		if(player == null) return;

		Room room = RoomManager.GetRoom(player.roomId);
		if(room == null){
			msg.Result = 1;
			player.Send(msg);
			return;
		}

		room.RemovePlayer(player.id);
		//返回协议
		msg.Result = 0;
		player.Send(msg);
	}


	//请求开始战斗
	public static void MsgStartBattle(ClientState c, MsgBase msgBase){
		MsgStartBattle msg = (MsgStartBattle)msgBase;
		Player player = c.player;
		if(player == null) return;
		//room
		Room room = RoomManager.GetRoom(player.roomId);
		if(room == null){
			msg.Result = 1;
			player.Send(msg);
			return;
		}
		//是否是房主
		if(!room.isOwner(player)){
			msg.Result = 1;
			player.Send(msg);
			return;
		}
		//开战
		if(!room.StartBattle()){
			msg.Result = 1;
			player.Send(msg);
			return;
		}
		//成功
		//msg.result = 0;
		//player.Send(msg);
	}

}


