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

### UI

Top: DB-Connection, some Settings, info etc.

Left: Selection Objects, favorites, Search etc. (similair to VS-Code)

Main: Dynamic diyplay of Object with Attributes and Methods

Opt. Right: AI-Assistant

### Routing

for now: Generally one database is Selected (later: add authentication etc, for desktop add managing Connections locally)

web-assembly-server-host/ -> search Tab

web-assembly-server-host/{ObjectId}/ -> display the Object

web-assembly-server-host/{ObjectId}/\*\*{some Pathing}/ -> display object routed to, navigation uses such Paths
