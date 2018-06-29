using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;


//升级结构体
struct Update
{
    public bool IsNeedUpdate;//是否需要升级
    public int currentNum;
};


//设备类
class DataItem
{
    public string strID;//设备ID
    public byte[] byteID;
    public bool status;//设备状态，有离线，在线，gps错误
    public Update tUpdate;   
    public DateTime HeartTime; //上一次心跳包发上来的时间
    public byte[] updateData; //所有数据,存放历史记录
    public int sendCmdFailTimes;//发送命令失败次数

    //public Queue<byte[]> recDataQueue = new Queue<byte[]>();//数据接收队列；queue是对象的先进先出集合
    //public Queue<byte[]> sendDataQueue = new Queue<byte[]>();//数据发送队列
    private Socket socket;//实际共用SendSocket，用来发送数据
    private EndPoint remote;//客户端节点

    private double Latitude;//纬度
    private double Longitude;//经度
    private int level;//能级
    private int battery;//电量
    private string strPreciseTime;//精确触发时刻

    /// <summary>
    /// 初始化DataItem
    /// </summary>
    public void Init(Socket serverSocket, byte[] id, string strid, int updateDataLength, EndPoint clientRemote)
    {       
        strID = strid;
        byteID = id;
        status = false;
        HeartTime = DateTime.Now;
        updateData = new byte[updateDataLength];
        sendCmdFailTimes = 0;     

        socket = serverSocket;
        remote = clientRemote;
        //other
        tUpdate.IsNeedUpdate = false;
        tUpdate.currentNum = 0;
    }

    //处理数据20180626取消
    //public void HandleData()
    //{
    //    if (recDataQueue.Count > 0)//命令已发送后，得到返回信息需要一段时间，再去解析数据
    //    {
    //        byte[] datagramBytes = recDataQueue.Dequeue();//读取 Queue<T> 开始处的对象并移除
    //        AnalyzeData(datagramBytes);
    //    }
    //}

    /// <summary>
    /// 发送数据
    /// </summary>
    //public void SendData()
    //{
    //    if (sendDataQueue.Count > 0)//没有待解析的命令，可以去发送命令
    //    {
    //        byte[] datagramBytes = sendDataQueue.Dequeue(); //读取 Queue<T> 开始处的对象并移除
    //        SendCmd(datagramBytes);
    //    }
    //}

    //处理数据和写入数据库
    public void AnalyzeData(byte[] datagramBytes)
    {
        try
        {
            switch (datagramBytes[2])
            {               
                //搜索设备
                case 0x00:
                    string[,] updateItem0 = new string[5, 2];
                    status = true;
                    HeartTime = DateTime.Now;
                    Latitude = datagramBytes[10] + (double)datagramBytes[11] / 60 + ((double)datagramBytes[12]/100+ (double)datagramBytes[13]/1000+ (double)datagramBytes[14]/100000)/60;
                    Longitude = datagramBytes[15] + (double)datagramBytes[16] / 60 + ((double)datagramBytes[17] / 100 + (double)datagramBytes[18] / 1000 + (double)datagramBytes[19] / 100000) / 60;
                    //保留6位小数
                    Longitude = (int)(Longitude * 1000000);
                    Longitude = Longitude / 1000000;

                    Latitude = (int)(Latitude * 1000000);
                    Latitude = Latitude / 1000000;

                    battery = (((int)datagramBytes[20] << 8) + datagramBytes[21]-3000)*3/100;//3v--0,6v--100
                    //更新经纬度和电量到数据库
                    updateItem0[0, 0] = "Latitude";
                    updateItem0[0, 1] = Latitude.ToString();
                    updateItem0[1, 0] = "Longitude";
                    updateItem0[1, 1] = Longitude.ToString();
                    updateItem0[2, 0] = "battery";
                    updateItem0[2, 1] = battery.ToString();
                    updateItem0[3, 0] = "lastLoginTime";
                    updateItem0[3, 1] = HeartTime.ToString("yyyy-MM-dd HH:mm:ss");
                    updateItem0[4, 0] = "status";
                    updateItem0[4, 1] = status.ToString();
                    DbClass.UpdateSensorInfo(strID, updateItem0);
                    //反馈命令执行状态
                    DbClass.UpdateCmd(strID, "cmdName", "ok");
                    //反馈接收到的命令名称
                    MainUdpClass.getClientDataSuccess();
                    break;

                case 0x01://获取触发时间和能级
                    string[,] updateItem1 = new string[4, 2];
                    string[,] updateItem2 = new string[10, 2];
                    strPreciseTime = datagramBytes[10].ToString().PadLeft(2, '0') + ":" + datagramBytes[11].ToString().PadLeft(2, '0') + ":" +
                                     datagramBytes[12].ToString().PadLeft(2, '0') + "." +
                                     (((int) datagramBytes[13] << 16) + ((int) datagramBytes[14] <<8) + 
                                      (int) datagramBytes[15]).ToString();
                    level = datagramBytes[16];
                    //写入数据库
                    updateItem1[0, 0] = "preciseTime";
                    updateItem1[0, 1] = strPreciseTime;
                    updateItem1[1, 0] = "level";
                    updateItem1[1, 1] = level.ToString();
                    updateItem1[2, 0] = "date";
                    updateItem1[2, 1] = DateTime.Now.ToString("yyyy-MM-dd");
                    updateItem1[3, 0] = "time";
                    updateItem1[3, 1] = DateTime.Now.ToString("HH:mm:ss");
                    DbClass.UpdateSensorInfo(strID, updateItem1);

                    //添加记录到trecord表
                    updateItem2[0, 0] = "Latitude";
                    updateItem2[0, 1] = Latitude.ToString();
                    updateItem2[1, 0] = "Longitude";
                    updateItem2[1, 1] = Longitude.ToString();
                    updateItem2[2, 0] = "battery";
                    updateItem2[2, 1] = battery.ToString();
                    updateItem2[3, 0] = "lastLoginTime";
                    updateItem2[3, 1] = HeartTime.ToString("yyyy-MM-dd HH:mm:ss");
                    updateItem2[4, 0] = "status";
                    updateItem2[4, 1] = status.ToString();
                    updateItem2[5, 0] = "preciseTime";
                    updateItem2[5, 1] = strPreciseTime;
                    updateItem2[6, 0] = "level";
                    updateItem2[6, 1] = level.ToString();
                    updateItem2[7, 0] = "date";
                    updateItem2[7, 1] = DateTime.Now.ToString("yyyy-MM-dd");
                    updateItem2[8, 0] = "time";
                    updateItem2[8, 1] = DateTime.Now.ToString("HH:mm:ss");
                    updateItem2[9, 0] = "deviceID";
                    updateItem2[9, 1] = strID;
                    DbClass.insertRecord(updateItem2);
                    //反馈命令执行状态
                    DbClass.UpdateCmd(strID, "cmdName", "ok");
                    //反馈接收到的命令名称
                    MainUdpClass.getClientDataSuccess();
                    break;

                case 0x02:
                    if (datagramBytes[10] == 0x55)
                    {
                        //反馈命令执行状态
                        DbClass.UpdateCmd(strID, "cmdName", "ok");
                        //反馈接收到的命令名称
                        MainUdpClass.getClientDataSuccess();
                    }
                    break;

                #region （暂时不用）设备升级相关命令
                //升级
                case 0x1E:
                    if (tUpdate.IsNeedUpdate == true)
                    {
                        if (datagramBytes[10] == 0x55)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "data", UtilClass.hex2String[tUpdate.currentNum]);
                            tUpdate.currentNum++;
                        }
                        else if (datagramBytes[10] == 0xAA)
                        {
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "fail");
                        }
                        //最多256个包(256K)
                        if (tUpdate.currentNum == 256)
                        {
                            tUpdate.IsNeedUpdate = false;
                            tUpdate.currentNum = 0;
                            //写入数据库
                            DbClass.UpdateCmd(strID, "cmdName", "ok");
                        }
                    }
                    break;

