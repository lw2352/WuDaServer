using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

/// <summary>
/// 数据库操作类
/// </summary>
public class DbClass
{
    //添加设备信息，如果不存在，还要在命令表中添加记录
    public static string addsensorinfo(string sensorintdeviceID, string sensorloginTime, string sensorStatus)
    {
        MySQLDB.InitDb();
        string sensorid = "0";
        //从数据库中查找当前ID是否存在
        try
        {
            DataSet ds1 = new DataSet("tdevice");
            string strSQL1 = "SELECT deviceID FROM tdevice where deviceID=" + "\"" + sensorintdeviceID + "\"";
            ds1 = MySQLDB.SelectDataSet(strSQL1, null);
            if (ds1 != null)
            {
                if (ds1.Tables[0].Rows.Count > 0)
                // 有数据集
                {
                    sensorid = ds1.Tables[0].Rows[0][0].ToString();

                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            UtilClass.writeLog(ex.ToString());
            return "fail"; //数据库异常
        }

        //************************************************************
        if (sensorid == "0") //若不存在,则添加
        {
            DataSet ds = new DataSet("tdevice");
            //string strResult = "";
            MySqlParameter[] parmss = null;
            string strSQL = "";
            bool IsDelSuccess = false;
            strSQL = " insert into tdevice (deviceID, lastLoginTime, status) values" +
                     "(?sensorintdeviceID,?sensorloginTime,?sensorStatus);";

            parmss = new MySqlParameter[]
            {
                    new MySqlParameter("?sensorintdeviceID", MySqlDbType.VarChar),
                    new MySqlParameter("?sensorloginTime", MySqlDbType.VarChar),
                    new MySqlParameter("?sensorStatus", MySqlDbType.VarChar)
            };
            parmss[0].Value = sensorintdeviceID;
            parmss[1].Value = sensorloginTime;
            parmss[2].Value = sensorStatus;

            try
            {
                IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);

                if (IsDelSuccess != false)
                {
                    //add 3-19，根据龚的要求，新上线设备后，要一起添加id号到命令表
                    strSQL = " insert into tcommand (deviceID) values" +
                             "(?sensorintdeviceID);";
                    parmss = new MySqlParameter[]
                    {
                            new MySqlParameter("?sensorintdeviceID", MySqlDbType.VarChar)
                    };
                    parmss[0].Value = sensorintdeviceID;
                    IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);
                    if (IsDelSuccess == false)
                    {
                        return "fail";
                    }

                    return "ok";
                }
                else
                {
                    return "fail";
                }


            }

            catch (Exception ex)
            {
                UtilClass.writeLog(ex.ToString());
                return "fail"; //数据库异常
            }
        }

        else //若ID存在,就更新update
        {
            DataSet ds = new DataSet("dssensorinfo");
            //string strResult = "";
            MySqlParameter[] parmss = null;
            string strSQL = "";
            bool IsDelSuccess = false;
            strSQL =
                "Update tdevice SET lastLoginTime=?sensorloginTime, status=?sensorStatus WHERE deviceID=?sensorintdeviceID";

            parmss = new MySqlParameter[]
            {
                    new MySqlParameter("?sensorintdeviceID", MySqlDbType.VarChar),
                    new MySqlParameter("?sensorloginTime", MySqlDbType.VarChar),
                    new MySqlParameter("?sensorStatus", MySqlDbType.VarChar)

            };
            parmss[0].Value = sensorintdeviceID;
            parmss[1].Value = sensorloginTime;
            parmss[2].Value = sensorStatus;

            try
            {
                IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);

                if (IsDelSuccess != false)
                {
                    return "ok";
                }
                else
                {
                    return "fail";
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex);
                UtilClass.writeLog(ex.ToString());
                return "fail"; //数据库异常
            }
        }

    }

    //更新所有设备在线信息为false
    public static string UpdateAllSensorInfoToFalse()
    {
        MySQLDB.InitDb();
        //string strResult = "";
        MySqlParameter[] parmss = null;
        string strSQL = "";
        bool IsDelSuccess = false;
        strSQL =
            "Update tdevice SET status = 'False'";
        try
        {
            IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);

            if (IsDelSuccess != false)
            {
                return "ok";
            }
            else
            {
                return "fail";
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex);
            UtilClass.writeLog(ex.ToString());
            return "fail";
        }
    }

    //更新设备信息
    public static string UpdateSensorInfo(string sensorintdeviceID, string updateItem, string updateValue)
    {
        MySQLDB.InitDb();
        //string strResult = "";
        MySqlParameter[] parmss = null;
        string strSQL = "";
        bool IsDelSuccess = false;
        strSQL =
            "Update tdevice SET " + updateItem + " =?sensorupdateItem WHERE deviceID=?sensorintdeviceID";
        parmss = new MySqlParameter[]
        {
                new MySqlParameter("?sensorintdeviceID", MySqlDbType.VarChar),
                new MySqlParameter("?sensorupdateItem", MySqlDbType.VarChar)
        };
        parmss[0].Value = sensorintdeviceID;
        parmss[1].Value = updateValue;

        try
        {
            IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);

            if (IsDelSuccess != false)
            {
                return "ok";
            }
            else
            {
                return "fail";
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex);
            UtilClass.writeLog(ex.ToString());
            return "fail";
        }
    }

    //更新设备信息
    public static string UpdateSensorInfo(string sensorintdeviceID, string[,] updateItem)
    {
        MySQLDB.InitDb();
        //string strResult = "";
        MySqlParameter[] parmss = null;
        string strSQL = "";
        bool IsDelSuccess = false;

        string strUpdateItem="";
        for (int i = 0; i < updateItem.Length / updateItem.GetLength(1); i++)
        {
            strUpdateItem +=updateItem[i, 0]  + " =" + "'" + updateItem[i, 1] + "' ";
            if (i < updateItem.Length / updateItem.GetLength(1) - 1)
            {
                strUpdateItem += ",";
            }
        }
        strSQL = "Update tdevice SET " + strUpdateItem + " WHERE deviceID=" + "'"+ sensorintdeviceID+ "'";
        try
        {
            IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);

            if (IsDelSuccess != false)
            {
                return "ok";
            }
            else
            {
                return "fail";
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex);
            UtilClass.writeLog(ex.ToString());
            return "fail";
        }
    }

    //插入记录
    public static string insertRecord(string[,] updateItem)
    {
        MySQLDB.InitDb();
        DataSet ds1 = new DataSet("trecord");
        MySqlParameter[] parmss = null;
        string strSQL = "";
        bool IsDelSuccess = false;
        string maxID="";

        //先插入一条记录
        string insertSQL = "insert into trecord () values()";
        IsDelSuccess = MySQLDB.ExecuteNonQry(insertSQL, parmss);
        //获取最大的自增id
        string getMaxIdstrSQL = "SELECT max(id) FROM trecord";
        ds1 = MySQLDB.SelectDataSet(getMaxIdstrSQL, null);
        if (ds1 != null)
        {
            if (ds1.Tables[0].Rows.Count > 0)
            {
                maxID = ds1.Tables[0].Rows[0][0].ToString();
                if (maxID == "")
                {
                    return "fail";
                }
            }
        }

        string strUpdateItem = "";
        for (int i = 0; i < updateItem.Length / updateItem.GetLength(1); i++)
        {
            strUpdateItem += updateItem[i, 0] + " =" + "'" + updateItem[i, 1] + "' ";
            if (i < updateItem.Length / updateItem.GetLength(1) - 1)
            {
                strUpdateItem += ",";
            }
        }
        strSQL = "Update trecord SET " + strUpdateItem + " WHERE id=" + "'" + maxID + "'";
        try
        {
            IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);

            if (IsDelSuccess != false)
            {
                return "ok";
            }
            else
            {
                return "fail";
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex);
            UtilClass.writeLog(ex.ToString());
            return "fail";
        }
    }

    /// <summary>
    /// 读取命令,返回二维数组,并重置字段为-1
    /// </summary>
    public static string[,] readCmd()
    {
        MySQLDB.InitDb();
        string[,] ret;
        //从数据库中查找当前ID是否存在
        try
        {
            DataSet ds1 = new DataSet("tcommand");
            string strSQL1 =
                "SELECT * FROM tcommand where (cmdName!='-1' AND cmdName!='ok' AND cmdName!='fail')";
            ds1 = MySQLDB.SelectDataSet(strSQL1, null);
            if (ds1 != null)
            {
                // 有数据集
                int count = ds1.Tables[0].Rows.Count;
                if (count > 0)
                {
                    ret = new string[count, 4];
                    for (int i = 0; i < count; i++)
                    {
                        ret[i, 0] = ds1.Tables[0].Rows[i]["deviceID"].ToString();
                        ret[i, 1] = ds1.Tables[0].Rows[i]["cmdName"].ToString();
                        ret[i, 2] = ds1.Tables[0].Rows[i]["operation"].ToString();
                        ret[i, 3] = ds1.Tables[0].Rows[i]["data"].ToString();
                    }

                    //重置字段为-1,add3-5
                    //3-24只修改对应ID的cmdName,不能全部修改，否则会把反馈信息“ok”覆盖掉！
                    for (int i = 0; i < count; i++)
                    {
                        string strSQL2 = "UPDATE tcommand SET cmdName = '-1' WHERE deviceID=" + "\"" + ret[i, 0].ToString() + "\"";
                        ds1 = MySQLDB.SelectDataSet(strSQL2, null);
                    }

                    return ret;
                }
                else return null;
            }
            else return null;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            UtilClass.writeLog(ex.ToString());
            return null;
        }
    }

    //更新设备命令信息
    public static string UpdateCmd(string sensorintdeviceID, string updateItem, string updateNum)
    {
        MySQLDB.InitDb();
        //string strResult = "";
        MySqlParameter[] parmss = null;
        string strSQL = "";
        bool IsDelSuccess = false;
        strSQL =
            "Update tcommand SET " + updateItem + " =?sensorupdateItem WHERE deviceID=?sensorintdeviceID";
        parmss = new MySqlParameter[]
        {
                new MySqlParameter("?sensorintdeviceID", MySqlDbType.VarChar),
                new MySqlParameter("?sensorupdateItem", MySqlDbType.VarChar)
        };
        parmss[0].Value = sensorintdeviceID;
        parmss[1].Value = updateNum;

        try
        {
            IsDelSuccess = MySQLDB.ExecuteNonQry(strSQL, parmss);

            if (IsDelSuccess != false)
            {
                return "ok";
            }
            else
            {
                return "fail";
            }
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex);
            UtilClass.writeLog(ex.ToString());
            return "fail";
        }
    }


}

