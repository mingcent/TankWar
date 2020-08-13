暑期实习自己做的一个小游戏，主要是在坦克大战的基础上对游戏的同步技术展开研究。

游戏的原型为《Unity 3d 网络游戏实战》罗培羽 中的项目。

项目包括客户端和服务器两部分。客户端基于Unity2019.3.15f1，服务器基于C#

**实现了：**

1. 乐观锁步帧同步
2. 客户端的预测与回滚
3. 网络数据包多倍串流
4. 客户端逻辑与显示分离，逻辑部分基于定点数实现。

**项目依赖：**

序列化：google protobuf

网络通信：[Enet](https://github.com/NateShoffner/ENetSharp.git)

定点数和物理库：[LogicPhysics](https://github.com/Prince-Ling/LogicPhysics.git)

**注意：**

服务器开启时需要连接mysql数据库，数据库存放着玩家的账号密码等信息。具体可以参考《Unity 3d 网络游戏实战》。

数据库的表信息见MySQL文件夹