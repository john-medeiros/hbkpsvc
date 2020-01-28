using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using System.Text;
using System.Security.Cryptography;

namespace hbkpsvc
{
    class Files
    {
        public static List<string> GetFilesToUpload(string directory, string regex)
        {
            string[] allFilesFound = null;
            List<string> filesToUpload = null;

            try
            {
                allFilesFound = Directory.GetFiles(directory, "*.*");
                filesToUpload = new List<string>();
                foreach (string fileFound in allFilesFound)
                {
                    if (Regex.IsMatch(Path.GetFileName(fileFound), regex))
                    {
                        filesToUpload.Add(fileFound);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Log(e.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return filesToUpload;
        }

        public static List<string> GetFilesFromStageToUpload(string stageDir)
        {
            string[] allFilesFound = null;
            List<string> filesToUpload = null;
            try
            {
                allFilesFound = Directory.GetFiles(stageDir, "*.gz");
                filesToUpload = new List<string>();

                foreach (string fileFound in allFilesFound)
                {
                    filesToUpload.Add(fileFound);
                }
            }
            catch (Exception e)
            {
                Logging.Log(e.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return filesToUpload;
        }

        public static void ClearStage(string stageDir)
        {
            try
            {
                Logging.Log("Removendo arquivos da pasta temporária...", System.Diagnostics.EventLogEntryType.Information);

                System.IO.DirectoryInfo di = new DirectoryInfo(ConfigGlobal.StageDir);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                Logging.Log("Remoção concluída", System.Diagnostics.EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                Logging.Log("Erro ao remover arquivos da pasta temporária.", System.Diagnostics.EventLogEntryType.Error);
                Logging.Log(e.Message, System.Diagnostics.EventLogEntryType.Error);
            }
        }

        public static void CompressFileToUpload(string filename)
        {

            FileInfo fileToBeGZipped = new FileInfo(filename);
            FileInfo gzipFileName = new FileInfo(string.Concat(fileToBeGZipped.FullName, ".gz"));
            using (FileStream fileToBeZippedAsStream = fileToBeGZipped.OpenRead())
            {
                using (FileStream gzipTargetAsStream = gzipFileName.Create())
                {
                    using (GZipStream gzipStream = new GZipStream(gzipTargetAsStream, CompressionMode.Compress))
                    {
                        try
                        {
                            fileToBeZippedAsStream.CopyTo(gzipStream);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }


        public static string GetFileCheckSumMD5(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        public static string CreateCheckSumFile(string file)
        {

            string checkSumFile = Path.Combine(ConfigGlobal.StageDir, Path.ChangeExtension(file, "md5"));
            string checkSum = GetFileCheckSumMD5(file);
            File.AppendAllText(checkSumFile, checkSum);

            return checkSumFile;
        }

        public static string GetMetricLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Hostname: ");
            sb.AppendLine(System.Environment.MachineName);
            sb.Append("OS: ");
            sb.AppendLine(System.Environment.OSVersion.VersionString);
            return sb.ToString();
        }

        public static string CreateLogFile(string file)
        {
            string metric = GetMetricLog();
            string logFile = Path.Combine(ConfigGlobal.StageDir, Path.ChangeExtension(file, "log"));
            File.AppendAllText(logFile, metric);
            return logFile;
        }


        public static void UploadFile(string file)
        {
            BasicAWSCredentials awsCredentials = null;
            IAmazonS3 client = null;
            TransferUtility utility = null;
            TransferUtilityUploadRequest request = null;
            StringBuilder sb = new StringBuilder();
            string message = null;

            try
            {
                

                awsCredentials = new BasicAWSCredentials(ConfigGlobal.CloudAccessKey, ConfigGlobal.CloudSecretKey);
                client = Amazon.AWSClientFactory.CreateAmazonS3Client(awsCredentials, RegionEndpoint.SAEast1);
                utility = new TransferUtility(client);
                request = new TransferUtilityUploadRequest();
                request.BucketName = ConfigGlobal.CloudBucket + @"/" + ConfigGlobal.CloudSubDirectory;
                request.FilePath = file;
                request.Key = Path.GetFileName(file);

                sb.Append("Iniciando upload: ");
                sb.Append("Destino: ");
                sb.AppendLine(request.BucketName);
                sb.Append("Arquivo: ");
                sb.AppendLine(file);
                Logging.Log(sb.ToString(), System.Diagnostics.EventLogEntryType.Information);
                sb.Clear();
                utility.Upload(request);
                sb.Append("Upload do arquivo ");
                sb.AppendLine(file);
                sb.AppendLine(" realizado.");
                Logging.Log(sb.ToString(), System.Diagnostics.EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                message = "Erro ao realizar upload para unidade de armazenamento na nuvem.";
                message += Environment.NewLine;
                message += e.Message;
                Logging.Log(message, System.Diagnostics.EventLogEntryType.Error);
                //Logging.Log(e.Message, System.Diagnostics.EventLogEntryType.Error);
            }
        }
    }
}
