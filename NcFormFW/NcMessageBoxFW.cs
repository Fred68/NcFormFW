using NcForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NcForms
{
#pragma warning disable CS8618
#pragma warning disable CS8622
#pragma warning disable CS8600
#pragma warning disable CS8603
	
	public class NcMessageBoxFW:NcForms.NcFormFW
	{

		#region << PRIVATE MEMBERS... >>
		const string STRING_EMPTY = "";
		const float MB_OPACITY = 1.0f;
		const float MB_MAX_SCREEN_RATIO = 0.75f;

		private System.Windows.Forms.Button button1;

		private System.Windows.Forms.Button[] bts;
		private RichTextBox rtbTesto;
		private Point _rtbMargin;
		System.Windows.Forms.Timer timer1 = null;

		#endregion

		/// <summary>
		/// Static object for the static funcion: NcMessageBox.Show(...)
		/// Alternative: ThreadLocal<NcMessageBox> _mb;
		/// </summary>
		[ThreadStatic]
		static NcMessageBoxFW _mb;

		/// <summary>
		/// Show dialog
		/// </summary>
		/// <param name="ncf">Parent NcForm to copy colors from</param>
		/// <param name="text">Content of the message</param>
		/// <param name="caption">Caption on the title bar</param>
		/// <param name="buttons">System.Windows.Forms.MessageBoxButtons argument</param>
		/// <returns></returns>
		public static DialogResult Show(NcForms.NcFormFW ncf,string text,string caption = STRING_EMPTY,MessageBoxButtons buttons = MessageBoxButtons.OK,int timerDelay = 0)
		{
			DialogResult res = DialogResult.Cancel;
			NcFormStyle ncFs = new NcFormStyle(NcWindowsStyles.None,NcFormWindowStates.Normal);     // NcWindowsStyles.None: no lower bar
			NcFormColor ncFc = (ncf != null) ? new NcFormColor(ncf.BackColor,ncf.TitleColor,ncf.StatusBarColor,ncf.ButtonsColor,MB_OPACITY) : NcFormColor.Normal;
			using(_mb = new NcForms.NcMessageBoxFW(ncFs,ncFc,buttons,timerDelay))
			{
				_mb.rtbTesto.BackColor = ncFc.backColor;
				_mb.ButtonsColor = ncFc.buttonsColor;
				_mb.SetText(text,caption);
				res = _mb.ShowDialog();
			}
			return res;
		}


		#region << PRIVATE FUNCTIONS... >>
		
		private NcMessageBoxFW(NcFormStyle style,NcFormColor color,MessageBoxButtons buttons = MessageBoxButtons.OK,int tmDelay = 0) : base(style,color)
		{
			InitializeComponent();
#warning Reimpostare in altra funzione ed eliminare la linea:
			rtbTesto.ScrollBars = RichTextBoxScrollBars.None;

			int margin = rtbTesto.Font.Height/2;
			SetTextBoxMargin(margin, margin);

			SetUpButtons(buttons);

			if(tmDelay > 0)
			{
				timer1 = new System.Windows.Forms.Timer();
				timer1.Interval = tmDelay;
				timer1.Tick += Timer1_Tick;
				timer1.Start();
			}

			Invalidate();
		}
		private NcMessageBoxFW() : this(NcFormStyle.Fixed,NcFormColor.Normal) { }
		private NcMessageBoxFW(NcForms.NcFormFW ncf) : this(ncf.NcStyle,ncf.NcColor) { }
		private NcMessageBoxFW(NcFormFW ncf,string text,string caption = STRING_EMPTY,MessageBoxButtons buttons = MessageBoxButtons.OK) :
					this(ncf.NcStyle,ncf.NcColor,buttons)
		{
			BackColor = ncf.BackColor;
			SetText(text,caption);
		}
		
		private void InitializeComponent()
		{
			button1 = new System.Windows.Forms.Button();
			rtbTesto = new RichTextBox();
			SuspendLayout();
			// 
			// button1
			// 
			button1.Location = new Point(395,271);
			button1.Name = "button1";
			button1.Size = new Size(75,30);
			button1.TabIndex = 3;
			button1.Text = "button1";
			button1.UseVisualStyleBackColor = true;
			// 
			// rtbTesto
			// 
			rtbTesto.BorderStyle = BorderStyle.None;
			rtbTesto.Location = new Point(0,28);
			rtbTesto.Name = "rtbTesto";
			rtbTesto.ReadOnly = true;
			rtbTesto.Size = new Size(492,128);
			rtbTesto.TabIndex = 5;
			rtbTesto.Text = "";
			// 
			// NcMessageBox
			// 
			ClientSize = new Size(492,329);
			Controls.Add(rtbTesto);
			Controls.Add(button1);
			Name = "NcMessageBox";
			FormClosing += NcMessageBox_FormClosing;
			Shown += NcMessageBox_Shown;
			Controls.SetChildIndex(button1,0);
			Controls.SetChildIndex(rtbTesto,0);
			ResumeLayout(false);
			PerformLayout();
		}

		private int GetLowerBarHeight()
		{
			return ((this.NcStyle.ncWindowsStyle & NcWindowsStyles.LowerBar) == 0) ? 0 : LowerBarHeight;
		}	

		private void SetTextBoxMargin(int marginx, int marginy)
		{
			_rtbMargin = new Point(marginx, marginy);
		}

		private void SetUpButtons(MessageBoxButtons buttons)
		{
			int nbuttons = 1;

			switch(buttons)
			{
				case MessageBoxButtons.OK:
				{
					nbuttons = 1;
				}
				break;

				case MessageBoxButtons.OKCancel:
				case MessageBoxButtons.YesNo:
				case MessageBoxButtons.RetryCancel:
				{
					nbuttons = 2;
				}
				break;

				case MessageBoxButtons.YesNoCancel:
				case MessageBoxButtons.AbortRetryIgnore:
				//case MessageBoxButtons.CancelTryContinue:
				{
					nbuttons = 3;
				}
				break;

				default:
					break;
			}

			bts = new System.Windows.Forms.Button[nbuttons];
			Size sz = button1.Size;
			Point lc = button1.Location;
			if(this.Width > lc.X + sz.Width)        // Calculate as right aligned
			{
				lc.X += this.Width - (lc.X + sz.Width);
			}

			button1.Visible = false;
			Controls.Remove(button1 as System.Windows.Forms.Button);

			for(int i = 0;i < nbuttons;i++)
			{
				bts[i] = new System.Windows.Forms.Button();
				bts[i].Size = sz;
				bts[i].Location = new Point(lc.X - sz.Width * i,lc.Y);
				bts[i].Visible = true;
				bts[i].BackColor = this.ButtonsColor;
				Controls.Add(bts[i]);
			}

			switch(buttons)
			{
				case MessageBoxButtons.OK:
				{
					bts[0].DialogResult = DialogResult.OK;
					bts[0].Text = "OK";
				}
				break;
				case MessageBoxButtons.YesNo:
				{
					bts[0].DialogResult = DialogResult.No;
					bts[0].Text = "No";
					bts[1].DialogResult = DialogResult.Yes;
					bts[1].Text = "Yes";

				}
				break;
				case MessageBoxButtons.YesNoCancel:
				{
					bts[0].DialogResult = DialogResult.Cancel;
					bts[0].Text = "Cancel";
					bts[1].DialogResult = DialogResult.No;
					bts[1].Text = "No";
					bts[2].DialogResult = DialogResult.Yes;
					bts[2].Text = "Yes";
				}
				break;
				case MessageBoxButtons.AbortRetryIgnore:
				{
					bts[0].DialogResult = DialogResult.Ignore;
					bts[0].Text = "Ignore";
					bts[1].DialogResult = DialogResult.Retry;
					bts[1].Text = "Retry";
					bts[2].DialogResult = DialogResult.Abort;
					bts[2].Text = "Abort";
				}
				break;
				case MessageBoxButtons.RetryCancel:
				{
					bts[0].DialogResult = DialogResult.Cancel;
					bts[0].Text = "Cancel";
					bts[1].DialogResult = DialogResult.Retry;
					bts[1].Text = "Retry";
				}
				break;
				//case MessageBoxButtons.CancelTryContinue:
				//{
				//	bts[0].DialogResult = DialogResult.Continue;
				//	bts[0].Text = "Continue";
				//	bts[1].DialogResult = DialogResult.TryAgain;
				//	bts[1].Text = "TryAgain";
				//	bts[2].DialogResult = DialogResult.Cancel;
				//	bts[2].Text = "Cancel";
				//}
				//break;
				case MessageBoxButtons.OKCancel:
				{
					bts[0].DialogResult = DialogResult.Cancel;
					bts[0].Text = "Cancel";
					bts[1].DialogResult = DialogResult.OK;
					bts[1].Text = "OK";
				}
				break;

			}
			Invalidate();
		}
		
		private void SetText(string text,string caption = "Help",string statusText = STRING_EMPTY)
		{
			rtbTesto.Text = text;
			Title = caption;
			if(statusText != STRING_EMPTY)
			{
				StatusText = statusText;
			}
			TopMost = true;
			AskClose = false;
		}
		
		private void NcMessageBox_Shown(object sender,EventArgs e)
		{
			rtbTesto.DeselectAll();
			bts[bts.Length - 1].Select();

			SuspendLayout();

			rtbTesto.Size = GetAdjustedRtboxSize();             // Ridimensiona richTextBox in base al testo e ai pulsanti
			rtbTesto.Location = new Point(_rtbMargin.X, _rtbMargin.Y + UpperBarHeight);  // Move richTextBox to the left and right margins
			
			Size sz = new Size(									// Calcola lo spostamento dei pulsanti per allinere quello più a destra (bts[0]) alla richtextbox
				rtbTesto.Location.X + rtbTesto.Size.Width - (bts[0].Location.X + bts[0].Size.Width),
				rtbTesto.Location.Y + rtbTesto.Size.Height + _rtbMargin.Y - bts[0].Location.Y
				);

			foreach(System.Windows.Forms.Button b in bts)       // Sposta i pulansti
			{
				b.Location += sz;
			}
				
			this.Size = new Size(                               // Ricalcola le dimensioni della finestra
				rtbTesto.Size.Width + 2*_rtbMargin.X,
				UpperBarHeight + rtbTesto.Size.Height + 3*_rtbMargin.Y + bts[0].Size.Height + GetLowerBarHeight()
				);

			this.CenterToScreen();

			ResumeLayout(false);
			PerformLayout();
		}

		/// <summary>
		/// Diemsnioni del testo della rich text box
		/// </summary>
		/// <param name="rtb"></param>
		/// <returns></returns>
		private Size MeasureRtBoxText(RichTextBox rtb)
		{
			Size txtSz = new Size();
			Font fnt = rtb.Font;
			txtSz = TextRenderer.MeasureText(rtb.Text,fnt,txtSz,TextFormatFlags.ExpandTabs);
			return txtSz;
		}

		/// <summary>
		/// Calcola le dimensioni della text box in base alla dimensione del testo 
		/// e la correggem se necessario
		/// </summary>
		/// <returns></returns>
		private Size GetAdjustedRtboxSize()
		{
			bool bScrollBars = false;
			Size txtSz = MeasureRtBoxText(rtbTesto);	// Dimensione della RTBox richiesta dal testo
			Size btnSz = GetButtonsSize();				// Dimensione dei pulsanti (tutti insieme)
			Size maxSz = new Size(                      // Dimensione max rispetto alla working area dello schermo
				(int)(Screen.FromControl(this).WorkingArea.Width * MB_MAX_SCREEN_RATIO),    // Working area: zona utile dello schermo
				(int)(Screen.FromControl(this).WorkingArea.Height * MB_MAX_SCREEN_RATIO)    // Bounds: dimensione totale dello schermo
				);
			
			int w = Math.Max(txtSz.Width,btnSz.Width);
			int h = txtSz.Height;

			if(w > maxSz.Width)
			{
				w = maxSz.Width;
				bScrollBars = true;
			}
			if(h > maxSz.Height)
			{
				h = maxSz.Height;
				bScrollBars = true;
			}

			if(bScrollBars)
			{
				rtbTesto.ScrollBars = RichTextBoxScrollBars.Both;
				w += SystemInformation.VerticalScrollBarWidth;
				h += SystemInformation.HorizontalScrollBarHeight;
			}
			else
			{
				rtbTesto.ScrollBars = RichTextBoxScrollBars.None;
			}

			rtbTesto.WordWrap = !bScrollBars;
			

			return new Size(w,h);
		}
		
		




		/// <summary>
		/// Calcola le dimensioni complessive dei pulsanti
		/// </summary>
		/// <returns></returns>
		private Size GetButtonsSize()
		{
			int xmin, xmax, ymin, ymax;
			xmin = ymin = int.MaxValue;
			xmax = ymax = int.MinValue;
			foreach(System.Windows.Forms.Button b in bts)
			{
				if(b.Left < xmin)
					xmin = b.Left;
				if(b.Top < ymin)
					ymin = b.Top;
				if(b.Right > xmax)
					xmax = b.Right;
				if(b.Bottom > ymax)
					ymax = b.Bottom;
			}
			return new Size(xmax - xmin,ymax - ymin);
		}

		/// <summary>
		/// Ferma il timer prime della chiusura del form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NcMessageBox_FormClosing(object sender,FormClosingEventArgs e)
		{
			if(timer1 != null)
			{
				timer1.Stop();
				#if DEBUG
				MessageBox.Show("Timer stopped");
				#endif

			}
			#if false
			MessageBox.Show("Closing dialog");
			#endif

		}

		/// <summary>
		/// Allo scadere del tempo, chiude il form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Timer1_Tick(object sender,EventArgs e)
		{
			#if DEBUG
			MessageBox.Show("Timer event");
			#endif
			Close();
		}


		#endregion

		
	}
	#pragma warning restore
}
