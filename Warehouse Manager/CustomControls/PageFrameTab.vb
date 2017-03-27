Public Class PageFrameTab
    Inherits TabItem

    Public Sub New(Page As Page)
        Header = Page.Title
        Dim Frame as new Frame
        Frame.Content = Page
        me.Content = Frame
        End Sub

    Friend Function GetChildType As Type
        Return DirectCast(Me.Content,Frame).Content.GetType()
    End Function


End Class
