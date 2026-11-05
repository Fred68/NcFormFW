using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

namespace NcForms
{
#pragma warning disable CS8618
#pragma warning disable CS8622
#pragma warning disable CS8600
#pragma warning disable CS8603
 




	/// <summary>
    /// Non Client Area Window base class
    /// </summary>
	public partial class NcFormFW : Form
	{
#if false
#if true
		const string MSG_SETTINGS =
@"FUNZIONE DA COMPLETARE

Prova testo multi linea
Chissà se funzionerà...                                                  ...correttamente
anche se con molti spazi ?





............................................................";
#else
		const string MSG_SETTINGS = ".";
#endif
#endif

		/************************/
		// Private members
		/************************/
		#region << PRIVATE MEMBERS... >>

		public const int tsItemExtraWidth = 3;
		public const int contentMinHeight = 10;

		//private System.ComponentModel.IContainer components;
		private ToolStripDropDownButton tsMenu;
		private ToolStripButton tsQuit;
		private ToolStripButton tsHelp;
		private ToolStripMenuItem tsmiHelp;
		private ToolStripMenuItem tsmiQuit;
		private ToolStripMenuItem settingsToolStripMenuItem;
		private ToolStripLabel tsTitle;
		private ToolStrip tsUpper;
		private ToolStrip tsLower;
		private ToolStripLabel statLabel;
		private ToolStripLabel reszLabel;

		NcFormStyle ncStyle;
		NcFormColor ncColor;

		Point startDrag, startCur, startResz;
		DateTime lastMouseDownEventTime;
		TimeSpan doubleClickDelay;			// Timespan identifying a double click on upper bar
		Size startSz, prevSz;
		bool dragging;
		bool resizing;
		float opacity;

		Size minTitleSz;
		bool bShowTitle;

		bool showTsHelp, showTsMenu, showTsMaxMin, showTsBar, showTsTop;
		Color colorTitle, colorStatusBar;
		Color colorButtons;
		int availWidthUpper, availWidthLower;

		private ToolStripButton tsMin;
		private ToolStripButton tsMax;
		private ToolStripButton tsBar;
		private ToolStripButton tsTop;
		
		private List<string> baseFormControlNames;

		bool hasMenu;
		bool hasMinMax;
		bool hasHelp;
		bool isResizable;
		bool hasTopMost;
		bool hasLowerBar;

		bool askClose, closing, loading;

		int minWidth;

		NcFormWindowStates ncWindowsState, prevNcWindowsState;
		private System.ComponentModel.IContainer components = null;

		#endregion
		/************************/

		/************************/
		// Constructors
		/************************/
		/// <summary>
		/// Empty constructor
		/// </summary>
		public NcFormFW() : this(NcFormStyle.Normal,NcFormColor.Normal) { }
		/// <summary>
		/// Constructor with same style and color
		/// </summary>
		/// <param name="ncf"></param>
		public NcFormFW(NcFormFW ncf) : this(ncf.ncStyle,ncf.ncColor) { }
		/// <summary>
		/// Constructor with params.
		/// NcForm potentially null fields are checked by a try...catch
		/// </summary>
		/// <param name="style"></param>		
		public NcFormFW(NcFormStyle style,NcFormColor color)
		{
			ncStyle = style;
			ncColor = color;
			askClose = closing = false;
			loading = true;
			hasMenu = (ncStyle.ncWindowsStyle & NcWindowsStyles.Menu) != 0;
			hasMinMax = (ncStyle.ncWindowsStyle & NcWindowsStyles.MinMax) != 0;
			hasHelp = (ncStyle.ncWindowsStyle & NcWindowsStyles.Help) != 0;
			isResizable = (ncStyle.ncWindowsStyle & NcWindowsStyles.Resizable) != 0;
			hasTopMost = (ncStyle.ncWindowsStyle & NcWindowsStyles.TopMost) != 0;
			hasLowerBar = ((ncStyle.ncWindowsStyle & NcWindowsStyles.LowerBar) != 0) || isResizable;

			try
			{
				InitializeComponent();
			}
			catch(Exception ex)
			{
				MessageBox.Show($"Errore {ex.Message}","Errore");
				Environment.Exit(0);
			}
			dragging = resizing = false;
			opacity = 0.7f;
			showTsHelp = true;          // Show Help icon and menu item
			showTsMenu = true;          // Show dropdown menu
			showTsMaxMin = true;        // Show maximize / minimize buttons
			showTsBar = true;           // Show OnlyBar button
			showTsTop = true;           // Show Topmost button
			TopMost = hasTopMost;       // Imposta subito lo stato topMost in base al flag
			bShowTitle = true;			// Show title text

			ncWindowsState = prevNcWindowsState = ncStyle.ncFormWindowState;

			TitleColor = ncColor.titleBarColor;
			ButtonsColor = ncColor.buttonsColor;
			StatusBarColor = ncColor.statusBarColor;
			BackColor = NcColor.backColor;
			Opacity = ncColor.opacity;
			lastMouseDownEventTime = DateTime.Now;
			DblClickDelaySeconds = NcFormStyle.dblclkOnBarSecondsDefault;
			baseFormControlNames = new List<string>();
			foreach (Control control in this.Controls)
			{
				baseFormControlNames.Add(control.Name);		// Controlli della classe base NcForm
			}
			
		}
		/************************/

