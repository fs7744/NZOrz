using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Hosting;

internal interface IConfigureContainerAdapter
{
    void ConfigureContainer(AppHostBuilderContext hostContext, object containerBuilder);
}