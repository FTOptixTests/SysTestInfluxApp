#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.WebUI;
using FTOptix.CoreBase;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.Recipe;
using FTOptix.DataLogger;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.ODBCStore;
using FTOptix.InfluxDBStore;
using FTOptix.Report;
using FTOptix.OPCUAClient;
using FTOptix.RAEtherNetIP;
using FTOptix.System;
using FTOptix.Retentivity;
using FTOptix.CommunicationDriver;
using FTOptix.UI;
using FTOptix.SerialPort;
using FTOptix.EventLogger;
using FTOptix.Alarm;
using FTOptix.Core;
using System.Threading;
#endregion

public class PopulateHeavyInfluxDB : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

     [ExportMethod]
    public void PopulateDatabase()
    {
        // Define the number of inserts to the database.
        int numberOfInserts = 1000;

        // Increment the variable value i-times. Each increment causes the HeavyInfluxDB logger to write to the 'heavy' InfluxDB.
        for (int i = 0; i < numberOfInserts; i++)
        {
            // Delay between each increment
            Thread.Sleep(1000);
            LogicObject.GetVariable("HeavyInfluxDBLoggerTrigger").Value = i+1;
            Log.Info($"Data written to 'heavy' Influx database; {i+1}");
        }  
    }

    [ExportMethod]
    public void DeleteQuery()
    {   
        // Get the name of the table (measurement) to delete from
        string tableName = "InfluxReadHeavyDB";

        // Create a store object
        var myStore = Project.Current.Get<Store>("DataStores/InfluxDBDatabase1");
        object[,] resultSet;
        string[] header;

        try 
        {
            // Execute a query to delete all rows from the table
            myStore.Query($"DELETE FROM {tableName}", out header, out resultSet);
        }
        catch (Exception e)
        {
            Log.Error($"Error deleting from {tableName}: {e.Message}");
        }
    }
}
