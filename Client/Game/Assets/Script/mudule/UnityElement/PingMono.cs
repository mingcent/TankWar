using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PingMono : UnityEngine.MonoBehaviour
{
    [SerializeField] private int PingVal;
    private GUIStyle fontStyle = new GUIStyle(); 
    private void OnGUI()
    {

        fontStyle.fontSize = 30;
        PingVal = FrameBufferService.PingVal;
        if (PingVal < 100)
        {
            fontStyle.normal.textColor = new Color(0, 1, 0);
        }
        else if (PingVal < 300)
        {
            fontStyle.normal.textColor = new Color(1, 1, 0);
        }
        else
        {
            fontStyle.normal.textColor = new Color(1, 0, 0);
        }
        
        GUI.Label(new Rect(0, 0, 100, 100), $"Ping: {PingVal}ms Delay: {FrameBufferService.DelayVal}ms ",fontStyle);
        GUIStyle fontStyle2 = new GUIStyle();
        fontStyle2.fontSize = 40;
        fontStyle2.normal.textColor = new Color(1, 0, 0);
        GUI.Label(new Rect(0, 50, 100, 100), $"Frame Count: {BattleManager.world.PlayerInputFrameCount}", fontStyle2);
    }
}

