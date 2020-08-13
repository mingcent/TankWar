using FixPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pb;

public class EntityUpdateSystem :BaseSystem
{
    public override void DoUpdate(VInt deltaTime)
    {
        base.DoUpdate(deltaTime);

        foreach (var tank in world.stateService_.GetTanks())
        {
            tank.Value.DoUpdate(deltaTime);
        }

        foreach (var bullet in world.stateService_.GetBullets())
        {
            bullet.DoUpdate(deltaTime);
        }

    }
    public void UpdateFrame(ServerFrame frame)
    {
        var tanks = world.stateService_.GetTanks();
    }
}

