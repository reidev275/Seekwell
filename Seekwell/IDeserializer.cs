using System.Collections.Generic;
using System.Data;

namespace Seekwell
{
	public interface IDeserializer
	{
		IEnumerable<T> Deserialize<T>(IDataReader reader) where T : new();
	}
}
