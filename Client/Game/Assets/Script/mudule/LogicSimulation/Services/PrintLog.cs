using UnityEngine;
using System.Collections;

using System.IO;

public class PrintLog 
{



    static int line = 0;
    public static void print(string info)
    {
        //每记录一次都进行换行操作
        line++;
        //Log存放的位置信息
        string path = Application.dataPath + "/LogFile.txt";
        StreamWriter sw;
        Debug.Log(path);

        if (line == 1)
        {

            //如果此值为false，则创建一个新文件，如果存在原文件，则覆盖。如果此值为true，则打开文件保留原来数据，如果找不到文件，则创建新文件。
            sw = new StreamWriter(path, false);
            string fileTitle = "日志文件创建的时间" + System.DateTime.Now.ToString();
            sw.WriteLine(fileTitle);
        }
        else
        {
            sw = new StreamWriter(path, true);
        }
        string lineInfo = line + "t" + "时刻" + Time.time + "：";
        sw.WriteLine(lineInfo);
        sw.WriteLine(info);
        Debug.Log(info);
        sw.Flush();
        sw.Close();
    }
}
