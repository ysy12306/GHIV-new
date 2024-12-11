Imports Microsoft.Web.WebView2.Core
Imports System.IO
Imports Newtonsoft.Json
Imports System.Threading.Thread
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel
Imports Newtonsoft.Json.Linq
Imports System.Reflection.Emit
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Button
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Windows.Forms
Imports System.Text
Imports Microsoft.Web.WebView2.WinForms
Imports System.Threading
Imports System.Text.Json

Public Class Mainform
    Public Taxon_Dataset As New DataSet
    Delegate Sub SetUrl(ByVal url As String)
    Dim set_web_main_url As SetUrl

    Delegate Sub AppendText(ByVal text As String)
    Dim Append_Text As AppendText

    Public Sub Append_info(ByVal info As String)
        Me.Invoke(Append_Text, New Object() {Format(Now(), "yyyy/MM/dd H:mm:ss") + vbTab + info + Chr(13)})
    End Sub

    Public Sub initialize_data()
        Dim Column_Select As New DataGridViewCheckBoxColumn
        Column_Select.HeaderText = "Select"
        DataGridView1.Columns.Insert(0, Column_Select)
        DataGridView1.AllowUserToAddRows = False

        Dim taxon_table As New System.Data.DataTable
        taxon_table.TableName = "Taxon Table"
        Dim Column_ID As New System.Data.DataColumn("ID", System.Type.GetType("System.Int32"))
        Dim Column_Taxon As New System.Data.DataColumn("Name")
        Dim Column_Seq As New System.Data.DataColumn("Sequence")
        Dim Column_Time As New System.Data.DataColumn("Continuous Traits")
        Dim Column_State As New System.Data.DataColumn("Discrete Traits")
        Dim Column_Count As New System.Data.DataColumn("Quantity")
        Dim Column_Organism As New System.Data.DataColumn("Organism")
        taxon_table.Columns.Add(Column_ID)
        taxon_table.Columns.Add(Column_Taxon)
        taxon_table.Columns.Add(Column_Seq)
        taxon_table.Columns.Add(Column_State)
        taxon_table.Columns.Add(Column_Time)
        taxon_table.Columns.Add(Column_Count)
        taxon_table.Columns.Add(Column_Organism)
        Taxon_Dataset.Tables.Add(taxon_table)

        dtView = Taxon_Dataset.Tables("Taxon Table").DefaultView
        dtView.AllowNew = False
        dtView.AllowDelete = False
        dtView.AllowEdit = False
    End Sub
    Private Sub RT_Append_Text(ByVal text As String)
        RichTextBox1.AppendText(text)
        RichTextBox1.Refresh()
    End Sub
    Private Sub web_main_seturl(ByVal url As String)
        WebView_main.Source = New Uri(url)
    End Sub
    Private Sub Mainform_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Dim filePath As String = root_path + "main\" + "setting.ini"
        SaveSettings(filePath, settings)
        End
    End Sub
    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        WebView_main.CreationProperties = New CoreWebView2CreationProperties With {
            .BrowserExecutableFolder = Path.Combine(root_path, "webview")
        }
        Dim userDataFolder As String = Path.Combine(root_path, "webview", "userdata")
        Dim webView2Environment As CoreWebView2Environment = Await CoreWebView2Environment.CreateAsync(Nothing, userDataFolder)
        Await WebView_main.EnsureCoreWebView2Async(webView2Environment)
        CheckForIllegalCrossThreadCalls = False
        current_thread = Math.Max(System.Environment.ProcessorCount - 2, 1)
        currentDirectory = Application.StartupPath
        set_web_main_url = New SetUrl(AddressOf web_main_seturl)
        Append_Text = New AppendText(AddressOf RT_Append_Text)
        WebView_main.Source = New Uri("file:///" + currentDirectory + "/main/" + language + "/main.html")
        initialize_data()
    End Sub
    Private Sub SaveWebPageAshtml(filePath As String)
        Dim currentUrl As String = WebView_main.CoreWebView2.Source
        Dim fileName As String = System.IO.Path.GetFileNameWithoutExtension(currentUrl.Replace("file:///", ""))
        Dim history_folder As String = Path.Combine(root_path, "history")
        Dim main_folder As String = Path.Combine(root_path, "main")
        Dim targetfolder As String = Path.Combine(System.IO.Path.GetDirectoryName(filePath), System.IO.Path.GetFileNameWithoutExtension(filePath))
        Dim targetResultfolder As String = Path.Combine(targetfolder, "results")

        ' 检查是否存在相应的HTML文件并复制
        If File.Exists(history_folder + "\" + fileName + ".html") Then




            If Not Directory.Exists(targetResultfolder) Then
                Directory.CreateDirectory(targetResultfolder)
            End If
            Dim files As String() = Directory.GetFiles(history_folder)
            For Each myfile As String In files
                Dim fileInfo As FileInfo = New FileInfo(myfile)
                If fileInfo.Name.StartsWith(fileName) Then
                    Dim targetFile As String = Path.Combine(targetResultfolder, fileInfo.Name)
                    File.Copy(myfile, targetFile, True)
                End If
            Next
            If Not Directory.Exists(Path.Combine(targetfolder, "main")) Then
                Directory.CreateDirectory(Path.Combine(targetfolder, "main"))
            End If
            CopyDirectory(main_folder, Path.Combine(targetfolder, "main"))

            Dim htmlContent As String = "<!DOCTYPE html>" & vbCrLf &
                                        "<html>" & vbCrLf &
                                        "<head>" & vbCrLf &
                                        "<meta charset='UTF-8'>" & vbCrLf &
                                        "<title>Redirect</title>" & vbCrLf &
                                        "<script type='text/javascript'>" & vbCrLf &
                                        "window.location.href = './" & System.IO.Path.GetFileNameWithoutExtension(filePath) & "/results/" & fileName & ".html';" & vbCrLf &
                                        "</script>" & vbCrLf &
                                        "</head>" & vbCrLf &
                                        "<body>" & vbCrLf &
                                        "</body>" & vbCrLf &
                                        "</html>"

            ' 写入HTML文件
            File.WriteAllText(filePath, htmlContent)
        End If
    End Sub
    Private Async Sub SaveWebPageAsPdf(filePath As String)
        ' 检查 WebView2 是否已初始化
        Try
            ' 调用 DevTools 协议生成 PDF
            Dim result As String = Await WebView_main.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.printToPDF", "{}")

            ' 解析返回的 JSON 结果
            Dim jsonDocument As JsonDocument = jsonDocument.Parse(result)
            Dim pdfDataBase64 As String = jsonDocument.RootElement.GetProperty("data").GetString()

            ' 将 base64 数据转换为字节数组
            Dim pdfData As Byte() = Convert.FromBase64String(pdfDataBase64)

            ' 保存 PDF 文件
            File.WriteAllBytes(filePath, pdfData)
            MessageBox.Show("PDF saved successfully!")
        Catch ex As Exception
            MessageBox.Show("Failed to save PDF: " & ex.Message)
        End Try
    End Sub
    Private Sub WebView_main_CoreWebView2InitializationCompleted(sender As Object, e As CoreWebView2InitializationCompletedEventArgs) Handles WebView_main.CoreWebView2InitializationCompleted
        WebView_main.CoreWebView2.Settings.IsStatusBarEnabled = False
        WebView_main.CoreWebView2.Settings.AreDefaultContextMenusEnabled = False
    End Sub

    Private Sub 载入序列ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 载入序列ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog
        opendialog.Filter = "Fasta File(*.fasta)|*.fas;*.fasta;*.fa|GenBank File|*.gb"
        opendialog.FileName = ""
        opendialog.Multiselect = False
        opendialog.DefaultExt = ".fas"
        opendialog.CheckFileExists = True
        opendialog.CheckPathExists = True
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            current_file = opendialog.FileName
            Taxon_Dataset.Tables("Taxon Table").Clear()
            DataGridView1.DataSource = Nothing
            TabControl1.SelectedIndex = 1
            data_loaded = False
            If opendialog.FileName.ToLower.EndsWith(".gb") Then
                Dim startInfo As New ProcessStartInfo()
                startInfo.FileName = currentDirectory + "analysis\build_gb.exe" ' 替换为实际的命令行程序路径
                startInfo.WorkingDirectory = currentDirectory + "history\" ' 替换为实际的运行文件夹路径
                startInfo.CreateNoWindow = create_no_window
                startInfo.Arguments = "-input " + """" + current_file + """" + " -outdir " + """" + "out_gb" + """"
                Dim process As Process = Process.Start(startInfo)
                process.WaitForExit()
                process.Close()
                load_csv_data(currentDirectory + "history\out_gb\gb_info.csv")
                data_type = "gb"
            Else
                form_config_stand.Show()
                data_type = "fas"
            End If
        End If
    End Sub



    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Select Case timer_id
            Case 0
                ProgressBar1.Value = PB_value
                TextBox1.Text = info_text
            Case 1
                If waiting Then
                    TabControl1.SelectedIndex = 0
                    Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/waiting.html"})
                    waiting = False
                End If

            Case 2
                Timer1.Enabled = False
                DataGridView1.DataSource = dtView
                dtView.AllowNew = False
                dtView.AllowEdit = True

                DataGridView1.Columns(1).ReadOnly = True
                DataGridView1.Columns(2).ReadOnly = True
                DataGridView1.Columns(3).ReadOnly = True
                DataGridView1.Columns(4).ReadOnly = False

                DataGridView1.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(2).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(3).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(4).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(5).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(6).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(7).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(0).Width = 50
                DataGridView1.Columns(1).Width = 50
                DataGridView1.Columns(2).Width = 240
                DataGridView1.Columns(3).Width = 400
                DataGridView1.Columns(4).Width = 120
                DataGridView1.Columns(5).Width = 120
                DataGridView1.Columns(6).Width = 60
                DataGridView1.Columns(7).Width = 120

                PB_value = 0
                info_text = ""
                Timer1.Enabled = True
                DataGridView1.RefreshEdit()
                GC.Collect()
                timer_id = 0
                data_loaded = True
            Case 3
                Timer1.Enabled = False
                Dim savedialog As New SaveFileDialog
                savedialog.Filter = "fasta File(*.fasta)|*.fas;*.fasta"
                savedialog.FileName = ""
                savedialog.DefaultExt = ".fasta"
                Dim resultdialog As DialogResult = savedialog.ShowDialog()
                If resultdialog = DialogResult.OK Then
                    safe_copy(root_path + "temp\temp_file.tmp", savedialog.FileName)
                    show_info("save to " + savedialog.FileName)
                End If
                timer_id = 0
                PB_value = 0
                info_text = ""
                Timer1.Enabled = True
                GC.Collect()
            Case 4
                Timer1.Enabled = False
                Dim savedialog As New SaveFileDialog
                savedialog.Filter = "csv file(*.csv)|*.csv;*.CSV"
                savedialog.FileName = ""
                savedialog.DefaultExt = ".csv"
                Dim resultdialog As DialogResult = savedialog.ShowDialog()
                If resultdialog = DialogResult.OK Then
                    safe_copy(root_path + "temp\temp_file.tmp", savedialog.FileName)
                    show_info("save to " + savedialog.FileName)
                End If
                timer_id = 0
                PB_value = 0
                info_text = ""
                Timer1.Enabled = True
                GC.Collect()
            Case 5
                Timer1.Enabled = False
                Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "history/" + current_file + ".html"})
                timer_id = 0
                PB_value = 0
                info_text = ""
                Timer1.Enabled = True
            Case 6
                Timer1.Enabled = False
                分析结果列表ToolStripMenuItem_Click(sender, e)
                timer_id = 0
                PB_value = 0
                info_text = ""
                Timer1.Enabled = True
            Case 7
                ProgressBar1.Value = PB_value
                TextBox1.Text = info_text
                If waiting Then
                    TabControl1.SelectedIndex = 0
                    Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/waiting.html"})
                    waiting = False
                End If
            Case 8
                Timer1.Enabled = False
                Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/main.html"})
                Taxon_Dataset.Tables("Taxon Table").Clear()
                DataGridView1.DataSource = Nothing
                MergeFastaFiles(form_config_barcode.TextBox4.Text)
                format_fasta(Path.Combine(form_config_barcode.TextBox4.Text, "Combined.fasta"))
                timer_id = 2
                PB_value = 0
                info_text = ""
                TabControl1.SelectedIndex = 1
                Timer1.Enabled = True
            Case 9
                info_text = "构建序列……"
                Timer1.Enabled = False
                MergeFastaFiles(Path.Combine(currentDirectory, "temp", "temp_quick"))
                format_fasta(Path.Combine(currentDirectory, "temp", "temp_quick", "Combined.fasta"))
                copy_temp2results()
                PB_value = 0
                DataGridView1.DataSource = dtView
                dtView.AllowNew = False
                dtView.AllowEdit = True

                DataGridView1.Columns(1).ReadOnly = True
                DataGridView1.Columns(2).ReadOnly = True
                DataGridView1.Columns(3).ReadOnly = True
                DataGridView1.Columns(4).ReadOnly = False

                DataGridView1.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(2).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(3).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(4).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(5).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(6).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(7).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(0).Width = 50
                DataGridView1.Columns(1).Width = 50
                DataGridView1.Columns(2).Width = 240
                DataGridView1.Columns(3).Width = 400
                DataGridView1.Columns(4).Width = 120
                DataGridView1.Columns(5).Width = 120
                DataGridView1.Columns(6).Width = 60
                DataGridView1.Columns(7).Width = 120
                PB_value = 0
                info_text = ""

                DataGridView1.RefreshEdit()
                GC.Collect()
                timer_id = 0
                data_loaded = True
                For i As Integer = 1 To dtView.Count
                    DataGridView1.Rows(i - 1).Cells(0).Value = True
                Next
                TabControl1.SelectedIndex = 0
                info_text = "计算分型……"
                Dim th1 As New Threading.Thread(AddressOf analysis_type)
                th1.Start(False)
                Timer1.Enabled = True
            Case 10
                Timer1.Enabled = False
                MergeFastaFiles(Path.Combine(currentDirectory, "temp", "temp_quick"))
                format_fasta(Path.Combine(currentDirectory, "temp", "temp_quick", "Combined.fasta"))
                copy_temp2results()
                PB_value = 0
                DataGridView1.DataSource = dtView
                dtView.AllowNew = False
                dtView.AllowEdit = True

                DataGridView1.Columns(1).ReadOnly = True
                DataGridView1.Columns(2).ReadOnly = True
                DataGridView1.Columns(3).ReadOnly = True
                DataGridView1.Columns(4).ReadOnly = False

                DataGridView1.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(2).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(3).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(4).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(5).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(6).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(7).SortMode = DataGridViewColumnSortMode.NotSortable
                DataGridView1.Columns(0).Width = 50
                DataGridView1.Columns(1).Width = 50
                DataGridView1.Columns(2).Width = 240
                DataGridView1.Columns(3).Width = 400
                DataGridView1.Columns(4).Width = 120
                DataGridView1.Columns(5).Width = 120
                DataGridView1.Columns(6).Width = 60
                DataGridView1.Columns(7).Width = 120
                PB_value = 0
                info_text = ""

                DataGridView1.RefreshEdit()
                GC.Collect()
                data_loaded = True
                For i As Integer = 1 To dtView.Count
                    DataGridView1.Rows(i - 1).Cells(0).Value = True
                    DataGridView1.Rows(i - 1).Cells(4).Value = "NEW"
                Next
                data_loaded = False
                add_data = True
                Timer1.Enabled = True
                load_csv_data(currentDirectory + "analysis\database\HIV_ch.csv")
                Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/waiting.html"})
                Dim th1 As New Threading.Thread(AddressOf analysis_network)
                th1.Start("modified_tcs")

            Case Else

        End Select
    End Sub
    Private Sub copy_temp2results()
        Dim currentDateTime As String = DateTime.Now.ToString("yyyyMMdd_HHmm")
        Dim targetFolder As String = Path.Combine(currentDirectory, "results", currentDateTime)
        If Not Directory.Exists(targetFolder) Then
            Directory.CreateDirectory(targetFolder)
        End If
        Dim sourceFolder As String = Path.Combine(currentDirectory, "temp", "temp_quick")
        Dim fileTypes As String() = {"*.fasta", "*.png", "*.txt"}
        For Each fileType As String In fileTypes
            Dim files As String() = Directory.GetFiles(sourceFolder, fileType)
            For Each tempfile As String In files
                Dim fileName As String = Path.GetFileName(tempfile)
                Dim destFile As String = Path.Combine(targetFolder, fileName)
                File.Copy(tempfile, destFile, True)
            Next
        Next
    End Sub
    Private Sub 载入数据ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 载入数据ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog
        opendialog.Filter = "CSV File (*.csv)|*.csv;*.CSV|ALL Files(*.*)|*.*"
        opendialog.FileName = ""
        opendialog.Multiselect = False
        opendialog.DefaultExt = ".csv"
        opendialog.CheckFileExists = True
        opendialog.CheckPathExists = True
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            TabControl1.SelectedIndex = 1
            DataGridView1.EndEdit()
            Taxon_Dataset.Tables("Taxon Table").Clear()
            DataGridView1.DataSource = Nothing
            data_loaded = False
            Dim th1 As New Threading.Thread(AddressOf load_csv_data)
            th1.Start(opendialog.FileName)
            'data_type = "fas"
        End If
    End Sub
    Public Sub load_csv_data(ByVal file_path As String)
        Dim dr As StreamReader = Nothing
        Try
            dr = New StreamReader(file_path)
            Dim line As String = dr.ReadLine
            Dim count As Integer = 0
            Dim pre_fasta_seq As Integer = 0
            If add_data And fasta_seq IsNot Nothing Then
                pre_fasta_seq = UBound(fasta_seq)
            Else
                add_data = False
            End If
            line = dr.ReadLine
            dtView.AllowNew = True
            Do
                If line <> "" Then
                    count += 1
                    dtView.AddNew()
                    Dim newrow() As String = line.Split(",")
                    ReDim Preserve newrow(6)
                    newrow(0) = pre_fasta_seq + count
                    ReDim Preserve fasta_seq(pre_fasta_seq + count)
                    fasta_seq(pre_fasta_seq + count) = newrow(2)
                    If fasta_seq(pre_fasta_seq + count).Length > 1000 Then
                        newrow(2) = newrow(2).Substring(0, 1000)
                    End If
                    dtView.Item(pre_fasta_seq + count - 1).Row.ItemArray = newrow
                End If
                line = dr.ReadLine
            Loop Until line Is Nothing

            dr.Close()
            add_data = False
            Append_info("Data loaded successfully!")
            timer_id = 2

        Catch ex As Exception
            MsgBox(ex.Message)
            If dr IsNot Nothing Then
                dr.Close()
            End If
            add_data = False
            timer_id = 0
        End Try

    End Sub

    Private Sub 保存数据ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 保存数据ToolStripMenuItem.Click
        Dim opendialog As New SaveFileDialog
        opendialog.Filter = "CSV File (*.csv)|*.csv;*.CSV"
        opendialog.FileName = ""
        opendialog.DefaultExt = ".csv"
        opendialog.CheckFileExists = False
        opendialog.CheckPathExists = True
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            If opendialog.FileName.ToLower.EndsWith(".csv") Then
                Dim dw As New StreamWriter(opendialog.FileName, False)
                Dim state_line As String = "ID,Name"
                For j As Integer = 3 To DataGridView1.ColumnCount - 1
                    state_line += "," + DataGridView1.Columns(j).HeaderText
                Next
                dw.WriteLine(state_line)
                For i As Integer = 1 To dtView.Count
                    state_line = i.ToString
                    For j As Integer = 2 To DataGridView1.ColumnCount - 1
                        If j = 3 Then
                            state_line += "," + fasta_seq(i)
                        Else
                            state_line += "," + dtView.Item(i - 1).Item(j - 1)
                        End If
                    Next
                    dw.WriteLine(state_line)
                Next
                dw.Close()
            End If
            Append_info（"Save Successfully!")
        End If
    End Sub

    Private Sub 分型ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 分型ToolStripMenuItem.Click

        If dtView.Count >= 1 Then
            Dim selected_count As Integer = 0
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    selected_count += 1
                End If
            Next
            If selected_count >= 1 Then
                TabControl1.SelectedIndex = 0
                Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/waiting.html"})
                Dim th1 As New Threading.Thread(AddressOf analysis_type)
                th1.Start(True)
            Else
                MsgBox("Please select at least one sequence!")
            End If

        End If
    End Sub


    Private Sub 本地分析ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 本地分析ToolStripMenuItem.Click
        If dtView.Count >= 1 Then
            Dim selected_count As Integer = 0
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    selected_count += 1
                End If
            Next
            If selected_count >= 1 Then

                TabControl1.SelectedIndex = 0
                Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/waiting.html"})
                Dim th1 As New Threading.Thread(AddressOf analysis_type)
                th1.Start(False)
            Else
                MsgBox("Please select at least one sequence!")
            End If

        End If
    End Sub
    Public Sub analysis_type(ByVal online As Boolean)
        Dim currentTime As DateTime = DateTime.Now
        Dim currentTimeStamp As Long = (currentTime - New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
        Dim formattedTime As String = currentTime.ToString("yyyy-MM-dd HH:mm")
        Dim in_path As String = root_path + "history\" + currentTimeStamp.ToString + ".fasta"
        Dim sw As New StreamWriter(in_path)
        For i As Integer = 1 To dtView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                sw.WriteLine(">" + dtView.Item(i - 1).Item(1))
                sw.WriteLine(fasta_seq(i))
            End If
        Next
        sw.Close()
        Dim out_path As String = root_path + "history\" + currentTimeStamp.ToString + ".json"
        Dim startInfo As New ProcessStartInfo()
        If online Then
            startInfo.FileName = currentDirectory + "analysis\sierra.exe" ' 替换为实际的命令行程序路径
            startInfo.WorkingDirectory = currentDirectory + "analysis\" ' 替换为实际的运行文件夹路径
            startInfo.Arguments = "fasta " + """" + in_path + """" + " -o " + """" + out_path + """"
            startInfo.CreateNoWindow = create_no_window
        Else
            startInfo.FileName = currentDirectory + "analysis\sierralocal.exe" ' 替换为实际的命令行程序路径
            startInfo.WorkingDirectory = currentDirectory + "analysis\" ' 替换为实际的运行文件夹路径
            startInfo.CreateNoWindow = create_no_window
            startInfo.Arguments = " " + """" + in_path + """" + " -alignment nuc"
        End If

        Dim process As Process = Process.Start(startInfo)
        process.WaitForExit()
        process.Close()
        Dim jsonfile As String
        If online Then
            jsonfile = root_path + "history\" + currentTimeStamp.ToString + ".0.json"
        Else
            jsonfile = root_path + "history\" + currentTimeStamp.ToString + "_results.json"
        End If
        info_text = "生成报告……"

        If File.Exists(jsonfile) Then
            Dim sr As New StreamReader(jsonfile)
            Dim json_str As String = sr.ReadToEnd
            sr.Close()
            Dim jsonarr As JArray = JsonConvert.DeserializeObject(json_str)
            Dim sw0 As New StreamWriter(currentDirectory + "history/" + currentTimeStamp.ToString + ".html")
            Dim sr1 As New StreamReader(currentDirectory + "main/" + language + "/header.txt")
            sw0.Write(sr1.ReadToEnd)
            sr1.Close()
            Dim sr_report As New StreamReader(currentDirectory + "main/" + language + "/body_report.txt")
            Dim body_report As String = sr_report.ReadToEnd
            sr_report.Close()
            body_report = body_report.Replace("$time$", formattedTime)
            body_report = body_report.Replace("$id$", currentTimeStamp)
            Dim raw_names_str As String = ""
            If raw_names(0) <> "" Then
                For i As Integer = 1 To raw_names.Length
                    raw_names_str += vbCrLf + "<tr>"
                    raw_names_str += "<td>" + i.ToString + "</td>"
                    raw_names_str += "<td>" + raw_names(i - 1) + "</td></tr>"
                Next
            End If

            body_report = body_report.Replace("$table$", raw_names_str)
            sw0.Write(body_report)

            For Each j As JObject In jsonarr
                Dim header As String = j.SelectToken("inputSequence.header")
                Dim subtypeText As String = j.SelectToken("subtypeText")
                Dim validationResults As JArray = j.SelectToken("validationResults")
                Dim val_level As String = ""
                Dim val_message As String = ""
                For Each k As JObject In validationResults
                    val_level = k.SelectToken("level")
                    val_message = k.SelectToken("message")
                    val_message = val_message.Replace(" positions were not sequenced or aligned:", "个位点没有测序或排序, 包括: ")
                    val_message = val_message.Replace(" PR ", " 蛋白酶(PR) ").Replace(" RT ", " 逆转录酶(RT) ")
                    val_message = val_message.Replace(". Of them, ", ". 其中有").Replace(" are at drug-resistance positions", "个位于耐药性区域")
                Next

                '写入main的html
                Dim sr2 As New StreamReader(currentDirectory + "main/" + language + "/body.txt")
                Dim body_str As String = sr2.ReadToEnd
                sr2.Close()

                Dim subtypeText_str As String
                Dim subtypeText_info As String
                If online Then
                    subtypeText_str = subtypeText.Split("(")(0)
                    subtypeText_info = subtypeText.Split("(")(1).Replace(")", "")
                    subtypeText_info = header + "的分型结果为" + subtypeText_str + ", 与最近缘的参考序列相比, 差异度为" + subtypeText_info
                    body_str = body_str.Replace("$header$", header)
                    body_str = body_str.Replace("$title$", "分型结果").Replace("$subtitle$", subtypeText_str).Replace("$body$", subtypeText_info)
                    body_str = body_str.Replace("$title1$", "附加信息").Replace("$subtitle1$", val_level).Replace("$body1$", val_message)
                Else
                    body_str = body_str.Replace("$header$", header)
                    For i As Integer = 1 To dtView.Count
                        If DataGridView1.Rows(i - 1).Cells(2).Value.ToString = header Then
                            DataGridView1.Rows(i - 1).Cells(4).Value = subtypeText
                        End If
                    Next
                    body_str = body_str.Replace("$title$", "分型结果").Replace("$subtitle$", subtypeText).Replace("$body$", "")
                    body_str = body_str.Replace("$title1$", "附加信息").Replace("$subtitle1$", "").Replace("$body1$", "无")
                End If
                Dim drug_info_str As String = ""
                Dim drugResistance As JArray = j.SelectToken("drugResistance")
                For Each k In drugResistance
                    Dim version_text As String = k.SelectToken("version.text")
                    Dim version_publishDate As String = k.SelectToken("version.publishDate")
                    Dim gene_name As String = k.SelectToken("gene.name")
                    Dim drugScores As JArray = k.SelectToken("drugScores")
                    For Each m In drugScores
                        Dim drug_info() As String = {m.SelectToken("drugClass.name"), m.SelectToken("drug.name"), m.SelectToken("drug.displayAbbr"), m.SelectToken("score"), m.SelectToken("text"), m.SelectToken("level")}
                        Dim drug_text As String = drug_info(4)
                        drug_info(4) = drug_info(4).Replace("Susceptible", "敏感")
                        drug_info(4) = drug_info(4).Replace("Potential ", "潜在")
                        drug_info(4) = drug_info(4).Replace("Low-Level Resistance", "低抵抗")
                        drug_info(4) = drug_info(4).Replace("Intermediate Resistanc", " 中抵抗")
                        drug_info(4) = drug_info(4).Replace("High-Level Resistanc", " 高抵抗")
                        drug_info(4) = drug_info(4) + " (" + drug_text + ")"
                        drug_info_str += vbCrLf + "<tr>"
                        For n As Integer = 0 To UBound(drug_info)
                            drug_info_str += "<td>" + drug_info(n) + "</td>"
                        Next
                        drug_info_str += "<td></td></tr>"
                    Next
                Next
                body_str = body_str.Replace("$table$", drug_info_str)
                sw0.Write(body_str)
            Next
            Dim sr3 As New StreamReader(currentDirectory + "main/" + language + "/footer.txt")
            sw0.Write(sr3.ReadToEnd)
            sw0.Close()
            Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "history/" + currentTimeStamp.ToString + ".html"})

            Dim sw4 As New StreamWriter(currentDirectory + "history/history.csv", True)
            If online Then
                sw4.WriteLine(formattedTime + "," + currentTimeStamp.ToString + ", Genotype (online)")
            Else
                sw4.WriteLine(formattedTime + "," + currentTimeStamp.ToString + ", Genotype")
            End If
            sw4.Close()
        End If
        info_text = ""

    End Sub

    Private Sub 全选ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 全选ToolStripMenuItem.Click
        For i As Integer = 1 To dtView.Count
            DataGridView1.Rows(i - 1).Cells(0).Value = True
        Next
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 清除ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 清除ToolStripMenuItem.Click
        For i As Integer = 1 To dtView.Count
            DataGridView1.Rows(i - 1).Cells(0).Value = False
        Next
        DataGridView1.RefreshEdit()
    End Sub

    Private Sub 前进ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 前进ToolStripMenuItem.Click
        WebView_main.GoForward()

    End Sub

    Private Sub 后退ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 后退ToolStripMenuItem.Click
        WebView_main.GoBack()
    End Sub

    Private Sub 单倍型网络ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 单倍型网络ToolStripMenuItem.Click
        If dtView.Count >= 1 Then
            Dim selected_count As Integer = 0
            Dim checked As Boolean = True
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    selected_count += 1
                End If

                If IsNumeric(DataGridView1.Rows(i - 1).Cells(5).Value) = False Then
                    checked = False
                    MsgBox("ID:" + DataGridView1.Rows(i - 1).Cells(1).Value.ToString + ", Date:" + DataGridView1.Rows(i - 1).Cells(6).Value + " is not a numerical value.")
                    Exit For
                End If
                If IsNumeric(DataGridView1.Rows(i - 1).Cells(6).Value) = False Then
                    checked = False
                    MsgBox("ID:" + DataGridView1.Rows(i - 1).Cells(1).Value.ToString + ", Quantity:" + DataGridView1.Rows(i - 1).Cells(5).Value + " is not a numerical value.")
                    Exit For
                ElseIf CInt(DataGridView1.Rows(i - 1).Cells(6).Value) <= 0 Then
                    checked = False
                    MsgBox("ID:" + DataGridView1.Rows(i - 1).Cells(1).Value.ToString + ", Quantity:" + DataGridView1.Rows(i - 1).Cells(5).Value + " cannot be less than zero.")
                    Exit For
                End If
            Next
            If checked Then
                If selected_count >= 1 Then
                    TabControl1.SelectedIndex = 0
                    Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/waiting.html"})
                    Dim th1 As New Threading.Thread(AddressOf analysis_network)
                    th1.Start("modified_tcs")
                Else
                    MsgBox("Please select at least one sequence!")
                End If
            End If
        Else
            MsgBox("Please select at least one sequence!")
        End If
    End Sub
    Public Sub analysis_network(ByVal network_type As String)
        info_text = "构建网络……"
        Dim currentTime As DateTime = DateTime.Now
        Dim success As Boolean = True
        Dim currentTimeStamp As Long = (currentTime - New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
        Dim formattedTime As String = currentTime.ToString("yyyy-MM-dd HH:mm")
        Dim in_path As String = root_path + "history\" + currentTimeStamp.ToString + ".fasta"
        Dim sw As New StreamWriter(in_path)
        Dim isaligened As Boolean = True
        Dim epsilon_str As String = ""
        If exe_mode = "advanced" Then
            Select Case network_type
                Case "msn", "mjn"
                    epsilon_str = " -e " + InputBox("Enter the epsilon", "Enter the epsilon", "0")
                Case Else
            End Select
        End If

        For i As Integer = 1 To dtView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                sw.WriteLine(">" + dtView.Item(i - 1).Item(1) + "=" + dtView.Item(i - 1).Item(5) + "=" + dtView.Item(i - 1).Item(4) + "$SPLIT$" + dtView.Item(i - 1).Item(3))
                sw.WriteLine(fasta_seq(i))
                If Len(fasta_seq(i)) <> Len(fasta_seq(1)) Then
                    isaligened = False
                End If
            End If
        Next
        sw.Close()
        PB_value = 10
        Dim out_path As String = root_path + "history\" + currentTimeStamp.ToString + "_aln.fasta"
        If isaligened Then
            File.Copy(in_path, out_path)
        Else
            Dim startInfo As New ProcessStartInfo()
            startInfo.FileName = currentDirectory + "analysis\mafft-win\mafft.bat" ' 替换为实际的命令行程序路径
            startInfo.WorkingDirectory = currentDirectory + "analysis\mafft-win\" ' 替换为实际的运行文件夹路径
            'startInfo.CreateNoWindow = True
            startInfo.Arguments = "--retree 2 --inputorder " + """" + in_path + """" + ">" + """" + out_path + """"
            Dim process As Process = Process.Start(startInfo)
            process.WaitForExit()
            process.Close()
        End If
        PB_value = 30
        If File.Exists(out_path) Then
            Dim encoding As Encoding = DetectFileEncoding(out_path)

            If encoding IsNot Nothing AndAlso Not encoding.Equals(Encoding.UTF8) Then
                ' 读取文件内容
                Dim fileContent As String = File.ReadAllText(out_path, encoding)

                ' 将文件内容以UTF-8编码保存
                File.WriteAllText(out_path, fileContent, Encoding.UTF8)
            End If
            Dim network_app As String = "fastHaN_win_intel.exe"
            If cpu_info.ToUpper.StartsWith("ARM") Then
                network_app = "fastHaN_win_arm.exe"
            End If
            If hap_fasta(out_path, root_path + "history\" + currentTimeStamp.ToString, new_line(False)) Then
                Dim startInfo_hap As New ProcessStartInfo()
                startInfo_hap.FileName = currentDirectory + "analysis\" + network_app ' 替换为实际的命令行程序路径
                startInfo_hap.WorkingDirectory = currentDirectory + "history\" ' 替换为实际的运行文件夹路径
                'startInfo_hap.CreateNoWindow = True
                startInfo_hap.Arguments = network_type + " -i " + """" + root_path + "history\" + currentTimeStamp.ToString + "_seq.phy" + """" + " -o " + """" + root_path + "history\" + currentTimeStamp.ToString + """" + epsilon_str
                Dim process_hap As Process = Process.Start(startInfo_hap)
                process_hap.WaitForExit()
                process_hap.Close()
                PB_value = 50
                If File.Exists(root_path + "history\" + currentTimeStamp.ToString + ".gml") Then
                    Dim startInfo_network As New ProcessStartInfo()
                    startInfo_network.FileName = currentDirectory + "analysis\GenNetworkConfig.exe" ' 替换为实际的命令行程序路径
                    startInfo_network.WorkingDirectory = currentDirectory + "history\" ' 替换为实际的运行文件夹路径
                    'startInfo_network.CreateNoWindow = True
                    startInfo_network.Arguments = currentTimeStamp.ToString + ".gml " + currentTimeStamp.ToString + ".json " + currentTimeStamp.ToString + ".meta " + currentTimeStamp.ToString
                    Dim process_network As Process = Process.Start(startInfo_network)
                    process_network.WaitForExit()
                    process_network.Close()
                    PB_value = 80
                    If File.Exists(root_path + "history\" + currentTimeStamp.ToString + ".js") Then
                        Dim sw0 As New StreamWriter(currentDirectory + "history/" + currentTimeStamp.ToString + ".html")
                        Dim sr1 As New StreamReader(currentDirectory + "main/" + language + "/tcsBU.txt")
                        sw0.Write(sr1.ReadToEnd.Replace("$data$", currentTimeStamp.ToString + ".js"))
                        sr1.Close()
                        sw0.Close()
                        Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "history/" + currentTimeStamp.ToString + ".html"})
                    Else
                        MsgBox("Haplotype network construction failed. Please check the data.")
                        Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/main.html"})
                        success = False
                    End If
                End If
            End If
        Else
            MsgBox("Sequence alignment failed. Please check the sequences.")
            Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/main.html"})
            success = False
        End If
        PB_value = 100
        If success Then
            Dim sw4 As New StreamWriter(currentDirectory + "history/history.csv", True)
            sw4.WriteLine(formattedTime + "," + currentTimeStamp.ToString + "," + network_type.ToUpper + " Network")
            sw4.Close()
        End If
        PB_value = 0
        info_text = ""


    End Sub

    Private Sub DataGridView1_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles DataGridView1.DataBindingComplete
        If data_loaded = False And dtView.Count > 0 Then
            For i As Integer = 1 To dtView.Count
                DataGridView1.Rows(i - 1).Cells(0).Value = True
            Next
        End If

    End Sub

    Private Sub WebView_main_Click(sender As Object, e As EventArgs) Handles WebView_main.Click

    End Sub

    Private Sub MJN单倍型网络ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MJN单倍型网络ToolStripMenuItem.Click
        If dtView.Count >= 1 Then
            Dim selected_count As Integer = 0
            Dim checked As Boolean = True
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    selected_count += 1
                End If

                If IsNumeric(DataGridView1.Rows(i - 1).Cells(5).Value) = False Then
                    checked = False
                    MsgBox("ID:" + DataGridView1.Rows(i - 1).Cells(1).Value.ToString + ", Date:" + DataGridView1.Rows(i - 1).Cells(6).Value + " is not a numerical value.")
                    Exit For
                End If
                If IsNumeric(DataGridView1.Rows(i - 1).Cells(6).Value) = False Then
                    checked = False
                    MsgBox("ID:" + DataGridView1.Rows(i - 1).Cells(1).Value.ToString + ", Quantity:" + DataGridView1.Rows(i - 1).Cells(5).Value + " is not a numerical value.")
                    Exit For
                ElseIf CInt(DataGridView1.Rows(i - 1).Cells(6).Value) <= 0 Then
                    checked = False
                    MsgBox("ID:" + DataGridView1.Rows(i - 1).Cells(1).Value.ToString + ", Quantity:" + DataGridView1.Rows(i - 1).Cells(5).Value + " cannot be less than zero.")
                    Exit For
                End If
            Next
            If checked Then
                If selected_count >= 1 Then
                    TabControl1.SelectedIndex = 0
                    Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/waiting.html"})
                    Dim th1 As New Threading.Thread(AddressOf analysis_network)
                    th1.Start("mjn")
                Else
                    MsgBox("Please select at least one sequence!")
                End If
            End If
        Else
            MsgBox("Please select at least one sequence!")
        End If
    End Sub

    Private Sub MSN单倍型网络ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MSN单倍型网络ToolStripMenuItem.Click
        If dtView.Count >= 1 Then
            Dim selected_count As Integer = 0
            Dim checked As Boolean = True
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    selected_count += 1
                End If

                If IsNumeric(DataGridView1.Rows(i - 1).Cells(5).Value) = False Then
                    checked = False
                    MsgBox("ID:" + DataGridView1.Rows(i - 1).Cells(1).Value.ToString + ", Date:" + DataGridView1.Rows(i - 1).Cells(6).Value + " is not a numerical value.")
                    Exit For
                End If
                If IsNumeric(DataGridView1.Rows(i - 1).Cells(6).Value) = False Then
                    checked = False
                    MsgBox("ID:" + DataGridView1.Rows(i - 1).Cells(1).Value.ToString + ", Quantity:" + DataGridView1.Rows(i - 1).Cells(5).Value + " is not a numerical value.")
                    Exit For
                ElseIf CInt(DataGridView1.Rows(i - 1).Cells(6).Value) <= 0 Then
                    checked = False
                    MsgBox("ID:" + DataGridView1.Rows(i - 1).Cells(1).Value.ToString + ", Quantity:" + DataGridView1.Rows(i - 1).Cells(5).Value + " cannot be less than zero.")
                    Exit For
                End If
            Next
            If checked Then
                If selected_count >= 1 Then
                    TabControl1.SelectedIndex = 0
                    Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/waiting.html"})

                    Dim th1 As New Threading.Thread(AddressOf analysis_network)
                    th1.Start("msn")
                Else
                    MsgBox("Please select at least one sequence!")
                End If
            End If
        Else
            MsgBox("Please select at least one sequence!")
        End If
    End Sub


    Private Sub 序列比对ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 序列比对ToolStripMenuItem.Click
        'DataGridView1.EndEdit()
        'DataGridView1.RefreshEdit()
        'If dtView.Count > 1 Then
        '    Dim selected_count As Integer = 0
        '    For i As Integer = 1 To dtView.Count
        '        If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
        '            selected_count += 1
        '        End If
        '    Next
        '    If selected_count > 1 Then
        '        TabControl1.SelectedIndex = 0
        '        Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/waiting.html"})
        '        Dim th1 As New Threading.Thread(AddressOf analysis_align)
        '        th1.Start("2")
        '    Else
        '        MsgBox("Please select at least two sequences!")
        '    End If
        'End If

    End Sub


    'Private Sub 载入待处理序列ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 载入待处理序列ToolStripMenuItem.Click
    '    Dim opendialog As New OpenFileDialog
    '    opendialog.Filter = "Fasta文件(*.*)|*.fas;*.fasta"
    '    opendialog.FileName = ""
    '    opendialog.Multiselect = False
    '    opendialog.DefaultExt = ".fas"
    '    opendialog.CheckFileExists = True
    '    opendialog.CheckPathExists = True
    '    Dim resultdialog As DialogResult = opendialog.ShowDialog()
    '    If resultdialog = DialogResult.OK Then
    '        TabControl1.SelectedIndex = 2
    '        TextBox6.Text = opendialog.FileName
    '    End If
    'End Sub

    Private Sub 获取序列信息ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 获取序列信息ToolStripMenuItem.Click
        TabControl1.SelectedIndex = 2
        form_config_info.Show()
    End Sub

    Private Sub 清理序列ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 清理序列ToolStripMenuItem.Click
        TabControl1.SelectedIndex = 2
        form_config_clean.Show()

    End Sub

    Private Sub 基于参考序列分型ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 基于参考序列分型ToolStripMenuItem.Click
        If dtView.Count >= 1 Then
            Dim selected_count As Integer = 0
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    selected_count += 1
                End If
            Next
            If selected_count >= 1 Then
                If form_config_type.Visible Then
                    form_config_type.Activate()
                Else

                    form_config_type.Show()
                End If

            Else
                MsgBox("Please select at least one sequence!")
            End If

        Else
            MsgBox("Please select at least one sequence!")
        End If
    End Sub

    Private Sub CSV生成序列ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CSV生成序列ToolStripMenuItem.Click
        form_config_data.Show()
    End Sub

    Private Sub 序列比对高速ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 序列比对高速ToolStripMenuItem.Click

    End Sub

    Private Sub 混合分型分析ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 混合分型分析ToolStripMenuItem.Click
        If dtView.Count >= 1 Then
            Dim selected_count As Integer = 0
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    selected_count += 1
                End If
            Next
            If selected_count >= 1 Then
                form_config_mix.Show()
            Else
                MsgBox("Please select at least one sequence!")
            End If
        Else
            MsgBox("Please select at least one sequence!")
        End If
    End Sub

    Private Sub 合并序列文件ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 合并序列文件ToolStripMenuItem.Click
        form_config_combine.Show()
    End Sub

    Private Sub 导出序列ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 导出序列ToolStripMenuItem.Click
        Dim opendialog As New SaveFileDialog
        opendialog.Filter = "FASTA File (*.fasta)|*.fasta;*.FASTA"
        opendialog.FileName = ""
        opendialog.DefaultExt = ".csv"
        opendialog.CheckFileExists = False
        opendialog.CheckPathExists = True
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            If opendialog.FileName.ToLower.EndsWith(".fasta") Then
                Dim split_sig As String = InputBox("", "Enter the delimiter", "|")
                Dim dw As New StreamWriter(opendialog.FileName, False)
                Dim state_line As String = ""
                For i As Integer = 1 To dtView.Count
                    state_line = ">" + dtView.Item(i - 1).Item(1).ToString.Replace(split_sig, "_")
                    For j As Integer = 4 To DataGridView1.ColumnCount - 1
                        state_line += split_sig + dtView.Item(i - 1).Item(j - 1).ToString.Replace(split_sig, "_")
                    Next
                    dw.WriteLine(state_line)
                    dw.WriteLine(fasta_seq(i))
                Next
                dw.Close()
            End If
            Append_info（"Save Successfully!")
        End If
    End Sub

    Private Sub 导出分型数据集ToolStripMenuItem_Click(sender As Object, e As EventArgs)
        Dim opendialog As New FolderBrowserDialog
        Dim resultdialog = opendialog.ShowDialog
        If resultdialog = DialogResult.OK Then
            Dim split_sig = "|"
            Dim dw As New StreamWriter(opendialog.SelectedPath + "\ref_combine.fasta", False)
            Dim state_line = ""
            For i = 1 To dtView.Count
                state_line = ">" + dtView.Item(i - 1).Item(1).ToString.Replace(split_sig, "_") + split_sig + dtView.Item(i - 1).Item(3).ToString.Replace(split_sig, "_")
                dw.WriteLine(state_line)
                dw.WriteLine(fasta_seq(i))
            Next
            dw.Close()
            Dim th1 As New Thread(AddressOf export_dataset)
            th1.Start(opendialog.SelectedPath)
        End If
    End Sub

    Public Sub export_dataset(ByVal input_folder As String)
        Dim file_type As String = "fas"
        Dim startInfo As New ProcessStartInfo()
        startInfo.FileName = currentDirectory + "analysis\MakeData.exe" ' 替换为实际的命令行程序路径
        startInfo.WorkingDirectory = currentDirectory + "history\" ' 替换为实际的运行文件夹路径
        startInfo.Arguments = "-input " + """" + input_folder + "\ref_combine.fasta" + """" + " -file_type " + file_type + " -out_dir " + """" + input_folder + """"
        Dim process As Process = Process.Start(startInfo)
        process.WaitForExit()
        process.Close()
        MsgBox("Processing completed!")
    End Sub

    Private Sub 日期转换数字ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 日期转换数字ToolStripMenuItem.Click
        Dim splitstr As String = InputBox("", "Enter the delimiter:", "-")

        For i As Integer = 1 To dtView.Count
            If IsNumeric(DataGridView1.Rows(i - 1).Cells(5).Value) = False Then
                Dim tmp_date() As String = DataGridView1.Rows(i - 1).Cells(5).Value.ToString.Split(splitstr)
                If UBound(tmp_date) = 1 Then
                    DataGridView1.Rows(i - 1).Cells(5).Value = (CInt(tmp_date(0)) + CInt(tmp_date(1)) / 12).ToString("F4")
                End If
                If UBound(tmp_date) = 2 Then
                    DataGridView1.Rows(i - 1).Cells(5).Value = (CInt(tmp_date(0)) + CInt(tmp_date(1)) / 12 + CInt(tmp_date(2)) / 30 / 12).ToString("F4")
                End If
            End If
        Next
    End Sub



    Private Sub 增加数据ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 增加数据ToolStripMenuItem.Click

    End Sub


    Private Sub 分割序列文件ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 分割序列文件ToolStripMenuItem.Click
        If dtView.Count >= 1 Then
            Dim selected_count As Integer = 0
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    selected_count += 1
                End If
            Next
            If selected_count >= 1 Then
                form_config_split.Show()
            Else
                MsgBox("Please select at least one sequence!")
            End If

        End If
    End Sub

    Private Sub AutoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AutoToolStripMenuItem.Click
        mafft_align("--auto")
    End Sub
    Public Sub mafft_align(ByVal method As String)
        DataGridView1.EndEdit()
        DataGridView1.RefreshEdit()
        If dtView.Count > 1 Then
            Dim selected_count As Integer = 0
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    selected_count += 1
                End If
            Next
            If selected_count > 1 Then
                TabControl1.SelectedIndex = 0
                Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/waiting.html"})
                Dim th1 As New Threading.Thread(AddressOf do_mafft_align)
                th1.Start(method)
            Else
                MsgBox("Please select at least two sequences!")
            End If
        End If
    End Sub
    Public Sub do_mafft_align(ByVal method As String)
        Dim currentTime As DateTime = DateTime.Now
        Dim currentTimeStamp As Long = (currentTime - New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
        Dim formattedTime As String = currentTime.ToString("yyyy-MM-dd HH:mm")
        Dim in_path As String = root_path + "history\" + currentTimeStamp.ToString + ".fasta"
        Dim sw As New StreamWriter(in_path)
        For i As Integer = 1 To dtView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                sw.WriteLine(">T" + dtView.Item(i - 1).Item(0).ToString)
                sw.WriteLine(fasta_seq(i))
            End If
        Next
        sw.Close()
        PB_value = 10
        Dim out_path As String = root_path + "history\" + currentTimeStamp.ToString + "_tmp.fasta"

        Dim startInfo As New ProcessStartInfo()
        startInfo.FileName = currentDirectory + "analysis\mafft-win\mafft.bat" ' 替换为实际的命令行程序路径
        startInfo.WorkingDirectory = currentDirectory + "analysis\mafft-win\" ' 替换为实际的运行文件夹路径
        startInfo.Arguments = method + " --inputorder " + """" + in_path + """" + ">" + """" + out_path + """"
        Dim process As Process = Process.Start(startInfo)
        process.WaitForExit()
        process.Close()

        If File.Exists(out_path) Then
            Dim encoding As Encoding = DetectFileEncoding(out_path)
            If encoding IsNot Nothing AndAlso Not encoding.Equals(Encoding.UTF8) Then
                ' 读取文件内容
                Dim fileContent As String = File.ReadAllText(out_path, encoding)
                ' 将文件内容以UTF-8编码保存
                File.WriteAllText(out_path, fileContent, Encoding.UTF8)
            End If

            Dim sr As New StreamReader(out_path)
            Dim line As String = sr.ReadLine
            Dim tmp_id As String = ""
            Do
                If line <> "" Then
                    If line(0) = ">" Then
                        tmp_id = line.Substring(2)
                        fasta_seq(Int(tmp_id)) = ""
                    Else
                        fasta_seq(Int(tmp_id)) += line.ToUpper
                    End If
                End If
                line = sr.ReadLine
            Loop Until line Is Nothing
            sr.Close()
            out_path = root_path + "history\" + currentTimeStamp.ToString + "_aln.fasta"
            Dim sw1 As New StreamWriter(out_path)
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    dtView.Item(i - 1).Item(2) = fasta_seq(i)
                    sw1.WriteLine(">" + dtView.Item(i - 1).Item(1))
                    sw1.WriteLine(fasta_seq(i))
                End If
            Next
            sw1.Close()
        Else
            Exit Sub
            MsgBox("Meet errors in align!")
        End If


        PB_value = 100
        Dim sw_align As New StreamWriter(root_path + "history\" + currentTimeStamp.ToString + ".js")
        Dim sr_align As New StreamReader(out_path)
        sw_align.Write("$(document).ready(function () { let data = " + """")
        sw_align.Write(sr_align.ReadToEnd.Replace(vbCrLf, "\n").Replace(vbCr, "\n").Replace(vbLf, "\n"))
        sr_align.Close()
        sw_align.Write("""" + ";" + vbCrLf)
        sw_align.Write("loadNewMSA(data);" + vbCrLf)
        sw_align.Write("});")
        sw_align.Close()
        If File.Exists(root_path + "history\" + currentTimeStamp.ToString + ".js") Then
            Dim sw0 As New StreamWriter(currentDirectory + "history/" + currentTimeStamp.ToString + ".html")
            Dim sr1 As New StreamReader(currentDirectory + "main/" + language + "/alignmentviewer.txt")
            sw0.Write(sr1.ReadToEnd.Replace("$data$", currentTimeStamp.ToString + ".js"))
            sr1.Close()
            sw0.Close()
            Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "history/" + currentTimeStamp.ToString + ".html"})
        End If
        PB_value = 0
        Dim sw4 As New StreamWriter(currentDirectory + "history/history.csv", True)
        sw4.WriteLine(formattedTime + "," + currentTimeStamp.ToString + ", Alignment")
        sw4.Close()
        PB_value = 0
    End Sub

    Private Sub FFTNS1VeryFastButVeryRoughToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FFTNS1VeryFastButVeryRoughToolStripMenuItem.Click
        mafft_align("--retree 1")

    End Sub

    Private Sub FFTNS2FastButRoughToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FFTNS2FastButRoughToolStripMenuItem.Click
        mafft_align("--retree 2")

    End Sub

    Private Sub GINSiVerySlowToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GINSiVerySlowToolStripMenuItem.Click
        mafft_align("--globalpair --maxiterate 16")
    End Sub

    Private Sub LINSiMostAccurateVerySlowToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LINSiMostAccurateVerySlowToolStripMenuItem.CheckedChanged

    End Sub

    Private Sub EINSiForLongUnalignableRegionsVerySlowToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EINSiForLongUnalignableRegionsVerySlowToolStripMenuItem.Click
        mafft_align("--genafpair  --maxiterate 16")

    End Sub
    Private Sub PPPAlgorithmToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PPPAlgorithmToolStripMenuItem.Click
        muscle_align("-align")
    End Sub
    Public Sub muscle_align(ByVal method As String)
        DataGridView1.EndEdit()
        DataGridView1.RefreshEdit()
        If dtView.Count > 1 Then
            Dim selected_count As Integer = 0
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    selected_count += 1
                End If
            Next
            If selected_count > 1 Then
                TabControl1.SelectedIndex = 0
                Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "main/" + language + "/waiting.html"})
                Dim th1 As New Threading.Thread(AddressOf do_muscle_align)
                th1.Start(method)
            Else
                MsgBox("Please select at least two sequences!")
            End If
        End If
    End Sub
    Public Sub do_muscle_align(ByVal method As String)
        Dim currentTime As DateTime = DateTime.Now
        Dim currentTimeStamp As Long = (currentTime - New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
        Dim formattedTime As String = currentTime.ToString("yyyy-MM-dd HH:mm")
        Dim in_path As String = root_path + "history\" + currentTimeStamp.ToString + ".fasta"
        Dim sw As New StreamWriter(in_path)
        For i As Integer = 1 To dtView.Count
            If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                sw.WriteLine(">T" + dtView.Item(i - 1).Item(0).ToString)
                sw.WriteLine(fasta_seq(i))
            End If
        Next
        sw.Close()
        PB_value = 10
        Dim out_path As String = root_path + "history\" + currentTimeStamp.ToString + "_tmp.fasta"

        Dim startInfo As New ProcessStartInfo()
        startInfo.FileName = currentDirectory + "analysis\muscle5.1.win64.exe" ' 替换为实际的命令行程序路径
        startInfo.WorkingDirectory = currentDirectory + "analysis\" ' 替换为实际的运行文件夹路径
        'startInfo.CreateNoWindow = True
        startInfo.Arguments = method + " " + """" + in_path + """" + " -output " + """" + out_path + """"
        Dim process As Process = Process.Start(startInfo)
        process.WaitForExit()
        process.Close()

        If File.Exists(out_path) Then
            Dim encoding As Encoding = DetectFileEncoding(out_path)
            If encoding IsNot Nothing AndAlso Not encoding.Equals(Encoding.UTF8) Then
                ' 读取文件内容
                Dim fileContent As String = File.ReadAllText(out_path, encoding)
                ' 将文件内容以UTF-8编码保存
                File.WriteAllText(out_path, fileContent, Encoding.UTF8)
            End If

            Dim sr As New StreamReader(out_path)
            Dim line As String = sr.ReadLine
            Dim tmp_id As String = ""
            Do
                If line <> "" Then
                    If line(0) = ">" Then
                        tmp_id = line.Substring(2)
                        fasta_seq(Int(tmp_id)) = ""
                    Else
                        fasta_seq(Int(tmp_id)) += line.ToUpper
                    End If
                End If
                line = sr.ReadLine
            Loop Until line Is Nothing
            sr.Close()
            out_path = root_path + "history\" + currentTimeStamp.ToString + "_aln.fasta"
            Dim sw1 As New StreamWriter(out_path)
            For i As Integer = 1 To dtView.Count
                If DataGridView1.Rows(i - 1).Cells(0).FormattedValue.ToString = "True" Then
                    dtView.Item(i - 1).Item(2) = fasta_seq(i)
                    sw1.WriteLine(">" + dtView.Item(i - 1).Item(1))
                    sw1.WriteLine(fasta_seq(i))
                End If
            Next
            sw1.Close()
        Else
            Exit Sub
            MsgBox("Met errors in align!")
        End If


        PB_value = 100
        Dim sw_align As New StreamWriter(root_path + "history\" + currentTimeStamp.ToString + ".js")
        Dim sr_align As New StreamReader(out_path)
        sw_align.Write("$(document).ready(function () { let data = " + """")
        sw_align.Write(sr_align.ReadToEnd.Replace(vbCrLf, "\n").Replace(vbCr, "\n").Replace(vbLf, "\n"))
        sr_align.Close()
        sw_align.Write("""" + ";" + vbCrLf)
        sw_align.Write("loadNewMSA(data);" + vbCrLf)
        sw_align.Write("});")
        sw_align.Close()
        If File.Exists(root_path + "history\" + currentTimeStamp.ToString + ".js") Then
            Dim sw0 As New StreamWriter(currentDirectory + "history/" + currentTimeStamp.ToString + ".html")
            Dim sr1 As New StreamReader(currentDirectory + "main/" + language + "/alignmentviewer.txt")
            sw0.Write(sr1.ReadToEnd.Replace("$data$", currentTimeStamp.ToString + ".js"))
            sr1.Close()
            sw0.Close()
            Me.Invoke(set_web_main_url, New Object() {"file:///" + currentDirectory + "history/" + currentTimeStamp.ToString + ".html"})
        End If
        PB_value = 0
        Dim sw4 As New StreamWriter(currentDirectory + "history/history.csv", True)
        sw4.WriteLine(formattedTime + "," + currentTimeStamp.ToString + ", Alignment")
        sw4.Close()
        PB_value = 0
    End Sub

    Private Sub Super5AlgorithmToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles Super5AlgorithmToolStripMenuItem.Click
        muscle_align("-super5")

    End Sub


    Private Sub EnglishToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EnglishToolStripMenuItem.Click
        If language = "EN" Then
            to_ch()
        Else
            to_en()
        End If
        settings("language") = language
    End Sub

    Private Sub HIV序列构建ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles HIV序列构建ToolStripMenuItem.Click

        form_config_barcode.Show()
    End Sub

    Private Sub 单样本鉴定ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 单样本鉴定ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "Sequence data|*.fas;*.fasta;*.fa;*.fq;*.fastq;*.fq.gz;*.fastq.gz",
            .FileName = "",
            .Multiselect = True,
            .DefaultExt = ".fq",
            .CheckFileExists = True,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            info_text = "清理数据……"
            timer_id = 7
            waiting = True
            add_data = False
            DeleteDir(Path.Combine(currentDirectory, "temp", "temp_quick"))
            Directory.CreateDirectory(Path.Combine(currentDirectory, "temp", "temp_quick"))

            DeleteDir(Path.Combine(currentDirectory, "temp", "temp_data"))
            Directory.CreateDirectory(Path.Combine(currentDirectory, "temp", "temp_data"))

            ReDim raw_names(opendialog.FileNames.Length - 1)
            Dim temp_i As Integer = 0
            For Each seq_file In opendialog.FileNames
                ' 获取目标文件路径
                Dim targetPath As String = Path.Combine(currentDirectory, "temp", "temp_data", Path.GetFileName(seq_file))
                raw_names(temp_i) = Path.GetFileName(seq_file)
                temp_i += 1
                File.Copy(seq_file, targetPath, True) ' True表示如果目标文件已存在，则覆盖
            Next

            Dim th1 As New Thread(AddressOf quick_barcode)
            th1.Start(9)
        End If
    End Sub
    Public Sub quick_barcode(ByVal next_pid As Integer)
        Try
            info_text = "分解数据……"
            PB_value = 11
            Directory.CreateDirectory(Path.Combine(currentDirectory, "temp", "temp_refs"))
            clean_fasta_file(Path.Combine(currentDirectory, "analysis", "database", "barcode.fasta"), Path.Combine(currentDirectory, "temp", "temp_refs", "barcode.fasta"))
            clean_fasta_file(Path.Combine(currentDirectory, "analysis", "database", "HIV_ref.fasta"), Path.Combine(currentDirectory, "temp", "temp_refs", "barcode_refs.fasta"))

            PB_value = 33
            Dim SI_split_barcode As New ProcessStartInfo With {
                    .FileName = Path.Combine(currentDirectory, "analysis", "split_barcode.exe"),
                    .WorkingDirectory = Path.Combine(currentDirectory, "temp"),
                    .CreateNoWindow = create_no_window,
                    .Arguments = "-i " + """" + Path.Combine(currentDirectory, "temp", "temp_data") + """" +
                    " -r " + """" + ".\temp_refs\barcode.fasta" + """" +
                    " -o " + """" + Path.Combine(currentDirectory, "temp", "temp_quick") + """" +
                    " -p " + current_thread.ToString +
                    " -w 7"
                }
            Dim process_split_barcode As Process = Process.Start(SI_split_barcode)
            process_split_barcode.WaitForExit()
            process_split_barcode.Close()
            PB_value = 66
            info_text = "构建分型……"

            Dim SI_build_barcode As New ProcessStartInfo With {
                .FileName = Path.Combine(currentDirectory, "analysis", "build_barcode.exe"),
                .WorkingDirectory = Path.Combine(currentDirectory, "temp"),
                .CreateNoWindow = create_no_window,
                .Arguments = "-i " + """" + Path.Combine(currentDirectory, "temp", "temp_quick", "clean_data") + """" +
                " -r " + """" + ".\temp_refs\barcode_refs.fasta" + """" +
                " -o " + """" + Path.Combine(currentDirectory, "temp", "temp_quick") + """" +
                " -p " + current_thread.ToString +
                " -m 0 -l 4"
            }
            Dim process_build_barcode As Process = Process.Start(SI_build_barcode)
            process_build_barcode.WaitForExit()
            process_build_barcode.Close()
            PB_value = 100
            'Dim result As DialogResult = MessageBox.Show("Analysis has been completed. Would you like to view the results file?", "Confirm Operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

            '' 根据用户的选择执行相应的操作
            'If result = DialogResult.Yes Then
            '    Process.Start("explorer.exe", """" + Path.Combine(currentDirectory, "temp", "temp_quick") + """")
            'End If
        Catch ex As Exception
            MsgBox（"程序遇到了错误，请检查日志文件。"）
        End Try
        info_text = ""
        PB_value = 0
        timer_id = next_pid
    End Sub

    Private Sub 导出报告ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 导出报告ToolStripMenuItem.Click
        Dim saveFileDialog As New SaveFileDialog()
        saveFileDialog.Filter = "HTML files (*.html)|*.html|PDF files (*.pdf)|*.pdf"
        saveFileDialog.Title = "保存报告"
        saveFileDialog.DefaultExt = "html"
        If saveFileDialog.ShowDialog() = DialogResult.OK Then
            Dim fileExtension As String = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower()
            ' 根据文件扩展名执行不同的保存操作
            If fileExtension = ".pdf" Then
                SaveWebPageAsPdf(saveFileDialog.FileName)
            ElseIf fileExtension = ".html" Then
                SaveWebPageAshtml(saveFileDialog.FileName)
            End If
        End If
    End Sub

    Private Sub 中国HIV分型ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles 中国HIV分型ToolStripMenuItem1.Click
        TabControl1.SelectedIndex = 1
        DataGridView1.EndEdit()
        data_loaded = False
        add_data = True
        Dim th1 As New Threading.Thread(AddressOf load_csv_data)
        th1.Start(currentDirectory + "analysis\database\HIV_ch.csv")

    End Sub

    Private Sub 用户数据ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 用户数据ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog
        Select Case data_type
            Case "gb"
                opendialog.Filter = "GenBank File|*.gb"
            Case Else
                opendialog.Filter = "fasta File(*.fasta)|*.fas;*.fasta"
        End Select
        opendialog.FileName = ""
        opendialog.Multiselect = False
        opendialog.DefaultExt = ".fas"
        opendialog.CheckFileExists = True
        opendialog.CheckPathExists = True
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            current_file = opendialog.FileName
            add_data = True
            TabControl1.SelectedIndex = 1
            data_loaded = False
            If opendialog.FileName.ToLower.EndsWith(".gb") Then
                Dim startInfo As New ProcessStartInfo()
                startInfo.FileName = currentDirectory + "analysis\build_gb.exe" ' 替换为实际的命令行程序路径
                startInfo.WorkingDirectory = currentDirectory + "history\" ' 替换为实际的运行文件夹路径
                startInfo.CreateNoWindow = create_no_window
                startInfo.Arguments = "-input " + """" + current_file + """" + " -outdir " + """" + "out_gb" + """" + " -clean false"
                Dim process As Process = Process.Start(startInfo)
                process.WaitForExit()
                process.Close()
                load_csv_data(currentDirectory + "history\out_gb\gb_info.csv")
            Else
                form_config_stand.Show()
            End If
        End If
    End Sub

    Private Sub 多样本分子网络ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 多样本分子网络ToolStripMenuItem.Click
        Dim opendialog As New OpenFileDialog With {
            .Filter = "Sequence data|*.fas;*.fasta;*.fa;*.fq;*.fastq;*.fq.gz;*.fastq.gz",
            .FileName = "",
            .Multiselect = True,
            .DefaultExt = ".fq",
            .CheckFileExists = True,
            .CheckPathExists = True
        }
        Dim resultdialog As DialogResult = opendialog.ShowDialog()
        If resultdialog = DialogResult.OK Then
            timer_id = 7
            waiting = True
            add_data = False
            DeleteDir(Path.Combine(currentDirectory, "temp", "temp_quick"))
            Directory.CreateDirectory(Path.Combine(currentDirectory, "temp", "temp_quick"))

            DeleteDir(Path.Combine(currentDirectory, "temp", "temp_data"))
            Directory.CreateDirectory(Path.Combine(currentDirectory, "temp", "temp_data"))

            For Each seq_file In opendialog.FileNames
                ' 获取目标文件路径
                Dim targetPath As String = Path.Combine(currentDirectory, "temp", "temp_data", Path.GetFileName(seq_file))
                File.Copy(seq_file, targetPath, True) ' True表示如果目标文件已存在，则覆盖
            Next
            form_main.TextBox1.Text = "分析进行中……"
            Dim th1 As New Thread(AddressOf quick_barcode)
            th1.Start(10)
        End If
    End Sub

    Private Sub 隐藏命令行窗口ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 隐藏命令行窗口ToolStripMenuItem.Click
        隐藏命令行窗口ToolStripMenuItem.Checked = 隐藏命令行窗口ToolStripMenuItem.Checked Xor True
        create_no_window = 隐藏命令行窗口ToolStripMenuItem.Checked
    End Sub

    Private Sub 分析结果列表ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles 分析结果列表ToolStripMenuItem.Click
        If File.Exists(currentDirectory + "history/history.csv") = False Then
            File.Create(currentDirectory + "history/history.csv").Close()
        End If
        Dim sw As New StreamWriter(currentDirectory + "history/history.html")
        Dim sr1 As New StreamReader(currentDirectory + "main/" + language + "/header_history.txt")
        sw.Write(sr1.ReadToEnd)
        sr1.Close()
        Dim sr2 As New StreamReader(currentDirectory + "history/history.csv")
        Dim lines() As String = sr2.ReadToEnd.Split(vbCrLf)

        For Each line In lines.Reverse
            If line <> "" Then
                Do
                    Dim line_list As String() = line.Split(",")
                    sw.WriteLine("<tr><td>" + line_list(0) + "</td><td>" + line_list(1) + "</td><td>" + line_list(2) + "</td> ")
                    If line_list(2).Contains("Network") Then
                        If language = "EN" Then
                            sw.WriteLine("<td><a href='./" + line_list(1) + ".html'>View Results</a>")
                            sw.WriteLine("<a href='./" + line_list(1) + "_seq.phy' target='_new'>Original Sequence</a> ")
                            sw.WriteLine("<a href='./" + line_list(1) + "_seq_trait.csv'  target='_new'>Trait Matrix</a>")
                            sw.WriteLine("<a href='./" + line_list(1) + "_hap.phy' target='_new'>Haplotype Sequence</a> ")
                            sw.WriteLine("<a href='./" + line_list(1) + "_hap_trait.csv'  target='_new'>Haplotype Trait Matrix</a>")
                            sw.WriteLine("<a href='./" + line_list(1) + "_seq2hap.csv'  target='_new'>Haplotype Source</a></td></tr>")
                        Else
                            sw.WriteLine("<td><a href='./" + line_list(1) + ".html'>查看</a>")
                            sw.WriteLine("<a href='./" + line_list(1) + "_seq.phy' target='_new'>原始序列</a> ")
                            sw.WriteLine("<a href='./" + line_list(1) + "_seq_trait.csv'  target='_new'>性状矩阵</a>")
                            sw.WriteLine("<a href='./" + line_list(1) + "_hap.phy' target='_new'>单倍型序列</a> ")
                            sw.WriteLine("<a href='./" + line_list(1) + "_hap_trait.csv'  target='_new'>单倍型性状矩阵</a>")
                            sw.WriteLine("<a href='./" + line_list(1) + "_seq2hap.csv'  target='_new'>单倍型来源</a></td></tr>")
                        End If
                    ElseIf line_list(2).Contains("Alignment") Then
                        If language = "EN" Then
                            sw.WriteLine("<td><a href='./" + line_list(1) + ".html'>View Results</a>")
                            sw.WriteLine("<a href='./" + line_list(1) + "_aln.fasta'  target='_new'>Sequence Alignment</a></td></tr>")
                        Else
                            sw.WriteLine("<td><a href='./" + line_list(1) + ".html'>查看</a>")
                            sw.WriteLine("<a href='./" + line_list(1) + "_aln.fasta'  target='_new'>多序列比对</a></td></tr>")
                        End If
                    ElseIf line_list(2).Contains("Genotype") Then
                        If language = "EN" Then
                            sw.WriteLine("<td><a href='./" + line_list(1) + ".html'>View Results</a>")
                            sw.WriteLine("<a href='./" + line_list(1) + ".csv'  target='_new'>Genotyping Results</a> ")
                            If File.Exists(currentDirectory + "history/" + "_null.csv") Then
                                sw.WriteLine("<a href='./" + line_list(1) + "_null.csv'  target='_new'>Failed Sequences</a> ")
                            End If
                            If File.Exists(currentDirectory + "history/" + line_list(1) + ".gz") Then
                                sw.WriteLine("<a href='./" + line_list(1) + ".gz'  target='_new'>Genotyping Database</a></td></tr>")
                            Else
                                sw.WriteLine("</tr>")
                            End If
                        Else
                            sw.WriteLine("<td><a href='./" + line_list(1) + ".html'>查看</a>")
                            sw.WriteLine("<a href='./" + line_list(1) + ".csv'  target='_new'>分型结果</a> ")
                            If File.Exists(currentDirectory + "history/" + "_null.csv") Then
                                sw.WriteLine("<a href='./" + line_list(1) + "_null.csv'  target='_new'>失败的序列</a> ")
                            End If
                            If File.Exists(currentDirectory + "history/" + line_list(1) + ".gz") Then
                                sw.WriteLine("<a href='./" + line_list(1) + ".gz'  target='_new'>分型数据库</a></td></tr>")
                            Else
                                sw.WriteLine("</tr>")
                            End If
                        End If

                    ElseIf line_list(2).Contains("Primer") Then
                        If language = "EN" Then
                            sw.WriteLine("<td><a href='" + line_list(3) + "'  target='_new'>Primer Output</a></td></tr>")
                        Else
                            sw.WriteLine("<td><a href='" + line_list(3) + "'  target='_new'>引物</a></td></tr>")
                        End If
                    Else
                        If language = "EN" Then
                            sw.WriteLine("<td><a href='./" + line_list(1) + ".html'>View Results</a>")
                            sw.WriteLine("<a Then href='./" + line_list(1) + ".0.json' target='_new'>json</a> ")
                            sw.WriteLine("<a href='./" + line_list(1) + ".fasta'  target='_new'>Sequence</a></td></tr>")
                        Else
                            sw.WriteLine("<td><a href='./" + line_list(1) + ".html'>查看</a>")
                            sw.WriteLine("<a Then href='./" + line_list(1) + ".0.json' target='_new'>json</a> ")
                            sw.WriteLine("<a href='./" + line_list(1) + ".fasta'  target='_new'>序列</a></td></tr>")
                        End If
                    End If
                    line = sr2.ReadLine

                Loop Until line Is Nothing
            End If
        Next

        sr2.Close()
        Dim sr3 As New StreamReader(currentDirectory + "main/" + language + "/footer_history.txt")
        sw.Write(sr3.ReadToEnd)
        sw.Close()

        WebView_main.Source = New Uri("file:///" + currentDirectory + "/history/history.html")
    End Sub
End Class
