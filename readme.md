# Table Top 2025


## The Narrative
A fictional threat actor, VoidSerpent, has backdoor'd a popular markdown notes app called BeaverNotes. Hosting the backdoored
version called BeaverNotesPro on a cloned version of the original site, they lure in victims with Google Ads.

An eager intern is distraught to learn that their favorite note taking app, Obsidian, is internally banned. Frothing with rage
they take to google to look for an alternative, stumbling upon BeaverNotesPro after clicking one of the advertisements.

The installer doesn't require administrative privileges and is a satisfying application, but eventually through a random administrative
prompt, the software gains a deeper level of persistence and permissions than expected.

### Challenges
  * Target automated analysis tooling by moving suspicious/malicious activity out to libraries.
  * Choosing a communication method that doesn't stick out but isn't very visible; WebSockets and NamedPipes.
  * Forcing an analyst to use deeper analysis methods, knowing our tools.

### The Tabletop
  * User needs a note taking app and searches google, but ends up clicking a malicious ad that distributes a backdoored installer.
  * The software installs without admin privileges, and has a scheduled update task.
  * The updater looks to be querying active directory and connects to an update endpoint (websocket).
  * The updater starts to collect unique pdf/office documents from the user, and taking C2 commands for other info collection.
  * The updater launches BeaverSync which asks for administrative permissions.
    * Then it archives notable files from the user's home directory to a "sync" endpoint.
    * Tampers with SCManager permissions to allow any user to start/stop/create a service without admin.
    * A binary is copied to the Windows directory, then installed as a service with InstallUtil and started.
  * BeaverUpdate receives custom commands from the websocket service and passes them to BeaverEleavationService over NamedPipes.