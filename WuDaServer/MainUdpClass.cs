using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Timers;

//命令状态结构体
struct CmdStatus
{
    public string cmdName;//当前正在执行的命令
    public string strID;//当前正在执行的设备ID号
};

/// <summary>
/// udp服务，存放主要逻辑
/// </summary>
public class MainUdpClass
{
    //服务是否开启
    private static bool IsServerOpen;
    //服务器IP
    public string ServerIP { get; set; }

    //监听端口号
    public int ServerPort { get; set; }
    //采用广播方式
    //客户端IP
    //public string ClientIP { get; set; }
    //客户端port
    //public int ClientPort { get; set; }
    //数据连接配置
    public string strSqlCfg { get; set; }

    public int bufferLen { get; set; }

    public int overTime { get; set; }//发送超时次数
    public int maxOverTime { get; set; }//超时的时间长度

    //public int checkRecDataQueueTimeInterval { get; set; } // 检查接收数据包队列时间休息间隔(ms)
    public int checkSendDataQueueTimeInterval { get; set; } // 检查发送命令队列时间休息间隔(ms)
    public int checkDataBaseQueueTimeInterval { get; set; }// 检查数据库命令队列时间休息间隔(ms)

    public int startID { get; set; }//起始ID号
    public int stopID { get; set; }//结束ID号

    private static int updateDataLength = 32 * 1024;//升级文件最大长度

    private static Socket ServerSocket;//用于接收
    //广播地址，255.255.255.255:6000
    private static IPEndPoint broadcastIpEndPoint = new IPEndPoint(IPAddress.Broadcast, 6000);
    private static EndPoint broadcastRemote = (EndPoint)(broadcastIpEndPoint);

    private static Hashtable htClient = new Hashtable(); //strID--DataItem
    private static byte[] buffer;//socket缓冲区

    //处理接收数据线程；ManualResetEvent:通知一个或多个正在等待的线程已发生事件
    //private ManualResetEvent checkRecDataQueueResetEvent = new ManualResetEvent(true);

    //处理发送数据线程，把数据哈希表中的数据复制到各个dataItem中的发送队列
    private ManualResetEvent checkSendDataQueueResetEvent = new ManualResetEvent(true);

    private ManualResetEvent checkDataBaseQueueResetEvent = new ManualResetEvent(true);

    private static CmdStatus tCmdStatus;
    private static Queue<byte[]> cmdQueue = new Queue<byte[]>();//命令发送队列
    private static System.Timers.Timer cmdTimer = new System.Timers.Timer();

    /// <summary>
    /// 组件类初始化
    /// </summary>
    private bool serviceInit()
    {
        try
        {
            //绑定定时器回调函数
            cmdTimer.Elapsed += new System.Timers.ElapsedEventHandler(cmdOverTime);
            cmdTimer.AutoReset = false;//单次
            cmdTimer.Interval = overTime;

            UtilClass.utilInit();
            CmdClass.cmdInit();
            MySQLDB.m_strConn = strSqlCfg;

            addDeviceToHashTable();//初始化哈希表，提前加好设备号

            //发送数据包处理线程
            if (!ThreadPool.QueueUserWorkItem(CheckSendDataQueue))
                return false;
            //接收数据包处理线程
            //if (!ThreadPool.QueueUserWorkItem(CheckRecDataQueue))
                //return false;
            //读取数据库命令线程
            if (!ThreadPool.QueueUserWorkItem(CheckDataBaseQueue))
                return false;
            IsServerOpen = true;
            return true;
        }
        catch (Exception e)
        {
            UtilClass.writeLog(e.ToString());
            return false;
        } 
    }

    #region 开启和关闭服务会调用的函数
    /// <summary>
    /// 服务开始，进行初始化，传入参数
    /// </summary>
    public void Start()
    {
        buffer = new byte[bufferLen];
        if (OpenServer() == true)
        {
            //socket服务开启成功后再初始化其他类
            if (serviceInit() == false)
            {
                UtilClass.writeLog("启动失败");
            }
            
            //初始化设备状态为false
            DbClass.UpdateAllSensorInfoToFalse();
            UtilClass.writeLog("启动成功");
        }
        else
        {
            UtilClass.writeLog("启动失败");
        }
    }

    public void Stop()
    {
        if (CloseServer() == true)
        {
            UtilClass.writeLog("停止成功");
        }
        else
        {
            UtilClass.writeLog("停止失败");
        }
    }

    #endregion