		/************************/
		// Public members
		/************************/

		/// <summary>
		/// NcStyle (readonly)
		/// </summary>
		public NcFormStyle NcStyle { get { return ncStyle; } }
		/// <summary>
		/// NcColor (readonly)
		/// </summary>
		public NcFormColor NcColor { get { return ncColor; } }
		
		/// <summary>
		/// Set/Get WindowsState: Minimized, Maximized, Normal, BarOnly
		/// </summary>
		public NcFormWindowStates NcWindowsState
		{
			get { return ncWindowsState; }
			protected set
			{
				switch(value)
				{
					case NcFormWindowStates.Normal:
						this.WindowState = FormWindowState.Normal;
						this.prevNcWindowsState = ncWindowsState;
						if(this.prevNcWindowsState == NcFormWindowStates.BarOnly)
						{
							this.Size = prevSz;
							//tsLower.Visible = true;
							tsLower.Visible = hasLowerBar;
							tsMin.Visible = tsMax.Visible = true;
							SetTitleBar();
							this.ncWindowsState = NcFormWindowStates.Normal;
						}
						else if(this.prevNcWindowsState == NcFormWindowStates.Minimized)
						{
							//tsLower.Visible = true;
							tsLower.Visible = hasLowerBar;
							this.ncWindowsState = NcFormWindowStates.Normal;
						}
						else if(this.prevNcWindowsState == NcFormWindowStates.Maximized)
						{
							//tsLower.Visible = true;
							tsLower.Visible = hasLowerBar;
							this.ncWindowsState = NcFormWindowStates.Normal;
						}
						tsMax.Text = "+";
						break;

					case NcFormWindowStates.Minimized:
						this.WindowState = FormWindowState.Minimized;
						this.prevNcWindowsState = ncWindowsState;
						this.ncWindowsState = NcFormWindowStates.Minimized;
						break;

					case NcFormWindowStates.Maximized:
						this.WindowState = FormWindowState.Maximized;
						tsMax.Text = "-";
						this.prevNcWindowsState = ncWindowsState;
						this.ncWindowsState = NcFormWindowStates.Maximized;
						break;

					case NcFormWindowStates.BarOnly:
						this.prevNcWindowsState = ncWindowsState;
						if(this.prevNcWindowsState == NcFormWindowStates.Normal)
						{
							this.WindowState = FormWindowState.Normal;
							prevSz = this.Size;
							this.ncWindowsState = NcFormWindowStates.BarOnly;
							tsLower.Visible = false;
							tsMin.Visible = tsMax.Visible = false;
							#if NET
							this.Size = new Size(Size.Width - int.Min(availWidthUpper,availWidthLower),tsUpper.Height);
							#else
							this.Size = new Size(Size.Width - Math.Min(availWidthUpper,availWidthLower),tsUpper.Height);
							#endif
							tsTitle.AutoSize = true;
						}
						break;

					default:
						break;
				}

			}

		}	
		/// <summary>
		/// Text on the status bar
		/// </summary>
		public string StatusText
		{
			get { return statLabel.Text; }
			protected set { statLabel.Text = value; }
		}
		/// <summary>
		/// Title text
		/// </summary>
		public string Title
		{
			get { return tsTitle.Text; }
			protected set
			{
				SetTitleBar(value);
			}
		}	
		/// <summary>
		/// Title always visible
		/// </summary>
		public bool ShowTitle
		{
			get
			{
				return bShowTitle;
			}
			set
			{
				bShowTitle = value;
			}
		}
		/// <summary>
		/// Opacity
		/// </summary>
		public new float Opacity
		{
			get { return opacity; }
			protected set
			{
				opacity = value;
				base.Opacity = opacity;
			}
		}
		/// <summary>
		/// Title bar color
		/// </summary>
		public Color TitleColor
		{
			get { return colorTitle; }
			protected set
			{
				colorTitle = value;
				tsUpper.BackColor = colorTitle;
			}
		}
		public Color ButtonsColor
		{
			get { return colorButtons; }
			protected set
			{
				colorButtons = value;
				SetButtonsColor();
			}
		}
		/// <summary>
		/// Status bar color
		/// </summary>
		public Color StatusBarColor
		{
			get { return colorStatusBar; }
			protected set
			{
				colorStatusBar = value;
				tsLower.BackColor = colorStatusBar;
			}
		}
		/// <summary>
		/// Title height (readonly)
		/// </summary>
		public int UpperBarHeight
		{
			get { return minTitleSz.Height + tsItemExtraWidth; }
		}
		public int LowerBarHeight
		{
			get { return tsLower.Height + tsItemExtraWidth; }
		}
		public int MinWidth
		{
			get {return minWidth;}
			protected set {SetMinWidth(value);}
		}
		/// <summary>
		/// TimeSpan to identify a double click on upper toolbar
		/// Default DoubleClickDelay = TimeSpan.FromSeconds(0.3);
		/// </summary>
		public double DblClickDelaySeconds
		{
			get {return doubleClickDelay.TotalSeconds;}
			protected set {doubleClickDelay = TimeSpan.FromSeconds(value);}
		}
		/// <summary>
		/// Ask before closing
		/// </summary>
		public bool AskClose
		{
			get {	return askClose;	}
			protected set {
					askClose = value;
					tsQuit.ToolTipText = askClose ? "Close" : "Close immediately";
				}
		}
		/// <summary>
		/// Resize form to content
		/// </summary>
		protected void ResizeToContent(int xDistance = 0, int yDistance = 0)
		{
			StringBuilder sb = new StringBuilder();
			Point pmin = new Point(int.MaxValue, int.MaxValue);
			Point pmax = new Point(int.MinValue, int.MinValue);
			bool xmin,xmax,ymin,ymax;
			xmin=xmax=ymin=ymax=false;
			foreach(Control control in this.Controls)
			{
				if(!IsBaseControl(control))
				{
					if(control.Dock == DockStyle.None)
					{
						if(control.Left < pmin.X) {pmin.X = control.Left; xmin = true;}
						if(control.Top < pmin.Y) {pmin.Y = control.Top; ymin = true;}
						if(control.Right > pmax.X) {pmax.X = control.Right; xmax = true;}
						if(control.Bottom > pmax.Y) {pmax.Y = control.Bottom; ymax = true;}
					}
				}

			}
			if(xmin && xmax && ymin && ymax)
			{
				Size newsize = new Size(pmax.X + xDistance, pmax.Y + tsLower.Height + yDistance);
				SetTitleBar();
				GetAdjustedSize(ref newsize);
				this.Size = newsize;
				ncWindowsState = NcFormWindowStates.Normal;
			}
		}
		/// <summary>
		/// Set bar font
		/// </summary>
		/// <param name="font"></param>
		/// <param name="ncbar"></param>
		protected void SetBarFont(Font font, NcBars ncbar)
		{
			int oldHgt = tsUpper.Height;
			if( (ncbar & NcBars.Upper) != 0 )		tsUpper.Font = font;
			if( (ncbar & NcBars.Lower) != 0 )		tsLower.Font = font;
			minTitleSz = tsUpper.Size;		// not tsTitle.Size;
			MoveControls(0, tsUpper.Height - oldHgt);
		}
		/// <summary>
		/// Return bar font (or null, if not specified)
		/// </summary>
		/// <param name="ncbar"></param>
		/// <returns></returns>
		protected Font GetBarFont(NcBars ncbar)
		{
			Font f = null;
			if(ncbar == NcBars.Upper)		
			{
				f = tsUpper.Font;
			}
			else if (ncbar == NcBars.Lower)
			{
				f = tsLower.Font;
			}
			return f;
		}		
		/// <summary>
		/// Version string
		/// </summary>
		/// <param name="execAssy">from Assembly.GetExecutingAssembly()</param>
		/// <returns></returns>
		virtual public string Version(Assembly asm, bool details = false)
		{
			StringBuilder strb = new StringBuilder();
			try
			{
				strb.AppendLine(Application.ProductName);
				if(asm != null)
				{
					# if NET
					System.Version? v = asm.GetName().Version;
					#else
					System.Version v = asm.GetName().Version;
					#endif
					if(v != null) strb.AppendLine($"Version: {v.ToString()} ({BuildTime(asm)})");
					if(details)
					{
						#if NET
						string? n;
						#else
						string n;
						#endif
						n = asm.GetName().Name;
						if(n != null) strb.AppendLine("Assembly name: " + n);
						strb.AppendLine("BuildTime time: "+ File.GetCreationTime(asm.Location).ToString());
						strb.AppendLine("BuildTime number: " + BuildTime(asm,true));
						strb.AppendLine("Executable path: " + Application.ExecutablePath);
					}
				}
				strb.AppendLine("Copyright: " + Application.CompanyName);
			}
			catch	{}
			return strb.ToString();
		}
		/// <summary>
		/// Build string
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		public string BuildTime(Assembly asm, bool number = false)
		{
			StringBuilder strb = new StringBuilder();
			if(asm != null)
			{
				DateTime dt = File.GetCreationTime(asm.Location);
				if(number)
					strb.Append(String.Format("{0:yyMMddhh}.{0:mmss}",dt));
				else
					strb.Append(dt.ToString("d"));
			}	
			return strb.ToString();
		}
		/// <summary>
		/// Move controls
		/// </summary>
		/// <param name="xDelta"></param>
		/// <param name="yDelta"></param>
		protected void MoveControls(int xDelta, int yDelta)
		{
			foreach(Control control in this.Controls)
				{
					if(!IsBaseControl(control))
					{
						if(control.Dock == DockStyle.None)
						{
							control.Location = new Point(control.Location.X + xDelta, control.Location.Y + yDelta);
							//if(control.Left < pmin.X) {pmin.X = control.Left; xmin = true;}
							//if(control.Top < pmin.Y) {pmin.Y = control.Top; ymin = true;}
							//if(control.Right > pmax.X) {pmax.X = control.Right; xmax = true;}
							//if(control.Bottom > pmax.Y) {pmax.Y = control.Bottom; ymax = true;}

							//sb.AppendLine($"{control.Name} top={control.Top} left={control.Left} bottom={control.Bottom} right={control.Right}");
						}
					}

				}
			}
		/// <summary>
		/// Add needed events to control
		/// if created after base.OnLoad event
		/// </summary>
		/// <param name="control"></param>
		protected void SetupControlEvents(Control control)
		{
			control.MouseEnter += eMouseEnter;
			control.MouseLeave += eMouseLeave;
		}

