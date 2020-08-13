using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pb;
using MsgBase = Google.Protobuf.IMessage;

public class RoomListPanel : BasePanel {
	//账号文本
	private Text idText;
	//战绩文本
	private Text scoreText;
	//创建房间按钮
	private Button createButton;
	//刷新列表按钮
	private Button reflashButton;
	//列表容器
	private Transform content;
	//房间物体
	private GameObject roomObj;
	//坦克视图
	private GameObject tankCamera;
	private GameObject tankObj;

	//初始化
	public override void OnInit() {
		skinPath = "RoomListPanel";
		layer = PanelManager.Layer.Panel;
	}

	//显示
	public override void OnShow(params object[] args) {
		//寻找组件
		idText = skin.transform.Find("InfoPanel/IdText").GetComponent<Text>();
		scoreText = skin.transform.Find("InfoPanel/ScoreText").GetComponent<Text>();
		createButton = skin.transform.Find("CtrlPanel/CreateButton").GetComponent<Button>();
		reflashButton = skin.transform.Find("CtrlPanel/ReflashButton").GetComponent<Button>();
		content = skin.transform.Find("ListPanel/Scroll View/Viewport/Content");
		roomObj = skin.transform.Find("Room").gameObject;
		tankCamera = skin.transform.Find("InfoPanel/TankCamera").gameObject;
		//不激活房间
		roomObj.SetActive(false);
		//显示id
		idText.text = GameMain.id;
		//按钮事件
		createButton.onClick.AddListener(OnCreateClick);
		reflashButton.onClick.AddListener(OnReflashClick);
		//协议监听
		NetManager.AddMsgListener("MsgGetAchieve", OnMsgGetAchieve);
		NetManager.AddMsgListener("MsgGetRoomList", OnMsgGetRoomList);
		NetManager.AddMsgListener("MsgCreateRoom", OnMsgCreateRoom);
		NetManager.AddMsgListener("MsgEnterRoom", OnMsgEnterRoom);
		//发送查询
		MsgGetAchieve msgGetAchieve = new MsgGetAchieve();
		NetManager.Send(msgGetAchieve);
		MsgGetRoomList msgGetRoomList = new MsgGetRoomList();
		NetManager.Send(msgGetRoomList);
		//创建坦克模型
		GameObject tankSkin = ResManager.LoadPrefab("tankPrefab");
		tankObj = (GameObject)Instantiate(tankSkin, tankCamera.transform);
		tankObj.transform.localPosition = new Vector3(0, -2, 25);
		tankObj.transform.Rotate(0,90,-30);
	}


	//关闭
	public override void OnClose() {
		//协议监听
		NetManager.RemoveMsgListener("MsgGetAchieve", OnMsgGetAchieve);
		NetManager.RemoveMsgListener("MsgGetRoomList", OnMsgGetRoomList);
		NetManager.RemoveMsgListener("MsgCreateRoom", OnMsgCreateRoom);
		NetManager.RemoveMsgListener("MsgEnterRoom", OnMsgEnterRoom);
	}

	//收到成绩查询协议
	public void OnMsgGetAchieve (MsgBase msgBase) {
		MsgGetAchieve msg = (MsgGetAchieve)msgBase;
		scoreText.text = msg.Win + "胜 " + msg.Lost + "负";
	}

	//收到房间列表协议s
	public void OnMsgGetRoomList (MsgBase msgBase) {
		MsgGetRoomList msg = (MsgGetRoomList)msgBase;
		//清除房间列表
		for(int i = content.childCount-1; i >= 0 ; i--){
			GameObject o = content.GetChild(i).gameObject;
			Destroy(o);
		}
		//重新生成列表
		if(msg.Rooms.Count == 0){
			return;
		}
		for(int i = 0; i < msg.Rooms.Count; i++){
			GenerateRoom(msg.Rooms[i]);
		}
	}

	//创建一个房间单元
	public void GenerateRoom(RoomInfo roomInfo){
		//创建物体
		GameObject o = Instantiate(roomObj);
		o.transform.SetParent(content);
		o.SetActive(true);
		o.transform.localScale = Vector3.one;
		//获取组件
		Transform trans = o.transform;
		Text idText = trans.Find("IdText").GetComponent<Text>();
		Text countText = trans.Find("CountText").GetComponent<Text>();
		Text statusText = trans.Find("StatusText").GetComponent<Text>();
		Button btn = trans.Find("JoinButton").GetComponent<Button>();
		//填充信息
		idText.text = roomInfo.Id.ToString();
		countText.text = roomInfo.Count.ToString();
		if(roomInfo.Status == 0){
			statusText.text = "准备中";
		}
		else{
			statusText.text = "战斗中";
		}
		//按钮事件
		btn.name = idText.text;
		btn.onClick.AddListener(delegate(){
			OnJoinClick(btn.name);
		});
	}

	//点击刷新按钮
	public void OnReflashClick(){
		MsgGetRoomList msg = new MsgGetRoomList();
		NetManager.Send(msg);
	}

	//点击加入房间按钮
	public void OnJoinClick(string idString) {
		MsgEnterRoom msg = new MsgEnterRoom();
		msg.Id = int.Parse(idString);
		NetManager.Send(msg);
	}

	//收到进入房间协议
	public void OnMsgEnterRoom (MsgBase msgBase) {
		MsgEnterRoom msg = (MsgEnterRoom)msgBase;
		//成功进入房间
		if(msg.Result == 0){
			PanelManager.Open<RoomPanel>();
			Close();
		}
		//进入房间失败
		else{
			PanelManager.Open<TipPanel>("进入房间失败");
		}
	}

	//点击新建房间按钮
	public void OnCreateClick() {
		MsgCreateRoom msg = new MsgCreateRoom();
		NetManager.Send(msg);
	}

	//收到新建房间协议
	public void OnMsgCreateRoom (MsgBase msgBase) {
		MsgCreateRoom msg = (MsgCreateRoom)msgBase;
		//成功创建房间
		if(msg.Result == 0){
			//PanelManager.Open<TipPanel>("创建成功");
			PanelManager.Open<RoomPanel>();
			Close();
		}
		//创建房间失败
		else{
			PanelManager.Open<TipPanel>("创建房间失败");
		}
	}

	public void Update(){
		//更新坦克视图
		tankObj.transform.Rotate(0, Time.deltaTime*2f, 0);
	}

}
