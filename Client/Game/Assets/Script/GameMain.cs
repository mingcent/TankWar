using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pb;
using MsgBase = Google.Protobuf.IMessage;
public class GameMain : MonoBehaviour {
	public static string id = "";

    private void Awake()
    {
		//挂载脚本
		gameObject.AddComponent<InputMono>(); 
		gameObject.AddComponent<PingMono>();

    }
    // Use this for initialization
    void Start () {
		//网络监听
		NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
		NetManager.AddMsgListener("MsgKick", OnMsgKick);
		//初始化
		PanelManager.Init();
		BattleManager.Init();
		//打开登陆面板
		PanelManager.Open<LoginPanel>();

	}


	// Update is called once per frame
	void Update () {
		NetManager.Update();
		BattleManager.world.Update();
	}

	//关闭连接
	void OnConnectClose(string err){
		Debug.Log("断开连接");
	} 

	//被踢下线
	void OnMsgKick(MsgBase msgBase){
		PanelManager.Open<TipPanel>("被踢下线");
	}
}
