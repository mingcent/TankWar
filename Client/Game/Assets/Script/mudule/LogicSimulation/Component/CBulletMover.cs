using FixPoint;
using System;


public partial class CBulletMover : BaseComponent
{
    public Bullet_ Owner => (Bullet_)baseEntity;

    public VInt speed => Owner.speed;

    public override void DoUpdate(VInt deltaTime)
    {
        // 移动

        VInt3 s = transform.forward * speed * deltaTime;
        transform.position += s;
        // 射程累积
        Owner.route += speed * deltaTime;
    }
}
