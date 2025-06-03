# Tabletop 2025.2

## The Narratives
### Threat Actor - Void Serpent
A fictional threat actor, VoidSerpent, has backdoor'd a popular markdown notes app called BeaverNotes. Hosting the backdoored version called BeaverNotesPro on a cloned version of the original site, they lure in victims with Google Ads. This threat actor is known for their low and slow espionage campaigns where low privilege and low noise methods are favored.

### Victim - Intern Bob
An eager intern is distraught to learn that their favorite note taking app, Obsidian, is internally banned. Frothing with rage
they take to google to look for an alternative, stumbling upon BeaverNotesPro after clicking one of the advertisements.


## The Tabletop
### General Event Timeline
- Intern searches google for an alternative to their favorite app, clicks the advertisement for one of the top results, and ends up installing Beaver Notes Pro.
- The installer adds a trusted root certificate to CurrentUser\Root, and creates two new scheduled tasks.
- Post install, BeaverUpdate.exe task is run and the process maintains a websocket connection to beaver.bitcorns.cc.
- BeaverUpdate.exe should be observed using LDAP to query the user's information and enumerating pdf/doc/docx files.
- BeaverUpdate.exe eventually enumerates LDAP users within groups containing "Executive" or "Purchasing".
- BeaverUpdate.exe eventually runs BeaverSync.exe which should be observed uploading the collected Active Directory and files of interest to beaverpro.sketchybins.com/sync.
- At some point BeaverUpdate.exe's websocket receives a command and runs a register command with BeaverSync, the user sees and completes and administrative prompt and a new service, BeaverElevateService is installed and runs.
- BeaverUpdate.exe can now accept C2 commands from the websocket connection and pass them over NamedPipe to the BeaverElevateService.
- BeaverElevateService should be observed enumerating AV solutions, in memory download and execute of .net assemblies, and other suspicious commands.
