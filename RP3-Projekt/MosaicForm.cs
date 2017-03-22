using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RP3_Projekt
{
    public partial class MosaicFrom : Form
    {
        private List<Bitmap> imageList = new List<Bitmap>();
        private List<string> nameList = new List<String>();
        private Bitmap _image;
        private string openedFileName;
        private bool isSaved = true;
        private Int32 nRows=2, nColls=2;

        public MosaicFrom()
        {
            InitializeComponent();
            _image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            splitContainer1.Panel1.Controls.Add(checkedListBox1);
            splitContainer1.Panel2.Controls.Add(pictureBox1);
            splitContainer1.SplitterDistance = 100;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            //add or remove image to picturebox
            nameList.Clear();
            foreach (string s in checkedListBox1.CheckedItems)
            {
                //Console.WriteLine(s);
                nameList.Add(s);
            }

            //event triggers before the item is checked
            if (e.NewValue == CheckState.Checked)
            {
                nameList.Add(checkedListBox1.Items[e.Index].ToString());
            }
            else
            {
                nameList.Remove(checkedListBox1.Items[e.Index].ToString());
            }
            drawImages();
            isSaved = false;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var d = new NewFileDialog())
            {
                d.ShowDialog();
                if (d.DialogResult == DialogResult.OK)
                {
                    pictureBox1.Width = d.ImgWidth;
                    pictureBox1.Height = d.ImgHeight;
                    _image = new Bitmap(d.ImgWidth, d.ImgHeight);
                    pictureBox1.Image = _image;
                    pictureBox1.DrawToBitmap(
                        (Bitmap)pictureBox1.Image,
                        new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
                    pictureBox1.Visible = true;
                    openedFileName = null;
                    isSaved = false;
                    checkedListBox1.Items.Clear();
                    //drawImages();
                }
            }
        }

        private void openFile()
        {
            using (var d = new OpenFileDialog())
            {
                d.Multiselect = true;
                d.Filter = "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|" +
                           "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff";
                var result = d.ShowDialog();
                if (result == DialogResult.OK)
                {
                    foreach (string fileName in d.FileNames)
                    {
                        try
                        {
                            checkedListBox1.Items.Add(fileName);
                            //nameList.Add(fileName);

                            drawImages();
                        }
                        catch (IOException) { }
                    }
                }
            }
        }

        private void drawImages()
        {
            using (Graphics graphi = Graphics.FromImage(_image))
                {
                graphi.Clear(Color.White);
                //pomonce variable
                int i = 0, j=0;
                int imgWidth=_image.Width/nRows, imgHeight=_image.Height/nColls;

                foreach (string fileName in nameList)
                {
                    var bmp = new Bitmap(fileName);
                    graphi.DrawImage(bmp, i*imgWidth, j*imgHeight, imgWidth, imgHeight);
                    if (++i >= nRows)
                    {
                        i = 0;
                        if (++j >= nColls) break;
                    }
                }
                //pictureBox1.Image = _image;
                //pictureBox1.Visible = true;
            }
        }

        private void saveFile()
        {
            if (openedFileName == null)
            {
                saveAsFile();
            }
            else
            {
                try
                {
                    pictureBox1.Image.
                        Save(openedFileName, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                catch (IOException) { }
                isSaved = true;
            }
        }
        private void saveAsFile()
        {
            using (var d = new SaveFileDialog())
            {
                d.Filter = "BMP|*.bmp";
                var result = d.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string filename = d.FileName;
                    try
                    {
                        pictureBox1.Image.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                    catch (IOException) { }
                    openedFileName = filename;
                    isSaved = true;
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isSaved)
            {
                switch (MessageBox.Show(
                    "Save changes?", "Save?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation))
                {
                    case DialogResult.Yes:
                        saveFile();
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void patternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var d = new Pattern())
            {
                d.ShowDialog();
                if (d.DialogResult == DialogResult.OK)
                {
                    nRows = d.nRows;
                    nColls = d.nColls;
                    drawImages();
                    isSaved = false;
                }
            }
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            checkedListBox1.Width = splitContainer1.SplitterDistance;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFile();
        }
    }
}
