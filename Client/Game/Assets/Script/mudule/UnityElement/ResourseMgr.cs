using UnityEngine;

public class ResourseMgr
{
	//加载预设
	public static GameObject LoadPrefab(string path)
	{
		return Resources.Load<GameObject>(path);
	}
}
