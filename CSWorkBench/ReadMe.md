# CSWorkBench

A tool to allow usage of Programmatic features in daily life, and demonstrate advantages of unified systems

Compromises to a optimal IT-Environment are tagged with (compromise)

## Concept

### Data

to manage Data the Registry from DynObjectStore is used

that handles storing of objects and linking objects via "Attachements"

### Frontend

Optimaly Server only provides the Data Objects and UI is built locally, also you could connect to multiple sources allowing search over multiple as well

For easier implementation and compatibility the server provides UI here (compromise), similair to Admin-Panel

maybe create local variant as well, to allow better managing of multiple objects, sources and local data like settings, favorites, git and AI-chats

**Browser-variant**

perspective from the server, similair to admin-panel (not focused on user)

DB-Connection fixed in providing server (if auth, then on accessing the UI-Server already)

Main: Dynamic display of Object with Attributes and Methods

Opt. Right: AI-Assistant

some buttons for new, search, settings

**Local Variant**

perspective from User

DB-Connections can be managed, with auth etc.

Left: manage connections, Git, settings

Main: Dynamic display of Object with Attributes and Methods

Opt. Right: AI-Assistant

some buttons for new, search, settings

### Routing

for now: Generally one database is Selected (later: add authentication etc, for desktop add managing Connections locally)

web-assembly-server-host/ -> search Tab

web-assembly-server-host/{ObjectId}/ -> display the Object

web-assembly-server-host/{ObjectId}/\*\*{some Pathing}/ -> display object routed to, navigation uses such Paths

## Issues

Classes without Parameterles constructors don't allow deserializing recursion, not sure if a fix is possible
