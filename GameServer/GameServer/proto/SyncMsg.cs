//同步坦克信息



public class MsgTankHp : MsgBase
{
	public MsgTankHp() { protoName = "MsgTankHp"; }
	public int hp;
}

public class MsgPlayerPing : MsgBase
{
	public MsgPlayerPing() { protoName = "MsgPlayerPing"; }
	public long sendTime;
	public long timeSinceGameStartMs;
}