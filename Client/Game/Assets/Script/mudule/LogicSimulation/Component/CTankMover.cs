using FixPoint;
using System;
using UnityEngine.Networking;

[Serializable]
public partial class CTankMover : BaseComponent
{
    public Tank Owner => (Tank)baseEntity;
    public PlayerInput input => Owner.input;

    public VInt speed => Owner.speed;
    public VInt steer => Owner.steer;

    public override void DoUpdate(VInt deltaTime)
    {

        var needChase = input.inputUV.sqrMagnitude > 10; // 是否有输入
        Owner.movDirection = 0;
        if (needChase && !Owner.IsDie())
        {
            // 旋转
            var x = input.inputUV.x;
            // 旋转
            transform.Rotate(x * steer * deltaTime);
            // 前进后退
            VInt y = input.inputUV.y;

            Owner.movDirection = y; // 用于履带的旋转方向

            VInt3 s = transform.forward * y * speed * deltaTime;
            transform.position += s;
        }
    }
}
