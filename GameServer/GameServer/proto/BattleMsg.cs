
//坦克信息
[System.Serializable]
public class TankInfo{
	public string id = "";	//玩家id
	public int camp = 0;	//阵营
	public int hp = 0;		//生命值
	public int frameid = 0; // 

	public int x = 0;		//位置 放大1000倍，定点数
	public int y = 0;
	public int z = 0;
	public int ex = 0;  //旋转  放大1000倍，定点数
	public int ey = 0;
	public int ez = 0;
}


//进入战场（服务端推送）
public class MsgEnterBattle:MsgBase {
	public MsgEnterBattle() {protoName = "MsgEnterBattle";}
	//服务端回
	public TankInfo[] tanks;
	public int mapId = 1;	//地图，只有一张
}

//战斗结果（服务端推送）
public class MsgBattleResult:MsgBase {
	public MsgBattleResult() {protoName = "MsgBattleResult";}
	//服务端回
	public int winCamp = 0;	 //获胜的阵营
}

//玩家退出（服务端推送）
public class MsgLeaveBattle:MsgBase {
	public MsgLeaveBattle() {protoName = "MsgLeaveBattle";}
	//服务端回
	public string id = "";	//玩家id
}