using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillPanel : BasePanel {
	//界面开始显示的时间
	private float startTime = 0;

	//初始化
	public override void OnInit() {
		skinPath = "KillPanel";
		layer = PanelManager.Layer.Tip;
	}
	//显示
	public override void OnShow(params object[] args) {
		startTime = Time.time;
	}
		
	//关闭
	public override void OnClose() {

	}

	//当按下确定按钮
	public void Update(){
		if(Time.time - startTime > 2f){
			Close();
		}
	}
}
