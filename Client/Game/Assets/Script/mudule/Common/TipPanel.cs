using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel {
	//提示文本
	private Text text;
	//确定按钮
	private Button okBtn;

	//初始化
	public override void OnInit() {
		skinPath = "TipPanel";
		layer = PanelManager.Layer.Tip;
	}
	//显示
	public override void OnShow(params object[] args) {
		//寻找组件
		text = skin.transform.Find("Text").GetComponent<Text>();
		okBtn = skin.transform.Find("OkBtn").GetComponent<Button>();
		//监听
		okBtn.onClick.AddListener(OnOkClick);
		//提示语
		if(args.Length == 1){
			text.text = (string)args[0];
		}
	}
		
	//关闭
	public override void OnClose() {

	}

	//当按下确定按钮
	public void OnOkClick(){
		Close();
	}
}