		/// <summary>
		/// Add item into main menu
		/// </summary>
		/// <param name="txt">Text</param>
		/// <param name="pos">Position in the menu (-1 at the end)</param>
		/// <param name="handler">Handler: public delegate void EventHandler(object sender, EventArgs e);</param>
		/// <returns></returns>
		protected ToolStripMenuItem AddSystemMenuItem(string txt, int pos, EventHandler handler)
		{
			ToolStripMenuItem tsmi = null;
			if(pos >= -1)
			{
				tsmi = new ToolStripMenuItem();
				// tsmi.Size = new System.Drawing.Size(10,10); Probabilmente superfluo
				tsmi.AutoSize = true;
				tsmi.Text = txt;
				tsmi.Click += handler;

				if(pos < 0 || pos >= tsMenu.DropDownItems.Count)
				{
					// tsMenu.DropDownItems.Add(tsmi);
					tsMenu.DropDownItems.Insert(tsMenu.DropDownItems.Count-1,tsmi); // Inserisce prima dell'ultima voce (Quit)
				}
				else
				{
					tsMenu.DropDownItems.Insert(pos,tsmi);
				}
			}
			return tsmi;
		}
		/************************/

		/**************************************/
		// Virtual functions (to be overridden)
		/**************************************/

		/// <summary>
		/// Help function
		/// </summary>
		protected virtual void OnHelp()
		{
			MessageBox.Show(Version(Assembly.GetExecutingAssembly()));
		}

