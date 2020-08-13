using UnityEngine;

public class TankView : BaseEntityView
{
	//轮子和履带
	public Transform wheels;
	public Transform track;
	private Tank tank => entity as Tank;

	//移动速度,仅用于履带滚动的动画计算
	public float speed => tank.speed.i*0.001f ;

	void Awake()
	{
		//轮子履带
		wheels = gameObject.transform.Find("Wheels");
		track = gameObject.transform.Find("Track");
	}

    new void Update()
    {
		base.Update();
		if (tank != null)
		{
			WheelUpdate(tank.movDirection.i*0.001f);// 暂时不转
		}
    }
    public void WheelUpdate(float axis)
	{
		//计算速度
		float v = Time.deltaTime * speed * axis * 100;
		//旋转每个轮子
		foreach (Transform wheel in wheels)
		{
			wheel.Rotate(new Vector3(v, 0, 0), Space.Self);
		}
		//滚动履带
		MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer>();
		if (mr == null)
		{
			return;
		};
		Material mtl = mr.material;
		mtl.mainTextureOffset += new Vector2(0, v / 256);

	}

	public void OnTankDead()
    {
		if (tank?.IsDie() == true)
		{
			//显示焚烧效果
			GameObject obj = ResManager.LoadPrefab("fire");
			GameObject explosion = Instantiate(obj, transform.position, transform.rotation);
            explosion.transform.SetParent(transform);
		}
	}
}

