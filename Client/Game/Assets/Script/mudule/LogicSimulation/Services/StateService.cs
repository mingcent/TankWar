using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using FixPoint;


public class StateService : BaseService
{
    public StateService(World _world)
    {
        world = _world;
    }

    // 存储游戏实体
    private Dictionary<string, Tank> _id2Tanks = new Dictionary<string, Tank>();
    private List<Bullet_> _bulletList = new List<Bullet_>();
    // 记录历史状态

    private Dictionary<int, GameState> _tick2Backup = new Dictionary<int, GameState>();

    public int PlayerCount { get => _id2Tanks.Count; }

    private void RemoveEntity(BaseEntity entity)
    {
        if (entity.GetType() == typeof(Tank))
        {
            var tank = entity as Tank;
            _id2Tanks.Remove(tank.id);
        }
        else if (entity.GetType() == typeof(Bullet_))
        {
            var bullet = entity as Bullet_;
            _bulletList.Remove(bullet);
        }
    }



    public Tank CreateTank(string prefabname, VInt3 position, VInt deg, string id)
    {
        var tank = new Tank();
        tank.world = world;
        tank.PrefabName = prefabname;
        tank.transform.position = position;
        tank.transform.deg = deg;
        tank.id = id;

        ViewMgr.BindEntityView(tank);

        tank.DoAwake();
        tank.DoStart();
        _id2Tanks[id] = tank;
        return tank;
    }

    public Bullet_ CreateBullet(string prefabname, VInt3 position, VInt deg, string id)
    {
        var bullet = new Bullet_();
        bullet.world = world;
        bullet.PrefabName = prefabname;
        bullet.transform.position = position;
        bullet.transform.deg = deg;
        bullet.id = id;

        ViewMgr.BindEntityView(bullet);
        bullet.DoAwake();
        bullet.DoStart();
        _bulletList.Add(bullet);
        return bullet;
    }


    public Dictionary<string, Tank> GetTanks()
    {
        return _id2Tanks;
    }

    public Bullet_[] GetBullets()
    {
        return _bulletList.ToArray();
    }


    public void DestroyEntity(BaseEntity entity)
    {
        if (entity != null)
        {
            entity.view?.OnDead();
            entity.DoDestroy();
            RemoveEntity(entity);
        } 
    }

    public void Reset()
    {
        foreach (var val in GetTanks().Values.ToArray())
        {
            DestroyEntity(val);
        }
        foreach (var val in GetBullets())
        {
            DestroyEntity(val);
        }
        _tick2Backup.Clear();
    }

    //////////////////////以上实现对Entity的增删改查，下面实现状态备份，为回滚做准备，为了简化序列化，只对Entity的状态进行序列化，不对整个对象进行序列化


    public void BackupState(int tick)

    {
        
        var tanksStateList = new List<TankState>();
        foreach (var pair in _id2Tanks)
        {
            var tankstate = new TankState();
            tankstate.transform.position = pair.Value.transform.position;
            tankstate.transform.deg = pair.Value.transform.deg;
            tankstate.id = pair.Value.id;
            tankstate.hp = pair.Value.hp;
            tankstate.fireInterval = pair.Value.fireInterval;
            tanksStateList.Add(tankstate);
        }

        var bulletsStateList = new List<BulletState>();

        foreach (var bullet in _bulletList)
        {
            var bulletstate = new BulletState();
            bulletstate.transform.position = bullet.transform.position;
            bulletstate.transform.deg = bullet.transform.deg;
            bulletstate.id = bullet.id;
            bulletstate.route = bullet.route;
            bulletsStateList.Add(bulletstate);
        }

        var currState = new GameState();
        currState.tanksState = tanksStateList.ToArray();
        currState.bulletsState = bulletsStateList.ToArray();

        _tick2Backup[tick] = currState;

    }

    public void RollbackTo(int tick)

    {
        if (_tick2Backup.TryGetValue(tick, out var backupData))
        {
            // 恢复坦克状态
            var tankState = backupData.tanksState;
            foreach (var val in tankState)
            {
                _id2Tanks[val.id].transform.position = val.transform.position;
                _id2Tanks[val.id].transform.deg = val.transform.deg;
                _id2Tanks[val.id].fireInterval = val.fireInterval;
                _id2Tanks[val.id].hp = val.hp;
            }
             
            // 删除并重新生成子弹
            foreach (var val in GetBullets())
            {
                DestroyEntity(val);
            }
            var bulletState = backupData.bulletsState;
            foreach (var val in bulletState)
            {
                var bullet = CreateBullet("bulletPrefab", val.transform.position, val.transform.deg, val.id);
                bullet.route = val.route;
            }
        }
    }

}



public class TankState
{
    public string id = "";
    public VTransform transform = new VTransform();
    public int hp;
    public VInt fireInterval;
}

public class BulletState
{
    public string id = "";
    public VTransform transform  = new VTransform();
    public VInt route;

}
public class GameState
{
    public TankState[] tanksState;
    public BulletState[] bulletsState;
}