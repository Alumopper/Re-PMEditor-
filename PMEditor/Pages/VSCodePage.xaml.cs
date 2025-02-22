using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;
// ReSharper disable InconsistentNaming

namespace PMEditor.Pages;

public partial class VSCodePage : Page
{
    // 引入 Win32 API
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern long GetWindowLong(IntPtr hWnd, int nIndex);

    private const int GWL_STYLE = -16;
    private const long WS_CHILD = 0x40000000;

    private Process? _vscode = null;

    public string? FilePath = null;
    
    public VSCodePage()
    {
        InitializeComponent();
    }

    public void Init()
    {
        // 检查 VS Code 可用性并嵌入
        if (EditorWindow.Instance.vscodePath != null)
        {
            EmbedVSCode(EditorWindow.Instance.vscodePath);
        }
        else
        {
            PromptInstallVSCode();
        }
    }
    
    private bool IsVSCodeAvailable(out string vscodePath)
    {
        // 检查是否可以通过 PATH 找到 code 命令
        vscodePath = "code"; // 默认使用 PATH 中的 code 命令
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "code",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            })?.Dispose();
            return true;
        }
        catch
        {
            // 如果 PATH 中找不到 code，则尝试检查默认安装路径
            string defaultPath = @"C:\Program Files\Microsoft VS Code\Code.exe";
            if (File.Exists(defaultPath))
            {
                vscodePath = defaultPath;
                return true;
            }
        }

        return false;
    }

    private void PromptInstallVSCode()
    {
        var result = MessageBox.Show(
            "未检测到 VS Code，请安装 VS Code 后重试。\n是否跳转到 VS Code 官方下载页面？",
            "VS Code 未安装",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            // 打开 VS Code 官方下载页面
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://code.visualstudio.com/",
                UseShellExecute = true
            });
        }
    }
    
    private void EmbedVSCode(string vscodePath)
    {
        try
        {
            _vscode = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = vscodePath,
                    Arguments = FilePath,
                    UseShellExecute = false
                }
            };
            _vscode.Start();
            //等待vscode启动
            _vscode.WaitForInputIdle();
            //获取vscode的句柄
            IntPtr vscodeHandle = _vscode.MainWindowHandle;
            //嵌入
            var windowsFormsHostHandle = WindowsFormsHost.Handle;
            SetParent(vscodeHandle, windowsFormsHostHandle);
            //设置样式为子窗口
            long sytle = GetWindowLong(vscodeHandle, GWL_STYLE);
            SetWindowLong(vscodeHandle, GWL_STYLE, sytle | WS_CHILD);
            //设置大小
            MoveWindow(vscodeHandle, 0, 0, (int)WindowsFormsHost.ActualWidth, (int)WindowsFormsHost.ActualHeight, true);
        }
        catch (Exception e)
        {
            MessageBox.Show("不好！嵌入 VS Code 失败啦: \n" + e.Message);
        }
    }

    private void VSCodePage_OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_vscode is { HasExited: false })
        {
            _vscode.Kill();
        }
    }
}