
using System;
//using UnityEngine;
using FixPoint;


[Serializable]
public abstract class VCollisionShape
{

	[NonSerialized]
	public bool dirty = true;

	[NonSerialized]
	public bool isBox;

	[NonSerialized]
	public BaseEntity owner;


	public abstract int AvgCollisionRadius
	{
		get;
	}

	public void OnEnable()
	{
		this.dirty = true;
	}

	public void ConditionalUpdateShape() // 碰撞检测前自动调用，更新自己的位置
	{
		// (this.dirty)
		//{
			this.UpdateShape(owner.transform.position, owner.transform.forward);
		//}
	}

	


	public static VCollisionShape InitEntityCollision(BaseEntity entity)
	{
		VCollisionShape vCollisionShape = null;
		if (null!=entity.colliderComponent)
		{
			vCollisionShape = entity.colliderComponent.CreateShape();
	
			if (vCollisionShape != null)
			{
				vCollisionShape.Born(entity);
			}
		}
		return vCollisionShape;
	}

	public virtual void Born(BaseEntity entity)
	{
		//entity.shape = this;
		this.owner = entity; 
	}

	public bool Intersects(VCollisionShape shape)
	{
		bool result = false;
		if (shape != null)
		{
			CollisionShapeType shapeType = shape.GetShapeType();
			if (shapeType == CollisionShapeType.Box)
			{
				result = this.Intersects((VCollisionBox)shape);
			}
			else if (shapeType == CollisionShapeType.CylinderSector)
			{
				result = this.Intersects((VCollisionCylinderSector)shape);
			}
			else
			{
				result = this.Intersects((VCollisionSphere)shape);
			}
		}
		return result;
	}

	public bool EdgeIntersects(VCollisionShape shape)
	{
		bool result = false;
		if (shape != null)
		{
			CollisionShapeType shapeType = shape.GetShapeType();
			if (shapeType == CollisionShapeType.Box)
			{
				result = this.EdgeIntersects((VCollisionBox)shape);
			}
			else if (shapeType == CollisionShapeType.CylinderSector)
			{
				result = this.EdgeIntersects((VCollisionCylinderSector)shape);
			}
			else
			{
				result = this.EdgeIntersects((VCollisionSphere)shape);
			}
		}
		return result;
	}

	public abstract bool Intersects(VCollisionBox obb);

	public abstract bool Intersects(VCollisionSphere s);

	public abstract bool Intersects(VCollisionCylinderSector cs);

	public abstract bool EdgeIntersects(VCollisionBox obb);

	public abstract bool EdgeIntersects(VCollisionSphere s);

	public abstract bool EdgeIntersects(VCollisionCylinderSector cs);

	public abstract void UpdateShape(VInt3 location, VInt3 forward);

	public abstract void UpdateShape(VInt3 location, VInt3 forward, int moveDelta);

	public abstract CollisionShapeType GetShapeType();

	public abstract void GetAabb2D(out VInt2 lt, out VInt2 size);

/*	public abstract bool AcceptFowVisibilityCheck(COM_PLAYERCAMP inHostCamp, GameFowManager fowMgr);*/
}
