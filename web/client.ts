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
  } else if (args == "joke") {
    const computerConfig = {
      "computer": {
        "manufacturer": "Dell",
        "model": "XPS 15",
        "serial_number": "XYZ1234567890",
        "operating_system": {
          "type": "Windows",
          "version": "11 Pro"
        },
        "processor": {
          "manufacturer": "Intel",
          "model": "Core i7-1165G7",
          "cores": 4,
          "threads": 8
        },
        "memory": [
          {
            "size_gb": 16,
            "type": "DDR4"
          }
        ],
        "storage": [
          {
            "capacity_gb": 512,
            "type": "SSD",
            "model": "Samsung PM9A3"
          }
        ]
      }
    };
    socket.send(
      JSON.stringify({
          event: "json",
          message: JSON.stringify(computerConfig, null, 2)
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