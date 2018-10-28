using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Graphic
{
    public partial class FrmSchema : Form
    {
        public FrmSchema()
        {
            InitializeComponent();
        }

        private const int Nmax = 50;
        public GScenario GNetwork;
        private const int XTextPixelOffset = 0;
        private const int YTextPixelOffset = 80;
        private const int XManageFormPixelOffset = 10;
        private const int YManageFormPixelOffset = 10;
        private int CurrObjDragIndx = 0;
        private int Xdown = 0;
        private int Ydown = 0;
        private DateTime Tdown;
        private int DragTimeMin = 300; // milliseconds
        private bool Dragging = false;
 
        private void Form1_Load(object sender, EventArgs e)
        {
            GNetwork = new GScenario(Nmax);
            GNetwork.Clear();
            GNetwork.CurrObjIndx = 0;
        }
   
        private void AddText(int Xbase, int Ybase, string Msg, bool UseOffset)
        {
            Graphics g = this.CreateGraphics();
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

        public void AddGObject(int x1, int y1, int x2, int y2, string ObjType)
        {
            Graphics g = this.CreateGraphics();
            Rectangle ObjRct = new Rectangle();
            Pen p = new Pen(Color.Blue);
            Image ObjImg;
            string ObjName = ObjType + "_" + GNetwork.LastIndexOfGObject(ObjType).ToString();
            //
            if (ObjType == "Line")
            {
                g.DrawLine(p, x1, y1, x2, y2);
                int xm = (x1 + x2) / 2;
                int ym = (y1 + y2) / 2;
                AddText(xm, ym, ObjName, false);
            }
            else
            {
                ObjImg = FindGObjectTypeImage(ObjType);
                ObjRct.X = x1;
                ObjRct.Y = y1;
                ObjRct.Height = ObjImg.Height;
                ObjRct.Width = ObjImg.Width;
                g.DrawImage(ObjImg, ObjRct);
                AddText(x1, y1, ObjName, true);
                x2 = x1 + ObjRct.Width;
                y2 = y1 + ObjRct.Height;
            }
            //
            GNetwork.AddGObject(ObjName, ObjType, x1, y1, x2, y2);
        }

        private Image FindGObjectTypeImage(string ObjType)
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

        private void ReDrawAll()
        {
            Graphics g = this.CreateGraphics();
            GObject CurrObj = new GObject();
            Rectangle Rct = new Rectangle();
            Pen p = new Pen(Color.Blue);
            Image ObjImg;
            int xm = 0;
            int ym = 0;
            string IsLine = "";
            for (int i=0; i < GNetwork.Nobj;i++ )
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
                        AddText(xm, ym, CurrObj.Name,false);
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
                            AddText(CurrObj.x1, CurrObj.y1, CurrObj.Name,true);
                            GNetwork.AdjustLinkedTo(CurrObj.Name);
                        } 
                        break;
                } 
            }   
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            ReDrawAll();
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

        private void routerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int X = 0;
            int Y = 25;
            AddGObject(X, Y, 0, 0, "Router");
        }

        private void linkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int X1 = 0;
            int Y1 = 0;
            int X2 = 100;
            int Y2 = 100;
            AddGObject(X1, Y1, X2, Y2, "Line");
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            string CoordsMsg = "";
            CoordsMsg = "x = " + e.X.ToString() + " : y = " + e.Y.ToString();
            toolStripStatusLabel1.Text = CoordsMsg;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            Xdown = e.X;
            Ydown = e.Y;
            Tdown = DateTime.Now;
            GObject GContainer = new GObject();
            int Container = GNetwork.FindContainerObject(Xdown, Ydown, ref GContainer,false);
            if (Container > -1)
            {
                Dragging = true;
                Cursor.Current = Cursors.Hand;
                CurrObjDragIndx = Container;
            }
            else
            {
                // Click out of all objects
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            int H = 0;
            int W = 0;
            GObject GContainer = new GObject();
            GObject GToDrag = new GObject();
            GToDrag = GNetwork.GObjects[CurrObjDragIndx];
            double d1 = 0;
            double d2 = 0;
            TimeSpan DTDrag = new TimeSpan();
            DTDrag = DateTime.Now.Subtract(Tdown);
            if ((Dragging==true) && (DTDrag.Milliseconds>DragTimeMin))
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
                    if (d1<=d2)
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
                    W = GToDrag.x2 - GToDrag.x1;
                    H = GToDrag.y2 - GToDrag.y1;
                    GToDrag.x1 = e.X;
                    GToDrag.y1 = e.Y;
                    GToDrag.x2 = e.X + W;
                    GToDrag.y2 = e.Y + H;
                    GNetwork.AdjustLinkedTo(GToDrag.Name);
                    }
                Cursor.Current = Cursors.Default;
                Dragging = false;
                this.Refresh();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void emitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int X = 0;
            int Y = 25;
            AddGObject(X, Y, 0, 0, "Emitter");
        }

        private void receiverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int X = 0;
            int Y = 25;
            AddGObject(X, Y, 0, 0, "Receiver");
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string LoadFileName = "";
            string ErrLoading = "";
            LoadFileName = CommFnc.FindFileToOpen();
            GNetwork.Clear();
            if (GNetwork.LoadFile(LoadFileName, ref ErrLoading) == true)
            {
                toolStripStatusLabel1.Text = "File loaded in memory.";
            }
            else
            {
                toolStripStatusLabel1.Text = ErrLoading;
            }
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
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