		/// <summary>
		/// Ask confirmation onFormClosing event
		/// </summary>
		/// <returns>true to cancel event</returns>
		protected virtual bool OnClosingCancelEvent()
		{
			bool cancel = false;
			if(MessageBox.Show("Quit","Quit ?",MessageBoxButtons.YesNo) != DialogResult.Yes)
			{
				cancel = true;	
			}
			return cancel;
		}

		/// <summary>
		/// Settings
		/// </summary>
		protected virtual void OnSettings(object sender,EventArgs e)
		{
			string msg =
#if false
@"Prova messaggio lungo per testare il comportamento di NcMessage Box
.
Testo lungo                                      con spazi inseriti per controllare il testo a capo automatico


.
.
.
.
..
...
....
.................................................................................";
#else
@"Da completare";
#endif
			
			NcMessageBoxFW.Show(this,msg,"DA COMPLETARE",MessageBoxButtons.OK);
		}

		/************************/

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}


		/*********************************/
		// Private functions and handlers
		/*********************************/
		#region << PRIVATE FUNCTIONS AND HANDLERS... >>
		private void NcForm_Load(object sender,EventArgs e)
		{
			SetupControls(this);		// Executed on derived class controls, not only base class controls.
			NcWindowsState = ncWindowsState;
			SetBarFont(ncStyle.barsFont,NcBars.All);
			loading = false;
		}
		
