using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NcForms
{
	public class NcSplashScreenFW : Form
	{
		bool _showImage;
		bool _showTxt;
		Size _size;
		int _scrPercent;
		Image _img = null;
		Point _imgOrigin = new Point(0,0);
		System.Windows.Forms.Timer _timer;

		//string _txt = string.Empty;

		/// <summary>
		/// Create and show a splash screen
		/// </summary>
		/// <param name="size">Form size</param>
		/// <param name="scrPercent">Size as screen percentage (0-100)</param>
		/// <param name="img">Image</param>
		/// <param name="resize_to_img">Resize form to image</param>
		/// <param name="delay_msec">delay ms</param>
		/// <param name="modal">modal (or modeless)</param>
		public NcSplashScreenFW(Size size, int scrPercent, Image img, bool resize_to_img, int delay_msec, bool modal)
		{
			SuspendLayout();
			ShowInTaskbar = false;
			ShowIcon = false;
			FormBorderStyle = FormBorderStyle.None;
			TopMost = true;
			StartPosition = FormStartPosition.CenterScreen;
			_scrPercent = scrPercent;
			_size = size;
			if(img != null)
			{
				if((img.Width > 0) && (img.Height > 0))
				{
					_img = img;
				}
			}
			//if(htmlTxt != null)
			//{
			//	if(htmlTxt.Length > 0)
			//	{
			//		_txt = htmlTxt;
			//	}
			//}
			
			RecalcSize();

			if(resize_to_img && (_img != null))
			{
				this.Size = _img.Size;
				_imgOrigin = new Point(0,0);
			}
			else
			{
				this.Size = _size;
			}
			
			if(_img != null)
			{
				PictureBox pb = new PictureBox();
				pb.Image = _img;
				pb.Location = _imgOrigin;
				//pb.SizeMode = PictureBoxSizeMode.StretchImage;
				pb.Size = _img.Size;
				this.Controls.Add(pb);
			}

			ResumeLayout();
			PerformLayout();

			if(delay_msec > 0)
			{
				_timer = new System.Windows.Forms.Timer();
				_timer.Interval = delay_msec;
				_timer.Tick += Timer1_Tick;
				_timer.Start();

				if(modal)
				{
					this.ShowDialog();
				}
				else
				{
					this.Show();
				}
			}
		}

		/// <summary>
		/// Recalculate form size, according to image and text
		/// </summary>
		void RecalcSize()
		{
			Size screenSize = new Size(0,0);

			if(Screen.PrimaryScreen != null)						// Get primary screen size
			{
				screenSize = Screen.PrimaryScreen.Bounds.Size;
				if((_scrPercent > 0) && (_scrPercent <= 100))		// Recalc size according to screen percentage
				{
					float fact = ((float)_scrPercent/100);
				
					int x = (int)(screenSize.Width * fact);
					int y = (int)(screenSize.Height * fact);
					if( (x>0) && (y>0))
					{
						_size = new Size(x,y);
					}
				}
			}

			if(_img != null)										// Enlarge size according to image size (if less than screen size)
			{
				int x = _size.Width;
				int y = _size.Height;
				bool resize = false;
				if((_img.Width > _size.Width) && (_img.Width < screenSize.Width))
				{
					x= _img.Width;
					resize = true;
				}
				if((_img.Height > _size.Height) && (_img.Height < screenSize.Height))
				{
					y = _img.Height;
					resize = true;
				}
				if(resize)
				{
					_size = new Size(x,y);
				}
			}

			if(_img != null)									// Adjust image origin
			{
				int x = 0;
				int y = 0;
				bool resize = false;
				if(_size.Width >= _img.Width)
				{
					x = (_size.Width - _img.Width) / 2;
					resize = true;
				}
				if(_size.Height >= _img.Height)
				{
					y = (_size.Height - _img.Height) / 2;
					resize = true;
				}
				if(resize)
				{
					_imgOrigin = new Point(x, y);
				}
			}
		}

		private void Timer1_Tick(object sender,EventArgs e)
		{
			#if DEBUG
			MessageBox.Show("Timer event");
			#endif
			if(_timer!=null)
			{
				_timer.Stop();
				_timer.Dispose();
			}
			Close();
			this.Dispose();
		}
	}
}
