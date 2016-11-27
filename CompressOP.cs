using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace CompressCtr
{
    public class CompressOP
    {
        #region 静态初始化器 解决文档名称乱码问题,出现乱码就是因为CodePage不对
        static CompressOP()
        {
            // 解决文档名称乱码问题,出现乱码就是因为CodePage不对
            Encoding gbk = Encoding.GetEncoding("gbk");
            ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = gbk.CodePage;
        }
        #endregion

        #region 压缩

        #region 将指定的多个文件压缩到指定的文件 +static bool ZipFiles(string destFilePath, params string[] srcFilePaths)
        /// <summary>
        /// 将指定的多个文件压缩到指定的文件
        /// </summary>
        /// <param name="destFilePath">文件的目的路径</param>
        /// <param name="srcFilePaths">源文件的路径</param>
        /// <returns></returns>
        public static bool ZipFiles(string destFilePath, params string[] srcFilePaths)
        {
            List<string> names = new List<string>();
            string tmpName;
            foreach (var item in srcFilePaths)
            {
                tmpName = System.IO.Path.GetFileName(item);
                if (names.Contains(tmpName))
                {
                    throw new Exception("发现同名的源文件(" + tmpName + ")导致不能成功压缩,请处理后再进行压缩!");
                }
                else
                {
                    names.Add(tmpName);
                }
            }
            using (ZipOutputStream s = new ZipOutputStream(File.Create(destFilePath)))
            {
                s.SetLevel(6);  //设置压缩等级，等级越高压缩效果越明显，但占用CPU也会更多
                for (int i = 0; i < srcFilePaths.Length; i++)
                {
                    using (FileStream fs = File.OpenRead(srcFilePaths[i]))
                    {
                        byte[] buffer = new byte[4 * 1024];  //缓冲区，每次操作大小
                        ZipEntry entry = new ZipEntry(names[i]); //创建压缩包内的文件
                        entry.DateTime = DateTime.Now;  //文件创建时间
                        s.PutNextEntry(entry);          //将文件写入压缩包

                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);    //读取文件内容(1次读4M，写4M)
                            s.Write(buffer, 0, sourceBytes);                    //将文件内容写入压缩相应的文件
                        } while (sourceBytes > 0);
                    }
                }
                s.CloseEntry();
            }
            return true;
        }
        #endregion

        #region 将指定的多个流以及流对应的文件名称压缩到指定的文件里 +static bool ZipFiles(string destFilePath, List<Stream> srcStreams, List<string> srcFileNames)
        /// <summary>
        /// 将指定的多个流以及流对应的文件名称压缩到指定的文件里
        /// </summary>
        /// <param name="destFilePath">文件的目的路径</param>
        /// <param name="srcStreams">源文件所在的流集合</param>
        /// <param name="srcFileNames">每个源文件流所对应的文件名称</param>
        /// <returns></returns>
        public static bool ZipFiles(string destFilePath, List<Stream> srcStreams, List<string> srcFileNames)
        {
            List<string> names = new List<string>();
            foreach (var item in srcFileNames)
            {
                if (names.Contains(item))
                {
                    throw new Exception("发现同名的源文件(" + item + ")导致不能成功压缩,请处理后再进行压缩!");
                }
                else
                {
                    names.Add(item);
                }
            }
            using (ZipOutputStream s = new ZipOutputStream(File.Create(destFilePath)))
            {
                s.SetLevel(6);  //设置压缩等级，等级越高压缩效果越明显，但占用CPU也会更多
                for (int i = 0; i < srcStreams.Count; i++)
                {
                    using (srcStreams[i])
                    {
                        byte[] buffer = new byte[4 * 1024];  //缓冲区，每次操作大小
                        ZipEntry entry = new ZipEntry(names[i].ToString()); //创建压缩包内的文件
                        entry.DateTime = DateTime.Now;  //文件创建时间
                        s.PutNextEntry(entry);          //将文件写入压缩包

                        int sourceBytes;
                        do
                        {
                            sourceBytes = srcStreams[i].Read(buffer, 0, buffer.Length);    //读取文件内容(1次读4M，写4M)
                            s.Write(buffer, 0, sourceBytes);                    //将文件内容写入压缩相应的文件
                        } while (sourceBytes > 0);
                    }
                }
                s.CloseEntry();
            }
            return true;
        }
        #endregion

        #region 将指定的多个文件压缩到网站的临时文件夹下(~/TempFiles) +static bool ZipFiles2Temp(string fileName, params string[] srcFilePaths)
        /// <summary>
        /// 将指定的多个文件压缩到网站的临时文件夹下(~/TempFiles)
        /// </summary>
        /// <param name="fileName">压缩后的文件名称</param>
        /// <param name="srcFilePaths"></param>
        /// <returns></returns>
        public static bool ZipFiles2Temp(string fileName, params string[] srcFilePaths)
        {
            string destFilePath = System.Web.HttpContext.Current.Server.MapPath("~/TempFiles") + "/" + fileName;
            if (File.Exists(destFilePath))
            {
                throw new Exception("临时文件夹中已存在文件:" + fileName + ",请重新设置压缩后的名字!");
            }

            return ZipFiles(destFilePath, srcFilePaths);
        } 
        #endregion

        #region 快速将指定的多个文件压缩到网站的临时文件夹下(~/TempFiles),返回压缩后的文件名称 +static string ZipFiles2TempFast(params string[] srcFilePaths)
        /// <summary>
        /// 快速将指定的多个文件压缩到网站的临时文件夹下(~/TempFiles),返回压缩后的文件名称
        /// </summary>
        /// <param name="srcFilePaths"></param>
        /// <returns></returns>
        public static string ZipFiles2TempFast(params string[] srcFilePaths)
        {
            string fileName = Guid.NewGuid().ToString().Replace("-", "") + ".zip";
            string destFilePath = System.Web.HttpContext.Current.Server.MapPath("~/TempFiles") + "/" + fileName;
            ZipFiles(destFilePath, srcFilePaths);
            return fileName;
        } 
        #endregion

        #region 快速将指定的多个文件流压缩到网站的临时文件夹下(~/TempFiles)并为每个流指定文件名称,返回压缩后的文件名称 +static string ZipFiles2TempFast(List<Stream> srcStreams, List<string> srcFileNames)
        /// <summary>
        /// 快速将指定的多个文件流压缩到网站的临时文件夹下(~/TempFiles)并为每个流指定文件名称,返回压缩后的文件名称
        /// </summary>
        /// <param name="srcStreams"></param>
        /// <param name="srcFileNames"></param>
        /// <returns></returns>
        public static string ZipFiles2TempFast(List<Stream> srcStreams, List<string> srcFileNames)
        {
            string fileName = Guid.NewGuid().ToString().Replace("-", "") + ".zip";
            string destFilePath = System.Web.HttpContext.Current.Server.MapPath("~/TempFiles") + "/" + fileName;
            ZipFiles(destFilePath, srcStreams, srcFileNames);
            return fileName;
        } 
        #endregion
        
        #region 将指定的文件夹压缩到指定的文件里 +static bool ZipFolders(string destFilePath, string srcFolderPath)
        /// <summary>
        /// 将指定的文件夹压缩到指定的文件里
        /// </summary>
        /// <param name="destFilePath"></param>
        /// <param name="srcFolderPath"></param>
        /// <returns></returns>
        public static bool ZipFolders(string destFilePath, string srcFolderPath)
        {
            using (ZipOutputStream s = new ZipOutputStream(new FileStream(destFilePath, FileMode.Create)))
            {
                Compress(srcFolderPath, s,"");
                s.CloseEntry();
            }
            return true;
        } 
        #endregion

        #region 将指定的文件夹压缩到网站临时文件夹下(~/TempFiles)并且指定好名称 +static bool ZipFolders2Temp(string destFileName, string srcFolderPath)
        /// <summary>
        /// 将指定的文件夹压缩到网站临时文件夹下(~/TempFiles)并且指定好名称
        /// </summary>
        /// <param name="destFileName"></param>
        /// <param name="srcFolderPath"></param>
        /// <returns></returns>
        public static bool ZipFolders2Temp(string destFileName, string srcFolderPath)
        {
            string destFilePath = System.Web.HttpContext.Current.Server.MapPath("~/TempFiles") + "/" + destFileName;
            if (File.Exists(destFilePath))
            {
                throw new Exception("临时文件夹中已存在文件:" + destFileName + ",请重新设置压缩后的名字!");
            }
            return ZipFolders(destFilePath, srcFolderPath);
        } 
        #endregion

        #region 快速将指定的文件夹压缩到网站临时文件夹下(~/TempFiles),返回压缩完后的文件名称 +static string ZipFolders2TempFast(string srcFolderPath)
        /// <summary>
        /// 快速将指定的文件夹压缩到网站临时文件夹下(~/TempFiles),返回压缩完后的文件名称
        /// </summary>
        /// <param name="srcFolderPath"></param>
        /// <returns></returns>
        public static string ZipFolders2TempFast(string srcFolderPath)
        {
            string fileName = Guid.NewGuid().ToString().Replace("-", "") + ".zip";
            string destFilePath = System.Web.HttpContext.Current.Server.MapPath("~/TempFiles") + "/" + fileName;
            ZipFolders2Temp(fileName, srcFolderPath);
            return fileName;
        } 
        #endregion
        
        #region 递归压缩目录 +static void Compress(string source, ZipOutputStream s)
        /// <summary>
        /// 递归压缩目录
        /// </summary>
        /// <param name="source">源目录</param>
        /// <param name="s">ZipOutputStream对象</param>
        public static void Compress(string source, ZipOutputStream s,string dirName)
        {
            string[] filenames = Directory.GetFileSystemEntries(source);
            foreach (string file in filenames)
            {
                if (Directory.Exists(file))
                {
                    Compress(file, s, (dirName == "" ? "" : (dirName + "/")) + GetLastDirOrFileNameByPath(file));  //递归压缩子文件夹                    
                }
                else
                {
                    using (FileStream fs = File.OpenRead(file))
                    {
                        byte[] buffer = new byte[4 * 1024];
                        ZipEntry entry = new ZipEntry((dirName==""?"":(dirName + "/")) + GetLastDirOrFileNameByPath(file));     //此处去掉盘符，如D:\123\1.txt 去掉D:
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);

                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }
            }
            if (dirName.Contains('/'))
            {
                string[] arr = dirName.Split('/');
                dirName = "";
                for (int i = 0; i < arr.Length-1; i++)
                {
                    dirName += "/" + arr[i];
                }
            }
        }

        private static string GetLastDirOrFileNameByPath(string filePath)
        {
            if (filePath.Contains("/") || filePath.Contains("\\"))
            {
                string[] arr = filePath.Split('/', '\\');
                List<string> list= arr.ToList();
                list.ForEach(i =>
                {
                    if (i == "")
                    {
                        list.Remove(i);
                    }
                });
                return list[list.Count - 1];
            }
            else
            {
                return filePath;
            }
        }
        #endregion

        #endregion

        #region 解压缩 +static bool Decompress(string srcFilePath, string destFolderPath)
        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="sourceFile">源文件</param>
        /// <param name="targetPath">目标路经</param>
        public static bool Decompress(string srcFilePath, string destFolderPath)
        {
            if (!File.Exists(srcFilePath))
            {
                throw new FileNotFoundException(string.Format("未能找到文件 '{0}' ", srcFilePath));
            }
            if (!Directory.Exists(destFolderPath))
            {
                Directory.CreateDirectory(destFolderPath);
            }
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(srcFilePath)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directorName = Path.Combine(destFolderPath, Path.GetDirectoryName(theEntry.Name));
                    string fileName = Path.Combine(directorName, Path.GetFileName(theEntry.Name));
                    // 创建目录
                    if (directorName.Length > 0)
                    {
                        Directory.CreateDirectory(directorName);
                    }
                    if (fileName != string.Empty)
                    {
                        using (FileStream streamWriter = File.Create(fileName))
                        {
                            int size = 4096;
                            byte[] data = new byte[4 * 1024];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else break;
                            }
                        }
                    }
                }
            }
            return true;
        }
        #endregion
    }
}