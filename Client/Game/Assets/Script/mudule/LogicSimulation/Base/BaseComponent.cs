using FixPoint;
using System;



public class BaseComponent : BaseLifeCycle
{
    public BaseEntity baseEntity { get; private set; }
    public VTransform transform { get; private set; }

    public virtual void BindEntity(BaseEntity entity)
    {
        this.baseEntity = entity;
        transform = entity.transform;
    }

    public override void DoAwake() { }
    public override void DoStart() { }
    public override void DoUpdate(VInt deltaTime) { }
    public override void DoDestroy() { }

}