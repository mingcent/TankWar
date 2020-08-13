using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pb;
using MsgBase = Google.Protobuf.IMessage;

public class Game
{
    public bool isRunning = false;
    public int Tick;
    public List<ServerFrame> _allHistoryFrames = new List<ServerFrame>(); //所有的历史帧

    public int PlayerCount; // 玩家数量
    public Room room;

    // time
    public int UpdateInterval = 30; // 30ms一帧
    public DateTime _startTime;
    private DateTime _lastUpdateTimeStamp;

    public long _gameStartTime = -1;
    public long realtimeSinceStartupMS =>
           (long)(DateTime.Now - _startTime).TotalMilliseconds; 
    public int _tickSinceGameStart =>
           (int)((realtimeSinceStartupMS-_gameStartTime) / UpdateInterval);

    //frame 
    public int ServerFrameCount;
    public Game(Room _room)
    {
        room = _room;
    }

    public void Start()
    {
        Tick = 0;
        _gameStartTime = -1;
        _startTime = _lastUpdateTimeStamp = DateTime.Now;
        isRunning = true;
        _allHistoryFrames.Clear();
        ServerFrameCount = 1;
    }

    public void Stop()
    {
        isRunning = false;
    }


    public void Update() // 外界驱动
    {
        if (!isRunning)
        {
            return;
        }
        var now = DateTime.Now;
        var _deltaTime = (int)(now - _lastUpdateTimeStamp).TotalMilliseconds;
        if (_deltaTime > UpdateInterval)
        {
            _lastUpdateTimeStamp = now;
            DoUpdate();
        }
    }

    private void DoUpdate() // 每30ms执行一次
    {
        if (!isRunning || _gameStartTime < 0) return;
        while (Tick < _tickSinceGameStart)  // 当前帧时间已到则强制推送帧，不在等待输入未到达的玩家
        {
            _CheckBorderServerFrame(true);
            Console.WriteLine("Update" );
        }
    }


    private bool _CheckBorderServerFrame(bool isForce = false)
    {
        if (!isRunning) return false;
        var frame = GetOrCreateFrame(Tick);
        var inputs = frame.Inputs;
        if (!isForce)
        {
            //是否所有的输入  都已经等到i
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].IsMissing == true)
                {
                    return false;
                }
            }
        }

        //Debug.Log($" Border input {Tick} isUpdate:{isForce} _tickSinceGameStart:{_tickSinceGameStart}");

        room.BroadcastFrames(); // Fix me 后期应考虑换成不可靠udp通道

        Tick++;
        return true;
    }



    public void SendMissingFrame(Player player, int misstick)
    {
        var msg = new MsgMissingServerFrames();

        int count = Tick - misstick; // 处理起始位置，每包三帧数据 实际为tick -1 -misstick+1，最后一个tick不发送，因为可能尚未建立，或玩家输入未全部收到

        //Console.WriteLine("SendMissingFrame count: "+count);

        var frames = new ServerFrame[count];
        for (int i = 0; i < count; i++)
        {
            frames[count - i - 1] = _allHistoryFrames[Tick-1 - i]; // 从倒数第二帧开始发送

        }

        msg.StartTick = frames[0].Tick;
        msg.Frames.Add(frames);

        player.Send(msg);
    }



    public void SendPlayerPing(Player player, MsgPlayerPing msg)
    {
        msg.TimeSinceGameStartMs = realtimeSinceStartupMS-_gameStartTime;
        player.SendPlayerPing(msg);
    }


    public void PushPlayerInput(Player player, MsgBase msg)
    {
        if (_gameStartTime < 0)
        {
            _gameStartTime = realtimeSinceStartupMS; // 收到第一帧数据时开始模拟
            Console.WriteLine(_gameStartTime);
        }
        var inputFrame = msg as MsgClientInputs;

        var tick = inputFrame.LastTick;
        
        var inputs = inputFrame.Inputs;

        if (inputs.Count >= 1)
        {
            player.frameCount = inputs.Count; // 以客服端预发送包帧数为服务器回包帧数
        }
        
        for (int i = inputs.Count - 1; i >= 0; i--) // 从后向前遍历 // 处理每包多帧情况
        {
            if (tick < Tick) // 晚到的帧直接丢弃
            {
                return;
            }
            var inputcmd = new PlayerInputCmd()
            {
                Cmd = inputs[i].Cmd
            };

            var frame = GetOrCreateFrame(tick);

            var id = player.frameid;
            frame.Inputs[id] = inputcmd;

            tick--;
        }


        _CheckBorderServerFrame(false); // 非强制发送，如果所有玩家帧收集完成则发送
    }


    private ServerFrame GetOrCreateFrame(int tick)
    {
        //扩充帧队列
        var frameCount = _allHistoryFrames.Count;
        if (frameCount <= tick)
        {
            var count = tick - _allHistoryFrames.Count + 1;
            for (int i = 0; i < count; i++)
            {
                _allHistoryFrames.Add(null);
            }
        }

        if (_allHistoryFrames[tick] == null)
        {
            _allHistoryFrames[tick] = new ServerFrame() { Tick = tick };
        }

        var frame = _allHistoryFrames[tick];
        if (frame.Inputs == null || frame.Inputs.Count != PlayerCount)
        {
            for(int i = 0;i<PlayerCount;i++)
            frame.Inputs.Add(new PlayerInputCmd() { IsMissing = true}); // 创建时即给默认输入
        }

        return frame;
    }
}

