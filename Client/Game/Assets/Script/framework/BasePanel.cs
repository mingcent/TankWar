using UnityEngine;
using System.Collections;

public class BasePanel : MonoBehaviour {
	//皮肤路径
	public string skinPath;
	//皮肤
	public GameObject skin;
	//层级
	public PanelManager.Layer layer = PanelManager.Layer.Panel;
	//初始化
	public void Init(){
		//皮肤
		GameObject skinPrefab = ResManager.LoadPrefab(skinPath);
		skin = (GameObject)Instantiate(skinPrefab);
	}
	//关闭
	public void Close(){
		string name = this.GetType().ToString();
		PanelManager.Close(name);
	}

	//初始化时
	public virtual void OnInit(){
	}
	//显示时
	public virtual void OnShow(params object[] para){
	}
	//关闭时
	public virtual void OnClose(){
	}
		
}
