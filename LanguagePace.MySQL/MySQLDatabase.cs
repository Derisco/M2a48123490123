﻿/* Copyright(C) LanguagePace.Com 
- All Rights Reserved
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* Written by Travis Wiggins, Erik Hattervig
* <LanguagePace@Yahoo.com>,
* July 27th 2017
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading;
using MySql.Data.MySqlClient;

namespace LanguagePace.MySQL
{
    /// <summary>
    ///     Class that encapsulates a MySQL database connections
    ///     and CRUD operations
    /// </summary>
    public class MySQLDatabase : IDisposable
    {
        private MySqlConnection _connection;

        /// <summary>
        ///     Default constructor uses the "DefaultConnection" connectionString
        /// </summary>
        public MySQLDatabase() 
            : this("DefaultConnection")
        { }

        /// <summary>
        ///     Constructor which takes the connection string name
        /// </summary>
        /// <param name="connectionStringName"></param>
        public MySQLDatabase(string connectionStringName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            _connection = new MySqlConnection(connectionString);
        }

        /// <summary>
        ///     Executes a non-query MySQL statement
        /// </summary>
        /// <param name="commandText">The MySQL query to execute</param>
        /// <param name="parameters">Optional parameter to pass to the querey</param>
        /// <returns>The count of records affected by the MySQL statement</returns>
        public int Execute(string commandText, Dictionary<string, object> parameters)
        {
            int result = 0;

            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            try
            {
                EnsureConnectionOpen();
                var command = CreateCommand(commandText, parameters, CommandType.Text);
                result = command.ExecuteNonQuery();
            }
            finally
            {
                EnsureConnectionClosed();
            }

            return result;
        }

        /// <summary>
        ///     Executes a non-query MySQL statement
        /// </summary>
        /// <param name="procedure">Name of procedure to execute</param>
        /// <param name="parameters">Optional parameter to pass to the querey</param>
        /// <returns>The count of records affected by the MySQL statement</returns>
        public int ExecuteStoredProcedure(string procedure, Dictionary<string, object> parameters)
        {
            int result = 0;

            if (String.IsNullOrEmpty(procedure))
            {
                throw new ArgumentException("Procedure cannot be null or empty.");
            }

            try
            {
                EnsureConnectionOpen();
                var command = CreateCommand(procedure, parameters, CommandType.StoredProcedure);
                result = command.ExecuteNonQuery();
            }
            finally
            {
                EnsureConnectionClosed();
            }


            return result;
        }

        /// <summary>
        ///     Executes a MySQL query that returns a single scalar value as the result.
        /// </summary>
        /// <param name="commandText">The MySQL query to execute</param>
        /// <param name="parameters">Optional parameters to pass to the querey</param>
        /// <returns></returns>
        public object QueryValue(string commandText, Dictionary<string, object> parameters)
        {
            object result = null;

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            try
            {
                EnsureConnectionOpen();
                var command = CreateCommand(commandText, parameters, CommandType.Text);
                result = command.ExecuteScalar();
            }
            finally
            {
                EnsureConnectionClosed();
            }

            return result;
        }

        /// <summary>
        ///     Executes a MySQL stored procedure that returns a list of rows as the result.
        /// </summary>
        /// <param name="procedure">The stored procedure to execute</param>
        /// <param name="parameters">Optional parameters to pass to the querey</param>
        /// <returns></returns>
        public List<Dictionary<string, string>> QueryStoredProcedure(string procedure, Dictionary<string, object> parameters)
        {
            List<Dictionary<string, string>> rows = null;

            if (String.IsNullOrEmpty(procedure))
            {
                throw new ArgumentException("Procedure cannot be null or empty.");
            }

            try
            {
                EnsureConnectionOpen();
                var command = CreateCommand(procedure, parameters, CommandType.StoredProcedure);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    rows = new List<Dictionary<string, string>>();
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, string>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = reader.GetName(i);
                            var columnValue = reader.IsDBNull(i) ? null : reader.GetString(i);
                            row.Add(columnName, columnValue);
                        }
                        rows.Add(row);
                    }

                }
            }
            finally
            {
                EnsureConnectionClosed();
            }

            return rows;
        }

        /// <summary>
        ///     Executes a SQL query that returns a list of rows as the result.
        /// </summary>
        /// <param name="commandText">The MySQL query to execute</param>
        /// <param name="parameters">Parameters to pass to the MySQL query</param>
        /// <returns>
        ///     A list of a Dictionary of Key, values pairs representing the
        ///     ColumnNam and corresponding value
        /// </returns>
        public List<Dictionary<string, string>> Query(string commandText, Dictionary<string, object> parameters)
        {
            List<Dictionary<string, string>> rows = null;
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text connot be null or empty.");
            }

            try
            {
                EnsureConnectionOpen();
                var command = CreateCommand(commandText, parameters, CommandType.Text);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    rows = new List<Dictionary<string, string>>();
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, string>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = reader.GetName(i);
                            var columnValue = reader.IsDBNull(i) ? null : reader.GetString(i);
                            row.Add(columnName, columnValue);
                        }
                        rows.Add(row);
                    }

                }
            }
            finally
            {
                EnsureConnectionClosed();
            }

            return rows;
        }

        /// <summary>
        ///     Opens a connection if not open
        /// </summary>
        private void EnsureConnectionOpen()
        {
            var retries = 3;
            if (_connection.State == ConnectionState.Open)
            {
                return;
            }
            else
            {
                while (retries >= 0 && _connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                    retries--;
                    Thread.Sleep(30);
                }
            }
        }

        /// <summary>
        ///     Closes a connection if open
        /// </summary>
        public void EnsureConnectionClosed()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        /// <summary>
        ///     Creates a MySQLCommand with the given parameters
        /// </summary>
        /// <param name="commandText">The MySQL query to execute</param>
        /// <param name="parameters">Parameters to pass to the MySQL query</param>
        /// <returns></returns>
        private MySqlCommand CreateCommand(string commandText, Dictionary<string, object> parameters, CommandType commandType)
        {
            MySqlCommand command = _connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            if (commandType == CommandType.StoredProcedure)
                AddParametersStoredProcedure(command, parameters);
            else
                AddParameters(command, parameters);

            return command;
        }

        /// <summary>
        ///     Adds the parameters to a MySQL command
        /// </summary>
        /// <param name="command">The MySQL query to execute</param>
        /// <param name="parameters">Parameters to pass to the MySQL query</param>
        private static void AddParameters(MySqlCommand command, Dictionary<string, object> parameters)
        {
            if (parameters == null)
            {
                return;
            }

            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = param.Key;
                parameter.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }

        private static void AddParametersStoredProcedure(MySqlCommand command, Dictionary<string, object> parameters)
        {
            if (parameters == null)
            {
                return;
            }

            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);
            }
        }

        /// <summary>
        ///     Helper method to return query a string value 
        /// </summary>
        /// <param name="commandText">The MySQL query to execute</param>
        /// <param name="parameters">Parameters to pass to the MySQL query</param>
        /// <returns>The string value resulting from the query</returns>
        public string GetStrValue(string commandText, Dictionary<string, object> parameters)
        {
            string value = QueryValue(commandText, parameters) as string;
            return value;
        }

        /// <summary>
        ///     Closes and disposes of the MySQL connection
        /// </summary>
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }


    }
}
