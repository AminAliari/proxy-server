# Network course final project
In this project I've built an proxy server to handle diffrent type of request from an udp client to a tcp server connection and vice versa.
this proxy can perform http requests (with redirects) and dns queries (type A and CNAME)

## <br>examples:

### Http:<br>
#### proxy
> proxy –s=udp:127.0.0.1:80 –d=tcp
#### client
>GET / HTTP/1.1
><br>Host: aut.ac.ir<br><br>

### <br>Dns:
#### proxy
> proxy –s=tcp:127.0.0.1:7878–d=udp
#### client
> type=A server=217.215.155.155 target=aut.ac.ir
