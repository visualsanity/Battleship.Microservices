﻿namespace Battleship.Microservices.Infrastructure.Repository
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Microsoft.Data.SqlClient;

    public class Initialisation
    {
        #region Methods

        public static void Setup(string sqlConnectionString)
        {
            try
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                var path =
                    $"{Path.GetDirectoryName((object) entryAssembly != null ? entryAssembly.Location : null)}{Path.DirectorySeparatorChar}Configuration{Path.DirectorySeparatorChar}Setup.sql";
                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    string[] strArray = new Regex("\\r{0,1}\\nGO\\r{0,1}\\n").Split(File.ReadAllText(path));
                    for (var index = 0; index < strArray.Length; ++index)
                    {
                        if (strArray[index] != string.Empty)
                            new SqlCommand(strArray[index], connection).ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exp)
            {
                throw new Exception("Execution failed.", exp);
            }
        }

        #endregion
    }
}