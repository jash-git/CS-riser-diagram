using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Graphic
{
    public partial class FrmManageObject : Form
    {
        public string GObjName;
        public string GObjType;
        public string OperationToDo = "";

        public FrmManageObject()
        {
            InitializeComponent();
        }

        private void FrmManageObject_Load(object sender, EventArgs e)
        {
            textBox1.Text = GObjName;
            textBox2.Text = GObjType;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //
            //   Valutate if something has modified
            //
            bool SomethingHasModified = false;
            SomethingHasModified = (textBox1.Text != GObjName);
            if (SomethingHasModified==true)
            {
                OperationToDo = "Modify";
            }
            //
            //   Load eventually modified Fields into public variables
            //
            GObjName = textBox1.Text;
            //
            //   Exit
            //
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure ?","Delete Confimation",MessageBoxButtons.YesNo,MessageBoxIcon.Question)
                == DialogResult.Yes)
                {
                OperationToDo = "Delete";
                }
            //
            //   Exit
            //
            this.Close();
        }

    }
}