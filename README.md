# NZOrz
NZOrz ( NeZha Orz ) is network proxy written in c# language, because vic lazy, so maybe no one know which day will be done


# ToDo

- [X] TCP server core
- [X] UDP server core
- [X] TCP proxy core
- [ ] LoadBalancingPolicy
- [ ] dns
- [ ] HealthCheck
- [ ] Configuration and reload config and rebind
- [ ] UDP proxy core
- [ ] HTTP1 server core
- [ ] HTTP2 server core
- [ ] HTTP3 server core
- [ ] HTTP proxy core
- [ ] proxy file config


# PS

1. Not base on asp.net, but core core is learn from Kestrel/Yarp and not support IIS.
2. Not support hot restart, because of it is not easy about [Migrate Socket between processes](https://github.com/dotnet/runtime/issues/48637) and do it with Cross-platform.
3. Vic is too lazy, so maybe no one know which day will be done.