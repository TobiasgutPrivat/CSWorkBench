# CSWorkBench

An UI-App which

Hosted and rendered from server

## Concept

connected to one Database per one Registry (like Memory)

maybe blocking objects per registry could be done to avoid conflicting actions from diffrent users (use singleton / scoped services)

### Routing

web-assembly-server-host/ -> search Tab

web-assembly-server-host/{ObjectId:int}/ -> display the Object

web-assembly-server-host/{ObjectId:int}/\*\*{some Pathing}/ -> display object routed to, navigation uses such Paths

### Contents

Main: Dynamic display of Object with Attributes and Methods

some buttons for new, search, settings

Opt. Right: AI-Assistant

Opt. Somewhere: settings (user management)

### Actions

creating objects

Editing objects (blocking)

run Functions (blocking)

deleting objects

on starting action (except create) check if nothing changed in registry
