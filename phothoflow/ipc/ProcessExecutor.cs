using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace phothoflow.ipc
{
    class ProcessExecutor
    {
        public static void ExecuteSilent(string path, string param)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(@path, param);

            startInfo.CreateNoWindow = true;   //不创建窗口
            startInfo.UseShellExecute = false;//不使用系统外壳程序启动，重定向时此处必须设为false
            startInfo.RedirectStandardOutput = true; //重定向输出，而不是默认的显示在dos控制台上

            Process p = null;
            string output = "";

            try
            {

                p = Process.Start(startInfo);
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

            }

            finally
            {

                if (p != null)

                    p.Close();
            }
        }
    }
}
