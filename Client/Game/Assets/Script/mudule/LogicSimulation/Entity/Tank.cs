using FixPoint;
using System;
using Pb;


public class Tank :BaseEntity
{
	
	// 属于哪一名玩家
	public string id = "";

	public int frameId;
	// 阵营
	public int camp = 0; 
	// 血量
	public int hp = 100;

	public TankView tankview => view as TankView;

	public PlayerInput input = new PlayerInput();
	public CTankMover mover = new CTankMover();
	public CFire fire = new CFire();
	// 移动速度
	public VInt speed = 10000;
	// 转动速度
	public VInt steer = 60000;

	public VInt movDirection = 0;
	// 子弹发射位置
	public VInt3 firepos
	{
		get => new VInt3(0, 5400, 0)+transform.forward * new VInt(11000);
	}
	// 子弹cd
	public VInt fireCD = 500;

	public VInt fireInterval = 0;// 距离上一次发射的时间



	public Tank()
	{
		colliderComponent = new SCollisionComponent();
		colliderComponent.shapeType = CollisionShapeType.Box;
		colliderComponent.Pos = new VInt3(0, 2500, 1470);
		colliderComponent.Size = new VInt3(7000, 5000, 10000);

		coliderShape = VCollisionShape.InitEntityCollision(this);
	}


	public void GetDemage()
	{
		if (!IsDie())
		{
			hp -= 10;
			////////////////////////////////////同步血量到服务器
			if (id == GameMain.id)
			{
				MsgTankHp msg = new MsgTankHp();
				msg.Hp = hp;
				NetManager.Send(msg);
			}
			//////////////////////////////////////
			if (IsDie())
			{
   				tankview?.OnTankDead();
			}
		}

	}

	public bool IsDie()
	{
		return hp<=0;
	}
    public override void DoAwake()
    {
        base.DoAwake();
		RegisterComponent(mover);
		RegisterComponent(fire);
    }

    public override void DoStart()
    {
        base.DoStart();
    }

    public override void DoUpdate(VInt deltaTime)
    {
        base.DoUpdate(deltaTime);
		//UnityEngine.Debug.Log(DateTime.Now+ id + " " + hp);

    }

}

