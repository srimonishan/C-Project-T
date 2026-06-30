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
              
