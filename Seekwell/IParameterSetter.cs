using System.Data.SqlClient;

namespace Seekwell
{
	public interface IParameterSetter
	{
		void SetParameters(SqlCommand command, object parameters);
	}
}
