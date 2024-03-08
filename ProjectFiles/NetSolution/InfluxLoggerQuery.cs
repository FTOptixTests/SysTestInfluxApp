#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.CoreBase;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.Recipe;
using FTOptix.DataLogger;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.ODBCStore;
using FTOptix.Report;
using FTOptix.OPCUAClient;
using FTOptix.RAEtherNetIP;
using FTOptix.MicroController;
using FTOptix.Retentivity;
using FTOptix.System;
using FTOptix.Alarm;
using FTOptix.CommunicationDriver;
using FTOptix.UI;
using FTOptix.SerialPort;
using FTOptix.Core;
using FTOptix.InfluxDBStore;
#endregion

public class InfluxLoggerQuery : BaseNetLogic
{
    public override void Start()
    {
        // Enable periodic query when the screen object is loaded at runtime
        LogicObject.GetVariable("EnablePeriodicQuery").Value = true;
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void VerifyRecordsCount()
    {
        // Create a store object
        var myStore = Project.Current.Get<Store>("DataStores/InfluxDBDatabase1");
        object[,] resultSet;
        string[] header;

        // Get the number of times the Influx logger has been triggered 
        int expectedRecordsCount = LogicObject.GetVariable("LoggerTriggerCount").Value;
        int recordsCount = -1;

        // Get the name of the table to count from
        string tableName = "InfluxLogger";

        try
        {
            // Execute the query and get the result set
            myStore.Query($"SELECT COUNT(*) FROM {tableName}", out header, out resultSet);

            // Check if the array is null
            if (resultSet == null)
            {
                Log.Warning("The resultSet is null.");
            }
            else
            {
                // Check if either dimension has a size of 0
                if (resultSet.Length == 0)
                {
                    Log.Warning("The resultSet is empty.");
                }
                else
                {
                    Log.Info("The resultSet has elements.");
                    // Get the number of records from the result set
                    recordsCount = Convert.ToInt32(resultSet[0, 0]);
                }
            }

        }
        catch (Exception ex)
        {
            Log.Error($"Error executing query: {ex.Message}");
            recordsCount = -1;
        }

        // Compare the expected and actual number of records
        if (recordsCount == expectedRecordsCount)
        {
            LogicObject.GetVariable("RecordsCountOK").Value = true;
            Log.Info($"RecordsCountOK. Expected number of records: {expectedRecordsCount}, Actual number of records: {recordsCount}");
        }
        else
        {
            LogicObject.GetVariable("RecordsCountOK").Value = false;
            Log.Error($"ERROR! Expected number of records: {expectedRecordsCount}, Actual number of records: {recordsCount}");
        }

        // Disable periodic query so that the query is only executed once
        LogicObject.GetVariable("EnablePeriodicQuery").Value = false; 
    }

}
