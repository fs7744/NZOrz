# NZOrz
NZOrz ( NeZha Orz ) is network proxy written in c# language, because vic lazy, so maybe no one know which day will be done


# ToDo

- [X] TCP server core
- [X] TCP proxy core
- [X] dns (use system dns, no query from dns server )
- [X] LoadBalancingPolicy
- [X] Passive HealthCheck
- [X] TCP Connected Active HealthCheck
- [X] Configuration 
- [X] reload config and rebind
- [X] Log
- [X] UDP server core
- [X] UDP proxy core
- [X] SNI proxy (no tls handle, tls base on upstream)
- [X] SNI proxy (tls handle, upstream no tls handle)
- [ ] HTTP1 server core
- [ ] HTTP2 server core
- [ ] HTTP3 server core
- [ ] HTTP proxy core
- [ ] Config Validators
- [ ] Metrics


# PS

1. Not base on asp.net, but core core is learn from Kestrel/Yarp and not support IIS.
2. Not support hot restart, because of it is not easy about [Migrate Socket between processes](https://github.com/dotnet/runtime/issues/48637) and do it with Cross-platform.
3. Vic is too lazy, so maybe no one know which day will be done.