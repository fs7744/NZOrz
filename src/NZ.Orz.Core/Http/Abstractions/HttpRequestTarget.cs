using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZ.Orz.Http;

internal enum HttpRequestTarget
{
    Unknown = -1,

    // origin-form is the most common
    OriginForm,

    AbsoluteForm,
    AuthorityForm,
    AsteriskForm
}