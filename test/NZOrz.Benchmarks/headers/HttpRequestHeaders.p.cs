using Microsoft.Extensions.Primitives;
using System.Numerics;

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

    public long? ContentLength
    {
        get { return _r.ContentLength; }
        set
        {
            if (value.HasValue)
            {
                _bits |= 0b_10UL;
                _r.ContentLength = value;
            }
            else
            {
                _bits &= ~0b_10UL;
                _r.ContentLength = default;
            }
        }
    }

    public StringValues ContentType
    {
        get { return _r.ContentType; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 0b_100UL;
                _r.ContentType = value;
            }
            else
            {
                _bits &= ~0b_100UL;
                _r.ContentType = default;
            }
        }
    }

    public StringValues Connection
    {
        get { return _r.Connection; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 0b_1000UL;
                _r.Connection = value;
            }
            else
            {
                _bits &= ~0b_1000UL;
                _r.Connection = default;
            }
        }
    }

    public StringValues UserAgent
    {
        get { return _r.UserAgent; }
        set
        {
            if (!StringValues.IsNullOrEmpty(value))
            {
                _bits |= 0b_1000_0UL;
                _r.UserAgent = value;
            }
            else
            {
                _bits &= ~0b_1000_0UL;
                _r.UserAgent = default;
            }
        }
    }

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

            case KnownHeaderType.ContentLength:
                _bits |= 0b_10UL;
                _r.ContentLength = ParseContentLength(value.ToString());
                return true;

            case KnownHeaderType.ContentType:
                ContentType = value;
                return true;

            case KnownHeaderType.Connection:
                Connection = value;
                return true;

            case KnownHeaderType.UserAgent:
                UserAgent = value;
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

            case KnownHeaderType.ContentLength:
                if ((_bits & 0b_10UL) != 0UL)
                {
                    _bits &= ~0b_10UL;
                    _r.ContentLength = default;
                    return true;
                }
                return true;

            case KnownHeaderType.ContentType:
                if ((_bits & 0b_100UL) != 0UL)
                {
                    _bits &= ~0b_100UL;
                    _r.ContentType = default;
                    return true;
                }
                return true;

            case KnownHeaderType.Connection:
                if ((_bits & 0b_1000UL) != 0UL)
                {
                    _bits &= ~0b_1000UL;
                    _r.Connection = default;
                    return true;
                }
                return true;

            case KnownHeaderType.UserAgent:
                if ((_bits & 0b_1000_0UL) != 0UL)
                {
                    _bits &= ~0b_1000_0UL;
                    _r.UserAgent = default;
                    return true;
                }
                return true;

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

            case KnownHeaderType.ContentLength:
                if ((_bits & 0b_10UL) != 0UL)
                {
                    value = HeaderUtilities.FormatNonNegativeInt64(_r.ContentLength.Value);
                    return true;
                }
                break;

            case KnownHeaderType.ContentType:
                if ((_bits & 0b_100UL) != 0UL)
                {
                    value = _r.ContentType;
                    return true;
                }
                break;

            case KnownHeaderType.Connection:
                if ((_bits & 0b_1000UL) != 0UL)
                {
                    value = _r.Connection;
                    return true;
                }
                break;

            case KnownHeaderType.UserAgent:
                if ((_bits & 0b_1000_0UL) != 0UL)
                {
                    value = _r.UserAgent;
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
            array[arrayIndex] = new KeyValuePair<string, StringValues>(TestHeaderNames.Host, _r.Host);
            ++arrayIndex;
        }
        if ((_bits & 0b_10UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(TestHeaderNames.ContentLength, HeaderUtilities.FormatNonNegativeInt64(_r.ContentLength.Value));
            ++arrayIndex;
        }
        if ((_bits & 0b_100UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(TestHeaderNames.ContentType, _r.ContentType);
            ++arrayIndex;
        }
        if ((_bits & 0b_1000UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(TestHeaderNames.Connection, _r.Connection);
            ++arrayIndex;
        }
        if ((_bits & 0b_1000_0UL) != 0UL)
        {
            if (arrayIndex == array.Length)
            {
                return;
            }
            array[arrayIndex] = new KeyValuePair<string, StringValues>(TestHeaderNames.UserAgent, _r.UserAgent);
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
                    _current = new KeyValuePair<string, StringValues>(TestHeaderNames.Host, _collection._r.Host);
                    _currentBits &= ~0b_1UL;
                    break;

                case 1:
                    _current = new KeyValuePair<string, StringValues>(TestHeaderNames.ContentLength, HeaderUtilities.FormatNonNegativeInt64(_collection._r.ContentLength.Value));
                    _currentBits &= ~0b_10UL;
                    break;

                case 2:
                    _current = new KeyValuePair<string, StringValues>(TestHeaderNames.ContentType, _collection._r.ContentType);
                    _currentBits &= ~0b_100UL;
                    break;

                case 3:
                    _current = new KeyValuePair<string, StringValues>(TestHeaderNames.Connection, _collection._r.Connection);
                    _currentBits &= ~0b_1000UL;
                    break;

                case 4:
                    _current = new KeyValuePair<string, StringValues>(TestHeaderNames.UserAgent, _collection._r.UserAgent);
                    _currentBits &= ~0b_1000_0UL;
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
        public long? ContentLength;
        public StringValues ContentType;
        public StringValues Connection;
        public StringValues UserAgent;
        //public StringValues Accept;
        //public StringValues Connection;
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