		#if NET
		private void SetTitleBar(string? txt = null)
		#else
		private void SetTitleBar(string txt = null)
		#endif
		{
			showTsMenu = hasMenu;               // Usa i flag del costruttore
			showTsHelp = hasHelp;
			showTsMaxMin = hasMinMax;
			showTsTop = hasTopMost;

			// Imposta visibilità
			reszLabel.Visible = isResizable;    // Usa il flag del costruttore
			tsMenu.Visible = showTsMenu;
			tsHelp.Visible = tsmiHelp.Visible = showTsHelp;
			tsMax.Visible = tsMin.Visible = tsBar.Visible = showTsMaxMin;
			tsTop.Visible = showTsTop;
			
			if(ncWindowsState != NcFormWindowStates.BarOnly)
			{
				tsLower.Visible = hasLowerBar;
			}

			if(txt != null) tsTitle.Text = txt;
			tsTitle.AutoSize = true;
			minTitleSz = tsTitle.Size;


			availWidthUpper = this.Width
								- (showTsMenu ? tsMenu.Width + tsItemExtraWidth : tsItemExtraWidth)
								- (showTsHelp ? tsHelp.Width + tsItemExtraWidth : tsItemExtraWidth)
								- (showTsTop ? tsTop.Width + tsItemExtraWidth : tsItemExtraWidth)
								-(showTsBar ? tsBar.Width + tsItemExtraWidth : tsItemExtraWidth)
								-tsQuit.Width
								- (showTsMaxMin ? tsMax.Width + tsMin.Width + 3 * tsItemExtraWidth : 3 * tsItemExtraWidth)
								- (tsQuit.Width + tsItemExtraWidth);

			availWidthLower = this.Width - statLabel.Width - reszLabel.Width;

			if(availWidthUpper < minTitleSz.Width)
			{
				tsTitle.Visible = bShowTitle;
			}
			else
			{
				tsTitle.AutoSize = false;
				tsTitle.Visible = true;
				tsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
				tsTitle.Size = new Size(availWidthUpper,minTitleSz.Height);
			}
		}

