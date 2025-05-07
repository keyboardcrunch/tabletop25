//const myUsername = prompt("Please enter your name") || "Anonymous";
const socket = new WebSocket(
  `ws://beaverpro.sketchybins.com/checkupdate`,
);

function addMessage(message) {
   console.log(`${message}`);
}

function sendCommand() {
  const commandElement = document.getElementById("data");
  const commandData = commandElement.value;
  console.log(commandData);
  socket.send(
    JSON.stringify({
      event: "command",
      command: commandData,
    }),
  )
}
