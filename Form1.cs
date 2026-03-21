using System.Runtime.InteropServices;

namespace WinOnTop;

public partial class Form1 : Form
{
    // Windows API 常量
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int GWL_EXSTYLE = -20;
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_CONTROL = 0x0002;
    private const int VK_F8 = 0x77;
    private const int HOTKEY_ID = 1;

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

    public Form1()
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // 注册全局热键 Ctrl+F8
        RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL, VK_F8);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // 注销热键
        UnregisterHotKey(this.Handle, HOTKEY_ID);

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
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
        {
            // Ctrl+F8 被按下，切换当前活动窗口的置顶状态
            ToggleTopMostForForegroundWindow();
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
