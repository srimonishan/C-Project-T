using System;
using System.IO;

namespace LocalArtisanCraftMarket.Utilities
{
    internal class Logger
    {
      private static readonly string logFile = "Logs/ErrorLog.txt";

      public static void LogError(Exception ex)
        {
            try
            {
                Directory.CreateDirectory("Logs");
                
                using (StreamWriter writer = new StreamWriter(logFile, true))
                {
                    writer.WriteLine("==================================");
                    writer.WriteLine("Date: " + DateTime.Now);
                    writer.WriteLine("Message: " + ex.Message);
                    writer.WriteLine("Stack Trace:");
                    writer.WriteLine(ex.StackTrace);
                    writer.WriteLine();
                }
