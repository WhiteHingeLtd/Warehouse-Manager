Imports System.Windows.Media.Animation
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
        Throw New NotImplementedException
    End Sub

    Dim RouteIndexes as new List(Of Integer)

    Private Sub LoadRouteData()
        UpdateStatus("Waiting for response from database, Please Wait...")
        ProcessInBackground(Sub()
                                RouteTable = MySql.SelectDataDictionary("SELECT * FROM whldata.location_routing ORDER BY RouteIndex;")
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
            Data.RouteID = entry("﻿RouteID")
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
    End Sub

    Private Sub RoutePlanner_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        UpdateUnsavedChanges()
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
        try
            
        Catch ex As Exception

        End Try

        Try
            RoutePlannerGrid.Children.Remove(sender)
            sender.Opacity = 1
            sender.Margin=new Thickness(1)
            sender.HorizontalAlignment = HorizontalAlignment.Stretch
            dim Inserted as Boolean = False

            VisualTreeHelper.HitTest(RouteList,Nothing,Function(result As HitTestResult)
                                                            Try
                                                                Routelist.Children.INsert(RouteList.Children.IndexOf(result.VisualHit),sender)
                                                                RenderAnimationIn(sender)
                                                                Inserted = true
                                                                sender.Background = brushes.LightGreen
                                                               UnsavedChanges += 1
                                                            Catch ex As Exception

                                                            End Try
                                                            
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


End Class
