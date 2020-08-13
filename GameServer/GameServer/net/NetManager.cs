using System;
using System.Net;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using ENet;
using MsgBase = Google.Protobuf.IMessage;
class NetManager
{
	//监听Socket
	public static Host host;
	//客户端Socket及状态信息
	public static Dictionary<Peer, ClientState> clients = new Dictionary<Peer, ClientState>();
	//ping间隔
	public static long pingInterval = 5;

	public static void StartLoop(int listenPort)
	{
		//新建host
		host = new Host();
		//
		host.Create((ushort)listenPort, 30);// 端口，最大连接数，待改进，暂时没有找到最大连接数的选项

		Console.WriteLine("[服务器]启动成功");
		//循环

		while (host.Service(1) >= 0)
		{
			Event enetEvent;

			while (host.CheckEvents(out enetEvent) > 0)
			{
				//Console.WriteLine("Server: " + enetEvent.Type.ToString());
				switch (enetEvent.Type)
				{
					case EventType.Connect:
						OnConnect(enetEvent.Peer);
						Console.WriteLine("玩家连接");
						break;

					case EventType.Receive:
						OnReceive(enetEvent);
						//enetEvent.Packet.Dispose();
						break;
					case EventType.Disconnect:
						OnClose(enetEvent.Peer);
						break;
				}
			}
			Timer();
		}




	}



	//读取Listenfd
	public static void OnConnect(Peer peer)
	{
		try
		{
			//Console.WriteLine("Accept " + clientfd.RemoteEndPoint.ToString());
			ClientState state = new ClientState();
			state.peer = peer;
			state.lastPingTime = GetTimeStamp();
			clients.Add(peer, state);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Accept fail" + ex.ToString());
		}
	}

	public static void OnClose(Peer peer)
	{
		if (clients.ContainsKey(peer))
		{
			var state = clients[peer];
			if (state != null)
			{
				//Console.WriteLine("Close: " + state.player.id);
				Close(state);
			}
		}
		
	}

	//关闭连接
	public static void Close(ClientState state)
	{
		//消息分发
		MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");
		object[] ob = { state };
		mei.Invoke(null, ob);
		//关闭
		state.peer.Disconnect(1);
		clients.Remove(state.peer);

	}

	//读取Clientfd
	public static void OnReceive(Event enetEvent)
	{	
		ClientState state = clients[enetEvent.Peer];
		ByteArray readBuff = state.readBuff;
		//接收
		int count = 0;
		//缓冲区不够，清除，若依旧不够，只能返回
		//当单条协议超过缓冲区长度时会发生
		if (readBuff.remain <= 0)
		{
			OnReceiveData(state);
			readBuff.MoveBytes();
		};
		if (readBuff.remain <= 0)
		{
			Console.WriteLine("Receive fail , maybe msg length > buff capacity");
			Close(state);
			return;
		}
		try
		{
			var recByte = enetEvent.Packet.GetBytes();
			readBuff.Write(recByte, 0, recByte.Length);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Receive SocketException " + ex.ToString());
			Close(state);
			return;
		}

		//处理二进制消息
		OnReceiveData(state);
		//移动缓冲区
		readBuff.CheckAndMoveBytes();
	}


	//数据处理
	public static void OnReceiveData(ClientState state)
	{
		ByteArray readBuff = state.readBuff;
		//消息长度
		if (readBuff.length <= 2)
		{
			return;
		}
		//消息体长度
		int readIdx = readBuff.readIdx;
		byte[] bytes = readBuff.bytes;
		Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
		if (readBuff.length < bodyLength)
		{
			return;
		}
		readBuff.readIdx += 2;
		//解析协议名
		int nameCount = 0;
		string protoName = Codec.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
		if (protoName == "")
		{
			Console.WriteLine("OnReceiveData MsgBase.DecodeName fail");
			Close(state);
			return;
		}
		readBuff.readIdx += nameCount;
		//解析协议体
		int bodyCount = bodyLength - nameCount;
		if (bodyCount < 0)
		{
			Console.WriteLine("OnReceiveData fail, bodyCount <0 ");
			Close(state);
			return;
		}
		MsgBase msgBase = Codec.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
		readBuff.readIdx += bodyCount;
		readBuff.CheckAndMoveBytes();
		//分发消息
		MethodInfo mi = typeof(MsgHandler).GetMethod(msgBase.Descriptor.Name);
		object[] o = { state, msgBase };

		
		if (mi != null)
		{
			mi.Invoke(null, o);
		}
		else
		{
			Console.WriteLine("OnReceiveData Invoke fail " + protoName);
		}
		//继续读取消息
		if (readBuff.length > 2)
		{
			OnReceiveData(state);
		}
	}




	//发送

	public static void Send(ClientState cs, MsgBase msg, PacketFlags flags = PacketFlags.Reliable)
	{
		//状态判断
		if (cs == null)
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
		//为简化代码，不设置回调
		try
		{
			if (flags == PacketFlags.Reliable)
			{
				cs.peer.Send_Reliable(1, sendBytes, 0, sendBytes.Length);
			}
			else if (flags == PacketFlags.Unsequenced)
			{
				cs.peer.Send_UnSequenced(2, sendBytes, 0, sendBytes.Length);
			}
			else
			{
				cs.peer.Send(0, sendBytes, 0, sendBytes.Length);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Socket Close on BeginSend" + ex.ToString());
		}
		host.Flush();
	}

	public static void SendPlayerPing(ClientState cs, MsgBase msg)
	{
		//状态判断
		if (cs == null)
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
		//为简化代码，不设置回调
		try
		{
			cs.peer.Send_UnSequenced(3, sendBytes, 0, sendBytes.Length);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Socket Close on BeginSend" + ex.ToString());
		}
		//host.Flush();

	}





	//定时器
	static void Timer()
	{
		//消息分发
		MethodInfo mei = typeof(EventHandler).GetMethod("OnTimer");
		object[] ob = { };
		mei.Invoke(null, ob);
	}

	//获取时间戳
	public static long GetTimeStamp()
	{
		TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
		return Convert.ToInt64(ts.TotalSeconds);
	}
}