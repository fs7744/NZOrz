using NZ.Orz.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Health;

public interface IHealthReporter
{
    void ReportFailed(DestinationState destinationState);

    void ReportSuccessed(DestinationState destinationState);
}