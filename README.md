# network-final-project

## example:

### Http:

#### proxy: 

> proxy –s=udp:127.0.0.1:80 –d=tcp

####client
>GET / HTTP/1.1
>Host: aut.ac.ir
>
>


### Dns:
#### proxy
> proxy –s=tcp:127.0.0.1:7878–d=udp

#### client
> type=A server=217.215.155.155 target=aut.ac.ir
