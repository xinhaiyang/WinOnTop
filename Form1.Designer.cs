namespace WinOnTop;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private PictureBox picHand;
    private Label lblDragHint;
    private Label lblHotkey;
    private Button btnExit;
    private GroupBox grpOneWay;
    private GroupBox grpOtherWay;
    private NotifyIcon notifyIcon;
    private ContextMenuStrip contextMenu;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // 窗体设置
        this.Text = "Window On Top";
        this.ClientSize = new Size(320, 220);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = SystemColors.Control;
        this.Icon = CreateApplicationIcon();

        // One Way 分组框
        grpOneWay = new GroupBox();
        grpOneWay.Text = "One Way:";
        grpOneWay.Location = new Point(12, 8);
        grpOneWay.Size = new Size(290, 90);
        grpOneWay.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point);

        // 手型图标
        picHand = new PictureBox();
        picHand.Location = new Point(20, 25);
        picHand.Size = new Size(48, 48);
        picHand.BorderStyle = BorderStyle.Fixed3D;
        picHand.BackColor = Color.White;
        picHand.Cursor = Cursors.Hand;
        picHand.SizeMode = PictureBoxSizeMode.CenterImage;
        picHand.MouseDown += picHand_MouseDown;
        picHand.MouseUp += picHand_MouseUp;

        // 绘制手型图标
        picHand.Paint += (sender, e) =>
        {
            DrawHandIcon(e.Graphics, picHand.ClientRectangle);
        };

        // 拖拽提示文字
        lblDragHint = new Label();
        lblDragHint.Text = "Drag the hand to the target\nwindow, release the mouse.";
        lblDragHint.Location = new Point(80, 28);
        lblDragHint.Size = new Size(190, 40);
        lblDragHint.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point);

        grpOneWay.Controls.Add(picHand);
        grpOneWay.Controls.Add(lblDragHint);

        // Other Way 分组框
        grpOtherWay = new GroupBox();
        grpOtherWay.Text = "Other Way:";
        grpOtherWay.Location = new Point(12, 105);
        grpOtherWay.Size = new Size(290, 65);
        grpOtherWay.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point);

        // 热键提示
        lblHotkey = new Label();
        lblHotkey.Text = "Ctrl+F8  Top / Cancel";
        lblHotkey.Location = new Point(20, 28);
        lblHotkey.Size = new Size(150, 20);
        lblHotkey.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point);

        // 退出按钮
        btnExit = new Button();
        btnExit.Text = "Exit";
        btnExit.Location = new Point(200, 24);
        btnExit.Size = new Size(75, 25);
        btnExit.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point);
        btnExit.Click += btnExit_Click;

        grpOtherWay.Controls.Add(lblHotkey);
        grpOtherWay.Controls.Add(btnExit);

        // 添加控件到窗体
        this.Controls.Add(grpOneWay);
        this.Controls.Add(grpOtherWay);

        // 初始化系统托盘图标
        InitializeTrayIcon();
    }

    private void InitializeTrayIcon()
    {
        // 创建右键菜单
        contextMenu = new ContextMenuStrip();

        // 显示/隐藏菜单项
        ToolStripMenuItem menuShow = new ToolStripMenuItem("Show");
        menuShow.Click += (sender, e) => ShowMainWindow();
        contextMenu.Items.Add(menuShow);

        // 分隔线
        contextMenu.Items.Add(new ToolStripSeparator());

        // 退出菜单项
        ToolStripMenuItem menuExit = new ToolStripMenuItem("Exit");
        menuExit.Click += (sender, e) => ExitApplication();
        contextMenu.Items.Add(menuExit);

        // 创建托盘图标
        notifyIcon = new NotifyIcon(components);
        notifyIcon.Icon = CreateApplicationIcon();
        notifyIcon.Text = "Window On Top";
        notifyIcon.ContextMenuStrip = contextMenu;
        notifyIcon.Visible = true;

        // 双击托盘图标显示窗口
        notifyIcon.DoubleClick += (sender, e) => ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
    }

    private void ExitApplication()
    {
        notifyIcon.Visible = false;
        Application.Exit();
    }

    private void DrawHandIcon(Graphics g, Rectangle rect)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // 绘制手型（使用简单的图形组合）
        using (Pen pen = new Pen(Color.Black, 2))
        using (Brush brush = new SolidBrush(Color.White))
        {
            int cx = rect.Width / 2;
            int cy = rect.Height / 2;

            // 手掌
            g.FillEllipse(brush, cx - 6, cy - 2, 12, 14);
            g.DrawEllipse(pen, cx - 6, cy - 2, 12, 14);

            // 食指
            g.FillRectangle(brush, cx - 2, cy - 14, 4, 14);
            g.DrawRectangle(pen, cx - 2, cy - 14, 4, 14);

            // 中指
            g.FillRectangle(brush, cx + 3, cy - 12, 4, 12);
            g.DrawRectangle(pen, cx + 3, cy - 12, 4, 12);

            // 无名指
            g.FillRectangle(brush, cx + 8, cy - 10, 4, 10);
            g.DrawRectangle(pen, cx + 8, cy - 10, 4, 10);

            // 小指
            g.FillRectangle(brush, cx - 7, cy - 8, 4, 10);
            g.DrawRectangle(pen, cx - 7, cy - 8, 4, 10);

            // 拇指
            g.FillEllipse(brush, cx - 12, cy + 2, 8, 6);
            g.DrawEllipse(pen, cx - 12, cy + 2, 8, 6);
        }
    }

    private Icon CreateApplicationIcon()
    {
        // 创建 32x32 的位图作为图标
        using (Bitmap bmp = new Bitmap(32, 32))
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            // 绘制置顶符号（一个窗口上面有一个钉子/箭头）
            using (Pen pen = new Pen(Color.DodgerBlue, 2))
            using (Brush brush = new SolidBrush(Color.DodgerBlue))
            using (Brush lightBrush = new SolidBrush(Color.LightSkyBlue))
            {
                int cx = 16;
                int cy = 16;

                // 绘制窗口轮廓
                g.FillRectangle(lightBrush, cx - 10, cy - 6, 20, 14);
                g.DrawRectangle(pen, cx - 10, cy - 6, 20, 14);

                // 绘制窗口标题栏
                using (Brush titleBrush = new SolidBrush(Color.DodgerBlue))
                {
                    g.FillRectangle(titleBrush, cx - 10, cy - 6, 20, 5);
                }

                // 绘制置顶箭头/钉子
                using (Brush pinBrush = new SolidBrush(Color.Red))
                {
                    // 箭头主体
                    Point[] arrow = new Point[]
                    {
                        new Point(cx, cy - 14),
                        new Point(cx - 4, cy - 6),
                        new Point(cx - 2, cy - 6),
                        new Point(cx - 2, cy - 2),
                        new Point(cx + 2, cy - 2),
                        new Point(cx + 2, cy - 6),
                        new Point(cx + 4, cy - 6)
                    };
                    g.FillPolygon(pinBrush, arrow);
                }
            }

            // 转换为图标
            IntPtr hIcon = bmp.GetHicon();
            Icon icon = Icon.FromHandle(hIcon);
            return icon;
        }
    }

    #endregion
}
