using System.Runtime.InteropServices;

namespace WinOnTop;

public partial class Form1 : Form
{
    // Windows API 常量
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020; // 点击穿透样式
    private const int GWL_EXSTYLE = -20;
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_CONTROL = 0x0002;
    private const int VK_F8 = 0x77;
    private const int VK_F9 = 0x78;
    private const int VK_F10 = 0x79;
    private const int HOTKEY_ID_TOPMOST = 1;
    private const int HOTKEY_ID_OPACITY = 2;
    private const int HOTKEY_ID_GHOST = 3;
    private const uint LWA_ALPHA = 0x00000002;

    // Windows API 函数
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private bool isDragging = false;
    private IntPtr lastTargetWindow = IntPtr.Zero;
    private IntPtr lastOpacityWindow = IntPtr.Zero;
    private IntPtr lastGhostWindow = IntPtr.Zero; // 幽灵模式窗口
    private byte currentOpacity = 255; // 当前透明度 (0-255)
    private bool isGhostMode = false; // 是否启用幽灵模式

    public Form1()
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // 注册全局热键 Ctrl+F8 (置顶/取消置顶)
        RegisterHotKey(this.Handle, HOTKEY_ID_TOPMOST, MOD_CONTROL, VK_F8);
        // 注册全局热键 Ctrl+F9 (设置透明度)
        RegisterHotKey(this.Handle, HOTKEY_ID_OPACITY, MOD_CONTROL, VK_F9);
        // 注册全局热键 Ctrl+F10 (幽灵模式 - 半透明+点击穿透)
        RegisterHotKey(this.Handle, HOTKEY_ID_GHOST, MOD_CONTROL, VK_F10);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // 注销热键
        UnregisterHotKey(this.Handle, HOTKEY_ID_TOPMOST);
        UnregisterHotKey(this.Handle, HOTKEY_ID_OPACITY);
        UnregisterHotKey(this.Handle, HOTKEY_ID_GHOST);

        // 隐藏托盘图标
        if (notifyIcon != null)
        {
            notifyIcon.Visible = false;
        }

        base.OnFormClosing(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        // 当窗口最小化时，隐藏到托盘
        if (this.WindowState == FormWindowState.Minimized)
        {
            this.Hide();
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY)
        {
            int hotkeyId = m.WParam.ToInt32();
            if (hotkeyId == HOTKEY_ID_TOPMOST)
            {
                // Ctrl+F8 被按下，切换当前活动窗口的置顶状态
                ToggleTopMostForForegroundWindow();
            }
            else if (hotkeyId == HOTKEY_ID_OPACITY)
            {
                // Ctrl+F9 被按下，为当前活动窗口设置透明度
                SetOpacityForForegroundWindow();
            }
            else if (hotkeyId == HOTKEY_ID_GHOST)
            {
                // Ctrl+F10 被按下，切换幽灵模式
                ToggleGhostModeForForegroundWindow();
            }
        }
        base.WndProc(ref m);
    }

    private void ToggleTopMostForForegroundWindow()
    {
        IntPtr hWnd = GetForegroundWindow();
        if (hWnd != IntPtr.Zero && hWnd != this.Handle && IsWindow(hWnd))
        {
            ToggleWindowTopMost(hWnd);
        }
    }

