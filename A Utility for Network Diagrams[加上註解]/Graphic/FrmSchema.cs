using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;

namespace Graphic
{
    public partial class FrmSchema : Form//主表單
    {
        public FrmSchema()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);//表單繪圖不閃爍_step01
        }

        private const int Nmax = 50;//容器的最大數量
        public GScenario GNetwork;//元件紀錄容器
        private const int XTextPixelOffset = 0;
        private const int YTextPixelOffset = 80;//元件文字敘述的Y軸距離
        private const int XManageFormPixelOffset = 10;//管理表單像素偏移量
        private const int YManageFormPixelOffset = 10;//管理表單像素偏移量
        private int CurrObjDragIndx = 0;
        private int Xdown = 0;
        private int Ydown = 0;
        private DateTime Tdown;
        private int DragTimeMin = 300; // milliseconds
        private bool Dragging = false;
 
        private void Form1_Load(object sender, EventArgs e)//程式進入點
        {
            GNetwork = new GScenario(Nmax);//建立元件紀錄容器
            GNetwork.Clear();//清空元件紀錄容器
            GNetwork.CurrObjIndx = 0;//元件紀錄容器
        }
        private void AddText(int Xbase, int Ybase, string Msg, bool UseOffset)//元件的名稱文字新增函數(此範例是直接在表單上建立畫布並直接顯示)
        {
            Graphics g = this.CreateGraphics();//直接是在表單建立畫布
            Font CurrFont = new Font("Arial", 8);
            int x = 0;
            int y = 0;
            if (UseOffset == true)
            {
                x = Xbase + XTextPixelOffset;
                y = Ybase + YTextPixelOffset;
            }
            else
            {
                x = Xbase;
                y = Ybase;
            }
            g.DrawString(Msg, CurrFont, new SolidBrush(Color.Black), x, y);
        }   
        private void AddText(Graphics g1,int Xbase, int Ybase, string Msg, bool UseOffset)//元件的名稱文字新增函數(此範例是直接在表單上建立畫布並直接顯示)
        {
            Graphics g = g1;//= this.CreateGraphics();//直接是在表單建立畫布
            Font CurrFont = new Font("Arial", 8);
            int x = 0;
            int y = 0;
            if (UseOffset==true)
            {
                x = Xbase + XTextPixelOffset;
                y = Ybase + YTextPixelOffset;
            }
            else
            {
                x = Xbase;
                y = Ybase;
            }
            g.DrawString(Msg, CurrFont, new SolidBrush(Color.Black), x, y);
        }

        public void AddGObject(int x1, int y1, int x2, int y2, string ObjType)//增加元件
        {
            Graphics g = this.CreateGraphics();//直接是在表單建立畫布
            Rectangle ObjRct = new Rectangle();//建立一個矩形變數
            Pen p = new Pen(Color.Blue);//建立藍色畫筆
            Image ObjImg;//建立圖片元件
            string ObjName = ObjType + "_" + GNetwork.LastIndexOfGObject(ObjType).ToString();//建立預設元件名稱
            //
            if (ObjType == "Line")//如果類型是線段
            {
                g.DrawLine(p, x1, y1, x2, y2);//把線段畫出來(藍色)
                int xm = (x1 + x2) / 2;
                int ym = (y1 + y2) / 2;
                AddText(xm, ym, ObjName, false);//新增文字
            }
            else
            {
                ObjImg = FindGObjectTypeImage(ObjType);//載入對應圖片
                ObjRct.X = x1;//設定矩形變數大小
                ObjRct.Y = y1;//設定矩形變數大小
                ObjRct.Height = ObjImg.Height;//設定矩形變數大小
                ObjRct.Width = ObjImg.Width;//設定矩形變數大小
                g.DrawImage(ObjImg, ObjRct);//匯出徒刑
                AddText(x1, y1, ObjName, true);//新增文字
                x2 = x1 + ObjRct.Width;//按照圖片實際大小修正參數
                y2 = y1 + ObjRct.Height;//按照圖片實際大小修正參數
            }
            //
            GNetwork.AddGObject(ObjName, ObjType, x1, y1, x2, y2);//新增元件
        }

        private Image FindGObjectTypeImage(string ObjType)//載入對應圖片函數
        {
            Image RetImg = null;
            switch (ObjType)
            {
                case "Network":
                    RetImg = imageList1.Images[0];
                    break;
                case "Router" :
                    RetImg = imageList1.Images[1];
                    break;
                case "Emitter":
                    RetImg = imageList1.Images[2];
                    break;
                case "Receiver":
                    RetImg = imageList1.Images[3];
                    break;
            }
            return RetImg;
        }

        private void ReDrawAll(PaintEventArgs e)//重會所有元件
        {
            //--
            //表單繪圖不閃爍_step02
            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
            BufferedGraphics myBuffer = currentContext.Allocate(e.Graphics, e.ClipRectangle);
            Graphics g = myBuffer.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.Clear(this.BackColor);
            //--

            //Graphics g = this.CreateGraphics();//直接是在表單建立畫布
            GObject CurrObj = new GObject();//建立元件
            Rectangle Rct = new Rectangle();//建立矩形元件
            Pen p = new Pen(Color.Blue);//建立畫筆
            Image ObjImg;
            int xm = 0;
            int ym = 0;
            string IsLine = "";
            for (int i=0; i < GNetwork.Nobj;i++ )//依序取出元件
            {
                CurrObj = GNetwork.GObjects[i];
                //
                if (CurrObj.Type == "") IsLine = "N/D";
                if (CurrObj.Type == "Line") IsLine = "Y";
                if ((CurrObj.Type != "Line") && (CurrObj.Type != "")) IsLine = "N";
                //
                switch (IsLine)
                {
                    case "Y":
                        g.DrawLine(p, CurrObj.x1, CurrObj.y1, CurrObj.x2, CurrObj.y2);
                        xm = (CurrObj.x1+CurrObj.x2)/2;
                        ym = (CurrObj.y1 + CurrObj.y2) / 2;
                        AddText(g,xm, ym, CurrObj.Name,false);
                        break;
                    case "N":
                        Rct.X = CurrObj.x1;
                        Rct.Y = CurrObj.y1;
                        Rct.Width = CurrObj.x2 - CurrObj.x1;
                        Rct.Height = CurrObj.y2 - CurrObj.y1;
                        if (CurrObj.Type != String.Empty)
                        {
                            ObjImg = FindGObjectTypeImage(CurrObj.Type);
                            g.DrawImage(ObjImg, Rct);
                            AddText(g,CurrObj.x1, CurrObj.y1, CurrObj.Name, true);
                            GNetwork.AdjustLinkedTo(CurrObj.Name);
                        }
                        break;
                } 
            }

            //--
            //表單繪圖不閃爍_step03
            myBuffer.Render(e.Graphics);
            g.Dispose();
            myBuffer.Dispose();//释放资源
            //--
        }

        private void Form1_Paint(object sender, PaintEventArgs e)//防止C#,在FORM畫完圖後,將視窗縮小再放大,圖就消失
        {
            ReDrawAll(e);
        }

        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int Xclicked = 0;
            int Yclicked = 0;
            Dragging = false;
            Xclicked = e.X;
            Yclicked = e.Y;
            GObject GContainer = new GObject();
            GObject GModified = new GObject();
            int Container = GNetwork.FindContainerObject(Xclicked, Yclicked, ref GContainer, false);
            if (Container>-1)
            {
                GModified = GContainer;
                FrmManageObject Manage = new FrmManageObject();
                Manage.GObjName = GContainer.Name;
                Manage.GObjType = GContainer.Type;
                Point IniPoint = new Point(Xclicked + XManageFormPixelOffset, Yclicked + YManageFormPixelOffset);
                Manage.StartPosition = FormStartPosition.Manual;
                Manage.Location = IniPoint;
                Manage.ShowDialog();
                switch (Manage.OperationToDo)
                {
                    case "Modify":
                        //
                        //      Load New Data from the Manage Form
                        //
                        GModified.Name = Manage.GObjName;
                        GNetwork.ModifyGObject(GContainer, GModified);
                        break;
                    case "Delete":
                        //
                        //      Delete the object with the original name
                        //      not with then eventually modified name!
                        //
                        GNetwork.DeleteGObject(GContainer);
                        break;
                }
                this.Invalidate();
            }
            else
            {
                //
                //    nothing to do
                //
            }
        }

        private void routerToolStripMenuItem_Click(object sender, EventArgs e)//增加Router元件
        {
            int X = 0;
            int Y = 25;
            AddGObject(X, Y, 0, 0, "Router");
        }

        private void linkToolStripMenuItem_Click(object sender, EventArgs e)//增加Line元件
        {
            int X1 = 0;
            int Y1 = 0;
            int X2 = 100;
            int Y2 = 100;
            AddGObject(X1, Y1, X2, Y2, "Line");
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)//單純顯示座標到下面顯示區
        {
            string CoordsMsg = "";
            CoordsMsg = "x = " + e.X.ToString() + " : y = " + e.Y.ToString();
            toolStripStatusLabel1.Text = CoordsMsg;
            MouseObject(e);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)//判斷有無選到元件，並做變數變動
        {
            Xdown = e.X;
            Ydown = e.Y;
            Tdown = DateTime.Now;
            GObject GContainer = new GObject();
            int Container = GNetwork.FindContainerObject(Xdown, Ydown, ref GContainer,false);//判斷是否有選到元件
            if (Container > -1)
            {
                Dragging = true;//拖曳成立
                Cursor.Current = Cursors.Hand;//變換滑鼠
                CurrObjDragIndx = Container;//紀錄目前元件的INDEX到全域變數
            }
            else
            {
                // Click out of all objects
            }
        }
        public void MouseObject(MouseEventArgs e)
        {
            int H = 0;
            int W = 0;
            GObject GContainer = new GObject();
            GObject GToDrag = new GObject();
            GToDrag = GNetwork.GObjects[CurrObjDragIndx];//取出該元件實體
            double d1 = 0;
            double d2 = 0;
            TimeSpan DTDrag = new TimeSpan();
            DTDrag = DateTime.Now.Subtract(Tdown);//計算時間間隔
            if ((Dragging == true) && (DTDrag.Milliseconds > DragTimeMin))
            {
                if ((GNetwork.GObjects[CurrObjDragIndx].Type == "Line")
                   && (GNetwork.FindContainerObject(e.X, e.Y, ref GContainer, true) > -1))
                {
                    //
                    //    What is the point of the line to link ? 
                    //    The nearest to (Xdown,Ydown)
                    //
                    d1 = CommFnc.distance(Xdown, Ydown, GToDrag.x1, GToDrag.y1);
                    d2 = CommFnc.distance(Xdown, Ydown, GToDrag.x2, GToDrag.y2);
                    if (d1 <= d2)
                    {
                        GToDrag.x1 = (GContainer.x1 + GContainer.x2) / 2;
                        GToDrag.y1 = (GContainer.y1 + GContainer.y2) / 2;
                        GToDrag.Lnk1 = GContainer.Name;
                    }
                    else
                    {
                        GToDrag.x2 = (GContainer.x1 + GContainer.x2) / 2;
                        GToDrag.y2 = (GContainer.y1 + GContainer.y2) / 2;
                        GToDrag.Lnk2 = GContainer.Name;
                    }
                }
                else
                {
                    W = GToDrag.x2 - GToDrag.x1;//移動元件的位置計算
                    H = GToDrag.y2 - GToDrag.y1;//移動元件的位置計算
                    GToDrag.x1 = e.X;//移動元件的位置計算
                    GToDrag.y1 = e.Y;//移動元件的位置計算
                    GToDrag.x2 = e.X + W;//移動元件的位置計算
                    GToDrag.y2 = e.Y + H;//移動元件的位置計算
                    GNetwork.AdjustLinkedTo(GToDrag.Name);
                }

                this.Refresh();//觸發重繪
            }
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)//
        {
            MouseObject(e);
            Cursor.Current = Cursors.Default;//換回滑鼠游標
            Dragging = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)//關閉程式
        {
            Application.Exit();
        }

        private void emitterToolStripMenuItem_Click(object sender, EventArgs e)//增加Emitter元件
        {
            int X = 0;
            int Y = 25;
            AddGObject(X, Y, 0, 0, "Emitter");
        }

        private void receiverToolStripMenuItem_Click(object sender, EventArgs e)//增加Receiver元件
        {
            int X = 25;//0;
            int Y = 25;
            AddGObject(X, Y, 0, 0, "Receiver");
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)//載入檔案
        {
            string LoadFileName = "";
            string ErrLoading = "";
            LoadFileName = CommFnc.FindFileToOpen();
            GNetwork.Clear();
            if (GNetwork.LoadFile(LoadFileName, ref ErrLoading) == true)
            {
                toolStripStatusLabel1.Text = "File loaded in memory.";
                this.Refresh();//觸發重繪 //作者忘了寫(可能是故意的)
            }
            else
            {
                toolStripStatusLabel1.Text = ErrLoading;
            }
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)//儲存檔案
        {
            string SaveFileName = "";
            string ErrSaving = "";
            SaveFileName = CommFnc.AssignFileToSave();
            if (GNetwork.SaveFile(SaveFileName, ref ErrSaving) == true)
            {
                toolStripStatusLabel1.Text = "File saved.";
            }
            else
            {
                toolStripStatusLabel1.Text = ErrSaving;
            }
        }

    }
}