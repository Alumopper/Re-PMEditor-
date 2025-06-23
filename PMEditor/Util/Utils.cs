using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PMEditor
{
    public partial class TrackEditorPage
    {
        
        /// <summary>
        /// 坐标对齐转换。返回的x坐标是对齐的，y坐标是相对于底部的长度
        /// </summary>
        /// <returns>
        /// 对齐后的时间，对齐后的小节数，对齐后的排数，对齐后鼠标的位置
        /// </returns>
        ///
        /// 
        public (double time, int measure ,int beat, Point mousePos, int rail) GetAlignedPoint(Point p)
        {
            //获取时间
            //获取鼠标位置，生成note位置预览
            var mousePos = p;
            var width = NotePanel.ActualWidth / 9;
            //x坐标对齐
            var rail = (int)(mousePos.X / width);
            //如果是Tap，额外需要对齐主线
            if (editingMode == 0 && Window.puttingTap)
            {
                if (rail == 0) { rail = 1; }
                if (rail == 2) { rail = 3; }
                if (rail == 4) { rail = 5; }
                if (rail == 6) { rail = 7; }
                if (rail == 8) { rail = 7; }
            }
            mousePos.X = rail * width;
            //获取当前小节数
            var (measure, deltime) = Window.track.GetMeasureFromTime(
                (NotePanel.ActualHeight - mousePos.Y) / NotePanel.ActualHeight * currDisplayLength + Window.playerTime
                );
            //获取当前bpm
            var bpm = Window.track.GetBPM(measure);
            //获取当前拍数
            var beat = (int)(deltime / 60 * bpm * divideNum);
            //计算y坐标
            var time = Window.track.GetTimeRange(measure).startTime + beat/(double)divideNum * (60 / bpm);
            mousePos.Y = GetBottomYFromTime(time);
            return (time, measure, beat, mousePos, rail);
        }

        /// <summary>
        /// 坐标时间转换
        /// </summary>
        /// <param name="y">相对于底部的长度</param>
        /// <returns></returns>
        public double GetTimeFromBottomY(double y)
        {
            return y / NotePanel.ActualHeight * currDisplayLength + Window.player.Position.TotalSeconds;
        }

        public double GetBottomYFromTime(double time)
        {
            return (time - Window.playerTime) / currDisplayLength * NotePanel.ActualHeight;
        }
    }

    public class Utils
    {
        public static Guid GeneUnNaturalUUID()
        {
            byte[] uuidByte = Guid.NewGuid().ToByteArray();
            // 修改版本号，将第13个字节设置为一个不存在的值（例如0xFF）
            uuidByte[6] = (byte)((uuidByte[6] & 0x0F) | 0xFF);

            // 修改变种，将第17个字节设置为一个不存在的值（例如0xFF）
            uuidByte[8] = (byte)((uuidByte[8] & 0x3F) | 0xFF);

            return new Guid(uuidByte);
        }

        public static string ToNBTUUID(Guid uuid)
        {
            byte[] bytes = uuid.ToByteArray();
            int[] array = new int[4];
            array[0] = BitConverter.ToInt32(bytes[0..4].ToArray());
            byte a = bytes[4];
            byte b = bytes[5];
            bytes[4] = bytes[6];
            bytes[5] = bytes[7];
            bytes[6] = a;
            bytes[7] = b;
            array[1] = BitConverter.ToInt32(bytes[4..8].ToArray());
            array[2] = BitConverter.ToInt32(bytes[8..12].Reverse().ToArray());
            array[3] = BitConverter.ToInt32(bytes[12..].Reverse().ToArray());

            return $"[I;{array[0]},{array[1]},{array[2]},{array[3]}]";
        }

        public static Guid FromNBTUUID(string uuid)
        {
            string[] array = uuid[3..^1].Split(',');
            byte[] bytes = new byte[16] {0,0,0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            BitConverter.GetBytes(int.Parse(array[0])).Reverse().ToArray().CopyTo(bytes, 0);
            BitConverter.GetBytes(int.Parse(array[1])).Reverse().ToArray().CopyTo(bytes, 4);
            byte a = bytes[4];
            byte b = bytes[5];
            bytes[4] = bytes[6];
            bytes[5] = bytes[7];
            bytes[6] = a;
            bytes[7] = b;
            BitConverter.GetBytes(int.Parse(array[2])).Reverse().ToArray().CopyTo(bytes, 8);
            bytes[8..12].Reverse().ToArray().CopyTo(bytes, 8);
            BitConverter.GetBytes(int.Parse(array[3])).Reverse().ToArray().CopyTo(bytes, 12);
            bytes[12..].Reverse().ToArray().CopyTo(bytes, 12);
            return new Guid(bytes);
        }

        public static string? GetVSCodePath()
        {
            // 获取 PATH 环境变量
            string? pathEnv = Environment.GetEnvironmentVariable("PATH");

            if (pathEnv == null)
            {
                return null;
            }

            // 分割 PATH 环境变量中的各个路径
            string[] paths = pathEnv.Split(Path.PathSeparator);

            foreach (var path in paths)
            {
                // 构建可能的 VSCode 可执行文件路径
                string potentialPath = Path.Combine(path, "code.cmd");

                // 检查文件是否存在
                if (File.Exists(potentialPath))
                {
                    //从父路径中找到Code.exe
                    string parentPath = Directory.GetParent(potentialPath).Parent.FullName;
                    string codePath = Path.Combine(parentPath, "Code.exe");
                    if (File.Exists(codePath))
                    {
                        return codePath;
                    }
                    else
                    {
                        return potentialPath;
                    }
                }
            }

            return null;
        }

        public static void CopyAllFiles(string sourceDir, string destDir)
        {
            // 确保目标目录存在
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // 获取源目录中的所有文件
            string[] files = Directory.GetFiles(sourceDir);

            // 遍历每个文件并复制到目标目录
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);  // true表示覆盖同名文件
                Console.WriteLine($"Copied: {file} to {destFile}");
            }

            // 获取源目录中的所有子目录
            string[] subDirectories = Directory.GetDirectories(sourceDir);

            // 遍历每个子目录并递归复制其中的文件
            foreach (string subDirectory in subDirectories)
            {
                string subDirectoryName = Path.GetFileName(subDirectory);
                string destSubDir = Path.Combine(destDir, subDirectoryName);
                CopyAllFiles(subDirectory, destSubDir);
            }
        }

        public static void OpenInExplorer(string path)
        {
            // 使用系统默认的文件管理器打开指定路径
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

    }
    
    public static class Extensions
    {
        public static T Apply<T>(this T obj, Action<T> action)
        {
            action(obj);
            return obj;
        }
    }

}