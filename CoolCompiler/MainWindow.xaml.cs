using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Xml;
using System.Diagnostics;

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;

using Antlr.Runtime;
using Antlr.Runtime.Tree;

using CoolCompiler.Properties;

namespace CoolCompiler
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		string CurrentFileName;
		CompletionWindow completionWindow;
		AbstractFoldingStrategy foldingStrategy;
		FoldingManager foldingManager;
		Compiler Compiler;
		StringBuilder Log;

		public MainWindow()
		{
			IHighlightingDefinition coolHighlighting;
			using (System.IO.Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("CoolCompiler.CoolHighlighting.xshd"))
			{
				if (s == null)
					throw new InvalidOperationException("Could not find embedded resource");
				using (XmlReader reader = new XmlTextReader(s))
				{
					coolHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
						HighlightingLoader.Load(reader, HighlightingManager.Instance);
				}
			}
			HighlightingManager.Instance.RegisterHighlighting("Cool Highlighting", new string[] { ".cool" }, coolHighlighting);

			InitializeComponent();

			foldingStrategy = new CoolFoldingStrategy();
			foldingManager = FoldingManager.Install(tbEditor.TextArea);

			tbEditor.ShowLineNumbers = true;

			tbEditor.TextArea.TextEntering += tbEditor_TextArea_TextEntering;
			tbEditor.TextArea.TextEntered += tbEditor_TextArea_TextEntered;

			DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
			foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
			foldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
			foldingUpdateTimer.Start();

			if (Settings.Default.WindowLeft != 0)
				Left = Settings.Default.WindowLeft;
			if (Settings.Default.WindowTop != 0)
				Top = Settings.Default.WindowTop;
			if (Settings.Default.WindowWidth != 0)
				Width = Settings.Default.WindowWidth;
			if (Settings.Default.WindowTop != 0)
				Height = Settings.Default.WindowHeight;

			if (!string.IsNullOrEmpty(Settings.Default.FileName))
				LoadFile(Settings.Default.FileName);

			Log = new StringBuilder();
			CoolTokens.Load("../../output/CoolGrammar.tokens");
		}

		void tbEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (e.Text == ".")
			{
				// open code completion after the user has pressed dot:
				completionWindow = new CompletionWindow(tbEditor.TextArea);
				// provide AvalonEdit with the data:
				IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
				data.Add(new CoolCompletionData("Item1"));
				data.Add(new CoolCompletionData("Item2"));
				data.Add(new CoolCompletionData("Item3"));
				data.Add(new CoolCompletionData("Another item"));
				completionWindow.Show();
				completionWindow.Closed += delegate
				{
					completionWindow = null;
				};
			}
		}

		void tbEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		{
			if (e.Text.Length > 0 && completionWindow != null)
			{
				if (!char.IsLetterOrDigit(e.Text[0]))
				{
					// Whenever a non-letter is typed while the completion window is open,
					// insert the currently selected element.
					completionWindow.CompletionList.RequestInsertion(e);
				}
			}
			// do not set e.Handled=true - we still want to insert the character that was typed
		}

		void foldingUpdateTimer_Tick(object sender, EventArgs e)
		{
			if (foldingStrategy != null)
			{
				foldingStrategy.UpdateFoldings(foldingManager, tbEditor.Document);
			}
		}

		private void openFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.InitialDirectory = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
			dlg.CheckFileExists = true;
			if (dlg.ShowDialog() == true)
			{
				LoadFile(dlg.FileName);
				Settings.Default.FileName = CurrentFileName;
				Settings.Default.Save();
			}
		}

		private void saveFile_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentFileName == null)
			{
				SaveFileDialog dlg = new SaveFileDialog();
				dlg.DefaultExt = ".cool";
				if (dlg.ShowDialog() ?? false)
				{
					CurrentFileName = dlg.FileName;
				}
				else
				{
					return;
				}
			}
			SaveFile(CurrentFileName);
		}

		private void btnCompile_Click(object sender, RoutedEventArgs e)
		{
			Compile();
		}

		private void btnCompileAndRun_Click(object sender, RoutedEventArgs e)
		{
			Compile();
			Run();
		}

		private void LoadFile(string fileName)
		{
			CurrentFileName = fileName;
			tbEditor.Load(fileName);
		}

		private void SaveFile(string fileName)
		{
			tbEditor.Save(fileName);
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Settings.Default.WindowLeft = Left;
			Settings.Default.WindowTop = Top;
			Settings.Default.WindowWidth = Width;
			Settings.Default.WindowHeight = Height;
			Settings.Default.Save();
		}

		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F5)
			{
				Compile();
				Run();
			}
			else if (e.Key == Key.F6)
			{
				Compile();
			}
		}

		private void Compile()
		{
			Log.AppendFormat("[{0}] Starting compilation...{1}", DateTime.Now.ToShortTimeString(), Environment.NewLine);

			tbEditor.Save(CurrentFileName);

			Compiler = new Compiler();
			Compiler.Compile(CurrentFileName, tbEditor.Text);

			if (Compiler.HasErrors)
			{
				Log.AppendFormat("[{0}] Some errors has been detected.{1}", DateTime.Now.ToShortTimeString(), Environment.NewLine);
				Log.AppendFormat("[{0}] Compilation failed. '{1}' has not generated {2}", DateTime.Now.ToShortTimeString(),
					Compiler.GeneratedProgramName, Environment.NewLine);
			}
			else
				Log.AppendFormat("[{0}] The '{1}' file has been generated and saved.{2}", DateTime.Now.ToShortTimeString(), 
					Compiler.GeneratedProgramName, Environment.NewLine);

			Log.AppendLine();

			tbLog.Text = Log.ToString();
			tabCtrlLogAndErrors.SelectedIndex = 1;
			tbLog.ScrollToEnd();

			FillLexerAndParserTables(Compiler.Tree.Tree);
			dataGridErrors.ItemsSource = Compiler.Errors;
			dgTokens.ItemsSource = Compiler.Tokens;
		}

		private void Run()
		{
			if (Compiler != null && !Compiler.HasErrors)
				Process.Start(Compiler.GeneratedProgramName);
		}

		private void FillLexerAndParserTables(ITree tree)
		{
			trvSyntaxTree.Items.Clear();
			FillLexerAndParserTables(tree, null);
		}

		private void FillLexerAndParserTables(ITree tree, TreeViewItem parentTreeViewItem)
		{
			if (tree != null)
			for (int i = 0; i < tree.ChildCount; i++)
			{
				var item = new TreeViewItem { Header = tree.GetChild(i).Text, Tag = tree.GetChild(i) };
				if (parentTreeViewItem == null)
					trvSyntaxTree.Items.Add(item);
				else
					parentTreeViewItem.Items.Add(item);
				FillLexerAndParserTables(tree.GetChild(i), item);
			}
		}
		
		private void Window_Closed(object sender, EventArgs e)
		{
			tbEditor.Save(CurrentFileName);
		}

		private void dataGridErrors_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (dataGridErrors.SelectedIndex != -1)
			{
				var compilerError = Compiler.Errors.ElementAt(dataGridErrors.SelectedIndex);
				if (compilerError.Line != null)
				{
					try
					{
						tbEditor.ScrollTo((int)compilerError.Line, (int)compilerError.ColumnStart);
						int offset = tbEditor.Document.GetOffset((int)compilerError.Line, (int)compilerError.ColumnStart);
						tbEditor.Select(offset, 0);
						tbEditor.Focus();
					}
					catch
					{

					}
				}
			}
		}

		private void dgTokens_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (dgTokens.SelectedIndex != -1)
			{
				try
				{
					var tokens = Compiler.Tokens.ElementAt(dgTokens.SelectedIndex);
					tbEditor.ScrollTo(tokens.Line, tokens.Column);
					int offset = tbEditor.Document.GetOffset(tokens.Line, tokens.Column);
					tbEditor.Select(offset, 0);
					tbEditor.Focus();
				}
				catch
				{

				}
			}
		}

		private void trvSyntaxTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (trvSyntaxTree.SelectedItem != null)
			{
				try
				{
					var node = (trvSyntaxTree.SelectedItem as TreeViewItem).Tag as ITree;
					tbEditor.ScrollTo(node.Line + 1, node.CharPositionInLine);
					int offset = tbEditor.Document.GetOffset(node.Line + 1, node.CharPositionInLine);
					tbEditor.Select(offset, 0);
					tbEditor.Focus();
				}
				catch
				{

				}
			}
		}
	}
}
