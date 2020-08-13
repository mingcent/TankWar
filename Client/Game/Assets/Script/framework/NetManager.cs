using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using ENet;
using System.Threading.Tasks;
using MsgBase = Google.Protobuf.IMessage;

public static class NetManager
{

	//定义Host
	static Host host;
	//定义Peer
	public static Peer peer;
	//接收缓冲区
	static ByteArray readBuff;
	//写入队列
	static Queue<ByteArray> writeQueue;
	//是否正在连接
	static bool isConnecting = false;
	//是否正在关闭
	static bool isClosing = false;
	// 是否连接
	static bool isConnected = false;
	//消息列表
	static List<MsgBase> msgList = new List<MsgBase>();
	//消息列表长度
	static int msgCount = 0;
	//每一次Update处理的消息量
	readonly static int MAX_MESSAGE_FIRE = 10;
	//是否启用心跳
	public static bool isUsePing = true;
	//心跳间隔时间
	public static int pingInterval = 5;
	//上一次发送PING的时间
	static float lastPingTime = 0;
	//上一次收到PONG的时间
	static float lastPongTime = 0;

	//事件
	public enum NetEvent
	{
		ConnectSucc = 1,
		ConnectFail = 2,
		Close = 3,
	}
	//事件委托类型
	public delegate void EventListener(String err);
	//事件监听列表
	private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();
	//添加事件监听
	public static void AddEventListener(NetEvent netEvent, EventListener listener)
	{
		//添加事件
		if (eventListeners.ContainsKey(netEvent))
		{
			eventListeners[netEvent] += listener;
		}
		//新增事件
		else
		{
			eventListeners[netEvent] = listener;
		}
	}
	//删除事件监听
	public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
	{
		if (eventListeners.ContainsKey(netEvent))
		{
			eventListeners[netEvent] -= listener;
		}
	}
	//分发事件
	private static void FireEvent(NetEvent netEvent, String err)
	{
		if (eventListeners.ContainsKey(netEvent))
		{
			eventListeners[netEvent](err);
		}
	}


	//消息委托类型
	public delegate void MsgListener(MsgBase msgBase);
	//消息监听列表
	private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();
	//添加消息监听
	public static void AddMsgListener(string msgName, MsgListener listener)
	{
		//添加
		if (msgListeners.ContainsKey(msgName))
		{
			msgListeners[msgName] += listener;
		}
		//新增
		else
		{
			msgListeners[msgName] = listener;
		}
	}
	//删除消息监听
	public static void RemoveMsgListener(string msgName, MsgListener listener)
	{
		if (msgListeners.ContainsKey(msgName))
		{
			msgListeners[msgName] -= listener;
		}
	}
	//分发消息
	private static void FireMsg(string msgName, MsgBase msgBase)
	{

		if (msgListeners.ContainsKey(msgName))
		{
			if (msgListeners[msgName] == null)
			{
				Debug.Log(msgName + " Doesn't exist");
				return;
			}
			msgListeners[msgName](msgBase);
		}
	}


	//连接
	public static void  Connect(string ip, int port)
	{
		//状态判断
		if (isConnected)
		{
			Debug.Log("Connect fail, already connected!");
			return;
		}
		if (isConnecting)
		{
			Debug.Log("Connect fail, isConnecting");
			return;
		}
		//初始化成员
		InitState();
		//Connect
		var address = new Address();
		address.SetHost(ip);
		address.Port = (ushort)port;
		isConnecting = true;
		peer = host.Connect(address, 4);
		isConnected = true;
		isConnecting = false;
	}

	//初始化状态
	private static void InitState()
	{
		//建立Host
		host = new Host();
		host.Create(null, 1);
		//接收缓冲区
		readBuff = new ByteArray();
		//写入队列
		writeQueue = new Queue<ByteArray>();
		//是否正在连接
		isConnecting = false;
		//是否正在关闭
		isClosing = false;
		//消息列表
		msgList = new List<MsgBase>();
		//消息列表长度
		msgCount = 0;
	}

	//Connect回调
	private static void ConnectCallback()
	{

		Debug.Log("Socket Connect Succ ");
		FireEvent(NetEvent.ConnectSucc, "");
		isConnecting = false;

	}


	//关闭连接
	public static void Close()
	{
		//状态判断
		if (!isConnected)
		{
			return;
		}
		if (isConnecting)
		{
			return;
		}

		//没有数据在发送
		else
		{
			host.Dispose();
			FireEvent(NetEvent.Close, "");
		}
	}

