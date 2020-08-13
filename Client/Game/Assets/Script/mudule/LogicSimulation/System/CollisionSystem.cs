using System;
using FixPoint;

public class CollisionSystem : BaseSystem
{
    private static DictionaryView<int, SCollisionComponent> s_componentCache = new DictionaryView<int, SCollisionComponent>();
    public override void DoUpdate(VInt deltaTime)
    {

        var tanks = world.stateService_.GetTanks();
        var bullets = world.stateService_.GetBullets();

        foreach (var bullet in bullets)
        {
            foreach (var tank in tanks.Values)
            {
                if(bullet == null) // 检测子弹是否已经销毁
                {
                    break;
                }
                var tankShape = tank.coliderShape;
                var bulletShape = bullet.coliderShape;
                if (bulletShape.Intersects(tankShape))
                {
                    tank.GetDemage();
                    bullet.OnExplosion(); //子弹击中第一个目标后会爆炸
                }
            }
        }

    }
}