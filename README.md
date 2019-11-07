# Message Me
Message Me is a file based Inter Process Communication (IPC) library for .NET applications on the same machine

## Usage

Download and install from Nuget

```
Install-Package Message.Me -Version 1.0.0
```

Define the interface between the two processes

```c#
public interface IHello
{
    string SayHello(string name);
}
```

Implement the interface handler service

```c#
internal class HelloService : IHello
{ 
    public string SayHello(string name)
    {
        return $"Hello, {name}!";
    }
}
````

Register the handler service and start the server to listen for messages in one process

```c#
var server = new Message.Me.Server();
server.RegisterService<IHello>(new HelloService(), Constants.CHANNEL_NAME);            
server.Start();
```

Use the client to send messages and get a response from the other process

```c#
var proxy = Message.Me.Client.GetServiceProxy<IHello>(Constants.CHANNEL_NAME);
var response = proxy.SayHello("Gaurav");
```
> Make sure you use the same channel name on both the client and server to ensure that they can talk to one another

**This library is tested only for interprocess communication between processes running under the same user account**
