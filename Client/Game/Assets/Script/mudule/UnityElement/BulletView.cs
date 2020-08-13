using UnityEngine;

public class BulletView : BaseEntityView
{ 
	public void OnExplosion()
	{
		base.ForceUpdate(); // 更新子弹位置至碰撞位置

		GameObject explode = ResManager.LoadPrefab("explosion");
		Instantiate(explode, transform.position, transform.rotation);
	}
}

