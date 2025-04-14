const socket = new WebSocket("ws://192.168.1.202/checkupdate?username=frank");
socket.addEventListener("open", () => {
  console.log(socket.readyState);
  socket.send("ping");
});
socket.addEventListener("message", (event) => {
  console.log(event.data);
});