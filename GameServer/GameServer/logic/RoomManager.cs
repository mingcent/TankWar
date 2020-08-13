using System;
using System.Collections.Generic;
using Pb;
using MsgBase = Google.Protobuf.IMessage;
public class RoomManager
{
	//最大id
	private static int maxId = 1;
	//房间列表
	public static Dictionary<int, Room> rooms = new Dictionary<int, Room>();

	//创建房间
	public static Room AddRoom(){
		maxId++;
		Room room = new Room();
		room.id = maxId;
		rooms.Add(room.id, room);
		return room;
	}

	//删除房间
	public static bool RemoveRoom(int id) {
		rooms.Remove(id);
		return true;
	}

	//获取房间
	public static Room GetRoom(int id) {
		if(rooms.ContainsKey(id)){
			return rooms[id];
		}
		return null;
	}

	//生成MsgGetRoomList协议
	public static MsgBase ToMsg(){
		MsgGetRoomList msg = new MsgGetRoomList();
		int count = rooms.Count;
		var roomInfos = new RoomInfo[count];
		//rooms
		int i = 0;
		foreach(Room room in rooms.Values){
			RoomInfo roomInfo = new RoomInfo();
			//赋值
			roomInfo.Id = room.id;
			roomInfo.Count = room.playerIds.Count;
			roomInfo.Status = (int)room.status;
			roomInfos[i] = roomInfo;
			i++;
		}
		msg.Rooms.Add(roomInfos);
		return msg;
	}

	//Update
	public static void Update(){
		foreach(Room room in rooms.Values){
			room.Update();
		}
	}
}


