using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.IO;

namespace Dynamsoft
{

    public class Webhook : IHttpHandler
    {

        private FileHelper mFileHelper;
        private string logFile;
        private string batchFile;

        public void ExecuteCommandSync(object command)
        {
            try
            {
                // https://www.codeproject.com/Articles/25983/How-to-Execute-a-Command-in-C
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                Console.WriteLine(result);

                // Guardando logs
                if (mFileHelper == null)
                {
                    mFileHelper = new FileHelper(logFile);
                }
                mFileHelper.WriteLog("git pull event");
            }
            catch (Exception objException)
            {
                // Log de error
                // Guardando logs
                if (mFileHelper == null)
                {
                    mFileHelper = new FileHelper(logFile);
                }
                mFileHelper.WriteLog("Error al ejecutar comandos cmd");
            }
        }

        public void ProcessRequest(HttpContext context)
        {

            logFile = Path.Combine(context.Server.MapPath("."), "log.txt");
            batchFile = Path.Combine(context.Server.MapPath("."), "github.bat");
            //System.Diagnostics.Process.Start(batchFile);
            try
            {
                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
                //Make the thread as background thread.
                objThread.IsBackground = true;
                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                //Start the thread.
                objThread.Start(batchFile);
            }
            catch (ThreadStartException objException)
            {
                // Log the exception
            }
            catch (ThreadAbortException objException)
            {
                // Log the exception
            }
            catch (Exception objException)
            {
                // Log the exception
            }

            context.Response.ContentType = "text/plain";
            context.Response.Write("Updated");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }

    public class FileHelper
    {
        private string _strFilePath = "";
        private bool enableLog = true;

        public FileHelper(string strPath)
        {
            _strFilePath = strPath;
        }

        public void Write(string strMessage)
        {
            FileStream fw = null;
            StreamWriter sw = null;

            try
            {
                fw = new FileStream(this._strFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                sw = new StreamWriter(fw);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.Write(strMessage + "\r\n");
                sw.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }

                if (fw != null)
                {
                    fw.Close();
                }
            }
        }

        public void WriteLog(string strLog)
        {
            if (!enableLog)
            {
                return;
            }

            try
            {
                string strLogs = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]  " + strLog + "\r\n";
                Write(strLogs);
            }
            catch (Exception)
            { }
        }
    }
}