using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using FixPoint;
using Pb;


public class World
{
    public static bool isRuning = false;

    public static string localid = "";// 本地玩家id
    public static int localFrameid; //本地玩家的帧位置

    public const int MaxPredictFrameCount = 30; // 最大预测帧数，超过时客户端会卡住
    public const long MaxSimulationMsPerFrame = 20; // 最大追帧时间

    public bool _hasReceiveServerFrame = false;
    // time
    public DateTime _gameStartTime;

    
    public static int UpdateInterval = 30; //  固定帧间隔
    public int RealUpdateInterval; //实际更新时间间隔


    public DateTime _startTime;
    private DateTime _lastUpdateTime;
    public long gameStartTime = -1; // 游戏开始模拟第一帧的时间
    public long realtimeSinceStartupMS =>
           (long)(DateTime.Now - _startTime).TotalMilliseconds;
    public int _tickSinceGameStart =>
           (int)((realtimeSinceStartupMS-gameStartTime) / UpdateInterval);


    // tick
    public int Currtick; // 当前执行帧
    public int inputTick;// 发送帧

    public int PreSendInputCount = 2; //客户端预发送帧数
    public int FramePredictCount = 0; //客户端超前服务器帧数
    public int inputTargetTick => _tickSinceGameStart + PreSendInputCount;
    public int TargetTick => _tickSinceGameStart + FramePredictCount;

    // frame
    public int PlayerInputFrameCount; // 每个输入包的操作帧数


    // services
    public StateService stateService_;
    public FrameBufferService frameBufferService_;

    // system
    public EntityUpdateSystem entityUpdateSystem_;
    public CollisionSystem collisionSystem_;


    public void Init()
    {
        // service init
        stateService_ = new StateService(this);
        frameBufferService_ = new FrameBufferService(this);
        NetService.world = this;

        // system init
        entityUpdateSystem_ = new EntityUpdateSystem { world = this };
        collisionSystem_ = new CollisionSystem { world = this };

    }

    public void Reset()
    {
        Currtick = 0;
        stateService_.Reset();
        frameBufferService_.Reset();
        RealUpdateInterval = UpdateInterval;
        PlayerInputFrameCount = 1;

    }

    public void Start()
    {
        Currtick = 0;
        inputTick = 0;
        isRuning = true;
        _startTime = DateTime.Now;
        _lastUpdateTime = DateTime.Now;

        gameStartTime = -1;// 未收到服务器帧前不进行模拟
        _hasReceiveServerFrame = false;

        //UnityEngine.Debug.Log("start");
        while (inputTick < PreSendInputCount)
        {
            SendInputs(inputTick++);
            //UnityEngine.Debug.Log(inputTick+ " start");
        }
    }

    public void Stop()
    {
        isRuning = false;
    }



    public void Update() // 外界驱动
    {
        if(!isRuning)
        {
            return;
        }
        var now = DateTime.Now;
        var _deltaTime = (int)(now - _lastUpdateTime).TotalMilliseconds;
        if (_deltaTime > RealUpdateInterval)
        {
            _lastUpdateTime = now;
            UpdateLogic(_deltaTime);

        }
    }



    private void UpdateLogic(int deltaTime)
    {
        if (_hasReceiveServerFrame && gameStartTime < 0)
        {
            gameStartTime = realtimeSinceStartupMS;
            //UnityEngine.Debug.Log(gameStartTime);
        }

        if (gameStartTime < 0)
        {
            return;
        }
        frameBufferService_.DoUpdate(deltaTime); // 对帧进行确认

        while (inputTick <= inputTargetTick)   // 保证发送帧跟随时间不断递增，
        {
            SendInputs(inputTick++);
            //UnityEngine.Debug.Log(inputTick+" Update Logic "+ inputTargetTick);
        }

        /////////////////////////////////////

        var maxContinueServerTick = frameBufferService_.MaxContinueServerTick;
        if ((Currtick - maxContinueServerTick) > MaxPredictFrameCount) // 当前执行帧超过服务器确认帧太多时，不会执行当前帧，客户端会卡住
        {
            //PrintLog.print("predict toomuch Currtick" + Currtick + "maxContinueServerTick"+maxContinueServerTick);
            return;
        }


        // Pursue Server frames 追帧
        var deadline = realtimeSinceStartupMS + MaxSimulationMsPerFrame;
        while (Currtick < frameBufferService_.MaxContinueServerTick-10) // 当前执行帧小于服务器帧
        {
            var tick = Currtick;
            var sFrame = frameBufferService_.GetServerFrame(tick);
            // UnityEngine.Debug.Log("Pursue111 " + Currtick + " " + frameBufferService_.MaxContinueServerTick);
            if (sFrame == null)
            {
                return;
            }

            frameBufferService_.PushLocalFrame(sFrame);

            Step(sFrame);

            //UnityEngine.Debug.Log("Pursue"+ Currtick + " "+frameBufferService_.CurTickInServer);
            
            if (realtimeSinceStartupMS > deadline) // 追帧超时，下一帧在追
            {
                //OnPursuingFrame();
                //PrintLog.print("pursing deadline "+realtimeSinceStartupMS + " " + deadline);
                return;
            }

        }


        // Roll back
        if (frameBufferService_.IsNeedRollback)
        {
            RollbackTo(frameBufferService_.NextTickToCheck); // 回滚到未check的帧处

            while (Currtick <= maxContinueServerTick) // 回滚后追帧
            {
                var sFrame = frameBufferService_.GetServerFrame(Currtick);
                frameBufferService_.PushLocalFrame(sFrame);
                Step(sFrame);
                //UnityEngine.Debug.Log(DateTime.Now + " Roll back");
            }
        }


        //Run frames
        while (Currtick <= TargetTick) // 当前执行帧小于随时间应该执行到的帧
        {
            var curTick = Currtick;
            ServerFrame frame = null;
            var sFrame = frameBufferService_.GetServerFrame(curTick);
            if (sFrame != null)
            {
                frame = sFrame;
                //UnityEngine.Debug.Log(" server frames");
            }
            else
            {
                //UnityEngine.Debug.Log(" local frames");
                var cFrame = frameBufferService_.GetLocalFrame(curTick);
                FillInputWithLastFrame(cFrame); // 二次预测
                frame = cFrame;
            }

            frameBufferService_.PushLocalFrame(frame);
            Step(frame);
            //UnityEngine.Debug.Log(DateTime.Now + " Run frames");
        }

    }




