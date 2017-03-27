Imports System.Windows.Controls.Ribbon
Imports WHLClasses
Imports WHLClasses.Authentication
Imports WHLClasses.Linnworks.Auth

Class MainWindow
    Inherits RibbonWindow

#Region "Vars"
    'Data Object Collections
    Friend Data_Skus As SkuCollection
    Friend Data_Employees As EmployeeCollection

    'User related objects
    Friend User_Employee As AuthClass
#End Region

#Region "Startup and runtime"

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        UpdateShellColor()
        AddHandler SystemParameters.StaticPropertyChanged, AddressOf UpdateShellColor
    End Sub

    Private Sub UpdateShellColor()
        MainRibbon.Background = New SolidColorBrush(Color.FromRgb((SystemParameters.WindowGlassColor.R + 510) / 3, (SystemParameters.WindowGlassColor.G + 510) / 3, (SystemParameters.WindowGlassColor.B + 510) / 3))
    End Sub

    Private Sub RibbonWindow_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        Dim Splash As New Splash
        Splash.HomeRef = Me
        Splash.ShowDialog()
        Me.Title = "Warehouse Manager       [" + User_Employee.AuthenticatedUser.FullName + "]"
    End Sub

#End Region

#Region "MainZone"

#End Region

#Region "App Menu"



#End Region

#Region "Ribbon"

    Private Sub Views_SalesDashboardButton_Click(sender As Object, e As RoutedEventArgs) Handles RoutePlannerButton.Click
        NewTab(New RoutePlanner)
    End Sub

#End Region

#Region "Methods"
    Private Sub NewTab(Control As ThreadedPage)
        Dim Tab As New PageFrameTab(Control)
        If Control.SupportsMultipleTabs then
            MainWindowTabControl.Items.Add(Tab)
            Tab.Focus()
            MainWindowTabControl.SelectedItem = Tab
        else
            'Check if the tab has already been opened
            dim Exists as Boolean = false
            dim ExistingRef as PageFrameTab
            For each Existing As Object in MainWindowTabControl.Items
                try
                    Dim TExisting as PageFrameTab = Existing
                    if Control.GetType = TExisting.GetChildType() Then
                        Exists = true
                        ExistingRef = TExisting
                    End If
                Catch ex As Exception

                End Try
                
            Next
            If not exists then
                MainWindowTabControl.Items.Add(Tab)
                Tab.Focus()
                MainWindowTabControl.SelectedItem = Tab
            Else 
                ExistingRef.focus
                MainWindowTabControl.SelectedItem = ExistingRef
            End If
        End If

        

    End Sub
#End Region
End Class
