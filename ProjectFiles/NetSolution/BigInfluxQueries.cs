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
using System.Threading;
using FTOptix.InfluxDBStore;
#endregion

public class BigInfluxQueries : BaseNetLogic
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
    public void VerifyDatabase()
    {   
        // Create a store object - this is the ODBC store
        var myStore = Project.Current.Get<Store>("DataStores/InfluxDBDatabase1");
        object[,] resultSet;
        string[] header;

        // Expected number of records in the table
        int expectedRecordsCount = 1000;
        int recordsCount = -1;

        string tableName = "InfluxReadHeavyDB";

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
        catch (Exception e)
        {
            Log.Error($"ERROR: {e.Message}");
        }

        // Compare the expected and actual number of records
        if (recordsCount == expectedRecordsCount)
        {
            LogicObject.GetVariable("RecordsCountOK").Value = true;
        }
        else
        {
            LogicObject.GetVariable("RecordsCountOK").Value = false;
            Log.Info($"ERROR! Expected number of records: {expectedRecordsCount}, Actual number of records: {recordsCount}");
        }
        
        // Get the 1st and the 1000th record from the result set and if that doesn't throw an exception, set the flag to true; if not - set the flag to false
        try
        {
            string selectQuery = $"SELECT * FROM {tableName}";
            myStore.Query(selectQuery, out header, out resultSet);

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
                    Log.Info($"Record {0}: {resultSet[0, 0]}, {resultSet[0, 1]}, {resultSet[0, 2]}...");
                    Log.Info($"Record {999}: {resultSet[999, 0]}, {resultSet[999, 1]}, {resultSet[999, 2]}...");
                    LogicObject.GetVariable("SelectRecordsOK").Value = true;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"ERROR: {e.Message}");
        }

        // Disable periodic query so that the query is only executed once
        LogicObject.GetVariable("EnablePeriodicQuery").Value = false;  
    }
}
