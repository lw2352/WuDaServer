using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 工具函数类
/// </summary>
public class UtilClass
{

    private static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(UtilClass));
    public static string[] hex2String = new string[256];
    private static Hashtable htStrToHex = new Hashtable(); //str--hex

    /// <summary>
    /// 保存日志
    /// </summary>
    /// <param name="msg"></param>
    public static void writeLog(string msg)
    {
        Console.WriteLine(msg + "\r\n");
        Log.Info(msg);
    }

    /// <summary>
    /// 初始化工具类，生成hex与string的对应关系
    /// </summary>
    public static void utilInit()
    {
        for (int i = 0; i < hex2String.Length; i++)
        {
            hex2String[i] = i.ToString("X2");
            htStrToHex.Add(hex2String[i], (byte)i);
        }
    }

    /// <summary>
    /// 字节数组转16进制字符串 
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string byteToHexStr(byte[] bytes)
    {
        try
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += hex2String[bytes[i]];
                }
            }
            return returnStr;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }

    /// <summary>
    /// 例如："7985"->[0x79,0x85]
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] hexStrToByte(string str)
    {
        try
        {
            byte[] bytes = new byte[str.Length / 2];
            string a;
            for (int i = 0, j = 0; i < str.Length; i++, i++, j++)
            {
                a = str.Substring(i, 2);
                bytes[j] = (byte)htStrToHex[a];
            }

            return bytes;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


}

