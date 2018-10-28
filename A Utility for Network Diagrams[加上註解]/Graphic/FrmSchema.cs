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
    public partial class FrmSchema : Form//�����
    {
        public FrmSchema()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);//����L�D���W�q_step01
        }

        private const int Nmax = 50;//�����������
        public GScenario GNetwork;//Ԫ���o�����
        private const int XTextPixelOffset = 0;
        private const int YTextPixelOffset = 80;//Ԫ�����֔�����Y�S���x
        private const int XManageFormPixelOffset = 10;//����������ƫ����
        private const int YManageFormPixelOffset = 10;//����������ƫ����
        private int CurrObjDragIndx = 0;
        private int Xdown = 0;
        private int Ydown = 0;
        private DateTime Tdown;
        private int DragTimeMin = 300; // milliseconds
        private bool Dragging = false;
 
        private void Form1_Load(object sender, EventArgs e)//��ʽ�M���c
        {
            GNetwork = new GScenario(Nmax);//����Ԫ���o�����
            GNetwork.Clear();//���Ԫ���o�����
            GNetwork.CurrObjIndx = 0;//Ԫ���o�����
        }
        private void AddText(int Xbase, int Ybase, string Msg, bool UseOffset)//Ԫ�������Q������������(�˹�����ֱ���ڱ���Ͻ��������Kֱ���@ʾ)
        {
            Graphics g = this.CreateGraphics();//ֱ�����ڱ�ν�������
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
        private void AddText(Graphics g1,int Xbase, int Ybase, string Msg, bool UseOffset)//Ԫ�������Q������������(�˹�����ֱ���ڱ���Ͻ��������Kֱ���@ʾ)
        {
            Graphics g = g1;//= this.CreateGraphics();//ֱ�����ڱ�ν�������
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

        public void AddGObject(int x1, int y1, int x2, int y2, string ObjType)//����Ԫ��
        {
            Graphics g = this.CreateGraphics();//ֱ�����ڱ�ν�������
            Rectangle ObjRct = new Rectangle();//����һ������׃��
            Pen p = new Pen(Color.Blue);//�����{ɫ���P
            Image ObjImg;//�����DƬԪ��
            string ObjName = ObjType + "_" + GNetwork.LastIndexOfGObject(ObjType).ToString();//�����A�OԪ�����Q
            //
            if (ObjType == "Line")//�������Ǿ���
            {
                g.DrawLine(p, x1, y1, x2, y2);//�Ѿ��ή�����(�{ɫ)
                int xm = (x1 + x2) / 2;
                int ym = (y1 + y2) / 2;
                AddText(xm, ym, ObjName, false);//��������
            }
            else
            {
                ObjImg = FindGObjectTypeImage(ObjType);//�d�댦���DƬ
                ObjRct.X = x1;//�O������׃����С
                ObjRct.Y = y1;//�O������׃����С
                ObjRct.Height = ObjImg.Height;//�O������׃����С
                ObjRct.Width = ObjImg.Width;//�O������׃����С
                g.DrawImage(ObjImg, ObjRct);//�R��ͽ��
                AddText(x1, y1, ObjName, true);//��������
                x2 = x1 + ObjRct.Width;//���ՈDƬ���H��С��������
                y2 = y1 + ObjRct.Height;//���ՈDƬ���H��С��������
            }
            //
            GNetwork.AddGObject(ObjName, ObjType, x1, y1, x2, y2);//����Ԫ��
        }

        private Image FindGObjectTypeImage(string ObjType)//�d�댦���DƬ����
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

        private void ReDrawAll(PaintEventArgs e)//�ؕ�����Ԫ��
        {
            //--
            //����L�D���W�q_step02
            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
            BufferedGraphics myBuffer = currentContext.Allocate(e.Graphics, e.ClipRectangle);
            Graphics g = myBuffer.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.Clear(this.BackColor);
            //--

            //Graphics g = this.CreateGraphics();//ֱ�����ڱ�ν�������
            GObject CurrObj = new GObject();//����Ԫ��
            Rectangle Rct = new Rectangle();//��������Ԫ��
            Pen p = new Pen(Color.Blue);//�������P
            Image ObjImg;
            int xm = 0;
            int ym = 0;
            string IsLine = "";
            for (int i=0; i < GNetwork.Nobj;i++ )//����ȡ��Ԫ��
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
            //����L�D���W�q_step03
            myBuffer.Render(e.Graphics);
            g.Dispose();
            myBuffer.Dispose();//�ͷ���Դ
            //--
        }

        private void Form1_Paint(object sender, PaintEventArgs e)//��ֹC#,��FORM����D��,��ҕ���sС�ٷŴ�,�D����ʧ
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

        private void routerToolStripMenuItem_Click(object sender, EventArgs e)//����RouterԪ��
        {
            int X = 0;
            int Y = 25;
            AddGObject(X, Y, 0, 0, "Router");
        }

        private void linkToolStripMenuItem_Click(object sender, EventArgs e)//����LineԪ��
        {
            int X1 = 0;
            int Y1 = 0;
            int X2 = 100;
            int Y2 = 100;
            AddGObject(X1, Y1, X2, Y2, "Line");
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)//�μ��@ʾ���˵������@ʾ�^
        {
            string CoordsMsg = "";
            CoordsMsg = "x = " + e.X.ToString() + " : y = " + e.Y.ToString();
            toolStripStatusLabel1.Text = CoordsMsg;
            MouseObject(e);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)//�Д��Пo�x��Ԫ�����K��׃��׃��
        {
            Xdown = e.X;
            Ydown = e.Y;
            Tdown = DateTime.Now;
            GObject GContainer = new GObject();
            int Container = GNetwork.FindContainerObject(Xdown, Ydown, ref GContainer,false);//�Д��Ƿ����x��Ԫ��
            if (Container > -1)
            {
                Dragging = true;//��ҷ����
                Cursor.Current = Cursors.Hand;//׃�Q����
                CurrObjDragIndx = Container;//�o�ĿǰԪ����INDEX��ȫ��׃��
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
            GToDrag = GNetwork.GObjects[CurrObjDragIndx];//ȡ��ԓԪ�����w
            double d1 = 0;
            double d2 = 0;
            TimeSpan DTDrag = new TimeSpan();
            DTDrag = DateTime.Now.Subtract(Tdown);//Ӌ��r�g�g��
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
                    W = GToDrag.x2 - GToDrag.x1;//�Ƅ�Ԫ����λ��Ӌ��
                    H = GToDrag.y2 - GToDrag.y1;//�Ƅ�Ԫ����λ��Ӌ��
                    GToDrag.x1 = e.X;//�Ƅ�Ԫ����λ��Ӌ��
                    GToDrag.y1 = e.Y;//�Ƅ�Ԫ����λ��Ӌ��
                    GToDrag.x2 = e.X + W;//�Ƅ�Ԫ����λ��Ӌ��
                    GToDrag.y2 = e.Y + H;//�Ƅ�Ԫ����λ��Ӌ��
                    GNetwork.AdjustLinkedTo(GToDrag.Name);
                }

                this.Refresh();//�|�l���L
            }
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)//
        {
            MouseObject(e);
            Cursor.Current = Cursors.Default;//�Q�ػ����Θ�
            Dragging = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)//�P�]��ʽ
        {
            Application.Exit();
        }

        private void emitterToolStripMenuItem_Click(object sender, EventArgs e)//����EmitterԪ��
        {
            int X = 0;
            int Y = 25;
            AddGObject(X, Y, 0, 0, "Emitter");
        }

        private void receiverToolStripMenuItem_Click(object sender, EventArgs e)//����ReceiverԪ��
        {
            int X = 25;//0;
            int Y = 25;
            AddGObject(X, Y, 0, 0, "Receiver");
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)//�d��n��
        {
            string LoadFileName = "";
            string ErrLoading = "";
            LoadFileName = CommFnc.FindFileToOpen();
            GNetwork.Clear();
            if (GNetwork.LoadFile(LoadFileName, ref ErrLoading) == true)
            {
                toolStripStatusLabel1.Text = "File loaded in memory.";
                this.Refresh();//�|�l���L //�������ˌ�(�����ǹ����)
            }
            else
            {
                toolStripStatusLabel1.Text = ErrLoading;
            }
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)//����n��
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