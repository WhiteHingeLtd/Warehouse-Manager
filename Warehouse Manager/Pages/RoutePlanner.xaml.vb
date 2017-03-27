Imports WHLClasses

Class RoutePlanner
    Friend Overrides Function SupportsMultipleTabs() As Boolean
        Return False
    End Function

    Dim RouteTable as List(of Dictionary(Of String,Object))
    Private Sub GetRouteButton_Click(sender As Object, e As RoutedEventArgs)
        UpdateStatus("Waiting for response from database, Please Wait...")
        ProcessInBackground(Sub()
                                RouteTable = MySql.SelectDataDictionary("SELECT * FROM whldata.location_routing ORDER BY RouteIndex;")
                                Worker.ReportProgress(0,"Done.")
                               End Sub)
        RouteList.Children.Clear()
        UpdateStatus("Loading Table...")
        For each Entry as Dictionary(of String, Object) in RouteTable
            Dim TB As new TextBlock()
            TB.Text = Entry("RouteBlockName") + " (Current Pos: "+Entry("RouteIndex").tostring+")"
            TB.Padding=New Thickness(5)
            TB.Margin=New Thickness(1)
            TB.Background = Brushes.LightBlue
            
            TB.HorizontalAlignment = HorizontalAlignment.Stretch
            TB.VerticalAlignment = VerticalAlignment.Top
            AddHandler TB.Mouseleftbuttondown, AddressOf Item_MouseDown
            AddHandler TB.mouseleftbuttonup, AddressOf Item_MouseUp
            AddHandler TB.MouseMove, AddressOf Item_MouseMove
            RouteList.Children.Add(TB)
        Next
        UpdateStatus("Done")
    End Sub

    Private Sub RoutePlanner_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        UpdateStatus("Done.")
    End Sub
    
    Private Sub OpenSpacesBetweenElements()
        for each child as textblock in RouteList.children
            child.Margin = new Thickness(1,7,1,8)
        Next
    End Sub
    Private Sub CloseSpacesBetweenElements()
        for each child as textblock in RouteList.children
            child.Margin = new Thickness(1)
        Next
    End Sub

    Private Sub Item_MouseDown(sender As TextBlock, e As MouseEventArgs)
        sender.CaptureMouse()
        'OpenSpacesBetweenElements
        sender.BringIntoView()
        sender.Tag = RouteList.children.

        Try
            Routelist.Children.Remove(sender)
            RoutePlannerGrid.Children.Add(sender)
        Catch ex as Exception
        End Try
        sender.Margin = New Thickness(e.GetPosition(RoutePlannerGrid).X,e.GetPosition(RoutePlannerGrid).Y,0,0)
    End Sub
    Private Sub Item_MouseUp(sender As TextBlock, e As MouseEventArgs)
        sender.ReleaseMouseCapture()
        'CloseSpacesBetweenElements
        Try
            RoutePlannerGrid.Children.Remove(sender)
            sender.Margin=new Thickness(1)
            Routelist.Children.Add(sender)
            
        Catch ex As Exception

        End Try
        
        
    End Sub
    Private Sub Item_MouseMove(sender As TextBlock, e As MouseEventArgs)
        if sender.IsMouseCaptured
            sender.Margin = New Thickness(e.GetPosition(RoutePlannerGrid).X,e.GetPosition(RoutePlannerGrid).Y,0,0)
        End If
    End Sub

End Class
