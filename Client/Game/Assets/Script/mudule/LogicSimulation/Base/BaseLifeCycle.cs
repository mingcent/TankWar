using System;
using FixPoint;


public class BaseLifeCycle
{
    public virtual void DoAwake() { }
    public virtual void DoStart() { }
    public virtual void DoUpdate(VInt deltaTime) { }
    public virtual void DoDestroy() { }
}