                //重启（并升级）
                case 0x21:
                    if (datagramBytes[10] == 0x55)
                    {
                        //写入数据库
                        DbClass.UpdateCmd(strID, "cmdName", "ok");
                    }
                    break;

                //读取版本号
                case 0x24:
                    //写入数据库
                    DbClass.UpdateCmd(strID, "data", UtilClass.hex2String[datagramBytes[10]]);
                    break;


                #endregion

                default:
                    break;
            }

            if (tUpdate.IsNeedUpdate == true)
            {
                SendCmd(SetUpdateCmd(tUpdate.currentNum));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            UtilClass.writeLog(ex.ToString());
        }
        UtilClass.writeLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "从硬件" + strID + "收到数据：" + UtilClass.byteToHexStr(datagramBytes));
    }

    /// <summary>
    /// 发送命令
    /// </summary>
    /// <param name="cmd"></param>
    public void SendCmd(byte[] cmd)
    {
        try
        {
            UtilClass.writeLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "向硬件" + strID + "发送数据：" + UtilClass.byteToHexStr(cmd));
            socket.BeginSendTo(cmd, 0, cmd.Length, SocketFlags.None, remote, new AsyncCallback(OnSend), socket);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            UtilClass.writeLog(ex.ToString());
        }
    }

    /// <summary>
    /// 异步发送数据
    /// </summary>
    /// <param name="ar">IAsyncResult</param>
    private void OnSend(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndSendTo(ar);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            UtilClass.writeLog(ex.ToString());
        }
    }

    /// <summary>
    /// 构造升级命令
    /// </summary>
    /// <param name="bulkCount"></param>
    /// <returns></returns>
    private byte[] SetUpdateCmd(int bulkCount)
    {
        /***************************************************************************7-读写位*8，9第几包**10-数据位*************************************/
        byte[] Cmd = new byte[1024 + 13];
        byte[] bytesbulkCount = new byte[2];
        bytesbulkCount = intToBytes(bulkCount);

        Cmd[0] = 0xA5;
        Cmd[1] = 0xA5;
        Cmd[2] = 0x1E;
        Cmd[3] = byteID[0];
        Cmd[4] = byteID[1];
        Cmd[5] = byteID[2];
        Cmd[6] = byteID[3];
        Cmd[7] = 0x01;
        Cmd[8] = bytesbulkCount[0];
        Cmd[9] = bytesbulkCount[1];
        for (int i = 0; i < 1024; i++)
        {
            Cmd[10 + i] = updateData[bulkCount * 1024 + i];
        }

        Cmd[1024 + 10 + 0] = 0xFF;
        Cmd[1024 + 10 + 1] = 0x5A;
        Cmd[1024 + 10 + 2] = 0x5A;
        return (Cmd);
    }

    /// <summary>
    /// 将int数值转换为占byte数组
    /// </summary>
    /// <param name="value">int</param>
    /// <returns>byte[]</returns>
    private byte[] intToBytes(int value)
    {
        byte[] src = new byte[2];

        src[0] = (byte)((value >> 8) & 0xFF);
        src[1] = (byte)(value & 0xFF);
        return src;
    }

}

