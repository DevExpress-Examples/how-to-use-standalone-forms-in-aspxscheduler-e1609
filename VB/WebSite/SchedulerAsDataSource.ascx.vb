﻿Imports System
Imports System.Data
Imports System.Configuration
Imports System.Collections
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls
Imports DevExpress.Web.ASPxScheduler
Imports DevExpress.XtraScheduler
Imports System.Data.OleDb
Imports System.Drawing

Partial Public Class SchedulerAsDataSource
    Inherits System.Web.UI.UserControl

    Protected Overrides Sub OnInit(ByVal e As EventArgs)
        MyBase.OnInit(e)
        SetupStatuses(ASPxScheduler1)
    End Sub
    Private Sub SetupStatuses(ByVal control As ASPxScheduler)
        control.Storage.Appointments.Statuses.Clear()
        Dim data As IEnumerable = StatusDataSource.Select(New DataSourceSelectArguments())
        For Each dataItem As DataRowView In data
            Dim name As String = DirectCast(dataItem.Row("Name"), String)
            Dim color As Color = GetStatusColor(dataItem.Row("Color"))
            control.Storage.Appointments.Statuses.Add(GetStatusColor(color), name, name)
        Next dataItem
    End Sub
    Private Function GetStatusColor(ByVal cl As Object) As Color
        If cl Is DBNull.Value Then
            Return Color.FromArgb(&HFFFFFF)
        End If
        If TypeOf cl Is Color Then
            Return DirectCast(cl, Color)
        End If
        Dim statusColor As Integer = Convert.ToInt32(cl)
        Return Color.FromArgb(statusColor)
    End Function

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
        DataBind()
    End Sub
    Public ReadOnly Property Scheduler() As ASPxScheduler
        Get
            Return ASPxScheduler1
        End Get
    End Property
    #Region "DataBind"
    Private lastInsertedAppointmentId As Integer
    Protected Sub ASPxScheduler1_AppointmentRowInserting(ByVal sender As Object, ByVal e As ASPxSchedulerDataInsertingEventArgs)
        ' Autoincremented primary key case
        e.NewValues.Remove("ID")
    End Sub
    Protected Sub ASPxScheduler1_AppointmentRowInserted(ByVal sender As Object, ByVal e As ASPxSchedulerDataInsertedEventArgs)
        ' Autoincremented primary key case
        e.KeyFieldValue = Me.lastInsertedAppointmentId
    End Sub
    Protected Sub AppointmentsDataSource_Inserted(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.SqlDataSourceStatusEventArgs)
        ' Autoincremented primary key case
        Dim connection As OleDbConnection = CType(e.Command.Connection, OleDbConnection)
        Using cmd As New OleDbCommand("SELECT @@IDENTITY", connection)
            Me.lastInsertedAppointmentId = DirectCast(cmd.ExecuteScalar(), Integer)
        End Using
    End Sub
    Protected Sub ASPxScheduler1_OnAppointmentsInserted(ByVal sender As Object, ByVal e As PersistentObjectsEventArgs)
        ' Autoincremented primary key case
        Dim count As Integer = e.Objects.Count
        System.Diagnostics.Debug.Assert(count = 1)
        Dim apt As Appointment = CType(e.Objects(0), Appointment)
        Dim storage As ASPxSchedulerStorage = DirectCast(sender, ASPxSchedulerStorage)
        storage.SetAppointmentId(apt, lastInsertedAppointmentId)
    End Sub
    #End Region
End Class
