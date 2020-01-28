using System;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using Amazon.S3.Util;

namespace hbkpsvc
{
    class ConfigGlobal
    {
        private static string _cloudAccessKey;
        private static string _cloudSecretKey;
        private static string _cloudBucket;
        private static string _cloudSubDirectory;
        private static string _sourceDir;
        private static string _stageDir;
        private static string _fileRegEx;
        private static bool _fileRegExCaseSensitive;
        private static long _intervalMs;
        private static bool _createMD5CheckFile;

        public static string CloudAccessKey { get => _cloudAccessKey; set => _cloudAccessKey = value; }
        public static string CloudSecretKey { get => _cloudSecretKey; set => _cloudSecretKey = value; }
        public static string CloudBucket { get => _cloudBucket; set => _cloudBucket = value; }
        public static string CloudSubDirectory { get => _cloudSubDirectory; set => _cloudSubDirectory = value; }
        public static string SourceDir { get => _sourceDir; set => _sourceDir = value; }
        public static string StageDir { get => _stageDir; set => _stageDir = value; }
        public static string FileRegEx { get => _fileRegEx; set => _fileRegEx = value; }
        public static bool FileRegExCaseSensitive { get => _fileRegExCaseSensitive; set => _fileRegExCaseSensitive = value; }
        public static long IntervalMs { get => _intervalMs; set => _intervalMs = value; }
        public static bool CreateMD5CheckFile { get => _createMD5CheckFile; set => _createMD5CheckFile = value; }

