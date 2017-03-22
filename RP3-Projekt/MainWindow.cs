using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace RP3_Projekt
{
    enum Tools { RectangleSelect, Pencil, Brush, Line, Circle, Ellipse, Rectangle }
    enum BrushType { None, SolidBrush , HatchBrush }

    //[ImplementPropertyChanged]
    public partial class MainWindow : Form//, INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;
        private string openedFileName;
        private bool isSaved = true;

        private bool dragging = false;
        private bool mouseDown = false;
        MouseButtons mouseButtonPressed;

        List<Point> currentStroke = new List<Point>();

        UndoRedo history;

        private Tools selectedTool = Tools.RectangleSelect;
        private Color primaryColor = Color.Black;
        private Color secondaryColor = Color.White;
        private Color drawingColor
        {
            get { return mouseButtonPressed == MouseButtons.Left ? primaryColor : secondaryColor; }
        }
        private Brush selectedBrush
        {
            get
            {
                switch (fill)
                {
                    case BrushType.None:
                        return new SolidBrush(Color.Transparent);
                    case BrushType.SolidBrush:
                        return new SolidBrush(drawingColor);
                    default:
                        return new HatchBrush(HatchStyle.Cross, drawingColor, Color.Transparent);
                }
            }
        }
        private BrushType fill;
        private DashStyle line = DashStyle.Solid;
        private Rectangle selectedArea;
        private Rectangle pastedArea;
        private ColorMatrix colorm;
        private float width;



        public MainWindow()
        {
            InitializeComponent();
            history = UndoRedo.getInstance(pictureBox1);
            //ui init
            widthComboBox.SelectedIndex = 2;
            fillComboBox.SelectedIndex = 0;
            lineComboBox.SelectedIndex = 0;         
        }

        #region Actions
        private void newFile()
        {
            using (var d = new NewFileDialog())
            {
                d.ShowDialog();
                if (d.DialogResult == DialogResult.OK)
                {
                    pictureBox1.Width = d.ImgWidth;
                    pictureBox1.Height = d.ImgHeight;
                    pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    pictureBox1.DrawToBitmap(
                        (Bitmap)pictureBox1.Image,
                        new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
                    pictureBox1.Visible = true;
                    openedFileName = null;
                    isSaved = false;
                }
            }
            
            
        }
        private void openFile()
        {
            using (var d = new OpenFileDialog())
            {
                d.Filter = "All Graphics Types|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|"+
                           "BMP|*.bmp|GIF|*.gif|JPG|*.jpg;*.jpeg|PNG|*.png|TIFF|*.tif;*.tiff";
                var result = d.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string fileName = d.FileName;
                    try
                    {
                        var bmp = new Bitmap(fileName);
                        pictureBox1.Width = bmp.Width;
                        pictureBox1.Height = bmp.Height;
                        pictureBox1.Image = bmp;
                        pictureBox1.Visible = true;
                        fileName = openedFileName;
                    }
                    catch (IOException) { }
                }
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
        private void cropImage()
        {
            if (!selectedArea.IsEmpty)
            {
                history.Save();
                currentStroke.Clear();
                Bitmap bmpImage = new Bitmap(pictureBox1.Image);
                pictureBox1.Size = selectedArea.Size;
                pictureBox1.Image = bmpImage.Clone(selectedArea, bmpImage.PixelFormat);
                selectedArea = new Rectangle();
            }
        }
        private void flipRotate(RotateFlipType r)
        {
            history.Save();
            isSaved = false;
            pictureBox1.Image.RotateFlip(r);
            pictureBox1.Height = pictureBox1.Image.Height;
            pictureBox1.Width = pictureBox1.Image.Width;
            pictureBox1.Invalidate();
        }
        #endregion

        #region Action events
        private void newStripButton_Click(object sender, EventArgs e)
        {
            newFile();
        }
        private void openStripButton_Click(object sender, EventArgs e)
        {
            openFile();
        }
        private void saveStripButton_Click(object sender, EventArgs e)
        {
            saveFile();
        }
        private void undoStripButton_Click(object sender, EventArgs e)
        {
            history.UndoAction();
        }
        private void redoStripButton_Click(object sender, EventArgs e)
        {
            history.RedoActon();
        }
        private void cropStripButton_Click(object sender, EventArgs e)
        {
            cropImage();
        }
        private void rotateLeftButton_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
                flipRotate(RotateFlipType.Rotate270FlipNone);
        }
        private void rotateRightButton_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
                flipRotate(RotateFlipType.Rotate90FlipNone);
        }
        private void flipHorizontalButton_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
                flipRotate(RotateFlipType.RotateNoneFlipY);
        }
        private void flipVerticalButton_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
                flipRotate(RotateFlipType.RotateNoneFlipX);
        }
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAsFile();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Tools
        private void drawSelectionRectangle(PaintEventArgs e)
        {
            selectedArea = new Rectangle(
                Math.Min(currentStroke.First().X, currentStroke.Last().X),
                Math.Min(currentStroke.First().Y, currentStroke.Last().Y),
                Math.Abs(currentStroke.First().X - currentStroke.Last().X),
                Math.Abs(currentStroke.First().Y - currentStroke.Last().Y));
            var p = new Pen(Color.DarkGray, 1);
            float[] dashes = { 5, 2 };
            p.DashPattern = dashes;
            e.Graphics.DrawRectangle(p, selectedArea);
        }
        private void drawRectangle(PaintEventArgs e)
        {
            var rec = new Rectangle(
                Math.Min(currentStroke.First().X, currentStroke.Last().X),
                Math.Min(currentStroke.First().Y, currentStroke.Last().Y),
                Math.Abs(currentStroke.First().X - currentStroke.Last().X),
                Math.Abs(currentStroke.First().Y - currentStroke.Last().Y));
            var p = new Pen(drawingColor, width);
            p.DashStyle = line;
            e.Graphics.DrawRectangle(p, rec);
            e.Graphics.FillRectangle(selectedBrush, rec);
        }
        private void drawLine(PaintEventArgs e)
        {
            var p = new Pen(drawingColor, width);
            p.DashStyle = line;
            e.Graphics.DrawLine(
                p,
                currentStroke.First(),
                currentStroke.Last()); 
        }
        private void drawPencil(PaintEventArgs e)
        {
            var p = new Pen(drawingColor, width);
            p.DashStyle = line;
            e.Graphics.DrawLines(
                p,
                currentStroke.ToArray());
        }
        private void drawCircle(PaintEventArgs e)
        {
            int a = Math.Min(
                Math.Abs(currentStroke.First().X - currentStroke.Last().X),
                Math.Abs(currentStroke.First().Y - currentStroke.Last().Y));
            var rec = new Rectangle(
                    Math.Min(currentStroke.First().X, currentStroke.Last().X),
                    Math.Min(currentStroke.First().Y, currentStroke.Last().Y),
                    a, a);
            var p = new Pen(drawingColor, width);
            p.DashStyle = line;
            e.Graphics.DrawEllipse(
                p,
                rec);
            e.Graphics.FillEllipse(selectedBrush, rec);
        }
        private void drawEllipse(PaintEventArgs e)
        {
            var rec = new Rectangle(
                Math.Min(currentStroke.First().X, currentStroke.Last().X),
                Math.Min(currentStroke.First().Y, currentStroke.Last().Y),
                Math.Abs(currentStroke.First().X - currentStroke.Last().X),
                Math.Abs(currentStroke.First().Y - currentStroke.Last().Y));
            var p = new Pen(drawingColor, width);
            p.DashStyle = line;
            e.Graphics.DrawEllipse(
                p,
                rec);
            e.Graphics.FillEllipse(selectedBrush, rec);

        }
        private void drawPastedRect(PaintEventArgs e)
        {
            
            Image clipboard_img = Clipboard.GetImage();
            if (dragging)
            {
                pastedArea = new Rectangle(
                Math.Abs(currentStroke.First().X - currentStroke.Last().X),
                Math.Abs(currentStroke.First().Y - currentStroke.Last().Y),
                clipboard_img.Width,
                clipboard_img.Height);
            }
            e.Graphics.DrawImage(clipboard_img, pastedArea);
        }
        private void applyFilter(PaintEventArgs e)
        {
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
                colorm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            e.Graphics.DrawImage(
                pictureBox1.Image,
                new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height),
                0, 0,
                pictureBox1.Width,
                pictureBox1.Height,
                GraphicsUnit.Pixel,
                imageAttributes);

        }
        #endregion

        #region Tool events
        private void primaryColorStripButton_Click(object sender, EventArgs e)
        {
            using (var d = new ColorDialog())
            {
                if (d.ShowDialog() == DialogResult.OK)
                {
                    primaryColor = d.Color;
                    primaryColorStripButton.BackColor = d.Color;
                }
            }
        }
        private void secondaryColorStripButton_Click(object sender, EventArgs e)
        {
            using (var d = new ColorDialog())
            {
                if (d.ShowDialog() == DialogResult.OK)
                {
                    secondaryColor = d.Color;
                    secondaryColorStripButton.BackColor = d.Color;
                }
            }
        }

        private void selectStripButton_Click(object sender, EventArgs e)
        {
            uncheckStripItems();
            selectedTool = Tools.RectangleSelect;
            selectStripButton.Checked = true;
        }
        private void lineStripButton_Click(object sender, EventArgs e)
        {
            uncheckStripItems();
            selectedTool = Tools.Line;
            lineStripButton.Checked = true;
        }
        private void circleStripButton_Click(object sender, EventArgs e)
        {
            uncheckStripItems();
            selectedTool = Tools.Circle;
            circleStripButton.Checked = true;
        }
        private void ellipseStripButton_Click(object sender, EventArgs e)
        {
            uncheckStripItems();
            selectedTool = Tools.Ellipse;
            ellipseStripButton.Checked = true;
        }
        private void rectangleStripButton_Click(object sender, EventArgs e)
        {
            uncheckStripItems();
            selectedTool = Tools.Rectangle;
            rectangleStripButton.Checked = true;
        }
        private void pencilStripButton_Click(object sender, EventArgs e)
        {
            uncheckStripItems();
            selectedTool = Tools.Pencil;
            pencilStripButton.Checked = true;
        }
        private void uncheckStripItems()
        {
            foreach (var i in toolsStrip.Items.OfType<ToolStripButton>())
                i.Checked = false;
        }
        #endregion

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //if (colorm != null)
              //  applyFilter(e);

            if (currentStroke.Count > 1)
            {
                if (!pastedArea.IsEmpty)
                {
                    drawPastedRect(e);
                    return;
                }
                switch (selectedTool)
                {
                    case Tools.RectangleSelect:
                        drawSelectionRectangle(e);
                        break;
                    case Tools.Rectangle:
                        drawRectangle(e);
                        goto default;
                    case Tools.Line:
                        drawLine(e);
                        goto default;
                    case Tools.Pencil:
                        drawPencil(e);
                        goto default;
                    case Tools.Circle:
                        drawCircle(e);
                        goto default;
                    case Tools.Ellipse:
                        drawEllipse(e);
                        goto default;
                    default:
                        isSaved = false;
                        break;
                }
            }
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!pastedArea.IsEmpty)
            {
                if (pastedArea.Contains(e.Location))
                    dragging = true;
            }
            currentStroke.Clear();
            selectedArea = new Rectangle();
            mouseDown = true;
            mouseButtonPressed = e.Button;
            currentStroke.Add(e.Location);
            pictureBox1.Invalidate();
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                currentStroke.Add(e.Location);
                pictureBox1.Invalidate();
            }
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            if(selectedTool != Tools.RectangleSelect || dragging)
            {
                history.Save();
                pictureBox1.DrawToBitmap(
                    (Bitmap)pictureBox1.Image,
                    new Rectangle(
                        0, 0, pictureBox1.Image.Width,
                        pictureBox1.Image.Height));
                currentStroke.Clear();
                isSaved = false;
            }
            dragging = false;
            pastedArea = new Rectangle();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isSaved)
            {
                switch (MessageBox.Show(
                    "Save changes?","Save?",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Exclamation))
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

        #region ComboBox events
        private void widthComboBox_Click(object sender, EventArgs e)
        {
            width = int.Parse((string)widthComboBox.SelectedItem);
        }
        private void fillComboBox_TextChanged(object sender, EventArgs e)
        {
            fill = (BrushType)fillComboBox.SelectedIndex;
        }
        private void lineComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (lineComboBox.SelectedIndex) {
                case 0:
                    line = DashStyle.Solid;
                    break;
                case 1:
                    line = DashStyle.Dash;
                    break;
                default:
                    line = DashStyle.DashDot;
                    break;
                }
        }
        #endregion

        #region Copy - Paste
        private void CopyToCliboard(Rectangle src_rect)
        {
            Bitmap bm = new Bitmap(src_rect.Width, src_rect.Height);

            using (Graphics gr = Graphics.FromImage(bm))
            {
                Rectangle dest_rect = new Rectangle(0, 0, src_rect.Width, src_rect.Height);
                gr.DrawImage(pictureBox1.Image, dest_rect, src_rect, GraphicsUnit.Pixel);
            }
            Clipboard.SetImage(bm);
        }
        private void copyStripButton_Click(object sender, EventArgs e)
        {
            if (!selectedArea.IsEmpty)
                CopyToCliboard(selectedArea);
        }
        private void cutStripButton_Click(object sender, EventArgs e)
        {
            if (!selectedArea.IsEmpty)
            {
                CopyToCliboard(selectedArea);
                history.Save();
                using (Graphics gr = Graphics.FromImage(pictureBox1.Image))
                {
                    using (SolidBrush br = new SolidBrush(Color.White))
                    {
                        gr.FillRectangle(br, selectedArea);
                    }
                }
                pictureBox1.Image = new Bitmap(pictureBox1.Image);
            }
        }
        private void pasteStripButton_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage()) return;

            Image clipboard_img = Clipboard.GetImage();
            pastedArea = new Rectangle(0, 0, clipboard_img.Width, clipboard_img.Height);
            var gr = pictureBox1.CreateGraphics();
            gr.DrawImage(clipboard_img, new Point(0, 0));
        }
        #endregion

        #region Filters
        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorm = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
                colorm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            var gr = Graphics.FromImage(bmp);
            var rect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);
            gr.DrawImage(
                pictureBox1.Image,
                new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height),
                0, 0,
                pictureBox1.Width,
                pictureBox1.Height,
                GraphicsUnit.Pixel,
                imageAttributes);
            history.Save();
            pictureBox1.Image = bmp;

        }
        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorm = new ColorMatrix(
                new float[][]
                {
                     new float[] {.393f, .349f, .272f, 0, 0},
                     new float[] {.769f, .686f, .534f, 0, 0},
                     new float[] {.189f, .168f, .131f, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {0, 0, 0, 0, 1}
                });
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
                colorm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            var gr = Graphics.FromImage(bmp);
            var rect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);
            gr.DrawImage(
                pictureBox1.Image,
                new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height),
                0, 0,
                pictureBox1.Width,
                pictureBox1.Height,
                GraphicsUnit.Pixel,
                imageAttributes);
            history.Save();
            pictureBox1.Image = bmp;

        }
        private void negativeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorm = new ColorMatrix(
                new float[][]
                {
                     new float[] {-1, 0, 0, 0, 0},
                     new float[] {0, -1, 0, 0, 0},
                     new float[] {0, 0, -1, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {1, 1, 1, 0, 1}
                });
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
                colorm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            var gr = Graphics.FromImage(bmp);
            var rect = new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height);
            gr.DrawImage(
                pictureBox1.Image,
                new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height),
                0, 0,
                pictureBox1.Width,
                pictureBox1.Height,
                GraphicsUnit.Pixel,
                imageAttributes);
            history.Save();
            pictureBox1.Image = bmp;
        }


        #endregion

        private void mosaicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = new MosaicFrom();
            this.Hide();
            d.ShowDialog();
            this.Show();
        }
    }
}

