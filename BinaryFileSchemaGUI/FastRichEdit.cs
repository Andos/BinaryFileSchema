using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace BinaryFileSchemaGUI
{
	public class FastRichEdit : RichTextBox
	{
		const short WM_PAINT = 0x00f;
		const short WM_USER = 0x0400;
		const short EM_GETSCROLLPOS = WM_USER + 221;
		const short EM_SETSCROLLPOS = WM_USER + 222;

		bool allowPaint = true;
		int pos = 0;
		int length = 0;
		Point scrollpos = new Point();

		public bool AllowPaint
		{
			get { return allowPaint; }
			set
			{
				if (allowPaint == false && value == true)
				{
					this.Select(pos, length);
					SetScrollPos(scrollpos);
					this.Enabled = true;
					this.Focus();
				}
				else if (value == false)
				{
					pos = this.SelectionStart;
					length = this.SelectionLength;
					scrollpos = this.GetScrollPos();
					this.Enabled = false;
				}
				allowPaint = value;
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_PAINT)
			{
				if (AllowPaint)
					base.WndProc(ref m);
				else
					m.Result = IntPtr.Zero;
			}
			else
				base.WndProc (ref m);
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct SeqPoint
		{
			public int x;
			public int y;
		}

		private unsafe Point GetScrollPos()
		{
			SeqPoint res = new SeqPoint();
			IntPtr ptr = new IntPtr(&res);
			Message m = Message.Create(this.Handle, EM_GETSCROLLPOS, IntPtr.Zero, ptr);
			this.WndProc(ref m);
			return new Point(res.x,res.y);
		}

		private unsafe void SetScrollPos( Point p )
		{
			SeqPoint res = new SeqPoint();
			res.x = p.X;
			res.y = p.Y;
			IntPtr ptr = new IntPtr(&res);
			Message m = Message.Create(this.Handle, EM_SETSCROLLPOS, IntPtr.Zero, ptr);
			this.WndProc(ref m);
		}



	}
}
