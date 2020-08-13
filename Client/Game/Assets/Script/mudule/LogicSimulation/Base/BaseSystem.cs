using System;
using FixPoint;

public class BaseSystem
{
    public World world;

    public bool enable = true;
    public virtual void DoUpdate(VInt deltaTime) { }
}