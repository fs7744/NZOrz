using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http.Abstractions;

internal interface IRequestProcessor
{
    Task ProcessRequestsAsync(HttpConnectionDelegate application);
}