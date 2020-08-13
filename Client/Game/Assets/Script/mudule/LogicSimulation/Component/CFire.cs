using FixPoint;
using System;
using System.Diagnostics;

public partial class CFire : BaseComponent
{
    public Tank Owner => (Tank)baseEntity;
    public PlayerInput input => Owner.input;

    public VInt3 pos => Owner.firepos + Owner.transform.position;
    public VInt deg => Owner.transform.deg;

    public override void DoUpdate(VInt deltaTime)
    {

        Owner.fireInterval += deltaTime;

        var isFire = input.isInputFire;
        if (isFire&& Owner.fireInterval > Owner.fireCD)
        {
            //UnityEngine.Debug.Log(deg);
            Owner.world.stateService_.CreateBullet("bulletPrefab",pos,deg,Owner.id);
            Owner.fireInterval = 0;
        }
    }
}