    void Step(ServerFrame frame)
    {

        Backup(Currtick); // 备份当前状态

        ProcessInputQueue(frame); // 派发输入到各个坦克

        for (int i = 0; i < 2; i++) // 每一逻辑帧分两步执行
        {
            // 更新游戏
            entityUpdateSystem_.DoUpdate(UpdateInterval/2);
            collisionSystem_.DoUpdate(UpdateInterval/2);
        }

        Currtick++;

        frameBufferService_.SetClientTick(Currtick);
    }



    void RollbackTo(int tick)
    {
        Currtick = tick;
        stateService_.RollbackTo(tick);
    }


    void Backup(int tick)
    {
        stateService_.BackupState(tick);
    }



    private void ProcessInputQueue(ServerFrame frame)
    {
        var inputs = frame.Inputs;
        var tanks = stateService_.GetTanks();
        foreach (var tank in tanks.Values)
        {
            tank.input.Reset();
            InputService.ExecuteCmd(tank, inputs[tank.frameId]);       
        }
    }



    private void SendInputs(int sendtick)
    {
        var input = InputService.GetInputCmd();
        var cFrame = new ServerFrame();

        var inputs = new PlayerInputCmd[stateService_.PlayerCount];
        for(int i = 0; i<inputs.Length; i++)
        {
            inputs[i] = InputService.EmptyInputCmd;
        }
        inputs[localFrameid] = input;
        cFrame.Tick = sendtick;
        cFrame.Inputs.Add(inputs);
        FillInputWithLastFrame(cFrame);
        frameBufferService_.PushLocalFrame(cFrame);
        //UnityEngine.Debug.Log(sendtick+ " "+ frameBufferService_.MaxServerTickInBuffer);

        if (sendtick > frameBufferService_.MaxServerTickInBuffer) // 发送帧必须超前服务器帧, todo 
        {
            

            int count = sendtick < PlayerInputFrameCount-1 ? sendtick + 1 : PlayerInputFrameCount; // 处理起始位置，每包三帧数据 

            //PrintLog.print("sendtick " + sendtick +"count "+ count);

            var inputsArray = new PlayerInputCmd[count];
            for (int i = 0; i < count; i++)
            {
                var frame = frameBufferService_.GetInputFrame(sendtick - i);
                inputsArray[count - i - 1] = frame.Inputs[localFrameid];
            }
            //tring s1 = UnityEngine.JsonUtility.ToJson(inputsArray[count-1]);
            //UnityEngine.Debug.Log(s1);

            MsgClientInputs msg = new MsgClientInputs();
            msg.Inputs.Add(inputsArray);
            msg.LastTick = sendtick;
           

            frameBufferService_.SendInput(msg);
            //NetManager.Send(msg,false);
            
            
        }

    }

    private void FillInputWithLastFrame(ServerFrame frame)
    {
        int tick = frame.Tick;
        var inputs = frame.Inputs;
        var lastServerInputs = tick == 0 ? null : frameBufferService_.GetFrame(tick - 1)?.Inputs;
        if (lastServerInputs == null)
            return;
        var myInput = inputs[localFrameid];
        //fill inputs with last frame's input (Input predict)
        for (int i = 0; i < stateService_.PlayerCount; i++)
        {
            inputs[i] = new PlayerInputCmd()
            {
                Cmd = lastServerInputs?[i]?.Cmd
            };
        }

        inputs[localFrameid] = myInput;
    }


}




