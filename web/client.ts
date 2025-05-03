const args = Deno.args[0];

const socket = new WebSocket("ws://127.0.0.1:8080/checkupdate?client=frank");

// SOCKET OPEN
socket.addEventListener("open", () => {
  console.log(socket.readyState);
  if (args=="message"){
    socket.send(
      JSON.stringify({
          event: "message",
          message: "Ping"
      }),
    );
    //socket.close();
  } else if (args == "email") {
    socket.send(
      JSON.stringify({
          event: "email",
          message: '[{"name": "<name>", "title": "Purchasing Manager", "email": "<email>"}]'
        }),
    );
    socket.close();
  } 
 });


// SOCKET MESSAGE
socket.addEventListener("message", (event) => {
  
  try {
    const message = JSON.parse(event.data);   
    if (message.event == "command") {
      switch (message.message) {
        case "email":
          console.log(`Received ${message.message} command.`);
          socket.send(
            JSON.stringify({
                event: "email",
                message: '[{"name": "<name>", "title": "Purchasing Manager", "email": "<email>"}]'
              }),
          );
          break;
        
        case "bark":
          console.log("bark bark");
          break;
      }
    }
    
  } catch {}
});