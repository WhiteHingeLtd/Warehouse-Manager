Imports System.Windows.Media.Animation
Imports Microsoft.VisualBasic.CompilerServices
Imports WHLClasses

Class RoutePlanner
    Friend Overrides Function SupportsMultipleTabs() As Boolean
        Return False
    End Function

    Dim RouteTable as List(of Dictionary(Of String,Object))
    Private Sub GetRouteButton_Click(sender As Object, e As RoutedEventArgs)
        if not UnsavedChanges>0 then
            LoadRouteData
        Else 
            Dim result as MsgBoxResult=MsgBox("You have unsaved changes to your route! Would you like to save them first?", MsgBoxStyle.YesNoCancel, "Route Planner")
            If result=MsgBoxResult.Yes Then
                UpdateStatus("Saving Changes.")
                SaveRouteData
                UpdateStatus("Changes Saved.")
                LoadRouteData
            Elseif result=MsgBoxResult.No Then
                UpdateStatus("Unsaved Changes Discarded.")
                LoadRouteData

            Else
                UpdateStatus("Operation Aborted.")
            End If
        End If
    End Sub

    Private Sub SaveRouteData()
        UpdateStatus("Saving Data...")
        dim tempKids as New List(Of RoutePlannerData)
        For each thing As Textblock in RouteList.Children
            tempKids.Add(thing.tag)
        Next
        ProcessInBackground(Sub()
                                dim Index as Integer = 0
                                For each RouteItem as RoutePlannerData in tempKids
                                    Index += 1
                                    Worker.ReportProgress(0,"Saving Route: " + RouteItem.RouteBlockName+ " ("+FormatPercent(Index/tempKids.Count,0)+")")
                                    dim Query as String = "UPDATE whldata.location_routing SET RouteIndex="+RouteItem.RouteIndex.ToString()+" WHERE RouteID="+RouteItem.RouteID.ToString()+";"
                                    insertUpdate(Query)
                                    
                                Next
                            End Sub)

        UpdateStatus("Data saved.")
    End Sub

    Dim RouteIndexes as new List(Of Integer)


    Private Sub LoadRouteData()
        UpdateStatus("Waiting for response from database, Please Wait...")
        dim WarehouseFilters as new List(Of String)
        For each child as CheckBox in WarehouseFilterStack.Children
            if child.IsChecked then
                WarehouseFilters.Add("WarehouseID="+child.Tag.ToString())
            End If
        Next
        If WarehouseFilters.Count > 0 Then
           
            dim Query as String = "SELECT * FROM whldata.location_routing WHERE ("+String.Join(" OR ",WarehouseFilters)+") ORDER BY RouteIndex;"
            ProcessInBackground(Sub()
                                    RouteTable = SelectDataDictionary(Query)
                                    Worker.ReportProgress(0,"Done.")
                                    End Sub)
            RouteList.Children.Clear()
            UnsavedChanges = 0
            UpdateUnsavedChanges()
            UpdateStatus("Loading Table...")
            Routeindexes.Clear()
            For each Entry as Dictionary(of String, Object) in RouteTable
            
                Dim TB As new TextBlock()
                TB.FontSize = 18
                TB.Padding=New Thickness(4)
                TB.Margin=New Thickness(1)
                TB.Background = Brushes.LightBlue
                Dim Data as New RoutePlannerData
                Data.RouteBlockName = entry("RouteBlockName")
                Data.RouteID = entry("RouteID")
                Data.RouteIndex = entry("RouteIndex")
                Data.WarehouseID = entry("WarehouseID")
                Data.ZoneID = entry("ZoneID")
                Data.TempID = UnsavedChanges
                TB.Tag = Data
                TB.HorizontalAlignment = HorizontalAlignment.Stretch
                TB.VerticalAlignment = VerticalAlignment.Top
                TB.Text = DirectCast(TB.Tag,routeplannerdata).RouteBlockName + "         ["  + DirectCast(TB.Tag,routeplannerdata).RouteIndex.ToString() + "]"
                AddHandler TB.Mouseleftbuttondown, AddressOf Item_MouseDown
                AddHandler TB.mouseleftbuttonup, AddressOf Item_MouseUp
                AddHandler TB.MouseMove, AddressOf Item_MouseMove
                AddHandler TB.MouseWheel, AddressOf Item_Scroll
                RouteList.Children.Add(TB)
                RouteIndexes.Add(data.RouteIndex)
                UnsavedChanges += 1
            Next

            UnsavedChanges=0

            UpdateStatus("Done")
        Else
            UpdateStatus("Operation Cancelled")
            MessageBox.Show(MainWindowRef,"You must select at least one warehouse to view route data for.","Route Planner",messageboxbutton.OK,MessageBoxImage.Error)
        End If
    End Sub

    Private Sub RoutePlanner_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        UpdateUnsavedChanges()
        
        UpdateStatus("Loading Warehouses")
        'Get the Warehouses to fill the warehouse stack panel
        dim Warehouses as List(Of Dictionary(Of String,Object)) = SelectDataDictionary("SELECT * FROM whldata.warehousereference;")
        For each Warehouse As Dictionary(Of String, Object) in Warehouses
            Dim WHFilter as New CheckBox 
            WHFilter.Content = Warehouse("WarehouseReferencecol")
            WHFilter.Tag = Warehouse("WarehouseID")
            WHFilter.IsChecked = True
            WHFilter.Margin = new Thickness(2)
            WarehouseFilterStack.Children.Add(WHFilter)
        Next 

        UpdateStatus("Loading Routes")
        LoadRouteData()

        UpdateStatus("Done")
    End Sub
    
    Dim UnsavedChanges as Integer = 0
    
    Friend Class RoutePlannerData
        Friend RouteID as Integer = 0
        Friend RouteIndex as Integer = 0
        Friend RouteBlockName as String = ""
        Friend WarehouseID as Integer = 0
        Friend ZoneID as integer = 0

        Friend TempID as integer = 0
    End Class

    Private Sub ApplyNewOrder
        UpdateStatus("Applying Changes...")
        Dim CurrentIndex As Integer = 0
        RouteIndexes.Sort()
        For each Item as textblock In RouteList.Children
            DirectCast(Item.Tag,routeplannerdata).RouteIndex = RouteIndexes(CurrentIndex)
            Item.Text = DirectCast(Item.Tag,routeplannerdata).RouteBlockName + "         ["  + DirectCast(Item.Tag,routeplannerdata).RouteIndex.ToString() + "]"
            Item.Background = brushes.LightBlue

            CurrentIndex += 1
        Next
        
        UpdateStatus("Changes have been applied on this PC only. Press ""Save Globally"" to save the changes to the system")
    End Sub

    Private Sub Item_MouseDown(sender As TextBlock, e As MouseEventArgs)
        sender.CaptureMouse()
        sender.BringIntoView()
        Try
            Routelist.Children.Remove(sender)
            sender.HorizontalAlignment = HorizontalAlignment.Left
            sender.Background = brushes.Aqua
            sender.Opacity = 0.7
            RoutePlannerGrid.Children.Add(sender)
        Catch ex as Exception
        End Try
        sender.Margin = New Thickness(e.GetPosition(RoutePlannerGrid).X-2,e.GetPosition(RoutePlannerGrid).Y-2,0,0)
    End Sub
    Private Sub Item_MouseUp(sender As TextBlock, e As MouseEventArgs)
        sender.Margin=new Thickness(e.GetPosition(RoutePlannerGrid).X+4,e.GetPosition(RoutePlannerGrid).Y,0,0)
        sender.ReleaseMouseCapture()
        
        'Now we can check if we're on top of anything

        Try
            RoutePlannerGrid.Children.Remove(sender)
            sender.Opacity = 1
            sender.Margin=new Thickness(1)
            sender.HorizontalAlignment = HorizontalAlignment.Stretch
            dim Inserted as Boolean = False

            VisualTreeHelper.HitTest(RouteList,Nothing,Function(Result As HitTestResult)
                                                            Try
                                                                Routelist.Children.INsert(RouteList.Children.IndexOf(Result.VisualHit),sender)
                                                                RenderAnimationIn(sender)
                                                                Inserted = true
                                                                sender.Background = brushes.LightGreen
                                                               UnsavedChanges += 1
                                                            Catch ex As Exception

                                                            End Try
                                                            Return HitTestResultBehavior.Continue
                                               End Function, New PointHitTestParameters(e.GetPosition(RouteList)))

            If not Inserted then
                Routelist.Children.Insert(DirectCast(sender.Tag,routeplannerdata).TempID, sender)
                RenderAnimationIn(sender)
                sender.Background = brushes.LightCoral
            End If
            
        Catch ex As Exception

        End Try
        UpdateUnsavedChanges()
        
    End Sub

    Private Sub UpdateUnsavedChanges()
        UpdateStatus(UnsavedChanges.ToString + " unsaved changes to the order.")
        UnsavedChangeCount.Text = UnsavedChanges.tostring
    End Sub

    Private Sub RenderAnimationIn(sender As TextBlock)
        Dim sb as Storyboard = new Storyboard()
        Dim tf as new ScaleTransform(1,1)
        'sender.LayoutTransformOrigin = New Point(1,0)
        sender.LayoutTransform = tf
        dim ease as New CubicEase
        ease.EasingMode = EasingMode.EaseOut

        dim Anim as New DoubleAnimation(0,1, New Duration(New TimeSpan(0,0,0,0,200)),FillBehavior.HoldEnd)
        Anim.EasingFunction = ease
        sb.Children.Add(Anim)
        sb.SetTargetProperty(Anim,new PropertyPath("LayoutTransform.ScaleY"))
        sb.SetTarget(Anim, sender)

        sb.Begin()
    End Sub
    Private Sub Item_MouseMove(sender As TextBlock, e As MouseEventArgs)
        if sender.IsMouseCaptured
            sender.Margin = New Thickness(e.GetPosition(RoutePlannerGrid).X-2,e.GetPosition(RoutePlannerGrid).Y-2,0,0)
        End If
    End Sub

    Private Sub Item_Scroll(sender As TextBlock, e As MouseWheelEventArgs)
        if sender.IsMouseCaptured
            RouteScroller.ScrollToVerticalOffset(RouteScroller.VerticalOffset - e.Delta)
        End If
    End Sub
    
    Private Sub ApplyRouteButton_Click
        ApplyNewOrder
    End Sub
    Private Sub SaveRouteButton_Click
        ApplyNewOrder
        SaveRouteData
    End Sub

    Friend Overrides Sub TabClosing(Byref Cancel As boolean)
         if not UnsavedChanges>0 then
            'Do nothing and let it run
         Else 
            Me.Focus()
            Dim result as MsgBoxResult=MsgBox("You have unsaved changes to your route! Would you like to save them first?", MsgBoxStyle.YesNoCancel, "Route Planner")
            If result=MsgBoxResult.Yes Then
                UpdateStatus("Saving Changes.")
                SaveRouteData
                UpdateStatus("Changes Saved.")
            Elseif result=MsgBoxResult.No Then
                UpdateStatus("Unsaved Changes Discarded.")
            Else
                UpdateStatus("Operation Aborted.")
                Cancel=True
            End If
         End If

    End Sub

    Friend Sub New(Main As Mainwindow)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        SetWindowRef(Main)
    End Sub

End Class
