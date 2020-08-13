using FixPoint;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using MsgBase = Google.Protobuf.IMessage;
using System.Linq;

// 这是一个被动的类，所有的动作都由网络消息驱动。

public class BattleManager {
	//战场中的坦克
	// public static Dictionary<string, BaseTank> tanks = new Dictionary<string, BaseTank>();
	public static World world = new World();
	//初始化
	public static void Init() {
		//添加监听
		NetManager.AddMsgListener("MsgEnterBattle", OnMsgEnterBattle);
		NetManager.AddMsgListener("MsgBattleResult", OnMsgBattleResult);
		NetManager.AddMsgListener("MsgServerFrames", OnMsgServerFrames);
		NetManager.AddMsgListener("MsgMissingServerFrames", OnMsgMissingServerFrames);
		NetManager.AddMsgListener("MsgPlayerPing", OnMsgPlayerPing);

		world.Init();

	}

	//开始战斗
	public static void EnterBattle(MsgEnterBattle msg) {
		//重置
		world.Reset();
		//关闭界面
		PanelManager.Close("RoomPanel");//可以放到房间系统的监听中
		PanelManager.Close("ResultPanel");
		PanelManager.Close("KillPanel");
		PanelManager.Close("BattlePanel");

		//产生坦克

		for(int i=0; i<msg.Tanks.Count; i++){
			GenerateTank(msg.Tanks[i]);

		}
		//打开界面
		PanelManager.Open<BattlePanel>();
		// 
		World.localid = GameMain.id;
		//UnityEngine.Debug.Log("start battle");
		world.Start();

	}

	//产生坦克
	public static void GenerateTank(TankInfo tankInfo){
		var pos = new VInt3(tankInfo.X, tankInfo.Y, tankInfo.Z);
		var deg = tankInfo.Y;
		Tank tank = null;
		if (tankInfo.Camp == 1)
		{
			tank = world.stateService_.CreateTank("tankPrefab", pos, deg, tankInfo.Id);
		}
		else
		{
			tank = world.stateService_.CreateTank("tankPrefab2", pos, deg, tankInfo.Id);
		}
		//本地坦克添加相机，保存frameid
		if(tankInfo.Id == GameMain.id) {
			tank.view.gameObject.AddComponent<CameraFollow>();
			World.localFrameid = tankInfo.Frameid;
		}

		//属性
		tank.camp = tankInfo.Camp;
		tank.hp = tankInfo.Hp;
		tank.frameId = tankInfo.Frameid;
	}


		
	//收到进入战斗协议
	public static void OnMsgEnterBattle(MsgBase msgBase){
		MsgEnterBattle msg = (MsgEnterBattle)msgBase;
		EnterBattle(msg);
	}

	//收到战斗结束协议
	public static void OnMsgBattleResult(MsgBase msgBase){
		MsgBattleResult msg = (MsgBattleResult)msgBase;
		// 停止模拟
		Debug.Log("Battle result");
		world.Stop();
		//判断显示胜利还是失败
		bool isWin = false;
		var tanks = world.stateService_.GetTanks();
		var tank = tanks[GameMain.id];
		if (tank != null && tank.camp == msg.WinCamp)
		{
			isWin = true;
		}

		//显示界面
		PanelManager.Open<ResultPanel>(isWin);
	}


	public static void OnMsgServerFrames(MsgBase msgBase)
    {
 		MsgServerFrames msg = (MsgServerFrames)msgBase;
		world.frameBufferService_.PushServerFrames(msg.Frames.ToArray());
	}

	public static void OnMsgMissingServerFrames(MsgBase msgBase)
	{
		MsgMissingServerFrames msg = (MsgMissingServerFrames)msgBase;
		world.frameBufferService_.PushMissingFrames(msg.Frames.ToArray());

	}

	public static void OnMsgPlayerPing(MsgBase msgBase)
	{
		var msg = msgBase as MsgPlayerPing;
		world.frameBufferService_.OnPlayerPing(msg);
	}
	

}
