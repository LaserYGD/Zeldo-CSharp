using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeldo.Interfaces
{
	public interface IMechanical
	{
		int Id { get; }

		void TriggerMechanism();
	}
}
