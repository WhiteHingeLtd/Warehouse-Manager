Public Class PageFrameTab
    Inherits TabItem

    Public Sub New(Page As ThreadedPage)
        Header = Page.Title
        Dim Frame as new Frame
        Frame.Content = Page
        mybase.Content = Frame
        ContentRef = Page
        End Sub

    Dim ContentRef as ThreadedPage

    Friend Function GetChildType As Type
        Return DirectCast(Mybase.Content,Frame).Content.GetType()
    End Function

    Friend Readonly Property Content as ThreadedPage
    Get
        return ContentRef
    End Get
    End Property


End Class
