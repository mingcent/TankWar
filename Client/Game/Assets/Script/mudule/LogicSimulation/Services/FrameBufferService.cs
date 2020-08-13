using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Pb;
using System.Net.NetworkInformation;
using System.Diagnostics;
using UnityEngine;

public class FrameBufferService : BaseService
{

    // buffer
    private int _bufferSize;
    // 未确认的服务器帧的最大数量，防止出现 ring buff 的覆盖，这里等于buffersize-1；如果状态的不是每一帧都有备份，这里就应该根据帧间隔确定
    private int _maxServerOverFrameCount;
    // 客户端最大预测帧数
    private int _maxClientPredictFrameCount = 30;


    private ServerFrame[] _serverBuffer;
    private ServerFrame[] _clientBuffer;



    // 用于计算ping值

    
    public static int PingVal { get; private set; }
    public static int DelayVal { get; private set; }


    private List<long> _pings = new List<long>();
    private List<long> _delays = new List<long>();
    private Dictionary<int, long> _tick2SendTime = new Dictionary<int, long>();
    private int _pingTimer;


    private long _guessServerStartTimestamp = Int64.MaxValue;
    private long _historyMinPing = Int64.MaxValue;
    private long _minPing = Int64.MaxValue;
    private long _maxPing = 0;

    // 已经收到丢失数据包
    private bool hasReceiveMissing = true;

    //下一要确认的帧
    public int NextTickToCheck { get; private set; }

    /// the tick client need run in next update
    private int _nextClientTick;

    // 服务器的当前帧，即收到的最大帧号
    public int CurTickInServer { get; private set; }

    // 存储在buffer中的最大帧，正常来说服务器的当前帧和最大帧应该是相等的，当未确认的帧数超过buff的长度，即首位相连时，curr会大于max
    public int MaxServerTickInBuffer { get; private set; } = -1;

    // 收到的连续帧的最大帧号，用于检测是否出现丢帧
    public int MaxContinueServerTick { get; private set; }
    // 是否需要，即出现确认失败
    public bool IsNeedRollback { get; private set; }

    public void SetClientTick(int tick)
    {
        _nextClientTick = tick + 1;
    }

    private PredictCountHelper helper;

    public void Reset()
    {
        MaxServerTickInBuffer = -1;
        MaxContinueServerTick = 0;
        NextTickToCheck = 0;
        CurTickInServer = 0;
        hasReceiveMissing = true;
        IsNeedRollback = false;
        _serverBuffer = new ServerFrame[_bufferSize];
        _clientBuffer = new ServerFrame[_bufferSize];
        helper = new PredictCountHelper(world, this);

    }


    public FrameBufferService(World _world)
    {

        world = _world;

        _bufferSize = 2000;
        _maxServerOverFrameCount = _bufferSize - 1;
        

    }

    public void OnPlayerPing(MsgPlayerPing msg)
    {
        //PushServerFrames(frames, isNeedDebugCheck);
        var ping = world.realtimeSinceStartupMS - msg.SendTime;
        _pings.Add(ping);
        if (ping > _maxPing) _maxPing = ping;
        if (ping < _minPing)
        {
            _minPing = ping;

            _guessServerStartTimestamp = 
                (world.realtimeSinceStartupMS - msg.TimeSinceGameStartMs) - _minPing / 2;

            if(_guessServerStartTimestamp <0) // 防止出现gameStarttime<0的情况
            {
                _guessServerStartTimestamp = 0;
            }
        }

        //Debug.Log("OnPlayerPing " + ping);
    }

    public void PushLocalFrame(ServerFrame frame)
    {
        var sIdx = frame.Tick % _bufferSize;
        //  Fix me
        // 确保 Assert(_clientBuffer[sIdx] == null || _clientBuffer[sIdx].tick <= frame.tick)     
        _clientBuffer[sIdx] = frame;
    }



