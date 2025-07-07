# CSWorkBench

A tool to allow usage of Programmatic features in daily life, and demonstrate advantages of unified systems

## Setup

docker compose up -d

## Concept

### Data

Objects:

- model: (id, class, serialized data)
- instances of specified class
- serialization per jsonserializer (recreates)

Attachments:

- model: (ParentId, path, name, ObjectId)
- represents the conection of an object attached to another object
- location stored as ObjectId + path within object + Attrname (e.g. #1234, somelist.2 , "an_attachment")

DB:

- models stored as tables, maybe include vector for objects

RealTime-Memory:

- registry for ObjectIds: {Object: ObjectId}
- registry for Objects: {ObjectId: Object}
- registry for attachements to Objects {Object, path, Object}

### Frontend

Optimaly Server only provides the Data Objects and UI is built locally, also you could connect to multiple sources allowing search over multiple as well

For easier implementation and compatibility the server provides UI here (compromise)

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
