using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Seekwell
{
	public static class CommandAsyncExtensions
	{
		public static async Task<IEnumerable<T>> QueryAsync<T>(this Command obj, string sql, object parameters = null) where T : new()
		{
			using (var connection = obj.CreateConnection())
			using (var command = obj.PrepareCommand(connection, sql, parameters))
			{
				await connection.OpenAsync();

				using (var reader = await command.ExecuteReaderAsync())
				{
					return obj.Deserializer.Deserialize<T>(reader);
				}
			}
		}

		public static async Task<int> ExecuteScalarAsync(this Command obj, string sql, object parameters = null)
		{
			using (var connection = obj.CreateConnection())
			using (var command = obj.PrepareCommand(connection, sql, parameters))
			{
				await connection.OpenAsync();
				var result = await command.ExecuteScalarAsync();
				return Convert.ToInt32(result);
			}
		}
	}
}