    public void PushMissingFrames(ServerFrame[] frame)
    {
        hasReceiveMissing = true;
        PushServerFrames(frame);
    }
    public void PushServerFrames(ServerFrame[] frames)
    {
        world._hasReceiveServerFrame = true;// 收到服务器帧，开始进行游戏模拟

        var lastTick = frames[frames.Length - 1].Tick;
        //PrintLog.print("+"+lastTick);
        if (CurTickInServer+1 != lastTick)// 每包最后一帧不连续则发生丢帧
        {
            helper.hasMissTick = true;
            //PrintLog.print("Miss Server Frame"+ CurTickInServer + " "+ lastTick);
        }


        var count = frames.Length;
        for (int i = 0; i < count; i++)
        {
            var data = frames[i];

            if (_tick2SendTime.TryGetValue(data.Tick, out var sendTick)) // 计算ping值，往返时间
            {
                var delay = world.realtimeSinceStartupMS - sendTick;
                _delays.Add(delay);
                _tick2SendTime.Remove(data.Tick);
            }


            if (data.Tick < NextTickToCheck) // 该帧已经确认
            {
                continue;
            }

            if (data.Tick > CurTickInServer)
            {
                CurTickInServer = data.Tick;
            }

            if (data.Tick >= NextTickToCheck + _maxServerOverFrameCount - 1)
            {
                //to avoid ringBuffer override the frame that have not been checked
                continue;
            }

            if (data.Tick > MaxServerTickInBuffer)
            {
                MaxServerTickInBuffer = data.Tick;
            }

            var targetIdx = data.Tick % _bufferSize;
            if (_serverBuffer[targetIdx] == null || _serverBuffer[targetIdx].Tick != data.Tick)
            {
                _serverBuffer[targetIdx] = data;
                //if (data.Tick > helper.nextCheckMissTick && data.Inputs[World.localFrameid].IsMissing && helper.missTick == -1)
                if (data.Inputs[World.localFrameid].IsMissing && helper.missTick == -1)
                    { // 服务器出现丢帧则提高提前量
                    helper.missTick = data.Tick;
                    //PrintLog.print("Miss "+data.Tick);
                }
            }
        }
    }



    public ServerFrame GetFrame(int tick)
    {
        var sFrame = GetServerFrame(tick);
        if (sFrame != null)
        {
            return sFrame;
        }

        return GetLocalFrame(tick);
    }

    public ServerFrame GetServerFrame(int tick)
    {
        if (tick > MaxServerTickInBuffer)
        {
            return null;
        }

        return _GetFrame(_serverBuffer, tick);
    }

    public ServerFrame GetLocalFrame(int tick)
    {
        if (tick >= _nextClientTick)
        {
            return null;
        }

        return _GetFrame(_clientBuffer, tick);
    }


    public ServerFrame GetInputFrame(int tick)
    {

        return _GetFrame(_clientBuffer, tick);
    }

    private ServerFrame _GetFrame(ServerFrame[] buffer, int tick)
    {
        var idx = tick % _bufferSize;
        var frame = buffer[idx];
        if (frame == null) return null;
        if (frame.Tick != tick) return null;
        return frame;
    }

    // 发送输入

    public void  SendInput(MsgClientInputs msg)
    {
        _tick2SendTime[msg.LastTick] = world.realtimeSinceStartupMS; // 记录发送时间

        NetManager.Send(msg, false);
    }



    // 对帧进行确认
    public void DoUpdate(int deltaTime)
    {
        NetService.SendPlayerPing(world.realtimeSinceStartupMS); // 发送ping请求

       
        helper.DoUpdate(deltaTime);
        UpdatePingVal(deltaTime);


        int worldTick = world.Currtick;
        IsNeedRollback = false;
        while (NextTickToCheck <= MaxContinueServerTick && NextTickToCheck < worldTick)
        {
            var sIdx = NextTickToCheck % _bufferSize;
            var cFrame = _clientBuffer[sIdx];
            var sFrame = _serverBuffer[sIdx];
            if (cFrame == null || cFrame.Tick != NextTickToCheck || sFrame == null ||
                sFrame.Tick != NextTickToCheck)
                break;
            //Check client guess input match the real input
            if (object.ReferenceEquals(sFrame, cFrame) || sFrame.Equals(cFrame)) // 比较两帧数据是否相等，Fix me 待检验Equal是否有效
            {
                NextTickToCheck++;
            }
            else
            {
                IsNeedRollback = true;
                break;
            }
        }


        if(hasReceiveMissing) //已经收到上次的丢包请求返回数据再进行下次丢包检测
        {
            // 以下判断是否发生丢帧
            int tick = NextTickToCheck;
            for (; tick <= MaxServerTickInBuffer; tick++)
            {
                var idx = tick % _bufferSize;
                if (_serverBuffer[idx] == null || _serverBuffer[idx].Tick != tick)
                {
                    break;
                }
            }

            MaxContinueServerTick = tick - 1;
            if (MaxContinueServerTick <= 0) return;
            if (MaxContinueServerTick < CurTickInServer-3 // has some middle frame pack was lost 客户端缓存大小为3
                || _nextClientTick >
                MaxContinueServerTick + (_maxClientPredictFrameCount - 3) //client has predict too much             ?? 为何要减3
            )
            {
                //Debug.Log("SendMissFrameReq " + MaxContinueServerTick);
                NetService.SendMissFrameReq(MaxContinueServerTick);
                hasReceiveMissing = false;
            }
        }
       
    }


