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
