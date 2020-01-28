using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.IO;

namespace hbkpsvc
{
    public partial class hbkpsvc : ServiceBase
    {

        private Timer _timer;
        private DateTime _lastRun = DateTime.Now.AddDays(-1);

        public hbkpsvc()
        {
            InitializeComponent();
        }

        void timer_Elapsed(object sender, EventArgs e)
        {
            _timer.Stop();
            ServiceCore();
            _timer.Start();
        }
        protected override void OnStart(string[] args)
        {
            try
            {
                ConfigGlobal.ReadConfig();
                Logging.Log("Iniciando serviço de backup de arquivos...", EventLogEntryType.Information);
                //ConfigGlobal.WriteLogGlobalConfig();
                _timer = new Timer(TimeSpan.FromSeconds(ConfigGlobal.IntervalMs).TotalMilliseconds);
                _timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                _timer.Start();
            }
            catch (Exception e)
            {
                Logging.Log(e.Message, EventLogEntryType.Error);
            }
        }

        protected override void OnStop()
        {
            Files.ClearStage(ConfigGlobal.StageDir);
            Logging.Log("Finalizando o serviço...", EventLogEntryType.Information);
        }


        static void ServiceCore()
        {
            string checkSumFile = null;
            string logFile = null;
            try
            {
                ConfigGlobal.ReadConfig();
                ConfigGlobal.CloudSubDirectory = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
                StringBuilder sb = new StringBuilder();
                var filesToUpload = Files.GetFilesToUpload(ConfigGlobal.SourceDir, ConfigGlobal.FileRegEx);
                Files.ClearStage(ConfigGlobal.StageDir);

                sb.Append("Quantidade de arquivos encontrados para processamento: ");
                sb.AppendLine(filesToUpload.Count.ToString());
                Logging.Log(sb.ToString(), EventLogEntryType.Information);
                sb.Clear();

                foreach (var fileToUpload in filesToUpload)
                {
                    sb.Append("Arquivo a ser processado: ");
                    sb.AppendLine(fileToUpload);
                    Logging.Log(sb.ToString(), EventLogEntryType.Information);
                    sb.Clear();
                    File.Copy(fileToUpload, Path.Combine(ConfigGlobal.StageDir, Path.GetFileName(fileToUpload)), true);
                    Files.CompressFileToUpload(Path.Combine(ConfigGlobal.StageDir, Path.GetFileName(fileToUpload)));
                    sb.Append("Arquivo compactado como: ");
                    sb.AppendLine(Path.Combine(ConfigGlobal.StageDir, Path.GetFileName(fileToUpload)));
                    Logging.Log(sb.ToString(), EventLogEntryType.Information);
                    sb.Clear();
                }
                var filesFromStageToUpload = Files.GetFilesFromStageToUpload(ConfigGlobal.StageDir);
                foreach (var fileFromStageToUpload in filesFromStageToUpload)
                {
                    Files.UploadFile(fileFromStageToUpload);
                    checkSumFile = Files.CreateCheckSumFile(fileFromStageToUpload);
                    Files.UploadFile(checkSumFile);
                    File.Delete(checkSumFile);
                    logFile = Files.CreateLogFile(fileFromStageToUpload);
                    Files.UploadFile(logFile);
                    File.Delete(logFile);
                }
            }
            catch (Exception e)
            {
                Logging.Log(e.Message, EventLogEntryType.Error);
            }
        }
    }
}
