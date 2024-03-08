#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.DataLogger;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.Alarm;
using FTOptix.Recipe;
using FTOptix.EventLogger;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using FTOptix.Core;
using FTOptix.InfluxDBStore;
#endregion

public class QueryViaNetLogic: BaseNetLogic
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
    public void DeleteQuery()
    {   
        // Get the name of the table (measurement) to delete from
        string tableName = "InfluxLogger";

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


    private int GetExpectedRecordsCount()
    {
        // Get the number of times the Influx logger has been triggered;
        // This is the expected number of records in the database
        int loggerTriggerCount = LogicObject.GetVariable("LoggerTriggerCount").Value;
        int expectedRecordsCount = loggerTriggerCount;

        Log.Info($"LoggerTriggerCount: {loggerTriggerCount}, expectedRecordsCount: {expectedRecordsCount}");

        return expectedRecordsCount;
    }


    [ExportMethod]
    private int CountRecords()
    {   
        // Create a store object 
        var myStore = Project.Current.Get<Store>("DataStores/InfluxDBDatabase1");
        object[,] resultSet;
        string[] header;

        // Get the name of the table to count from
        string tableName = "InfluxLogger";

        // Initialize the variable
        int recordsCount = -1;

        try 
        {
            // Execute a query to count all rows from the table 
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
                    recordsCount = 0;
                    LogicObject.GetVariable("RecordsCount").Value = recordsCount.ToString();
                    Log.Warning("The resultSet is empty.");
                }
                else
                {
                    Log.Info("The resultSet has elements.");
                    recordsCount = Convert.ToInt32(resultSet[0, 0]);
                    // Display the records count
                    LogicObject.GetVariable("RecordsCount").Value = resultSet[0, 0].ToString();
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error counting from {tableName}: {e.Message}");
        }

        return recordsCount;
    }


    [ExportMethod]
    public void VerifyRecordsCount()
    {   
        // Get the expected number of records
        int expectedRecordsCount = GetExpectedRecordsCount();

        // Get the actual number of records in the table
        int numberOfRecords = CountRecords();

        // Compare the expected and actual number of records
        if (numberOfRecords == expectedRecordsCount)
        {
            LogicObject.GetVariable("RecordsCountOK").Value = true;
        }
        else
        {
            LogicObject.GetVariable("RecordsCountOK").Value = false;
        }
    }

}
