using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace RP3_Projekt
{
    class UndoRedo
    {
        private Stack<Image> UndoStack;
        private Stack<Image> RedoStack;
        public bool IsUndoable { get { return UndoStack.Count != 0; } }
        public bool IsRedoable { get { return RedoStack.Count != 0; } }
        private PictureBox pbox;

        private static UndoRedo INSTANCE = new UndoRedo();

        private UndoRedo()
        {
            UndoStack = new Stack<Image>();
            RedoStack = new Stack<Image>();
        }

        public static UndoRedo getInstance(PictureBox pbox)
        {
            INSTANCE.pbox = pbox;
            return INSTANCE;
        }
        public void UndoAction()
        {
            if (IsUndoable)
            {
                RedoStack.Push(pbox.Image);
                if (pbox.Size != UndoStack.Peek().Size)
                    pbox.Size = UndoStack.Peek().Size;
                pbox.Image = UndoStack.Pop();
            }
        }
        public void RedoActon()
        {
            if (IsRedoable)
            {
                UndoStack.Push(pbox.Image);
                if(pbox.Size != RedoStack.Peek().Size)
                    pbox.Size = RedoStack.Peek().Size;
                pbox.Image = RedoStack.Pop();
            }
        }
        public void Save()
        {
            Image img = new Bitmap(pbox.Image);
            UndoStack.Push(img);
            RedoStack.Clear();
            
        }



    }
}