    private void ToggleWindowTopMost(IntPtr hWnd)
    {
        // 检查窗口是否已经是置顶状态
        int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
        bool isTopMost = (exStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST;

        if (isTopMost)
        {
            // 取消置顶
            SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
        else
        {
            // 设置置顶
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
    }

    private void SetWindowTopMost(IntPtr hWnd)
    {
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    }

    private void SetOpacityForForegroundWindow()
    {
        IntPtr hWnd = GetForegroundWindow();
        if (hWnd != IntPtr.Zero && hWnd != this.Handle && IsWindow(hWnd))
        {
            lastOpacityWindow = hWnd;
            // 使用当前滑块设置的透明度值
            SetWindowOpacity(hWnd, currentOpacity);
        }
    }

    private void SetWindowOpacity(IntPtr hWnd, byte opacity)
    {
        if (hWnd == IntPtr.Zero || !IsWindow(hWnd))
            return;

        // 获取当前窗口样式
        int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
        
        // 添加 WS_EX_LAYERED 样式（如果还没有）
        if ((exStyle & WS_EX_LAYERED) == 0)
        {
            SetWindowLong(hWnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
        }

        // 设置透明度 (0-255)
        SetLayeredWindowAttributes(hWnd, 0, opacity, LWA_ALPHA);
    }

    private void RestoreWindowOpacity(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero || !IsWindow(hWnd))
            return;

        // 恢复完全不透明
        SetLayeredWindowAttributes(hWnd, 0, 255, LWA_ALPHA);
    }

    private void UpdateOpacityFromSlider(int sliderValue)
    {
        // 滑块值 0-100 转换为 0-255
        currentOpacity = (byte)(sliderValue * 255 / 100);
        
        // 如果有上次设置透明度的窗口，实时更新
        if (lastOpacityWindow != IntPtr.Zero && IsWindow(lastOpacityWindow))
        {
            SetWindowOpacity(lastOpacityWindow, currentOpacity);
        }
    }

    private void SetOpacityFromMenu(int percentage)
    {
        // 更新滑块值
        if (trackOpacity != null)
        {
            trackOpacity.Value = percentage;
            lblOpacityValue.Text = $"{percentage}%";
        }
        
        // 更新当前透明度
        currentOpacity = (byte)(percentage * 255 / 100);
        
        // 获取当前活动窗口并应用透明度
        IntPtr hWnd = GetForegroundWindow();
        if (hWnd != IntPtr.Zero && hWnd != this.Handle && IsWindow(hWnd))
        {
            lastOpacityWindow = hWnd;
            SetWindowOpacity(hWnd, currentOpacity);
        }
    }

    private void ToggleGhostModeForForegroundWindow()
    {
        IntPtr hWnd = GetForegroundWindow();
        if (hWnd != IntPtr.Zero && hWnd != this.Handle && IsWindow(hWnd))
        {
            ToggleGhostModeForWindow(hWnd);
        }
    }

    private void ToggleGhostModeFromMenu()
    {
        IntPtr hWnd = GetForegroundWindow();
        if (hWnd != IntPtr.Zero && hWnd != this.Handle && IsWindow(hWnd))
        {
            ToggleGhostModeForWindow(hWnd);
        }
    }

    private void ToggleGhostModeForWindow(IntPtr hWnd)
    {
        // 检查当前是否已经是幽灵模式
        int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
        bool isCurrentlyGhost = (exStyle & WS_EX_TRANSPARENT) == WS_EX_TRANSPARENT;

        if (isCurrentlyGhost)
        {
            // 取消幽灵模式
            DisableGhostMode(hWnd);
            UpdateGhostStatusLabel(false);
        }
        else
        {
            // 启用幽灵模式
            EnableGhostMode(hWnd);
            UpdateGhostStatusLabel(true);
        }
    }

    private void UpdateGhostStatusLabel(bool isGhost)
    {
        // 更新界面上的状态标签
        Control[] labels = this.Controls.Find("lblGhostStatus", true);
        if (labels.Length > 0 && labels[0] is Label lbl)
        {
            lbl.Text = isGhost ? "Status: Ghost Mode" : "Status: Normal";
            lbl.ForeColor = isGhost ? Color.Red : Color.Gray;
        }
    }

    private void EnableGhostMode(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero || !IsWindow(hWnd))
            return;

        lastGhostWindow = hWnd;

        // 获取当前窗口样式
        int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
        
        // 添加 WS_EX_LAYERED 和 WS_EX_TRANSPARENT 样式
        int newStyle = exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT;
        SetWindowLong(hWnd, GWL_EXSTYLE, newStyle);

        // 设置透明度为 50% (128)
        SetLayeredWindowAttributes(hWnd, 0, 128, LWA_ALPHA);
    }

    private void DisableGhostMode(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero || !IsWindow(hWnd))
            return;

        // 获取当前窗口样式
        int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
        
        // 移除 WS_EX_TRANSPARENT 样式（保留 WS_EX_LAYERED）
        int newStyle = exStyle & ~WS_EX_TRANSPARENT;
        SetWindowLong(hWnd, GWL_EXSTYLE, newStyle);

        // 恢复完全不透明
        SetLayeredWindowAttributes(hWnd, 0, 255, LWA_ALPHA);
    }

    private void picHand_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            isDragging = true;
            picHand.Cursor = Cursors.Cross;
        }
    }

    private void picHand_MouseUp(object sender, MouseEventArgs e)
    {
        if (isDragging)
        {
            isDragging = false;
            picHand.Cursor = Cursors.Hand;

            // 获取鼠标位置下的窗口
            IntPtr hWnd = WindowFromPoint(Cursor.Position);
            if (hWnd != IntPtr.Zero && hWnd != this.Handle && IsWindow(hWnd))
            {
                SetWindowTopMost(hWnd);
            }
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(System.Drawing.Point pt);

    private void btnExit_Click(object sender, EventArgs e)
    {
        this.Close();
    }
}
