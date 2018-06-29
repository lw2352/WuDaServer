using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 命令构造类
/// </summary>
public class CmdClass
{
    //命令名称和命令号的一些对应关系存在哈希表中
    private static Hashtable htCmdNum = new Hashtable(); //str--hex

    /// <summary>
    /// 命令初始化
    /// </summary>
    public static void cmdInit()
    {
        //读写操作
        htCmdNum.Add("read", 0x00);
        htCmdNum.Add("write", 0x01);
        //命令号
        htCmdNum.Add("search", 0x00);
        htCmdNum.Add("getTimeAndLevel", 0x01);
        htCmdNum.Add("resetLevel", 0x02);
    }

    //构造命令
    public static byte[] makeCommand(string cmdFlag, string rw, string data, byte[] mcuID)
    {
        try
        {
            if (cmdFlag == null || rw == null || data == null || mcuID == null || cmdFlag == "-1" || rw == "-1" || data == "-1")
            {
                return null;
            }
            byte len = (byte)(data.Length / 2);
            byte[] buf = new byte[len + 13+3];
            byte[] byteData = UtilClass.hexStrToByte(data);
            int i;
            byte crcRet;

            //lora
            buf[0] = mcuID[1];//地址高8位
            buf[1] = mcuID[2];//地址低8位
            buf[2] = mcuID[3];//信道

            buf[3] = 0xA5;
            buf[4] = 0xA5;
            if (!htCmdNum.ContainsKey(cmdFlag) || !htCmdNum.ContainsKey(rw))
            {
                return null;
            }

            buf[5] = Convert.ToByte(htCmdNum[cmdFlag]);
            buf[6] = mcuID[0];
            buf[7] = mcuID[1];
            buf[8] = mcuID[2];
            buf[9] = mcuID[3];
            buf[10] = Convert.ToByte(htCmdNum[rw]);
            buf[11] = (byte)(len >> 8);
            buf[12] = (byte)(len & 0xFF);

            for (i = 0; i < len; i++)
            {
                buf[13 + i] = byteData[i];
            }

            crcRet = 0xFF;

            buf[13 + len + 0] = crcRet;
            buf[13 + len + 1] = 0x5A;
            buf[13 + len + 2] = 0x5A;

            return buf;
        }
        catch (Exception ex)
        {
            UtilClass.writeLog(ex.ToString());
            return null;
        }

    }
}

