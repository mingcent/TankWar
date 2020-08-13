using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pb;
using MsgBase = Google.Protobuf.IMessage;
public class BattlePanel : BasePanel {
	//hp
	private Image hpFill;
	private Text hpText;
	//info
	private Text camp1Text;
	private Text camp2Text;

	//初始化
	public override void OnInit() {
		skinPath = "BattlePanel";
		layer = PanelManager.Layer.Panel;
	}
	//显示
	public override void OnShow(params object[] args) {
		//寻找组件
		hpFill = skin.transform.Find("HpBar/Fill").GetComponent<Image>();
		hpText = skin.transform.Find("HpBar/HpText").GetComponent<Text>();
		camp1Text = skin.transform.Find("CampInfo/Camp1Text").GetComponent<Text>();
		camp2Text = skin.transform.Find("CampInfo/Camp2Text").GetComponent<Text>();
		ReflashCampInfo();

		NetManager.AddMsgListener("MsgLeaveBattle", OnMsgLeaveBattle);
		var tanks = BattleManager.world.stateService_.GetTanks();
		Tank tank = tanks[GameMain.id];
		if(tank != null){
			ReflashHp(tank.hp);
		}

	}


	//更新信息

	private void ReflashCampInfo(){
        int count1 = 0;
        int count2 = 0;
        foreach (Tank tank in BattleManager.world.stateService_.GetTanks().Values)
        {
            if (tank.IsDie())
            {
                continue;
            }

            if (tank.camp == 1) { count1++; };
            if (tank.camp == 2) { count2++; };
        }
        camp1Text.text = "红:" + count1.ToString();
        camp2Text.text = count2.ToString() + ":蓝";
    }

	//更新hp
	private void ReflashHp(int hp){
		if(hp < 0){hp=0;}
		hpFill.fillAmount = hp/100f;
		hpText.text = "hp:" + hp;
	}

	//关闭
	public override void OnClose() {
		NetManager.RemoveMsgListener("MsgLeaveBattle", OnMsgLeaveBattle);
	}

	//收到玩家退出协议
	public void OnMsgLeaveBattle(MsgBase msgBase){
		ReflashCampInfo();
	}

	//收到击中协议




	public int count = 10;
    private void Update()
    {
		if (count > 0)
		{
			count--;
			return;
		}
		count = 10;

		var tanks = BattleManager.world.stateService_.GetTanks();
		Tank tank = tanks[GameMain.id];
		if (tank != null)
		{
			ReflashHp(tank.hp);
		}
		ReflashCampInfo();
	}
}