		#if false
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800,450);
			this.Text = "NcFormFW";
		}
		#endif

		/*********************************/
		
		private void InitializeComponent()
		{
			this.tsUpper = new System.Windows.Forms.ToolStrip();
			this.tsMenu = new System.Windows.Forms.ToolStripDropDownButton();
			this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiQuit = new System.Windows.Forms.ToolStripMenuItem();
			this.tsTop = new System.Windows.Forms.ToolStripButton();
			this.tsTitle = new System.Windows.Forms.ToolStripLabel();
			this.tsQuit = new System.Windows.Forms.ToolStripButton();
			this.tsHelp = new System.Windows.Forms.ToolStripButton();
			this.tsMax = new System.Windows.Forms.ToolStripButton();
			this.tsMin = new System.Windows.Forms.ToolStripButton();
			this.tsBar = new System.Windows.Forms.ToolStripButton();
			this.tsLower = new System.Windows.Forms.ToolStrip();
			this.statLabel = new System.Windows.Forms.ToolStripLabel();
			this.reszLabel = new System.Windows.Forms.ToolStripLabel();
			this.tsUpper.SuspendLayout();
			this.tsLower.SuspendLayout();
			this.SuspendLayout();
			// 
			// tsUpper
			// 
			this.tsUpper.BackColor = System.Drawing.SystemColors.Control;
			this.tsUpper.CanOverflow = false;
			this.tsUpper.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsMenu,
            this.tsTop,
            this.tsTitle,
            this.tsQuit,
            this.tsHelp,
            this.tsMax,
            this.tsMin,
            this.tsBar});
			this.tsUpper.Location = new System.Drawing.Point(0, 0);
			this.tsUpper.Name = "tsUpper";
			this.tsUpper.Size = new System.Drawing.Size(491, 25);
			this.tsUpper.TabIndex = 0;
			this.tsUpper.Text = "tsUpper";
			tsUpper.MouseDown += ts_MouseDown;
			tsUpper.MouseUp += ts_MouseUp;

			// 
			// tsMenu
			// 
			this.tsMenu.AccessibleDescription = "";
			this.tsMenu.AccessibleName = "";
			this.tsMenu.AutoToolTip = false;
			this.tsMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
			{
				this.settingsToolStripMenuItem,
				this.tsmiHelp,
				this.tsmiQuit
			});
			this.tsMenu.Name = "tsMenu";
			this.tsMenu.Size = new System.Drawing.Size(26, 22);
			this.tsMenu.Text = "v";
			// 
			// settingsToolStripMenuItem
			// 
			this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
			this.settingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
			this.settingsToolStripMenuItem.Text = "Settings";
			settingsToolStripMenuItem.Click += OnSettings;

			// 
			// tsmiHelp
			// 
			this.tsmiHelp.Name = "tsmiHelp";
			this.tsmiHelp.Size = new System.Drawing.Size(116, 22);
			this.tsmiHelp.Text = "Help";
			tsmiHelp.Click += tsHelp_Click;
			// 
			// tsmiQuit
			// 
			this.tsmiQuit.Name = "tsmiQuit";
			//this.tsmiQuit.AutoSize = true;
			//this.tsmiQuit.Size = new System.Drawing.Size(10,10);
			this.tsmiQuit.Size = new System.Drawing.Size(116, 22);
			this.tsmiQuit.Text = "Quit";
			tsmiQuit.Click += tsmiQuit_Click;
			// 
			// tsTop
			// 
			this.tsTop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsTop.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsTop.Name = "tsTop";
			this.tsTop.Size = new System.Drawing.Size(23, 22);
			this.tsTop.Text = "T";
			this.tsTop.ToolTipText = "OnTop";
			tsTop.Click += tsTop_Click;
			// 
			// tsTitle
			// 
			this.tsTitle.AutoSize = false;
			this.tsTitle.BackColor = System.Drawing.SystemColors.Control;
			this.tsTitle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.tsTitle.ImageTransparentColor = System.Drawing.Color.White;
			this.tsTitle.Name = "tsTitle";
			this.tsTitle.Size = new System.Drawing.Size(100, 22);
			this.tsTitle.Text = "Title";
			this.tsTitle.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			tsTitle.MouseDown += tsTitle_MouseDown;
			tsTitle.MouseUp += tsTitle_MouseUp;
			// 
			// tsQuit
			// 
			this.tsQuit.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tsQuit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsQuit.Name = "tsQuit";
			this.tsQuit.Size = new System.Drawing.Size(23, 22);
			this.tsQuit.Text = "X";
			this.tsQuit.ToolTipText = "Close";
			tsQuit.Click += tsClose_Click;
			// 
			// tsHelp
			// 
			this.tsHelp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tsHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsHelp.Name = "tsHelp";
			this.tsHelp.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.tsHelp.Size = new System.Drawing.Size(23, 22);
			this.tsHelp.Text = "?";
			this.tsHelp.ToolTipText = "Help";
			tsHelp.Click += tsHelp_Click;
			// 
			// tsMax
			// 
			this.tsMax.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tsMax.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsMax.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsMax.Name = "tsMax";
			this.tsMax.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.tsMax.Size = new System.Drawing.Size(23, 22);
			this.tsMax.Text = "+";
			this.tsMax.ToolTipText = "Maximize";
			tsMax.Click += tsMax_Click;
			// 
			// tsMin
			// 
			this.tsMin.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tsMin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsMin.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsMin.Name = "tsMin";
			this.tsMin.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.tsMin.Size = new System.Drawing.Size(23, 22);
			this.tsMin.Text = "_";
			this.tsMin.ToolTipText = "Minimize";
			tsMin.Click += tsMin_Click;
			// 
			// tsBar
			// 
			this.tsBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tsBar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsBar.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsBar.Name = "tsBar";
			this.tsBar.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.tsBar.Size = new System.Drawing.Size(23, 22);
			this.tsBar.Text = "=";
			this.tsBar.ToolTipText = "BarOnly";
			tsBar.Click += tsBar_Click;
			// 
			// tsLower
			// 
			this.tsLower.CanOverflow = false;
			this.tsLower.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tsLower.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statLabel,
            this.reszLabel});
			this.tsLower.Location = new System.Drawing.Point(0, 318);
			this.tsLower.Name = "tsLower";
			this.tsLower.Size = new System.Drawing.Size(491, 25);
			this.tsLower.TabIndex = 2;
			this.tsLower.Text = "tsLower";
			// 
			// statLabel
			// 
			this.statLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.statLabel.Name = "statLabel";
			this.statLabel.Size = new System.Drawing.Size(39, 22);
			this.statLabel.Text = "Status";
			// 
			// reszLabel
			// 
			this.reszLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.reszLabel.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.reszLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.reszLabel.Name = "reszLabel";
			this.reszLabel.Size = new System.Drawing.Size(23, 22);
			this.reszLabel.Text = "<>";
			reszLabel.MouseDown += reszLabel_MouseDown;
			reszLabel.MouseEnter += reszLabel_MouseEnter;
			reszLabel.MouseLeave += reszLabel_MouseLeave;
			// 
			// NcFormFW
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(491, 343);
			this.Controls.Add(this.tsLower);
			this.Controls.Add(this.tsUpper);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "NcFormFW";

			FormClosing += NcForm_FormClosing;
			Load += NcForm_Load;
			ResizeEnd += NcForm_ResizeEnd;
			MouseEnter += eMouseEnter;
			MouseLeave += eMouseLeave;
			MouseUp += NcForm_MouseUp;
			Resize += NcForm_Resize;

			this.tsUpper.ResumeLayout(false);
			this.tsUpper.PerformLayout();
			this.tsLower.ResumeLayout(false);
			this.tsLower.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		
		/*********************************/

		
		
		private void SetMinWidth(int mw)
		{
			if(mw >= 0)
			{
				minWidth = mw;
			}
		}
		/// <summary>
		/// Set-up form controls' handlers
		/// </summary>
		/// <param name="control"></param>
		private void SetupControls(Control control)
		{
			SetButtonsColor();
			SetEnterLeaveForControls(control,eMouseEnter,eMouseLeave);
			SetEnterLeaveForSingleControl(tsUpper,eMouseEnter,eMouseLeave,etsMouseEnter,etsMouseLeave);
			SetMouseMoveSingleControl(tsUpper,Mouse_Move);
			SetTitleBar();
		}
		private bool IsBaseControl(Control control)
		{
			return baseFormControlNames.Contains(control.Name);
		}
		private string GetControlNames()
		{	
			StringBuilder sb = new StringBuilder();
			foreach (Control control in this.Controls)
			{
				sb.AppendLine(control.Name);
			}
			return sb.ToString();
		}
		private bool GetAdjustedSize(ref Size size)
		{
			bool resize = false;
			Size newsize = size;
			#if NET
			int minWidth = this.Width - int.Min(availWidthUpper,availWidthLower);
			#else
			int minWidth = this.Width - Math.Min(availWidthUpper,availWidthLower);
			#endif
			int minHeight = tsUpper.Height + tsLower.Height + contentMinHeight;

			if(size.Width < minWidth)
			{
				newsize.Width = minWidth;
				resize = true;
			}
			if(size.Height < minHeight)
			{
				newsize.Height = minHeight;
				resize = true;
			}

			if(resize)		size = newsize;
			return resize;
		}
		private void AdjustSize()
		{
			Size newsize = this.Size;
			if(GetAdjustedSize(ref newsize))
			{
				this.Size = newsize;
			}
		}
		private void SwitchBarOnly()
		{
			if(NcWindowsState == NcFormWindowStates.Normal)
			{
				NcWindowsState = NcFormWindowStates.BarOnly;
			}
			else if(NcWindowsState == NcFormWindowStates.BarOnly)
			{
				NcWindowsState = NcFormWindowStates.Normal;
			}
		}
		private void SetEnterLeaveForControls(Control control,EventHandler eVmouseEnter,EventHandler eVmouseLeave)
		{
			foreach(Control childControl in control.Controls)
			{
				childControl.MouseEnter += eVmouseEnter;
				childControl.MouseLeave += eVmouseLeave;
				SetEnterLeaveForControls(childControl,eVmouseEnter,eVmouseLeave);
			}

		}
		private void SetButtonsColor()
		{
			foreach(Control control in this.Controls)
				{
					if(control.GetType() == typeof(Button))
					{
						control.BackColor = ButtonsColor;
					}
				}
		}
		private void ClearEnterLeaveForControls(Control control,EventHandler eVmouseEnter,EventHandler eVmouseLeave)
		{
			foreach(Control childControl in control.Controls)
			{
				childControl.MouseEnter -= eVmouseEnter;
				childControl.MouseLeave -= eVmouseLeave;
				ClearEnterLeaveForControls(childControl,eVmouseEnter,eVmouseLeave);
			}

		}
		private void SetEnterLeaveForSingleControl(Control control,EventHandler oldVmouseEnter,EventHandler oldVmouseLeave,EventHandler eVmouseEnter,EventHandler eVmouseLeave)
		{
			control.MouseEnter -= oldVmouseEnter;
			control.MouseLeave -= oldVmouseLeave;
			control.MouseEnter += eVmouseEnter;
			control.MouseLeave += eVmouseLeave;
		}
		private void SetMouseMoveSingleControl(Control control,MouseEventHandler eVmouseMove)
		{
			this.MouseMove += eVmouseMove;
		}
		private void tsClose_Click(object sender,EventArgs e)
		{
			Close();
		}
		private void tsmiQuit_Click(object sender,EventArgs e)
		{
			Close();
		}
		private void NcForm_FormClosing(object sender,FormClosingEventArgs e)
		{
			closing = true;
			if(askClose && !loading)
			{
				if(OnClosingCancelEvent())
				{
					e.Cancel = true;
					closing = false;
				}
			}
		}
		private void eMouseEnter(object sender,EventArgs e)
		{
			base.Opacity = 1f;
			this.Focus();	// Get focus, no need of an extra click to get it
		}
		/// <summary>
		/// MouseLeave handler.
		/// If closing, no action is performed (the control might have been destroyed)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void eMouseLeave(object sender,EventArgs e)
		{
			if(!closing)
			{
				base.Opacity = Opacity;
			}
		}
		private void etsMouseEnter(object sender,EventArgs e)
		{
			eMouseEnter(sender,e);
			this.Cursor = Cursors.Cross;
			dragging = false;   // Se esce, non cattura il MouseUp. Al rientro deve interrompere il dragging
		}
		private void etsMouseLeave(object sender,EventArgs e)
		{
			eMouseLeave(sender,e);
			this.Cursor = Cursors.Default;
		}
		private void ts_MouseDown(object sender,MouseEventArgs e)
		{
			Mouse_Down(sender,e);
		}
		private void ts_MouseUp(object sender,MouseEventArgs e)
		{
			Mouse_Up(sender,e);
		}
		private void tsTitle_MouseUp(object sender,MouseEventArgs e)
		{
			Mouse_Up(sender,e);
		}
		private void tsTitle_MouseDown(object sender,MouseEventArgs e)
		{
			Mouse_Down(sender,e);
		}
		private void Mouse_Down(object sender,MouseEventArgs e)
		{
			DateTime dateTime = DateTime.Now;
			if(dateTime - lastMouseDownEventTime < doubleClickDelay)
			{
				tsBar_DoubleClick();
				return;
			}
			lastMouseDownEventTime = dateTime;
			if((NcWindowsState == NcFormWindowStates.Normal) || (NcWindowsState == NcFormWindowStates.BarOnly))
			{
				startDrag = this.Location;
				startCur = Cursor.Position;
				dragging = true;
				this.Capture = true;
			}
		}
		private void Mouse_Up(object sender,MouseEventArgs e)
		{
			if(dragging)
			{
				dragging = false;
			}
			if(resizing)
			{
				resizing = false;
			}
			this.Capture = false;
			//SetTextStatus($"MouseUp {e.Location.ToString()}");
		}
		private void Mouse_Move(object sender,MouseEventArgs e)
		{
			if(dragging && ((NcWindowsState == NcFormWindowStates.Normal) || (NcWindowsState == NcFormWindowStates.BarOnly)))
			{
				Point dif = Point.Subtract(Cursor.Position,new Size(startCur));
				this.Location = Point.Add(startDrag,new Size(dif));
				Invalidate();
			}
			if(resizing && (NcWindowsState == NcFormWindowStates.Normal))
			{
				Point dif = Point.Subtract(Cursor.Position,new Size(startResz));
				Size newSz = Size.Add(startSz,new Size(dif));
				this.Size = newSz;
				Invalidate();
			}

		}
		private void reszLabel_MouseEnter(object sender,EventArgs e)
		{
			Cursor = Cursors.SizeNWSE;
			//statLabel.Text = "Enter";
		}
		private void reszLabel_MouseLeave(object sender,EventArgs e)
		{
			Cursor = Cursors.Default;
		}
		private void reszLabel_MouseDown(object sender,MouseEventArgs e)
		{
			if(NcWindowsState == NcFormWindowStates.Normal)
			{
				resizing = true;
				startResz = Cursor.Position;
				startSz = this.Size;
				this.Capture = true;
			}
		}
		/// <summary>
		/// With this.Capture mouse events are captured by the form, not by reszLabel_MouseUp(...)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NcForm_MouseUp(object sender,MouseEventArgs e)
		{
			if(resizing)
			{
				this.Capture = false;
				resizing = false;
				NcForm_ResizeEnd(sender,e);
			}
		}
		private void NcForm_ResizeEnd(object sender,EventArgs e)
		{
			SetTitleBar();
			AdjustSize();
		}
		private void tsMin_Click(object sender,EventArgs e)
		{
			NcWindowsState = NcFormWindowStates.Minimized;
		}
		private void tsMax_Click(object sender,EventArgs e)
		{
			if(NcWindowsState == NcFormWindowStates.Normal)
			{
				NcWindowsState = NcFormWindowStates.Maximized;
			}
			else
			{
				NcWindowsState = NcFormWindowStates.Normal;
			}
		}
		private void tsBar_Click(object sender,EventArgs e)
		{
			SwitchBarOnly();
		}
		/// <summary>
		/// Called when the form is restore after having been minimized
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NcForm_Resize(object sender,EventArgs e)
		{
			if(NcWindowsState == NcFormWindowStates.Minimized)
			{
				NcWindowsState = NcFormWindowStates.Normal;
			}
			SetTitleBar();
		}
		private void tsTop_Click(object sender,EventArgs e)
		{
			TopMost = !(TopMost);   // Scambia lo stato
			tsTop.Text = TopMost ? "T" : "t";
		}
		private void tsHelp_Click(object sender,EventArgs e)
		{
			OnHelp();		// Chiama la funzione virtuale (che può subire l'override)
		}		
		private void tsBar_DoubleClick()
		{
			SwitchBarOnly();
		}
		#endregion
		/************************/

	}
	#pragma warning restore
}
