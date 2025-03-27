using Microsoft.Extensions.Primitives;
using System.Numerics;
using System.Reflection.PortableExecutable;

namespace NZ.Orz.Http;

public partial class HttpRequestHeaders
{
    private ulong _bits;
    private HeaderReferences _r = new HeaderReferences();

    public StringValues Host
    {
        get { return _r.Host; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 0b_1UL;
                _r.Host = value;
            }
            else
            {
                _bits &= ~0b_1UL;
                _r.Host = default;
            }
        }
    }

    //private long? _contentLength;

    //public long? ContentLength
    //{
    //    get { return _contentLength; }
    //    set
    //    {
    //        _contentLength = value;
    //    }
    //}

    //public StringValues CacheControl
    //{
    //    get { return _r.CacheControl; }
    //    set
    //    {
    //        _r.CacheControl = value;
    //    }
    //}

    private void FastClear()
    {
        var tempBits = _bits;
        _bits = 0;
        if (BitOperations.PopCount(tempBits) > 12)
        {
            _r = default(HeaderReferences);
            return;
        }
    }

    private bool FastAdd(KnownHeaderType k, StringValues value)
    {
        switch (k)
        {
            case KnownHeaderType.Host:
                Host = value;
                return true;

            case KnownHeaderType.Unknown:
            default:
                return false;
        }
    }

    private bool FastRemove(KnownHeaderType k)
    {
        switch (k)
        {
            case KnownHeaderType.Host:
                if ((_bits & 0b_1UL) != 0UL)
                {
                    _bits &= ~0b_1UL;
                    _r.Host = default;
                    return true;
                }
                return false;

            case KnownHeaderType.Unknown:
            default:
                return false;
        }
    }

    private bool FastTryGetValue(KnownHeaderType k, out StringValues value)
    {
        switch (k)
        {
            case KnownHeaderType.Host:
                if ((_bits & 0b_1UL) != 0UL)
                {
                    value = _r.Host;
                    return true;
                }
                break;

            case KnownHeaderType.Unknown:
            default:
                break;
        }
        value = default;
        return false;
    }

    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
    {
        if (arrayIndex < 0)
        {
            return;
        }
        if ((_bits & 0b_1UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.Host, _r.Host);
            ++arrayIndex;
        }
        ((ICollection<KeyValuePair<string, StringValues>>?)dict)?.CopyTo(array, arrayIndex);
    }

    public partial struct Enumerator
    {
        public bool MoveNext()
        {
            switch (_next)
            {
                case 0:
                    _current = new KeyValuePair<string, StringValues>(HeaderNames.Host, _collection._r.Host);
                    _currentBits &= ~0b_1UL;
                    break;

                default:
                    if (_enumerator != null && _enumerator.MoveNext())
                    {
                        _current = _enumerator.Current;
                        return true;
                    }
                    return false;
            }

            if (_currentBits != 0)
            {
                _next = BitOperations.TrailingZeroCount(_currentBits);
                return true;
            }
            else
            {
                _next = -1;
                return true;
            }
        }
    }

    private struct HeaderReferences
    {
        public StringValues Host;
        //public StringValues Accept;
        //public StringValues Connection;
        //public StringValues UserAgent;
        //public StringValues Authority;
        //public StringValues Method;
        //public StringValues Path;
        //public StringValues Protocol;
        //public StringValues Scheme;
        //public StringValues AcceptCharset;
        //public StringValues AcceptEncoding;
        //public StringValues AcceptLanguage;
        //public StringValues AccessControlRequestHeaders;
        //public StringValues AccessControlRequestMethod;
        //public StringValues AltUsed;
        //public StringValues Authorization;
        //public StringValues Baggage;
        //public StringValues CacheControl;
        //public StringValues ContentType;
        //public StringValues Cookie;
        //public StringValues CorrelationContext;
        //public StringValues Date;
        //public StringValues Expect;
        //public StringValues From;
        //public StringValues GrpcAcceptEncoding;
        //public StringValues GrpcEncoding;
        //public StringValues GrpcTimeout;
        //public StringValues IfMatch;
        //public StringValues IfModifiedSince;
        //public StringValues IfNoneMatch;
        //public StringValues IfRange;
        //public StringValues IfUnmodifiedSince;
        //public StringValues KeepAlive;
        //public StringValues MaxForwards;
        //public StringValues Origin;
        //public StringValues Pragma;
        //public StringValues ProxyAuthorization;
        //public StringValues Range;
        //public StringValues Referer;
        //public StringValues RequestId;
        //public StringValues TE;
        //public StringValues TraceParent;
        //public StringValues TraceState;
        //public StringValues TransferEncoding;
        //public StringValues Translate;
        //public StringValues Upgrade;
        //public StringValues UpgradeInsecureRequests;
        //public StringValues Via;
        //public StringValues Warning;
    }
}