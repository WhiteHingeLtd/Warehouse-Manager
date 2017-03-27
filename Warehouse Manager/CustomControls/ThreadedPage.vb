Imports System.ComponentModel
Imports System.Windows.Threading

Public Class ThreadedPage
    Inherits Page

    Friend Overridable function SupportsMultipleTabs() As boolean
        return true
    End function

    Friend Timer as DispatcherTimer
    Friend Worker as BackgroundWorker
    dim Status as textblock
    dim ClockBlock as textblock
    
    Friend Sub New()
        
        ' Add any initialization after the InitializeComponent() call.
        Timer  = New DispatcherTimer(New TimeSpan(0,0,0,1), DispatcherPriority.Normal, AddressOf TimerTick, mybase.Dispatcher)
        SetUpWorker
    End Sub

    Private Sub SetUpWorker()
        Worker = new BackgroundWorker
        Worker.WorkerReportsProgress = true
        AddHandler Worker.DoWork, AddressOf WorkerHandler
        AddHandler Worker.ProgressChanged, Addressof WorkerProgress
    End Sub

    Public Sub TimerTick()
        If ClockBlock is nothing then ClockBlock=Me.Template.FindName("ClockBlock", Me)
        ClockBlock.text = now.ToString("HH:mm:ss")
    End Sub

    Friend Sub UpdateStatus(NewStatus as string)
        If Status is nothing then Status=Me.Template.FindName("StatusBlock", Me)
        
        Status.Text = NewStatus
        'StatusText..text = NewStatus
    End Sub

    
    Dim WorkerRunning as Boolean = false
    Friend Sub ProcessInBackground(Process As Action)
        WorkerRunning = false
        worker.RunWorkerAsync(Process)
        While Worker.IsBusy() or WorkerRunning
            Forms.Application.DoEvents()
            Threading.Thread.Sleep(16)
        End While
    End Sub

    Private Sub WorkerHandler(sender as BackgroundWorker, e As DoWorkEventArgs)
        DirectCast(e.Argument, Action).Invoke()
        WorkerRunning = false
    End Sub
    
    Friend Overridable Sub WorkerProgress(sender As Object, e As ProgressChangedEventArgs)
        UpdateStatus(e.UserState.tostring)
    End Sub

End Class

