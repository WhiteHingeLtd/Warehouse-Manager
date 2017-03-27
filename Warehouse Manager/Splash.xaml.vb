Imports System.ComponentModel
Imports System.Reflection
Imports System.Threading
Imports WHLClasses
Imports WHLClasses.Authentication

Public Class Splash
    Dim Worker as new System.ComponentModel.BackgroundWorker
    Friend HomeRef as MainWindow
    Friend Sub SplashLoad()
        AddHandler worker.DoWork, addressof SplashProxy
        AddHandler worker.ProgressChanged, addressof proxytextupdate
        AddHandler Worker.RunWorkerCompleted, AddressOf ProxyFinished
        worker.WorkerReportsProgress = true
        worker.RunWorkerAsync()
    End Sub

    Private Sub LoadAssemblies(Assembly As Assembly)
        For each name As AssemblyName in Assembly.GetReferencedAssemblies
            worker.ReportProgress(0,"Loading " + name.Name.ToString())
            thread.Sleep(25)
            if Not AppDomain.CurrentDomain.GetAssemblies.Any(function (x As assembly)x.FullName = name.FullName)
                LoadAssemblies(Assembly.load(name))
            End If
            
        Next
    End Sub

    Friend sub SplashProxy(sender as Object, e As DoWorkEventArgs)
        Worker.ReportProgress(0,"Loading Assemblies")
        LoadAssemblies(Me.GetType.Assembly)
        dim loader as new GenericDataController
        Worker.ReportProgress(0,"Loading Item Data")
        'HomeRef.Data_Skus = loader.SmartSkuCollLoad()
        Worker.ReportProgress(0,"Loading Employee Data")
        homeref.Data_Employees = New EmployeeCollection()
        Worker.ReportProgress(0,"Preparing Authentication")
        homeref.User_Employee = New AuthClass
        
    End sub

    Private Sub UpdateText(NewText As String)
        LoadingStatsText.Text += vbNewLine + NewText
    End Sub

    Friend Sub ProxyTextUpdate(sender As Object, e As Progresschangedeventargs)
        UpdateText(e.UserState.tostring)
    End Sub
    
    Friend Sub ProxyFinished()
        UpdateText("Requesting User Login")
        'HomeRef.User_Employee.Authorise()
        'UpdateText("Logged in: " + homeref.User_Employee.AuthenticatedUser.FullName)
        me.Close()
    End Sub

    Private Sub ListingManagerSplash_Loaded(sender As Object, e As RoutedEventArgs) Handles ListingManagerSplash.Loaded
        splashload()
    End Sub
End Class