    private void UpdatePingVal(int deltaTime)
    {
        _pingTimer += deltaTime;
        if (_pingTimer > 500) // 
        {
            _pingTimer = 0;
            DelayVal = (int)(_delays.Sum() / Math.Max(_delays.Count, 1));
            _delays.Clear();

            var Ping = (int)(_pings.Sum() / Math.Max(_pings.Count, 1));
            _pings.Clear();

            if (Ping != 0)
            {
                PingVal = Ping;
            }

             if (_minPing < _historyMinPing && world.gameStartTime != -1) {
                    _historyMinPing = _minPing;
                    world.gameStartTime = Math.Min(_guessServerStartTimestamp,world.gameStartTime);
                //PrintLog.print(" world.gameStartTime................................................ "+ world.gameStartTime);
                //UnityEngine.Debug.Log(" world.gameStartTime " + world.gameStartTime);
            }

            _minPing = Int64.MaxValue;
            _maxPing = 0;
        }
    }


    public class PredictCountHelper
    {
        public PredictCountHelper(World _world, FrameBufferService cmdBuffer)
        {
            this._cmdBuffer = cmdBuffer;
            this.world = _world;
        }

        public int missTick = -1;
        public int nextCheckMissTick = 0;
        public bool hasMissTick;

        private World world;
        private FrameBufferService _cmdBuffer;
        private int _timer;
        private int _checkInterval = 2000;
        //private int missCount = 0;
        private int lastMissTick = -1;
        private int continueMissingCount = 0;
        private int bufferCount = 1;

        public void DoUpdate(int deltaTime)
        {
            _timer += deltaTime;

            if (_timer > _checkInterval)
            {
                _timer = 0;
                if (!hasMissTick)
                { //一定时间内没有 lost pack 
                    //var preSend = _cmdBuffer._maxPing * 1.0f / world.UpdateInterval;

                    var delayTick = (int)(PingVal * 0.5f / World.UpdateInterval);

                    if (bufferCount > 1)
                    {
                        bufferCount--;
                    }

                    var targetFramePredictCount = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.CeilToInt(bufferCount + delayTick), 0, 60);

                    world.FramePredictCount = targetFramePredictCount;
                    world.PreSendInputCount = targetFramePredictCount + 2;

                    world.PlayerInputFrameCount = bufferCount;// > 3 ? _preTickCount : 3;

                    //PrintLog.print("targetFramePredictCount:" + targetFramePredictCount + " ping:"+PingVal +" delayTick:" + delayTick + " _preTickCount:" + world.PlayerInputFrameCount);

                    //PrintLog.print("MaxPing targetFramePredictCount:" + NetManager.peer.RoundTripTime);
                }

                hasMissTick = false;
                //PrintLog.print("hasMissTick = false;");
            }

            if (missTick != -1)
            { // 发生丢帧
              //var delayTick = world.TargetTick - missTick;
              // 判断连续丢帧情况，连续丢帧不应增加buffer
                //UnityEngine.Debug.Log("continue " + lastMissTick +" "+ missTick);
                if ((missTick - lastMissTick) == 1)
                {
                    continueMissingCount++;
                }else
                {
                    continueMissingCount = 0;
                }
                lastMissTick = missTick;

                var delayTick =(int)(PingVal * 0.5f / World.UpdateInterval);

                UnityEngine.Debug.Log("continue "+ continueMissingCount);
                
                if (_cmdBuffer._maxPing< 500 && bufferCount < 15 && continueMissingCount <1) // 最多发15包
                {
                    bufferCount++;
                }

                var targetFramePredictCount = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.CeilToInt(bufferCount + delayTick), 0, 60);

                //UnityEngine.Debug.Log("PreSendInputCount2 " + targetPreSendTick);

                world.FramePredictCount = targetFramePredictCount;
                world.PreSendInputCount = targetFramePredictCount + 2;

                world.PlayerInputFrameCount = bufferCount;//> 3 ? _preTickCount : 3;

                //PrintLog.print("Loss targetFramePredictCount:" + targetFramePredictCount + " ping:" + PingVal + " delayTick:" + delayTick);

                nextCheckMissTick = world.TargetTick;
                missTick = -1;
                hasMissTick = true;
            }

        }
    }


}

