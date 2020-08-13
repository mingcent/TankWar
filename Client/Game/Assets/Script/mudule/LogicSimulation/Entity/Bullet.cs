using System;
using FixPoint;


public class Bullet_ : BaseEntity
{
    // 发射该子弹的玩家id
    public string id = "";
    public CBulletMover mover = new CBulletMover();
    // 移动速度
    public VInt speed = 300000;

    // 射程
    public VInt distance = 1000000;
    // 飞行距离
    public VInt route;
    public BulletView  bulletview => view as BulletView;
    // 标记是否已经销毁，用于标记，防止对坦克造成二次伤害，之后统一销毁子弹
    public bool isDead = false;
    public Bullet_()
    {
        colliderComponent = new SCollisionComponent();
        colliderComponent.shapeType = CollisionShapeType.Sphere;
        colliderComponent.Pos = new VInt3(0, 0, 0);
        colliderComponent.Size = new VInt3(500, 0, 0);

        coliderShape = VCollisionShape.InitEntityCollision(this);
    }




    public override void DoAwake()
    {
        base.DoAwake();
        RegisterComponent(mover);
    }

    public override void DoStart()
    {
        base.DoStart();
    }

    public override void DoUpdate(VInt deltaTime)
    {
        base.DoUpdate(deltaTime);
        if (route > distance)
        {
            OnExplosion();
        }
    }

    public void OnExplosion()
    {
        bulletview?.OnExplosion();
        OnDead();
    }
}

