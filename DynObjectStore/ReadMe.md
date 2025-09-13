# TODO

maybe remove type info from Db-Table instead use the one from json string

# Ideas

Entities (semantic Real-world Objects) can be wrapped to split up big object graphs

this would allow layz loading and change management per Entity

The definition of Entities can be made on class level, this needs to be configured in code or config

https://chatgpt.com/share/68c51374-a86c-8007-8af6-e9f04990ccaa

# Concept

based on Idea of a (Global entity-centric IT Environement)[https://www.notion.so/Global-Entity-Centric-IT-1bd37f05f33a8024bd0aea28f3f3baba]

Registry:

This acts like a runtime memory of Objects in a Database.
There should always only be one Registry running per Database, and all interactions with Data should go through that registry.
Services like UI, API, Agent can use that single Registry to access Data

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
- registry for attachements {<Object, path, name>, Object} used to track what object is at a specific path
- registry for attachementIds {Object, [<Object, path, name>]} used to track what paths are there for one parent

## Decisions

- implementing git like feature in here, does not make sense because this is about local computing, git like systems can though be implemented as C# module allowing interaction from Computer to server or similair

## issues

- (resolvable) paths can loose connection when objects change (e.g. list/1/person changes if an element before get's removed)
  (possible solution) maybe id's can be tracked of objects within an object (like ref in serialization), bind to instance during runtime and deserialize (can maybe be integrated in serialization adding reference to diffrent rootobject -> not needing attachements seperate anymore)

- recursion does not work if classes don't have parameterless constructors

# Setup

**Requires**

- some DB server with according implementation of IDBConnection and according schema
- initialization of DBConnection and Registry

## Tested:

PgSqlServer with schema from init/schema.sql ([download](https://www.postgresql.org/download/))

PgAdmin4 and Command Line Tools are usefull for administration and testing

## Untested

docker-compose maybe works
