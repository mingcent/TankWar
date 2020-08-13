using FixPoint;
using System;
using System.Collections.Generic;


public class BaseEntity :BaseLifeCycle
{
    public World world;
    public string PrefabName;
    public BaseEntityView view;

    public VTransform transform = new VTransform();

    public SCollisionComponent colliderComponent;         // 碰撞系统根据此属性生成相应的碰撞器

    public VCollisionShape coliderShape;
    protected List<BaseComponent> allComponents;

    protected void RegisterComponent(BaseComponent comp)
    {
        if (allComponents == null)
        {
            allComponents = new List<BaseComponent>();
        }
        allComponents.Add(comp);

        comp.BindEntity(this);
    }


    public override void DoAwake()
    {
        if (allComponents == null) return;
        foreach (var comp in allComponents)
        {
            comp.DoAwake();
        }
    }

    public override void DoStart()
    {
        if (allComponents == null) return;
        foreach (var comp in allComponents)
        {
            comp.DoStart();
        }
    }

    public override void DoUpdate(VInt deltaTime)
    {
        if (allComponents == null) return;
        foreach (var comp in allComponents)
        {
            comp.DoUpdate(deltaTime);
        }
    }

    public override void DoDestroy()
    {
        if (allComponents == null) return;
        foreach (var comp in allComponents)
        {
            comp.DoDestroy();
        }
    }
    public void OnDead()
    {
        world.stateService_.DestroyEntity(this);
    }

}

