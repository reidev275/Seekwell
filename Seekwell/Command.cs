using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace Seekwell
{
	public class Command
	{
		readonly string _connectionString;
		readonly IParameterSetter _parameterSetter = new ParameterSetter();
		readonly IDeserializer _deserializer = new Deserializer();

		public IDeserializer Deserializer { get { return _deserializer; } }

		/// <summary>
		/// Timeout for underlying SqlCommand.  Default is 30 seconds.
		/// </summary>
		public TimeSpan Timeout { get; set; }

		/// <summary>
		/// Creates a command with the first connection string found by the ConfigurationManager
		/// </summary>
		public Command() : this(GetFirstConnectionString()) { }

		public Command(string connectionString)
		{
			Timeout = TimeSpan.FromSeconds(30);
			_connectionString = connectionString;
		}

		static string GetFirstConnectionString()
		{
			for (int i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
			{
				var connection = ConfigurationManager.ConnectionStrings[i];
				if (connection.Name != "LocalSqlServer")
				{
					return connection.ConnectionString;
				}
			}
			return null;
		}

		public IEnumerable<T> Query<T>(string sql) where T : new()
		{
			return Query<T>(sql, null);
		}

		public IEnumerable<T> Query<T>(string sql, object parameters) where T : new()
		{
			using (var connection = CreateConnection())
			using (var command = PrepareCommand(connection, sql, parameters))
			{
				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					return Deserializer.Deserialize<T>(reader);
				}
			}
		}

		public int ExecuteScalar(string sql)
		{
			return ExecuteScalar(sql, null);
		}

		public int ExecuteScalar(string sql, object parameters)
		{
			using (var connection = CreateConnection())
			using (var command = PrepareCommand(connection, sql, parameters))
			{
				connection.Open();
				var result = command.ExecuteScalar();
				return Convert.ToInt32(result);
			}
		}

		public SqlConnection CreateConnection()
		{
			return new SqlConnection(_connectionString);
		}

		public SqlCommand PrepareCommand(SqlConnection connection, string sql, object parameters)
		{
			var result = connection.CreateCommand();
			result.CommandText = sql;
			result.CommandTimeout = (int)Timeout.TotalSeconds;
			_parameterSetter.SetParameters(result, parameters);
			return result;
		}
	}
}
