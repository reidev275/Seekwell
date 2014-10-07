using System;
using System.Data.SqlClient;

namespace Seekwell
{
	public class ParameterSetter : IParameterSetter
	{
		public void SetParameters(SqlCommand command, object parameters)
		{
			if (parameters != null)
			{
				var properties = parameters.GetType().GetProperties();
				foreach (var property in properties)
				{
					var value = property.GetValue(parameters, null) ?? DBNull.Value;
					command.Parameters.AddWithValue(property.Name, value);
				}
			}
		}
	}
}
