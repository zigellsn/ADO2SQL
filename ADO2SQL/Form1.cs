using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ADO2SQL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox2.Text) && !String.IsNullOrEmpty(textBox3.Text))
            {
                DataSet ds = new DataSet();
                ds.ReadXmlSchema(textBox2.Text);
                ds.ReadXml(textBox3.Text);
                string export = String.Empty;
                if (rbSQLite.Checked == true)
                {
                    export = SQLiteExport.GetCreateSQLDb(ds);
                    textBox1.Text = export;
                }
                else
                    MessageBox.Show("Noch nicht implementiert");
            }
            else
                MessageBox.Show("Bitte ein XML-Schema UND eine XML-Datenbank angeben");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox2.Text) && !String.IsNullOrEmpty(textBox3.Text)
                && !String.IsNullOrEmpty(textBox1.Text))
            {
                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (FileStream fs = File.Create(saveFileDialog1.FileName))
                    {
                        AddText(fs, textBox1.Text);
                    }

                }
            }
            else
                MessageBox.Show("Bitte erst einen Export erzeugen");

        }

        private static void AddText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox3.Text = openFileDialog2.FileName;
            }
        }
    }
}
