using System;
using FixPoint;

public class SCollisionComponent
{
	public CollisionShapeType shapeType = CollisionShapeType.Sphere;

	public VInt3 Pos = VInt3.zero;

	public VInt3 Size = new VInt3(500, 500, 500);

	public VInt3 Size2 = new VInt3(0, 0, 0);

	public VCollisionShape CreateShape()
	{
		/*		DebugHelper.Assert(!Singleton<BattleLogic>.instance.isFighting || Singleton<GameLogic>.instance.bInLogicTick || Singleton<FrameSynchr>.instance.isCmdExecuting);*/
		VCollisionShape result = null;
		switch (this.shapeType)
		{
			case CollisionShapeType.Box:
				result = new VCollisionBox
				{
					Size = this.Size,
					Pos = this.Pos
				};
				break;
			case CollisionShapeType.Sphere:
				result = new VCollisionSphere
				{
					Pos = this.Pos,
					Radius = this.Size.x
				};
				break;
			case CollisionShapeType.CylinderSector:
				{
					VCollisionCylinderSector cylinder = new VCollisionCylinderSector();
					cylinder.Pos = this.Pos;
					cylinder.Radius = this.Size.x;
					cylinder.Height = this.Size.y;
					cylinder.Degree = this.Size.z;
					cylinder.Rotation = this.Size2.x;
					result = cylinder;
					break;
				}
		}
		return result;
	}
}