        public static void ReadConfig()
        {
            var serviceIniConfig = new IniFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"));

            //Console.WriteLine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"));

            Logging.Log("Lendo arquivo de configurações disponível em: '" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini") + "'.", System.Diagnostics.EventLogEntryType.Information);

            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini")))
            {
                Logging.Log("Arquivo de configuração não encontrado. O serviço precisa de um arquivo de configurações para ser inicializado. Arquivo: "+ Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"), System.Diagnostics.EventLogEntryType.Error);
                Environment.Exit(1);
            }

            if (serviceIniConfig.KeyExists("access_key", "Cloud"))
            {
                CloudAccessKey = serviceIniConfig.Read("access_key", "Cloud").ToString();
            }
            else
            {
                //IsConfigFileValid = false;
                Logging.Log("A chave 'access_key' não foi encontrada no arquivo de configuração.", System.Diagnostics.EventLogEntryType.Error);
                Environment.Exit(1);
            }

            if (serviceIniConfig.KeyExists("access_secret", "Cloud"))
            {
                CloudSecretKey = serviceIniConfig.Read("access_secret", "Cloud").ToString();
            }
            else
            {
                //IsConfigFileValid = false;
                Logging.Log("A chave 'access_secret' não foi encontrada no arquivo de configuração.", System.Diagnostics.EventLogEntryType.Error);
                Environment.Exit(1);
            }

            if (serviceIniConfig.KeyExists("client", "Client"))
            {
                CloudSubDirectory = serviceIniConfig.Read("client", "Client").ToString();
            }
            else
            {
                //IsConfigFileValid = false;
                Logging.Log("A chave 'client' não foi encontrada no arquivo de configuração.", System.Diagnostics.EventLogEntryType.Error);
                Environment.Exit(1);
            }

            if (serviceIniConfig.KeyExists("source_dir", "Properties"))
            {
                SourceDir = serviceIniConfig.Read("source_dir", "Properties").ToString();

                if (!Directory.Exists(SourceDir))
                {
                    Logging.Log("A chave 'source_dir' não possui um diretório válido.", System.Diagnostics.EventLogEntryType.Error);
                    Environment.Exit(1);
                }
            }
            else
            {
                //IsConfigFileValid = false;
                Logging.Log("A chave 'source_dir' não foi encontrada no arquivo de configuração.", System.Diagnostics.EventLogEntryType.Error);
                Environment.Exit(1);
            }

            if (serviceIniConfig.KeyExists("stage_dir", "Properties"))
            {
                StageDir = serviceIniConfig.Read("stage_dir", "Properties").ToString();

                if (!Directory.Exists(StageDir))
                {
                    Logging.Log("A chave 'source_dir' não possui um diretório válido.", System.Diagnostics.EventLogEntryType.Error);
                    Environment.Exit(1);
                }
            }
            else
            {
                //IsConfigFileValid = false;
                Logging.Log("A chave 'stage_dir' não foi encontrada no arquivo de configuração.", System.Diagnostics.EventLogEntryType.Error);
                Environment.Exit(1);
            }

            if (serviceIniConfig.KeyExists("file_regex", "Properties"))
            {
                FileRegEx = serviceIniConfig.Read("file_regex", "Properties").ToString();
            }
            else
            {
                //IsConfigFileValid = false;
                Logging.Log("A chave 'file_regex' não foi encontrada no arquivo de configuração.", System.Diagnostics.EventLogEntryType.Error);
                Environment.Exit(1);
            }

            if (serviceIniConfig.KeyExists("file_regex_case_sensitive", "Properties"))
            {
                try
                {
                    FileRegExCaseSensitive = Convert.ToBoolean(Convert.ToInt64(serviceIniConfig.Read("file_regex_case_sensitive", "Properties")));
                }
                catch (Exception e)
                {
                    //IsConfigFileValid = false;
                    Logging.Log("A chave 'file_regex_case_sensitive' não possui um valor de configuração válido.", System.Diagnostics.EventLogEntryType.Error);
                    Logging.Log(e.Message, System.Diagnostics.EventLogEntryType.Error);
                    Environment.Exit(1);
                }

            }
            else
            {
                //IsConfigFileValid = false;
                Logging.Log("A chave 'file_regex_case_sensitive' não foi encontrada no arquivo de configuração.", System.Diagnostics.EventLogEntryType.Error);
                Environment.Exit(1);
            }

            if (serviceIniConfig.KeyExists("interval_ms", "Properties"))
            {
                try
                {
                    IntervalMs = Convert.ToInt64(serviceIniConfig.Read("interval_ms", "Properties"));
                }
                catch (Exception e)
                {
                    //IsConfigFileValid = false;
                    Logging.Log("A chave 'interval_ms' não possui um valor de configuração válido.", System.Diagnostics.EventLogEntryType.Error);
                    Logging.Log(e.Message, System.Diagnostics.EventLogEntryType.Error);
                    Environment.Exit(1);
                }

            }
            else
            {
                //IsConfigFileValid = false;
                Logging.Log("A chave 'interval_ms' não foi encontrada no arquivo de configuração.", System.Diagnostics.EventLogEntryType.Error);
                Environment.Exit(1);
            }

            if (serviceIniConfig.KeyExists("bucket", "Cloud"))
            {
                BasicAWSCredentials awsCredentials = null;
                IAmazonS3 client = null;

                try
                {
                    CloudBucket = serviceIniConfig.Read("bucket", "Cloud").ToString();
                    awsCredentials = new BasicAWSCredentials(ConfigGlobal.CloudAccessKey, ConfigGlobal.CloudSecretKey);
                    client = Amazon.AWSClientFactory.CreateAmazonS3Client(awsCredentials, RegionEndpoint.SAEast1);
                    if (AmazonS3Util.DoesS3BucketExist(client, CloudBucket))
                    {
                        Logging.Log("O bucket informado no arquivo de configurações foi encontrado.", System.Diagnostics.EventLogEntryType.Information);
                    }
                    else
                    {
                        Logging.Log("O bucket informado no arquivo de configurações não foi encontrado. Por favor, verifique se há conexão com a internet ou se o bucket informado realmente existe.", System.Diagnostics.EventLogEntryType.Warning);
                    }
                }
                catch (Exception e)
                {
                    Logging.Log(e.Message, System.Diagnostics.EventLogEntryType.Error);
                    Environment.Exit(1);
                }
            }
            else
            {
                //IsConfigFileValid = false;
                Logging.Log("A chave 'bucket' não foi encontrada no arquivo de configuração.", System.Diagnostics.EventLogEntryType.Error);
                Environment.Exit(1);
            }


            if (serviceIniConfig.KeyExists("create_md5_checksum_file", "Properties"))
            {
                try
                {
                    CreateMD5CheckFile = Convert.ToBoolean(Convert.ToInt64(serviceIniConfig.Read("create_md5_checksum_file", "Properties")));
                }
                catch (Exception e)
                {
                    //IsConfigFileValid = false;
                    Logging.Log("A chave 'create_md5_checksum_file' não possui um valor de configuração válido.", System.Diagnostics.EventLogEntryType.Error);
                    Logging.Log(e.Message, System.Diagnostics.EventLogEntryType.Error);
                    Environment.Exit(1);
                }

            }
            else
            {
                //IsConfigFileValid = false;
                Logging.Log("A chave 'create_md5_checksum_file' não foi encontrada no arquivo de configuração.", System.Diagnostics.EventLogEntryType.Error);
                Environment.Exit(1);
            }

            Logging.Log("Leitura de arquivo de configurações feita com sucesso.", System.Diagnostics.EventLogEntryType.Information);
        }
    }
}
