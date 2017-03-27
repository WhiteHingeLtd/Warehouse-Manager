Class Test
    Friend ReadOnly SupportsMultiInstance As Boolean = false

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        TiemBlock.Text = now.tostring
    End Sub
End Class
