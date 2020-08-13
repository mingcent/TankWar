using System;
using ENet;
using Pb;
using MsgBase = Google.Protobuf.IMessage;
public partial class MsgHandler {


	//注册协议处理
	public static void MsgRegister(ClientState c, MsgBase msgBase){
		MsgRegister msg = (MsgRegister)msgBase;
		//注册
		if(DbManager.Register(msg.Id, msg.Pw)){
			DbManager.CreatePlayer(msg.Id);
			msg.Result = 0;
		}
		else{
			msg.Result = 1;
		}
		NetManager.Send(c, msg, PacketFlags.Reliable);
	}


	//登陆协议处理
	public static void MsgLogin(ClientState c, MsgBase msgBase){
		MsgLogin msg = (MsgLogin)msgBase;
		//密码校验
		if(!DbManager.CheckPassword(msg.Id, msg.Pw)){
			msg.Result = 1;
			NetManager.Send(c, msg,PacketFlags.Reliable);
			return;
		}
		//不允许再次登陆
		if(c.player != null){
			msg.Result = 1;
			NetManager.Send(c, msg, PacketFlags.Reliable);
			return;
		}
		//如果已经登陆，踢下线
		if(PlayerManager.IsOnline(msg.Id)){
			//发送踢下线协议
			Player other = PlayerManager.GetPlayer(msg.Id);
			MsgKick msgKick = new MsgKick();
			msgKick.Reason = 0;
			other.Send(msgKick);
			//断开连接
			NetManager.Close(other.state);
		}
		//获取玩家数据
		PlayerData playerData = DbManager.GetPlayerData(msg.Id);
		if(playerData == null){
			msg.Result = 1;
			NetManager.Send(c, msg, PacketFlags.Reliable);
			return;
		}
		//********************************************不能直接添加，应检测该玩家是否在房间中，断线重连
		//构建Player
		Player player = new Player(c);
		player.id = msg.Id;
		player.data = playerData;
		PlayerManager.AddPlayer(msg.Id, player);
		c.player = player;
		//返回协议
		msg.Result = 0;
		player.Send(msg);
	}
}
