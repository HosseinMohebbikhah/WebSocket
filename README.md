# miniLocalMessenger

This is a mini messenger by WebSocket

miniLocalMessenger is developed by C# language.
In this program, it works as text data transfer and more methods will be added to it in future updates.

Using the [System.Net.WebSockets](https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets.websocket?view=net-7.0), [System.Net](https://learn.microsoft.com/en-us/dotnet/api/system.net?view=net-7.0) classes this project has progressed, but it can only send and receive text data, which we will face in the future when file transfer is added. This program is without any special UI, only the focus is on the codes and coding process.

In the client section, we can send data to another client by ID, and also to send data to the server that only the server receives, we must write 111 in the To box to send it to the server.

In the Server section, we can send data to all clients, and you can also send data to some of the clients, but I did not do the corresponding coding, because the server is the only role in the interface between the clients. By writing a message and pressing the send button, it will be sent to all clients.
Because all data passes through the server, we can have a history of transfers.

Gallery :

![image](https://github.com/HosseinMohebbikhah/miniLocalMessanger/blob/main/img/image.png)