    /// <summary>
    /// 开启socket服务
    /// </summary>
    /// <returns></returns>
    private bool OpenServer()
    {
        try
        {
            IPEndPoint myclient = new IPEndPoint(IPAddress.Any, 0);
            EndPoint myRemote = (EndPoint)(myclient);

            //初始化服务端IP，设置端口号         
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //配置广播发送socket
            ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            //服务端绑定网络地址
            ServerSocket.Bind(ipEndPoint);
            //开始异步接收数据
            ServerSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref myRemote, new AsyncCallback(OnReceive), myRemote);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            UtilClass.writeLog(e.ToString());
            return false;
        }
    }

    /// <summary>
    /// 关闭socket
    /// </summary>
    public bool CloseServer()
    {
        try
        {
            IsServerOpen = false;

            //checkRecDataQueueResetEvent.WaitOne();
            checkSendDataQueueResetEvent.WaitOne();
            checkDataBaseQueueResetEvent.WaitOne();

            ServerSocket.Dispose();

            htClient.Clear();

            GC.SuppressFinalize(this);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            UtilClass.writeLog(e.ToString());
            return false;
        }

    }


    /// <summary>
    /// 接收数据
    /// </summary>
    /// <param name="ar"></param>
    private void OnReceive(IAsyncResult ar)
    {
        int len = -1;
        string strID;
        byte[] id = new byte[4];
        try
        {
            EndPoint remote = (EndPoint)(ar.AsyncState);
            len = ServerSocket.EndReceiveFrom(ar, ref remote);

            //报文格式过滤
            if (buffer[0] == 0xA5 && buffer[1] == 0xA5 && buffer[len - 2] == 0x5A && buffer[len - 1] == 0x5A)
            {
                Array.Copy(buffer, 3, id, 0, 4);
                strID = UtilClass.byteToHexStr(id);

                //20180626由于武大项目采用轮询方式通信，因此不需要判断
                //判断哈希表中是否存在当前ID，不存在则创建，存在则把数据加入队列
                //if (htClient.ContainsKey(strID) == false)
                //{
                //    DataItem dataItem = new DataItem();
                //    dataItem.Init(ServerSocket, id, strID, updateDataLength, broadcastRemote); //初始化dataItem
                //    htClient.Add(strID, dataItem);
                //    //把设备信息存入数据库
                //    DbClass.addsensorinfo(strID, dataItem.HeartTime.ToString("yyyy-MM-dd HH:mm:ss"), dataItem.status.ToString());
                //}
                //else
                //{
                    DataItem dataItem = (DataItem)htClient[strID]; //取出address对应的dataitem
                    byte[] recData = new byte[len];
                    Array.Copy(buffer, recData, len);
                    dataItem.AnalyzeData(recData);
                    
                //}
            }
            //继续接收数据
            ServerSocket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remote, new AsyncCallback(OnReceive), remote);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            UtilClass.writeLog(ex.ToString());
        }
    }

    #region 数据接收异常处理
    //定时器回调，接收超时
    private void cmdOverTime(object source, ElapsedEventArgs e)
    {
        DataItem dataItem = (DataItem)htClient[tCmdStatus.strID]; //取出ID对应的dataitem
        if (dataItem.sendCmdFailTimes == maxOverTime)
        {
            cmdQueue.Dequeue();
            dataItem.status = false;
            dataItem.sendCmdFailTimes = 0;
            //update sql
            DbClass.UpdateSensorInfo(dataItem.strID, "status", dataItem.status.ToString());
        }
        else
        {
            byte[] cmd = cmdQueue.Dequeue(); //读取 Queue<T> 开始处的对象并移除
            cmdQueue.Enqueue(cmd);//加到队尾            
            dataItem.sendCmdFailTimes++;
            UtilClass.writeLog("设备" + tCmdStatus.strID + "接收"+tCmdStatus.cmdName+"命令超时次数为"+ dataItem.sendCmdFailTimes.ToString());
        }
    }

    //成功收到设备数据，没有超时
    public static void getClientDataSuccess()
    {
        cmdTimer.Stop();//关闭定时器
        cmdQueue.Dequeue();//移除一条命令
    }
    

    #endregion

    #region 任务
    //处理发送队列
    private void CheckSendDataQueue(object state)
    {
        checkSendDataQueueResetEvent.Reset(); //Reset()将事件状态设置为非终止状态，导致线程阻止。
        while (IsServerOpen)
        {
            try
            {                           
                if (cmdQueue.Count > 0)
                {
                    //提取命令
                    string strID;
                    byte[] id = new byte[4];
                    byte[] cmd = cmdQueue.Peek(); //返回位于 Queue<T> 开始处的对象但不将其移除
                    Array.Copy(cmd, 6, id, 0, 4);
                    strID = UtilClass.byteToHexStr(id);
                    //准备发送                   
                    if (tCmdStatus.strID != strID) //不是上一条发送的命令
                    {
                        DataItem dataItem = (DataItem) htClient[strID]; //取出address对应的dataitem
                        tCmdStatus.strID = strID;
                        dataItem.SendCmd(cmd);
                        //开定时器
                        cmdTimer.Start();
                    }
                }
                else//发送队列里面的命令数量为0
                {
                    tCmdStatus.cmdName = null;
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                UtilClass.writeLog(ex.ToString());
            }
            Thread.Sleep(checkSendDataQueueTimeInterval); //当前数据处理线程休眠一段时间
        }
        checkSendDataQueueResetEvent.Set();
    }

    //20180626收到数据后，直接处理
    //处理接收队列
    //private void CheckRecDataQueue(object state)
    //{
    //    checkRecDataQueueResetEvent.Reset(); //Reset()将事件状态设置为非终止状态，导致线程阻止。
    //    while (IsServerOpen)
    //    {
    //        try
    //        {
    //            foreach (DataItem dataItem in htClient.Values)
    //            {
    //                dataItem.HandleData();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(ex);
    //            UtilClass.writeLog(ex.ToString());
    //        }
    //        Thread.Sleep(checkRecDataQueueTimeInterval); //当前数据处理线程休眠一段时间
    //    }
    //    checkRecDataQueueResetEvent.Set();
    //}

    //读取数据库命令线程
    private void CheckDataBaseQueue(object state)
    {
        //add 3-17 粗心掉了一句
        checkDataBaseQueueResetEvent.Reset(); //Reset()将事件状态设置为非终止状态，导致线程阻止。
        while (IsServerOpen)
        {
            try
            {
                /*1.先读取tcommand表中所有deviceID的cmdName字段
                 * 2.“-1”表示无命令，其余值表示命令名称“name”
                 * 3.根据name来读取对应ID的operation和data
                 * 4.构造命令
                 * 5.把命令加入发送队列
                 * 6.如果是升级文件的路径，则需要读取文件内容并存入dataitem，设置标志位
                 * 7.如果是设置卡号，需要在dataitem设置大数组，加标志位并分多包发送。
                 * */
                /*
                 *ret[i, 0] = ds1.Tables[0].Rows[i]["deviceID"].ToString();
                        ret[i, 1] = ds1.Tables[0].Rows[i]["cmdName"].ToString();
                        ret[i, 2] = ds1.Tables[0].Rows[i]["operation"].ToString();
                        ret[i, 3] = ds1.Tables[0].Rows[i]["data"].ToString();
                 */
                string[,] cmdStrings = DbClass.readCmd();
                if (cmdStrings != null && cmdQueue.Count == 0)//先判定是否为空,并且上次的命令执行完
                {
                    tCmdStatus.cmdName = cmdStrings[0, 1];
                    tCmdStatus.strID = null;
                    //数据添加到全局的queue队列，由发送任务来分发给设备类。如果上一次的命令没有执行完，不接受新命令
                    if (tCmdStatus.cmdName == "search")
                    {
                        addSearchCmd();
                    }

                    for (int i = 0; i < cmdStrings.Length / cmdStrings.GetLength(1); i++)
                    {
                        if (htClient.ContainsKey(cmdStrings[i, 0]))
                        {
                            DataItem dataItem = (DataItem)htClient[cmdStrings[i, 0]];

                            //有一些指令需要多包发送和读取                        
                            if (cmdStrings[i, 1] == "update")
                            {
                                using (FileStream fsSource = new FileStream(cmdStrings[i, 3],
                                    FileMode.Open, FileAccess.Read))
                                {
                                    // Read the source file into a byte array.
                                    for (int j = 0; j < updateDataLength; j++) //先用0xFF填充
                                    {
                                        dataItem.updateData[j] = 0xFF;
                                    }
                                    int numBytesToRead = (int)fsSource.Length;
                                    if (numBytesToRead > 0)
                                    {
                                        // Read may return anything from 0 to numBytesToRead.
                                        fsSource.Read(dataItem.updateData, 0, numBytesToRead);
                                    }

                                }
                                //设置升级属性
                                dataItem.tUpdate.IsNeedUpdate = true;
                            }
                            else//普通指令可以直接构造并发送
                            {
                                byte[] cmd = CmdClass.makeCommand(cmdStrings[i, 1], cmdStrings[i, 2],
                                    cmdStrings[i, 3],
                                    dataItem.byteID);
                                if (cmd != null)
                                {
                                    cmdQueue.Enqueue(cmd);
                                }
                            }

                        }//end of if (htClient.ContainsKey(cmdStrings[i, 0]))

                    }//end of for

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                UtilClass.writeLog(ex.ToString());
            }
            Thread.Sleep(checkDataBaseQueueTimeInterval);
        }
        checkDataBaseQueueResetEvent.Set();
    }
    #endregion


    //添加search命令
    public void addSearchCmd()
    {
        foreach (DataItem dataItem in htClient.Values)
        {
            byte[] cmd = CmdClass.makeCommand("search", "read",
                "00",
                dataItem.byteID);
            cmdQueue.Enqueue(cmd);
        }
    }

    //添加设备到哈希表
    public void addDeviceToHashTable()
    {
        string strID;
        
        for (int i = startID; i <= stopID; i++)
        {
            byte[] id = { 0x00, 0x00, 0x00, 0x1E };//信道为0x1E，固定不变//要放在里面，哈希表存的是指针
            id[1] = (byte)(i >> 8);
            id[2] = (byte)(i & 0xFF);
            strID = UtilClass.byteToHexStr(id);

            DataItem dataItem = new DataItem();
            dataItem.Init(ServerSocket, id, strID, updateDataLength, broadcastRemote); //初始化dataItem
            htClient.Add(strID, dataItem);
            DbClass.addsensorinfo(strID, dataItem.HeartTime.ToString("yyyy-MM-dd HH:mm:ss"), dataItem.status.ToString());
        }
    }
}