	//发送数据
	public static void Send(MsgBase msg, bool Reliable = true)
	{
		//状态判断
		if (!isConnected)
		{
			return;
		}
		if (isConnecting)
		{
			return;
		}
		if (isClosing)
		{
			return;
		}
		//数据编码
		byte[] nameBytes = Codec.EncodeName(msg);
		byte[] bodyBytes = Codec.Encode(msg);

		int len = nameBytes.Length + bodyBytes.Length;
		byte[] sendBytes = new byte[2 + len];
		//组装长度
		sendBytes[0] = (byte)(len % 256);
		sendBytes[1] = (byte)(len / 256);
		//组装名字
		Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
		//组装消息体
		Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
		// 发送
		if (Reliable == true)
		{
			peer.Send_Reliable((byte)1, sendBytes, 0, sendBytes.Length);// 可靠发送
		}
		else
		{
			peer.Send_UnSequenced((byte)2, sendBytes,0,sendBytes.Length);// 非可靠发送
		}
		host.Flush();
	}

	public static void SendPlayerPing(MsgBase msg)
	{
		//状态判断
		if (!isConnected)
		{
			return;
		}
		if (isConnecting)
		{
			return;
		}
		if (isClosing)
		{
			return;
		}
		//数据编码
		byte[] nameBytes = Codec.EncodeName(msg);
		byte[] bodyBytes = Codec.Encode(msg);

		int len = nameBytes.Length + bodyBytes.Length;
		byte[] sendBytes = new byte[2 + len];
		//组装长度
		sendBytes[0] = (byte)(len % 256);
		sendBytes[1] = (byte)(len / 256);
		//组装名字
		Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
		//组装消息体
		Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
		// 发送

		peer.Send_UnSequenced((byte)3, sendBytes, 0, sendBytes.Length);// 可靠发送,通道3
		host.Flush();
	}


	//Receive回调
	public static void ReceiveCallback(ENet.Event enetEvent)
	{

		var packet = enetEvent.Packet.GetBytes();
		//var rchannel = enetEvent.ChannelID; // channelid 用来标识通道，不同的通道可以使用不同的通信模式，即可靠和不可靠。

		readBuff.Write(packet, 0, packet.Length); //将收到的数据包放入消息队列

		OnReceiveData(); // 可以考虑新开一个线程处理
		enetEvent.Packet.Dispose();

	}

	//数据处理
	public static void OnReceiveData()
	{
		//消息长度
		if (readBuff.length <= 2)
		{
			return;
		}
		//获取消息体长度
		int readIdx = readBuff.readIdx;
		byte[] bytes = readBuff.bytes;
		Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
		if (readBuff.length < bodyLength)
			return;
		readBuff.readIdx += 2;
		//解析协议名
		int nameCount = 0;
		string protoName = Codec.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
		//Debug.Log(protoName);
		if (protoName == "")
		{
			Debug.Log("OnReceiveData MsgBase.DecodeName fail");
			return;
		}
		
		//解析协议体
		int bodyCount = bodyLength - nameCount;

		if (bodyCount < 0)
		{
			Debug.Log("Receive Fail bodyCount = "+bodyCount);
			return;
		}


		readBuff.readIdx += nameCount;
		MsgBase msgBase = Codec.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
		readBuff.readIdx += bodyCount;
		readBuff.CheckAndMoveBytes();
		//添加到消息队列
		lock (msgList)
		{
			msgList.Add(msgBase);
			msgCount++;
		}
		//继续读取消息
		if (readBuff.length > 2)
		{
			OnReceiveData();
		}
	}

	//Update
	public static void Update()
	{
		EnetServiceUpdate(); // enet 时钟
		MsgUpdate(); // 派发网络消息
	}

	//更新消息
	public static void EnetServiceUpdate()
	{
		if (isConnected)
		{
			if (host.Service(1) >= 0)
			{
				ENet.Event enetEvent;
				try
				{
					while (host.CheckEvents(out enetEvent) > 0)
					{
						switch (enetEvent.Type)
						{
							case ENet.EventType.Connect:
								ConnectCallback();
								break;
							case ENet.EventType.Receive:
								ReceiveCallback(enetEvent);
								break;
							case ENet.EventType.Disconnect:
								Close();
								break;
						}
					}
				}
				catch (InvalidOperationException)
				{
					
				}

			}
		}
	}


	public static void MsgUpdate()
	{
		//初步判断，提升效率
		if (msgCount == 0)
		{
			return;
		}
		//重复处理消息
		for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
		{
			//获取第一条消息
			MsgBase msgBase = null;
			lock (msgList)
			{
				if (msgList.Count > 0)
				{
					msgBase = msgList[0];
					msgList.RemoveAt(0);
					msgCount--;
				}
			}
			//分发消息
			if (msgBase != null)
			{
				FireMsg(msgBase.Descriptor.Name, msgBase);
			}
			//没有消息了
			else
			{
				break;
			}
		}
	}
}
