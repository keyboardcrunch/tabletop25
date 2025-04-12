# Table Top 2025


## Server
The server is a Deno 2 Oak web server that hosts a cloned Beaver Notes website that will be used in malicious
Google Ads to poison searches. Most of the page links have been nullified, and only a Windows download will be
available (compromised/repackaged malicious installer).


### Routes
  - /download.html
  - /downloads (malicious payloads)
  - /crashreport (exfiltrated data)
  - /checkupdate (websocket c2 for malicious updater task)

### WebSocket
The /checkupdate endpoint is for websockets, where the "update" scheduled task will connect to and listen for and
send messages. 

### Client Commands
  * getemail: "first last"
  * abort
  * urlexec: "URL"
  * powershell: "powershell"

### Server Messages
  * getemail: "EMAIL"


## The Narratives

### Targeting the Analyst
  * Challenge assumptions for the needs of administrative privileges.
  * Choosing a communication method that doesn't stick out but isn't very visible.
  * Forcing an analyst to use deeper analysis methods, knowing our tools.

### The Tabletop
  * User needs a note taking app and searches google, but ends up clicking a malicious ad that distributes a backdoored installer.
  * The software installs without admin privileges, and has a scheduled update task.
  * The updater looks to be querying active directory and connects to an update endpoint (websocket).
  * The updater starts to collect unique pdf/office documents from the user, and taking C2 commands for other info collection.
  * The updater is observed starting WerFault with a custom dll loaded, and WerFault uploads a crashdump.